using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Dynamo.DocumentationBrowser.Properties;
using Dynamo.ViewModels;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// Class responsible for creating the Node Info part of the node documentation. 
    /// </summary>
    internal static class NodeDocumentationHtmlGenerator
    {
        private const string STYLE_RESOURCE = "Dynamo.DocumentationBrowser.Docs.MarkdownStyling.html";

        /// <summary>
        /// Creates the Node information section which all nodes have
        /// even if they dont have additional markdown documentation.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        internal static string FromAnnotationEventArgs(OpenNodeAnnotationEventArgs e)
        {
            if (e is null)
                throw new ArgumentNullException(nameof(e));

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(DocumentationBrowserUtils.GetContentFromEmbeddedResource(STYLE_RESOURCE));
            sb.AppendLine(CreateHeader(e));
            sb.AppendLine(CreateNodeInfo(e));

            return sb.ToString();
        }

        private static string CreateHeader(OpenNodeAnnotationEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<h1>{e.Type}</h1>");
            sb.AppendLine($"<p><i>{e.MinimumQualifiedName}</i></p>");
            sb.AppendLine("<hr>");

            return sb.ToString();
        }

        private static string CreateNodeInfo(OpenNodeAnnotationEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<h2>{Resources.NodeDocumentationNodeInfo}</h2>");
            sb.AppendLine("<table class=\"table--noborder\">");
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{Resources.NodeDocumentationNodeType}</td>");
            sb.AppendLine($"<td>{e.Type}</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{Resources.NodeDocumentationDescription}</td>");
            sb.AppendLine($"<td>{Regex.Replace(e.Description, @"\r\n?|\n", "<br>")}</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{Resources.NodeDocumentationCategory}</td>");
            sb.AppendLine($"<td>{e.Category}</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{Resources.NodeDocumentationInputs}</td>");
            sb.AppendLine("<td>");
            for (int i = 0; i < e.InputNames.Count(); i++)
            {
                sb.AppendLine(
                    $"<li style=\"margin-bottom: 5px\"><b><u>{e.InputNames.ElementAt(i)}</u></b><br>{Regex.Replace(e.InputDescriptions.ElementAt(i), @"\r\n?|\n", "<br>")}</li>");
            }
            sb.AppendLine("</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{Resources.NodeDocumentationOutputs}</td>");
            sb.AppendLine("<td>");
            for (int i = 0; i < e.OutputNames.Count(); i++)
            {
                sb.AppendLine(
                    $"<li style=\"margin-bottom: 5px\"><b><u>{e.OutputNames.ElementAt(i)}</u></b><br>{Regex.Replace(e.OutputDescriptions.ElementAt(i), @"\r\n?|\n", "<br>")}</li>");
            }
            sb.AppendLine("</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("<hr>");

            return sb.ToString();
        }
    }
}
