using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using XmlDocToMarkdown;

namespace XmlDocToMarkdown
{
    /// <summary>
    /// this class constructs the actual markdown
    /// </summary>
    public static class JSONMarkDown
    {
        private static string ApiStabilityTag = "api_stability";
        internal static string ApiStabilityStub = "stability=";
        internal static string ApiStabilityTemplate = "| stability index:" + "{0}";
        internal static string returnType;

        /// <summary>
        /// Gets or sets the method return type
        /// </summary>        
        internal static string ReturnType
        {
            get { return returnType; }
            set
            {
                if (value == "Void")
                    returnType = "void";
                else
                    returnType = value;
            }
        }

        private static string propertySetType;

        /// <summary>
        /// Gets or sets the type of the property set.
        /// value : {get; set;}
        /// </summary>        
        internal static string PropertySetType
        {
            get
            {
                return "{" + propertySetType + "}";
            }
            set
            {
                propertySetType = value;
            }
        }

        private static Dictionary<string, string> templates =
                new Dictionary<string, string>
                    {
                    {"doc", "{0}{1}"},
                    {"type", "{0}"},
                    {"field", "{0}{1}"},
                    {"property", "{0}{1}"},
                    {"method", "{0}{1}"},
                    {"event", "{0}{1}"},
                    {"summary", "{0}"},
                    {"remarks", "{0}"},
                    {"example", "{0}"},
                    {"seePage", "{0}"},
                    {"seeAnchor", "{1}{0}"},
                    {"param", "{0}{1}" },
                    {"exception", "{0}|{0} {1}" },
                    {"returns", "{0}"},
                    {"none", ""},
                    {"typeparam", "{0}{1}" },
                    {"c", "{0}"},
                    {ApiStabilityTag, ApiStabilityTemplate}
                    };

        private static Func<string, XElement, string[]> d =
            new Func<string, XElement, string[]>((att, node) => new[]
                {
                    node.Attribute(att).Value.ToClassString(),
                    node.Nodes().NodeMarkDown()
                });

        /// <summary>
        /// Template for constructors
        /// </summary>
        private static Func<string, XElement, string[]> tType =
            new Func<string, XElement, string[]>((att, node) => new[]
                {
                    node.Nodes().NodeMarkDown()
                });
        /// <summary>
        /// Template for properties
        /// </summary>
        private static Func<string, string, XElement, string[]> pType =
            new Func<string, string, XElement, string[]>((att1, att2, node) =>
            {
                var methodName = node.Attribute(att1).Value.Split('.').Last();
                methodName = methodName + " " + JSONMarkDown.PropertySetType;
                var convertedMethodName = ConvertGenericParameters(node, methodName, att2);
                convertedMethodName = string.Join(" ", ReturnType, convertedMethodName);
                return new[]
                {
                    convertedMethodName,
                    node.Nodes().NodeJSONMarkDown()
                };
            });

        /// <summary>
        /// Template for events
        /// </summary>
        private static Func<string, string, XElement, string[]> eType =
           new Func<string, string, XElement, string[]>((att1, att2, node) =>
           {
               var methodName = node.Attribute(att1).Value.Split('.').Last();
               var convertedMethodName = ConvertGenericParameters(node, methodName, att2);
               convertedMethodName = string.Join(" ", ReturnType, convertedMethodName);
               return new[]
                {
                    convertedMethodName,
                    node.Nodes().NodeJSONMarkDown()
                };
           });

        /// <summary>
        /// Template for methods
        /// </summary>f
        private static Func<string, string, XElement, object[]> mType =
            (att1, att2, node) =>
            {
                var methodName = node.Attribute(att1).Value.Split(':').Last();
                var convertedMethodName = ConvertGenericParameters(node, methodName, att2);
                convertedMethodName = string.Join(" ", ReturnType, convertedMethodName);
                var nodes = node.Nodes().ReOrderXMLElements();
                return new object[]
                {
                    convertedMethodName,
                    nodes.ToJSONTableMarkDown(),
                };
            };


