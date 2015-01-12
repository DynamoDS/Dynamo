using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.UI;
using System.IO;

namespace Dynamo.Nodes
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
        public const string CORE_STRINGS = "Core.Strings";
        public const string CORE_LISTS_CREATE = "Core.List.Create";
        public const string CORE_LISTS_ACTION = "Core.List.Actions";        
        public const string CORE_LISTS_QUERY = "Core.List.Query";
        public const string CORE_VIEW = "Core.View";
        public const string CORE_ANNOTATE = "Core.Annotate";
        public const string CORE_EVALUATE = "Core.Evaluate";
        public const string CORE_TIME = "Core.Time";
        public const string CORE_SCRIPTING = "Core.Scripting";
        public const string CORE_FUNCTIONS = "Core.Functions";
        public const string CORE_IO = "Core.File";

        public const string LOGIC = "Core.Logic";
        public const string LOGIC_MATH_ARITHMETIC = "Logic.Math.Arithmetic";
        public const string LOGIC_MATH_ROUNDING = "Logic.Math.Rounding";
        public const string LOGIC_MATH_CONSTANTS = "Logic.Math.Constants";
        public const string LOGIC_MATH_TRIGONOMETRY = "Logic.Math.Trigonometry";
        public const string LOGIC_MATH_RANDOM = "Logic.Math.Random";
        public const string LOGIC_MATH_OPTIMIZE = "Logic.Math.Optimize";
        public const string LOGIC_EFFECT = "Logic.Effect";
        public const string LOGIC_COMPARISON = "Logic.Comparison";
        public const string LOGIC_LOOP = "Logic.Loop";


        public const string GEOMETRY = "Geometry";

        public const string GEOMETRY_CURVE_CREATE = "Geometry.Curve.Create";
        public const string GEOMETRY_CURVE_DIVIDE = "Geometry.Curve.Divide";
        public const string GEOMETRY_CURVE_PRIMITIVES = "Geometry.Curve.Primitives";
        public const string GEOMETRY_CURVE_QUERY = "Geometry.Curve.Query";
        public const string GEOMETRY_CURVE_FIT = "Geometry.Curve.Fit";

        public const string GEOMETRY_POINT_CREATE = "Geometry.Point.Create";
        public const string GEOMETRY_POINT_MODIFY = "Geometry.Point.Modify";
        public const string GEOMETRY_POINT_QUERY = "Geometry.Point.Query";
        public const string GEOMETRY_POINT_GRID = "Geometry.Point.Grid";
        public const string GEOMETRY_POINT_TESSELATE = "Geometry.Point.Tesselate";

        public const string GEOMETRY_SOLID_BOOLEAN = "Geometry.Solid.Boolean";
        public const string GEOMETRY_SOLID_CREATE = "Geometry.Solid.Create";
        public const string GEOMETRY_SOLID_MODIFY = "Geometry.Solid.Modify";
        public const string GEOMETRY_SOLID_PRIMITIVES = "Geometry.Solid.Primitives";
        public const string GEOMETRY_SOLID_QUERY = "Geometry.Solid.Extract";
        public const string GEOMETRY_SOLID_REPAIR = "Geometry.Solid.Repair";

        public const string GEOMETRY_SURFACE_CREATE = "Geometry.Surface.Create";
        public const string GEOMETRY_SURFACE_QUERY = "Geometry.Surface.Query";
        public const string GEOMETRY_SURFACE_UV = "Geometry.Surface.UV";
        public const string GEOMETRY_SURFACE_DIVIDE = "Geometry.Surface.Divide";

        public const string GEOMETRY_TRANSFORM_APPLY = "Geometry.Transform.Apply";
        public const string GEOMETRY_TRANSFORM_MODIFY = "Geometry.Transform.Modify";
        public const string GEOMETRY_TRANSFORM_CREATE = "Geometry.Transform.Create";

        public const string GEOMETRY_INTERSECT = "Geometry.Intersect";

        public const string GEOMETRY_EXPERIMENTAL_PRIMITIVES = "Geometry.Experimental.Primitives";
        public const string GEOMETRY_EXPERIMENTAL_SURFACE = "Geometry.Experimental.Surface";
        public const string GEOMETRY_EXPERIMENTAL_CURVE = "Geometry.Experimental.Curve";
        public const string GEOMETRY_EXPERIMENTAL_SOLID = "Geometry.Experimental.Solid";
        public const string GEOMETRY_EXPERIMENTAL_MODIFY = "Geometry.Experimental.Modify";
        public const string GEOMETRY_EXPERIMENTAL_VIEW = "Geometry.Experimental.View";

        public const string REVIT = "Revit";
        public const string REVIT_DOCUMENT = "Revit.Document";
        public const string REVIT_DATUMS = "Revit.Datums";
        public const string REVIT_FAMILIES = "Revit.Families";
        public const string REVIT_SELECTION = "Revit.Selection";
        public const string REVIT_VIEW = "Revit.View";
        public const string REVIT_REFERENCE = "Revit.Reference";
        public const string REVIT_PARAMETERS = "Revit.Parameters";
        public const string REVIT_BAKE = "Revit.Bake";
        public const string REVIT_API = "Revit.API";

        public const string ANALYZE = "Analyze";
        public const string ANALYZE_MEASURE = "Analyze.Measure";
        public const string ANALYZE_DISPLAY = "Analyze.Display";
        public const string ANALYZE_COLOR = "Analyze.Color";
        public const string ANALYZE_STRUCTURE = "Analyze.Structure";
        public const string ANALYZE_CLIMATE = "Analyze.Climate";
        public const string ANALYZE_ACOUSTIC = "Analyze.Acoustic";
        public const string ANALYZE_SOLAR = "Analyze.Solar";

        public const string IO = "Input/Output";
        public const string IO_FILE = "Input/Output.File";
        public const string IO_NETWORK = "Input/Output.Network";
        public const string IO_HARDWARE = "Input/Output.Hardware";
    }

    public static class Utilities
    {
        public static string Ellipsis(string value, int desiredLength)
        {
            return desiredLength > value.Length ? value : value.Remove(desiredLength - 1) + "...";
        }

        /// <summary>
        /// <para>This method patches the fullyQualifiedName of a given type. It 
        /// updates the given name to its newer form (i.e. "Dynamo.Nodes.Xyz")
        /// if it matches the older form (e.g. "Dynamo.Elements.Xyz").</para>
        /// <para>The method also attempts to update "XYZ/UV" convention to 
        /// "Xyz/Uv" to comply with the new Dynamo naming convention.</para>
        /// </summary>
        /// <param name="fullyQualifiedName">A fully qualified name. An example
        /// of this would be "Dynamo.Elements.dynNode".</param>
        /// <returns>The processed fully qualified name. For an example, the 
        /// name "Dynamo.Elements.UV" will be returned as "Dynamo.Nodes.Uv".
        /// </returns>
        public static string PreprocessTypeName(string fullyQualifiedName)
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
        public static void SaveTraceDataToXmlDocument(XmlDocument document,
            IEnumerable<KeyValuePair<Guid, List<string>>> nodeTraceDataList)
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

                    callsiteXmlElement.InnerText = data;
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
        public static IEnumerable<KeyValuePair<Guid, List<string>>>
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

            var loadedData = new Dictionary<Guid, List<string>>();
            if (!query.Any()) // There's no data, return empty dictionary.
                return loadedData;

            XmlElement sessionElement = query.ElementAt(0);
            foreach (XmlElement nodeElement in sessionElement.ChildNodes)
            {
                var guid = Guid.Parse(nodeElement.GetAttribute(Configurations.NodeIdAttribName));
                var callsites = nodeElement.ChildNodes.Cast<XmlElement>().Select(e => e.InnerText).ToList();
                loadedData.Add(guid, callsites);
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

            var relativeUri = documentUri.MakeRelativeUri(assemblyUri);
            var relativePath = relativeUri.OriginalString.Replace('/', '\\');
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

        private static bool HasPathInformation(string fileNameOrPath)
        {
            int indexOfSeparator =
                fileNameOrPath.IndexOfAny(
                    new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });

            return indexOfSeparator >= 0;
        }
    }
}
