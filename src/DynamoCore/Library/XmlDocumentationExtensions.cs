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

        public static string GetXmlDocumentation(this TypedParameter member)
        {
            XDocument xml = null;

            if (member.Function != null && member.Function.Assembly != null)
                xml = DocumentationServices.GetForAssembly(member.Function.Assembly);

            return GetXmlDocumentation(member, xml);
        }

        public static string GetXmlDocumentation(this TypedParameter parameter, XDocument xml)
        {
            if (xml == null) return String.Empty;

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

            if (member.Assembly != null)
                xml = DocumentationServices.GetForAssembly(member.Assembly);

            return GetXmlDocumentation(member, xml);
        }

        private static string GetXmlDocumentation(this FunctionDescriptor member, XDocument xml)
        {
            if (xml == null) return String.Empty;

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

    }

}