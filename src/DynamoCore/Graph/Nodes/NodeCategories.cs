using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using Dynamo.Configuration;
using Dynamo.Engine;
using Dynamo.Library;
using ProtoCore;

namespace Dynamo.Graph.Nodes
{
    /// <summary>
    /// Built-in Dynamo Categories. If you want your node to appear in one of the existing Dynamo
    /// categories, then use these constants. This ensures that if the names of the categories
    /// change down the road, your node will still be placed there.
    /// </summary>
    public static class BuiltinNodeCategories
    {
        public const string CORE = "Core";
        public const string CORE_INPUT = "Core.Input";
        public const string CORE_WEB = "Core.Web";
        public const string CORE_LISTS_CREATE = "Core.List.Create";
        public const string CORE_LISTS_ACTION = "Core.List.Actions";
        public const string CORE_LISTS_QUERY = "Core.List.Query";
        public const string CORE_VIEW = "Core.View";
        public const string CORE_EVALUATE = "Core.Evaluate";
        public const string CORE_SCRIPTING = "Core.Scripting";
        public const string CORE_IO = "Core.File";
        public const string CORE_UNITS = "Core.Units";

        public const string LOGIC = "Core.Logic";
        public const string LOGIC_MATH_OPTIMIZE = "Logic.Math.Optimize";


        public const string GEOMETRY_CATEGORY = "Geometry";
        public const string GEOMETRY = "Geometry.Geometry";

        public const string ANALYZE_SOLAR = "Analyze.Solar";

        public const string IO = "Input/Output";
        public const string IO_HARDWARE = "Input/Output.Hardware";
    }

    /// <summary>
    /// Service class for formatting node text data
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Returns cut version of a given <see cref="System.String"/> value
        /// </summary>
        /// <param name="value"><see cref="System.String"/> value to cut</param>
        /// <param name="desiredLength">Max allowed number of symbols</param>
        /// <returns>Cut version of a given <see cref="System.String"/> value</returns>
        internal static string Ellipsis(string value, int desiredLength)
        {
            return desiredLength > value.Length ? value : value.Remove(desiredLength - 1) + "...";
        }

        /// <summary>
        /// <para>This method patches the fullyQualifiedName of a given type. It 
        /// updates the given name to its newer form (i.e. "Dynamo.Graph.Nodes.Xyz")
        /// if it matches the older form (e.g. "Dynamo.Elements.Xyz").</para>
        /// <para>The method also attempts to update "XYZ/UV" convention to 
        /// "Xyz/Uv" to comply with the new Dynamo naming convention.</para>
        /// </summary>
        /// <param name="fullyQualifiedName">A fully qualified name. An example
        /// of this would be "Dynamo.Elements.dynNode".</param>
        /// <returns>The processed fully qualified name. For an example, the 
        /// name "Dynamo.Elements.UV" will be returned as "Dynamo.Graph.Nodes.Uv".
        /// </returns>
        internal static string PreprocessTypeName(string fullyQualifiedName)
        {
            if (string.IsNullOrEmpty(fullyQualifiedName))
                throw new ArgumentNullException("fullyQualifiedName");

            // older files will have nodes in the Dynamo.Elements namespace
            const string oldPrefix = "Dynamo.Elements.";
            const string newPrefix = "Dynamo.Nodes.";
            string className;

            // Attempt to extract the class name out of the fully qualified 
            // name, regardless of whether it is in the form of the older 
            // "Dynamo.Elements.XxxYyy" or the newer "Dynamo.Nodes.XxxYyy".
            // 
            if (fullyQualifiedName.StartsWith(oldPrefix))
                className = fullyQualifiedName.Substring(oldPrefix.Length);
            else if (fullyQualifiedName.StartsWith(newPrefix))
                className = fullyQualifiedName.Substring(newPrefix.Length);
            else
            {
                // We are only expected to process names of our built-in types,
                // and if we're given any of the system types, then we'll just
                // return them as-is without any patches.
                // 
                return fullyQualifiedName;
            }

            // Remove prefix of 'dyn' from older files.
            if (className.StartsWith("dyn"))
                className = className.Remove(0, 3);

            // Older files will have nodes that use "XYZ" and "UV" 
            // instead of "Xyz" and "Uv". Update these names.
            className = className.Replace("XYZ", "Xyz");
            className = className.Replace("UV", "Uv");
            return newPrefix + className; // Always new prefix from now on.
        }

