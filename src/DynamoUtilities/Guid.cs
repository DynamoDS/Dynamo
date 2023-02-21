using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

// drawn from: https://github.com/LogosBible/Logos.Utility/blob/master/src/Logos.Utility/GuidUtility.cs
namespace Dynamo.Utilities
{
    /// <summary>
    /// Helper methods for working with <see cref="Guid"/>.
    /// </summary>
    public static class GuidUtility
    {
        /// <summary>
        /// attempts to parse a string into a guid - 
        /// if this fails, uses the create method to create
        /// a deterministic UUID.
        /// </summary>
        /// <param name="idstring"></param>
        /// <returns></returns>
        public static Guid tryParseOrCreateGuid(string idstring)
        {
            Guid id;
            if (!Guid.TryParse(idstring, out id))
            {
                id = GuidUtility.Create(GuidUtility.UrlNamespace, idstring);
            }

            return id;
        }

        /// <summary>
        /// Creates a name-based UUID using the algorithm from RFC 4122 §4.3.
        /// </summary>
        /// <param name="namespaceId">The ID of the namespace.</param>
        /// <param name="name">The name (within that namespace).</param>
        /// <returns>A UUID derived from the namespace and name.</returns>
        /// <remarks>See <a href="http://code.logos.com/blog/2011/04/generating_a_deterministic_guid.html">Generating a deterministic GUID</a>.</remarks>
        public static Guid Create(Guid namespaceId, string name)
        {
            return Create(namespaceId, name, 5);
        }

        /// <summary>
        /// Creates a name-based UUID using the algorithm from RFC 4122 §4.3.
        /// </summary>
        /// <param name="namespaceId">The ID of the namespace.</param>
        /// <param name="name">The name (within that namespace).</param>
        /// <param name="version">The version number of the UUID to create; this value must be either
        /// 3 (for MD5 hashing) or 5 (for SHA-1 hashing).</param>
        /// <returns>A UUID derived from the namespace and name.</returns>
        /// <remarks>See <a href="http://code.logos.com/blog/2011/04/generating_a_deterministic_guid.html">Generating a deterministic GUID</a>.</remarks>
        public static Guid Create(Guid namespaceId, string name, int version)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (version != 3 && version != 5)
                throw new ArgumentOutOfRangeException("version", "version must be either 3 or 5.");

            // convert the name to a sequence of octets (as defined by the standard or conventions of its namespace) (step 3)
            // ASSUME: UTF-8 encoding is always appropriate
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);

            // convert the namespace UUID to network order (step 3)
            byte[] namespaceBytes = namespaceId.ToByteArray();
            SwapByteOrder(namespaceBytes);

            // comput the hash of the name space ID concatenated with the name (step 4)
            byte[] hash;
            using (HashAlgorithm algorithm = version == 3 ? (HashAlgorithm)MD5.Create() : SHA1.Create())
            {
                algorithm.TransformBlock(namespaceBytes, 0, namespaceBytes.Length, null, 0);
                algorithm.TransformFinalBlock(nameBytes, 0, nameBytes.Length);
                hash = algorithm.Hash;
            }

            // most bytes from the hash are copied straight to the bytes of the new GUID (steps 5-7, 9, 11-12)
            byte[] newGuid = new byte[16];
            Array.Copy(hash, 0, newGuid, 0, 16);

            // set the four most significant bits (bits 12 through 15) of the time_hi_and_version field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
            newGuid[6] = (byte)((newGuid[6] & 0x0F) | (version << 4));

            // set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively (step 10)
            newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

            // convert the resulting UUID to local byte order (step 13)
            SwapByteOrder(newGuid);
            return new Guid(newGuid);
        }

        /// <summary>
        /// The namespace for fully-qualified domain names (from RFC 4122, Appendix C).
        /// </summary>
        public static readonly Guid DnsNamespace = new Guid("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

        /// <summary>
        /// The namespace for URLs (from RFC 4122, Appendix C).
        /// </summary>
        public static readonly Guid UrlNamespace = new Guid("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

        /// <summary>
        /// The namespace for ISO OIDs (from RFC 4122, Appendix C).
        /// </summary>
        public static readonly Guid IsoOidNamespace = new Guid("6ba7b812-9dad-11d1-80b4-00c04fd430c8");

        // Converts a GUID (expressed as a byte array) to/from network order (MSB-first).
        internal static void SwapByteOrder(byte[] guid)
        {
            SwapBytes(guid, 0, 3);
            SwapBytes(guid, 1, 2);
            SwapBytes(guid, 4, 5);
            SwapBytes(guid, 6, 7);
        }

        private static void SwapBytes(byte[] guid, int left, int right)
        {
            byte temp = guid[left];
            guid[left] = guid[right];
            guid[right] = temp;
        }

        /// <summary>
        /// Performs an update to all Guids inside the json string before deserialization.
        /// Targets specifically Guids without the '-' hyphen, which are all the workspace elements.
        /// Replacing all occurrences of each individual Guid guarantees that the relationships between the elements are retained.
        /// </summary>
        /// <param name="jsonData">Json representation of workspace.</param>
        /// <returns>String representation of workspace after all elements' Guids replaced.</returns>
        internal static string UpdateWorkspaceGUIDs(string jsonData)
        {
            string pattern = @"([a-z0-9]{32})";
            string updatedJsonData = jsonData;

            // The unique collection of Guids
            var mc = Regex.Matches(jsonData, pattern)
                .Cast<Match>()
                .Select(m => m.Value)
                .Distinct();

            foreach (var match in mc)
            {
                updatedJsonData = updatedJsonData.Replace(match, Guid.NewGuid().ToString("N"));
            }

            return updatedJsonData;
        }
    }
}