        /// <summary>
        /// Add stability index
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns>Returns the name and its stability index</returns>
        private static string CheckAndAppendStability(XElement node, string methodName)
        {
            if (node.Elements("api_stability").Any())
            {
                methodName = node.Elements("api_stability").Select(stabilityTag => stabilityTag.Value)
                    .Aggregate(methodName, (current, value) =>
                        current + string.Format(" " + XmlToMarkdown.ApiStabilityTemplate, value));
            }
            else
            {
                methodName = methodName + string.Format(" " + XmlToMarkdown.ApiStabilityTemplate, 1);
            }

            return methodName;
        }

        /// <summary>
        /// Converts the generic parameters.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="attribute">attribute = name / param / typeparam.</param>
        /// <returns>converted name</returns>
        public static string ConvertGenericParameters(XElement node, string methodName, string attribute)
        {
            //Seperate the method name and paramaeters (ex: Func(a,b) = [Func] + [a,b])
            var methodParams = methodName.Split('(', ')');
            var stringList = new CommaDelimitedStringCollection();
            if (methodParams.Count() > 1)
            {
                //Store the method name. the method name is Dynamo.Test.Test(int).
                //so splitting by . to get the method name as Test(int)
                methodName = methodParams[0].Split('.').Last();
                //Split the Parameters (ex: (a,b) = [a], [b]) 
                methodParams = methodParams[1].Split(',');
                int i = 0;
                foreach (var param in node.Elements(attribute))
                {
                    //when you add a parameter to a function, there is a possibility that comment is 
                    //not updated immediately. In that case add the param name to only those parameters 
                    //in the method name
                    if (methodParams.Count() > i)
                    {
                        //Extract only the classname , not the entire namespace
                        var className = string.Empty;
                        if (methodParams[i].Contains("Dynamo"))
                        {
                            className = methodParams[i].Split('.').Last();
                            //className = className.ConstructUrl(methodParams[i]);
                        }
                        else
                        {
                            className = methodParams[i].Split('.').Last();
                        }

                        stringList.Add(className + " " + param.Attribute("name").Value);
                    }
                    i++;
                }
                //case 1: Param tag on the method.
                if (stringList.Count > 0)
                {
                    methodName = methodName + "(" + stringList + ")";
                }
                //case 1: No Param tag on the method.        
                else
                {
                    methodName = methodName + "(" + methodParams[0] + ")";
                }

                //add api_stability as super script. If there is no stability tag
                //then add a default value.
               // methodName = CheckAndAppendStability(node, methodName);
            }
            else
            {
                if (methodName.Contains("."))
                {
                    methodName = methodName.Split('.').Last();
                    methodName = methodName + "( )";
                }
                //methodName = "&nbsp;&nbsp;" + methodName;
               // methodName = CheckAndAppendStability(node, methodName);
            }

            return methodName;
        }

        /// <summary>
        /// Template methods
        /// </summary>
        private static Dictionary<string, Func<XElement, IEnumerable<object>>> methods =
            new Dictionary<string, Func<XElement, IEnumerable<object>>>
                {
                    {"doc", x=> new[]{
                        x.Element("assembly").Element("name").Value,
                        x.Element("members").Elements("member").ToMarkDown()
                    }},
                    {"type", x=>tType("name", x)},
                    {"field", x=> d("name", x)},
                    {"property", x=> pType("name","param", x)},
                    {"method",x=>mType("name", "param", x)},
                    {"event", x=>eType("name", "param",  x)},
                    {"summary", x=> new[]{ x.Nodes().NodeJSONMarkDown() }},
                    {"remarks", x => new[]{x.Nodes().NodeJSONMarkDown()}},
                    {"example", x => new[]{x.Value.ToCodeBlock()}},
                    {"seePage", x=> d("cref", x) },
                    {"seeAnchor", x=> { var xx = d("cref", x); xx[0] = xx[0].ToLower(); return xx; }},
                    {"param", x => d("name", x) },
                    {"exception", x => d("cref", x) },
                    {"returns", x => new[]{x.Nodes().NodeJSONMarkDown()}},
                    {"none", x => new string[0]},
                    {"typeparam", x => d("name", x)},
                    {"paramref", x => new string[0]},
                    {"value", x => new string[0]},
                    {"c", x => new[]{x.Value}},
                    {"list", x => new string[0]},
                    // dynamo specific
                    {"notranslation", x => new string[0]},
                    {"search", x => new string[0]},
                    {"filterpriority", x => new string[0]},
                };