        /// <summary>
        /// This method returns a name for the icon based on name of the node.
        /// </summary>
        /// <param name="descriptor">Function descriptor, that contains all info about node.</param>
        /// <param name="overridePrefix">
        /// overridePrefix is used as default value for generating node icon name.
        /// If overridePrefix is empty, it uses QualifiedName property.
        /// e.g. Autodesk.DesignScript.Geometry.CoordinateSystem.ByOrigin
        /// </param>
        internal static string TypedParametersToString(FunctionDescriptor descriptor, string overridePrefix = "")
        {
            var builder = new StringBuilder();

            foreach (TypedParameter tp in descriptor.Parameters)
            {
                string typeOfParameter = tp.Type.ToString();

                // Check to see if there is array indexer symbols '[]', if so turn their 
                // dimensionality into a number (e.g. 'bool[][]' turned into 'bool2').
                int squareBrackets = typeOfParameter.Count(x => x == '[');
                if (squareBrackets > 0)
                {
                    if (typeOfParameter.Contains("[]..[]"))
                    {
                        // Remove square brackets.
                        typeOfParameter = typeOfParameter.Replace("[]..[]", "");
                        // Add number of them.
                        typeOfParameter = String.Concat(typeOfParameter, "N");
                    }
                    else
                    {
                        // Remove square brackets.
                        int index = typeOfParameter.IndexOf('[');
                        typeOfParameter = typeOfParameter.Substring(0, index).TrimEnd();

                        // Add number of them.
                        typeOfParameter = String.Concat(typeOfParameter, squareBrackets.ToString());
                    }
                }

                if (builder.Length > 0)
                    builder.Append("-");

                typeOfParameter = typeOfParameter.Split('.').Last();
                builder.Append(typeOfParameter);
            }

            // If the caller does not supply a prefix, use default logic to generate one.
            if (string.IsNullOrEmpty(overridePrefix))
                overridePrefix = NormalizeAsResourceName(descriptor.QualifiedName);

            return overridePrefix + "." + builder.ToString();
        }

        /// <summary>
        /// Returns formatted category name replacing delimeters and 
        /// if category is too long leaving only first and last subcategories.
        /// </summary>
        /// <param name="fullCategoryName">Full category name 
        /// including all parent categories up to root one.</param>
        /// <returns>Formatted category name.</returns>
        internal static string ShortenCategoryName(string fullCategoryName)
        {
            if (string.IsNullOrEmpty(fullCategoryName))
                return string.Empty;

            var catName = fullCategoryName.Replace(Configurations.CategoryDelimiterString, Configurations.CategoryDelimiterWithSpaces);

            // if the category name is too long, we strip off the interior categories
            if (catName.Length > 50)
            {
                var s = catName.Split(Configurations.ShortenedCategoryDelimiter.ToArray()).Select(x => x.Trim()).ToList();
                if (s.Count() > 4)
                {
                    s = new List<string>()
                                        {
                                            s[0],
                                            "...",
                                            s[s.Count - 3],
                                            s[s.Count - 2],
                                            s[s.Count - 1]
                                        };
                    catName = String.Join(Configurations.CategoryDelimiterWithSpaces, s);
                }
            }

            return catName;
        }

        /// <summary>
        /// Call this method to associate/remove the target file path with/from
        /// the given XmlDocument object.
        /// </summary>
        /// <param name="document">The XmlDocument with which the target file 
        /// path is to be associated. This parameter cannot be null.</param>
        /// <param name="targetFilePath">The target file path to be associated 
        /// with the given XmlDocument. If this parameter is null or an empty 
        /// string, then any target file path that was previously associated 
        /// will be removed.</param>
        internal static void SetDocumentXmlPath(XmlDocument document, string targetFilePath)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            if (document.DocumentElement == null)
            {
                const string message = "'XmlDocument.DocumentElement' cannot be null";
                throw new ArgumentException(message);
            }

            var rootElement = document.DocumentElement;
            if (string.IsNullOrEmpty(targetFilePath))
            {
                rootElement.RemoveAttribute(Configurations.FilePathAttribName);
                return;
            }

