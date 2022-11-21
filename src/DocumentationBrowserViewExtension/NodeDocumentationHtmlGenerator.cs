using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using Dynamo.DocumentationBrowser.Properties;
using Dynamo.Logging;
using Dynamo.ViewModels;
using SharpDX.DXGI;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// Class responsible for creating the Node Info part of the node documentation. 
    /// </summary>
    internal static class NodeDocumentationHtmlGenerator
    {
        #region Constants

        private const string RESOURCE_PREFIX = "Dynamo.DocumentationBrowser.Docs.";

        #endregion

        /// <summary>
        /// Creates the Node information section which all nodes have
        /// even if they don't have additional markdown documentation.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        internal static string FromAnnotationEventArgs(OpenNodeAnnotationEventArgs e, string breadCrumbs, string mkDown)
        {
            if (e is null)
                throw new ArgumentNullException(nameof(e));

            StringBuilder sb = new StringBuilder();
            
            sb.AppendLine(CreateHeader(e, breadCrumbs));
            sb.AppendLine(CreateBody(e, mkDown));

            return sb.ToString();
        }

        internal static string OpenDocument()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<body>");
            sb.AppendLine("<div class=\"div--body\">");

            return sb.ToString();
        }

        internal static string CloseDocument()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(@"</div>");
            sb.Append(@"</body>");

            return sb.ToString();
        }

        private static string CreateHeader(OpenNodeAnnotationEventArgs e, string breadCrumbs)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<h1>{e.Type}</h1>");
            sb.AppendLine($"<p><i>{e.MinimumQualifiedName}</i></p>");
            if(!string.IsNullOrEmpty(breadCrumbs)) sb.AppendLine($"<p>{breadCrumbs}</p>");

            return sb.ToString();
        }

        private static string CreateBody(OpenNodeAnnotationEventArgs e, string mkDown)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(CreateInfo(e, mkDown));
            if (e.NodeInfos.Any())
            {
                sb.AppendLine(CreateHelp(e));
            }
            sb.AppendLine(CreateInputs(e));

            return sb.ToString();
        }

        private static string CreateInfo(OpenNodeAnnotationEventArgs e, string mkDown)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<details open>");
            sb.AppendLine(CreateExpanderTitle(Resources.NodeDocumentationNodeInfo));
            sb.AppendLine(CreateNodeInfo(e));
            sb.AppendLine(InjectImageNavigation(mkDown));
            sb.AppendLine("<br>");
            sb.AppendLine(@"</details>");

            return sb.ToString();
        }

        private static string InjectImageNavigation(string mkDown)
        {
            if (string.IsNullOrEmpty(mkDown)) return string.Empty;

            StringBuilder sb = new StringBuilder();

            var mkArray = mkDown.Split(new string[] {"\r\n", "\r", "\n"}, StringSplitOptions.None).Where(s => !string.IsNullOrEmpty(s)).ToArray();
            var imageRow = mkArray.Last();
            if (!imageRow.Contains("img"))
            {
                sb.AppendLine(mkArray.First());
                return sb.ToString();   // terminate early if no image
            }

            imageRow = imageRow.Replace("<p>", "").Replace("</p>", "");
            imageRow = imageRow.Insert(4, @" id='drag--img' class='resizable--img' ");

            sb.AppendLine(string.Join(Environment.NewLine, mkArray.Take(mkArray.Count() - 1).ToArray()));
            sb.AppendLine("<div class=\"container\" id=\"img--container\">");
            sb.AppendLine(imageRow);
            sb.AppendLine("<div class=\"btn--container\">");
            sb.AppendLine(
                $"<button type=\"button\" id=\"zoomfit\" class=\"button fitIcon\"  title=\"{Resources.ImageFitToolTip}\" ></button>");
            sb.AppendLine(
                $"<button type=\"button\" id=\"zoomin\" class=\"button plusIcon\" title=\"{Resources.ImageZoomInToolTip}\" ></button>\r\n");
            sb.AppendLine(
                $"<button type=\"button\" id=\"zoomout\" class=\"button minusIcon\" title=\"{Resources.ImageZoomOutToolTip}\" ></button>\r\n");
            sb.AppendLine(@"</div>");
            sb.AppendLine("<div class=\"btn--insert--container\">");

            var tooltip = Resources.ImageInsertToolTip.Replace("\\n", Environment.NewLine);
            sb.AppendLine(
                $"<button type=\"button\" id=\"insert\" class=\"button insertIcon\" title=\"{tooltip}\" ></button>\r\n");
            sb.AppendLine(@"</div>");
            sb.AppendLine(@"</div>");

            return sb.ToString();
        }

        private static string CreateHelp(OpenNodeAnnotationEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<details open>");
            sb.AppendLine(CreateExpanderTitle(Resources.NodeDocumentationIssueHelp));
            for (int i = 0; i < e.NodeInfos.Count(); i++)
            {
                try
                {
                    sb.AppendLine("<br>");
                    sb.AppendLine($"<strong>{"Message"}</strong>");
                    sb.AppendLine($"<p>{GetNthRowFromStringSplit(e.NodeInfos.ElementAt(i).Message, 0)}</p>");

                    var help = e.NodeInfos.ElementAt(i).Message.Split(new string[] {". "}, StringSplitOptions.None);
                    var html = help[1].Split(new string[] {"href="}, StringSplitOptions.None)[1];
                    var helpHtml =
                        DocumentationBrowserUtils.GetContentFromEmbeddedResource($"{RESOURCE_PREFIX + html}");

                    sb.AppendLine(helpHtml);
                }
                catch (Exception ex)
                {
                    LogWarning(ex.Message, WarningLevel.Mild);
                }
            }
            sb.AppendLine(@"</details>");

            return sb.ToString();
        }

        private static string CreateInputs(OpenNodeAnnotationEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<details open>");
            sb.AppendLine(CreateExpanderTitle(Resources.NodeDocumentationInputsAndOutputs));
            sb.AppendLine(CreateInputsAndOutputs(e));
            sb.AppendLine(@"</details>");

            return sb.ToString();
        }
        
        private static string CreateExpanderTitle(string title)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<summary>");
            if (title.Equals(Resources.NodeDocumentationIssueHelp))
            {
                sb.AppendLine($"<strong style=\"margin-right: 10px;\">{title}</strong>");
                sb.AppendLine("<span class=\"alertIcon\" style=\"margin-top: 4px; width: 14px; height: 14px\">");
                sb.AppendLine(@"</span>");
            }
            else
            {
                sb.AppendLine($"<strong>{title}</strong>");
            }
            sb.AppendLine("<span class=\"icon\">");
            sb.AppendLine(@"</span>");
            sb.AppendLine(@"</summary>");

            return sb.ToString();
        }

        private static string CreateNodeInfo(OpenNodeAnnotationEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            if (!e.Type.Equals(e.OriginalName))
            {
                sb.AppendLine($"<h2>{Resources.NodeDocumentationOriginalNodeName}</h2>");
                sb.AppendLine($"<p>{e.OriginalName}</p>");
            }
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
            sb.AppendLine($"<th class=\"table--border\">{Resources.NodeInputsName}</th>");
            sb.AppendLine($"<th class=\"table--border\">{Resources.NodeInputsType}</th>");
            sb.AppendLine($"<th class=\"table--border\">{Resources.NodeInputsDescription}</th>");
            sb.AppendLine($"<th class=\"table--border\">{Resources.NodeInputsValue}</th>");
            sb.AppendLine(@"</tr>");

            for (int i = 0; i < e.InputNames.Count(); i++)
            {
                sb.AppendLine("<tr class=\"table--border\">");
                sb.AppendLine($"<td class=\"table--border\">{e.InputNames.ElementAt(i)}</td>");
                sb.AppendLine($"<td class=\"table--border\">{GetTypeFromDescription(e.InputDescriptions.ElementAt(i))}</td>");
                sb.AppendLine($"<td class=\"table--border\">{GetNthRowFromStringSplit(e.InputDescriptions.ElementAt(i), 0)}</td>");
                sb.AppendLine($"<td class=\"table--border broken\">{GetDefaultValueFromDescription(e.InputDescriptions.ElementAt(i))}</td>");
                sb.AppendLine(@"</tr>");
            }

            sb.AppendLine(@"</td>");
            sb.AppendLine(@"</tr>");
            sb.AppendLine(@"</table>");


            sb.AppendLine($"<h2>{Resources.NodeDocumentationOutputs}</h2>");
            sb.AppendLine("<table class=\"table--border\">");
            sb.AppendLine("<tr class=\"table--border\">");
            sb.AppendLine($"<th class=\"table--border\">{Resources.NodeInputsName}</th>");
            sb.AppendLine($"<th class=\"table--border\">{Resources.NodeInputsDescription}</th>");
            sb.AppendLine($"<th class=\"table--border\">{Resources.NodeInputsDataType}</th>");
            sb.AppendLine(@"</tr>");

            for (int i = 0; i < e.OutputNames.Count(); i++)
            {
                sb.AppendLine(" <tr class=\"table--border\">");
                sb.AppendLine($"<td class=\"table--border\">{e.OutputNames.ElementAt(i)}</td>");
                sb.AppendLine($"<td class=\"table--border\">{GetNthRowFromStringSplit(e.OutputDescriptions.ElementAt(i), 0)}</td>");
                sb.AppendLine(@"</tr>");
            }

            sb.AppendLine(@"</td>");
            sb.AppendLine(@"</tr>");
            sb.AppendLine(@"</table>");

            return sb.ToString();
        }

         private static string GetTypeFromDescription(string element)
         {
             var stringArr = element.Split(new string[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);
             if(stringArr.Length > 2) return stringArr[2];
             return string.Empty;
         }

         private static string GetDefaultValueFromDescription(string element)
         {
             var stringArr = element.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
             foreach (var line in stringArr)
             {
                 if (line.ToLowerInvariant().Contains("default value"))
                 {
                     return line.Remove(0, 16);
                 }
             }
             return string.Empty;
         }

        private static string GetNthRowFromStringSplit(string element, int row)
         {
             var stringArr = element.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
             return stringArr[row];
         }

        internal static Action<ILogMessage> MessageLogged;
        private static void LogWarning(string msg, WarningLevel level) => MessageLogged?.Invoke(LogMessage.Warning(msg, level));

    }
}
