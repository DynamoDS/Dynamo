﻿using System;
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

// modified from : https://gist.github.com/lontivero/593fc51f1208555112e0

namespace Dynamo.Docs
{
    class Program
    {
        static void Main(string[] args)
        {
            var asm = Assembly.LoadFrom(args[0]);
            var namespaces = GetAllNamespacesInAssemblyWithPublicMembers(asm);

            var docsFolder = CreateDocsFolder();

            var xml = XDocument.Load(args[1]);

            foreach (var ns in namespaces)
            {
                var cleanNamespace = ns.Replace('.', '_');
                var outputDir = Path.Combine(Path.GetFullPath(docsFolder.FullName), cleanNamespace);
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }
                var publicTypes = asm.GetTypes().Where(t => t.Namespace == ns).Where(t => t.IsPublic);

                foreach (var t in publicTypes)
                {
                    GenerateMarkdownDocumentForType(t, outputDir, xml);
                }
            }

            GenerateDocYaml();

            //var xml = File.ReadAllText(args[0]);
            //var doc = XDocument.Parse(xml);
            //var md = doc.Root.ToMarkDown();
            //Console.WriteLine(md);

            //System.IO.File.WriteAllText(@"result.md", md);
        }

        private static DirectoryInfo CreateDocsFolder()
        {
            var folderPath = DocRootPath();

            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }

            var docsFolder = Directory.CreateDirectory(folderPath);

