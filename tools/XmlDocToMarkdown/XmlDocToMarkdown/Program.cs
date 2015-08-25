using System;
using System.Collections.Generic;
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

            var sb = new StringBuilder();

            sb.AppendLine("#" + t.Name);
            sb.Append(GetMarkdownForType(members, t.FullName));
            sb.AppendLine("---");

            sb.AppendLine("##Methods:  ");
            foreach (var method in t.GetMethods().Where(m => m.IsPublic))
            {
                var methodParams = method.GetParameters();
                var fullMethodName = methodParams.Any() ?
                    method.Name + "(" + string.Join(",", methodParams.Select(pi => pi.ParameterType.FullName)) + ")" :
                    method.Name;

                Debug.WriteLine(t.FullName + "." + fullMethodName);
                sb.Append(GetMarkdownForMethod(members, t.FullName + "." + fullMethodName));
            }
            sb.AppendLine("---");

            sb.AppendLine("##Properties:  ");
            foreach (var property in t.GetProperties())
            {
                sb.Append(GetMarkdownForProperty(members, t.FullName + "." + property.Name));
            }
            sb.AppendLine("---");

            sb.AppendLine("##Events:  ");
            foreach (var e in t.GetEvents())
            {
                sb.Append(GetMarkdownForEvent(members, t.FullName + "." + e.Name));
            }
            sb.AppendLine("---");

            var fileName = t.Name + ".md";
            var filePath = Path.Combine(folder, t.Name + ".md");
            System.IO.File.WriteAllText(filePath, sb.ToString());
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
            var foundType = members.Where(e => e.Attribute("name").Value == memberName).FirstOrDefault();
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
                    {"c", "`{0}`"}
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

        private static Func<string, XElement, string[]> mType =
            new Func<string, XElement, string[]>((att, node) =>{ 
                var methodName = node.Attribute(att).Value.Split(':').Last();
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
                    {"method",x=>mType("name", x)},
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
                    {"filterpriority", x => new string[0]}
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
    }
}
