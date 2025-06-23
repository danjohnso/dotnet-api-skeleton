using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace Skeleton.SimpleJwt.Extensions
{
    internal static class HashExtensions
    {
        public static byte[]? Sha512(this byte[]? input)
        {
            if (input is null)
            {
                return null;
            }

            return SHA512.HashData(input);
        }

        /// <summary>
        /// Returns a Base64 string, not URL or cookie name safe
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull(nameof(input))]
        public static string? Sha512(this string? input)
        {
            if (input.IsWhiteSpace())
            {
                return null;
            }

            return Convert.ToBase64String(SHA512.HashData(Encoding.UTF8.GetBytes(input)));
        }

        /// <summary>
        /// Returns Hex string representation, URL and cookie name safe
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [return: NotNullIfNotNull(nameof(input))]
        public static string? Sha512Hex(this string? input)
        {
            if (input.IsWhiteSpace())
            {
                return null;
            }

            return Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(input))).Replace("-", string.Empty);
        }
    }
}