        /// <summary>
        /// Converts the node value to markdown format.
        /// </summary>
        /// <param name="e">The node.</param>
        /// <returns>Markdown string</returns>
        internal static string ToJSONMarkDown(this XNode e)
        {
            string name;
            if (e.NodeType == XmlNodeType.Element)
            {
                var el = (XElement)e;
                name = el.Name.LocalName;
                if (name == "member")
                {
                    switch (el.Attribute("name").Value[0])
                    {
                        case 'F': name = "field"; break;
                        case 'P': name = "property"; break;
                        case 'T': name = "type"; break;
                        case 'E': name = "event"; break;
                        case 'M': name = "method"; break;
                        default: name = "none"; break;
                    }
                }
                if (name == "see")
                {
                    var anchor = el.Attribute("cref").Value.StartsWith("!:#");
                    name = anchor ? "seeAnchor" : "seePage";
                }

                if (!methods.ContainsKey(name))
                {
                    return "";
                }

                var vals = methods[name](el).ToArray();

                string str = "";
                //switch (vals.Length)
                //{
                //    case 1:
                //        str = string.Format(templates[name], vals[0]);
                //        if (name.Equals("returns") && (e.Parent != null))
                //        {
                //            switch ((string)vals[0])
                //            {
                //                case "":
                //                case "none":
                //                    Console.WriteLine(e.Parent.FirstAttribute.Value);
                //                    break;
                //            }
                //        }
                //        break;
                //    case 2:
                //        str = (string)vals[0] == "" ? "" : string.Format(templates[name], vals[0], vals[1]);

                //        if (name.Equals("param"))
                //        {
                //            if (String.IsNullOrEmpty((string)vals[1]))
                //            {
                //                Console.WriteLine(e.Parent.FirstAttribute.Value);
                //            }
                //        }
                //        break;
                //    case 3: str = string.Format(templates[name], vals[0], vals[1], vals[2]); break;
                //    case 4: str = string.Format(templates[name], vals[0], vals[1], vals[2], vals[3]); break;
                //}

                if (vals.Length > 0) str = vals[0].ToString();
                return str;
            }

            if (e.NodeType == XmlNodeType.Text)
                return Regex.Replace(((XText)e).Value.Replace('\n', ' '), @"\s+", " ");

            return "";
        }

        internal static string ToJSONMarkDown(this IEnumerable<XNode> es)
        {
            return es.Aggregate("", (current, x) => current + x.ToJSONMarkDown());
        }

        /// <summary>
        /// This is for methods.
        /// </summary>
        /// <param name="es">The es.</param>
        /// <returns></returns>
        public static string ToJSONTableMarkDown(this IEnumerable<XNode> es)
        {
            return es.Aggregate("", (current, x) => current + x.ToJSONMarkDown());
        }

        public static string NodeJSONMarkDown(this IEnumerable<XNode> es)
        {
            return es.Aggregate("", (current, x) => current + x.ToJSONMarkDown());
        }

        static string ToCodeBlock(this string s)
        {
            var lines = s.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var blank = lines[0].TakeWhile(x => x == ' ').Count() - 4;
            return string.Join("\n", lines.Select(x => new string(x.SkipWhile((y, i) => i < blank).ToArray())));
        }

        static string ToClassString(this string s)
        {
            if (s.Contains("T:"))
            {
                var className = s.Replace("T:", "");
                var methodName = className.Split('.').Last();
                if (className.Contains("Dynamo"))
                {
                    var url = className.ConstructUrl(methodName);
                    return url;
                }

                return methodName;
            }

            return s;
        }
    }
}
