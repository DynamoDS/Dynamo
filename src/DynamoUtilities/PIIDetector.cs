
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
    public static class PIIDetector
    {
        /// <summary>
        /// Removes the PII data from a JSON workspace
        /// </summary>
        /// <param name="jsonObject"></param>
        /// <returns></returns>
        public static Tuple<string, JObject> RemovePIIData(JObject jsonObject)
        {
            JObject jObjectResult = jsonObject;
            string uuid = string.Empty;

            foreach (var properties in jObjectResult.Properties())
            {
                if (properties.Name == "Uuid")
                {
                    uuid = properties.Value.ToString();
                }
                else if (properties.Name == "Nodes")
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
            return new Tuple<string, JObject>(uuid, jObjectResult);
        }

        static string emailPattern = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
        static string websitePattern = @"(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+";
        static string directoryPattern = @"(^([a-z]|[A-Z]):(?=\\(?![\0-\37<>:""/\\|?*])|\/(?![\0-\37<>:""/\\|?*])|$)|^\\(?=[\\\/][^\0-\37<>:""/\\|?*]+)|^(?=(\\|\/)$)|^\.(?=(\\|\/)$)|^\.\.(?=(\\|\/)$)|^(?=(\\|\/)[^\0-\37<>:""/\\|?*]+)|^\.(?=(\\|\/)[^\0-\37<>:""/\\|?*]+)|^\.\.(?=(\\|\/)[^\0-\37<>:""/\\|?*]+))((\\|\/)[^\0-\37<>:""/\\|?*]+|(\\|\/)$)*()";
        static string creditCardPattern = @"(\d{4}[-, ]\d{4})";
        static string ssnPattern = @"\d{3}[- ]\d{2}[- ]\d{4}";
        static string ipPattern = @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";
        static string datePattern = @"(3[01]|[12][0-9]|0?[1-9])(\/|-)(1[0-2]|0?[1-9])\2([0-9]{2})?[0-9]{2}";

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
