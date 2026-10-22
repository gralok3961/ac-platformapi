using System.Security.Cryptography;
using System.Text.Json;
using Core.Dtos;

namespace Core.Helpers
{
    public static class PaymentIdempotency
    {
        // Preserve the existing v1 wire format so recorded retries keep the same key.
        public static string CreateKey(ProcessPaymentRequest request)
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(new object[]
            {
                "hashtrion.process-payment.v1", request.BusinessId, request.CheckoutId, request.CheckoutCurrency, request.CheckoutAmount,
                request.PlatformProfile, request.PaymentDescription,
                request.Items.Select(x => new object[] { x.Id, x.Title, x.Quantity, x.UnitAmount, x.LineTotal }).ToArray(),
                new object[] { request.Instrument.HandlerId, request.Instrument.Id, request.Instrument.Type, request.Instrument.Network, request.Instrument.Asset,
                    request.Instrument.Credential.Type, request.Instrument.Credential.Token,
                    request.Instrument.Credential.Expiry.ToUniversalTime().ToString("O", System.Globalization.CultureInfo.InvariantCulture) }
            });
            try { return "ucp_" + Convert.ToHexString(SHA256.HashData(body)).ToLowerInvariant(); }
            finally { CryptographicOperations.ZeroMemory(body); }
        }
    }
}
