using System.Security.Cryptography;
using System.Text;

namespace Dynamo.Utilities
{
    internal class Hash
    {
        internal static byte[] GetHash(byte[] bytes)
        {
            using (var hashAlgorithm = HashAlgorithm.Create())
            {
                return hashAlgorithm.ComputeHash(bytes);
            }
        }

        internal static byte[] GetHashFromString(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return GetHash(bytes);
        }

        internal static string GetFilenameFromHash(byte[] bytes)
        {
            var builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.AppendFormat(@"{0:x2}", b);
            }
            return builder.ToString();
        }

        internal static string GetHashFilenameFromString(string str)
        {
            var hash = GetHashFromString(str);
            return GetFilenameFromHash(hash);
        }
    }
}
