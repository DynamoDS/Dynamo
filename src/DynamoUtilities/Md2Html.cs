using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DynamoUtilities.Properties;
using Newtonsoft.Json.Linq;

namespace Dynamo.Utilities
{
    /// <summary>
    /// Utilities for converting Markdown to html and for sanitizing html
    /// The Md2Html command line tool is used for doing the actual conversion/santizing
    /// (This tool is delivered as part of Dynamo)
    /// This class is not thread safe so please instantiate this class in the same thread that
    /// you intend to use.
    /// But multiple instances of the class is supported.
    /// </summary>

    internal class Md2Html : CLIWrapper
    {
        private string relativePath = Path.Combine(@"Md2Html", @"Md2Html.exe");
        private int processCommunicationTimeoutms = 5000;

        /// <summary>
        /// Constructor
        /// Start the CLI tool and keep it around
        /// </summary>
        internal Md2Html()
        {
            StartProcess(relativePath,string.Empty);
        }

        /// <summary>
        /// Kill the CLI tool, if still running
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            KillProcess();
        }

        /// <summary>
        /// Kill the CLI tool, if still running
        /// </summary>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Converts a markdown string into Html.
        /// </summary>
        /// <param name="mdString"></param>
        /// <param name="mdPath"></param>
        /// <returns>Returns converted markdown as html</returns>
        internal string ParseMd2Html(string mdString, string mdPath)
        {
            if (!started)
            {
                return GetCantStartErrorMessage();
            }

            try
            {
                process.StandardInput.WriteLine(@"<<<<<Convert>>>>>");
                process.StandardInput.WriteLine(mdPath);
                EncodeMDContent(ref mdString);
                process.StandardInput.WriteLine(mdString);
                process.StandardInput.WriteLine(@"<<<<<Eod>>>>>");
            }
            catch (Exception e) when (e is IOException || e is ObjectDisposedException)
            {
                KillProcess();
                return GetCantCommunicateErrorMessage();
            }

            return GetData(processCommunicationTimeoutms);
        }

        private void EncodeMDContent(ref string mdContent)
        {
            //Encode to base64 due that the Tools / Md2Html console app is using a different encoding and special characters are lost when sending info to Stdio

            //Get the MD file content and encode it
            EncodeBase64(ref mdContent, @"#+\s[^\n]*\n(.*?)(?=\n##?\s|$)");

            //Get the MD file headers and encode it
            EncodeBase64(ref mdContent, @"#+\s(.*?\n)");
        }

        /// <summary>
        /// This method will apply the Regular Expression provided over the md content and then encode the md content to base64 
        /// </summary>
        /// <param name="mdContent">MD file content read usually from the fallback_docs folder</param>
        /// <param name="regEx">Regular Expression that will be applied over the md content</param>
        private void EncodeBase64(ref string mdContent, string regEx)
        {
            Regex rxExp = new Regex(regEx, RegexOptions.Singleline);
            MatchCollection matches = rxExp.Matches(mdContent);
            foreach (Match match in matches)
            {
                if (match.Groups.Count == 0) continue;

                var UTF8Content = match.Groups[1].Value.Trim();

                //When the line starts with ![ then means that the value is a image then we don't encode the content
                if (!string.IsNullOrEmpty(UTF8Content) && !UTF8Content.StartsWith("!["))
                {
                    var UTF8ContentBytes = Encoding.UTF8.GetBytes(UTF8Content);
                    var contentBase64String = Convert.ToBase64String(UTF8ContentBytes);
                    mdContent = mdContent.Replace(UTF8Content, contentBase64String);                   
                }
            }                 
        }

        internal static void DecodeHTMLContent(ref string htmlContent)
        {
            //Usually the response from Md2Html.exe are in the format <<<<< response >>>>> so we avoid decoding those strings
            if (!htmlContent.Contains("<<<<<"))
            {
                //Decode HTML File paragraphs
                DecodeBase64(ref htmlContent, @"<p>(.*?)</p>");

                //Decode HTML File headers
                DecodeBase64(ref htmlContent, @"<h2\s.*>(.*?)</h2>");
            }
        }

        internal static void DecodeBase64(ref string htmlContent, string regExp)
        {
            if (!string.IsNullOrEmpty(htmlContent))
            {
                //TODO- missing to support other HTML tags
                var Matches = Regex.Matches(htmlContent, regExp, RegexOptions.IgnoreCase);
                string encodedString = string.Empty;
                if (Matches.Count > 0)
                {
                    //Add validation is groups is null
                    encodedString = Matches[0].Groups[1].Value;
                    if (!string.IsNullOrEmpty(encodedString) && !encodedString.Trim().StartsWith("<img"))
                    {
                        //Due that in some cases there are nested tags inside <p> or <h2> and if the info is not encoded then it will send an exception so we will leave it as it is
                        try
                        {
                            var base64Line = Convert.FromBase64String(encodedString);
                            var decodedString = Encoding.UTF8.GetString(base64Line);
                            htmlContent = htmlContent.Replace(encodedString, decodedString);
                        }
                        catch (Exception) { }
                    }
                }
            }
        }

        /// <summary>
        /// Sanitize Html
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Returns Sanitized Html or an empty string if no sanitization was needed</returns>
        internal string SanitizeHtml(string content)
        {
            if (!started)
            {
                return GetCantStartErrorMessage();
            }

            try
            {
                process.StandardInput.WriteLine(@"<<<<<Sanitize>>>>>");
                process.StandardInput.WriteLine(content);
                process.StandardInput.WriteLine(@"<<<<<Eod>>>>>");
            }
            catch (Exception e) when (e is IOException || e is ObjectDisposedException)
            {
                KillProcess();
                return GetCantCommunicateErrorMessage();
            }

            return GetData(processCommunicationTimeoutms);
        }

        /// <summary>
        /// Can't start Error message
        /// </summary>
        /// <returns>Returns error message</returns>
        protected override string GetCantStartErrorMessage()
        {
            return string.Format(Resources.Md2HtmlCantStartError, GetToolPath(relativePath));
        }

        /// <summary>
        /// Can't communicate Error message
        /// </summary>
        /// <returns>Returns error message</returns>
        protected override string GetCantCommunicateErrorMessage()
        {
            return string.Format(Resources.Md2HtmlCantCommunicateError, GetToolPath(relativePath));
        }
    }
}
