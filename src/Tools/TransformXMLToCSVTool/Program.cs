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
        private static String XMLProcessPath { get; set; }
        private static String CSVResultPath { get; set; }

        static void Main(string[] args)
        {           
            XMLProcessPath = args.Length == 0 ? "" : args[0];
            CSVResultPath = args.Length > 1 ? args[1] : "";
            XSLTToAnyType();
        }
        /// <summary>
        /// Method that process a XML in format Nunit2 into text, checking whether the needed paths exist and create them if not, the main goal of this method is to filter the slow tests and fast tests.
        /// </summary>
        public static void XSLTToAnyType()
        {
            try
            {               
                if (CreateFullFileAndCheckResultPath())
                {
                    allLines = File.ReadAllLines(String.Format(@"{0}\{1}", getPathToCSVResults(), textFileComplete)).ToList();
                    processAllLines();

                    CreateFilesResult(getPathToCSVResults(), textFileWithFiltersSlowTests, TypeOfFile.SlowTest);
                    CreateFilesResult(getPathToCSVResults(), textFileWithFiltersFastTests, TypeOfFile.FastTest);
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

                try
                {
                    xslCompiledTransform.Transform(getPathXmlToProcess(), null, stringWriter);
                }
                catch(Exception ex)
                {
                    Console.WriteLine((ex.Message.Contains("root") || ex.Message.Contains("raíz")) ? "XML file is empty.":"Check XML File.");
                    Console.ReadLine();
                }

                stringWriter.Close();

                String _CSVResultPath = getPathToCSVResults();

                if (!Directory.Exists(_CSVResultPath))
                    Directory.CreateDirectory(_CSVResultPath);

                if (File.Exists(String.Format(@"{0}\{1}", _CSVResultPath, textFileComplete)))
                    File.Delete(String.Format(@"{0}\{1}", _CSVResultPath, textFileComplete));

                File.AppendAllText(String.Format(@"{0}\{1}", _CSVResultPath, textFileComplete), stringBuilder.ToString());

                return File.Exists(String.Format(@"{0}\{1}", _CSVResultPath, textFileComplete));
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
                
                switch (typeOfFile)
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

                if (File.Exists(String.Format(@"{0}\{1}", directoryPath, textFileName)))
                    File.Delete(String.Format(@"{0}\{1}", directoryPath, textFileName));

                File.AppendAllLines(String.Format(@"{0}\{1}", directoryPath, textFileName), lstTestFixtureName);
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
        private static string getPathXmlToProcess()
        {
            try
            {
                if(XMLProcessPath== string.Empty)
                {
                    if(!Directory.Exists(String.Format(@"{0}\XML", DirectoryPath)))
                    {
                        Directory.CreateDirectory(String.Format(@"{0}\XML", DirectoryPath));
                    }
                }

                XMLProcessPath = XMLProcessPath == string.Empty ? String.Format(@"{0}\XML\{1}", DirectoryPath, xmlFilePathResult) : XMLProcessPath;
                return XMLProcessPath == string.Empty ? String.Format(@"{0}\XML\{1}", DirectoryPath, xmlFilePathResult) : XMLProcessPath;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                return string.Empty;
            }
        }
        private static string getPathToCSVResults()
        {
            try
            {
                if (CSVResultPath == string.Empty)
                {
                    if (!Directory.Exists(String.Format(@"{0}\Result", DirectoryPath)))
                    {
                        Directory.CreateDirectory(String.Format(@"{0}\Result", DirectoryPath));
                    }
                }

                CSVResultPath = CSVResultPath == string.Empty ? String.Format(@"{0}\Result", DirectoryPath) : CSVResultPath;
                return CSVResultPath == string.Empty ? String.Format(@"{0}\Result", DirectoryPath) : CSVResultPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                return string.Empty;
            }
        }
        private static void processAllLines()
        {
            List<String> newAllLines = new List<string>();
            foreach (var line in allLines)
            {
                string Class = line.Split(new char[] { ',' })[0];
                string NameSpace = string.Empty;
                string Dll = string.Empty;
              
                if (line.Split(new char[] { ',' })[1] == string.Empty)
                    continue;

                

                foreach (var type in line.Split(new char[] { ',' })[1].Split(new char[] { '.' }))
                {
                    if (type != Class && !type.Contains(Class))
                    {
                        NameSpace = NameSpace + type + "." ;
                    }
                    else
                    {
                        NameSpace = NameSpace + Class;
                        break;
                    }

                }
                foreach (var type in line.Split(new char[] { ',' })[2].Split(new char[] { '\\' }))
                {
                    if (type.Contains(".dll"))
                    {
                        Dll = type;
                        break;
                    }

                }

                newAllLines.Add(String.Format("{0},{1},{2}", Class, NameSpace,Dll));
            
            }
            allLines.Clear();
            allLines = newAllLines;
        }
    }

}


