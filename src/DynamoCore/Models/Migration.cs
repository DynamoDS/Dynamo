using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using Dynamo.Utilities;
using System.Collections.Generic;

namespace Dynamo.Models
{
    internal class Migration
    {
        /// <summary>
        /// A version after which this migration will be applied.
        /// </summary>
        public Version Version { get; set; }
        
        /// <summary>
        /// The action to perform during the upgrade.
        /// </summary>
        public Action Upgrade { get; set; }

        /// <summary>
        /// A migration which can be applied to a workspace to upgrade the workspace to the current version.
        /// </summary>
        /// <param name="v">A version number specified as x.x.x.x after which a workspace will be upgraded</param>
        /// <param name="upgrade">The action to perform during the upgrade.</param>
        public Migration(Version v, Action upgrade)
        {
            Version = v;
            Upgrade = upgrade;
        }
    }

    public class MigrationManager
    {
        private static MigrationManager _instance;

        /// <summary>
        /// The singleton instance property.
        /// </summary>
        public static MigrationManager Instance
        {
            get { return _instance ?? (_instance = new MigrationManager()); }
        }

        /// <summary>
        /// A collection of types which contain migration methods.
        /// </summary>
        public List<Type> MigrationTargets { get; set; }

        /// <summary>
        /// The private constructor.
        /// </summary>
        private MigrationManager()
        {
            MigrationTargets = new List<Type>();
        }

        /// <summary>
        /// Runs all migration methods found on the listed migration target types.
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <param name="version"></param>
        public void ProcessWorkspaceMigrations(XmlDocument xmlDoc, Version workspaceVersion)
        {
            var methods = MigrationTargets.SelectMany(x => x.GetMethods(BindingFlags.Public | BindingFlags.Static));

            var migrations =
                (from method in methods
                    let attribute =
                        method.GetCustomAttributes(false)
                            .OfType<WorkspaceMigrationAttribute>()
                            .FirstOrDefault()
                    where attribute != null
                    let result = new { method, attribute.From, attribute.To }
                    orderby result.From
                    select result).ToList();

            var currentVersion = dynSettings.Controller.DynamoModel.HomeSpace.WorkspaceVersion;

            while (workspaceVersion != null && workspaceVersion < currentVersion)
            {
                var nextMigration = migrations.FirstOrDefault(x => x.From >= workspaceVersion);

                if (nextMigration == null)
                    break;

                nextMigration.method.Invoke(null, new object[] { xmlDoc });
                workspaceVersion = nextMigration.To;
            }
        }

        public void ProcessNodesInWorkspace(XmlDocument xmlDoc, Version workspaceVersion)
        {
            XmlNodeList elNodes = xmlDoc.GetElementsByTagName("Elements");
            if (elNodes == null || (elNodes.Count == 0))
                elNodes = xmlDoc.GetElementsByTagName("dynElements");

            // A new list to store migrated nodes.
            List<XmlElement> migratedNodes = new List<XmlElement>();

            XmlNode elNodesList = elNodes[0];
            foreach (XmlNode elNode in elNodesList.ChildNodes)
            {
                string typeName = elNode.Attributes["type"].Value;
                typeName = Dynamo.Nodes.Utilities.PreprocessTypeName(typeName);
                System.Type type = Dynamo.Nodes.Utilities.ResolveType(typeName);

                // Migrate the given node into one or more new nodes.
                NodeMigrationData migrationData = this.MigrateXmlNode(elNode, type, workspaceVersion);
                migratedNodes.AddRange(migrationData.MigratedNodes);
            }

            // Replace the old child nodes with the migrated nodes. Note that 
            // "RemoveAll" also remove all attributes, but since we don't have 
            // any attribute here, it is safe. Added an assertion to make sure 
            // we revisit this codes if we do add attributes to 'elNodesList'.
            // 
            System.Diagnostics.Debug.Assert(elNodesList.Attributes.Count == 0);
            elNodesList.RemoveAll();

            foreach (XmlElement migratedNode in migratedNodes)
                elNodesList.AppendChild(migratedNode);
        }

