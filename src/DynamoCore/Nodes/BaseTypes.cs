using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Autodesk.DesignScript.Runtime;
using Dynamo.Core;
using Dynamo.Models;
using Dynamo.Services;
using Dynamo.Utilities;
using System.Globalization;

using Dynamo.ViewModels;

using ProtoCore.AST.AssociativeAST;
using System.IO;
using Dynamo.UI;
using System.Web;

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
        enum ButtonId
        {
            OK = 43420,
            Cancel,
            DownloadLatest,
            Proceed,
            Submit
        }

        public static string Ellipsis(string value, int desiredLength)
        {
            if (desiredLength > value.Length)
            {
                return value;
            }
            else
            {
                return value.Remove(desiredLength - 1) + "...";
            }
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
            string oldPrefix = "Dynamo.Elements.";
            string newPrefix = "Dynamo.Nodes.";
            string className = string.Empty;

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
        /// <para>Resolve either a built-in type or a system type, given its fully
        /// qualified name. This method performs the search with the following 
        /// order:</para>
        /// <para>1. Search among the built-in types registered with 
        /// DynamoModel.BuiltInTypesByName dictionary</para>
        /// <para>2. Search among the available .NET runtime types</para>
        /// <para>3. Search among built-in types, taking their "also-known-as" 
        /// attributes into consideration when matching the type name</para>
        /// </summary>
        /// <param name="fullyQualifiedName"></param>
        /// <returns></returns>
        public static System.Type ResolveType(DynamoModel dynamoModel, string fullyQualifiedName)
        {
            if (string.IsNullOrEmpty(fullyQualifiedName))
                throw new ArgumentNullException("fullyQualifiedName");

            TypeLoadData tData = null;
            var builtInTypes = dynamoModel.BuiltInTypesByName;
            if (builtInTypes.TryGetValue(fullyQualifiedName, out tData))
                return tData.Type; // Found among built-in types, return it.

            //try and get a system type by this name
            Type type = Type.GetType(fullyQualifiedName);
            if (null != type)
                return type;

            // If we still can't find the type, try the also known as attributes.
            foreach (var builtInType in dynamoModel.BuiltInTypesByName)
            {
                var attribs = builtInType.Value.Type.GetCustomAttributes(
                    typeof(AlsoKnownAsAttribute), false);

                if (attribs.Count() <= 0)
                    continue;

                AlsoKnownAsAttribute akaAttrib = attribs[0] as AlsoKnownAsAttribute;
                if (akaAttrib.Values.Contains(fullyQualifiedName))
                {
                    dynamoModel.Logger.Log(string.Format(
                        "Found matching node for {0} also known as {1}",
                        builtInType.Key, fullyQualifiedName));

                    return builtInType.Value.Type; // Found a matching type.
                }
            }

            dynamoModel.Logger.Log(string.Format(
                "Could not load node of type: {0}", fullyQualifiedName));

            dynamoModel.Logger.Log("Loading will continue but nodes " +
                "might be missing from your workflow.");

            return null;
        }

        public static TypeLoadData GetDataForType(DynamoModel dynamoModel, Type t)
        {
            //only load types that are in the right namespace, are not abstract
            //and have the elementname attribute
            var obsoleteMsg = "";

            var obsAttrs = t.GetCustomAttributes(typeof(ObsoleteAttribute), true);
            if (null != obsAttrs && obsAttrs.Any())
            {
                var attr = obsAttrs[0] as ObsoleteAttribute;
                if (null != attr)
                {
                    obsoleteMsg = attr.Message;
                    if (string.IsNullOrEmpty(obsoleteMsg))
                        obsoleteMsg = "Obsolete";
                }
            }

            if (!IsNodeSubType(t) && t.Namespace != "Dynamo.Nodes")
                return null;

            //if we are running in revit (or any context other than NONE) use the DoNotLoadOnPlatforms attribute, 
            //if available, to discern whether we should load this type
            if (!dynamoModel.Context.Equals(Context.NONE))
            {
                object[] platformExclusionAttribs = t.GetCustomAttributes(typeof(DoNotLoadOnPlatformsAttribute), false);
                if (platformExclusionAttribs.Length > 0)
                {
                    string[] exclusions = (platformExclusionAttribs[0] as DoNotLoadOnPlatformsAttribute).Values;

                    //if the attribute's values contain the context stored on the Model
                    //then skip loading this type.
                    if (exclusions.Reverse().Any(e => e.Contains(dynamoModel.Context)))
                        return null;
                }
            }
            return new TypeLoadData(t.Assembly, t, obsoleteMsg);
        }
        
        /// <summary>
        ///     Determine if a Type is a node.  Used by LoadNodesFromAssembly to figure
        ///     out what nodes to load from other libraries (.dlls).
        /// </summary>
        /// <parameter>The type</parameter>
        /// <returns>True if the type is node.</returns>
        public static bool IsNodeSubType(Type t)
        {
            return //t.Namespace == "Dynamo.Nodes" &&
                   !t.IsAbstract &&
                   t.IsSubclassOf(typeof(NodeModel));
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
                var message = "'XmlDocument.DocumentElement' cannot be null";
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
                var message = "'XmlDocument.DocumentElement' cannot be null";
                throw new ArgumentException(message);
            }

            // If XmlDocument is opened from an existing file...
            if (!string.IsNullOrEmpty(document.BaseURI))
            {
                Uri documentUri = new Uri(document.BaseURI, UriKind.Absolute);
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
                var message = "Document does not have a root element";
                throw new ArgumentException(message, "document");
            }

            if (nodeTraceDataList == null)
                throw new ArgumentNullException("nodeTraceDataList");

            if (nodeTraceDataList.Count() <= 0)
            {
                var message = "Trade data list must be non-empty";
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
                var message = "Document does not have a root element";
                throw new ArgumentException(message, "document");
            }

            var childNodes = document.DocumentElement.ChildNodes.Cast<XmlElement>();
            var sessionXmlTagName = Configurations.SessionTraceDataXmlTag;
            var query = from childNode in childNodes
                        where childNode.Name.Equals(sessionXmlTagName)
                        select childNode;

            var loadedData = new Dictionary<Guid, List<string>>();
            if (query.Count() <= 0) // There's no data, return empty dictionary.
                return loadedData;

            XmlElement sessionElement = query.ElementAt(0);
            foreach (XmlElement nodeElement in sessionElement.ChildNodes)
            {
                List<string> callsites = new List<string>();
                foreach (XmlElement callsiteElement in nodeElement.ChildNodes)
                    callsites.Add(callsiteElement.InnerText);

                var guid = nodeElement.GetAttribute(Configurations.NodeIdAttribName);
                loadedData.Add(Guid.Parse(guid), callsites);
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

            Uri documentUri = new Uri(basePath, UriKind.Absolute);
            Uri assemblyUri = new Uri(subjectPath, UriKind.Absolute);

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

            Uri baseUri = new Uri(basePath, UriKind.Absolute);
            Uri relativeUri = new Uri(relativePath, UriKind.Relative);
            Uri resultUri = new Uri(baseUri, relativeUri);
            return resultUri.LocalPath;
        }

        /// <summary>
        /// Call this method to display a message box when a file of an older 
        /// version cannot be opened by the current version of Dynamo.
        /// </summary>
        /// <param name="fileVersion">Version of the input file.</param>
        /// <param name="currVersion">Current version of the Dynamo.</param>
        internal static void DisplayObsoleteFileMessage( DynamoModel dynamoModel,
            string fullFilePath, Version fileVersion, Version currVersion)
        {
            var fileVer = ((fileVersion != null) ? fileVersion.ToString() : "Unknown");
            var currVer = ((currVersion != null) ? currVersion.ToString() : "Unknown");

            InstrumentationLogger.LogPiiInfo("ObsoleteFileMessage", fullFilePath +
                " :: fileVersion:" + fileVer + " :: currVersion:" + currVer);

            var summary = "Your file cannot be opened";
            var description = string.Format("Your file '{0}' of version '{1}' cannot " +
                "be opened by this version of Dynamo ({2})", fullFilePath, fileVersion, currVersion);

            var imageUri = "/DynamoCore;component/UI/Images/task_dialog_obsolete_file.png";
            var args = new Dynamo.UI.Prompts.TaskDialogEventArgs(
                new Uri(imageUri, UriKind.Relative),
                "Obsolete File", summary, description);

            args.AddRightAlignedButton((int)Utilities.ButtonId.OK, "OK");

            dynamoModel.OnRequestTaskDialog(null, args);
        }

        /// <summary>
        /// Call this method to display an error message in an event when live 
        /// runner throws an exception that is not handled anywhere else. This 
        /// message instructs user to save their work and restart Dynamo.
        /// </summary>
        /// <param name="exception">The exception to display.</param>
        internal static void DisplayEngineFailureMessage(DynamoModel dynamoModel, Exception exception)
        {
            StabilityTracking.GetInstance().NotifyCrash();
            InstrumentationLogger.LogAnonymousEvent("EngineFailure", "Stability");

            if (exception != null)
            {
                InstrumentationLogger.LogException(exception);
            }

            var summary = "Unhandled exception in Dynamo engine";
            var description = "The virtual machine that powers Dynamo is " +
                "experiencing some unexpected errors internally and is likely " +
                "having great difficulties pulling itself together. It is " +
                "recommended that you save your work now and reload the file. " +
                "Giving the Dynamo VM a new lease of life can potentially make " +
                "it feel happier and behave better.\n\n" +
                "If you don't mind, it would be helpful for you to send us your " +
                "file. That will make it quicker for us to get these issues fixed.";

            if (exception is ProtoCore.Exceptions.HeapCorruptionException)
            {
                description = exception.Message;
            }

            var imageUri = "/DynamoCore;component/UI/Images/task_dialog_crash.png";
            var args = new Dynamo.UI.Prompts.TaskDialogEventArgs(
                new Uri(imageUri, UriKind.Relative),
                "Unhandled exception", summary, description);

            args.AddRightAlignedButton((int)Utilities.ButtonId.Submit, "Submit Bug To Github");
            args.AddRightAlignedButton((int)Utilities.ButtonId.OK, "Arrrrg, ok");
            args.Exception = exception;

            dynamoModel.OnRequestTaskDialog(null, args);
            if (args.ClickedButtonId == (int)Utilities.ButtonId.Submit)
                DynamoViewModel.ReportABug(null);
        }

        private static bool HasPathInformation(string fileNameOrPath)
        {
            int indexOfSeparator = fileNameOrPath.IndexOfAny(new char[]
            {
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            });

            return indexOfSeparator >= 0;
        }

        /// <summary>
        /// Displays file open error dialog if the file is of a future version than the currently installed version
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <param name="fileVersion"></param>
        /// <param name="currVersion"></param>
        /// <returns> true if the file must be opened and false otherwise </returns>
        internal static bool DisplayFutureFileMessage(DynamoModel dynamoModel, string fullFilePath, Version fileVersion, Version currVersion)
        {
            var fileVer = ((fileVersion != null) ? fileVersion.ToString() : "Unknown");
            var currVer = ((currVersion != null) ? currVersion.ToString() : "Unknown");

            InstrumentationLogger.LogPiiInfo("FutureFileMessage", fullFilePath +
                " :: fileVersion:" + fileVer + " :: currVersion:" + currVer);

            var summary = "Your file may not open correctly";
            var description = string.Format("Your file '{0}' was created in future version '{1}' and may not " +
                "open correctly in your installed version of Dynamo '{2}'", fullFilePath, fileVersion, currVersion);

            var imageUri = "/DynamoCore;component/UI/Images/task_dialog_future_file.png";
            var args = new Dynamo.UI.Prompts.TaskDialogEventArgs(
                new Uri(imageUri, UriKind.Relative),
                "Future File", summary, description);
            args.ClickedButtonId = (int)Utilities.ButtonId.Cancel;
            
            args.AddRightAlignedButton((int)Utilities.ButtonId.Cancel, "Cancel");
            args.AddRightAlignedButton((int)Utilities.ButtonId.DownloadLatest, "Download latest version");
            args.AddRightAlignedButton((int)Utilities.ButtonId.Proceed, "Proceed anyway");
            
            dynamoModel.OnRequestTaskDialog(null, args);
            if (args.ClickedButtonId == (int)Utilities.ButtonId.DownloadLatest)
            {
                // this should be an event on DynamoModel
                DynamoViewModel.DownloadDynamo();
                return false;
            }

            return args.ClickedButtonId == (int)Utilities.ButtonId.Proceed;
        }
    }

    public abstract partial class VariableInput : NodeModel
    {
        public VariableInput(WorkspaceModel ws)
            : base(ws)
        {
        }

        protected abstract string GetInputRootName();
        protected abstract string GetTooltipRootName();

        protected virtual int GetInputNameIndex()
        {
            return InPortData.Count;
        }

        protected internal virtual void RemoveInput()
        {
            var count = InPortData.Count;
            if (count > 0)
            {
                InPortData.RemoveAt(count - 1);
            }
        }

        protected internal virtual void AddInput()
        {
            var idx = GetInputNameIndex();
            InPortData.Add(new PortData(GetInputRootName() + idx, GetTooltipRootName() + idx));
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData)
            {
                XmlElement input = xmlDoc.CreateElement("Input");

                input.SetAttribute("name", inport.NickName);

                nodeElement.AppendChild(input);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            int i = InPortData.Count;
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (i > 0)
                {
                    i--;
                    continue;
                }

                if (subNode.Name == "Input")
                {
                    InPortData.Add(new PortData(subNode.Attributes["name"].Value, ""));
                }
            }
            RegisterAllPorts();
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            XmlDocument xmlDoc = element.OwnerDocument;
            foreach (var inport in InPortData)
            {
                XmlElement input = xmlDoc.CreateElement("Input");
                input.SetAttribute("name", inport.NickName);
                element.AppendChild(input);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called

            if (context == SaveContext.Undo)
            {
                //Reads in the new number of ports required from the data stored in the Xml Element
                //during Serialize (nextLength). Changes the current In Port Data to match the
                //required size by adding or removing port data.
                int currLength = InPortData.Count;
                XmlNodeList inNodes = element.SelectNodes("Input");
                int nextLength = inNodes.Count;
                if (nextLength > currLength)
                {
                    for (; currLength < nextLength; currLength++)
                    {
                        XmlNode subNode = inNodes.Item(currLength);
                        string nickName = subNode.Attributes["name"].Value;
                        InPortData.Add(new PortData(nickName, ""));
                    }
                }
                else if (nextLength < currLength)
                    InPortData.RemoveRange(nextLength, currLength - nextLength);

                RegisterAllPorts();
            }
        }

        #endregion
    }

    public abstract partial class VariableInputAndOutput : NodeModel
    {
        protected VariableInputAndOutput(WorkspaceModel ws) : base(ws)
        {
        }

        protected abstract string GetInputRootName();
        protected abstract string GetOutputRootName();
        protected abstract string GetTooltipRootName();

        protected virtual int GetInputNameIndex()
        {
            return InPortData.Count;
        }

        protected virtual void RemoveInput()
        {
            var count = InPortData.Count;
            if (count > 0)
            {
                InPortData.RemoveAt(count - 1);
                OutPortData.RemoveAt(count - 1);
            }
        }

        protected internal virtual void AddInput()
        {
            var idx = GetInputNameIndex();
            InPortData.Add(new PortData(GetInputRootName() + idx, GetTooltipRootName() + idx));
            OutPortData.Add(new PortData(GetOutputRootName() + idx, GetTooltipRootName() + idx));
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData)
            {
                XmlElement input = xmlDoc.CreateElement("Input");

                input.SetAttribute("name", inport.NickName);

                nodeElement.AppendChild(input);
            }

            foreach (var outport in OutPortData)
            {
                XmlElement output = xmlDoc.CreateElement("Output");

                output.SetAttribute("name", outport.NickName);

                nodeElement.AppendChild(output);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            int i = InPortData.Count;
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (i > 0)
                {
                    i--;
                    continue;
                }

                if (subNode.Name == "Input")
                {
                    InPortData.Add(new PortData(subNode.Attributes["name"].Value, ""));
                }
                else if (subNode.Name == "Output")
                {
                    OutPortData.Add(new PortData(subNode.Attributes["name"].Value, ""));
                }
            }
            RegisterAllPorts();
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            XmlDocument xmlDoc = element.OwnerDocument;
            foreach (var inport in InPortData)
            {
                XmlElement input = xmlDoc.CreateElement("Input");
                input.SetAttribute("name", inport.NickName);
                element.AppendChild(input);
            }
            foreach (var outport in OutPortData)
            {
                XmlElement output = xmlDoc.CreateElement("Output");
                output.SetAttribute("name", outport.NickName);
                element.AppendChild(output);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called

            if (context == SaveContext.Undo)
            {
                //Reads in the new number of ports required from the data stored in the Xml Element
                //during Serialize (nextLength). Changes the current In Port Data to match the
                //required size by adding or removing port data.

                // INPUTS
                int currLength = InPortData.Count;
                XmlNodeList inNodes = element.SelectNodes("Input");
                int nextLength = inNodes.Count;
                if (nextLength > currLength)
                {
                    for (; currLength < nextLength; currLength++)
                    {
                        XmlNode subNode = inNodes.Item(currLength);
                        string nickName = subNode.Attributes["name"].Value;
                        InPortData.Add(new PortData(nickName, ""));
                    }
                }
                else if (nextLength < currLength)
                    InPortData.RemoveRange(nextLength, currLength - nextLength);

                // OUTPUTS
                currLength = OutPortData.Count;
                XmlNodeList outNodes = element.SelectNodes("Output");
                nextLength = outNodes.Count;
                if (nextLength > currLength)
                {
                    for (; currLength < nextLength; currLength++)
                    {
                        XmlNode subNode = outNodes.Item(currLength);
                        string nickName = subNode.Attributes["name"].Value;
                        OutPortData.Add(new PortData(nickName, ""));
                    }
                }
                else if (nextLength < currLength)
                    OutPortData.RemoveRange(nextLength, currLength - nextLength);

                RegisterAllPorts();
            }
        }

        #endregion
    }

    /// <summary>
    /// Description:
    /// Builds sublists from a list. Inputs are a list and an offset to indicate the number of items to skip before
    /// the start of each subsequent sublist. Enter a range of values using series syntax to indicate the first sublist.
    /// </summary>
    [NodeName("Build Sublists")]
    [NodeCategory(BuiltinNodeCategories.CORE_LISTS_CREATE)]
    [NodeDescription("Build sublists from a list using DesignScript range syntax.")]
    public partial class Sublists : BasicInteractive<string>
    {
        public Sublists(WorkspaceModel ws): base(ws)
        {
            InPortData.Add(new PortData("list", "The list from which to create sublists."));
            InPortData.Add(new PortData("offset", "The offset to apply to the sub-list. Ex. The range \"0..2\" with an offset of 1 will yield sublists {0,1,2}{1,2,3}{2,3,4}..."));

            OutPortData.RemoveAt(0); //remove the existing blank output
            OutPortData.Add(new PortData("list", "The sublists."));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Longest;
            Value = "";
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            base.LoadNode(nodeElement);
            processTextForNewInputs();
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("value", Value);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called
            processTextForNewInputs();
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                Value = helper.ReadString("value");
            }
        }

        #endregion

        private void processTextForNewInputs()
        {
            var parameters = new List<string>();

            try
            {
                _parsed = DoubleInput.ParseValue(Value, new[] { ',' }, parameters, TokenConvert);

                if (InPortData.Count > 2)
                    InPortData.RemoveRange(2, InPortData.Count - 2);

                foreach (string parameter in parameters)
                {
                    InPortData.Add(new PortData(parameter, "variable"));
                }

                RegisterInputPorts();
                ClearRuntimeError();
            }
            catch (Exception e)
            {
                Error(e.Message);
            }
        }

        private double TokenConvert(double value)
        {
            return value;
        }

        internal static readonly Regex IdentifierPattern = new Regex(@"(?<id>[a-zA-Z_][^ ]*)|\[(?<id>\w(?:[^}\\]|(?:\\}))*)\]");
        internal static readonly string[] RangeSeparatorTokens = { "..", ":", };
        private List<DoubleInput.IDoubleSequence> _parsed;

        private static List<Tuple<int, int, int>> processText(string text, int maxVal, Func<string, int> idFoundCallback)
        {
            text = text.Replace(" ", "");

            string[] chunks = text.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (!chunks.Any())
                throw new Exception("Sub-list expression could not be parsed.");

            var ranges = new List<Tuple<int, int, int>>();

            foreach (string chunk in chunks)
            {
                string[] valueRange = chunk.Split(RangeSeparatorTokens, StringSplitOptions.RemoveEmptyEntries);

                int start = 0;
                int step = 1;

                if (!int.TryParse(valueRange[0], out start))
                {
                    var match = IdentifierPattern.Match(valueRange[0]);
                    if (match.Success)
                    {
                        start = idFoundCallback(match.Groups["id"].Value);
                    }
                    else
                    {
                        throw new Exception("Range start could not be parsed.");
                    }
                }

                int end = start;

                if (valueRange.Length > 1)
                {
                    if (!int.TryParse(valueRange[1], out end))
                    {
                        var match = IdentifierPattern.Match(valueRange[1]);
                        if (match.Success)
                        {
                            end = idFoundCallback(match.Groups["id"].Value);
                        }
                        else
                        {
                            throw new Exception("Range " + (valueRange.Length > 2 ? "step" : "end") + "could not be parsed.");
                        }
                    }
                }

                if (valueRange.Length > 2)
                {
                    if (!int.TryParse(valueRange[2], out end))
                    {
                        var match = IdentifierPattern.Match(valueRange[2]);
                        if (match.Success)
                        {
                            step = idFoundCallback(match.Groups["id"].Value);
                        }
                        else
                        {
                            throw new Exception("Range end could not be parsed.");
                        }
                    }
                }

                if (start < 0 || end < 0 || step <= 0)
                    throw new Exception("Range values must be greater than zero.");

                //if any values are greater than the length of the list - fail
                if (start >= maxVal || end >= maxVal)
                    throw new Exception("The start or end of a range is greater than the number of available elements in the list.");

                ranges.Add(Tuple.Create(start, end, step));
            }

            return ranges;
        }

        //public override Value Evaluate(FSharpList<Value> args)
        //{
        //    var list = ((Value.List)args[0]).Item;
        //    var len = list.Length;
        //    var offset = Convert.ToInt32(((Value.Number)args[1]).Item);

        //    if (offset <= 0)
        //        throw new Exception("\"" + InPortData[1].NickName + "\" argument must be greater than zero.");

        //    //sublist creation semantics are as follows:
        //    //EX. 1..2,5..8
        //    //This expression says give me elements 1-2 then jump 3 and give me elements 5-8
        //    //For a list 1,2,3,4,5,6,7,8,9,10, this will give us
        //    //1,2,5,8,2,3,6,9

        //    var paramLookup = args.Skip(2)
        //                          .Select(
        //                              (x, i) => new { Name = InPortData[i + 2].NickName, Argument = x })
        //                          .ToDictionary(x => x.Name, x => ((Value.Number)x.Argument).Item);

        //    var ranges = _parsed
        //        .Select(x => x.GetValue(paramLookup).Select(Convert.ToInt32).ToList())
        //        .ToList();

        //    //move through the list, creating sublists
        //    var finalList = new List<Value>();

        //    for (int j = 0; j < len; j += offset)
        //    {
        //        var currList = new List<Value>();

        //        var query = ranges.Where(r => r[0] + j <= len - 1 && r.Last() + j <= len - 1);
        //        foreach (var range in query)
        //        {
        //            currList.AddRange(range.Select(i => list.ElementAt(j + i)));
        //        }

        //        if (currList.Any())
        //            finalList.Add(FScheme.Value.NewList(currList.ToFSharpList()));
        //    }

        //    return FScheme.Value.NewList(finalList.ToFSharpList());
        //}

        protected override string SerializeValue(string val)
        {
            return val;
        }

        protected override string DeserializeValue(string val)
        {
            return val;
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            // Create DSFunction node
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            var newNode = MigrationManager.CreateFunctionNodeFrom(oldNode);
            MigrationManager.SetFunctionSignature(newNode, "DSCoreNodes.dll",
                "List.Sublists", "List.Sublists@var[]..[],var[]..[],int");
            newNode.SetAttribute("lacing","shortest");
            migrationData.AppendNode(newNode);
            string newNodeId = MigrationManager.GetGuidFromXmlElement(newNode);

            // Create code block node
            string rangesString = "{0}";
            foreach (XmlNode childNode in oldNode.ChildNodes)
            {
                if (childNode.Name.Equals(typeof(string).FullName))
                    rangesString = "{" + childNode.Attributes[0].Value + "};";
            }

            XmlElement codeBlockNode = MigrationManager.CreateCodeBlockNodeModelNode(
                data.Document, oldNode, 0, rangesString);
            migrationData.AppendNode(codeBlockNode);
            string codeBlockNodeId = MigrationManager.GetGuidFromXmlElement(codeBlockNode);

            // Update connectors
            for (int idx = 0; true; idx++)
            {
                PortId oldInPort = new PortId(newNodeId, idx + 2, PortType.INPUT);
                PortId newInPort = new PortId(codeBlockNodeId, idx, PortType.INPUT);
                XmlElement connector = data.FindFirstConnector(oldInPort);

                if (connector == null)
                    break;

                data.ReconnectToPort(connector, newInPort);
            }

            PortId oldInPort1 = new PortId(newNodeId, 1, PortType.INPUT);
            PortId newInPort2 = new PortId(newNodeId, 2, PortType.INPUT);
            XmlElement connector1 = data.FindFirstConnector(oldInPort1);

            data.ReconnectToPort(connector1, newInPort2);
            data.CreateConnector(codeBlockNode, 0, newNode, 1);

            return migrationData;
        }
    }

    #region Functions

    [NodeName("Compose Functions")]
    [NodeCategory(BuiltinNodeCategories.CORE_FUNCTIONS)]
    [NodeDescription("Composes two single parameter functions into one function.")]
    public class ComposeFunctions : NodeModel
    { 
        public ComposeFunctions(WorkspaceModel ws) : base(ws)
        {
            InPortData.Add(new PortData("f", "A Function"));
            InPortData.Add(new PortData("g", "A Function"));
            OutPortData.Add(new PortData("g ∘ f", "Composed function: g(f(x))"));

            RegisterAllPorts();
        }

        //public override Value Evaluate(FSharpList<Value> args)
        //{
        //    var f = ((Value.Function)args[0]).Item;
        //    var g = ((Value.Function)args[1]).Item;

        //    return Value.NewFunction(Utils.ConvertToFSchemeFunc(x => g.Invoke(Utils.MakeFSharpList(f.Invoke(x)))));
        //}

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);

            XmlElement composeNode = MigrationManager.CloneAndChangeName(oldNode, 
                "DSCoreNodesUI.HigherOrder.ComposeFunctions","Compose Function");
            composeNode.SetAttribute("inputcount", "2");
            migratedData.AppendNode(composeNode);

            return migratedData;
        }
    }

    #endregion

    //TODO: Setup proper IsDirty smart execution management
    [NodeName("Apply Function")]
    [NodeCategory(BuiltinNodeCategories.CORE_EVALUATE)]
    [NodeDescription("Applies a function to arguments.")]
    public class Apply1 : VariableInput
    {
        public Apply1(WorkspaceModel ws) : base(ws)
        { 
            InPortData.Add(new PortData("func", "Function"));
            OutPortData.Add(new PortData("result", "Result of function application."));

            RegisterAllPorts();
        }

        protected override string GetInputRootName()
        {
            return "arg";
        }

        protected override string GetTooltipRootName()
        {
            return "Argument #";
        }

        //public override Value Evaluate(FSharpList<Value> args)
        //{
        //    var f = ((Value.Function)args[0]).Item;
        //    var fArgs = args.Tail;

        //    return f.Invoke(fArgs);
        //}

        protected internal override void RemoveInput()
        {
            if (InPortData.Count > 1)
                base.RemoveInput();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            foreach (var inport in InPortData.Skip(1))
            {
                XmlElement input = xmlDoc.CreateElement("Input");

                input.SetAttribute("name", inport.NickName);

                nodeElement.AppendChild(input);
            }
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name == "Input")
                {
                    var attr = subNode.Attributes["name"].Value;

                    if (!attr.Equals("func"))
                        InPortData.Add(new PortData(subNode.Attributes["name"].Value, ""));
                }
            }
            RegisterAllPorts();
        }

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migratedData = new NodeMigrationData(data.Document);
            XmlElement oldNode = data.MigratedNodes.ElementAt(0);
            string oldNodeId = MigrationManager.GetGuidFromXmlElement(oldNode);

            XmlElement applyNode = MigrationManager.CloneAndChangeName(oldNode,
                "DSCoreNodesUI.HigherOrder.ApplyFunction", "Apply Function");

            int numberOfArgs = oldNode.ChildNodes.Count + 1;
            string numberOfArgsString = numberOfArgs.ToString();
            applyNode.SetAttribute("inputcount", numberOfArgsString);
            migratedData.AppendNode(applyNode);

            return migratedData;
        }
    }
    
    public abstract partial class BasicInteractive<T> : NodeModel
    {
        private T _value;
        public virtual T Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value == null || !_value.Equals(value))
                {
                    _value = value;
                    //dynamoModel.Logger.Log("Value changed to: " + _value);
                    RequiresRecalc = value != null;
                    RaisePropertyChanged("Value");
                }
            }
        }

        protected abstract T DeserializeValue(string val);
        protected abstract string SerializeValue(T val);

        protected BasicInteractive(WorkspaceModel ws)
            : base(ws)
        {
            Type type = typeof(T);
            OutPortData.Add(new PortData("", type.Name));
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(T).FullName);
            outEl.SetAttribute("value", Value.ToString());
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(T).FullName))
                {
                    Value = DeserializeValue(subNode.Attributes[0].Value);
                }
            }
        }

        public override string PrintExpression()
        {
            return Value.ToString();
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                var document = element.OwnerDocument;
                XmlElement childElement = document.CreateElement(typeof(T).FullName);
                childElement.SetAttribute("value", SerializeValue(this.Value));
                element.AppendChild(childElement);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                foreach (XmlNode childNode in element.ChildNodes)
                {
                    if (childNode.Name.Equals(typeof(T).FullName) == false)
                        continue;

                    this.Value = DeserializeValue(childNode.Attributes["value"].Value);
                    break;
                }
            }
        }

        #endregion
    }

    public abstract class Double : BasicInteractive<double>
    {
        public override bool IsConvertible
        {
            get { return true; }
        }

        public Double(WorkspaceModel ws) : base(ws) { }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildDoubleNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }
    }

    public abstract class Integer : BasicInteractive<int>
    {
        public Integer(WorkspaceModel ws) : base(ws) { }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(int).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }
    }

    public abstract class Bool : BasicInteractive<bool>
    {
        public Bool(WorkspaceModel ws) : base(ws) { }
    }

    public abstract partial class AbstractString : BasicInteractive<string>
    {
        public AbstractString(WorkspaceModel ws) : base(ws) { }

        public override string PrintExpression()
        {
            return "\"" + base.PrintExpression() + "\"";
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("stringValue", Value);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called
            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                Value = helper.ReadString("stringValue");
            }
        }

        #endregion
    }

    [NodeName("String")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Creates a string.")]
    [IsDesignScriptCompatible]
    public partial class StringInput : AbstractString
    {
        public override string Value
        {
            get
            {
                return HttpUtility.HtmlDecode(base.Value);
            }
            set
            {
                base.Value = value;
            }
        }

        public StringInput(WorkspaceModel ws): base(ws)
        {
            RegisterAllPorts();
            Value = "";
        }

        protected override string SerializeValue(string val)
        {
            return val;
        }

        protected override string DeserializeValue(string val)
        {
            return val;
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            XmlElement outEl = xmlDoc.CreateElement(typeof(string).FullName);
            outEl.SetAttribute("value", Value.ToString(CultureInfo.InvariantCulture));
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name.Equals(typeof(string).FullName))
                {
                    foreach (XmlAttribute attr in subNode.Attributes)
                    {
                        if (attr.Name.Equals("value"))
                        {
                            Value = DeserializeValue(attr.Value);
                        }
                    }
                }
            }
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildStringNode(this.Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }

        [NodeMigration(from: "0.5.3.0", to: "0.6.3.0")]
        public static NodeMigrationData Migrate_0530_to_0600(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);

            XmlNode nodeElement = data.MigratedNodes.ElementAt(0);
            XmlNode newNode = nodeElement.CloneNode(true);

            var query = from XmlNode subNode in newNode.ChildNodes
                        where subNode.Name.Equals(typeof(string).FullName)
                        from XmlAttribute attr in subNode.Attributes
                        where attr.Name.Equals("value")
                        select attr;

            foreach (XmlAttribute attr in query)
                attr.Value = HttpUtility.UrlDecode(attr.Value);

            migrationData.AppendNode(newNode as XmlElement);
            return migrationData;
        }
    }

    public delegate double ConversionDelegate(double value);

    [NodeName("Number")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("Creates a number.")]
    [IsDesignScriptCompatible]
    public partial class DoubleInput : NodeModel
    {
        public DoubleInput(WorkspaceModel ws)
            : base(ws)
        {
            OutPortData.Add(new PortData("", ""));
            RegisterAllPorts();

            _convertToken = Convert;
            Value = "0";
        }

        public virtual double Convert(double value)
        {
            return value;
        }

        private List<IDoubleSequence> _parsed;
        private string _value;
        protected ConversionDelegate _convertToken;

        public string Value
        {
            get { return _value; }
            set
            {
                if (_value != null && _value.Equals(value))
                    return;

                _value = value;

                var idList = new List<string>();

                try
                {
                    _parsed = ParseValue(value, new[] { '\n' }, idList, _convertToken);

                    InPortData.Clear();

                    foreach (var id in idList)
                    {
                        InPortData.Add(new PortData(id, "variable"));
                    }

                    RegisterInputPorts();
                    ClearRuntimeError();

                    ArgumentLacing = InPortData.Any() ? LacingStrategy.Longest : LacingStrategy.Disabled;
                }
                catch (Exception e)
                {
                    Error(e.Message);
                }

                RequiresRecalc = value != null;
                RaisePropertyChanged("Value");
            }
        }

        public override bool IsConvertible
        {
            get { return true; }
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement(typeof(double).FullName);
            outEl.SetAttribute("value", Value);
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes.Cast<XmlNode>().Where(subNode => subNode.Name.Equals(typeof(double).FullName)))
            {
                Value = subNode.Attributes[0].Value;
            }
        }

        #region Serialization/Deserialization Methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called

            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                helper.SetAttribute("doubleInputValue", Value);
            }
        }

        protected override void DeserializeCore(XmlElement element, SaveContext context)
        {
            base.DeserializeCore(element, context); //Base implementation must be called

            if (context == SaveContext.Undo)
            {
                XmlElementHelper helper = new XmlElementHelper(element);
                this.Value = helper.ReadString("doubleInputValue");
            }
        }

        #endregion

        /* disable the migration path from number node to CBN.

        [NodeMigration(from: "0.6.3.0", to: "0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            NodeMigrationData migrationData = new NodeMigrationData(data.Document);
            XmlElement original = data.MigratedNodes.ElementAt(0);

            // Escape special characters for display in code block node.
            string content = ExtensionMethods.GetChildNodeDoubleValue(original);

            bool isValidContent = false;

            try
            {
                var identifiers = new List<string>();
                var doubleSequences = DoubleInput.ParseValue(content,
                    new[] { '\n' }, identifiers, (x) => { return x; });

                if (doubleSequences != null && (doubleSequences.Count == 1))
                {
                    IDoubleSequence sequence = doubleSequences[0];
                    if (sequence is DoubleInput.Range) // A range expression.
                        isValidContent = true;
                    else if (sequence is DoubleInput.Sequence) // A sequence.
                        isValidContent = true;
                    else if (sequence is DoubleInput.OneNumber) // A number.
                        isValidContent = true;
                }
            }
            catch (Exception)
            {
            }

            if (isValidContent == false)
            {
                // TODO(Ben): Convert into a dummy node here?
            }
            else
            {
                XmlElement newNode = MigrationManager.CreateCodeBlockNodeFrom(original);
                newNode.SetAttribute("CodeText", content);
                migrationData.AppendNode(newNode);
            }

            return migrationData;
        }

        */
        
        public static List<IDoubleSequence> ParseValue(string text, char[] seps, List<string> identifiers, ConversionDelegate convertToken)
        {
            var idSet = new HashSet<string>(identifiers);
            return text.Replace(" ", "").Split(seps, StringSplitOptions.RemoveEmptyEntries).Select(
                delegate(string x)
                {
                    var rangeIdentifiers = x.Split(
                        Sublists.RangeSeparatorTokens,
                        StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

                    if (rangeIdentifiers.Length > 3)
                        throw new Exception("Bad range syntax: not of format \"start..end[..(increment|#count)]\"");

                    if (rangeIdentifiers.Length == 0)
                        throw new Exception("No identifiers found.");

                    IDoubleInputToken startToken = ParseToken(rangeIdentifiers[0], idSet, identifiers);

                    if (rangeIdentifiers.Length > 1)
                    {
                        if (rangeIdentifiers[1].StartsWith("#"))
                        {
                            var countToken = rangeIdentifiers[1].Substring(1);
                            IDoubleInputToken endToken = ParseToken(countToken, idSet, identifiers);

                            if (rangeIdentifiers.Length > 2)
                            {
                                if (rangeIdentifiers[2].StartsWith("#") || rangeIdentifiers[2].StartsWith("~"))
                                    throw new Exception("Cannot use range or approx. identifier on increment field when one has already been used to specify a count.");
                                return new Sequence(startToken, ParseToken(rangeIdentifiers[2], idSet, identifiers), endToken, convertToken);
                            }

                            return new Sequence(startToken, new DoubleToken(1), endToken, convertToken) as IDoubleSequence;
                        }
                        else
                        {
                            IDoubleInputToken endToken = ParseToken(rangeIdentifiers[1], idSet, identifiers);

                            if (rangeIdentifiers.Length > 2)
                            {
                                if (rangeIdentifiers[2].StartsWith("#"))
                                {
                                    var count = rangeIdentifiers[2].Substring(1);
                                    IDoubleInputToken countToken = ParseToken(count, idSet, identifiers);

                                    return new CountRange(startToken, countToken, endToken, convertToken);
                                }

                                if (rangeIdentifiers[2].StartsWith("~"))
                                {
                                    var approx = rangeIdentifiers[2].Substring(1);
                                    IDoubleInputToken approxToken = ParseToken(approx, idSet, identifiers);

                                    return new ApproxRange(startToken, approxToken, endToken, convertToken);
                                }

                                return new Range(startToken, ParseToken(rangeIdentifiers[2], idSet, identifiers), endToken, convertToken);
                            }

                            double identifierValue0, identifierValue1;
                            var canBeParsed0 = System.Double.TryParse(rangeIdentifiers[0], out identifierValue0);
                            var canBeParsed1 = System.Double.TryParse(rangeIdentifiers[1], out identifierValue1);

                            //both of the value can be parsed as double
                            if (canBeParsed0 && canBeParsed1)
                            {
                                if (identifierValue0 < identifierValue1) 
                                    return new Range(startToken, new DoubleToken(1), endToken, convertToken) as IDoubleSequence;
                                else
                                    return new Range(startToken, new DoubleToken(-1), endToken, convertToken) as IDoubleSequence;                              
                            }

                            //the input cannot be parsed as double, return a default function and let it handle the error
                            return new Range(startToken, new DoubleToken(1), endToken, convertToken) as IDoubleSequence;
                        }

                    }

                    return new OneNumber(startToken, convertToken) as IDoubleSequence;
                }).ToList();
        }

        private static IDoubleInputToken ParseToken(string id, HashSet<string> identifiers, List<string> list)
        {
            double dbl;
            if (double.TryParse(id, NumberStyles.Any, CultureInfo.InvariantCulture, out dbl))
                return new DoubleToken(dbl);

            var match = Sublists.IdentifierPattern.Match(id);
            if (match.Success)
            {
                var tokenId = match.Groups["id"].Value;
                if (!identifiers.Contains(tokenId))
                {
                    identifiers.Add(tokenId);
                    list.Add(tokenId);
                }
                return new IdentifierToken(tokenId);
            }

            throw new Exception("Bad identifier syntax: \"" + id + "\"");
        }

        //public override Value Evaluate(FSharpList<Value> args)
        //{
        //    var paramDict = InPortData.Select(x => x.NickName)
        //        .Zip(args, Tuple.Create)
        //        .ToDictionary(x => x.Item1, x => ((Value.Number)x.Item2).Item);

        //    return _parsed.Count == 1
        //        ? _parsed[0].GetFSchemeValue(paramDict)
        //        : FScheme.Value.NewList(_parsed.Select(x => x.GetFSchemeValue(paramDict)).ToFSharpList());
        //}

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            var paramDict = InPortData.Select(x => x.NickName)
                   .Zip(inputAstNodes, Tuple.Create)
                   .ToDictionary(x => x.Item1, x => x.Item2);

            AssociativeNode rhs;

            if (null == _parsed)
            {
                rhs = AstFactory.BuildNullNode();
            }
            else
            {
                List<AssociativeNode> newInputs = _parsed.Count == 1
                    ? new List<AssociativeNode> { _parsed[0].GetAstNode(paramDict) }
                    : _parsed.Select(x => x.GetAstNode(paramDict)).ToList();

                rhs = newInputs.Count == 1
                        ? newInputs[0]
                        : AstFactory.BuildExprList(newInputs);
            }

            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }

        public interface IDoubleSequence
        {
            object GetFSchemeValue(Dictionary<string, double> idLookup);
            IEnumerable<double> GetValue(Dictionary<string, double> idLookup);
            AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup);
        }

        private class OneNumber : IDoubleSequence
        {
            private readonly IDoubleInputToken _token;
            private readonly double? _result;
            private readonly ConversionDelegate _convert;

            public OneNumber(IDoubleInputToken t, ConversionDelegate convertToken)
            {
                _token = t;
                _convert = convertToken;

                if (_token is DoubleToken)
                    _result = _convert(GetValue(null).First());
            }

            public object GetFSchemeValue(Dictionary<string, double> idLookup)
            {
                return GetValue(idLookup).First();
            }

            public IEnumerable<double> GetValue(Dictionary<string, double> idLookup)
            {
                yield return _result ?? _token.GetValue(idLookup);
            }

            public AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup)
            {
                if (_result == null)
                {
                    return _token.GetAstNode(idLookup);
                }
                else
                {
                    return _result.HasValue
                        ? (new DoubleToken(_result.Value)).GetAstNode(idLookup)
                        : new NullNode() as AssociativeNode;
                }
            }
        }

        private class Sequence : IDoubleSequence
        {
            private readonly IDoubleInputToken _start;
            private readonly IDoubleInputToken _step;
            private readonly IDoubleInputToken _count;
            private readonly ConversionDelegate _convert;

            private readonly IEnumerable<double> _result;

            public Sequence(IDoubleInputToken start, IDoubleInputToken step, IDoubleInputToken count, ConversionDelegate convertToken)
            {
                _start = start;
                _step = step;
                _count = count;
                _convert = convertToken;

                if (_start is DoubleToken && _step is DoubleToken && _count is DoubleToken)
                {
                    _result = GetValue(null);
                }
            }

            public object GetFSchemeValue(Dictionary<string, double> idLookup)
            {
                return GetValue(idLookup);
            }

            public IEnumerable<double> GetValue(Dictionary<string, double> idLookup)
            {
                if (_result == null)
                {
                    var step = _step.GetValue(idLookup);

                    if (step == 0)
                        throw new Exception("Can't have 0 step.");

                    var start = _start.GetValue(idLookup);
                    var count = (int)_count.GetValue(idLookup);

                    if (count < 0)
                    {
                        count *= -1;
                        start += step * (count - 1);
                        step *= -1;
                    }

                    return CreateSequence(start, step, count);
                }
                return _result;
            }

            private static IEnumerable<double> CreateSequence(double start, double step, int count)
            {
                for (var i = 0; i < count; i++)
                {
                    yield return start;
                    start += step;
                }
            }

            public AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup)
            {
                var rangeExpr = new RangeExprNode
                {
                    FromNode = _start.GetAstNode(idLookup),
                    ToNode = _count.GetAstNode(idLookup),
                    StepNode = _step.GetAstNode(idLookup),
                    HasRangeAmountOperator = true,
                    stepoperator = ProtoCore.DSASM.RangeStepOperator.stepsize
                };
                return rangeExpr;
            }
        }

        private class Range : IDoubleSequence
        {
            private readonly IDoubleInputToken _start;
            private readonly IDoubleInputToken _step;
            private readonly IDoubleInputToken _end;
            private readonly ConversionDelegate _convert;

            private readonly IEnumerable<double> _result;

            public Range(IDoubleInputToken start, IDoubleInputToken step, IDoubleInputToken end, ConversionDelegate convertToken)
            {
                _start = start;
                _step = step;
                _end = end;
                _convert = convertToken;

                if (_start is DoubleToken && _step is DoubleToken && _end is DoubleToken)
                {
                    _result = GetValue(null);
                }
            }

            public object GetFSchemeValue(Dictionary<string, double> idLookup)
            {
                return GetValue(idLookup);
            }

            public IEnumerable<double> GetValue(Dictionary<string, double> idLookup)
            {
                if (_result == null)
                {
                    var step = _convert(_step.GetValue(idLookup));

                    if (step == 0)
                        throw new Exception("Can't have 0 step.");

                    var start = _convert(_start.GetValue(idLookup));
                    var end = _convert(_end.GetValue(idLookup));

                    return Process(start, step, end);
                }
                return _result;
            }

            private IEnumerable<double> _Range(double start, double step, double stop)
            {
                var current = start;
                while (current <= stop)
                {
                    yield return current;
                    current += step;
                }
            }

            protected virtual IEnumerable<double> Process(double start, double step, double end)
            {
                if (step < 0)
                {
                    step *= -1;
                    var tmp = end;
                    end = start;
                    start = tmp;
                }

                var countingUp = start < end;

                return countingUp
                    ? _Range(start, step, end)
                    : _Range(end, step, start).Reverse();
            }

            protected virtual ProtoCore.DSASM.RangeStepOperator GetRangeExpressionOperator()
            {
                return ProtoCore.DSASM.RangeStepOperator.stepsize;
            }

            public AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup)
            {
                var rangeExpr = new RangeExprNode
                {
                    FromNode = _start.GetAstNode(idLookup),
                    ToNode = _end.GetAstNode(idLookup),
                    StepNode = _step.GetAstNode(idLookup),
                    stepoperator = GetRangeExpressionOperator()
                };
                return rangeExpr;
            }
        }

        private class CountRange : Range
        {
            public CountRange(IDoubleInputToken startToken, IDoubleInputToken countToken, IDoubleInputToken endToken, ConversionDelegate convertToken)
                : base(startToken, countToken, endToken, convertToken)
            { }

            protected override IEnumerable<double> Process(double start, double count, double end)
            {
                var c = (int)count;

                var neg = c < 0;

                c = Math.Abs(c) - 1;

                if (neg)
                    c *= -1;

                return base.Process(start, Math.Abs(start - end) / c, end);
            }

            protected override ProtoCore.DSASM.RangeStepOperator GetRangeExpressionOperator()
            {
                return ProtoCore.DSASM.RangeStepOperator.num;
            }
        }

        private class ApproxRange : Range
        {
            public ApproxRange(IDoubleInputToken start, IDoubleInputToken step, IDoubleInputToken end, ConversionDelegate convertToken)
                : base(start, step, end, convertToken)
            { }

            protected override IEnumerable<double> Process(double start, double approx, double end)
            {
                var neg = approx < 0;

                var a = Math.Abs(approx);

                var dist = end - start;
                var stepnum = 1;
                if (dist != 0)
                {
                    var ceil = (int)Math.Ceiling(dist / a);
                    var floor = (int)Math.Floor(dist / a);

                    if (ceil != 0 && floor != 0)
                    {
                        var ceilApprox = Math.Abs(dist / ceil - a);
                        var floorApprox = Math.Abs(dist / floor - a);
                        stepnum = ceilApprox < floorApprox ? ceil : floor;
                    }
                }

                if (neg)
                    stepnum *= -1;

                return base.Process(start, Math.Abs(dist) / stepnum, end);
            }

            protected override ProtoCore.DSASM.RangeStepOperator GetRangeExpressionOperator()
            {
                return ProtoCore.DSASM.RangeStepOperator.approxsize;
            }
        }

        interface IDoubleInputToken
        {
            double GetValue(Dictionary<string, double> idLookup);
            AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup);
        }

        private struct IdentifierToken : IDoubleInputToken
        {
            private readonly string _id;

            public IdentifierToken(string id)
            {
                _id = id;
            }

            public double GetValue(Dictionary<string, double> idLookup)
            {
                return idLookup[_id];
            }

            public AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup)
            {
                return idLookup[_id];
            }
        }

        private struct DoubleToken : IDoubleInputToken
        {
            private readonly double _d;

            public DoubleToken(double d)
            {
                _d = d;
            }

            public double GetValue(Dictionary<string, double> idLookup)
            {
                return _d;
            }

            public AssociativeNode GetAstNode(Dictionary<string, AssociativeNode> idLookup)
            {
                if (Math.Floor(_d) == _d)
                    return AstFactory.BuildIntNode((int)_d);
                return AstFactory.BuildDoubleNode(_d);
            }
        }
    }

    /// <summary>
    /// A class used to store a name and associated item for a drop down menu
    /// </summary>
    public class DynamoDropDownItem:IComparable
    {
        public string Name { get; set; }
        public object Item { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public DynamoDropDownItem(string name, object item)
        {
            Name = name;
            Item = item;
        }

        public int CompareTo(object obj)
        {
            var a = obj as DynamoDropDownItem;
            if (a == null)
                return 1;

            return this.Name.CompareTo(a);
        }

    }

    /// <summary>
    /// Base class for all nodes using a drop down
    /// </summary>
    public abstract partial class DropDrownBase : NodeModel
    {
        protected ObservableCollection<DynamoDropDownItem> items = new ObservableCollection<DynamoDropDownItem>();
        public ObservableCollection<DynamoDropDownItem> Items
        {
            get { return items; }
            set
            {
                items = value;
                RaisePropertyChanged("Items");
            }
        }

        private int selectedIndex = 0;
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                //do not allow selected index to
                //go out of range of the items collection
                if (value > Items.Count - 1)
                {
                    selectedIndex = -1;
                }
                else
                    selectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
            }
        }

        protected DropDrownBase(WorkspaceModel ws) : base(ws)
        {
            Items.CollectionChanged += Items_CollectionChanged;
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            nodeElement.SetAttribute("index", SelectedIndex.ToString());
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            try
            {
                SelectedIndex = Convert.ToInt32(nodeElement.Attributes["index"].Value);
            }
            catch { }
        }

        public abstract void PopulateItems();

        /// <summary>
        /// When the dropdown is opened, the node's implementation of PopulateItemsHash is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void combo_DropDownOpened(object sender, EventArgs e)
        {
            PopulateItems();
        }

        /// <summary>
        /// Executed when the items collection has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //SortItems();
        }

        /// <summary>
        /// The base behavior for the drop down node is to return the item at the selected index in the Items collection.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        //public override Value Evaluate(FSharpList<Value> args)
        //{
        //    return Value.NewContainer(Items[SelectedIndex].Item);
        //}
    }
}
