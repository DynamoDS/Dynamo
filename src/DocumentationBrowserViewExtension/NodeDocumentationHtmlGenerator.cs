using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Input;
using Dynamo.DocumentationBrowser.Properties;
using Dynamo.ViewModels;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// Class responsible for creating the Node Info part of the node documentation. 
    /// </summary>
    internal static class NodeDocumentationHtmlGenerator
    {
        /// <summary>
        /// Creates the Node information section which all nodes have
        /// even if they don't have additional markdown documentation.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        internal static string FromAnnotationEventArgs(OpenNodeAnnotationEventArgs e, string mkDown)
        {
            if (e is null)
                throw new ArgumentNullException(nameof(e));

            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine(CreateHeader(e));
            sb.AppendLine(CreateBody(e, mkDown));

            return sb.ToString();
        }

        internal static string OpenDocument()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<body>");
            sb.AppendLine("<div>");

            return sb.ToString();
        }

        internal static string CloseDocument()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(@"</div>");
            sb.Append(@"</body>");

            return sb.ToString();
        }

        private static string CreateHeader(OpenNodeAnnotationEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<h1>{e.Type}</h1>");
            sb.AppendLine($"<p><i>{e.MinimumQualifiedName}</i></p>");

            return sb.ToString();
        }

        private static string CreateBody(OpenNodeAnnotationEventArgs e, string mkDown)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(CreateInfo(e, mkDown));
            //sb.AppendLine(CreateHelp(e));
            sb.AppendLine(CreateInputs(e));

            return sb.ToString();
        }

        private static string CreateInfo(OpenNodeAnnotationEventArgs e, string mkDown)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<details open>");
            sb.AppendLine(CreateExpanderTitle("Node Information"));
            sb.AppendLine(CreateNodeInfo(e));
            sb.AppendLine(mkDown);
            sb.AppendLine(@"</details>");

            return sb.ToString();
        }

        private static string CreateHelp(OpenNodeAnnotationEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<details open>");
            sb.AppendLine(CreateExpanderTitle("Node Issue Help"));
            sb.AppendLine(@"</details>");

            return sb.ToString();
        }

        private static string CreateInputs(OpenNodeAnnotationEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<details open>");
            sb.AppendLine(CreateExpanderTitle("Inputs"));
            sb.AppendLine(CreateInputsAndOutputs(e));
            sb.AppendLine(@"</details>");

            return sb.ToString();
        }
        
        private static string CreateExpanderTitle(string title)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<summary>");
            sb.AppendLine($"<strong>{title}</strong>");
            sb.AppendLine("<span class=\"icon\">");
            sb.AppendLine(@"</span>");
            sb.AppendLine(@"</summary>");

            return sb.ToString();
        }

        private static string CreateNodeInfo(OpenNodeAnnotationEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine($"<h2>{Resources.NodeDocumentationNodeType}</h2>");
            sb.AppendLine($"<p>{e.Type}</p>");
            sb.AppendLine($"<h2>{Resources.NodeDocumentationDescription}</h2>");
            sb.AppendLine($"<p>{Regex.Replace(e.Description, @"\r\n?|\n", "<br>")}</p>");
            
            return sb.ToString();
        }

         private static string CreateInputsAndOutputs(OpenNodeAnnotationEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"<h2>{Resources.NodeDocumentationInputs}</h2>");
            sb.AppendLine("<table class=\"table--border\">");
            sb.AppendLine("<tr class=\"table--border\">");
            sb.AppendLine($"<th class=\"table--border\">{"Name"}</th>");
            sb.AppendLine($"<th class=\"table--border\">{"Type"}</th>");
            sb.AppendLine($"<th class=\"table--border\">{"Description"}</th>");
            sb.AppendLine($"<th class=\"table--border\">{"Default value"}</th>");
            sb.AppendLine(@"</tr>");

            //sb.AppendLine("<tr class=\"table--border\">");
            //sb.AppendLine($"<td class=\"table--border\">{Resources.NodeDocumentationCategory}</td>");
            //sb.AppendLine($"<td class=\"table--border\">{e.Category}</td>");
            //sb.AppendLine(@"</tr>");
            //sb.AppendLine("<tr class=\"table--border\">");
            //sb.AppendLine($"<td class=\"table--border\">{Resources.NodeDocumentationInputs}</td>");
            //sb.AppendLine("<td class=\"table--border\">");

            for (int i = 0; i < e.InputNames.Count(); i++)
            {
                sb.AppendLine("<tr class=\"table--border\">");
                sb.AppendLine($"<td class=\"table--border\">{e.InputNames.ElementAt(i)}</td>");
                sb.AppendLine($"<td class=\"table--border\">{GetTypeFromDescription(e.InputDescriptions.ElementAt(i))}</td>");
                sb.AppendLine($"<td class=\"table--border\">{GetDescriptionFromDescription(e.InputDescriptions.ElementAt(i))}</td>");
                sb.AppendLine($"<td class=\"table--border\">{GetDefaultValueFromDescription(e.InputDescriptions.ElementAt(i))}</td>");
                sb.AppendLine(@"</tr>");
            }

            sb.AppendLine(@"</td>");
            sb.AppendLine(@"</tr>");
            sb.AppendLine(@"</table>");


            sb.AppendLine($"<h2>{Resources.NodeDocumentationOutputs}</h2>");
            sb.AppendLine("<table class=\"table--border\">");
            sb.AppendLine("<tr class=\"table--border\">");
            sb.AppendLine($"<th class=\"table--border\">{"Name"}</th>");
            sb.AppendLine($"<th class=\"table--border\">{"Description"}</th>");
            sb.AppendLine($"<th class=\"table--border\">{"Data type"}</th>");
            sb.AppendLine(@"</tr>");

            for (int i = 0; i < e.OutputNames.Count(); i++)
            {
                sb.AppendLine("<tr class=\"table--border\">");
                sb.AppendLine($"<td class=\"table--border\">{e.OutputNames.ElementAt(i)}</td>");
                sb.AppendLine($"<td class=\"table--border\">{GetDescriptionFromDescription(e.OutputDescriptions.ElementAt(i))}</td>");
                sb.AppendLine(@"</tr>");
            }

            sb.AppendLine(@"</td>");
            sb.AppendLine(@"</tr>");
            sb.AppendLine(@"</table>");

            return sb.ToString();
        }

         private static string GetTypeFromDescription(string elementAt)
         {
             var stringArr = elementAt.Split(new string[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);
             if(stringArr.Length > 2) return stringArr[2];
             return string.Empty;
         }

         private static string GetDefaultValueFromDescription(string elementAt)
         {
             var stringArr = elementAt.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
             foreach (var line in stringArr)
             {
                 if (line.ToLowerInvariant().Contains("default value"))
                 {
                     return line.Remove(0, 16);
                 }
             }
             return string.Empty;
         }

        private static string GetDescriptionFromDescription(string elementAt)
         {
             var stringArr = elementAt.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
             return stringArr[0];
         }
    }
}
