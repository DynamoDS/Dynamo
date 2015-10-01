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
    public static class XmlToMarkdown
    {
        private static string ApiStabilityTag = "api_stability";
        internal static string ApiStabilityStub = "stability=";
        internal static string ApiStabilityTemplate = "| stability index:" + "{0}";
        internal static string returnType;

        internal static string ReturnType
        {
            get { return returnType.MarkDownFormat("Italic"); }
            set
            {
                if (value == "Void")
                    returnType = "void";
                else
                    returnType = value;
            }
        }

        private static string propertySetType;

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
                    {"doc", "## {0} ##\n\n{1}\n\n"},
                    {"type", "{0}\n"},
                    {"field", "{0}\n\n{1}\n"},
                    {"property", "|{0}  \n| ------------- | :--------------- \n{1}\n"},
                    {"method", "|{0}  \n| ------------- | :--------------- \n{1}"},
                    {"event", "|{0}  \n| ------------- | :--------------- \n{1}\n"},
                    {"summary", "| {0}\n"},
                    {"remarks", "| {0}\n"},
                    {"example", "| **Example:** | _C# code_\n\n```c#\n{0}\n```\n"},
                    {"seePage", "[[{1}|{0}]]"},
                    {"seeAnchor", "[{1}]({0})"},
                    {"param", "| **{0}**\n|{1}\n" },
                    {"exception", "| **[[{0}|{0}]]:** | {1}\n" },
                    {"returns", "| **Return Value:** {0}\n"},
                    {"none", ""},
                    {"typeparam", "| **TypeParam : *{0}* =** {1}\n" },
                    {"c", "| `{0}`\n"},                   
                    {ApiStabilityTag, ApiStabilityTemplate}
                };

        private static Func<string, XElement, string[]> d =
            new Func<string, XElement, string[]>((att, node) => new[]
                {
                    node.Attribute(att).Value, 
                    node.Nodes().NodeMarkDown()
                });

        private static Func<string, XElement, string[]> tType =
            new Func<string, XElement, string[]>((att, node) => new[]
                {
                    node.Nodes().NodeMarkDown()                  
                });
        private static Func<string, string, XElement, string[]> pType =
            new Func<string, string, XElement, string[]>((att1, att2, node) =>
            {
                var methodName = node.Attribute(att1).Value.Split('.').Last();
                methodName = methodName + " " + XmlToMarkdown.PropertySetType;
                var convertedMethodName = ConvertGenericParameters(node, methodName, att2);               
                convertedMethodName = string.Join(" ", ReturnType, convertedMethodName.MarkDownFormat("Bold"));
                return new[]
                {
                    convertedMethodName,
                    node.Nodes().NodeMarkDown()
                };
            });

        private static Func<string, string, XElement, string[]> eType =
           new Func<string, string, XElement, string[]>((att1, att2, node) =>
           {
               var methodName = node.Attribute(att1).Value.Split('.').Last();
               var convertedMethodName = ConvertGenericParameters(node, methodName, att2);
               convertedMethodName = string.Join(" ", ReturnType, convertedMethodName.MarkDownFormat("Bold"));
               return new[]
                {
                    convertedMethodName,
                    node.Nodes().NodeMarkDown()
                };
           });

        private static Func<string, string, XElement, object[]> mType =
            (att1, att2, node) =>
            {
                var methodName = node.Attribute(att1).Value.Split(':').Last();
                var convertedMethodName = ConvertGenericParameters(node, methodName, att2);
                convertedMethodName = string.Join(" ", ReturnType, convertedMethodName.MarkDownFormat("Bold"));
                var nodes = node.Nodes().ReOrderXMLElements();
                return new object[]
                {                   
                    convertedMethodName,
                    nodes.ToTableMarkDown(),
                };
            };


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
                            className = className.ConstructUrl(methodParams[i]);
                        }
                        else
                        {
                            className = methodParams[i].Split('.').Last();
                        }

                        stringList.Add(className.MarkDownFormat("Italic") + " " + param.Attribute("name").Value);
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
                methodName = "&nbsp;&nbsp;" + methodName;
                methodName = CheckAndAppendStability(node, methodName);
            }
            else
            {
                if (methodName.Contains("."))
                {
                    methodName = methodName.Split('.').Last();
                    methodName = methodName + "( )";
                }
                methodName = "&nbsp;&nbsp;" + methodName;
                methodName = CheckAndAppendStability(node, methodName);
            }

            return methodName;
        }

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
                    {"summary", x=> new[]{ x.Nodes().NodeMarkDown() }},
                    {"remarks", x => new[]{x.Nodes().NodeMarkDown()}},
                    {"example", x => new[]{x.Value.ToCodeBlock()}},
                    {"seePage", x=> d("cref", x) },
                    {"seeAnchor", x=> { var xx = d("cref", x); xx[0] = xx[0].ToLower(); return xx; }},
                    {"param", x => d("name", x) },
                    {"exception", x => d("cref", x) },
                    {"returns", x => new[]{x.Nodes().NodeMarkDown()}},
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

        internal static string ToMarkDown(this XNode e)
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
                switch (vals.Length)
                {
                    case 1: str = string.Format(templates[name], vals[0]); break;
                    case 2:
                        //return empty row for TypeParam.
                        str = (string) vals[0] == "" ? "| &nbsp;\n" : string.Format(templates[name], vals[0], vals[1]);
                        break;
                    case 3: str = string.Format(templates[name], vals[0], vals[1], vals[2]); break;
                    case 4: str = string.Format(templates[name], vals[0], vals[1], vals[2], vals[3]); break;
                }

                return str;
            }

            if (e.NodeType == XmlNodeType.Text)
                return Regex.Replace(((XText)e).Value.Replace('\n', ' '), @"\s+", " ");

            return "";
        }

        internal static string ToMarkDown(this IEnumerable<XNode> es)
        {
            return es.Aggregate("", (current, x) => current + x.ToMarkDown());
        }

        internal static string ToTableMarkDown(this IEnumerable<XNode> es)
        {
            return es.Aggregate("", (current, x) => current + x.ToMarkDown());
        }

        internal static string NodeMarkDown(this IEnumerable<XNode> es)
        {
            return es.Aggregate("", (current, x) => current + x.ToMarkDown());
        }

        static string ToCodeBlock(this string s)
        {
            var lines = s.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var blank = lines[0].TakeWhile(x => x == ' ').Count() - 4;
            return string.Join("\n", lines.Select(x => new string(x.SkipWhile((y, i) => i < blank).ToArray())));
        }
    }
}
