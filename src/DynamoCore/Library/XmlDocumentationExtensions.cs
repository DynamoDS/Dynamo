using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.Interfaces;
using Dynamo.Library;

namespace Dynamo.Engine
{
    /// <summary>
    /// Provides extension methods for reading XML documentation from reflected members.
    /// </summary>
    public static class XmlDocumentationExtensions
    {
        private static Dictionary<string, MemberDocumentNode> documentNodes =
                   new Dictionary<string, MemberDocumentNode>();

        #region Public methods

        /// <summary>
        /// Get a description of a parameter from the its documentation xml,
        /// using the corresponding FunctionDescriptor.
        /// </summary>
        /// <param name="parameter">The TypedParameter object corresponding to the parameter.</param>
        /// <param name="xml"></param>
        /// <returns>The contents of the documentation description for the parameter.</returns>
        internal static string GetDescription(this TypedParameter parameter, XmlReader xml = null)
        {
            return GetMemberElement(parameter.Function, xml,
                DocumentElementType.Description, parameter.Name);
        }

        /// <summary>
        /// Get a summary of a method from its documentation xml, 
        /// using the corresponding FunctionDescriptor object.
        /// </summary>
        /// <param name="member">The FunctionDescriptor object corresponding to the method.</param>
        /// <param name="xml"></param>
        /// <returns>The contents of the documentation summary tag.</returns>
        internal static string GetSummary(this FunctionDescriptor member, XmlReader xml = null)
        {
            return GetMemberElement(member, xml, DocumentElementType.Summary);
        }

        /// <summary>
        /// Get a collection of search tags for a method from its documentation xml,
        /// using the corresponding FunctionDescriptor object.
        /// </summary>
        /// <param name="member">The FunctionDescriptor object corresponding to the method.</param>
        /// <param name="xml"></param>
        /// <returns>A collection of search tags.</returns>
        internal static IEnumerable<string> GetSearchTags(this FunctionDescriptor member, XmlReader xml = null)
        {
            return GetMemberElement(member, xml, DocumentElementType.SearchTags)
                .Split(',')
                .Select(x => x.Trim())
                .Where(x => x != String.Empty);
        }

        /// <summary>
        /// Get a collection of search tag weights for a method from its documentation xml,
        /// using the corresponding FunctionDescriptor object.
        /// </summary>
        /// <param name="member">The FunctionDescriptor object corresponding to the method.</param>
        /// <param name="xml"></param>
        /// <returns>A collection of search weights, or an empty collection if the search weights tag is emtpy.</returns>
        internal static IEnumerable<double> GetSearchTagWeights(this FunctionDescriptor member, XmlReader xml = null)
        {
            var weights = GetMemberElement(member, xml, DocumentElementType.SearchTagWeights);
            if (string.IsNullOrEmpty(weights))
            {
                return new List<double>();
            }

            return weights
                .Split(',')
                .Select(x => x.Trim())
                .Where(x => x != String.Empty)
                .Select(x => Convert.ToDouble(x, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Get a collection of return descriptions for a method from its documentation xml,
        /// using the corresponding FunctionDescriptor object.
        /// </summary>
        /// <param name="member">The FunctionDescriptor object corresponding to the method.</param>
        /// <param name="xml"></param>
        /// <returns>A collection of return descriptions from the documentation returns tag.</returns>
        internal static IEnumerable<Tuple<string, string>> GetReturns(this FunctionDescriptor member, XmlReader xml = null)
        {
            var node = GetMemberDocumentNode(member, xml);
            if (node == null)
            {
                return Enumerable.Empty<Tuple<string,string>>();
            }
            return node.Returns;
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

        private static MemberDocumentNode GetMemberDocumentNode(
            FunctionDescriptor function,
            XmlReader xml )
        {
            //customNodeDefinitions typedParameters don't have functionDescriptors
            if (function == null)
            {
                return null;
            }
            var assemblyName = function.Assembly;

            if (string.IsNullOrEmpty(assemblyName))
                return null;

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
                    return null;
                if (documentNodes.ContainsKey(overloadedName))
                    documentNode = documentNodes[overloadedName];
            }

            return documentNode;
        }

        private static string GetMemberElement(
            FunctionDescriptor function,
            XmlReader xml,
            DocumentElementType property,
            string paramName = "")
        {
            var documentNode = GetMemberDocumentNode(function, xml);

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

                case DocumentElementType.SearchTagWeights:
                    return documentNode.SearchTagWeights;

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
                case FunctionType.GenericFunction:
                    return member.FunctionName;
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
            SearchTags,
            SearchTagWeights,
            Returns
        }

        private enum DocumentElementType
        {
            Summary,
            Description,
            SearchTags,
            SearchTagWeights
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

                            case "returns":
                                if (reader.MoveToAttribute("name"))
                                {
                                    currentParamName = reader.Value;
                                }
                                else
                                {
                                    currentParamName = null;
                                }
                                currentTag = XmlTagType.Returns;
                                break;

                            case "search":
                                currentTag = XmlTagType.SearchTags;
                                break;

                            case "weights":
                                currentTag = XmlTagType.SearchTagWeights;
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
                            case XmlTagType.Returns:
                                currentDocNode.Returns.Add(new Tuple<string,string>(currentParamName, reader.Value.CleanUpDocString()));
                                break;
                            case XmlTagType.SearchTags:
                                currentDocNode.SearchTags = reader.Value.CleanUpDocString();
                                break;
                            case XmlTagType.SearchTagWeights:
                                currentDocNode.SearchTagWeights = reader.Value.CleanUpDocString();
                                break;
                        }

                        break;
                }
            }
        }

        #endregion
    }

}