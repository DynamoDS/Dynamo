using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace XmlDocToMarkdown
{
    /// <summary>
    /// this class creates the markdown file
    /// </summary>
    public static class JSONMarkDownLibrary
    {
        private static string[] separators = new string[] { "``", "`", "(", ")" };
        private const string lt = "<*";
        private const string gt = "*>";

        /// <summary>
        /// Generates the type of the markdown document for.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="folder">The folder.</param>
        /// <param name="xml">The XML.</param>
        public static void GenerateMarkdownDocumentForType(Type t, string folder, XDocument xml)
        {
            string[] temp = t.Name.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            var typeName = temp[0];
            var members = xml.Root.Element("members").Elements("member");

            //For creating a JSON object
            var jsonMD = new DllSerialization();
            jsonMD.Constructors = new List<object>();
            jsonMD.Methods = new List<object>();
            jsonMD.Properties = new List<object>();
            jsonMD.Events = new List<object>();
            jsonMD.ObsoleteMethods = new List<object>();
            jsonMD.ObsoleteProperties = new List<object>();
            jsonMD.ObsoleteEvents = new List<object>();

            var customAttr = t.GetCustomAttributes(true);
            for (int j = 0; j < customAttr.Length; j++)
            {
                var test = customAttr[j].GetType();
                var name = test.Name;
                if(name.Contains("Obsolete"))
                {
                    var ttt = "found";
                 }
            }


            if (t.IsGenericType)
            {
                var genericParam = t.GetGenericArguments();
                var genericParamName = string.Join(",", genericParam.Select(ty => ty.Name));
                var methodName = typeName + genericParamName;
                var len = t.FullName.Split('.');
                methodName = string.Join(".", len.Take(len.Length - 1)) + "." + methodName;
                GetMarkdownForType(members, methodName, jsonMD);
            }
            else
            {
                string fullName = t.FullName.Replace("+", ".");
                GetMarkdownForType(members, fullName, jsonMD);
            }

            GenerateMarkDownForConstructors(t, members, jsonMD);

            GenerateMarkDownForMethods(t, members, jsonMD);

            GenerateMarkDownForProperties(t, members, jsonMD);

            GenerateMarkDownForEvents(t, members, jsonMD);

            string json = JsonConvert.SerializeObject(jsonMD,Formatting.Indented);

            var fileName = typeName + ".json";
            var filePath = Path.Combine(folder, fileName);

            System.IO.File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// Generates the mark down for constructors.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="members">The members.</param>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        private static StringBuilder GenerateMarkDownForConstructors(Type t, IEnumerable<XElement> members, DllSerialization jsonMD)
        {
            bool foundType = false;
            if (!t.GetConstructors().Any())
            {
                return null;
            }
            
            foreach (var method in t.GetConstructors(BindingFlags.FlattenHierarchy |
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                var methodParams = method.GetParameters();
                var methodName = method.Name.Replace("ctor", t.Name);

                var fullMethodName = methodParams.Any() ?
               methodName + "(" + string.Join(",", methodParams.Select(pi => pi.ParameterType.FullName)) + ")" :
               methodName;

                var fullName = t.FullName.Replace("+", ".");
                var current = GetMarkdownForMethod(members, fullName + fullMethodName, jsonMD);
                if (current != "" && !foundType) foundType = true;
                jsonMD.Constructors.Add(current);
            }
            
            return null;
        }

        /// <summary>
        /// Generates the mark down for methods.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="members">The members.</param>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        private static StringBuilder GenerateMarkDownForMethods(Type t, IEnumerable<XElement> members,  DllSerialization jsonMD)
        {
            bool foundObsoleteMethod = false;
            if (!t.GetMethods().Any())
            {
                return null;
            }
            
            foreach (var method in t.GetMethods().Where(m => m.IsPublic))
            {
                var customAttrs = method.GetCustomAttributes(true);
                for (int j = 0; j < customAttrs.Length; j++)
                {
                    var test = customAttrs[j].GetType();
                    var name = test.Name;
                    if (name.Contains("Obsolete"))
                    {
                        foundObsoleteMethod = true;
                    }
                }

                var methodParams = method.GetParameters();
                var methodName = method.Name;
                JSONMarkDown.ReturnType = method.ReturnType.ConstructReturnType();               
                
                //If the method is List<T,T>
                if (method.IsGenericMethod)
                {                    
                    var typeParam = method.GetGenericArguments();
                    //this returns T,T
                    var genericParamName = string.Join(",", typeParam.Select(ty => ty.Name));
                    //this returns List<T,T>
                    methodName = methodName + genericParamName;                   
                }
                
                var fullMethodName = methodParams.Any() ?
                    methodName + "(" + string.Join(",", methodParams.Select(pi => pi.ParameterType.FullName)) + ")" :
                    methodName;

                Debug.WriteLine(t.FullName + "." + fullMethodName);

                var fullName = t.FullName.Replace("+", ".");
                var current = GetMarkdownForMethod(members, fullName + "." + fullMethodName, jsonMD);
                if (current != "")
                {
                    jsonMD.Methods.Add(current);
                }
                if(current != "" && foundObsoleteMethod)
                {
                    jsonMD.ObsoleteMethods.Add(current);
                }
                
            }

            return null;
        }

        /// <summary>
        /// Generates the mark down for properties.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="members">The members.</param>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        private static StringBuilder GenerateMarkDownForProperties(Type t, IEnumerable<XElement> members,
            DllSerialization jsonMD)
        {
            bool foundObsoleteMethod = false;
            if (!t.GetProperties().Any())
            {
                return null;
            }

            //If the Type is enum, then no URL is constructed
            foreach (var property in t.GetProperties())
            {
                var customAttrs = property.GetCustomAttributes(true);
                for (int j = 0; j < customAttrs.Length; j++)
                {
                    var test = customAttrs[j].GetType();
                    var name = test.Name;
                    if (name.Contains("Obsolete"))
                    {
                        foundObsoleteMethod = true;
                    }
                }
                JSONMarkDown.ReturnType = property.PropertyType.ConstructJSONReturnType();               
                if (property.GetMethod != null)
                {
                    JSONMarkDown.PropertySetType = "get;";
                }
                 if (property.SetMethod != null)
                 {
                    JSONMarkDown.PropertySetType  = "get;set;";
                 }
                var fullName = t.FullName.Replace("+", ".");
                var propertyNameSpace = ConvertGenericParameterName(fullName);
                var current = GetMarkdownForProperty(members, propertyNameSpace + "." + property.Name, jsonMD);
                if (current != "")
                {
                    jsonMD.Properties.Add(current);
                }
                if (current != "" && foundObsoleteMethod)
                {
                    jsonMD.ObsoleteProperties.Add(current);
                }

            }

            
            return null;
        }

        public static string ConstructJSONReturnType(this Type T)
        {
            string[] separators = new string[] { "``", "`", "(", ")" };
            var list = new List<String>();
            if (!T.IsGenericType)
            {
                if (T.FullName != null && T.FullName.Contains("Dynamo") && !T.IsEnum)
                {
                    var className = T.FullName.ReplaceSpecialCharacters("+", ".").Split('.').Last();
                    return className;
                }
                return T.Name;
            }

            //for generic type
            string[] splitByChar = T.Name.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            var tempName = splitByChar[0];

            var typeParam = T.GetGenericArguments();
            //this returns T,T           
            if (typeParam.Any())
            {
                list.AddRange(typeParam.Select(ConstructJSONReturnType));
            }

            if (list.Any())
            {
                var genericParamName = string.Join(",", list);
                return tempName +  genericParamName;
            }

            return string.Empty;
        }

        /// <summary>
        /// Generates the mark down for events.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="members">The members.</param>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        private static StringBuilder GenerateMarkDownForEvents(Type t, IEnumerable<XElement> members,
           DllSerialization jsonMD)
        {
            bool foundObsoleteMethod = false;
            if (!t.GetEvents().Any())
            {
                return null;
            }
            foreach (var e in t.GetEvents())
            {
                JSONMarkDown.ReturnType = string.Empty;
                var customAttrs = e.GetCustomAttributes(true);
                for (int j = 0; j < customAttrs.Length; j++)
                {
                    var test = customAttrs[j].GetType();
                    var name = test.Name;
                    if (name.Contains("Obsolete"))
                    {
                        foundObsoleteMethod = true;
                    }
                }
                var fullName = t.FullName.Replace("+", ".");
                var eventNameSpace = ConvertGenericParameterName(fullName);
                var current = GetMarkdownForEvent(members, eventNameSpace + "." + e.Name, jsonMD);
                if (current != "")
                {
                    jsonMD.Events.Add(current);
                }
                if (current != "" && foundObsoleteMethod)
                {
                    jsonMD.ObsoleteEvents.Add(current);
                }
            }

            return null;
        }
        /// <summary>
        /// Converts the name of the generic parameter.
        /// There are four options here.
        ///  1. ISource
        ///  2. ISource`` or ISource`. 
        ///  3. ISource``.Properties
        ///  4. ISource``(Int).
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="typeParamName">Name of the type parameter.</param>
        /// <returns> converted parameter </returns>
        public static string ConvertGenericParameterName(string text, string typeParamName = "T")
        {
            string toReplace = lt + typeParamName + gt;
            var returnText = string.Empty;

            if (!text.Contains("``")
                && !text.Contains("`"))
            {
                return text;
            }

            string[] temp = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < temp.Count(); i++)
            {
                switch (i)
                {
                    case 0:
                        returnText = temp[0] + toReplace;
                        break;
                    case 1:
                        if (temp[1].Contains("."))
                        {
                            returnText = returnText + "." + temp[1].Split('.')[1];
                        }
                        break;
                    case 2:
                        returnText = returnText + "(" + temp[2] + ")";
                        break;
                }
            }

            return returnText;
        }
        /// <summary>
        /// Gets the markdown for method.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <returns></returns>
        private static string GetMarkdownForMethod(IEnumerable<XElement> members, string methodName, DllSerialization jsonMD)
        {
            return GetMarkdownForMember(members, string.Format("M:{0}", methodName), jsonMD);
        }

        /// <summary>
        /// Gets the markdown for type.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        private static string GetMarkdownForType(IEnumerable<XElement> members, string typeName, DllSerialization jsonMD)
        {
            var str = GetMarkdownForMember(members, string.Format("T:{0}", typeName), jsonMD);
            return str.Replace("|", "");
        }

        /// <summary>
        /// Gets the markdown for property.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        private static string GetMarkdownForProperty(IEnumerable<XElement> members, string propertyName, DllSerialization jsonMD)
        {
            return GetMarkdownForMember(members, string.Format("P:{0}", propertyName), jsonMD);
        }

        /// <summary>
        /// Gets the markdown for event.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <returns></returns>
        private static string GetMarkdownForEvent(IEnumerable<XElement> members, string eventName, DllSerialization jsonMD)
        {
            return GetMarkdownForMember(members, string.Format("E:{0}", eventName), jsonMD);
        }

        /// <summary>
        /// Gets the markdown for member.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <returns></returns>
        private static string GetMarkdownForMember(IEnumerable<XElement> members, string memberName, DllSerialization jsonMD)
        {
            var foundType = members.FirstOrDefault(e => e.Attribute("name").Value == memberName);
            if (foundType == null)
            {
                return string.Empty;
            }

            return foundType.ToJSONMarkDown();
        }

        /// <summary>
        /// Gets all namespaces in assembly with public members.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllNamespacesInAssemblyWithPublicMembers(Assembly assembly)
        {
            return assembly.GetTypes().Where(t => t.IsPublic).Select(t => t.Namespace).Distinct();
        }
    }
}
