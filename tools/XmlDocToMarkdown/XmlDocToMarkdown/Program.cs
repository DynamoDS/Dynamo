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

// modified from : https://gist.github.com/lontivero/593fc51f1208555112e0

namespace Dynamo.Docs
{
    class Program
    {
        private static string[] separators = new string[] { "``", "`","(",")"};
        private const string lt = "<*";
        private const string gt = "*>";
        private static readonly string[] repoHeader =
        {
            "site_name: Dynamo",
            "repo_url: http://dynamods.github.io/DynamoAPI/",
            "site_author: Dynamo",
            "pages:",
            "- Home: index.md",
            "- API:"
        };

        private const string themedir = "theme_dir: Theme";
        private static string returnType;
       
        static void Main(string[] args)
        {
            var asm = Assembly.LoadFrom(args[0]);
            var namespaces = GetAllNamespacesInAssemblyWithPublicMembers(asm);

            var docsFolder = CreateDocsFolder();

            if (docsFolder != null)
            {
                var xml = XDocument.Load(args[1]);

                HandleConstructors(xml);
                HandleGenerics(xml);

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
            }
            else
            {
                Console.WriteLine("Cannot create docs folder.");
            }
        }

        private static DirectoryInfo CreateDocsFolder()
        {
            try
            {
                var folderPath = DocRootPath();

                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true);
                }

                var docsFolder = Directory.CreateDirectory(folderPath);

                return docsFolder;
            }

            catch (Exception ex)
            {
                Console.WriteLine("Cannot create docs folder." + ex.Message);
                return null;
            }
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
                foreach (var str in repoHeader)
                {
                    tw.WriteLine(str);
                }                

                foreach (var dirPath in Directory.GetDirectories(DocRootPath()))
                {
                    var di = new DirectoryInfo(dirPath);
                    var files = di.GetFiles();
                    if (!files.Any())
                    {
                        Console.WriteLine("No documentation files available in {0}", dirPath);
                        continue;
                    }

                    tw.WriteLine(string.Format("    - {0}:", di.Name.Replace('_', '.')));

                    foreach (var fi in files)
                    {
                        var shortFileName = fi.Name.Split('.').First();
                        var shortDirPath = string.Format("{0}/{1}", di.Name, fi.Name);
                        tw.WriteLine(string.Format("      - '{0}' : '{1}'", shortFileName, shortDirPath));
                    }
                }

