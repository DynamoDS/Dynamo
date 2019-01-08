using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Engine;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.NodeLoaders;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Presets;
using ProtoCore.Namespace;

namespace Dynamo.Graph.Workspaces
{
    /// <summary>
    /// This class contains methods and properties that defines a customnodeworkspace.
    /// </summary>
    public class CustomNodeWorkspaceModel : WorkspaceModel, ICustomNodeWorkspaceModel
    {
        /// <summary>
        /// Returns identifier of the custom node
        /// </summary>
        public Guid CustomNodeId
        {
            get { return customNodeId; }
            private set
            {
                if (value == customNodeId) 
                    return;

                var oldId = customNodeId;
                customNodeId = value;
                OnFunctionIdChanged(oldId);
                OnDefinitionUpdated();
                OnInfoChanged();
                RaisePropertyChanged("CustomNodeId");
            }
        }

        private Guid customNodeId;

        #region Contructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomNodeWorkspaceModel"/> class
        /// by given information about it and node factory
        /// </summary>
        /// <param name="info">Information for creating custom node workspace</param>
        /// <param name="factory">Node factory to create nodes</param>
        public CustomNodeWorkspaceModel(WorkspaceInfo info, NodeFactory factory)
            : this(factory,
                Enumerable.Empty<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                Enumerable.Empty<AnnotationModel>(),
                Enumerable.Empty<PresetModel>(),
                new ElementResolver(),
                info) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomNodeWorkspaceModel"/> class
        /// by given information about it and specified item collections
        /// </summary>
        /// <param name="factory">Node factory to create nodes</param>
        /// <param name="nodes">Node collection of the workspace</param>
        /// <param name="notes">Note collection of the workspace</param>
        /// <param name="annotations">Group collection of the workspace</param>
        /// <param name="presets">Preset collection of the workspace</param>
        /// <param name="elementResolver">ElementResolver responsible for resolving 
        /// a partial class name to its fully resolved name</param>
        /// <param name="info">Information for creating custom node workspace</param>
        public CustomNodeWorkspaceModel( 
            NodeFactory factory,
            IEnumerable<NodeModel> nodes, 
            IEnumerable<NoteModel> notes, 
            IEnumerable<AnnotationModel> annotations,
            IEnumerable<PresetModel> presets,
            ElementResolver elementResolver, 
            WorkspaceInfo info)
            : base(nodes, notes, annotations, info, factory, presets, elementResolver)
        {
            Debug.WriteLine("Creating a custom node workspace...");

            HasUnsavedChanges = false;

            CustomNodeId = Guid.Parse(info.ID);
            Category = info.Category;
            Description = info.Description;
            IsVisibleInDynamoLibrary = info.IsVisibleInDynamoLibrary;
            PropertyChanged += OnPropertyChanged;
        }

        #endregion

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Name")
                OnInfoChanged();

            if (args.PropertyName == "Category" || args.PropertyName == "Description")
            {
                HasUnsavedChanges = true;
                OnInfoChanged();
            }
        }

        /// <summary>
        ///     All CustomNodeDefinitions which this Custom Node depends on.
        /// </summary>
        public IEnumerable<CustomNodeDefinition> CustomNodeDependencies
        {
            get
            {
                return Nodes
                    .OfType<Function>()
                    .Select(node => node.Definition)
                    .Where(def => def.FunctionId != CustomNodeId)
                    .Distinct();
            }
        }

        /// <summary>
        ///     The definition of this custom node, based on the current state of this workspace.
        /// </summary>
        public CustomNodeDefinition CustomNodeDefinition
        {
            get
            {
                return new CustomNodeDefinition(CustomNodeId, Name, Nodes);
            }
        }

        /// <summary>
        ///     The information about this custom node, based on the current state of this workspace.
        /// </summary>
        public CustomNodeInfo CustomNodeInfo
        {
            get
            {
                return new CustomNodeInfo(CustomNodeId, Name, Category, Description, FileName, IsVisibleInDynamoLibrary);
            }
        }