        public NodeMigrationData MigrateXmlNode(XmlNode elNode, System.Type type, Version workspaceVersion)
        {
            var migrations = (from method in type.GetMethods()
                              let attribute =
                                  method.GetCustomAttributes(false).OfType<NodeMigrationAttribute>().FirstOrDefault()
                              where attribute != null
                              let result = new { method, attribute.From, attribute.To }
                              orderby result.From
                              select result).ToList();

            Version currentVersion = dynSettings.Controller.DynamoModel.HomeSpace.WorkspaceVersion;

            XmlElement nodeToMigrate = elNode as XmlElement;
            NodeMigrationData migrationData = new NodeMigrationData(elNode.OwnerDocument);
            migrationData.AppendNode(elNode as XmlElement);

            while (workspaceVersion != null && workspaceVersion < currentVersion)
            {
                var nextMigration = migrations.FirstOrDefault(x => x.From >= workspaceVersion);

                if (nextMigration == null)
                    break;

                object ret = nextMigration.method.Invoke(this, new object[] { migrationData });
                migrationData = ret as NodeMigrationData;
                workspaceVersion = nextMigration.To;
            }

            return migrationData;
        }

        /// <summary>
        /// Remove revision number from 'fileVersion' (so we get '0.6.3.0' 
        /// instead of '0.6.3.20048'). This way all migration methods with 
        /// 'NodeMigration.from' attribute value '0.6.3.xyz' can be used to 
        /// migrate nodes in workspace version '0.6.3.ijk' (i.e. the revision 
        /// number does not have to be exact match for a migration method to 
        /// work).
        /// </summary>
        /// <param name="version">The version string to convert into Version 
        /// object. Valid examples include "0.6.3" and "0.6.3.20048".</param>
        /// <returns>Returns the Version object representation of 'version' 
        /// argument, except without the 'revision number'.</returns>
        /// 
        internal static Version VersionFromString(string version)
        {
            Version ver = string.IsNullOrEmpty(version) ?
                new Version(0, 0, 0, 0) : new Version(version);

            // Ignore revision number.
            return new Version(ver.Major, ver.Minor, ver.Build);
        }

        /// <summary>
        /// Call this method to create an empty DSFunction node that contains 
        /// basic function node information.
        /// </summary>
        /// <param name="document">The XmlDocument to create the node in.</param>
        /// <param name="assembly">Name of the assembly that implements this 
        /// function.</param>
        /// <param name="nickname">The nickname to display on the node.</param>
        /// <param name="signature">The signature of the function.</param>
        /// <returns>Returns the XmlElement that represents a DSFunction node 
        /// with its basic function information with default attributes.</returns>
        /// 
        public static XmlElement CreateFunctionNode(XmlDocument document,
            string assembly, string nickname, string signature)
        {
            XmlElement element = document.CreateElement("Dynamo.Nodes.DSFunction");
            element.SetAttribute("type", "Dynamo.Nodes.DSFunction");
            element.SetAttribute("assembly", assembly);
            element.SetAttribute("nickname", nickname);
            element.SetAttribute("function", signature);

            // Attributes with default values (as in DynamoModel.OpenWorkspace).
            element.SetAttribute("isVisible", "true");
            element.SetAttribute("isUpstreamVisible", "true");
            element.SetAttribute("lacing", "Disabled");
            element.SetAttribute("x", "0.0");
            element.SetAttribute("y", "0.0");
            element.SetAttribute("guid", Guid.NewGuid().ToString());
            return element;
        }

