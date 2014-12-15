using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using Dynamo.Utilities;
using System.Collections.Generic;
using System.IO;

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
        /// <summary>
        /// Enumerator to determine if migration should proceed or abort. This 
        /// enumerator is to be used with MigrationManager.ShouldMigrateFile().
        /// </summary>
        public enum Decision
        {
            /// <summary>
            /// The migration should not proceed and the file open operation 
            /// should be aborted. This can be used to indicate that a version 
            /// of file that is no longer supported and no migration path is 
            /// provided.
            /// </summary>
            Abort,

            /// <summary>
            /// File migration should proceed to migrate the older file version 
            /// to a newer one.
            /// </summary>
            Migrate,

            /// <summary>
            /// The file version is up-to-date and the file can be used as-is 
            /// without migration.
            /// </summary>
            Retain
        }

        private static MigrationManager _instance;

        private static int IdentifierIndex = 0;

        private static int NewNodeOffsetX = -150;

        private static int NewNodeOffsetY = 100;

        public static int GetNextIdentifierIndex()
        {
            return IdentifierIndex++;
        }

        public static void ResetIdentifierIndex()
        {
            IdentifierIndex = 0;
        }

        /// <summary>
        /// The singleton instance property.
        /// </summary>
        public static MigrationManager Instance
        {
            get { return _instance ?? (_instance = new MigrationManager()); }
        }

        private MigrationReport migrationReport;

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
        public void ProcessWorkspaceMigrations(DynamoModel dynamoModel, XmlDocument xmlDoc, Version workspaceVersion)
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

            var currentVersion = dynamoModel.HomeSpace.WorkspaceVersion;

            while (workspaceVersion != null && workspaceVersion < currentVersion)
            {
                var nextMigration = migrations.FirstOrDefault(x => x.From >= workspaceVersion);

                if (nextMigration == null)
                    break;

                nextMigration.method.Invoke(null, new object[] { xmlDoc });
                workspaceVersion = nextMigration.To;
            }
        }

        public void ProcessNodesInWorkspace(DynamoModel dynamoModel, XmlDocument xmlDoc, Version workspaceVersion)
        {
            if (DynamoModel.EnableMigrationLogging)
            {
                // For each new file opened, create a new migration report
                migrationReport = new MigrationReport();
            }

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
                System.Type type = Dynamo.Nodes.Utilities.ResolveType(dynamoModel, typeName);

                if (type == null)
                {
                    // If we are not able to resolve the type given its name, 
                    // turn it into a deprecated node so that user is aware.
                    migratedNodes.Add(MigrationManager.CreateMissingNode(
                        elNode as XmlElement, 1, 1));

                    continue; // Error displayed in console, continue on.
                }

                // Migrate the given node into one or more new nodes.
                var migrationData = this.MigrateXmlNode(dynamoModel.HomeSpace, elNode, type, workspaceVersion);
                migratedNodes.AddRange(migrationData.MigratedNodes);
            }

            if (DynamoModel.EnableMigrationLogging)
            {
                string dynFilePath = xmlDoc.BaseURI;
                migrationReport.WriteToXmlFile(dynFilePath);
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

        public NodeMigrationData MigrateXmlNode(WorkspaceModel homespace, XmlNode elNode, System.Type type, Version workspaceVersion)
        {
            var migrations = (from method in type.GetMethods()
                              let attribute =
                                  method.GetCustomAttributes(false).OfType<NodeMigrationAttribute>().FirstOrDefault()
                              where attribute != null
                              let result = new { method, attribute.From, attribute.To }
                              orderby result.From
                              select result).ToList();

            var currentVersion = MigrationManager.VersionFromWorkspace(homespace);

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

                if (DynamoModel.EnableMigrationLogging)
                {
                    // record migration data for successful migrations
                    migrationReport.AddMigrationDataToNodeMap(nodeToMigrate.Name, migrationData.MigratedNodes);
                }
                workspaceVersion = nextMigration.To;
            }

            return migrationData;
        }

        /// <summary>
        /// Call this method to backup the DYN file specified by originalPath. The 
        /// new file will be backed up to a location where Dynamo has write access to.
        /// </summary>
        /// <param name="originalPath">Path of the original DYN file to be backed up.</param>
        /// <param name="backupPath">Path of the backed up file. This value will be a valid 
        /// file path only if this method returns true.</param>
        /// <returns>Returns true if the backup was successful, or false otherwise.</returns>
        /// 
        internal static bool BackupOriginalFile(string originalPath, ref string backupPath)
        {
            backupPath = string.Empty;

            if (string.IsNullOrEmpty(originalPath))
                throw new ArgumentException("Argument cannot be empty", "originalPath");
            if (!System.IO.File.Exists(originalPath))
                throw new System.IO.FileNotFoundException("File not found", originalPath);

            try
            {
                string folder = GetBackupFolder(Path.GetDirectoryName(originalPath), true);
                string destFileName = GetUniqueFileName(folder, Path.GetFileName(originalPath));
                System.IO.File.Copy(originalPath, destFileName);
                backupPath = destFileName;
                return true;
            }
            catch (System.IO.IOException)
            {
                // If we caught an IO exception, fall through and let the rest handle this 
                // (by saving to other locations). Any other exception will be thrown to the 
                // caller for handling.
            }

            try
            {
                var myDocs = Environment.SpecialFolder.MyDocuments;
                string folder = GetBackupFolder(Environment.GetFolderPath(myDocs), true);
                string destFileName = GetUniqueFileName(folder, Path.GetFileName(originalPath));
                System.IO.File.Copy(originalPath, destFileName);
                backupPath = destFileName;
                return true;
            }
            catch (System.IO.IOException)
            {
                return false; // Okay I give up.
            }
        }

        /// <summary>
        /// Call this method to get the unique backup file name within the given folder.
        /// </summary>
        /// <param name="folder">The folder where file search should happen.</param>
        /// <param name="fileNameWithExtension">The name of the original file which is 
        /// to be backed-up. This argument should have an extension, although it is not 
        /// mandatory.</param>
        /// <returns>Returns the full path to a unique file name for the backup file.</returns>
        /// 
        internal static string GetUniqueFileName(string folder, string fileNameWithExtension)
        {
            string[] fileNames = Directory.GetFiles(folder, fileNameWithExtension + ".*.backup");
            int indexToUse = GetUniqueIndex(fileNames);

            // The file name will be in the form of "fileName.NNN.backup".
            string fileName = fileNameWithExtension + string.Format(".{0}.backup", indexToUse);
            return Path.Combine(folder, fileName);
        }

        /// <summary>
        /// Call this method with a root directory path information, and then 
        /// a backup sub-directory will be created below it (if one does not 
        /// already exist).
        /// </summary>
        /// <param name="baseFolder">This is a directory inside which a new 
        /// backup sub-directory will be created. If this paramter does not 
        /// represent a valid directory name, an exception will be thrown.
        /// </param>
        /// <param name="create">Set this parameter to false if the creation of 
        /// the backup sub-directory is not desired. Typically this means the
        /// method is called from within a test case and it is only interested 
        /// in getting the resulting path back without actually creating a new 
        /// backup sub-directory.</param>
        /// <returns>Returns full path to the backup folder created.</returns>
        /// 
        internal static string GetBackupFolder(string baseFolder, bool create)
        {
            if (string.IsNullOrEmpty(baseFolder))
                throw new ArgumentNullException("rootFolder");

            if (Directory.Exists(baseFolder) == false)
            {
                var message = string.Format("Folder {0} does not exist", baseFolder);
                throw new ArgumentException(message, "rootFolder");
            }

            var backupFolderName = Dynamo.UI.Configurations.BackupFolderName;

            var subFolder = Path.Combine(baseFolder, backupFolderName);
            if (create && (Directory.Exists(subFolder) == false))
                Directory.CreateDirectory(subFolder);

            return subFolder;
        }

        /// <summary>
        /// Call this method to determine the next available backup file name from the 
        /// given set of file names.
        /// </summary>
        /// <param name="fileNames">An array of file names, each in the form of 
        /// 'FileName.NNN.backup'.</param>
        /// <returns>Returns the next available index to use as backup file name</returns>
        /// 
        internal static int GetUniqueIndex(string[] fileNames)
        {
            if (fileNames == null || (fileNames.Length <= 0))
                return 0;

            string result = fileNames.Aggregate((prevFileName, currFileName) =>
            {
                int prev = ExtractFileIndex(prevFileName);
                int curr = ExtractFileIndex(currFileName);
                return ((prev > curr) ? prevFileName : currFileName);
            });

            // Use the next larger integer as index.
            return ExtractFileIndex(result) + 1;
        }

        /// <summary>
        /// Call this method to extract the index of a backup file.
        /// </summary>
        /// <param name="fileName">The file name of a backup file. This parameter 
        /// must be in the form of 'FileName.NNN.backup', where 'NNN' is an 
        /// integer value. The file name must also have a '*.backup' extension.
        /// </param>
        /// <returns>Returns the integer equivalent of the backup file index 
        /// 'NNN' if the call is successful.</returns>
        /// 
        internal static int ExtractFileIndex(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName");

            if (Path.GetExtension(fileName) != ".backup")
            {
                var msg = "File name must be in 'fileName.NNN.backup' form.";
                throw new ArgumentException(msg, "fileName");
            }

            // Get rid of ".backup" extension.
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            int dotIndex = fileNameWithoutExtension.LastIndexOf('.');
            if (dotIndex == -1)
            {
                var msg = "File name must be in 'fileName.NNN.backup' form.";
                throw new ArgumentException(msg, "fileName");
            }

            // Extract 'NNN' and convert it into the corresponding integer value.
            return Int32.Parse(fileNameWithoutExtension.Substring(dotIndex + 1));
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
        /// object. Valid examples include "0.6.3.0" and "0.6.3.20048".</param>
        /// <returns>Returns the Version object representation of 'version' 
        /// argument, except without the 'revision number'.</returns>
        /// 
        internal static Version VersionFromString(string version)
        {
            Version ver = string.IsNullOrEmpty(version) ?
                new Version(0, 0, 0, 0) : new Version(version);

            // Ignore revision number.
            return new Version(ver.Major, ver.Minor, ver.Build, 0);
        }

        /// <summary>
        /// Call this method to obtain the version of current WorkspaceModel.
        /// Note that the revision number is dropped as both "0.7.0.1234" 
        /// should be treated as the same version as "0.7.0.5678", and no file 
        /// migration should take place.
        /// </summary>
        /// <param name="workspace">The WorkspaceModel to get the Version from.
        /// </param>
        /// <returns>Returns the Version object representing the workspace 
        /// version with the revision set to 0.</returns>
        /// 
        internal static Version VersionFromWorkspace(WorkspaceModel workspace)
        {
            // Ignore revision number.
            var ver = workspace.WorkspaceVersion;
            return new Version(ver.Major, ver.Minor, ver.Build, 0);
        }

        /// <summary>
        /// Call this method to determine if migration should take place 
        /// for the input DYN/DYF file based on the given version numbers.
        /// </summary>
        /// <param name="fileVersion">The version of input file.</param>
        /// <param name="currVersion">The version of Dynamo software.</param>
        /// <returns>Returns the decision if the migration should take place or 
        /// not. See "Decision" enumeration for details of each field.</returns>
        /// 
        internal static Decision ShouldMigrateFile(
            Version fileVersion, Version currVersion)
        {
            // We currently enable migration for testing scenario. This is to 
            // avoid large number of test failures with this change, and also 
            // ensure that our tests continue to exercise migration code changes.
            // 
            if (DynamoModel.IsTestMode)
            {
                if (fileVersion < currVersion)
                    return Decision.Migrate;

                return Decision.Retain;
            }

            //Force the file to go through the migration process, when the file version
            //is 0.7.0.x. 
            //Reason: There were files creaeted in 0.6.x with wrong version number 0.7.0.
            //Force them to migration will manage to have those files migrated properly.
            //Related YouTrack Defect: MAGN3767
            if (fileVersion == new Version(0, 7, 0, 0))
            {
                return Decision.Migrate;
            }

            // For end-users, disable migration.
            if (fileVersion < currVersion)
                return Decision.Migrate;

            return Decision.Retain; // User has latest file, allow usage.
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
        public static XmlElement CreateFunctionNode(XmlDocument document, XmlElement oldNode,
            int nodeIndex, string assembly, string nickname, string signature)
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
            element.SetAttribute("guid", Guid.NewGuid().ToString());

            element.SetAttribute("x",
                (Convert.ToDouble(oldNode.GetAttribute("x"))
                + NewNodeOffsetX).ToString());
            element.SetAttribute("y",
                (Convert.ToDouble(oldNode.GetAttribute("y"))
                + nodeIndex * NewNodeOffsetY).ToString());

            return element;
        }

        public static XmlElement CreateVarArgFunctionNode(XmlDocument document, XmlElement oldNode,
            int nodeIndex, string assembly, string nickname, string signature, string inputcount)
        {
            XmlElement element = document.CreateElement("Dynamo.Nodes.DSVarArgFunction");
            element.SetAttribute("type", "Dynamo.Nodes.DSVarArgFunction");
            element.SetAttribute("assembly", assembly);
            element.SetAttribute("nickname", nickname);
            element.SetAttribute("function", signature);
            element.SetAttribute("inputcount", inputcount);

            // Attributes with default values (as in DynamoModel.OpenWorkspace).
            element.SetAttribute("isVisible", "true");
            element.SetAttribute("isUpstreamVisible", "true");
            element.SetAttribute("lacing", "Disabled");
            element.SetAttribute("guid", Guid.NewGuid().ToString());

            element.SetAttribute("x",
                (Convert.ToDouble(oldNode.GetAttribute("x"))
                + NewNodeOffsetX).ToString());
            element.SetAttribute("y",
                (Convert.ToDouble(oldNode.GetAttribute("y"))
                + nodeIndex * NewNodeOffsetY).ToString());

            return element;
        }

        public static XmlElement CreateCodeBlockNodeModelNode(XmlDocument document, XmlElement oldNode,
            int nodeIndex, string codeTest)
        {
            XmlElement element = document.CreateElement("Dynamo.Nodes.CodeBlockNodeModel");
            element.SetAttribute("type", "Dynamo.Nodes.CodeBlockNodeModel");

            element.SetAttribute("nickname", "Code Block");
            element.SetAttribute("CodeText", codeTest);
            element.SetAttribute("ShouldFocus", "false");

            // Attributes with default values (as in DynamoModel.OpenWorkspace).
            element.SetAttribute("isVisible", "true");
            element.SetAttribute("isUpstreamVisible", "true");
            element.SetAttribute("lacing", "Disabled");
            element.SetAttribute("guid", Guid.NewGuid().ToString());

            element.SetAttribute("x",
                (Convert.ToDouble(oldNode.GetAttribute("x"))
                + NewNodeOffsetX).ToString());
            element.SetAttribute("y",
                (Convert.ToDouble(oldNode.GetAttribute("y"))
                + nodeIndex * NewNodeOffsetY).ToString());

            return element;
        }

        public static XmlElement CreateNode(XmlDocument document, XmlElement oldNode,
            int nodeIndex, string name, string nickname)
        {
            XmlElement element = document.CreateElement(name);
            element.SetAttribute("type", name);
            element.SetAttribute("nickname", nickname);

            // Attributes with default values (as in DynamoModel.OpenWorkspace).
            element.SetAttribute("isVisible", "true");
            element.SetAttribute("isUpstreamVisible", "true");
            element.SetAttribute("lacing", "Disabled");
            element.SetAttribute("guid", Guid.NewGuid().ToString());

            element.SetAttribute("x",
                (Convert.ToDouble(oldNode.GetAttribute("x"))
                + NewNodeOffsetX).ToString());
            element.SetAttribute("y",
                (Convert.ToDouble(oldNode.GetAttribute("y"))
                + nodeIndex * NewNodeOffsetY).ToString());

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
        /// Create a custom node as a replacement for an existing node.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="srcElement"></param>
        /// <param name="id">The custom node id.</param>
        /// <param name="name">The custom node name.</param>
        /// <param name="description">The custom node's description.</param>
        /// <param name="inputs">A list of input names.</param>
        /// <param name="outputs">A list of output names.</param>
        /// <returns></returns>
        public static XmlElement CreateCustomNodeFrom(XmlDocument document, XmlElement srcElement,
            string id, string name, string description, List<string> inputs, List<string> outputs)
        {
            if (srcElement == null)
                throw new ArgumentNullException("srcElement");

            XmlElement funcEl = document.CreateElement("Dynamo.Nodes.Function");

            foreach (XmlAttribute attribute in srcElement.Attributes)
                funcEl.SetAttribute(attribute.Name, attribute.Value);

            funcEl.SetAttribute("type", "Dynamo.Nodes.Function");

            var idEl = document.CreateElement("ID");
            idEl.SetAttribute("value", id);

            var nameEl = document.CreateElement("Name");
            nameEl.SetAttribute("value", name);

            var descripEl = document.CreateElement("Description");
            descripEl.SetAttribute("value", description);

            var inputsEl = document.CreateElement("Inputs");
            foreach (var input in inputs)
            {
                var inputEl = document.CreateElement("Input");
                inputEl.SetAttribute("value", input);
                inputsEl.AppendChild(inputEl);
            }

            var outputsEl = document.CreateElement("Outputs");
            foreach (var output in outputs)
            {
                var outputEl = document.CreateElement("Output");
                outputEl.SetAttribute("value", output);
                outputsEl.AppendChild(outputEl);
            }

            funcEl.AppendChild(idEl);
            funcEl.AppendChild(nameEl);
            funcEl.AppendChild(descripEl);
            funcEl.AppendChild(inputsEl);
            funcEl.AppendChild(outputsEl);

            return funcEl;
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

        public static XmlElement CreateVarArgFunctionNodeFrom(XmlElement srcElement)
        {
            if (srcElement == null)
                throw new ArgumentNullException("srcElement");

            int childNumber = srcElement.ChildNodes.Count;
            string childNumberString = childNumber.ToString();

            XmlDocument document = srcElement.OwnerDocument;
            XmlElement dstElement = document.CreateElement("Dynamo.Nodes.DSVarArgFunction");

            foreach (XmlAttribute attribute in srcElement.Attributes)
                dstElement.SetAttribute(attribute.Name, attribute.Value);

            dstElement.SetAttribute("type", "Dynamo.Nodes.DSVarArgFunction");
            dstElement.SetAttribute("inputcount", childNumberString);
            return dstElement;
        }

        /// <summary>
        /// Call this method to create an empty Code Block node, with all 
        /// attributes carried over from an existing src XmlElement.
        /// </summary>
        /// <param name="srcElement">The source element from which the Code 
        /// Block node XmlElement is constructed. All attributes of the source 
        /// XmlElement will be copied over, and Code Block node specific 
        /// attributes will be added.</param>
        /// <returns>Returns an XmlElement that represents the resulting Code
        /// Block node.</returns>
        /// 
        public static XmlElement CreateCodeBlockNodeFrom(XmlElement srcElement)
        {
            if (srcElement == null)
                throw new ArgumentNullException("srcElement");

            XmlDocument document = srcElement.OwnerDocument;
            XmlElement dstElement = document.CreateElement("Dynamo.Nodes.CodeBlockNodeModel");

            foreach (XmlAttribute attribute in srcElement.Attributes)
                dstElement.SetAttribute(attribute.Name, attribute.Value);

            dstElement.SetAttribute("CodeText", string.Empty);
            dstElement.SetAttribute("ShouldFocus", "false");
            dstElement.SetAttribute("nickname", "Code Block");
            dstElement.SetAttribute("lacing", "Disabled");
            dstElement.SetAttribute("type", "Dynamo.Nodes.CodeBlockNodeModel");
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
        /// <param name="nickname">The new nickname, by which this node is known.</param>
        /// <returns>Returns the cloned and updated XmlElement.</returns>
        /// 
        public static XmlElement CloneAndChangeName(XmlElement element, string type, string nickname)
        {
            XmlDocument document = element.OwnerDocument;
            XmlElement cloned = document.CreateElement(type);

            foreach (XmlAttribute attribute in element.Attributes)
                cloned.SetAttribute(attribute.Name, attribute.Value);

            cloned.SetAttribute("type", type);
            cloned.SetAttribute("nickname", nickname);
            return cloned;
        }


        /// <summary>
        /// Call this method to create a dummy node, should a node failed to be 
        /// migrated. This results in a dummy node with a description of what the 
        /// original node type was, and also retain the number of input and output
        /// ports.
        /// </summary>
        /// <param name="element">XmlElement representing the original node which
        /// has failed migration.</param>
        /// <param name="inportCount">The number of input ports required on the 
        /// new dummy node. This number must be a positive number greater or 
        /// equal to zero.</param>
        /// <param name="outportCount">The number of output ports required on the 
        /// new dummy node. This number must be a positive number greater or 
        /// equal to zero.</param>
        /// <returns>Returns a new XmlElement representing the dummy node.</returns>
        /// 
        public static XmlElement CreateDummyNode(
            XmlElement element, int inportCount, int outportCount)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (inportCount < 0 || (outportCount < 0))
            {
                var message = "Argument value must be equal or larger than zero";
                throw new ArgumentException(message, "inportCount/outportCount");
            }

            var dummyNodeName = "DSCoreNodesUI.DummyNode";
            XmlDocument document = element.OwnerDocument;
            XmlElement dummy = document.CreateElement(dummyNodeName);

            foreach (XmlAttribute attribute in element.Attributes)
                dummy.SetAttribute(attribute.Name, attribute.Value);

            dummy.SetAttribute("type", dummyNodeName);
            dummy.SetAttribute("legacyNodeName", element.GetAttribute("type"));
            dummy.SetAttribute("inputCount", inportCount.ToString());
            dummy.SetAttribute("outputCount", outportCount.ToString());
            dummy.SetAttribute("nodeNature", "Deprecated");

            XmlElement originalNode = document.CreateElement("OriginalNodeContent");

            //clone a copy of the original node
            XmlElement nodeContent = (XmlElement)element.Clone();

            //append the original node content as a child of the dummy node
            originalNode.AppendChild(nodeContent);
            dummy.AppendChild(originalNode);

            return dummy;
        }

        /// <summary>
        /// Call this method to convert a DSFunction/DSVarArgFunction element into 
        /// an equivalent dummy node to indicate that a function node cannot be 
        /// resolved during load time. This method retains the number of input 
        /// ports based on the function signature that comes with the XmlElement 
        /// that represent the function node.
        /// </summary>
        /// <param name="element">XmlElement representing the original DSFunction
        /// node which has failed function resolution. This XmlElement must be of 
        /// type "DSFunction" or "DSVarArgFunction" otherwise an exception will be 
        /// thrown.</param>
        /// <returns>Returns a new XmlElement representing the dummy node.</returns>
        /// 
        public static XmlElement CreateUnresolvedFunctionNode(XmlElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            if (element.Name.Equals("Dynamo.Nodes.DSFunction") == false)
            {
                if (element.Name.Equals("Dynamo.Nodes.DSVarArgFunction") == false)
                {
                    var message = "Only DSFunction/DSVarArgFunction should be here.";
                    throw new ArgumentException(message);
                }
            }

            var type = element.Attributes["type"].Value;
            if (type.Equals("Dynamo.Nodes.DSFunction") == false)
            {
                if (type.Equals("Dynamo.Nodes.DSVarArgFunction") == false)
                {
                    var message = "Only DSFunction/DSVarArgFunction should be here.";
                    throw new ArgumentException(message);
                }
            }

            var nicknameAttrib = element.Attributes["nickname"];
            if (nicknameAttrib == null)
                throw new ArgumentException("'nickname' attribute missing.");

            var nickname = nicknameAttrib.Value;
            if (string.IsNullOrEmpty(nickname))
                throw new ArgumentException("'nickname' attribute missing.");

            // Determine the number of input and output count (always 1).
            int inportCount = DetermineFunctionInputCount(element);
            var assembly = DetermineAssemblyName(element);

            // Create an XmlElement representation of the new dummy node.
            var dummy = CreateDummyNode(element, inportCount, 1);
            dummy.SetAttribute("legacyNodeName", nickname);
            dummy.SetAttribute("legacyAssembly", assembly);
            dummy.SetAttribute("nodeNature", "Unresolved");
            return dummy;
        }

        /// <summary>
        /// Call this method to create a dummy node, should a node failed to be 
        /// migrated. This results in a dummy node with a description of what the 
        /// original node type was, and also retain the number of input and output
        /// ports.
        /// </summary>
        /// <param name="element">XmlElement representing the original node which
        /// has failed migration.</param>
        /// <param name="inportCount">The number of input ports required on the 
        /// new dummy node. This number must be a positive number greater or 
        /// equal to zero.</param>
        /// <param name="outportCount">The number of output ports required on the 
        /// new dummy node. This number must be a positive number greater or 
        /// equal to zero.</param>
        /// <returns>Returns a new XmlElement representing the dummy node.</returns>
        /// 
        public static XmlElement CreateMissingNode(
            XmlElement element, int inportCount, int outportCount)
        {
            var dummy = CreateDummyNode(element, inportCount, outportCount);
            dummy.SetAttribute("nodeNature", "Unresolved");
            return dummy;
        }

        private static int DetermineFunctionInputCount(XmlElement element)
        {
            int additionalPort = 0;

            // "DSVarArgFunction" is a "VariableInputNode", therefore it will 
            // have "inputcount" as one of the attributes. If such attribute 
            // does not exist, throw an ArgumentException.
            if (element.Name.Equals("Dynamo.Nodes.DSVarArgFunction"))
            {
                var inputCountAttrib = element.Attributes["inputcount"];

                if (inputCountAttrib == null)
                {
                    throw new ArgumentException(string.Format(
                        "Function inputs cannot be determined ({0}).",
                        element.GetAttribute("nickname")));
                }

                return Convert.ToInt32(inputCountAttrib.Value);
            }

            var signature = string.Empty;
            var signatureAttrib = element.Attributes["function"];
            if (signatureAttrib != null)
                signature = signatureAttrib.Value;
            else if (element.ChildNodes.Count > 0)
            {
                // We have an old file format with "FunctionItem" child element.
                var childElement = element.ChildNodes[0] as XmlElement;
                signature = string.Format("{0}@{1}",
                    childElement.GetAttribute("DisplayName"),
                    childElement.GetAttribute("Parameters").Replace(';', ','));

                // We need one more port for instance methods/properties.
                switch (childElement.GetAttribute("Type"))
                {
                    case "InstanceMethod":
                    case "InstanceProperty":
                        additionalPort = 1; // For taking the instance itself.
                        break;
                }
            }

            if (string.IsNullOrEmpty(signature))
            {
                var message = "Function signature cannot be determined.";
                throw new ArgumentException(message);
            }

            int atSignIndex = signature.IndexOf('@');
            if (atSignIndex >= 0) // An '@' sign found, there's param information.
            {
                signature = signature.Substring(atSignIndex + 1); // Skip past '@'.
                var parts = signature.Split(new char[] { ',' });
                return ((parts != null) ? parts.Length : 1) + additionalPort;
            }

            return additionalPort + 1; // At least one.
        }

        private static string DetermineAssemblyName(XmlElement element)
        {
            var assemblyName = string.Empty;
            var assemblyAttrib = element.Attributes["assembly"];
            if (assemblyAttrib != null)
                assemblyName = assemblyAttrib.Value;
            else if (element.ChildNodes.Count > 0)
            {
                // We have an old file format with "FunctionItem" child element.
                var childElement = element.ChildNodes[0] as XmlElement;
                var funcItemAsmAttrib = childElement.Attributes["Assembly"];
                if (funcItemAsmAttrib != null)
                    assemblyName = funcItemAsmAttrib.Value;
            }

            if (string.IsNullOrEmpty(assemblyName))
                return string.Empty;

            try { return Path.GetFileName(assemblyName); }
            catch (Exception) { return string.Empty; }
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
        /// <returns>Returns the notmatching connector if one is found, or null 
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

        public void RemoveFirstConnector(PortId portId)
        {
            if (connectorRoot == null || (connectorRoot.ChildNodes == null))
                return;

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

                connectorRoot.RemoveChild(connector);
            }
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

        public NodeMigrationAttribute(string from, string to = "")
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

        public WorkspaceMigrationAttribute(string from, string to = "")
        {
            From = new Version(from);
            To = String.IsNullOrEmpty(to) ? null : new Version(to);
        }
    }
}
