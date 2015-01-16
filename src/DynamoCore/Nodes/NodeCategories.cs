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
        public const string CORE = /*NXLT*/"Core";
        public const string CORE_INPUT = /*NXLT*/"Core.Input";
        public const string CORE_STRINGS = /*NXLT*/"Core.Strings";
        public const string CORE_LISTS_CREATE = /*NXLT*/"Core.List.Create";
        public const string CORE_LISTS_ACTION = /*NXLT*/"Core.List.Actions";
        public const string CORE_LISTS_QUERY = /*NXLT*/"Core.List.Query";
        public const string CORE_VIEW = /*NXLT*/"Core.View";
        public const string CORE_ANNOTATE = /*NXLT*/"Core.Annotate";
        public const string CORE_EVALUATE = /*NXLT*/"Core.Evaluate";
        public const string CORE_TIME = /*NXLT*/"Core.Time";
        public const string CORE_SCRIPTING = /*NXLT*/"Core.Scripting";
        public const string CORE_FUNCTIONS = /*NXLT*/"Core.Functions";
        public const string CORE_IO = /*NXLT*/"Core.File";

        public const string LOGIC = /*NXLT*/"Core.Logic";
        public const string LOGIC_MATH_ARITHMETIC = /*NXLT*/"Logic.Math.Arithmetic";
        public const string LOGIC_MATH_ROUNDING = /*NXLT*/"Logic.Math.Rounding";
        public const string LOGIC_MATH_CONSTANTS = /*NXLT*/"Logic.Math.Constants";
        public const string LOGIC_MATH_TRIGONOMETRY = /*NXLT*/"Logic.Math.Trigonometry";
        public const string LOGIC_MATH_RANDOM = /*NXLT*/"Logic.Math.Random";
        public const string LOGIC_MATH_OPTIMIZE = /*NXLT*/"Logic.Math.Optimize";
        public const string LOGIC_EFFECT = /*NXLT*/"Logic.Effect";
        public const string LOGIC_COMPARISON = /*NXLT*/"Logic.Comparison";
        public const string LOGIC_LOOP = /*NXLT*/"Logic.Loop";


        public const string GEOMETRY = /*NXLT*/"Geometry";

        public const string GEOMETRY_CURVE_CREATE = /*NXLT*/"Geometry.Curve.Create";
        public const string GEOMETRY_CURVE_DIVIDE = /*NXLT*/"Geometry.Curve.Divide";
        public const string GEOMETRY_CURVE_PRIMITIVES = /*NXLT*/"Geometry.Curve.Primitives";
        public const string GEOMETRY_CURVE_QUERY = /*NXLT*/"Geometry.Curve.Query";
        public const string GEOMETRY_CURVE_FIT = /*NXLT*/"Geometry.Curve.Fit";

        public const string GEOMETRY_POINT_CREATE = /*NXLT*/"Geometry.Point.Create";
        public const string GEOMETRY_POINT_MODIFY = /*NXLT*/"Geometry.Point.Modify";
        public const string GEOMETRY_POINT_QUERY = /*NXLT*/"Geometry.Point.Query";
        public const string GEOMETRY_POINT_GRID = /*NXLT*/"Geometry.Point.Grid";
        public const string GEOMETRY_POINT_TESSELATE = /*NXLT*/"Geometry.Point.Tesselate";

        public const string GEOMETRY_SOLID_BOOLEAN = /*NXLT*/"Geometry.Solid.Boolean";
        public const string GEOMETRY_SOLID_CREATE = /*NXLT*/"Geometry.Solid.Create";
        public const string GEOMETRY_SOLID_MODIFY = /*NXLT*/"Geometry.Solid.Modify";
        public const string GEOMETRY_SOLID_PRIMITIVES = /*NXLT*/"Geometry.Solid.Primitives";
        public const string GEOMETRY_SOLID_QUERY = /*NXLT*/"Geometry.Solid.Extract";
        public const string GEOMETRY_SOLID_REPAIR = /*NXLT*/"Geometry.Solid.Repair";

        public const string GEOMETRY_SURFACE_CREATE = /*NXLT*/"Geometry.Surface.Create";
        public const string GEOMETRY_SURFACE_QUERY = /*NXLT*/"Geometry.Surface.Query";
        public const string GEOMETRY_SURFACE_UV = /*NXLT*/"Geometry.Surface.UV";
        public const string GEOMETRY_SURFACE_DIVIDE = /*NXLT*/"Geometry.Surface.Divide";

        public const string GEOMETRY_TRANSFORM_APPLY = /*NXLT*/"Geometry.Transform.Apply";
        public const string GEOMETRY_TRANSFORM_MODIFY = /*NXLT*/"Geometry.Transform.Modify";
        public const string GEOMETRY_TRANSFORM_CREATE = /*NXLT*/"Geometry.Transform.Create";

        public const string GEOMETRY_INTERSECT = /*NXLT*/"Geometry.Intersect";

        public const string GEOMETRY_EXPERIMENTAL_PRIMITIVES = /*NXLT*/"Geometry.Experimental.Primitives";
        public const string GEOMETRY_EXPERIMENTAL_SURFACE = /*NXLT*/"Geometry.Experimental.Surface";
        public const string GEOMETRY_EXPERIMENTAL_CURVE = /*NXLT*/"Geometry.Experimental.Curve";
        public const string GEOMETRY_EXPERIMENTAL_SOLID = /*NXLT*/"Geometry.Experimental.Solid";
        public const string GEOMETRY_EXPERIMENTAL_MODIFY = /*NXLT*/"Geometry.Experimental.Modify";
        public const string GEOMETRY_EXPERIMENTAL_VIEW = /*NXLT*/"Geometry.Experimental.View";

        public const string REVIT = /*NXLT*/"Revit";
        public const string REVIT_DOCUMENT = /*NXLT*/"Revit.Document";
        public const string REVIT_DATUMS = /*NXLT*/"Revit.Datums";
        public const string REVIT_FAMILIES = /*NXLT*/"Revit.Families";
        public const string REVIT_SELECTION = /*NXLT*/"Revit.Selection";
        public const string REVIT_VIEW = /*NXLT*/"Revit.View";
        public const string REVIT_REFERENCE = /*NXLT*/"Revit.Reference";
        public const string REVIT_PARAMETERS = /*NXLT*/"Revit.Parameters";
        public const string REVIT_BAKE = /*NXLT*/"Revit.Bake";
        public const string REVIT_API = /*NXLT*/"Revit.API";

        public const string ANALYZE = /*NXLT*/"Analyze";
        public const string ANALYZE_MEASURE = /*NXLT*/"Analyze.Measure";
        public const string ANALYZE_DISPLAY = /*NXLT*/"Analyze.Display";
        public const string ANALYZE_COLOR = /*NXLT*/"Analyze.Color";
        public const string ANALYZE_STRUCTURE = /*NXLT*/"Analyze.Structure";
        public const string ANALYZE_CLIMATE = /*NXLT*/"Analyze.Climate";
        public const string ANALYZE_ACOUSTIC = /*NXLT*/"Analyze.Acoustic";
        public const string ANALYZE_SOLAR = /*NXLT*/"Analyze.Solar";

        public const string IO = /*NXLT*/"Input/Output";
        public const string IO_FILE = /*NXLT*/"Input/Output.File";
        public const string IO_NETWORK = /*NXLT*/"Input/Output.Network";
        public const string IO_HARDWARE = /*NXLT*/"Input/Output.Hardware";
    }

    public static class Utilities
    {
        public static string Ellipsis(string value, int desiredLength)
        {
            return desiredLength > value.Length ? value : value.Remove(desiredLength - 1) + /*NXLT*/"...";
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
                throw new ArgumentNullException(/*NXLT*/"fullyQualifiedName");

            // older files will have nodes in the Dynamo.Elements namespace
            const string oldPrefix = /*NXLT*/"Dynamo.Elements.";
            const string newPrefix = /*NXLT*/"Dynamo.Nodes.";
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
            if (className.StartsWith(/*NXLT*/"dyn"))
                className = className.Remove(0, 3);

            // Older files will have nodes that use "XYZ" and "UV" 
            // instead of "Xyz" and "Uv". Update these names.
            className = className.Replace(/*NXLT*/"XYZ", /*NXLT*/"Xyz");
            className = className.Replace(/*NXLT*/"UV", /*NXLT*/"Uv");
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
                throw new ArgumentNullException(/*NXLT*/"document");

            if (document.DocumentElement == null)
            {
                const string message = /*NXLT*/"'XmlDocument.DocumentElement' cannot be null";
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
                throw new ArgumentNullException(/*NXLT*/"document");

            if (document.DocumentElement == null)
            {
                const string message = /*NXLT*/"'XmlDocument.DocumentElement' cannot be null";
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
                    string.Format(/*NXLT*/"'{0}' attribute not found in XmlDocument",
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
                throw new ArgumentNullException(/*NXLT*/"document");

            if (document.DocumentElement == null)
            {
                const string message = /*NXLT*/"Document does not have a root element";
                throw new ArgumentException(message, /*NXLT*/"document");
            }

            if (nodeTraceDataList == null)
                throw new ArgumentNullException(/*NXLT*/"nodeTraceDataList");

            if (!nodeTraceDataList.Any())
            {
                const string message = /*NXLT*/"Trade data list must be non-empty";
                throw new ArgumentException(message, /*NXLT*/"nodeTraceDataList");
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
                throw new ArgumentNullException(/*NXLT*/"document");

            if (document.DocumentElement == null)
            {
                const string message = /*NXLT*/"Document does not have a root element";
                throw new ArgumentException(message, /*NXLT*/"document");
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
                throw new ArgumentNullException(/*NXLT*/"basePath");

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
                relativePath = /*NXLT*/".\\" + relativePath;
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
                throw new ArgumentNullException(/*NXLT*/"basePath");
            if (string.IsNullOrEmpty(relativePath))
                throw new ArgumentNullException(/*NXLT*/"relativePath");

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