        /// <summary>
        /// Call this method to create a XmlElement with a set of attributes 
        /// carried over from the source XmlElement. The new XmlElement will 
        /// have a name of "Dynamo.Nodes.DSFunction".
        /// </summary>
        /// <param name="srcElement">The source XmlElement object.</param>
        /// <param name="attribNames">The list of attribute names whose values 
        /// are to be carried over to the resulting XmlElement. This list is 
        /// mandatory and it cannot be empty. If a specified attribute cannot 
        /// be found in srcElement, an empty attribute with the same name will 
        /// be created in the resulting XmlElement.</param>
        /// <returns>Returns the resulting XmlElement with specified attributes
        /// duplicated from srcElement. The resulting XmlElement will also have
        /// a mandatory "type" attribute with value "Dynamo.Nodes.DSFunction".
        /// </returns>
        /// 
        public static XmlElement CreateFunctionNodeFrom(
            XmlElement srcElement, string[] attribNames)
        {
            if (srcElement == null)
                throw new ArgumentNullException("srcElement");
            if (attribNames == null || (attribNames.Length <= 0))
                throw new ArgumentException("Argument cannot be empty", "attribNames");

            XmlDocument document = srcElement.OwnerDocument;
            XmlElement dstElement = document.CreateElement("Dynamo.Nodes.DSFunction");

            foreach (string attribName in attribNames)
            {
                var value = srcElement.GetAttribute(attribName);
                dstElement.SetAttribute(attribName, value);
            }

            dstElement.SetAttribute("type", "Dynamo.Nodes.DSFunction");
            return dstElement;
        }

        /// <summary>
        /// Call this method to create a duplicated XmlElement with 
        /// all the attributes found from the source XmlElement.
        /// </summary>
        /// <param name="srcElement">The source XmlElement to duplicate.</param>
        /// <returns>Returns the duplicated XmlElement with all attributes 
        /// found in the source XmlElement. The resulting XmlElement will also 
        /// have a mandatory "type" attribute with value "Dynamo.Nodes.DSFunction".
        /// </returns>
        /// 
        public static XmlElement CreateFunctionNodeFrom(XmlElement srcElement)
        {
            if (srcElement == null)
                throw new ArgumentNullException("srcElement");

            XmlDocument document = srcElement.OwnerDocument;
            XmlElement dstElement = document.CreateElement("Dynamo.Nodes.DSFunction");

            foreach (XmlAttribute attribute in srcElement.Attributes)
                dstElement.SetAttribute(attribute.Name, attribute.Value);

            dstElement.SetAttribute("type", "Dynamo.Nodes.DSFunction");
            return dstElement;
        }

        /// <summary>
        /// Call this method to create a clone of the original XmlElement and 
        /// change its type at one go. This method preserves all the attributes 
        /// while updating only the type name.
        /// </summary>
        /// <param name="element">The XmlElement to be cloned and the type name 
        /// updated.</param>
        /// <param name="type">The fully qualified name of the new type.</param>
        /// <returns>Returns the cloned and updated XmlElement.</returns>
        /// 
        public static XmlElement CloneAndChangeType(XmlElement element, string type)
        {
            XmlElement cloned = element.CloneNode(false) as XmlElement;
            cloned.SetAttribute("type", type);
            return cloned;
        }

        public static void SetFunctionSignature(XmlElement element,
            string assemblyName, string methodName, string signature)
        {
            element.SetAttribute("assembly", assemblyName);
            element.SetAttribute("nickname", methodName);
            element.SetAttribute("function", signature);
        }

        public static string GetGuidFromXmlElement(XmlElement element)
        {
            return element.Attributes["guid"].Value;
        }
    }

    /// <summary>
    /// This structure uniquely identifies a given port in the graph.
    /// </summary>
    public struct PortId
    {
        public PortId(string owningNode, int portIndex, PortType type)
            : this()
        {
            this.OwningNode = owningNode;
            this.PortIndex = portIndex;
            this.PortType = type;
        }

        public string OwningNode { get; private set; }
        public int PortIndex { get; private set; }
        public PortType PortType { get; private set; }
    }

    /// <summary>
    /// This class contains the resulting nodes as a result of node migration.
    /// Note that this class may contain other information (e.g. connectors) in
    /// the future in the event a migration process results in other elements.
    /// </summary>
    /// 
    public class NodeMigrationData
    {
        private XmlNode connectorRoot = null;
        private List<XmlElement> migratedNodes = new List<XmlElement>();

