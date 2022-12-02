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
            using (var hashAlgorithm = HashAlgorithm.Create())
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
        /// <returns>hash a valid filename string</returns>
        internal static string GetFilenameFromHash(byte[] bytes)
        {
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.AppendFormat(@"{0:x2}", b);
            }
            return builder.ToString();
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
    }
}
