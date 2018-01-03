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
        /// <summary>
        /// Construct the markdown files
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            var path1 = @"C:\GitHub\Dynamo\bin\AnyCPU\Debug\DynamoCore.dll";
            var path2 = @"C:\GitHub\Dynamo\bin\AnyCPU\Debug\DynamoCore.xml";

            var asm = Assembly.LoadFrom(path1);
            var namespaces = MarkDownLibrary.GetAllNamespacesInAssemblyWithPublicMembers(asm);

            var docsFolder = Helper.CreateDocsFolder();

            if (docsFolder != null)
            {
                var xml = XDocument.Load(path2);

                Helper.HandleConstructors(xml);
                Helper.HandleGenerics(xml);
                foreach (var ns in namespaces)
                {
                    var cleanNamespace = ns.Replace('.', '_');
                    var outputDir = Path.Combine(Path.GetFullPath(docsFolder.FullName), cleanNamespace);
                    if (!Directory.Exists(outputDir))
                    {
                        Directory.CreateDirectory(outputDir);
                    }
                    var publicTypes = asm.GetTypes().Where(t => t.Namespace == ns).Where(t => t.IsPublic || t.IsNestedPublic);

                    foreach (var t in publicTypes)
                    {
                       //  MarkDownLibrary.GenerateMarkdownDocumentForType(t, outputDir, xml);
                        JSONMarkDownLibrary.GenerateMarkdownDocumentForType(t, outputDir, xml);
                    }
                }

                Helper.GenerateDocYaml();
            }
            else
            {
                Console.WriteLine("Cannot create docs folder.");
            }
        }
      
    }   
}