        public NodeMigrationData(XmlDocument document)
        {
            this.Document = document;

            XmlNodeList cNodes = document.GetElementsByTagName("Connectors");
            if (cNodes.Count == 0)
                cNodes = document.GetElementsByTagName("dynConnectors");

            connectorRoot = cNodes[0]; // All the connectors in document.
        }

        #region Connector Management Methods

        /// <summary>
        /// Call this method to find the connector in the associate 
        /// XmlDocument, given its start and end port information.
        /// </summary>
        /// <param name="startPort">The identity of the start port.</param>
        /// <param name="endPort">The identity of the end port.</param>
        /// <returns>Returns the matching connector if one is found, or null 
        /// otherwise.</returns>
        /// 
        public XmlElement FindConnector(PortId startPort, PortId endPort)
        {
            if (connectorRoot != null && (connectorRoot.ChildNodes != null))
            {
                foreach (XmlNode node in connectorRoot.ChildNodes)
                {
                    XmlElement connector = node as XmlElement;
                    XmlAttributeCollection attribs = connector.Attributes;
                    if (startPort.OwningNode != attribs[0].Value)
                        continue;
                    if (endPort.OwningNode != attribs[2].Value)
                        continue;

                    if (startPort.PortIndex != Convert.ToInt16(attribs[1].Value))
                        continue;
                    if (endPort.PortIndex != Convert.ToInt16(attribs[3].Value))
                        continue;

                    return connector; // Found the matching connector.
                }
            }

            return null;
        }

        /// <summary>
        /// Call this method to retrieve the first connector given a port. This
        /// method is a near equivalent of FindConnectors, but only return the 
        /// first connector found. This way the caller codes can be simplified 
        /// in a way that it does not have the validate the returned list for 
        /// item count before accessing its element.
        /// </summary>
        /// <param name="portId">The identity of the port for which the first 
        /// connector is to be retrieved.</param>
        /// <returns>Returns the first connector found to connect to the given 
        /// port, or null otherwise.</returns>
        /// 
        public XmlElement FindFirstConnector(PortId portId)
        {
            if (connectorRoot == null || (connectorRoot.ChildNodes == null))
                return null;

            foreach (XmlNode node in connectorRoot.ChildNodes)
            {
                XmlElement connector = node as XmlElement;
                XmlAttributeCollection attribs = connector.Attributes;

                if (portId.PortType == PortType.INPUT)
                {
                    if (portId.OwningNode != attribs["end"].Value)
                        continue;
                    if (portId.PortIndex != Convert.ToInt16(attribs["end_index"].Value))
                        continue;
                }
                else
                {
                    if (portId.OwningNode != attribs["start"].Value)
                        continue;
                    if (portId.PortIndex != Convert.ToInt16(attribs["start_index"].Value))
                        continue;
                }

                return connector; // Found one, look no further.
            }

            return null;
        }

        /// <summary>
        /// Given a port, get all connectors that connect to it.
        /// </summary>
        /// <param name="portId">The identity of the port for which connectors 
        /// are to be retrieved.</param>
        /// <returns>Returns the list of connectors connecting to the given 
        /// port, or null if no connection is found connecting to it.</returns>
        /// 
        public IEnumerable<XmlElement> FindConnectors(PortId portId)
        {
            if (connectorRoot == null || (connectorRoot.ChildNodes == null))
                return null;

            List<XmlElement> foundConnectors = null;
            foreach (XmlNode node in connectorRoot.ChildNodes)
            {
                XmlElement connector = node as XmlElement;
                XmlAttributeCollection attribs = connector.Attributes;

                if (portId.PortType == PortType.INPUT)
                {
                    if (portId.OwningNode != attribs["end"].Value)
                        continue;
                    if (portId.PortIndex != Convert.ToInt16(attribs["end_index"].Value))
                        continue;
                }
                else
                {
                    if (portId.OwningNode != attribs["start"].Value)
                        continue;
                    if (portId.PortIndex != Convert.ToInt16(attribs["start_index"].Value))
                        continue;
                }

                if (foundConnectors == null)
                    foundConnectors = new List<XmlElement>();

                foundConnectors.Add(connector);

                // There can only be one connector for input port...
                if (portId.PortType == PortType.INPUT)
                    break; // ... so look no further.
            }

            return foundConnectors;
        }