                tw.WriteLine(themedir);
                tw.Flush();
            }
        }

        /// <summary>
        /// Replace the ctor with appropriate class name
        /// </summary>
        /// <param name="xml">The XML.</param>
        private static void HandleConstructors(XDocument xml)
        {
            var members = xml.Root.Element("members").Elements("member");

            //get all the constructors from xml.
            var constructors = members.Where(x => x.Attribute("name").Value.Contains("#ctor"));           
            try
            {
                foreach (var constructor in constructors)
                {
                    var text = constructor.Attribute("name").Value;
                    var name = text.Split(".").ToArray();
                    text = new StringBuilder(text).Replace("#ctor", name[2]).ToString();                    
                    constructor.Attribute("name").Value = text;
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Replace the special characters in generics.
        /// </summary>
        /// <param name="xml">The XML.</param>
        private static void HandleGenerics(XDocument xml)
        {
            var members = xml.Root.Element("members").Elements("member");

            //get all the generic members. Generic methods in XML has special characters 
            // List<T> in xml is List``1. So, replace them with correct values. Here, instead
            // of angular brackets we use [ ]             
            var genericMembers = members.Where(x => x.Attribute("name").Value.Contains("``") ||
                                                    x.Attribute("name").Value.Contains("`"));
            foreach (var genericMember in genericMembers)
            {
                if (genericMember.Element("typeparam") != null)
                {
                    var typeParamelem = genericMember.Element(("typeparam"));
                    if (typeParamelem != null)
                    {
                        var typeParamName = typeParamelem.Attribute("name").Value;
                        var text = ConvertGenericParameterName(genericMember.Attribute("name").Value, typeParamName);
                        genericMember.Attribute("name").Value = text;
                    }
                }

                //if there is no typeparam (possible if it is not documented correctly, then just replace it by T.
                else
                {
                    var text = ConvertGenericParameterName(genericMember.Attribute("name").Value);
                    genericMember.Attribute("name").Value = text;
                }
            }
        }

        private static void GenerateMarkdownDocumentForType(Type t, string folder, XDocument xml)
        {          
           string[] temp = t.Name.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            var typeName = temp[0];
           var members = xml.Root.Element("members").Elements("member");
                      
           var sb = new StringBuilder();

            sb.AppendLine("#" + typeName);
            if (t.IsGenericType)
            {
                var genericParam = t.GetGenericArguments();
                var genericParamName = string.Join(",", genericParam.Select(ty => ty.Name));
                var methodName = typeName + "<*" + genericParamName + "*>";
                var len = t.FullName.Split('.');
                methodName = string.Join(".", len.Take(len.Length - 1)) + "." + methodName;
                sb.Append(GetMarkdownForType(members, methodName));
                sb.AppendLine("---");
            }
            else
            {
                sb.Append(GetMarkdownForType(members, t.FullName));
                sb.AppendLine("---");
            }
           
            sb.AppendLine("##Constructors ");
            if (!t.GetConstructors().Any())
            {
                sb.AppendLine("####No public constructors defined");
            }
            foreach (var method in t.GetConstructors(BindingFlags.FlattenHierarchy |
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                var methodParams = method.GetParameters();
                var methodName = method.Name.Replace("ctor", t.Name);

                var fullMethodName = methodParams.Any() ?
               methodName + "(" + string.Join(",", methodParams.Select(pi => pi.ParameterType.FullName)) + ")" :
               methodName;

                var current = GetMarkdownForMethod(members, t.FullName + fullMethodName);
                sb.Append(current);
                sb.AppendLine();
            }            
            
            sb.AppendLine("---");

            sb.AppendLine("##Methods  ");
            if (!t.GetMethods().Any())
            {
                sb.AppendLine("####No public methods defined");
            }
            foreach (var method in t.GetMethods().Where(m => m.IsPublic))
            {
                var methodParams = method.GetParameters();
                var methodName = method.Name;
                XmlToMarkdown.ReturnType = method.ReturnType.Name;                
                //If the method is List<T,T>
                if (method.IsGenericMethod)
                {
                    var typeParam = method.GetGenericArguments();
                    //this returns T,T
                    var genericParamName = string.Join(",", typeParam.Select(ty => ty.Name));
                    //this returns List<T,T>
                    methodName = methodName + "<*" + genericParamName + "*>";
                }
                var fullMethodName = methodParams.Any() ?
                    methodName + "(" + string.Join(",", methodParams.Select(pi => pi.ParameterType.FullName)) + ")" :
                    methodName;

                Debug.WriteLine(t.FullName + "." + fullMethodName);

                var current = GetMarkdownForMethod(members, t.FullName + "." + fullMethodName);
                sb.Append(current);
                sb.AppendLine();
            }
            sb.AppendLine("---");

            sb.AppendLine("##Properties  ");

            if (!t.GetProperties().Any())
            {
                sb.AppendLine("####No public properties defined");
            }

            foreach (var property in t.GetProperties())
            {                
                XmlToMarkdown.ReturnType  = property.PropertyType.Name;
                var propertyNameSpace = ConvertGenericParameterName(t.FullName);
                var current = GetMarkdownForProperty(members, propertyNameSpace + "." + property.Name);                
                sb.Append(current);
                sb.AppendLine();
            }
            sb.AppendLine("---");

            sb.AppendLine("##Events  ");
            if (!t.GetEvents().Any())
            {
                sb.AppendLine("No public events defined");
            }
            foreach (var e in t.GetEvents())
            {
                XmlToMarkdown.ReturnType = string.Empty;
                var eventNameSpace = ConvertGenericParameterName(t.FullName);                
                var current = GetMarkdownForEvent(members, eventNameSpace + "." + e.Name);                
                sb.Append(current);
                sb.AppendLine();
            }
            sb.AppendLine("---");

            var fileName = typeName + ".md";
            var filePath = Path.Combine(folder, fileName);
            System.IO.File.WriteAllText(filePath, sb.ToString());
        }

        /// <summary>
        /// Converts the name of the generic parameter.
        /// There are four options here.
        ///  1. ISource
        ///  2. ISource`` or ISource`. 
        ///  3. ISource``.Properties
        ///  4. ISource``(Int).
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="typeParamName">Name of the type parameter.</param>
        /// <returns> converted parameter </returns>
        private static string ConvertGenericParameterName(string text , string typeParamName = "T")
        {
            string toReplace = lt + typeParamName + gt;
            var returnText = string.Empty;

            if (!text.Contains("``")
                && !text.Contains("`"))
            {
                return text;
            }
                                             
            string[] temp = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                                
            for (int i = 0; i < temp.Count(); i++)
            {
                switch (i)
                {
                    case 0:
                        returnText = temp[0] + toReplace;
                        break;
                    case 1:                        
                        if (temp[1].Contains("."))
                        {
                            returnText = returnText + "." + temp[1].Split('.')[1];
                        }
                        break;
                    case 2:
                        returnText = returnText + "(" + temp[2] + ")";
                        break;
                }
            }
                       
            return returnText;
        }
        private static string GetMarkdownForMethod(IEnumerable<XElement> members, string methodName)
        {          
            return GetMarkdownForMember(members, string.Format("M:{0}", methodName));
        }

        private static string GetMarkdownForType(IEnumerable<XElement> members, string typeName)
        {
            var str = GetMarkdownForMember(members, string.Format("T:{0}", typeName));
            return str.Replace("|", "");
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
        internal static string ApiStabilityTemplate = "<sup>" + "{0}" + "</sup>";
        internal static string returnType;
        internal static string ReturnType
        {
            get
            {
                return returnType;
            }
            set
            {
                if (value == "Void")
                    returnType = "void";
                else
                    returnType = value;
            }
        }

        private static Dictionary<string, string> templates =
            new Dictionary<string, string>
                {
                    {"doc", "## {0} ##\n\n{1}\n\n"},
                    {"type", "{0}\n"},
                    {"field", "{0}\n\n{1}\n"},
                    {"property", "| {0}  \n| ------------- | :--------------- | -------------:\n{1}\n"},
                    {"method", "| {0} | | stability index: \n| ------------- | :--------------- | -------------: \n{1}"},
                    {"event", "| {0}  \n| ------------- | :--------------- | -------------:\n{1}\n"},
                    {"summary", "| {0}\n"},
                    {"remarks", "| **Remarks:** | {0}\n"},
                    {"example", "| **Example:** | _C# code_\n\n```c#\n{0}\n```\n"},
                    {"seePage", "[[{1}|{0}]]"},
                    {"seeAnchor", "[{1}]({0})"},
                    {"param", "| **{0}:** | {1}\n" },
                    {"exception", "| **[[{0}|{0}]]:** | {1}\n" },
                    {"returns", "| **Return Value:** {0}\n"},
                    {"none", ""},
                    {"typeparam", "| **{0}:** | {1}\n" },
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
        private static Func<string, string, XElement,string[]> dType =
            new Func<string, string, XElement, string[]>((att1,att2,node) =>            
         
            {
                var methodName = node.Attribute(att1).Value.Split('.').Last();                
                var convertedMethodName = ConvertGenericParameters(node, methodName, att2);
                convertedMethodName = string.Join(" ", ReturnType, convertedMethodName);
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
                convertedMethodName = string.Join(" ", ReturnType, convertedMethodName);
                               
                return new object[]
                {                   
                    convertedMethodName,
                    node.Nodes().NodeMarkDown(),
                };
            };


        private static string CheckAndAppendStability(XElement node, string methodName)
        {
            if (node.Elements("api_stability").Any())
            {
                methodName = node.Elements("api_stability").Select(stabilityTag => stabilityTag.Value)
                    .Aggregate(methodName, (current, value) =>
                        current + string.Format(" " + XmlToMarkdown.ApiStabilityTemplate , value));
            }
            else
            {
                methodName = methodName + string.Format(" " + XmlToMarkdown.ApiStabilityTemplate , 1);
            }

            return methodName;
        }

        public static string ConvertGenericParameters(XElement node,string methodName,string attribute)
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
                            var url = ConstructUrl(methodParams[i]) + "/" + className;
                            className = "[" + className + "]" + "(" + url + ")";
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
                methodName = CheckAndAppendStability(node, methodName);
            }
            else
            {
                if (methodName.Contains("."))
                {
                    methodName = methodName.Split('.').Last();
                    methodName = methodName + "( )";
                }
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
                    {"property", x=> dType("name","param", x)},
                    {"method",x=>mType("name", "param", x)},
                    {"event", x=>dType("name", "param",  x)},
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

        static string ConstructUrl(String methodName)
        {
            var appSettings = ConfigurationManager.AppSettings["ServerUrl"];
            var url = appSettings ?? String.Empty;
            if (url != String.Empty)
            {
                var nameSpace = methodName.Split('.');
                url = url + "/" + string.Join("_", nameSpace.Take(nameSpace.Length - 1));
            }

            return url;
        }
    }
}