            rootElement.SetAttribute(Configurations.FilePathAttribName, targetFilePath);
        }

        /// <summary>
        /// Call this method to retrieve the associated target file path from 
        /// the given XmlDocument object. An exception will be thrown if such 
        /// target file path was never associated with the XmlDocument object.
        /// </summary>
        /// <param name="document">The XmlDocument object from which the 
        /// associated target file path is to be retrieved.</param>
        /// <returns>Returns the associated target file path.</returns>
        internal static string GetDocumentXmlPath(XmlDocument document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            if (document.DocumentElement == null)
            {
                const string message = "'XmlDocument.DocumentElement' cannot be null";
                throw new ArgumentException(message);
            }

            // If XmlDocument is opened from an existing file...
            if (!string.IsNullOrEmpty(document.BaseURI))
            {
                var documentUri = new Uri(document.BaseURI, UriKind.Absolute);
                if (documentUri.IsFile)
                    return documentUri.LocalPath;
            }

            var rootElement = document.DocumentElement;
            var attrib = rootElement.Attributes[Configurations.FilePathAttribName];

            if (attrib == null)
            {
                throw new InvalidOperationException(
                    string.Format("'{0}' attribute not found in XmlDocument",
                    Configurations.FilePathAttribName));
            }

            return attrib.Value;
        }

        /// <summary>
        /// Call this method to serialize given node-data-list pairs into an 
        /// XmlDocument. Serialized data in the XmlDocument can be loaded by a 
        /// call to LoadTraceDataFromXmlDocument method.
        /// </summary>
        /// <param name="document">The target document to which the trade data 
        /// is to be written. This parameter cannot be null and must represent 
        /// a valid XmlDocument object.</param>
        /// <param name="nodeTraceDataList">A dictionary of node-data-list pairs
        /// to be saved to the XmlDocument. This parameter cannot be null and 
        /// must represent a non-empty list of node-data-list pairs.</param>
        internal static void SaveTraceDataToXmlDocument(XmlDocument document,
            IEnumerable<KeyValuePair<Guid, List<CallSite.RawTraceData>>> nodeTraceDataList)
        {
            #region Parameter Validations

            if (document == null)
                throw new ArgumentNullException("document");

            if (document.DocumentElement == null)
            {
                const string message = "Document does not have a root element";
                throw new ArgumentException(message, "document");
            }

            if (nodeTraceDataList == null)
                throw new ArgumentNullException("nodeTraceDataList");

            if (!nodeTraceDataList.Any())
            {
                const string message = "Trade data list must be non-empty";
                throw new ArgumentException(message, "nodeTraceDataList");
            }

            #endregion

            #region Session Xml Element

            var sessionElement = document.CreateElement(
                Configurations.SessionTraceDataXmlTag);

            document.DocumentElement.AppendChild(sessionElement);

            #endregion

            #region Serialize Node Xml Elements

            foreach (var pair in nodeTraceDataList)
            {
                var nodeElement = document.CreateElement(
                    Configurations.NodeTraceDataXmlTag);

                // Set the node ID attribute for this element.
                var nodeGuid = pair.Key.ToString();
                nodeElement.SetAttribute(Configurations.NodeIdAttribName, nodeGuid);
                sessionElement.AppendChild(nodeElement);

                foreach (var data in pair.Value)
                {
                    var callsiteXmlElement = document.CreateElement(
                        Configurations.CallsiteTraceDataXmlTag);
                    callsiteXmlElement.SetAttribute(Configurations.CallSiteID, data.ID);

                    callsiteXmlElement.InnerText = data.Data;
                    nodeElement.AppendChild(callsiteXmlElement);
                }
            }

            #endregion
        }

        /// <summary>
        /// Call this method to load serialized node-data-list pairs (through a 
        /// prior call to SaveTraceDataToXmlDocument) from a given XmlDocument.
        /// </summary>
        /// <param name="document">The XmlDocument from which serialized node-
        /// data-list pairs are to be deserialized.</param>
        /// <returns>Returns a dictionary of deserialized node-data-list pairs
        /// loaded from the given XmlDocument.</returns>
        internal static IEnumerable<KeyValuePair<Guid, List<CallSite.RawTraceData>>>
            LoadTraceDataFromXmlDocument(XmlDocument document)
        {
            if (document == null)
                throw new ArgumentNullException("document");

            if (document.DocumentElement == null)
            {
                const string message = "Document does not have a root element";
                throw new ArgumentException(message, "document");
            }

            var childNodes = document.DocumentElement.ChildNodes.Cast<XmlElement>();
            var sessionXmlTagName = Configurations.SessionTraceDataXmlTag;
            var query = from childNode in childNodes
                        where childNode.Name.Equals(sessionXmlTagName)
                        select childNode;

            var loadedData = new Dictionary<Guid, List<CallSite.RawTraceData>>();
            if (!query.Any()) // There's no data, return empty dictionary.
                return loadedData;

            XmlElement sessionElement = query.ElementAt(0);
            foreach (XmlElement nodeElement in sessionElement.ChildNodes)
            {
                var guid = Guid.Parse(nodeElement.GetAttribute(Configurations.NodeIdAttribName));
                List<CallSite.RawTraceData> callsiteTraceData = new List<CallSite.RawTraceData>();
                foreach (var child in nodeElement.ChildNodes.Cast<XmlElement>())
                {
                    var callsiteId = string.Empty;
                    if (child.HasAttribute(Configurations.CallSiteID))
                    {
                        callsiteId = child.GetAttribute(Configurations.CallSiteID);
                    }
                    var traceData = child.InnerText;
                    callsiteTraceData.Add(new CallSite.RawTraceData(callsiteId, traceData));
                }
                loadedData.Add(guid, callsiteTraceData);
            }

            return loadedData;
        }

        /// <summary>
        /// Call this method to compute the relative path of a subject path 
        /// relative to the given base path.
        /// </summary>
        /// <param name="basePath">The base path which relative path is to be 
        /// computed from. This base path does not need to point to a valid file
        /// on disk, but it cannot be an empty string.</param>
        /// <param name="subjectPath">The subject path of which the relative
        /// path is to be computed. If this path is not empty but does not 
        /// represent a valid path string, a UriFormatException is thrown.</param>
        /// <returns>Returns the path of the subject relative to the given base 
        /// path.</returns>
        internal static string MakeRelativePath(string basePath, string subjectPath)
        {
            if (string.IsNullOrEmpty(basePath))
                throw new ArgumentNullException("basePath");

            if (string.IsNullOrEmpty(subjectPath))
                return string.Empty;

            // Determine if we have any directory information in the 
            // subjectPath. For example, we won't want to form a relative 
            // path if the input of this method is just "ProtoGeometry.dll".
            if (!HasPathInformation(subjectPath))
                return subjectPath;

            var documentUri = new Uri(basePath, UriKind.Absolute);
            var assemblyUri = new Uri(subjectPath, UriKind.Absolute);

            var relativeUri = documentUri.MakeRelativeUri(assemblyUri).OriginalString;

            // MakeRelativePath should not return a URL encoded path.
            // new Uri() results in a URL encoded path.
            // Therefore, to undo that, we need to call UrlDecode on it.
            // Also, UrlDecode will convert + to space, but the Uri creation
            // doesn't encode + as %2B. In order to avoid + in the filename
            // being converted to space, we need to encode + as %2B before calling it.
            var relativePath = WebUtility.UrlDecode(relativeUri.Replace("+", "%2B")).Replace('/', '\\');

            if (!HasPathInformation(relativePath))
            {
                relativePath = ".\\" + relativePath;
            }

            return relativePath;
        }

        /// <summary>
        /// Call this method to form the absolute path to target pointed to by 
        /// relativePath parameter. The absolute path is formed by computing both
        /// base path and the relative path.
        /// </summary>
        /// <param name="basePath">The base path from which the absolute path is 
        /// to be computed. This argument cannot be null or empty.</param>
        /// <param name="relativePath">The relative path to the target. This 
        /// argument cannot be null or empty.</param>
        /// <returns>Returns the absolute path.</returns>
        internal static string MakeAbsolutePath(string basePath, string relativePath)
        {
            if (string.IsNullOrEmpty(basePath))
                throw new ArgumentNullException("basePath");
            if (string.IsNullOrEmpty(relativePath))
                throw new ArgumentNullException("relativePath");

            // Determine if we have any directory information in the 
            // subjectPath. For example, we won't want to form an absolute 
            // path if the input of this method is just "ProtoGeometry.dll".
            if (!HasPathInformation(relativePath))
                return relativePath;

            var baseUri = new Uri(basePath, UriKind.Absolute);
            var relativeUri = new Uri(relativePath, UriKind.Relative);
            var resultUri = new Uri(baseUri, relativeUri);
            return resultUri.LocalPath;
        }

        /// <summary>
        /// Returns words from text, e.g. ImportFromCSV to ("Import","From","CSV")
        /// </summary>
        /// <param name="text">incoming string</param>
        /// <param name="maxCharacters">Max number of characters per row</param>
        internal static IEnumerable<string> WrapText(string text, int maxCharacters)
        {
            List<string> words = new List<string>();
            if (string.IsNullOrWhiteSpace(text) || maxCharacters < 2)
                return words;

            StringBuilder currentWord = new StringBuilder(text[0].ToString());
            for (int i = 1; i < text.Length; i++)
            {
                var curr = text[i];
                var prev = text[i - 1];

                if (Char.IsUpper(curr) && !Char.IsUpper(prev))
                {
                    words.Add(currentWord.ToString());
                    currentWord.Clear();
                }
                currentWord.Append(curr);
            }
            // Add last word.
            if (currentWord.Length != 0)
                words.Add(currentWord.ToString());

            // Try to merge words.
            List<string> result_words = new List<string>();
            currentWord.Clear();
            currentWord.Append(words[0]);
            foreach (var word in words)
            {
                // Case for first word, just pass it.
                if (currentWord.ToString() == word)
                    continue;

                if ((currentWord.Length + word.Length + 1) <= maxCharacters)
                {
                    currentWord.Append(" ");
                    currentWord.Append(word);
                }
                else
                {
                    result_words.Add(currentWord.ToString());
                    currentWord.Clear();
                    currentWord.Append(word);
                }
            }
            // Add last word.
            if (currentWord.Length != 0)
                result_words.Add(currentWord.ToString());

            return result_words;
        }

        /// <summary>
        /// Reduces the number of rows, based on the entries inside rows parameter.
        /// E.g. rows = { "Insert", "Day", "Of", "Week", "Here" }, maxRows == 3
        /// Result { "Insert", "Day", "Of Week Here" }
        /// </summary>
        /// <param name="rows">Incoming rows</param>
        /// <param name="maxRows">Max number of rows</param>
        internal static IEnumerable<string> ReduceRowCount(List<string> rows, int maxRows)
        {
            if (rows == null || maxRows <= 0)
                throw new ArgumentException();

            var results = new List<string>();
            foreach (var row in rows)
            {
                // There are still room in the results list.
                if (results.Count < maxRows)
                {
                    results.Add(row);
                    continue;
                }

                // Already full, keep appending to last row.
                var lastRow = results.Last();
                results.Remove(lastRow);
                results.Add(lastRow + " " + row);
            }

            return results;
        }

        /// <summary>
        /// Truncate each entry in the given "rows" to a maximum of "maxCharacters".
        /// For examples, given that "maxCharacters" equals to "8":
        /// 
        ///     { "Surface", "Analysis Data" } => { "Surface", "..s Data" }
        ///     { "By", "Geometry", "Coordinate", "System" } => { "By", "Geometry", "Coordi..", "System" }
        ///     { "By Geometry", "Coordinate System" } => { "By Geo..", "..System" }
        /// </summary>
        /// <param name="rows">Incomming rows</param>
        /// <param name="maxCharacters">Max number characters per row</param>
        internal static IEnumerable<string> TruncateRows(IEnumerable<string> rows, int maxCharacters)
        {
            if (rows == null || maxCharacters <= 2)
                throw new ArgumentException();
            if (rows.Count() == 0)
                return rows;

            int twoDotsLength = Configurations.TwoDots.Length;
            var maxAfterTruncate = maxCharacters - twoDotsLength;
            var results = new List<string>();
            var lastRow = rows.Last();

            foreach (var row in rows)
            {
                if (row.Length <= maxCharacters)
                {
                    results.Add(row);
                    continue;
                }

                if (row != lastRow || rows.Count() == 1)
                {
                    // Rows other than the last get dots appended to the end.
                    // Or if there is only one row.
                    results.Add(row.Substring(0, maxAfterTruncate) + Configurations.TwoDots);
                }
                else
                {
                    // The final row gets the dots added to the front. 
                    var offset = row.Length - maxAfterTruncate;
                    results.Add(Configurations.TwoDots + row.Substring(offset));
                }
            }

            return results;
        }

        /// <summary>
        /// Returns formatted <see cref="System.String"/> value 
        /// leaving only letters, numbers, dots and dashes.
        /// </summary>
        /// <param name="resource"><see cref="System.String"/> value to format</param>
        /// <returns>Formatted <see cref="System.String"/> value</returns>
        internal static string NormalizeAsResourceName(string resource)
        {
            if (string.IsNullOrWhiteSpace(resource))
                return "";

            StringBuilder newText = new StringBuilder(resource.Length);

            // Dots and minus we add, they are for overloaded methods.
            var query = resource.Where(
                c =>
                {
                    if (c == '.' || (c == '-'))
                        return true;

                    return Char.IsLetterOrDigit(c);
                });

            foreach (var c in query)
                newText.Append(c);

            var result = newText.ToString();
            return ((result == "-") ? string.Empty : result);
        }

        private static bool HasPathInformation(string fileNameOrPath)
        {
            int indexOfSeparator =
                fileNameOrPath.IndexOfAny(
                    new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });

            return indexOfSeparator >= 0;
        }
    }
}
