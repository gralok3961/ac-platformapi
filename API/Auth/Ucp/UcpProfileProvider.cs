using System.Net.Sockets;
using System.Text.Json;

namespace API.Auth.Ucp
{
    public interface IUcpProfileProvider
    {
        Task<JsonElement> GetPublicKeysAsync(Uri profile, CancellationToken cancellationToken);
    }

    public sealed class UcpProfileProvider(IOptions<UcpSecurityOptions> security, TimeProvider clock) : IUcpProfileProvider
    {
        private const int MaximumProfileBytes = 128 * 1024;
        private readonly IOptions<UcpSecurityOptions> _security = security;
        private readonly TimeProvider _clock = clock;

        // Same blocked ranges as before, expressed as readable CIDR networks.
        private static readonly IPNetwork[] BlockedNetworks =
        [
            IPNetwork.Parse("0.0.0.0/8"),
            IPNetwork.Parse("10.0.0.0/8"),
            IPNetwork.Parse("100.64.0.0/10"),
            IPNetwork.Parse("127.0.0.0/8"),
            IPNetwork.Parse("168.63.129.16/32"), // Azure host service
            IPNetwork.Parse("169.254.0.0/16"),   // Includes cloud metadata
            IPNetwork.Parse("172.16.0.0/12"),
            IPNetwork.Parse("192.0.0.0/24"),
            IPNetwork.Parse("192.0.2.0/24"),
            IPNetwork.Parse("192.31.196.0/24"),
            IPNetwork.Parse("192.52.193.0/24"),
            IPNetwork.Parse("192.88.99.0/24"),
            IPNetwork.Parse("192.168.0.0/16"),
            IPNetwork.Parse("192.175.48.0/24"),
            IPNetwork.Parse("198.18.0.0/15"),
            IPNetwork.Parse("198.51.100.0/24"),
            IPNetwork.Parse("203.0.113.0/24"),
            IPNetwork.Parse("224.0.0.0/3"),
            IPNetwork.Parse("2001::/23"),
            IPNetwork.Parse("2001:db8::/32"),
            IPNetwork.Parse("2002::/16"),        // 6to4 transition addresses
            IPNetwork.Parse("2620:4f:8000::/48"),
            IPNetwork.Parse("3fff::/20")
        ];
        private static readonly IPNetwork GlobalIPv6Network = IPNetwork.Parse("2000::/3");

        private readonly object _cacheLock = new();
        private readonly Dictionary<string, (JsonElement Keys, DateTimeOffset Expires)> _cache = new(StringComparer.Ordinal);
        private readonly SemaphoreSlim _discoverySlots = new(4, 4);
        private DateTimeOffset _window;
        private int _discoveryCount;

