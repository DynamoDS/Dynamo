using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Dynamo.Utilities
{
    internal class Hash
    {
        /// <summary>
        /// Get the hash value
        /// </summary>
        /// <param name="bytes">input as a byte array</param>
        /// <returns>hash as a byte array</returns>
        internal static byte[] GetHash(byte[] bytes)
        {
            using (var hashAlgorithm = SHA256.Create())
            {
                return hashAlgorithm.ComputeHash(bytes);
            }
        }

        /// <summary>
        /// Get the hash value
        /// </summary>
        /// <param name="str">input as a string</param>
        /// <returns>hash as a byte array</returns>
        internal static byte[] GetHashFromString(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return GetHash(bytes);
        }

        /// <summary>
        /// Get a valid filename for a hash
        /// </summary>
        /// <param name="bytes">hash as a byte array</param>
        /// <returns>hash as a valid filename string</returns>
        internal static string GetFilenameFromHash(byte[] bytes)
        {
            return ToBase32String(bytes);
        }

        /// <summary>
        /// Get hash file name
        /// </summary>
        /// <param name="str">inout as a string</param>
        /// <returns>hash as a valid filename</returns>
        internal static string GetHashFilenameFromString(string str)
        {
            var hash = GetHashFromString(str);
            return GetFilenameFromHash(hash);
        }

        /// <summary>
        /// /// The different characters allowed in Base32 encoding.
        /// </summary>
        /// <remarks>
        /// This is a 32-character subset of the twenty-six letters A–Z and six digits 2–7.
        /// <see cref="https://en.wikipedia.org/wiki/Base32" />
        /// </remarks>
        internal static string Base32AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        /// <summary> /// Converts a byte array into a Base32 string.
        /// </summary>
        /// <param name="input">The string to convert to Base32.</param>
        /// <param name="addPadding">Whether or not to add RFC3548 '='-padding to the string.</param>
        /// <returns>A Base32 string.</returns>
        /// <remarks>
        /// https://tools.ietf.org/html/rfc3548#section-2.2 indicates padding MUST be added unless the reference to the RFC tells us otherswise.
        /// https://github.com/google/google-authenticator/wiki/Key-Uri-Format indicates that padding SHOULD be omitted.
        /// To meet both requirements, you can omit padding when required.
        /// </remarks>
        internal static string ToBase32String(byte[] input, bool addPadding = false)
        {
            if (input == null || input.Length == 0)
            {
                return string.Empty;
            }

            var bits = input.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')).Aggregate((a, b) => a + b)
                .PadRight((int)(Math.Ceiling((input.Length * 8) / 5d) * 5), '0');
            var result = Enumerable.Range(0, bits.Length / 5)
                .Select(i => Base32AllowedCharacters.Substring(Convert.ToInt32(bits.Substring(i * 5, 5), 2), 1))
                .Aggregate((a, b) => a + b);

            if (addPadding)
            {
                result = result.PadRight((int)(Math.Ceiling(result.Length / 8d) * 8), '=');
            }

            return result;
        }
    }
}


