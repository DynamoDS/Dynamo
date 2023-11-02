
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
        const string Nodes = "Nodes";
        const string InputValue = "InputValue";
        const string HintPath = "HintPath";
        const string Code = "Code";
        const string View = "View";
        const string Annotations = "Annotations";
        const string Title = "Title";

        /// <summary>
        /// Removes the PII data from a JSON workspace indicating the status of the result
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        public static Tuple<JObject,bool> RemovePIIData(JObject jsonObject)
        {
            JObject jObjectResult = jsonObject;
            bool removeResult = true;

            try
            {
                foreach (var properties in jObjectResult.Properties())
                {
                    if (properties.Name == Nodes)
                    {
                        var nodes = (JArray)properties.Value;
                        foreach (JObject node in nodes)
                        {
                            node.Children<JProperty>().ToList().ForEach(property =>
                            {
                                if (property.Name == InputValue || property.Name == HintPath || property.Name == Code)
                                {
                                    property.Value = RemovePIIData((string)property.Value);
                                }
                            });
                        }
                    }
                    else if (properties.Name == View)
                    {
                        var view = (JObject)properties.Value;
                        var viewProperties = view.Children<JProperty>();

                        var annotations = (JArray)viewProperties.FirstOrDefault(x => x.Name == Annotations).Value;
                        foreach (JObject annotation in annotations)
                        {
                            annotation.Children<JProperty>().ToList().ForEach(property =>
                            {
                                if (property.Name == Title)
                                {
                                    property.Value = RemovePIIData((string)property.Value);
                                }
                            });
                        }
                    }
                }
            }
            catch
            {
                removeResult = false;
            }

            return new Tuple<JObject, bool>(jObjectResult, removeResult);
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
