using System;
using System.Security.Cryptography;
using System.Text;

namespace Dynamo.Exceptions
{
    /// <summary>
    /// Thrown when a file the user approved from its preview no longer has the content that was
    /// previewed at the moment Dynamo reads it to load. Nothing from the file has been loaded.
    /// </summary>
    internal sealed class FileChangedSincePreviewException : Exception
    {
        internal FileChangedSincePreviewException(string filePath)
            : base($"'{filePath}' changed after it was previewed.")
        {
            FilePath = filePath;
        }

        /// <summary>
        /// The file whose content changed.
        /// </summary>
        internal string FilePath { get; }

        /// <summary>
        /// Hashes file text the same way for the preview and for the load, so the two can be compared.
        /// </summary>
        /// <param name="fileContents">The file text as read by File.ReadAllText.</param>
        /// <returns>The SHA-256 hash of the text's UTF-8 bytes, as lowercase hex.</returns>
        internal static string ComputeContentHash(string fileContents)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(fileContents ?? string.Empty));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        /// <summary>
        /// Throws when an expected hash is given and the loaded text does not match it.
        /// </summary>
        /// <param name="filePath">The file being loaded.</param>
        /// <param name="fileContents">The text that is about to be loaded.</param>
        /// <param name="expectedContentHash">The hash of the previewed text, or null to skip the check.</param>
        internal static void ThrowIfChanged(string filePath, string fileContents, string expectedContentHash)
        {
            if (expectedContentHash == null) return;
            if (!string.Equals(ComputeContentHash(fileContents), expectedContentHash, StringComparison.Ordinal))
            {
                throw new FileChangedSincePreviewException(filePath);
            }
        }
    }
}
