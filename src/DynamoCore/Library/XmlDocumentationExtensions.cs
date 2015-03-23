using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.Interfaces;
using Dynamo.Library;

namespace Dynamo.DSEngine
{
    /// <summary>
    /// Provides extension methods for reading XML documentation from reflected members.
    /// </summary>
    public static class XmlDocumentationExtensions
    {
        private static Dictionary<string, MemberDocumentNode> documentNodes =
                   new Dictionary<string, MemberDocumentNode>();

        #region Public methods

        public static string GetSummary(this FunctionDescriptor member, IPathManager pathManager)
        {
            XmlReader xml = null;

            if (member.Assembly != null)
                xml = DocumentationServices.GetForAssembly(member.Assembly, pathManager);

            return member.GetSummary(xml);
        }

        public static string GetDescription(this TypedParameter member, IPathManager pathManager)
        {
            XmlReader xml = null;

            if (member.Function != null && member.Function.Assembly != null)
                xml = DocumentationServices.GetForAssembly(member.Function.Assembly, pathManager);

            return member.GetDescription(xml);
        }

        public static IEnumerable<string> GetSearchTags(this FunctionDescriptor member)
        {
            XmlReader xml = null;

            if (member.Assembly != null)
                xml = DocumentationServices.GetForAssembly(member.Assembly, member.PathManager);

            return member.GetSearchTags(xml);
        }

        #endregion

        #region Overloads specifying XDocument

        public static string GetDescription(this TypedParameter parameter, XmlReader xml)
        {
            if (xml == null) return String.Empty;

            return GetMemberElement(parameter.Function,
                "description", xml).CleanUpDocString();
        }

        public static string GetSummary(this FunctionDescriptor member, XmlReader xml)
        {
            if (xml == null) return String.Empty;

            return GetMemberElement(member, "summary", xml).CleanUpDocString();
        }

        public static IEnumerable<string> GetSearchTags(this FunctionDescriptor member, XmlReader xml)
        {
            if (xml == null) return new List<string>();

            return GetMemberElement(member, "search", xml)
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
            string property, XmlReader xml, string paramName = "")
        {
            var assemblyName = function.Assembly;
            var fullyQualifiedName = GetMemberElementName(function);
            if (!documentNodes.ContainsKey(fullyQualifiedName))
                LoadDataFromXml(xml);

            // Case for operators.
            if (assemblyName == null)
                return String.Empty;

            var keyForOverloaded = documentNodes.Keys.
                        Where(key => key.Contains(function.ClassName + "." + function.FunctionName)).FirstOrDefault();

            switch (property)
            {
                case "summary":
                    if (documentNodes.ContainsKey(fullyQualifiedName))
                        return documentNodes[fullyQualifiedName].Summary;

                    // Fallback for overloaded methods.
                    if (keyForOverloaded != null)
                        return documentNodes[keyForOverloaded].Summary;

                    return String.Empty;

                case "description":
                    if (documentNodes.ContainsKey(fullyQualifiedName))
                        if (documentNodes[fullyQualifiedName].Parameters.ContainsKey(paramName))
                            return documentNodes[fullyQualifiedName].Parameters[paramName];

                    // Fallback for overloaded methods.
                    if (keyForOverloaded != null)
                        if (documentNodes[keyForOverloaded].Parameters.ContainsKey(paramName))
                            return documentNodes[keyForOverloaded].Parameters[paramName];

                    return String.Empty;
                case "search":
                    if (documentNodes.ContainsKey(fullyQualifiedName))
                        return documentNodes[fullyQualifiedName].SearchTags;

                    // Fallback for overloaded methods.
                    if (keyForOverloaded != null)
                        return documentNodes[keyForOverloaded].SearchTags;

                    return String.Empty;
                default:
                    return String.Empty;
            }
        }

        private static string PrimitiveMap(string s)
        {
            switch (s)
            {
                case "[]":
                    return "System.Collections.IList";
                case "var[]..[]":
                    return "System.Collections.IList";
                case "var":
                    return "System.Object";
                case "double":
                    return "System.Double";
                case "int":
                    return "System.Int32";
                case "bool":
                    return "System.Boolean";
                case "string":
                    return "System.String";
                default:
                    return s;
            }
        }

        private static string GetMemberElementName(FunctionDescriptor member)
        {
            char prefixCode;

            string memberName = member.ClassName + "." + member.FunctionName;

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
                        member.Parameters.Select(x => x.Type.ToShortString()).Select(PrimitiveMap).ToArray()
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

        private enum XmlTagType
        {
            None,
            Member,
            Summary,
            Parameter,
            SearchTags
        }

        private static void LoadDataFromXml(XmlReader reader)
        {
            if (reader == null)
                return;

            MemberDocumentNode currentDocNode = new MemberDocumentNode();
            XmlTagType currentTag = XmlTagType.None;
            string currentParamName = String.Empty;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (reader.Name)
                        {
                            case "member":
                                // Find attribute "name".
                                if (reader.MoveToAttribute("name"))
                                {
                                    currentDocNode = new MemberDocumentNode(reader.Value);
                                    documentNodes.Add(currentDocNode.FullyQualifiedName, currentDocNode);
                                }
                                currentTag = XmlTagType.Member;
                                break;

                            case "summary":
                                currentTag = XmlTagType.Summary;
                                break;

                            case "param":
                                if (reader.MoveToAttribute("name"))
                                {
                                    currentParamName = reader.Value;
                                }
                                currentTag = XmlTagType.Parameter;
                                break;

                            case "search":
                                currentTag = XmlTagType.SearchTags;
                                break;

                            default:
                                currentTag = XmlTagType.None;
                                break;
                        }
                        break;
                    case XmlNodeType.Text:
                        switch (currentTag)
                        {
                            case XmlTagType.Summary:
                                currentDocNode.Summary = reader.Value;
                                break;
                            case XmlTagType.Parameter:
                                currentDocNode.Parameters.Add(currentParamName, reader.Value);
                                break;
                            case XmlTagType.SearchTags:
                                currentDocNode.SearchTags = reader.Value;
                                break;
                        }

                        break;
                }
            }
        }

        #endregion
    }

}