            return docsFolder;
        }

        private static string DocRootPath()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "docs");
        }

        /// <summary>
        /// Generate the mkdocs.ymp file describing the structura of the documentation.
        /// </summary>
        /// <param name="filePath"></param>
        private static void GenerateDocYaml()
        {
            var ymlPath = Path.Combine(Directory.GetParent(DocRootPath()).FullName, "mkdocs.yml");
            using (var tw = File.CreateText(ymlPath))
            {
                tw.WriteLine("site_name: Dynamo");
                tw.WriteLine("pages:");
                tw.WriteLine("- Home: index.md");
                tw.WriteLine("- API:");

                foreach(var dirPath in Directory.GetDirectories(DocRootPath()))
                {
                    var di = new DirectoryInfo(dirPath);
                    var files = di.GetFiles();
                    if (!files.Any())
                    {
                        Console.WriteLine("No documentation files available in {0}", dirPath);
                        continue;
                    }

                    tw.WriteLine(string.Format("    - {0}:", di.Name.Replace('_','.')));

                    foreach(var fi in files)
                    {
                        var shortFileName = fi.Name.Split('.').First();
                        var shortDirPath = string.Format("{0}/{1}", di.Name, fi.Name);
                        tw.WriteLine(string.Format("      - '{0}' : '{1}'", shortFileName, shortDirPath));
                    }
                }

                tw.WriteLine("theme: readthedocs");
                tw.Flush();
            }
        }

        private static void GenerateMarkdownDocumentForType(Type t, string folder, XDocument xml)
        {
            var members = xml.Root.Element("members").Elements("member");

            //get all the generic members. Generic methods in XML has special characters 
            // List<T> in xml is List``1. So, replace them with correct values. Here, instead
            // of angular brackets we use [ ]
            var genericMembers = members.Where(x => x.Attribute("name").Value.Contains("``1") ||
                                                    x.Attribute("name").Value.Contains("``0"));
            foreach (var genericMember in genericMembers)
            {
                if (genericMember.Element("typeparam") != null)
                {
                    var typeParamelem = genericMember.Element(("typeparam"));
                    if (typeParamelem != null)
                    {
                        var typeParamName = typeParamelem.Attribute("name").Value;
                        var text = genericMember.Attribute("name").Value;
                        text = text.Replace("``1", "[" + typeParamName + "]");
                        text = text.Replace("``0", "[" + typeParamName + "]");

                        genericMember.Attribute("name").Value = text;
                    }
                }                                                  
            }
            var sb = new StringBuilder();

            sb.AppendLine("#" + t.Name);
            sb.Append(GetMarkdownForType(members, t.FullName));
            sb.AppendLine("---");

            sb.AppendLine("##Methods:  ");
            foreach (var method in t.GetMethods().Where(m => m.IsPublic))
            {
                var methodParams = method.GetParameters();
                var methodName = method.Name;
                //If the method is List<T,T>
                if (method.IsGenericMethod)
                {
                    var typeParam = method.GetGenericArguments();
                    //this returns T,T
                    var genericParamName = string.Join(",", typeParam.Select(ty => ty.Name));          
                    //this returns List<T,T>
                    methodName = methodName + "[" + genericParamName + "]";
                }
                var fullMethodName = methodParams.Any() ?
                    methodName + "(" + string.Join(",", methodParams.Select(pi => pi.ParameterType.FullName)) + ")" :
                    methodName;
                
                Debug.WriteLine(t.FullName + "." + fullMethodName);

                var current = GetMarkdownForMethod(members, t.FullName + "." + fullMethodName);
                var result = CheckAndAppendStability(current);

                sb.Append(result);
            }
            sb.AppendLine("---");

            sb.AppendLine("##Properties:  ");
            foreach (var property in t.GetProperties())
            {
                var current = GetMarkdownForProperty(members, t.FullName + "." + property.Name);
                var result = CheckAndAppendStability(current);
                sb.Append(result);
            }
            sb.AppendLine("---");

            sb.AppendLine("##Events:  ");
            foreach (var e in t.GetEvents())
            {
                var current = GetMarkdownForEvent(members, t.FullName + "." + e.Name);
                var result = CheckAndAppendStability(current);
                sb.Append(result);
            }
            sb.AppendLine("---");

            var fileName = t.Name + ".md";
            var filePath = Path.Combine(folder, t.Name + ".md");
            System.IO.File.WriteAllText(filePath, sb.ToString());
        }

        private static string CheckAndAppendStability(string current)
        {
            if (string.IsNullOrEmpty(current))
            {
                return current;
            }

            if (current.Contains(XmlToMarkdown.ApiStabilityStub))
            {
                return current;
            }

            return current += string.Format(XmlToMarkdown.ApiStabilityTemplate+"\n", 1);
        }

        private static string GetMarkdownForMethod(IEnumerable<XElement> members, string methodName)
        {
            return GetMarkdownForMember(members, string.Format("M:{0}", methodName));
        }

        private static string GetMarkdownForType(IEnumerable<XElement> members, string typeName)
        {
            return GetMarkdownForMember(members, string.Format("T:{0}", typeName));
        }

        private static string GetMarkdownForProperty(IEnumerable<XElement> members, string propertyName)
        {
            return GetMarkdownForMember(members, string.Format("P:{0}", propertyName));
        }

        private static string GetMarkdownForEvent(IEnumerable<XElement> members, string eventName)
        {
            return GetMarkdownForMember(members, string.Format("E:{0}", eventName));
        }

        private static string GetMarkdownForMember(IEnumerable<XElement> members, string memberName)
        {          
            var foundType = members.FirstOrDefault(e => e.Attribute("name").Value == memberName);
            if (foundType == null)
            {
                return string.Empty;
            }

            return foundType.ToMarkDown();
        }

        private static IEnumerable<string> GetAllNamespacesInAssemblyWithPublicMembers(Assembly assembly)
        {
            return assembly.GetTypes().Where(t => t.IsPublic).Select(t => t.Namespace).Distinct();
        }
    }

    static class XmlToMarkdown
    {
        private static string ApiStabilityTag = "api_stability";
        internal static string ApiStabilityStub = "stability=";
        internal static string ApiStabilityTemplate = ApiStabilityStub + "{0}";

        private static Dictionary<string,string> templates = 
            new Dictionary<string, string>
                {
                    {"doc", "## {0} ##\n\n{1}\n\n"},
                    {"type", "# {0}\n\n{1}\n"},
                    {"field", "##### {0}\n\n{1}\n"},
                    {"property", "##### {0}\n\n{1}\n"},
                    {"method", "##### {0}\n\n{1}\n"},
                    {"event", "##### {0}\n\n{1}\n"},
                    {"summary", "{0}\n\n"},
                    {"remarks", "\n\n>{0}\n\n"},
                    {"example", "_C# code_\n\n```c#\n{0}\n```\n\n"},
                    {"seePage", "[[{1}|{0}]]"},
                    {"seeAnchor", "[{1}]({0})"},
                    {"param", "|Name | Description |\n|-----|------|\n|{0}: |{1}|\n" },
                    {"exception", "[[{0}|{0}]]: {1}\n\n" },
                    {"returns", "Returns: {0}\n\n"},
                    {"none", ""},
                    {"typeparam", ""},
                    {"c", "`{0}`"},
                    {"search", ">{0}"},
                    {"notranslation", "\n\n>{0}\n\n"},
                    {ApiStabilityTag, ApiStabilityTemplate}
                };

        private static Func<string, XElement, string[]> d = 
            new Func<string, XElement, string[]>((att, node) => new[]
                {
                    node.Attribute(att).Value, 
                    node.Nodes().ToMarkDown()
                });

        private static Func<string, XElement, string[]> dType =
            new Func<string, XElement, string[]>((att, node) => new[]
                {
                    node.Attribute(att).Value.Split('.').Last(), 
                    node.Nodes().ToMarkDown()
                });

        private static Func<string, string, XElement, string[]> mType =
            new Func<string, string, XElement, string[]>((att1, att2, node) =>{ 
                var methodName = node.Attribute(att1).Value.Split(':').Last();
                //Seperate the method name and paramaeters (ex: Func(a,b) = [Func] + [a,b])
                var methodParams = methodName.Split('(', ')');   
                var stringList = new CommaDelimitedStringCollection();                
                if (methodParams.Count() > 1)
                {
                    methodName = methodParams[0]; //Store the method name
                    //Split the Parameters (ex: (a,b) = [a], [b]) 
                    methodParams = methodParams[1].Split(',');                                       
                    int i = 0;
                    foreach (var param in node.Elements(att2))
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
                                var url = ConstructUrl(methodParams[i]) + "/" + className;
                                var style = "color:#CC0000";
                                className = "<a href = " + url + " style= " + style + ">" + className + "</a>" ;
                            }
                            else
                            {
                                className = methodParams[i].Split('.').Last();  
                            }
                                                     
                            stringList.Add(className + " " + param.Attribute("name").Value);
                        }
                        i++;
                    }
                    if (stringList.Count > 0)
                    {                        
                        methodName = methodName + "(" + stringList.ToString() + ")";
                    }
                }                                                                                                      
               
                return new[]
                {
                    methodName.Contains("(")? methodName : methodName + "()", 
                    node.Nodes().ToMarkDown()
                };
            });
       
        
        private static Dictionary<string, Func<XElement, IEnumerable<string>>> methods = 
            new Dictionary<string, Func<XElement, IEnumerable<string>>>
                {
                    {"doc", x=> new[]{
                        x.Element("assembly").Element("name").Value,
                        x.Element("members").Elements("member").ToMarkDown()
                    }},
                    {"type", x=>dType("name", x)},
                    {"field", x=> d("name", x)},
                    {"property", x=> dType("name", x)},
                    {"method",x=>mType("name", "param", x)},
                    {"event", x=>dType("name", x)},
                    {"summary", x=> new[]{ x.Nodes().ToMarkDown() }},
                    {"remarks", x => new[]{x.Nodes().ToMarkDown()}},
                    {"example", x => new[]{x.Value.ToCodeBlock()}},
                    {"seePage", x=> d("cref", x) },
                    {"seeAnchor", x=> { var xx = d("cref", x); xx[0] = xx[0].ToLower(); return xx; }},
                    {"param", x => d("name", x) },
                    {"exception", x => d("cref", x) },
                    {"returns", x => new[]{x.Nodes().ToMarkDown()}},
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
                    {"api_stability", x => new []{x.Value}}
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
                    case 2: str = string.Format(templates[name], vals[0], vals[1]); break;
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

        static string ToCodeBlock(this string s)
        {
            var lines = s.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var blank = lines[0].TakeWhile(x => x == ' ').Count() - 4;
            return string.Join("\n", lines.Select(x => new string(x.SkipWhile((y, i) => i < blank).ToArray())));
        }

        static string ConstructUrl(String methodName)
        {            
            var appSettings = ConfigurationManager.AppSettings["ServerUrl"];
            var url = appSettings ?? String.Empty;
            if (url != String.Empty)
            {
                var nameSpace = methodName.Split('.');
                url = url + "/" + string.Join("_",nameSpace.Take(nameSpace.Length - 1));                
            }

            return url;
        }
    }
}
