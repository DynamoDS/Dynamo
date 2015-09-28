using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XmlDocToMarkdown
{
    public static class Helper
    {
        private static readonly string[] repoHeader =
        {
            "site_name: Dynamo",
            "repo_url: http://dynamods.github.io/DynamoAPI/",
            "site_author: Dynamo",
            "pages:",
            "- Home: index.md",
            "- API:"
        };
        private const string ThemeDir = "theme_dir: Theme";
        
        public static DirectoryInfo CreateDocsFolder()
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

        /// <summary>
        /// Generate the mkdocs.ymp file describing the structura of the documentation.
        /// </summary>
        /// <param name="filePath"></param>
        public static void GenerateDocYaml()
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

                tw.WriteLine(ThemeDir);
                tw.Flush();
            }
        }


        /// <summary>
        /// Replace the ctor with appropriate class name
        /// </summary>
        /// <param name="xml">The XML.</param>
        public static void HandleConstructors(XDocument xml)
        {
            var members = xml.Root.Element("members").Elements("member");

            //get all the constructors from xml.
            var constructors = members.Where(x => x.Attribute("name").Value.Contains("#ctor"));
            try
            {
                foreach (var constructor in constructors)
                {
                    var text = constructor.Attribute("name").Value;
                    var name = text.Split('.').ToArray();
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
        public static void HandleGenerics(XDocument xml)
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
                        var text = MarkDownLibrary.ConvertGenericParameterName(genericMember.Attribute("name").Value, typeParamName);
                        genericMember.Attribute("name").Value = text;
                    }
                }

                //if there is no typeparam (possible if it is not documented correctly, then just replace it by T.
                else
                {
                    var text = MarkDownLibrary.ConvertGenericParameterName(genericMember.Attribute("name").Value);
                    genericMember.Attribute("name").Value = text;
                }
            }
        }       

        private static string DocRootPath()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "docs");
        }

    }
}
