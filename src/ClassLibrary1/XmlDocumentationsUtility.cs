using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Xml;

using Dynamo;
using Dynamo.DSEngine;

using ProtoFFI;
using ProtoCore;
using ProtoAssociative;

namespace XmlDocumentationsUtility
{
    public class XmlDocumentationsUtility
    {
           
         static void Main(string[] args)
         {
             
            XmlDocument xml = new XmlDocument();
            xml.Load(@"C:\Users\t_sancl\Documents\Visual Studio 2013\Projects\ConsoleApplication2\ConsoleApplication2\bin\Debug\ProtoGeometry.xml");
            XmlNode root = xml.DocumentElement;
            XmlNodeList elemList = xml.GetElementsByTagName("member");
      
            for (int i = 0; i < elemList.Count; i++)
            {
                string attrVal = elemList[i].Attributes["name"].Value;
                char[] delimiters = {':','(','.'};
                string[] words = attrVal.Split(delimiters);
                int size = words.Length;
                string type = "";
                switch (attrVal[0])
                {
                    case 'M':
                        //methodParameterList = extractMethodParameter(attrVal, words);
                        string[] splitted = attrVal.Split(':', '(');
                        type = splitted[splitted.Length - 1];
                        splitted = type.Split('.');
                        string method = splitted[splitted.Length - 1];
                        type = "";

                        for (int j = 1; j < size - 1; j++)
                        {
                            type += splitted[j];

                            if (j != size - 2)
                                type += '.';
                        }
                        Console.WriteLine("Type " + type);
                        Console.WriteLine("Method " + method);//isPropertyAvailable(words[);                      
                        break;
                    case 'P':
                        for (int j = 1; j < size-1; j++)
                        {
                            type += words[j];

                            if(j != size-2)
                                type+='.';
                        }
                        Console.WriteLine("Type " + type);
                        Console.WriteLine("Property " + words[size-1]);//isPropertyAvailable(words[);
                        break;
                    case 'T':
                        Console.WriteLine("Type " + words[size-1]);//isTypeAvailable
                        break;
                    case 'F':
                        for (int j = 1; j < size-1; j++)
                        {
                            type += words[j];

                            if(j != size-2)
                                type+='.';
                        }
                        Console.WriteLine("Type " + type);
                        Console.WriteLine("Field " + words[size-1]); //isFieldAvaliable
                        break;
                    default:
                        break;
                }

                //extract first char
                //call API based on char
                //if true, remove, i--
                //if (attrVal == "pret")
                //{
                //    elemList[i].ParentNode.RemoveChild(elemList[i]);
                //    i--;
                //}
            // Console.WriteLine(attrVal);
            }
            //xml.Save("ProtoGeometry_copy.xml");
            Console.ReadLine();
        }

        private static string extractMethod(string attrVal){
            for (int i = 0; i < attrVal.Length; i++)
            {
                if (attrVal[i] == '(')
                    break;
            }
                return "";
        }
        private static List<string> extractMethodParameter(string attrVal, string[] words)
        {
            int index = 0;
            List<string> list = new List<string>();
            bool isAnyParameter = false;

            for (int i = 0; i < attrVal.Length;i++)
            {
                if (attrVal[i] == '(')
                {
                    index++; list.Add(words[index]);
                    isAnyParameter = true;
                    while (attrVal[i] != ')')
                    {
                        if (attrVal[i] == ',')
                        {
                            index++; list.Add(words[index]);
                        } 
                        else if (attrVal[i] == '{')
                        {
                            index++; list.Add(words[index]);
                            while (attrVal[i] != '}')
                            {    
                                if (attrVal[i] == ',')
                                {
                                    index++; list.Add(words[index]);
                                } i++;
                            }
                            index++; list.Add(words[index]);
                        }
                        i++;
                    }
                    index++; 

                    if(words[index] != "")
                        list.Add(words[index]);
                }
                
                
            }
            if (!isAnyParameter)
                list.Add(words[index + 1]);
            return list;
        }

        private static void extractDLL()
        {
            //Assembly assembly = Assembly.LoadFile("C:\\Users\\t_sancl\\Documents\\GitHub\\Dynamo\\bin\\AnyCPU\\Debug\\Analysis.dll");

            //System.Type[] types = assembly.GetTypes();
            //foreach(System.Type type in types){
            //   PropertyInfo[] properties = type.GetProperties();
            //   MethodInfo[] methods = type.GetMethods();
            //   FieldInfo[] fields = type.GetFields();

            //    foreach (PropertyInfo property in properties)
            //   {
            //       Console.WriteLine("Property: " + property.Name);
            //   }
            //   foreach (MethodInfo method in methods)
            //   {
            //       Console.WriteLine("Method: " + method.Name);
            //   }
            //   foreach (FieldInfo field in fields)
            //   {
            //       Console.WriteLine("Field: " + field.Name);
            //   }
            //}
            //Console.ReadLine();
        }
    }
}

         
       

  
