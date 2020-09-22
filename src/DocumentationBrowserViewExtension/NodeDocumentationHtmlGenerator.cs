using System;
using System.Text;
using System.Text.RegularExpressions;
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
            sb.AppendLine(string.Format("<h1>{0}</h1>", e.Type));
            sb.AppendLine(string.Format("<p><i>{0}</i></p>", e.Namespace));
            sb.AppendLine("<hr>");

            return sb.ToString();
        }

        private static string CreateNodeInfo(OpenNodeAnnotationEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<h2>Node Info</h2>");
            sb.AppendLine("<table class=\"table--noborder\">");
            sb.AppendLine("<tr>");
            sb.AppendLine(string.Format("<td>{0}</td>", "Node Type"));
            sb.AppendLine(string.Format("<td>{0}</td>", e.Type));
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine(string.Format("<td>{0}</td>", "Description"));
            sb.AppendLine(string.Format("<td>{0}</td>", Regex.Replace(e.Description, @"\r\n?|\n", "<br>")));
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine(string.Format("<td>{0}</td>", "Category"));
            sb.AppendLine(string.Format("<td>{0}</td>", e.Category));
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine(string.Format("<td>{0}</td>", "Inputs"));
            sb.AppendLine("<td>");
            for (int i = 0; i < e.InputNames.Count; i++)
            {
                sb.AppendLine(string.Format("<li style=\"margin-bottom: 5px\"><b><u>{0}</u></b><br>{1}</li>", e.InputNames[i], Regex.Replace(e.InputDescriptions[i], @"\r\n?|\n", "<br>")));
            }
            sb.AppendLine("</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("<tr>");
            sb.AppendLine(string.Format("<td>{0}</td>", "Outputs"));
            sb.AppendLine("<td>");
            for (int i = 0; i < e.OutputNames.Count; i++)
            {
                sb.AppendLine(string.Format("<li style=\"margin-bottom: 5px\"><b><u>{0}</u></b><br>{1}</li>", e.OutputNames[i], Regex.Replace(e.OutputDescriptions[i], @"\r\n?|\n", "<br>")));
            }
            sb.AppendLine("</td>");
            sb.AppendLine("</tr>");
            sb.AppendLine("</table>");
            sb.AppendLine("<hr>");

            return sb.ToString();
        }
    }
}
