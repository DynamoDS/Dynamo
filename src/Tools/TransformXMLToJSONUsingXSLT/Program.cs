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
        static string textFile = ConfigurationManager.AppSettings["textFile"];
        static string textFileWithFilters = ConfigurationManager.AppSettings["textFileWithFilters"];
        static int applyFilters = int.Parse(ConfigurationManager.AppSettings["applyFilters"].ToString());



        static void Main(string[] args)
        {
            XSLTToAnyType();
        }
        public static void XSLTToAnyType()
        {
            XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
            xslCompiledTransform.Load(String.Format(@"{0}\XSLT\{1}", DirectoryPath, xsltFile));
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            xslCompiledTransform.Transform(String.Format(@"{0}\XML\{1}", DirectoryPath, xmlFilePathResult), null, stringWriter);
            stringWriter.Close();
            List<String> lstTestFixtureName = new List<String>();

            if (!Directory.Exists(String.Format(@"{0}\Result", DirectoryPath)))
                Directory.CreateDirectory(String.Format(@"{0}\Result", DirectoryPath));

            if (File.Exists(String.Format(@"{0}\Result\{1}", DirectoryPath, textFile)))
                File.Delete(String.Format(@"{0}\Result\{1}", DirectoryPath, textFile));

            File.AppendAllText(String.Format(@"{0}\Result\{1}", DirectoryPath, textFile), stringBuilder.ToString());

            if (File.Exists(String.Format(@"{0}\Result\{1}", DirectoryPath, textFile)) && applyFilters == 1)
            {
                NameValueCollection section = (NameValueCollection)ConfigurationManager.GetSection("Filters");
                var allLines = File.ReadAllLines(String.Format(@"{0}\Result\{1}", DirectoryPath, textFile));

                foreach (String filter in section)
                {
                    lstTestFixtureName.AddRange(allLines.Where(l => l.Split(new char[] { ',' })[0].ToString() == filter));
                }
                if (File.Exists(String.Format(@"{0}\Result\{1}", DirectoryPath, textFileWithFilters)))
                    File.Delete(String.Format(@"{0}\Result\{1}", DirectoryPath, textFileWithFilters));


                File.AppendAllLines(String.Format(@"{0}\Result\{1}", DirectoryPath, textFileWithFilters), lstTestFixtureName);
            }

        }

    }

}

