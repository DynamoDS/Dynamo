using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// Contains sufficient data to create a <see cref="WorkspaceModel"/> object
    /// </summary>
    public class WorkspaceInfo
    {
        //in dynamo 1.x the workspace name for home workspaces was always "Home" no matter what the language was.
        private const string dynamo1HomeWorkspaceNameString = "Home";
        public WorkspaceInfo(string id, string name, string description, RunType runType)
        {
            ID = id;
            Name = name;
            Description = description;
            Zoom = 1.0;
            X = 0;
            Y = 0;
            RunType = runType;
            RunPeriod = RunSettings.DefaultRunPeriod;
            HasRunWithoutCrash = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkspaceInfo"/> class
        /// with default workspace data.
        /// </summary>
        public WorkspaceInfo()
        {
            Zoom = 1.0;
            X = 0; 
            Y = 0;
            RunType = RunType.Automatic;
            RunPeriod = RunSettings.DefaultRunPeriod;
            HasRunWithoutCrash = true;
        }

        internal static bool FromXmlDocument(XmlDocument xmlDoc, string path, bool isTestMode, 
            bool forceManualExecutionMode, ILogger logger, out WorkspaceInfo workspaceInfo)
        {
            try
            {
                string funName = null;
                double cx = 0;
                double cy = 0;
                double zoom = 1.0;
                double scaleFactor = 1.0;
                string id = "";
                string category = "";
                string description = "";
                string version = "";
                var runType = RunType.Manual;
                int runPeriod = RunSettings.DefaultRunPeriod;
                bool hasRunWithoutCrash = false;
                bool isVisibleInDynamoLibrary = true;

                var topNode = xmlDoc.GetElementsByTagName("Workspace");

                // legacy support
                if (topNode.Count == 0)
                {
                    topNode = xmlDoc.GetElementsByTagName("dynWorkspace");
                }

                // load the header
                foreach (XmlNode node in topNode)
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.Equals("X"))
                            cx = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        else if (att.Name.Equals("Y"))
                            cy = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        else if (att.Name.Equals("zoom"))
                            zoom = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        else if (att.Name.Equals("ScaleFactor"))
                            scaleFactor = double.Parse(att.Value, CultureInfo.InvariantCulture);
                        else if (att.Name.Equals("Name"))
                            funName = att.Value;
                        else if (att.Name.Equals("ID"))
                            id = att.Value;
                        else if (att.Name.Equals("Category"))
                            category = att.Value;
                        else if (att.Name.Equals("Description"))
                            description = att.Value;
                        else if (att.Name.Equals("HasRunWithoutCrash"))
                            hasRunWithoutCrash = bool.Parse(att.Value);
                        else if (att.Name.Equals("IsVisibleInDynamoLibrary"))
                            isVisibleInDynamoLibrary = bool.Parse(att.Value);
                        else if (att.Name.Equals("Version"))
                            version = att.Value;
                        else if (att.Name.Equals("RunType"))
                        {
                            if (forceManualExecutionMode || !Enum.TryParse(att.Value, false, out runType))
                            {
                                runType = RunType.Manual;
                            }
                        }
                        else if (att.Name.Equals("RunPeriod"))
                            runPeriod = Int32.Parse(att.Value);
                    }
                }

                // we have a dyf and it lacks an ID field, we need to assign it
                // a deterministic guid based on its name.  By doing it deterministically,
                // files remain compatible
                if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(funName) && funName != dynamo1HomeWorkspaceNameString)
                {
                    id = GuidUtility.Create(GuidUtility.UrlNamespace, funName).ToString();
                }

                workspaceInfo = new WorkspaceInfo
                {
                    ID = id,
                    Name = funName,
                    X = cx,
                    Y = cy,
                    Zoom = zoom,
                    ScaleFactor = scaleFactor,
                    FileName = path,
                    Category = category,
                    Description = description,
                    Version = version,
                    RunType  = runType,
                    RunPeriod = runPeriod,
                    HasRunWithoutCrash = hasRunWithoutCrash,
                    IsVisibleInDynamoLibrary = isVisibleInDynamoLibrary
                };
                return true;
            }
            catch (Exception ex)
            {
                logger.Log(Properties.Resources.OpenWorkbenchError);
                logger.Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);

                //TODO(Steve): Need a better way to handle this kind of thing. -- MAGN-5712
                if (isTestMode)
                    throw; // Rethrow for NUnit.

                workspaceInfo = null;
                return false;
            }
        }

        /// <summary>
        /// Return a boolean indicating if successfully deserialized workspace info object from Json file
        /// </summary>
        /// <param name="jsonDoc">Target Josn</param>
        /// <param name="path">Target path</param>
        /// <param name="isTestMode">Boolean indicating if Dynamo is running in Test Mode</param>
        /// <param name="forceManualExecutionMode">Boolean indicating if forcing manual mode</param>
        /// <param name="logger">Dynamo logger</param>
        /// <param name="workspaceInfo">Return object</param>
        /// <returns>A boolean indicating success</returns>
        internal static bool FromJsonDocument(String jsonDoc, string path, bool isTestMode,
            bool forceManualExecutionMode, ILogger logger, out WorkspaceInfo workspaceInfo)
        {
            var jObject = (JObject)JsonConvert.DeserializeObject(jsonDoc);
            try
            {
                double cx = 0;
                double cy = 0;
                double zoom = 1.0;
                double scaleFactor = 1.0;
                string version = "";
                var runType = RunType.Manual;
                int runPeriod = RunSettings.DefaultRunPeriod;
                bool hasRunWithoutCrash = false;
                bool isVisibleInDynamoLibrary = true;

                JToken value;
                string funName = jObject.TryGetValue("Name", out value)? value.ToString(): "";
                string id = jObject.TryGetValue("Uuid", out value) ? value.ToString() : "";
                string category = jObject.TryGetValue("Category", out value) ? value.ToString() : "";
                string description = jObject.TryGetValue("Description", out value) ? value.ToString() : "";
                // we have a dyf and it lacks an ID field, we need to assign it
                // a deterministic guid based on its name.  By doing it deterministically,
                // files remain compatible
                //TODO(mjk) we should get rid of this and throw instead since non hame names are now valid in json format.
                if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(funName) && funName != dynamo1HomeWorkspaceNameString)
                {
                    id = GuidUtility.Create(GuidUtility.UrlNamespace, funName).ToString();
                }

                // Parse the following info when graph contains a "View" block
                if (jObject.TryGetValue("View", out value))
                {
                    JObject viewObject = value.ToObject<JObject>();
                    Double.TryParse((viewObject.TryGetValue("X", out value) ? value.ToString(): "0"), out cx);
                    Double.TryParse((viewObject.TryGetValue("Y", out value) ? value.ToString() : "0"), out cy);
                    Double.TryParse((viewObject.TryGetValue("Zoom", out value) ? value.ToString() : "1.0"), out zoom);

                    // Parse the following info when "View" block contains a "Dynamo" block
                    if (viewObject.TryGetValue("Dynamo", out value))
                    {
                        JObject dynamoObject = value.ToObject<JObject>();
                        Double.TryParse((dynamoObject.TryGetValue("ScaleFactor", out value) ? value.ToString(): "1.0"), out scaleFactor);
                        Boolean.TryParse((dynamoObject.TryGetValue("HasRunWithoutCrash", out value) ? value.ToString(): "false"), out hasRunWithoutCrash);
                        Boolean.TryParse((dynamoObject.TryGetValue("IsVisibleInDynamoLibrary", out value) ? value.ToString(): "true"), out isVisibleInDynamoLibrary);
                        version = dynamoObject.TryGetValue("Version", out value)? value.ToString() : "";
                        if (forceManualExecutionMode || !Enum.TryParse((dynamoObject.TryGetValue("RunType", out value)? value.ToString(): "false"), false, out runType))
                        {
                            runType = RunType.Manual;
                        }
                        Int32.TryParse((dynamoObject.TryGetValue("RunPeriod", out value)? value.ToString() : RunSettings.DefaultRunPeriod.ToString()), out runPeriod);
                    }
                }

                workspaceInfo = new WorkspaceInfo
                {
                    ID = id,
                    Name = funName,
                    X = cx,
                    Y = cy,
                    Zoom = zoom,
                    ScaleFactor = scaleFactor,
                    FileName = path,
                    Category = category,
                    Description = description,
                    Version = version,
                    RunType = runType,
                    RunPeriod = runPeriod,
                    HasRunWithoutCrash = hasRunWithoutCrash,
                    IsVisibleInDynamoLibrary = isVisibleInDynamoLibrary
                };
                return true;
            }
            catch (Exception ex)
            {
                logger.Log(Properties.Resources.OpenWorkbenchError);
                logger.Log(ex);
                Debug.WriteLine(ex.Message + ":" + ex.StackTrace);

                if (isTestMode)
                    throw; // Rethrow for NUnit.

                workspaceInfo = null;
                return false;
            }
        }

        /// <summary>
        /// Returns version of Dynamo where the workspace was created
        /// </summary>
        public string Version { get; internal set; }

        /// <summary>
        /// Returns description of the workspace
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Returns full category name of custom node
        /// </summary>
        public string Category { get; internal set; }

        /// <summary>
        /// Returns X coordinate of top left corner of visible workspace part
        /// </summary>
        public double X { get; internal set; }

        /// <summary>
        /// Returns Y coordinate of top left corner of visible workspace part
        /// </summary>
        public double Y { get; internal set; }

        /// <summary>
        /// Returns zoom value of the workspace
        /// </summary>
        public double Zoom { get; internal set; }

        /// <summary>
        /// Returns the scale factor for ProtoGeometry geometries
        /// </summary>
        public double ScaleFactor { get; internal set; }

        /// <summary>
        /// Returns name of the workspace
        /// </summary>
        public string Name { get; internal set; }
        
        /// <summary>
        /// Returns <see cref="System.Guid"/> identifier string value of custom node workspace 
        /// </summary>
        public string ID { get; internal set; }

        /// <summary>
        /// Returns file name of the workspace
        /// </summary>
        public string FileName { get; internal set; }

        /// <summary>
        /// Returns run type of the home workspace
        /// </summary>
        public RunType RunType { get; internal set; }

        /// <summary>
        /// Returns run period value of the home workspace if RunType is Periodic
        /// </summary>
        public int RunPeriod { get; internal set; }

        /// <summary>
        /// Indicates if the workspace was executed successfully at last time 
        /// </summary>
        public bool HasRunWithoutCrash { get; internal set; }

        /// <summary>
        /// Indicates if the custom node is visible node library
        /// </summary>
        public bool IsVisibleInDynamoLibrary { get; internal set; }

        /// <summary>
        /// Indicates whether the workspace is custom node or home workspace
        /// </summary>
        public bool IsCustomNodeWorkspace
        {
            get { return !string.IsNullOrEmpty(ID); }
        }
    }
}
