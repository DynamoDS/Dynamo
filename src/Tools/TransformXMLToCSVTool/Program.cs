using System;
using System.Reflection;
using System.IO;
using System.Text;
using System.Configuration;
using System.Collections.Generic;
using System.Xml.Xsl;
using System.Linq;
using System.Collections.Specialized;

namespace TransformXMLToJSONUsingXSLT
{
    class Program
    {
        static string DirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
        static string xmlFilePathResult = ConfigurationManager.AppSettings["xmlFilePathResult"];
        static string xsltFile = ConfigurationManager.AppSettings["xsltFile"];
        static string textFileComplete = ConfigurationManager.AppSettings["textFileComplete"];
        static string textFileWithFiltersSlowTests = ConfigurationManager.AppSettings["textFileWithFiltersSlowTests"];
        static string textFileWithFiltersFastTests = ConfigurationManager.AppSettings["textFileWithFiltersFastTests"];
        static List<String> allLines = new List<string>();
        static List<String> restLines = new List<string>();

        static void Main(string[] args)
        {
            XSLTToAnyType();
        }
        public static void XSLTToAnyType()
        {
            try
            {               
                if (CreateFullFileAndCheckResultPath())
                {
                    allLines = File.ReadAllLines(String.Format(@"{0}\Result\{1}", DirectoryPath, textFileComplete)).ToList();                 
                    CreateFilesResult(DirectoryPath, textFileWithFiltersSlowTests, TypeOfFile.SlowTest);
                    CreateFilesResult(DirectoryPath, textFileWithFiltersFastTests, TypeOfFile.FastTest);
                }                
            }
            catch(IOException ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }

        }       
        private static bool CreateFullFileAndCheckResultPath()
        {
            try
            {
                XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
                xslCompiledTransform.Load(String.Format(@"{0}\XSLT\{1}", DirectoryPath, xsltFile));
                StringBuilder stringBuilder = new StringBuilder();
                StringWriter stringWriter = new StringWriter(stringBuilder);
                xslCompiledTransform.Transform(String.Format(@"{0}\XML\{1}", DirectoryPath, xmlFilePathResult), null, stringWriter);
                stringWriter.Close();
                
                if (!Directory.Exists(String.Format(@"{0}\Result", DirectoryPath)))
                    Directory.CreateDirectory(String.Format(@"{0}\Result", DirectoryPath));

                if (File.Exists(String.Format(@"{0}\Result\{1}", DirectoryPath, textFileComplete)))
                    File.Delete(String.Format(@"{0}\Result\{1}", DirectoryPath, textFileComplete));

                File.AppendAllText(String.Format(@"{0}\Result\{1}", DirectoryPath, textFileComplete), stringBuilder.ToString());

                return File.Exists(String.Format(@"{0}\Result\{1}", DirectoryPath, textFileComplete));
            }
            catch(IOException ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                return false;
            }
        }

        private static bool CreateFilesResult(string directoryPath, string textFileName , TypeOfFile typeOfFile)
        {
            try
            {
                List<String> lstTestFixtureName = new List<String>();                
                NameValueCollection section = (NameValueCollection)ConfigurationManager.GetSection("Filters");
               
                switch(typeOfFile)
                {
                    case TypeOfFile.FastTest:
                        lstTestFixtureName.Clear();                                                
                            foreach(string line in allLines)
                            {
                            if (!restLines.Contains(line))
                             {
                               lstTestFixtureName.Add(line);
                             }
                            }
                        restLines.Clear();
                        break;
                    case TypeOfFile.SlowTest:
                        lstTestFixtureName.Clear();
                        foreach (string line in allLines)
                        {
                            foreach (String filter in section)
                            {
                                if (line.Split(new char[] { ',' })[0].ToString() == filter)
                                {
                                    if (!lstTestFixtureName.Contains(line))
                                    {
                                        lstTestFixtureName.Add(line);
                                        restLines.Add(line);
                                    }                                                                                                          
                                }                               
                            }
                        }
                        break;
                }

                if (File.Exists(String.Format(@"{0}\Result\{1}", directoryPath, textFileName)))
                    File.Delete(String.Format(@"{0}\Result\{1}", directoryPath, textFileName));

                File.AppendAllLines(String.Format(@"{0}\Result\{1}", directoryPath, textFileName), lstTestFixtureName);
                lstTestFixtureName.Clear();
                
                return true;
            }
            catch(IOException ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                return false;
            }
        }
        private enum TypeOfFile
        {
            FastTest=0,
            SlowTest=1
        }
    }

}


