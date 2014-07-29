using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace NunitUtils
{
    class Program
    {
        private static void GetFailures(string filePath)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node">test-suite node</param>
        //private static void RecurseThroFile(XmlNode node, string category, FileStream fs)
        private static void RecurseThroFile(XmlNode node, string category, StreamWriter sw)
        {
            while (node.Attributes.GetNamedItem("type").Value == "Namespace")
            {
                if (node.FirstChild.Name == "results")
                {
                    XmlNode resultsNode = node.FirstChild;
                    foreach (XmlNode sNode in resultsNode.ChildNodes)
                    {
                        RecurseThroFile(sNode, category, sw);
                    }
                    return;
                }

            }
            if (node.Attributes.GetNamedItem("type").Value == "TestFixture")
            {
                if (node.FirstChild.Name == "results")
                {
                    XmlNode resultsNode = node.FirstChild;
                    foreach (XmlNode tNode in resultsNode.ChildNodes)
                    {
                        // tNode is the test-case

                        if (category == "Ignored")
                        {
                            // Find ignored nodes
                            if (tNode.Attributes.GetNamedItem("result").Value == "Ignored")
                            {
                                string str = tNode.Attributes.GetNamedItem("name").Value;
                                sw.WriteLine(str);
                            }
                        }
                        else
                        {
                            // Find Category nodes
                            XmlNode catNode = tNode.SelectSingleNode("categories");
                            if (catNode != null)
                            {
                                foreach (XmlNode cNode in catNode.ChildNodes)
                                {
                                    if (cNode.Attributes.GetNamedItem("name").Value == category)
                                    {
                                        string str = tNode.Attributes.GetNamedItem("name").Value;
                                        sw.WriteLine(str);
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Parse the input Nunit XML to filter test-cases by Category
        /// and prints the list to a new file
        /// </summary>
        /// <param name="args">args[0] = NUnit XML file
        /// args[1] = filter category
        /// args[2] = output file</param>
        static void Main(string[] args)
        {
            string xmlFilePath = args[0];
            string category = args[1];
            StreamWriter sw = new StreamWriter(args[2] + ".log");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilePath);

            XmlNodeList nodelist = xmlDoc.SelectNodes("/test-results/test-suite/results/test-suite");

            foreach (XmlNode node in nodelist)
            {
                RecurseThroFile(node, category, sw);
            }
            sw.Close();
            Console.WriteLine("Xml Parsing Complete\n");
        }
    }
}
