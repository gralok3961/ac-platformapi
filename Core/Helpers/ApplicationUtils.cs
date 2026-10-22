namespace Core.Helpers
{
    public static class ApplicationUtils
    {
        public static string GeneratePaymentCode(string businessCode)
        {
            businessCode = businessCode.Trim().ToUpperInvariant();

            var ulid = UlidGenerator.Generate(UlidLength.Short);

            return $"{businessCode}-{ulid}";
        }
    }
}
