using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Graph.Annotations;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Nodes.CustomNodes;
using Dynamo.Graph.Nodes.NodeLoaders;
using Dynamo.Graph.Notes;
using Dynamo.Graph.Presets;
using Dynamo.Interfaces;
using ProtoCore.Namespace;

namespace Dynamo.Graph.Workspaces
{
    public class CustomNodeWorkspaceModel : WorkspaceModel, ICustomNodeWorkspaceModel
    {
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

        public CustomNodeWorkspaceModel( 
            WorkspaceInfo info, 
            NodeFactory factory)
            : this(
                factory,
                Enumerable.Empty<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                Enumerable.Empty<AnnotationModel>(),
                Enumerable.Empty<PresetModel>(),
                new ElementResolver(),
                info) { }

        public CustomNodeWorkspaceModel( 
            NodeFactory factory,
            IEnumerable<NodeModel> nodes, 
            IEnumerable<NoteModel> notes, 
            IEnumerable<AnnotationModel> annotations,
            IEnumerable<PresetModel> presets,
            ElementResolver elementResolver, 
            WorkspaceInfo info)
            : base(nodes, notes,annotations, info, factory,presets, elementResolver)
        {
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

        public void SetInfo(string newName = null, string newCategory = null, string newDescription = null, string newFilename = null)
        {
            PropertyChanged -= OnPropertyChanged;
            
            Name = newName??Name;
            Category = newCategory??Category;
            Description = newDescription??Description;
            FileName = newFilename??FileName;
            
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

        protected override void RequestRun()
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

        public event Action DefinitionUpdated;
        internal virtual void OnDefinitionUpdated()
        {
            if (SilenceDefinitionUpdated) return;

            var handler = DefinitionUpdated;
            if (handler != null) handler();
        }

        public event Action<Guid> FunctionIdChanged;
        protected virtual void OnFunctionIdChanged(Guid oldId)
        {
            var handler = FunctionIdChanged;
            if (handler != null) handler(oldId);
        }

        public override bool SaveAs(string newPath, ProtoCore.RuntimeCore runtimeCore, bool isBackUp = false)
        {
            if (isBackUp)
                return base.SaveAs(newPath, runtimeCore, isBackUp);

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

            return base.SaveAs(newPath, runtimeCore, isBackUp);
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

        protected override void SerializeSessionData(XmlDocument document, ProtoCore.RuntimeCore runtimeCore)
        {
            // Since custom workspace does not have any runtime data to persist,
            // do not allow base class to serialize any session data.
        }
    }
}