        /// <summary>
        /// Reconnect a given connector to another port identified by "port".
        /// </summary>
        /// <param name="connector">The connector to update. Note that this 
        /// parameter can be null, in which case there won't be any movement 
        /// performed. This simplifies the caller so that it does not have to 
        /// do a null-check before every call to this method (connectors may 
        /// not present).</param>
        /// <param name="port">The new port to connect to.</param>
        /// 
        public void ReconnectToPort(XmlElement connector, PortId port)
        {
            if (connector == null) // Connector does not exist.
                return;

            XmlAttributeCollection attribs = connector.Attributes;
            if (port.PortType == PortType.INPUT) // We're updating end point.
            {
                attribs["end"].Value = port.OwningNode;
                attribs["end_index"].Value = port.PortIndex.ToString();
            }
            else // Updating the start point.
            {
                attribs["start"].Value = port.OwningNode;
                attribs["start_index"].Value = port.PortIndex.ToString();
            }
        }

        public void CreateConnector(XmlElement startNode,
            int startIndex, XmlElement endNode, int endIndex)
        {
            XmlElement connector = this.Document.CreateElement(
                "Dynamo.Models.ConnectorModel");

            connector.SetAttribute("start", MigrationManager.GetGuidFromXmlElement(startNode));
            connector.SetAttribute("start_index", startIndex.ToString());
            connector.SetAttribute("end", MigrationManager.GetGuidFromXmlElement(endNode));
            connector.SetAttribute("end_index", endIndex.ToString());
            connector.SetAttribute("portType", "0"); // Always zero, probably legacy issue.

            // Add new connector to document.
            connectorRoot.AppendChild(connector);
        }

        public void CreateConnectorFromId(string startNodeId,
            int startIndex, string endNodeId, int endIndex)
        {
            XmlElement connector = this.Document.CreateElement(
                "Dynamo.Models.ConnectorModel");

            connector.SetAttribute("start", startNodeId);
            connector.SetAttribute("start_index", startIndex.ToString());
            connector.SetAttribute("end", endNodeId);
            connector.SetAttribute("end_index", endIndex.ToString());
            connector.SetAttribute("portType", "0"); // Always zero, probably legacy issue.

            // Add new connector to document.
            connectorRoot.AppendChild(connector);
        }

        public void CreateConnector(XmlElement connector)
        {
            connectorRoot.AppendChild(connector);
        }

        #endregion

        #region Node Management Methods

        public void AppendNode(XmlElement node)
        {
            migratedNodes.Add(node);
        }

        #endregion

        #region Public Class Properties

        public XmlDocument Document { get; private set; }

        public IEnumerable<XmlElement> MigratedNodes
        {
            get { return this.migratedNodes; }
        }

        #endregion
    }

    /// <summary>
    /// Marks methods on a NodeModel to be used for version migration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NodeMigrationAttribute : Attribute
    {
        /// <summary>
        /// Latest Version this migration applies to.
        /// </summary>
        public Version From { get; private set; }

        /// <summary>
        /// Version this migrates to.
        /// </summary>
        public Version To { get; private set; }

        public NodeMigrationAttribute(string from, string to="")
        {
            From = new Version(from);
            To = String.IsNullOrEmpty(to) ? null : new Version(to);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class WorkspaceMigrationAttribute : Attribute
    {
        public Version From { get; private set; }
        public Version To { get; private set; }

        public WorkspaceMigrationAttribute(string from, string to="")
        {
            From = new Version(from);
            To = String.IsNullOrEmpty(to) ? null : new Version(to);
        }
    }
}
