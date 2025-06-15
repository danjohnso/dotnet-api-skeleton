using System.Security.Cryptography;
using System.Text;

namespace Skeleton.Core.Extensions
{
    public static class HashExtensions
    {
        public static byte[]? Sha512(this byte[]? input)
        {
            if (input == null)
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
		public static string? Sha512(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = SHA512.HashData(bytes);

            return Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Returns Hex string representation, URL and cookie name safe
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string? Sha512Hex(this string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = SHA512.HashData(bytes);

            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }
}
