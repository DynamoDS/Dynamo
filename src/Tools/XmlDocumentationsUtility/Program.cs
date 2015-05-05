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
        internal enum Type { Field, Method, Property, Type };

        internal struct MemberData
        {
            public Type type;
            public string TypeName;
            public string MemberName;
        }

        /// <summary>
        /// helper function to isolate the elementName from method parameters and type
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        internal static string GetMemberElement(string elementName)
        {
            string memberElement = elementName.Substring(elementName.IndexOf(':') + 1);
            int methodParameterStartingIndex = memberElement.IndexOf('(');

            if (methodParameterStartingIndex != -1)
                memberElement = memberElement.Substring(0, methodParameterStartingIndex);
            
            return memberElement;
        }

        /// <summary>
        /// helper funcation to split TypeName and MemberName
        /// <remarks>does not return correct value for Type type</remarks>
        /// </summary>
        /// <param name="memberElement"></param>
        /// <returns></returns>
        internal static Tuple<string,string> GetTypeAndMemberName(string memberElement)
        {
            int typeNameStartingIndex = 0;
            typeNameStartingIndex = memberElement.LastIndexOf('.');
            string typeName = memberElement.Substring(0, typeNameStartingIndex);
            string memberName = memberElement.Substring(typeNameStartingIndex + 1);
           
            return Tuple.Create(typeName,memberName);
        }

        /// <summary>
        /// parse member element (type, property, method, field)
        /// from the given elementName
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        internal static MemberData ParseMemberElement(string elementName)
        {
            MemberData memberData = new MemberData();
            char typeId = elementName[0];
            string memberElement = GetMemberElement(elementName);
            var typeMember = GetTypeAndMemberName(memberElement);

            switch(typeId)
            {
                case 'F' :
                    memberData.type = Type.Field;                    
                    memberData.TypeName = typeMember.Item1;
                    memberData.MemberName = typeMember.Item2;
                    break;

                case 'M' :
                    memberData.type = Type.Method;        
                    memberData.TypeName = typeMember.Item1;
                    memberData.MemberName = typeMember.Item2;
                    break;
                case 'P' :
                    memberData.type = Type.Property;        
                    memberData.TypeName = typeMember.Item1;
                    memberData.MemberName = typeMember.Item2;
                    break;
                case 'T' :
                    memberData.type = Type.Type;
                    memberData.TypeName = memberElement;
                    memberData.MemberName = "";
                    break;
                default :
                    break;
            }
            return memberData;
        }
        
        /// <summary>
        /// recursively search for xml files inside en-US folder
        /// to remove hidden nodes inside xml.
        /// only en-US needs to be iterated since other language resources 
        /// is derived from the en-US resources
        /// </summary>
        /// <param name="searchDirectory"></param>
        private static void RecursiveCultureXmlSearch(string searchDirectory)
        {
            foreach (string directory in Directory.GetDirectories(searchDirectory))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directory);

                if(dirInfo.Name == "en-US")
                {
                    string[] xmlFiles = Directory.GetFiles(directory, "*.xml");

                    foreach (string xmlPath in xmlFiles)
                    {
                        string xmlName = Path.GetFileNameWithoutExtension(xmlPath);
                        string dllPath = Path.Combine(Path.GetDirectoryName(xmlPath), @"..\", string.Format("{0}.dll", xmlName));
                        if (!File.Exists(dllPath))
                            continue;

                        string path = Path.GetFullPath(dllPath);
                        ZeroTouchModule zeroTouchModule = null;

                        try
                        {
                            zeroTouchModule = new ZeroTouchModule(path);
                        }
                        catch(System.Exception e)
                        {
                            Console.WriteLine("Cannot load the ZeroTouchModule dll\n"
                                             +"Only Properties.Resources will be removed for this xml documentation\n"
                                             + e.Message);
                        }
                        RemoveDocumentationForHiddenNodes(Path.GetFullPath(xmlPath), zeroTouchModule);
                    }
                }
                RecursiveCultureXmlSearch(directory);
            }
        }

        /// <summary>
        /// main function
        /// </summary>
        /// <param name="args">args[0] points to bin directory</param>
        static void Main(string[] args)
        {
            try
            {
                string rootDir = Path.GetFullPath(args[0]);
                DirectoryInfo dirInfo = new DirectoryInfo(rootDir);

                if(dirInfo.Exists)
                    RecursiveCultureXmlSearch(rootDir);
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            
        }

        /// <summary>
        /// iterates through member nodes inside the given xml file,
        /// and remove the node if it's hidden.
        /// If there's no member nodes/ all of the member nodes have been removed,
        /// the files is deleted
        /// </summary>
        /// <param name="xmlPath"></param>
        /// <param name="zeroTouchModule"></param>
        internal static void RemoveDocumentationForHiddenNodes(string xmlPath,ZeroTouchModule zeroTouchModule)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlPath);        
            XmlNodeList elemList = xml.GetElementsByTagName("member");
            MemberData memberData;

            for (int i = 0; i < elemList.Count; i++)
            {
                string elementName = elemList[i].Attributes["name"].Value;
                bool hasToBeRemoved = false;
                memberData = ParseMemberElement(elementName);

                if (zeroTouchModule == null)
                {
                    if (memberData.TypeName.Contains("Properties.Resources"))
                        hasToBeRemoved = true;
                }
                else
                {
                    switch (memberData.type)
                    {
                        case Type.Field:

                        case Type.Property:

                            if (!zeroTouchModule.PropertyExists(memberData.TypeName, memberData.MemberName))
                                hasToBeRemoved = true;
                            break;

                        case Type.Method:

                            if (!zeroTouchModule.MethodExists(memberData.TypeName, memberData.MemberName))
                                hasToBeRemoved = true;
                            break;

                        case Type.Type:

                            if (!zeroTouchModule.TypeExists(memberData.TypeName))
                                hasToBeRemoved = true;
                            break;

                        default: break;

                    }
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