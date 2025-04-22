using Dynamo.Utilities;
using HelixToolkit.SharpDX.Core.Utilities;
using System.IO;
using System.Reflection;
using System.Threading;

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
        internal static string GetContentFromEmbeddedResource(string resourceName, string locale = "")
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePrefix = "Dynamo.DocumentationBrowser.Docs.";
            var result = "";
            if (!string.IsNullOrEmpty(locale))
            {
                if (locale == "Default")
                    locale = Thread.CurrentThread.CurrentCulture.Name;

                resourceName = resourceName.Replace(resourcePrefix, resourcePrefix + locale.Replace('-', '_') + ".");
            }

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
        internal static string GetSyntaxHighlighting(string locale = "")
        {
            var syntaxHighlightingContent = DocumentationBrowserUtils.GetContentFromEmbeddedResource(SYNTAX_HIGHLIGHTING, locale);
            if (string.IsNullOrWhiteSpace(syntaxHighlightingContent))
                return string.Empty;

            return syntaxHighlightingContent;
        }
    }
}
