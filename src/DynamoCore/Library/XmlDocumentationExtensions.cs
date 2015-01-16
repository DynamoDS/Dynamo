using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;

using Dynamo.Library;

namespace Dynamo.DSEngine
{
    /// <summary>
    /// Provides extension methods for reading XML documentation from reflected members.
    /// </summary>
    public static class XmlDocumentationExtensions
    {
        #region Public methods

        public static string GetSummary(this FunctionDescriptor member)
        {
            XDocument xml = null;

            if (member.Assembly != null)
                xml = DocumentationServices.GetForAssembly(member.Assembly);

            return member.GetSummary(xml);
        }

        public static string GetDescription(this TypedParameter member)
        {
            XDocument xml = null;

            if (member.Function != null && member.Function.Assembly != null)
                xml = DocumentationServices.GetForAssembly(member.Function.Assembly);

            return member.GetDescription(xml);
        }

        public static IEnumerable<string> GetSearchTags(this FunctionDescriptor member)
        {
            XDocument xml = null;

            if (member.Assembly != null)
                xml = DocumentationServices.GetForAssembly(member.Assembly);

            return member.GetSearchTags(xml);
        }

        #endregion

        #region Overloads specifying XDocument

        public static string GetDescription(this TypedParameter parameter, XDocument xml)
        {
            if (xml == null) return String.Empty;

            return GetMemberElement(parameter.Function,
                String.Format(/*NXLT*/"param[@name='{0}']", parameter.Name), xml).CleanUpDocString();
        }

        public static string GetSummary(this FunctionDescriptor member, XDocument xml)
        {
            if (xml == null) return String.Empty;

            return GetMemberElement(member, /*NXLT*/"summary", xml).CleanUpDocString();
        }

        public static IEnumerable<string> GetSearchTags(this FunctionDescriptor member, XDocument xml)
        {
            if (xml == null) return new List<string>();

            return GetMemberElement(member, /*NXLT*/"search", xml)
                .CleanUpDocString()
                .Split(',')
                .Select(x => x.Trim())
                .Where(x => x != String.Empty);
        }

        #endregion

        #region Helpers

        private static string CleanUpDocString(this string text)
        {
            if (String.IsNullOrEmpty(text)) return String.Empty;

            // trim, clean up new lines, and clean out excessive internal spaces
            var sb = new StringBuilder();
            var lastChar = ' ';

            foreach (var currentChar in text.Trim())
            {
                if (currentChar == ' ' && lastChar == ' ') continue;
                if (currentChar == '\n') continue;
                sb.Append(currentChar);
                lastChar = currentChar;
            }

            return sb.ToString();
        }

        private static string GetMemberElement(FunctionDescriptor function,
            string suffix, XDocument xml)
        {
            // Construct the entire function descriptor name, including CLR style names
            string clrMemberName = GetMemberElementName(function);

            // match clr member name
            var match = xml.XPathEvaluate(
                String.Format(/*NXLT*/"string(/doc/members/member[@name='{0}']/{1})", clrMemberName, suffix));

            if (match is String && !string.IsNullOrEmpty((string)match))
            {
                return match as string;
            }

            // fallback, namespace qualified method name
            var methodName = function.QualifiedName;

            // match with fallback
            match = xml.XPathEvaluate(
                String.Format(
                /*NXLT*/"string(/doc/members/member[contains(@name,'{0}')]/{1})", methodName, suffix));

            if (match is String && !string.IsNullOrEmpty((string)match))
            {
                return match as string;
            }

            return String.Empty;
        }

        private static string PrimitiveMap(string s)
        {
            switch (s)
            {
                case /*NXLT*/"[]":
                    return /*NXLT*/"System.Collections.IList";
                case /*NXLT*/"var[]..[]":
                    return /*NXLT*/"System.Collections.IList";
                case /*NXLT*/"var":
                    return /*NXLT*/"System.Object";
                case /*NXLT*/"double":
                    return /*NXLT*/"System.Double";
                case /*NXLT*/"int":
                    return /*NXLT*/"System.Int32";
                case /*NXLT*/"bool":
                    return /*NXLT*/"System.Boolean";
                case /*NXLT*/"string":
                    return /*NXLT*/"System.String";
                default:
                    return s;
            }
        }

        private static string GetMemberElementName(FunctionDescriptor member)
        {
            char prefixCode;

            string memberName = member.ClassName + /*NXLT*/"." + member.FunctionName;

            switch (member.Type)
            {
                case FunctionType.Constructor:
                    // XML documentation uses slightly different constructor names
                    memberName = memberName.Replace(/*NXLT*/".ctor", /*NXLT*/"#ctor");
                    goto case FunctionType.InstanceMethod;

                case FunctionType.InstanceMethod: 
                     prefixCode = 'M';

                    // parameters are listed according to their type, not their name
                    string paramTypesList = String.Join(
                        /*NXLT*/",",
                        member.Parameters.Select(x => x.Type).Select(PrimitiveMap).ToArray()
                        );

                    if (!String.IsNullOrEmpty(paramTypesList)) memberName += /*NXLT*/"(" + paramTypesList + /*NXLT*/")";
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
                    throw new ArgumentException(/*NXLT*/"Unknown member type", /*NXLT*/"member");
            }

            // elements are of the form "M:Namespace.Class.Method"
            return String.Format(/*NXLT*/"{0}:{1}", prefixCode, memberName);
        }

        #endregion
    }

}