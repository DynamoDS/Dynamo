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
    public static class MarkDownLibrary
    {
        private static string[] separators = new string[] { "``", "`", "(", ")" };
        private const string lt = "<*";
        private const string gt = "*>";

        public static void GenerateMarkdownDocumentForType(Type t, string folder, XDocument xml)
        {
            string[] temp = t.Name.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            var typeName = temp[0];
            var members = xml.Root.Element("members").Elements("member");

            var sb = new StringBuilder();

            sb.AppendLine("#" + typeName);
            if (t.IsGenericType)
            {
                var genericParam = t.GetGenericArguments();
                var genericParamName = string.Join(",", genericParam.Select(ty => ty.Name));
                var methodName = typeName + "<*" + genericParamName + "*>";
                var len = t.FullName.Split('.');
                methodName = string.Join(".", len.Take(len.Length - 1)) + "." + methodName;
                sb.Append(GetMarkdownForType(members, methodName));
                sb.AppendLine("---");
            }
            else
            {
                sb.Append(GetMarkdownForType(members, t.FullName));
                sb.AppendLine("---");
            }

            sb.AppendLine("##Constructors ");
            sb = GenerateMarkDownForConstructors(t, members, sb);

            sb.AppendLine();

            sb.AppendLine("##Methods  ");
            sb = GenerateMarkDownForMethods(t, members, sb);

            sb.AppendLine();

            sb.AppendLine("##Properties  ");
            sb = GenerateMarkDownForProperties(t, members, sb);
            
            sb.AppendLine();

            sb.AppendLine("##Events  ");
            sb = GenerateMarkDownForEvents(t, members, sb);

            sb.AppendLine();

            var fileName = typeName + ".md";
            var filePath = Path.Combine(folder, fileName);
            System.IO.File.WriteAllText(filePath, sb.ToString());
        }

        /// <summary>
        /// Generates the mark down for constructors.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="members">The members.</param>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        private static StringBuilder GenerateMarkDownForConstructors(Type t, IEnumerable<XElement> members, StringBuilder sb)
        {
            if (!t.GetConstructors().Any())
            {
                sb.AppendLine("####No public constructors defined");
            }
            //foreach (var check in t.GetConstructors())
            //{
            //    var cc = Attribute.GetCustomAttribute(check, typeof (CompilerGeneratedAttribute));
            //}
            foreach (var method in t.GetConstructors(BindingFlags.FlattenHierarchy |
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                var methodParams = method.GetParameters();
                var methodName = method.Name.Replace("ctor", t.Name);

                var fullMethodName = methodParams.Any() ?
               methodName + "(" + string.Join(",", methodParams.Select(pi => pi.ParameterType.FullName)) + ")" :
               methodName;

                var current = GetMarkdownForMethod(members, t.FullName + fullMethodName);
                sb.Append(current);
                sb.AppendLine();
            }

            return sb;
        }

        /// <summary>
        /// Generates the mark down for methods.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="members">The members.</param>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        private static StringBuilder GenerateMarkDownForMethods(Type t, IEnumerable<XElement> members, StringBuilder sb)
        {
            if (!t.GetMethods().Any())
            {
                sb.AppendLine("####No public methods defined");
            }
            
            foreach (var method in t.GetMethods().Where(m => m.IsPublic && !m.IsSpecialName && 
                m.DeclaringType.Name != "Objects"))
            {
                var methodParams = method.GetParameters();
                var methodName = method.Name;
                XmlToMarkdown.ReturnType = method.ReturnType.ConstructReturnType();               
                
                //If the method is List<T,T>
                if (method.IsGenericMethod)
                {                    
                    var typeParam = method.GetGenericArguments();
                    //this returns T,T
                    var genericParamName = string.Join(",", typeParam.Select(ty => ty.Name));
                    //this returns List<T,T>
                    methodName = methodName + "<*" + genericParamName + "*>";                   
                }
                
                var fullMethodName = methodParams.Any() ?
                    methodName + "(" + string.Join(",", methodParams.Select(pi => pi.ParameterType.FullName)) + ")" :
                    methodName;

                Debug.WriteLine(t.FullName + "." + fullMethodName);

                var current = GetMarkdownForMethod(members, t.FullName + "." + fullMethodName);
                sb.Append(current);
                sb.AppendLine();
            }

            return sb;
        }

        /// <summary>
        /// Generates the mark down for properties.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="members">The members.</param>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        private static StringBuilder GenerateMarkDownForProperties(Type t, IEnumerable<XElement> members,
            StringBuilder sb)
        {
            if (!t.GetProperties().Any())
            {
                sb.AppendLine("####No public properties defined");
            }

            //If the Type is enum, then no URL is constructed
            foreach (var property in t.GetProperties())
            {
                XmlToMarkdown.ReturnType = property.PropertyType.ConstructReturnType();               
                if (property.GetMethod != null)
                {
                    XmlToMarkdown.PropertySetType = "get;";
                }
                 if (property.SetMethod != null)
                 {
                     XmlToMarkdown.PropertySetType  = "get;set;";
                 }
                var propertyNameSpace = ConvertGenericParameterName(t.FullName);
                var current = GetMarkdownForProperty(members, propertyNameSpace + "." + property.Name);               
                sb.Append(current);
                sb.AppendLine();
            }
            return sb;
        }

        /// <summary>
        /// Generates the mark down for events.
        /// </summary>
        /// <param name="t">The t.</param>
        /// <param name="members">The members.</param>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        private static StringBuilder GenerateMarkDownForEvents(Type t, IEnumerable<XElement> members,
           StringBuilder sb)
        {
            if (!t.GetEvents().Any())
            {
                sb.AppendLine("####No public events defined");
            }
            foreach (var e in t.GetEvents())
            {
                XmlToMarkdown.ReturnType = string.Empty;
                var eventNameSpace = ConvertGenericParameterName(t.FullName);
                var current = GetMarkdownForEvent(members, eventNameSpace + "." + e.Name);
                sb.Append(current);
                sb.AppendLine();
            }

            return sb;
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
        private static string GetMarkdownForMethod(IEnumerable<XElement> members, string methodName)
        {
            return GetMarkdownForMember(members, string.Format("M:{0}", methodName));
        }

        private static string GetMarkdownForType(IEnumerable<XElement> members, string typeName)
        {
            var str = GetMarkdownForMember(members, string.Format("T:{0}", typeName));
            return str.Replace("|", "");
        }

        private static string GetMarkdownForProperty(IEnumerable<XElement> members, string propertyName)
        {
            return GetMarkdownForMember(members, string.Format("P:{0}", propertyName));
        }

        private static string GetMarkdownForEvent(IEnumerable<XElement> members, string eventName)
        {
            return GetMarkdownForMember(members, string.Format("E:{0}", eventName));
        }

        private static string GetMarkdownForMember(IEnumerable<XElement> members, string memberName)
        {
            var foundType = members.FirstOrDefault(e => e.Attribute("name").Value == memberName);
            if (foundType == null)
            {
                return string.Empty;
            }

            return foundType.ToMarkDown();
        }

        public static IEnumerable<string> GetAllNamespacesInAssemblyWithPublicMembers(Assembly assembly)
        {
            return assembly.GetTypes().Where(t => t.IsPublic).Select(t => t.Namespace).Distinct();
        }
    }
}
