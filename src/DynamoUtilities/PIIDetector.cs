
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Dynamo.Utilities
{
    /// <summary>
    /// Helper Class for removing PII Data from a JSON workspace
    /// </summary>
    internal static class PIIDetector
    {
        /// <summary>
        /// Removes the PII data from a JSON workspace
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        public static JObject RemovePIIData(JObject jsonObject)
        {
            JObject jObjectResult = jsonObject;

            foreach (var properties in jObjectResult.Properties())
            {
                if (properties.Name == "Nodes")
                {
                    if (properties.Value.Type == JTokenType.Array)
                    {
                        var nodes = (JArray)properties.Value;
                        foreach (JObject node in nodes)
                        {
                            var nodeProperties = node.Children<JProperty>();

                            JProperty inputValue = nodeProperties.FirstOrDefault(x => x.Name == "InputValue");
                            JProperty hintPath = nodeProperties.FirstOrDefault(x => x.Name == "HintPath");
                            JProperty code = nodeProperties.FirstOrDefault(x => x.Name == "Code");

                            if (inputValue != null) { inputValue.Value = RemovePIIData((string)inputValue.Value); }
                            if (hintPath != null) { hintPath.Value = RemovePIIData((string)hintPath.Value); }
                            if (code != null) { code.Value = RemovePIIData((string)code.Value); }
                        }
                    }
                }
                else if (properties.Name == "View")
                {
                    var view = (JObject)properties.Value;
                    var viewProperties = view.Children<JProperty>();
                    var annotations = viewProperties.FirstOrDefault(x => x.Name == "Annotations");

                    if (annotations != null)
                    {
                        if (annotations.Value.Type == JTokenType.Array)
                        {
                            var annotationsList = (JArray)annotations.Value;

                            foreach (JObject annotation in annotationsList)
                            {
                                var annotationProperties = annotation.Children<JProperty>();
                                JProperty title = annotationProperties.FirstOrDefault(x => x.Name == "Title");
                                if (title != null) { title.Value = RemovePIIData((string)title.Value); }
                            }
                        }
                    }
                }
            }
            return jObjectResult;
        }

        static string emailPattern = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        static string websitePattern = @"(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+";
        static string directoryPattern = @"(^([a-z]|[A-Z]):(?=\\(?![\0-\37<>:""/\\|?*])|\/(?![\0-\37<>:""/\\|?*])|$)|^\\(?=[\\\/][^\0-\37<>:""/\\|?*]+)|^(?=(\\|\/)$)|^\.(?=(\\|\/)$)|^\.\.(?=(\\|\/)$)|^(?=(\\|\/)[^\0-\37<>:""/\\|?*]+)|^\.(?=(\\|\/)[^\0-\37<>:""/\\|?*]+)|^\.\.(?=(\\|\/)[^\0-\37<>:""/\\|?*]+))((\\|\/)[^\0-\37<>:""/\\|?*]+|(\\|\/)$)*()";
        static string creditCardPattern = @"(\d{4}[-, ]\d{4})";
        static string ssnPattern = @"\d{3}[- ]\d{2}[- ]\d{4}";
        static string ipPattern = @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";
        static string datePattern = @"\d{1,2}[/-]\d{1,2}[/-]\d{2,4}";

        public static JToken GetNodeById(JObject jsonWorkspace,string nodeId)
        {
            return jsonWorkspace["Nodes"].Where(t => t.Value<string>("Id") == nodeId).Select(t => t).FirstOrDefault();
        }

        public static JToken GetNodeValue(JObject jsonWorkspace, string nodeId,string propertyName)
        {
            var node = jsonWorkspace["Nodes"].Where(t => t.Value<string>("Id") == nodeId).Select(t => t).FirstOrDefault();
            var property = node.Children<JProperty>().FirstOrDefault(x => x.Name == propertyName);
            return property.Value;
        }

        public static JToken GetNoteValue(JObject jsonWorkspace, string nodeId)
        {
            var x = jsonWorkspace["View"]["Annotations"];
            var note = jsonWorkspace["View"]["Annotations"].Where(t => t.Value<string>("Id") == nodeId).Select(t => t).FirstOrDefault();
            var property = note.Children<JProperty>().FirstOrDefault(x => x.Name == "Title");
            return property.Value;
        }

        internal static bool ContainsEmail(string value) { return new Regex(emailPattern).Match(value).Success; }
        internal static bool ContainsWebsite(string value) { return new Regex(websitePattern).Match(value).Success; }
        internal static bool ContainsDirectory(string value) { return new Regex(directoryPattern).Match(value).Success; }
        internal static bool ContainsCreditCard(string value) { return new Regex(creditCardPattern).Match(value).Success; }
        internal static bool ContainsSSN(string value) { return new Regex(ssnPattern).Match(value).Success; }
        internal static bool ContainsIpAddress(string value) { return new Regex(ipPattern).Match(value).Success; }
        internal static bool ContainsDate(string value) { return new Regex(datePattern).Match(value).Success; }

        /// <summary>
        /// Removes the PII data based on the information patterns
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static string RemovePIIData(string data)
        {
            string result;
            result = Regex.Replace(data, emailPattern, "");
            result = Regex.Replace(result, websitePattern, ""); 
            result = Regex.Replace(result, directoryPattern, "");
            result = Regex.Replace(result, creditCardPattern, "");
            result = Regex.Replace(result, ssnPattern, "");
            result = Regex.Replace(result, ipPattern, "");
            result = Regex.Replace(result, datePattern, "");

            return result;
        }
    }
}
