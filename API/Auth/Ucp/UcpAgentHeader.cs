using StructuredFieldValues;

namespace API.Auth.Ucp
{
    internal static class UcpAgentHeader
    {
        private const int MaximumHeaderLength = 8192;
        private static readonly char[] NumberTerminators = [' ', '\t', ';', ')', ','];

        public static Uri ParseProfile(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length > MaximumHeaderLength)
                throw InvalidHeader();

            var parseError = SfvParser.ParseDictionary(input, out var fields);
            if (parseError.HasValue)
                throw InvalidHeader();

            ValidateDictionarySyntax(input, fields.Count);

            if (!fields.TryGetValue("profile", out var profile) || profile.Value is not string url)
                throw InvalidHeader();

            return ParseProfileUrl(url);
        }

        private static Uri ParseProfileUrl(string url)
        {
            if (url.Any(character => char.IsWhiteSpace(character) || char.IsControl(character) || character == '\\'))
                throw InvalidHeader();

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                throw InvalidHeader();

            if (uri.UserInfo.Length != 0 || uri.Fragment.Length != 0 || uri.Host.Length == 0)
                throw InvalidHeader();

            return uri;
        }

        private static void ValidateDictionarySyntax(string input, int parsedMemberCount)
        {
            // The package parses the dictionary. These checks enforce the stricter
            // RFC 8941 syntax and duplicate-member policy used by this API.
            var memberCount = 1;
            var insideString = false;

            for (var index = 0; index < input.Length; index++)
            {
                var character = input[index];

                // Ignore quoted text, including escaped quotes such as \".
                if (insideString)
                {
                    if (character == '\\')
                        index++;
                    else if (character == '"')
                        insideString = false;

                    continue;
                }

                if (character == '"')
                {
                    insideString = true;
                    continue;
                }

                if (character == ',')
                {
                    memberCount++;
                    continue;
                }

                // Date (@...) and display-string (%"...") values belong to RFC 9651.
                var isDate = character == '@';
                var isDisplayString = character == '%' && index + 1 < input.Length && input[index + 1] == '"';
                if (isDate || isDisplayString)
                    throw InvalidHeader();

                var startsValue = index > 0 && input[index - 1] is '=' or ' ' or '(';
                if (!startsValue)
                    continue;

                if (character == ':')
                {
                    // Byte sequences use :base64: and must not contain whitespace.
                    var closingColonIndex = input.IndexOf(':', index + 1);
                    if (closingColonIndex < 0)
                        throw InvalidHeader();

                    var base64Value = input.Substring(index + 1, closingColonIndex - index - 1);
                    if (base64Value.Any(char.IsWhiteSpace))
                        throw InvalidHeader();

                    index = closingColonIndex;
                    continue;
                }

                if (char.IsAsciiDigit(character) || character == '-')
                {
                    // A number must end with a digit; reject values such as "1." or "-".
                    var numberEnd = input.IndexOfAny(NumberTerminators, index);
                    if (numberEnd < 0)
                        numberEnd = input.Length;

                    if (!char.IsAsciiDigit(input[numberEnd - 1]))
                        throw InvalidHeader();

                    index = numberEnd - 1;
                }
            }

            // The package overwrites duplicate names, so they reduce the parsed count.
            if (memberCount != parsedMemberCount)
                throw InvalidHeader();
        }

        private static FormatException InvalidHeader()
        {
            return new FormatException("UCP-Agent must contain a valid HTTPS profile URL in an RFC 8941 dictionary.");
        }
    }
}
