using Dynamo.Utilities;
using System.IO;
using System.Reflection;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// Common utilities used across classes in the DocumentationBrowserViewExtension.
    /// </summary>
    internal static class DocumentationBrowserUtils
    {
        /// <summary>
        /// Returns the DPIScript
        /// </summary>
        /// <param name="content"></param>
        internal static string GetDPIScript()
        {
            return ResourceUtilities.DPISCRIPT;
        }
        /// <summary>
        /// Returns the Image Navigation Script
        /// </summary>
        /// <returns></returns>
        internal static string GetImageNavigationScript()
        {
            return ResourceUtilities.IMGNAVIGATIONSCRIPT;
        }
        /// <summary>
        /// Returns the content of an embedded resource file.
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        internal static string GetContentFromEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var result = "";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        private const string SYNTAX_HIGHLIGHTING = "Dynamo.DocumentationBrowser.Docs.syntaxHighlight.html";

        /// <summary>
        /// Inject syntax highlighting into a html string.
        /// </summary>
        /// <param name="content"></param>
        internal static string GetSyntaxHighlighting()
        {
            var syntaxHighlightingContent = DocumentationBrowserUtils.GetContentFromEmbeddedResource(SYNTAX_HIGHLIGHTING);
            if (string.IsNullOrWhiteSpace(syntaxHighlightingContent))
                return string.Empty;

            return syntaxHighlightingContent;
        }
    }
}
