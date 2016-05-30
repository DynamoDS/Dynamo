using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XmlDocToMarkdown
{
    public static class MarkDownExtensions
    {
        private static Dictionary<string, int> nodeDictionary = new Dictionary<string, int>()
        {            
            {"returns", 1},
            {"summary", 2},
            {"param", 3},
            {"typeparam", 4},
            {"remarks", 5},
            {"example", 6},
            {"exception",7},
            {"c", 8},
            {"search", 9},
            {"SeePage", 10},
            {"SeeAnchor", 11},
            {"api_stability",12},
            {"filterpriority",13},
            {"notranslation",14}
        };

        /// <summary>
        /// Reorder the xml nodes. this is only for methods
        /// see nodeDictionary for the order.
        /// </summary>
        /// <param name="listOfNodes">The list of nodes.</param>
        /// <returns>re-ordered nodes</returns>
        public static IEnumerable<XNode> ReOrderXMLElements(this IEnumerable<XNode> listOfNodes)
        {
            var newListOfNodes = new List<XNode>();
            var elements = new Dictionary<XElement, int>();
            foreach (var node in listOfNodes)
            {
                var element =  node as XElement;
                if (element != null)
                {
                    if (nodeDictionary.ContainsKey(element.Name.LocalName))
                    {
                        if (element.Name.LocalName == "returns")
                        {
                            if (element.Value == string.Empty || element.Value == "")
                            {
                                element.Value = "none";
                            }
                        }
                        elements.Add(element, nodeDictionary[element.Name.LocalName]);
                    }
                }
            }

            //for css table design, always have the first row empty if there 
            //are no type returns tags.
            if (!elements.Any(tr => tr.Key.ToString().Contains("returns")))
            {
                var ele = new XElement("returns", new XAttribute("name", "")) { Value = "none" };
                elements.Add(ele, 1);
            }

            var ordered = elements.OrderBy(x => x.Value);
            
            foreach (var kk in ordered)
            {
                newListOfNodes.Add(kk.Key);
            }

            return newListOfNodes;
        }


        /// <summary>
        /// Constructs the type of the return.
        /// Generics are updated. ex: IList''object to "IList<object>"
        /// If the return type is from Dynamo, then URL will be constructed
        /// </summary>
        /// <param name="T">The t.</param>
        /// <returns>return type</returns>
        public static string ConstructReturnType(this Type T)
        {
            string[] separators = new string[] { "``", "`", "(", ")" }; 
            var list = new List<String>();
            if (!T.IsGenericType)
            {
                if (T.FullName != null && T.FullName.Contains("Dynamo") && !T.IsEnum)
                {
                    var className = T.FullName.ReplaceSpecialCharacters("+", ".").Split('.').Last();                
                    return className.ConstructUrl(
                        T.FullName).ReplaceSpecialCharacters("+", "/");                    
                }
                return T.Name;
            }

            //for generic type
            string[] splitByChar = T.Name.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            var tempName = splitByChar[0];

            var typeParam = T.GetGenericArguments();
            //this returns T,T           
            if (typeParam.Any())
            {
                list.AddRange(typeParam.Select(ConstructReturnType));
            }

            if (list.Any())
            {
                var genericParamName = string.Join(",", list);
                return tempName + "<*" + genericParamName + "*>";
            }

            return string.Empty;
        }

        /// <summary>
        /// Formar the markdown
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="format">The format.</param>
        /// <returns>Formatted Mark down string</returns>
        public static string MarkDownFormat(this string name, string format)
        {
            var returnString = String.Empty;
            if (!String.IsNullOrEmpty(name))
            {
                switch (format)
                {
                    case "Bold":
                        //string can be Test or Test | stability.
                        var splits = name.Split('|');
                        if (splits.Count() > 1)
                        {
                            returnString = "**" + splits[0].Trim() + "**";
                            returnString = returnString + " | " + splits[1];
                        }
                        else
                        {
                            returnString = "**" + name + "**";
                        }
                        break;

                    case "Italic":
                        if (!string.IsNullOrEmpty(name))
                        {
                            returnString = "*" + name.Trim() + "*";
                        }
                        break;

                }
            }

            return returnString;
        }

        /// <summary>
        /// Constructs the URL.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>URL string</returns>
        public static string ConstructUrl(this String className,String methodName)
        {
            var appSettings = ConfigurationManager.AppSettings["ServerUrl"];
            var url = appSettings ?? String.Empty;
            if (url != String.Empty)
            {
                var nameSpace = methodName.Split('.');
                url = url + "/" + string.Join("_", nameSpace.Take(nameSpace.Length - 1));
            }                       
            return "[" + className + "]" + "(" + url + "/" + className + ")";
        }

        /// <summary>
        /// Replaces the special characters.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <param name="toReplace">To replace.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns>string value</returns>
        public static string ReplaceSpecialCharacters(this string className, string toReplace, string delimiter)
        {
            if (string.IsNullOrEmpty(className))
                return string.Empty;

            var returnString =  className.Replace(toReplace, delimiter);
            return returnString;
        }

    }
}
