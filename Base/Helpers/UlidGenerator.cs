public enum UlidLength
{
    Long,    // 26 chars (48bit time + 80bit random)
    Medium,  // 16 chars (48bit time + 30bit random)
    Short    // 14 chars (48bit time + 20bit random)
}
public static class UlidGenerator
{
    private static readonly char[] Base32 =
        "0123456789ABCDEFGHJKMNPQRSTVWXYZ".ToCharArray();

    public static string Generate(UlidLength length)
    {
        int totalLength;
        int randomBits;

        switch (length)
        {
            case UlidLength.Long:
                totalLength = 26;
                randomBits = 80;
                break;

            case UlidLength.Medium:
                totalLength = 16;
                randomBits = 30;
                break;

            case UlidLength.Short:
                totalLength = 14;
                randomBits = 20;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(length));
        }

        Span<char> buffer = stackalloc char[totalLength];

        // Encode the 48-bit Unix timestamp as 10 Base32 characters.
        long time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        for (int i = 9; i >= 0; i--)
        {
            buffer[i] = Base32[time & 31];
            time >>= 5;
        }

        // Random part
        int randomChars = (randomBits + 4) / 5; // ceil(bits / 5)

        if (randomBits <= 30)
        {
            int random = Random.Shared.Next(1 << randomBits);
            for (int i = 10; i < 10 + randomChars; i++)
            {
                buffer[i] = Base32[random & 31];
                random >>= 5;
            }
        }
        else
        {
            // 80bit random (Long ULID)
            for (int i = 10; i < 10 + randomChars; i++)
            {
                buffer[i] = Base32[Random.Shared.Next(32)];
            }
        }

        return new string(buffer);
    }
}
