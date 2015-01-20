using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Nodes;

namespace Dynamo.Models
{
    public class CustomNodeWorkspaceModel : WorkspaceModel
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
            string name, string category, string description, double x, double y, Guid customNodeId,
            NodeFactory factory)
            : this(
                name,
                category,
                description,
                factory,
                Enumerable.Empty<NodeModel>(),
                Enumerable.Empty<NoteModel>(),
                x,
                y,
                customNodeId) { }

        public CustomNodeWorkspaceModel(
            string name, string category, string description, NodeFactory factory, IEnumerable<NodeModel> e, IEnumerable<NoteModel> n, double x, double y, Guid customNodeId) 
            : base(name, e, n, x, y, factory)
        {
            CustomNodeId = customNodeId;
            HasUnsavedChanges = false;
            Category = category;
            Description = description;

            PropertyChanged += OnPropertyChanged;
        }

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

        #endregion

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
                return new CustomNodeInfo(CustomNodeId, Name, Category, Description, FileName);
            }
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

        public override void OnAstUpdated()
        {
            base.OnAstUpdated();
            HasUnsavedChanges = true;
            OnDefinitionUpdated();
        }

        public event Action InfoChanged;
        protected virtual void OnInfoChanged()
        {
            Action handler = InfoChanged;
            if (handler != null) handler();
        }

        public event Action DefinitionUpdated;
        protected virtual void OnDefinitionUpdated()
        {
            var handler = DefinitionUpdated;
            if (handler != null) handler();
        }

        public event Action<Guid> FunctionIdChanged;
        protected virtual void OnFunctionIdChanged(Guid oldId)
        {
            var handler = FunctionIdChanged;
            if (handler != null) handler(oldId);
        }

        public override bool SaveAs(string newPath, ProtoCore.Core core)
        {
            var originalPath = FileName;

            if (!base.SaveAs(newPath, core))
                return false;

            // A SaveAs to an existing function id prompts the creation of a new 
            // custom node with a new function id
            if (originalPath != newPath)
            {
                CustomNodeId = Guid.NewGuid();

                // This comes after updating the Id, as if to associate the new name
                // with the new Id.
                SetInfo(Path.GetFileNameWithoutExtension(newPath));
            }

            return true;
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

        protected override void SerializeSessionData(XmlDocument document, ProtoCore.Core core)
        {
            // Since custom workspace does not have any runtime data to persist,
            // do not allow base class to serialize any session data.
        }
    }
}
