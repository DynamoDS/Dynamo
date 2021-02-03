using System.IO;
using System.Reflection;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// Common utilities used across classes in the DocumentationBrowserViewExtension.
    /// </summary>
    internal static class DocumentationBrowserUtils
    {
        private const string DPISCRIPT = @"<script> function getDPIScale()
        {
            var dpi = 96.0;
            if (window.screen.deviceXDPI != undefined)
            {
                dpi = window.screen.deviceXDPI;
            }
            else
            {
                var tmpNode = document.createElement('DIV');
                tmpNode.style.cssText = 'width:1in;height:1in;position:absolute;left:0px;top:0px;z-index:99;visibility:hidden';
                document.body.appendChild(tmpNode);
                dpi = parseInt(tmpNode.offsetWidth);
                tmpNode.parentNode.removeChild(tmpNode);
            }

            return dpi / 96.0;
        }

        function adaptDPI()
        {
            var dpiScale = getDPIScale();
            document.body.style.zoom = dpiScale;

            var widthPercentage = ((100.0 / dpiScale)-5).toString() + '%';
            document.body.style.width = widthPercentage;
        }
        adaptDPI() 
        </script>";

        /// <summary>
        /// Returns the DPIScript
        /// </summary>
        /// <param name="content"></param>
        internal static string GetDPIScript()
        {
            return DPISCRIPT;
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
