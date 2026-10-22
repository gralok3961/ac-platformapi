namespace Infrastructure.Helpers
{
    public static class AgentAccountPublicIdGenerator
    {
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static string Generate()
        {
            // 128 fresh random bits, encoded as RFC 4648 Base32 without padding.
            Span<byte> bytes = stackalloc byte[16];
            RandomNumberGenerator.Fill(bytes);

            Span<char> publicId = stackalloc char[30];
            "agt_".AsSpan().CopyTo(publicId);

            uint buffer = 0;
            var bits = 0;
            var position = 4;

            foreach (var value in bytes)
            {
                buffer = (buffer << 8) | value;
                bits += 8;

                while (bits >= 5)
                {
                    bits -= 5;
                    publicId[position++] = Alphabet[(int)((buffer >> bits) & 31)];
                }
            }

            // The remaining three bits occupy the high bits of the final symbol.
            publicId[position] = Alphabet[(int)((buffer << (5 - bits)) & 31)];
            return new string(publicId);
        }
    }
}