        // This is being used to remove mismatching related to shared custom nodes
        // described here http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-9333
        /// <summary>
        ///     Gets appropriate name of workspace for sharing.
        /// </summary>
        /// <returns>The name of workspace for sharing</returns>
        public override string GetSharedName()
        {
            string result;

            try
            {
                string[] splited = this.FileName.Split(new string[] {@"\"}, StringSplitOptions.None);
                result = splited[splited.Length - 1].Replace(".dyf", "");
            }
            catch
            {
                result = this.Name;
            }
            return result;
        }

        /// <summary>
        /// Updates custom node information by given data
        /// </summary>
        /// <param name="newName">New name of the workspace. 
        /// The name will not change if the parameter is omitted.</param>
        /// <param name="newCategory">New category of the workspace. 
        /// The category will not change if the parameter is omitted.</param>
        /// <param name="newDescription">New description of the workspace. 
        /// The description will not change if the parameter is omitted.</param>
        /// <param name="newFilename">New file name of the workspace. 
        /// The file name will not change if the parameter is omitted.</param>
        public void SetInfo(string newName = null, string newCategory = null, string newDescription = null, string newFilename = null)
        {
            PropertyChanged -= OnPropertyChanged;

            Name = newName ?? Name;
            Category = newCategory ?? Category;
            Description = newDescription ?? Description;
            FileName = newFilename ?? FileName;

            PropertyChanged += OnPropertyChanged;

            if (newName != null || newCategory != null || newDescription != null || newFilename != null)
                OnInfoChanged();
        }

        /// <summary>
        ///     Search category for this workspace, if it is a Custom Node.
        /// </summary>
        public string Category
        {
            get { return category; }
            set
            {
                category = value;
                RaisePropertyChanged("Category");
            }
        }

        private string category;

        /// <summary>
        ///     A description of the workspace
        /// </summary>
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                RaisePropertyChanged("Description");
            }
        }

        private string description;

        /// <summary>
        ///     Custom node visibility in the Dynamo library
        /// </summary>
        public bool IsVisibleInDynamoLibrary
        {
            get { return isVisibleInDynamoLibrary; }
            set
            {
                isVisibleInDynamoLibrary = value;
            }
        }

        private bool isVisibleInDynamoLibrary;

        internal override void RequestRun()
        {
            base.RequestRun();
            HasUnsavedChanges = true;
            OnDefinitionUpdated();
        }

        protected override void NodeModified(NodeModel node)
        {
            base.NodeModified(node);
            RequestRun();
        }

        /// <summary>
        /// Notifies listeners that custom node workspace has changed
        /// </summary>
        public event Action InfoChanged;
        protected virtual void OnInfoChanged()
        {
            Action handler = InfoChanged;
            if (handler != null) handler();
        }

        /// <summary>
        /// Disable the DefinitionUpdated event. This might be desirable to lump a large number of 
        /// workspace changes into a single event.
        /// </summary>
        internal bool SilenceDefinitionUpdated { get; set; }

        /// <summary>
        /// Notifies all custom node instances that definition has changed
        /// </summary>
        public event Action DefinitionUpdated;
        internal virtual void OnDefinitionUpdated()
        {
            if (SilenceDefinitionUpdated) return;

            var handler = DefinitionUpdated;
            if (handler != null) handler();
        }

        /// <summary>
        /// Notifies listeners that custom node identifier has changed
        /// </summary>
        public event Action<Guid> FunctionIdChanged;
        protected virtual void OnFunctionIdChanged(Guid oldId)
        {
            var handler = FunctionIdChanged;
            if (handler != null) handler(oldId);
        }

        /// <summary>
        /// Saves custom node workspace
        /// </summary>
        /// <param name="newPath">New location to save the workspace.</param>
        /// <param name="isBackup">Indicates whether saved workspace is backup or not. If it's not backup,
        /// we should add it to recent files. Otherwise leave it.</param>
        /// <param name="engine"></param>
        /// <returns></returns>
        public override void Save(string newPath, bool isBackup = false, EngineController engine = null)
        {
            var originalPath = FileName;

            // A SaveAs to an existing function id prompts the creation of a new 
            // custom node with a new function id
            if (originalPath != newPath)
            {
                FileName = newPath;
                // If it is a newly created node, no need to generate a new guid
                if (!string.IsNullOrEmpty(originalPath))
                    CustomNodeId = Guid.NewGuid();

                // This comes after updating the Id, as if to associate the new name
                // with the new Id.
                SetInfo(Path.GetFileNameWithoutExtension(newPath));
            }

            base.Save(newPath, isBackup, engine);
        }

        protected override bool PopulateXmlDocument(XmlDocument document)
        {
            if (!base.PopulateXmlDocument(document))
                return false;

            var root = document.DocumentElement;
            if (root == null)
                return false;
            
            var guid = CustomNodeDefinition != null ? CustomNodeDefinition.FunctionId : Guid.NewGuid();
            root.SetAttribute("ID", guid.ToString());
            root.SetAttribute("Description", Description);
            root.SetAttribute("Category", Category);
            
            return true;
        }
    }
}
