using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml;
using System.IO;
using NodeDocumentationUtility;

namespace XmlDocumentationsUtility
{
    public class XmlDocumentationsUtility
    {

        static void Main(string[] args)
        {
            try
            {
                //remove and check for node documentation xml under Debug configuration
                string xmlDir = Path.Combine(args[0], "bin", "AnyCPU", "Debug", "en-US");
                if (Directory.Exists(xmlDir))
                {
                    string[] xmlFiles = Directory.GetFiles(xmlDir, "*.xml");

                    foreach (string xmlPath in xmlFiles)
                    {
                        string xmlName = Path.GetFileNameWithoutExtension(xmlPath);
                        string dllPath = Path.Combine(Path.GetDirectoryName(xmlPath), @"..\", string.Format("{0}.dll", xmlName));
                        if (!File.Exists(dllPath))
                            continue;

                        string path = Path.GetFullPath(dllPath);
                        ZeroTouchModule zeroTouchModule = new ZeroTouchModule(path);
                        CheckAndDeleteXmlNodes(Path.GetFullPath(xmlPath), zeroTouchModule);
                    }
                }

                //remove and check for node documentation xml under Release configuration
                xmlDir = Path.Combine(args[0], "bin", "AnyCPU", "Release", "en-US");
                if (Directory.Exists(xmlDir))
                {
                    string[] xmlFiles = Directory.GetFiles(xmlDir, "*.xml");

                    foreach (string xmlPath in xmlFiles)
                    {
                        string xmlName = Path.GetFileNameWithoutExtension(xmlPath);
                        string dllPath = Path.Combine(Path.GetDirectoryName(xmlPath), @"..\", string.Format("{0}.dll", xmlName));
                        if (!File.Exists(dllPath))
                            continue;

                        string path = Path.GetFullPath(dllPath);
                        ZeroTouchModule zeroTouchModule = new ZeroTouchModule(path);
                        CheckAndDeleteXmlNodes(Path.GetFullPath(xmlPath), zeroTouchModule);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            
        }

        private static void CheckAndDeleteXmlNodes(string xmlPath,ZeroTouchModule zeroTouchModule)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlPath);        
            XmlNodeList elemList = xml.GetElementsByTagName("member");

            for (int i = 0; i < elemList.Count; i++)
            {
                string attrVal = elemList[i].Attributes["name"].Value;
                string type = "";
                bool hasToBeRemoved = false;
                int index = 0;
                switch (attrVal[0])
                {
                    //extracting and checking method
                    case 'M':
                        //format: M:type.method(...)
                        //extract type and method

                        string[] splittedM = attrVal.Split(':', '(');

                        type = splittedM[1];
                        index = type.Length - 1;
                        
                        //search for the latest '.' in string,
                        //which is corresponded to the start of the method's name

                        while(type[index] != '.')
                            index--;

                        string method = type.Substring(index + 1);
                        type = type.Substring(0, index);

                        if (!zeroTouchModule.MethodExists(type, method))
                            //method is not exist, the xml node needs to be removed
                            hasToBeRemoved = true;
                                            
                        break;

                    //extracting and checking property
                    case 'P':
                        //format: P:type.property
                        //extract type and property

                        string[] splittedP = attrVal.Split(':');

                        type = splittedP[1];
                        index = type.Length - 1;

                        //search for the latest '.' in string,
                        //which is corresponded to the start of the properties' name

                        while(type[index] != '.')
                            index--;

                        string property = type.Substring(index + 1);
                        type = type.Substring(0, index);

                        if(!zeroTouchModule.PropertyExists(type, property))
                            //property is not exist, the xml node needs to be removed
                            hasToBeRemoved = true;;

                        break;

                    //extracting and checking type
                    case 'T':
                        //format: T:type
                      
                        string[] splittedT = attrVal.Split(':');

                        if(!zeroTouchModule.TypeExists(splittedT[1]))
                            //type is not exist, the xml node needs to be removed
                            hasToBeRemoved = true;;
                        break;

                    //extracting and checking field
                    case 'F':
                        //format: F:type.field
                        //extract type and field
                        //similar to the property extraction

                        string[] splittedF = attrVal.Split(':');

                        type = splittedF[1];
                        index = type.Length - 1;
                        while(type[index] != '.')
                            index--;
                        string field = type.Substring(index + 1);
                        type = type.Substring(0, index);

                        
                        if(!zeroTouchModule.PropertyExists(type, field))
                            hasToBeRemoved = true;;
                        break;
                    default:
                        break;
                }

                if (hasToBeRemoved)
                {
                    elemList[i].ParentNode.RemoveChild(elemList[i]);
                    i--;
                }
            }
            //save the update
            xml.Save(xmlPath);

            //node is empty, delete the xml file
            if (elemList.Count == 0)
                File.Delete(xmlPath);
        }

   }
    
}