        public async Task<JsonElement> GetPublicKeysAsync(Uri profile, CancellationToken cancellationToken)
        {
            var options = _security.Value;
            if (profile.Scheme != Uri.UriSchemeHttps ||
                !options.TrustedPlatformProfiles.Contains(profile.AbsoluteUri, StringComparer.Ordinal))
                throw InvalidProfile();

            lock (_cacheLock)
            {
                if (_cache.TryGetValue(profile.AbsoluteUri, out var cached) && cached.Expires > _clock.GetUtcNow())
                    return cached.Keys;

                _cache.Remove(profile.AbsoluteUri);
                if (_clock.GetUtcNow() - _window >= TimeSpan.FromMinutes(1))
                {
                    _window = _clock.GetUtcNow();
                    _discoveryCount = 0;
                }
                if (++_discoveryCount > 60)
                    throw new HttpRequestException("UCP profile discovery is at capacity.");
            }

            if (!await _discoverySlots.WaitAsync(0, cancellationToken))
                throw new HttpRequestException("UCP profile discovery is at capacity.");

            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(options.ProfileTimeoutSeconds));
                var addresses = await Dns.GetHostAddressesAsync(profile.IdnHost, timeout.Token);
                using var handler = CreateHandler(profile, addresses);
                using var client = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
                using var request = new HttpRequestMessage(HttpMethod.Get, profile)
                {
                    Version = HttpVersion.Version11,
                    VersionPolicy = HttpVersionPolicy.RequestVersionExact
                };
                request.Headers.Accept.ParseAdd("application/json");
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
                var keys = await ReadResponseAsync(response, timeout.Token);
                CachePublicKeys(profile, keys, response, options.ProfileCacheSeconds);
                return keys;
            }
            finally
            {
                _discoverySlots.Release();
            }
        }

        private void CachePublicKeys(Uri profile, JsonElement keys, HttpResponseMessage response, int maximumSeconds)
        {
            var cache = response.Headers.CacheControl;
            if (cache?.NoStore == true || cache?.NoCache == true)
                return;

            var remainingSeconds = (cache?.MaxAge?.TotalSeconds ?? maximumSeconds)
                - (response.Headers.Age?.TotalSeconds ?? 0);
            var expires = _clock.GetUtcNow().AddSeconds(Math.Clamp(remainingSeconds, 0, maximumSeconds));

            lock (_cacheLock)
            {
                // Only the validated allowlist (at most 32 URLs) can enter this cache.
                _cache[profile.AbsoluteUri] = (keys, expires);
            }
        }

        internal static SocketsHttpHandler CreateHandler(Uri profile, IPAddress[] addresses)
        {
            if (profile.Scheme != Uri.UriSchemeHttps || addresses.Length == 0 || addresses.Any(address => !IsPublicAddress(address)))
                throw InvalidProfile();

            var validatedAddresses = addresses.ToArray();
            return new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                UseProxy = false,
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.None,
                ConnectTimeout = TimeSpan.FromSeconds(3),
                MaxResponseHeadersLength = 16,
                // Pin this connection to the checked DNS results. TLS still validates the
                // original hostname; there is no second DNS lookup or certificate bypass.
                ConnectCallback = (context, token) => ConnectAsync(context, profile, validatedAddresses, token)
            };
        }

        private static async ValueTask<Stream> ConnectAsync(
            SocketsHttpConnectionContext context, Uri profile, IPAddress[] addresses, CancellationToken cancellationToken)
        {
            if (!string.Equals(context.DnsEndPoint.Host, profile.IdnHost, StringComparison.OrdinalIgnoreCase) ||
                context.DnsEndPoint.Port != profile.Port)
                throw InvalidProfile();

            foreach (var address in addresses)
            {
                var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    await socket.ConnectAsync(new IPEndPoint(address, profile.Port), cancellationToken);
                    return new NetworkStream(socket, ownsSocket: true);
                }
                catch (SocketException)
                {
                    socket.Dispose();
                }
                catch
                {
                    socket.Dispose();
                    throw;
                }
            }

            throw new HttpRequestException("UCP profile could not be reached.");
        }

        internal static async Task<JsonElement> ReadResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (response.StatusCode != HttpStatusCode.OK || response.Content.Headers.ContentLength > MaximumProfileBytes ||
                response.Content.Headers.ContentEncoding.Count != 0 ||
                response.Content.Headers.ContentType?.MediaType is not ("application/json" or "application/ucp+json"))
                throw InvalidProfile();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var body = new MemoryStream();
            var buffer = new byte[8192];
            int count;
            while ((count = await stream.ReadAsync(buffer, cancellationToken)) != 0)
            {
                if (body.Length + count > MaximumProfileBytes)
                    throw InvalidProfile();

                body.Write(buffer, 0, count);
            }

            using var document = JsonDocument.Parse(body.ToArray(), new JsonDocumentOptions
            {
                MaxDepth = 32,
                AllowDuplicateProperties = false
            });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("ucp", out var ucp) ||
                ucp.ValueKind != JsonValueKind.Object || !ucp.TryGetProperty("version", out var version) ||
                version.ValueKind != JsonValueKind.String || version.GetString() != UcpSecurityOptions.SupportedVersion)
                throw InvalidProfile();

            return ReadPublicKeys(root);
        }

        internal static bool IsPublicAddress(IPAddress address)
        {
            var isIPv4 = address.AddressFamily == AddressFamily.InterNetwork;
            var isGlobalIPv6 = address.AddressFamily == AddressFamily.InterNetworkV6
                && address.ScopeId == 0 && GlobalIPv6Network.Contains(address);

            return (isIPv4 || isGlobalIPv6) && !BlockedNetworks.Any(network => network.Contains(address));
        }

        internal static JsonElement ReadPublicKeys(JsonElement root)
        {
            // UCP 2026-08-25 key discovery reads keys[]. Never revive removed keys from a compatibility alias.
            if (!root.TryGetProperty("keys", out var keys) ||
                keys.ValueKind != JsonValueKind.Array || keys.GetArrayLength() is < 1 or > 32)
                throw InvalidProfile();

            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var key in keys.EnumerateArray())
            {
                if (key.ValueKind != JsonValueKind.Object || !key.TryGetProperty("kid", out var kid) ||
                    kid.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(kid.GetString()) ||
                    kid.GetString()!.Length > 256 || !ids.Add(kid.GetString()!) || key.TryGetProperty("d", out _))
                    throw InvalidProfile();
            }

            return keys.Clone();
        }

        private static FormatException InvalidProfile() => new("UCP profile is unavailable or invalid.");
    }
}
