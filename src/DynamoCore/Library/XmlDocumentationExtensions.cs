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

        /// <param name="xml">Don't set it, it's just for tests.</param>
        public static string GetDescription(this TypedParameter parameter, XmlReader xml = null)
        {
            return GetMemberElement(parameter.Function, xml,
                DocumentElementType.Description, parameter.Name);
        }

        /// <param name="xml">Don't set it, it's just for tests.</param>
        public static string GetSummary(this FunctionDescriptor member, XmlReader xml = null)
        {

            return GetMemberElement(member, xml, DocumentElementType.Summary);
        }

        /// <param name="xml">Don't set it, it's just for tests.</param>
        public static IEnumerable<string> GetSearchTags(this FunctionDescriptor member, XmlReader xml = null)
        {
            return GetMemberElement(member, xml, DocumentElementType.SearchTags)
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

        private static string GetMemberElement(
            FunctionDescriptor function,
            XmlReader xml,
            DocumentElementType property,
            string paramName = "")
        {
            var assemblyName = function.Assembly;

            if (string.IsNullOrEmpty(assemblyName) || (function.Type == FunctionType.GenericFunction))
                return String.Empty; // Operators, or generic global function in DS script.

            var fullyQualifiedName = MemberDocumentNode.MakeFullyQualifiedName
                (assemblyName, GetMemberElementName(function));

            if (!documentNodes.ContainsKey(fullyQualifiedName))
            {
                if (xml == null)
                    xml = DocumentationServices.GetForAssembly(function.Assembly, function.PathManager);
                LoadDataFromXml(xml, assemblyName);
            }

            MemberDocumentNode documentNode = null;
            if (documentNodes.ContainsKey(fullyQualifiedName))
                documentNode = documentNodes[fullyQualifiedName];
            else
            {
                var overloadedName = documentNodes.Keys.
                        Where(key => key.Contains(function.ClassName + "." + function.FunctionName)).FirstOrDefault();

                if (overloadedName == null)
                    return String.Empty;
                if (documentNodes.ContainsKey(overloadedName))
                    documentNode = documentNodes[overloadedName];
            }
            
            if (documentNode == null)
                return String.Empty;
            if (property.Equals(DocumentElementType.Description) && !documentNode.Parameters.ContainsKey(paramName))
                return String.Empty;

            switch (property)
            {
                case DocumentElementType.Summary:
                    return documentNode.Summary;

                case DocumentElementType.Description:
                    return documentNode.Parameters[paramName];

                case DocumentElementType.SearchTags:
                    return documentNode.SearchTags;

                default:
                    throw new ArgumentException("property");
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

            string memberName = member.FunctionName;
            if (!string.IsNullOrEmpty(member.ClassName))
                memberName = member.ClassName + "." + member.FunctionName;

            switch (member.Type)
            {
                case FunctionType.Constructor:
                    // XML documentation uses slightly different constructor names
                    int lastPoint = member.ClassName.LastIndexOf(".");
                    if (lastPoint == -1)
                        goto case FunctionType.InstanceMethod;

                    string classNameWithoutNamespace = member.ClassName.Substring(lastPoint + 1);
                    // If classname is the same as function name, then it's usual constructor.
                    // Otherwise it's static method which return type is the same as class.
                    if (classNameWithoutNamespace == member.FunctionName)
                        memberName = member.ClassName + ".#ctor";

                    goto case FunctionType.InstanceMethod;

                case FunctionType.InstanceMethod: 
                     prefixCode = 'M';

                    // parameters are listed according to their type, not their name
                    string paramTypesList = String.Join(
                        ",",
                        member.Parameters.Select(x => x.Type.ToString()).Select(PrimitiveMap).ToArray()
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

        private enum DocumentElementType
        {
            Summary,
            Description,
            SearchTags
        }

        private static void LoadDataFromXml(XmlReader reader, string assemblyName)
        {
            if (reader == null)
                return;

            MemberDocumentNode currentDocNode = null;
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
                                    currentDocNode = new MemberDocumentNode(assemblyName, reader.Value);
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
                        if (currentDocNode == null)
                            continue;

                        switch (currentTag)
                        {
                            case XmlTagType.Summary:
                                currentDocNode.Summary = reader.Value.CleanUpDocString();
                                break;
                            case XmlTagType.Parameter:
                                currentDocNode.Parameters.Add(currentParamName, reader.Value.CleanUpDocString());
                                break;
                            case XmlTagType.SearchTags:
                                currentDocNode.SearchTags = reader.Value.CleanUpDocString();
                                break;
                        }

                        break;
                }
            }
        }

        #endregion
    }

}