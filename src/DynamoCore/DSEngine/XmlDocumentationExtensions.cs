using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Dynamo.DSEngine
{
    /// <summary>
    /// Provides extension methods for reading XML comments from reflected members.
    /// </summary>
    public static class XmlDocumentationExtensions
    {

        private static Dictionary<string, XDocument> cachedXml;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static XmlDocumentationExtensions()
        {
            cachedXml = new Dictionary<string, XDocument>(StringComparer.OrdinalIgnoreCase);
        }

        public static string GetXmlDocumentation(this TypedParameter member)
        {
            XDocument xml = null;

            if (member.Function != null && member.Function.Assembly != null && cachedXml.ContainsKey(member.Function.Assembly))
                xml = cachedXml[member.Function.Assembly];
            else
                return "";

            return GetXmlDocumentation(member, xml);
        }

        public static string GetXmlDocumentation(this TypedParameter parameter, XDocument xml)
        {
            return xml.XPathEvaluate(
                String.Format(
                    "string(/doc/members/member[@name='{0}']/param[@name='{1}'])",
                    GetMemberElementName(parameter.Function),
                    parameter.Name )
                ).ToString().Trim();
        }

        public static string GetXmlDocumentation(this FunctionDescriptor member)
        {
            XDocument xml = null;

            if (member.Assembly != null && cachedXml.ContainsKey(member.Assembly))
                xml = cachedXml[member.Assembly];
            else
                return "";

            return GetXmlDocumentation(member, xml);
        }

        private static string GetXmlDocumentation(FunctionDescriptor member, XDocument xml)
        {
            return xml.XPathEvaluate(
                String.Format(
                    "string(/doc/members/member[@name='{0}']/summary)",
                    GetMemberElementName(member)
                    )
                ).ToString().Trim();
        }

        private static string PrimitiveMap(string s)
        {
            switch (s)
            {
                case "var":
                    return "System.Object";
                case "double":
                    return "System.Double";
                case "int":
                    return "System.Int";
                case "bool":
                    return "System.Boolean";
                default:
                    return s;
            }
        }

        private static object GetMemberElementName(FunctionDescriptor member)
        {
            char prefixCode;

            string memberName = member.ClassName + "." + member.Name;

            switch (member.Type)
            {
                case FunctionType.Constructor:
                    // XML documentation uses slightly different constructor names
                    memberName = memberName.Replace(".ctor", "#ctor");
                    goto case FunctionType.InstanceMethod;

                case FunctionType.InstanceMethod: 
                     prefixCode = 'M';

                    // parameters are listed according to their type, not their name
                    string paramTypesList = String.Join(
                        ",",
                        member.Parameters.Select(x => x.Type).Select(PrimitiveMap).ToArray()
                        );
                    
                    if (!String.IsNullOrEmpty(paramTypesList)) memberName += "(" + paramTypesList + ")";
                    break;

                case FunctionType.StaticMethod:
                    goto case FunctionType.InstanceMethod;
                    break;

                case FunctionType.InstanceProperty:
                    prefixCode = 'P';
                    break;

                case FunctionType.StaticProperty:
                    goto case FunctionType.InstanceProperty;
                    break;

                default:
                    throw new ArgumentException("Unknown member type", "member");
            }

            // elements are of the form "M:Namespace.Class.Method"
            return String.Format("{0}:{1}", prefixCode, memberName);
        }

        /// <summary>
        /// Returns the XML documentation (summary tag) for the specified member.
        /// </summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="xml">XML documentation.</param>
        /// <returns>The contents of the summary tag for the member.</returns>
        public static string GetXmlDocumentation(this MemberInfo member, XDocument xml)
        {
            return xml.XPathEvaluate(
                String.Format(
                    "string(/doc/members/member[@name='{0}']/summary)",
                    GetMemberElementName(member)
                    )
                ).ToString().Trim();
        }

        /// <summary>
        /// Returns the XML documentation (returns/param tag) for the specified parameter.
        /// </summary>
        /// <param name="parameter">The reflected parameter (or return value).</param>
        /// <returns>The contents of the returns/param tag for the parameter.</returns>
        public static string GetXmlDocumentation(this ParameterInfo parameter)
        {
            AssemblyName assemblyName = parameter.Member.Module.Assembly.GetName();
            return GetXmlDocumentation(parameter, assemblyName.Name + ".xml");
        }

        /// <summary>
        /// Returns the XML documentation (returns/param tag) for the specified parameter.
        /// </summary>
        /// <param name="parameter">The reflected parameter (or return value).</param>
        /// <param name="pathToXmlFile">Path to the XML documentation file.</param>
        /// <returns>The contents of the returns/param tag for the parameter.</returns>
        public static string GetXmlDocumentation(this ParameterInfo parameter, string pathToXmlFile)
        {
            AssemblyName assemblyName = parameter.Member.Module.Assembly.GetName();
            XDocument xml = null;

            if (cachedXml.ContainsKey(assemblyName.FullName))
                xml = cachedXml[assemblyName.FullName];
            else
                cachedXml[assemblyName.FullName] = (xml = XDocument.Load(pathToXmlFile));

            return GetXmlDocumentation(parameter, xml);
        }

        /// <summary>
        /// Returns the XML documentation (returns/param tag) for the specified parameter.
        /// </summary>
        /// <param name="parameter">The reflected parameter (or return value).</param>
        /// <param name="xml">XML documentation.</param>
        /// <returns>The contents of the returns/param tag for the parameter.</returns>
        public static string GetXmlDocumentation(this ParameterInfo parameter, XDocument xml)
        {
            if (parameter.IsRetval || String.IsNullOrEmpty(parameter.Name))
                return xml.XPathEvaluate(
                    String.Format(
                        "string(/doc/members/member[@name='{0}']/returns)",
                        GetMemberElementName(parameter.Member)
                        )
                    ).ToString().Trim();
            else
                return xml.XPathEvaluate(
                    String.Format(
                        "string(/doc/members/member[@name='{0}']/param[@name='{1}'])",
                        GetMemberElementName(parameter.Member),
                        parameter.Name
                        )
                    ).ToString().Trim();
        }

        /// <summary>
        /// Returns the expected name for a member element in the XML documentation file.
        /// </summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The name of the member element.</returns>
        private static string GetMemberElementName(MemberInfo member)
        {
            char prefixCode;
            string memberName = (member is Type)
                ? ((Type)member).FullName // member is a Type
                : (member.DeclaringType.FullName + "." + member.Name); // member belongs to a Type

            switch (member.MemberType)
            {
                case MemberTypes.Constructor:
                    // XML documentation uses slightly different constructor names
                    memberName = memberName.Replace(".ctor", "#ctor");
                    goto case MemberTypes.Method;
                case MemberTypes.Method: 
                     prefixCode = 'M';

                    // parameters are listed according to their type, not their name
                    string paramTypesList = String.Join(
                        ",",
                        ((MethodBase)member).GetParameters()
                            .Cast<ParameterInfo>()
                            .Select(x => x.ParameterType.FullName
                            ).ToArray()
                        );
                    if (!String.IsNullOrEmpty(paramTypesList)) memberName += "(" + paramTypesList + ")";
                    break;

                case MemberTypes.Event:
                    prefixCode = 'E';
                    break;

                case MemberTypes.Field:
                    prefixCode = 'F';
                    break;

                case MemberTypes.NestedType:
                    // XML documentation uses slightly different nested type names
                    memberName = memberName.Replace('+', '.');
                    goto case MemberTypes.TypeInfo;
                case MemberTypes.TypeInfo:
                    prefixCode = 'T';
                    break;

                case MemberTypes.Property:
                    prefixCode = 'P';
                    break;

                default:
                    throw new ArgumentException("Unknown member type", "member");
            }

            // elements are of the form "M:Namespace.Class.Method"
            return String.Format("{0}:{1}", prefixCode, memberName);
        }

        /// <summary>
        /// Returns the XML documentation (summary tag) for the specified member.
        /// </summary>
        /// <param name="member">The reflected member.</param>
        /// <returns>The contents of the summary tag for the member.</returns>
        public static string GetXmlDocumentation(this MemberInfo member)
        {
            AssemblyName assemblyName = member.Module.Assembly.GetName();
            return GetXmlDocumentation(member, assemblyName.Name + ".xml");
        }

        /// <summary>
        /// Returns the XML documentation (summary tag) for the specified member.
        /// </summary>
        /// <param name="member">The reflected member.</param>
        /// <param name="pathToXmlFile">Path to the XML documentation file.</param>
        /// <returns>The contents of the summary tag for the member.</returns>
        public static string GetXmlDocumentation(this MemberInfo member, string pathToXmlFile)
        {
            AssemblyName assemblyName = member.Module.Assembly.GetName();
            XDocument xml = null;

            if (cachedXml.ContainsKey(assemblyName.FullName))
                xml = cachedXml[assemblyName.FullName];
            else
                cachedXml[assemblyName.FullName] = (xml = XDocument.Load(pathToXmlFile));

            return GetXmlDocumentation(member, xml);
        }

        /// <summary>
        /// Load Xml documentation for a given assembly
        /// </summary>
        /// <param name="pathToAssembly"></param>
        internal static bool LoadXmlDocumentation(string pathToAssembly)
        {

            if (cachedXml.ContainsKey(pathToAssembly))
                return true;

            var pathToXmlFile = pathToAssembly.Replace(".dll", ".xml");
            pathToXmlFile = Path.GetFullPath(pathToXmlFile);

            if (!File.Exists(pathToXmlFile)) return false;

            cachedXml[pathToAssembly] = XDocument.Load(pathToXmlFile);
            return true;

        }
    }

}