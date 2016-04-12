using System;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// Contains sufficient data to create a <see cref="WorkspaceModel"/> object
    /// </summary>
    public class WorkspaceInfo
    {
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
                if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(funName) && funName != "Home")
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
        /// Version of Dynamo where the workspace was created
        /// </summary>
        public string Version { get; internal set; }

        /// <summary>
        /// Description of the workspace
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        /// Full category name of custom node
        /// </summary>
        public string Category { get; internal set; }

        /// <summary>
        /// X coordinate of top left corner of visible workspace part
        /// </summary>
        public double X { get; internal set; }

        /// <summary>
        /// Y coordinate of top left corner of visible workspace part
        /// </summary>
        public double Y { get; internal set; }

        /// <summary>
        /// Zoom value of the workspace
        /// </summary>
        public double Zoom { get; internal set; }

        /// <summary>
        /// Name of the workspace
        /// </summary>
        public string Name { get; internal set; }
        
        /// <summary>
        /// <see cref="System.Guid"/> identifier string value of custom node workspace 
        /// </summary>
        public string ID { get; internal set; }

        /// <summary>
        /// File name of the workspace
        /// </summary>
        public string FileName { get; internal set; }

        /// <summary>
        /// Run type of the home workspace
        /// </summary>
        public RunType RunType { get; internal set; }

        /// <summary>
        /// Run period value of the home workspace if RunType is Periodic
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
