using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.DSEngine;
using Dynamo.Library;
using Dynamo.Nodes;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Models
{
    public class CustomNodeWorkspaceModel : WorkspaceModel
    {
        [Obsolete("No longer supported.", true)]
        private DynamoModel dynamoModel;

        public Guid CustomNodeId { get; private set; }

        #region Contructors
        
        public CustomNodeWorkspaceModel(
            string name, string category, string description, double x, double y, Guid customNodeId)
            : this(
                name,
                category,
                description,
                Enumerable.Empty<NodeModel>(),
                Enumerable.Empty<ConnectorModel>(),
                x,
                y, customNodeId) { }

        public CustomNodeWorkspaceModel(
            string name, string category, string description, IEnumerable<NodeModel> e,
            IEnumerable<ConnectorModel> c, double x, double y, Guid customNodeId) 
            : base(name, e, c, x, y)
        {
            CustomNodeId = customNodeId;
            //WatchChanges = true;
            HasUnsavedChanges = false;
            Category = category;
            Description = description;

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Name" || args.PropertyName == "Category" || args.PropertyName == "Description")
            {
                HasUnsavedChanges = true;
                OnInfoChanged();
            }
        }

        #endregion

        /// <summary>
        /// TODO
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
        /// TODO
        /// </summary>
        public CustomNodeDefinition CustomNodeDefinition
        {
            get
            {
                return new CustomNodeDefinition(CustomNodeId, Name, Nodes);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        public CustomNodeInfo CustomNodeInfo
        {
            get
            {
                return new CustomNodeInfo(CustomNodeId, Name, Category, Description, FileName);
            }
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

        public event Action InfoChanged;
        protected virtual void OnInfoChanged()
        {
            Action handler = InfoChanged;
            if (handler != null) handler();
        }

        [Obsolete("No longer supported.", true)]
        public List<Function> GetExistingNodes()
        {
            return dynamoModel.AllNodes
                .OfType<Function>()
                .Where(el => el.Definition == CustomNodeDefinition)
                .ToList();
        }

        public override bool SaveAs(string newPath, ProtoCore.Core core)
        {
            var originalPath = FileName;
            var originalGuid = CustomNodeDefinition.FunctionId;
            var newGuid = Guid.NewGuid();
            var doRefactor = originalPath != newPath && originalPath != null;

            Name = Path.GetFileNameWithoutExtension(newPath);

            // need to do change the function id temporarily so saved file is correct
            if (doRefactor)
            {
                CustomNodeDefinition.FunctionId = newGuid;
            }

            if (!base.SaveAs(newPath))
            {
                return false;
            }

            if (doRefactor)
            {
                CustomNodeDefinition.FunctionId = originalGuid;
            }

            if (originalPath == null)
            {
                CustomNodeDefinition.AddToSearch(dynamoModel.SearchModel);
                dynamoModel.SearchModel.OnRequestSync();
                CustomNodeDefinition.UpdateCustomNodeManager(dynamoModel.CustomNodeManager);
            }

            // A SaveAs to an existing function id prompts the creation of a new 
            // custom node with a new function id
            if ( doRefactor )
            {
                // if the original path does not exist
                if ( !File.Exists(originalPath) )
                {
                    CustomNodeDefinition.FunctionId = newGuid;
                    CustomNodeDefinition.SyncWithWorkspace(dynamoModel, true, true);
                    return false;
                }

                var newDef = CustomNodeDefinition;

                // reload the original funcdef from its path
                dynamoModel.CustomNodeManager.Remove(originalGuid);
                dynamoModel.CustomNodeManager.AddFileToPath(originalPath);
                var origDef = dynamoModel.CustomNodeManager.GetFunctionDefinition(originalGuid);
                if (origDef == null)
                {
                    return false;
                }

                // reassign existing nodes to point to newly deserialized function def
                var instances = dynamoModel.AllNodes
                        .OfType<Function>()
                        .Where(el => el.Definition.FunctionId == originalGuid);

                foreach (var node in instances)
                    node.ResyncWithDefinition(origDef);

                // update this workspace with its new id
                newDef.FunctionId = newGuid;
                newDef.SyncWithWorkspace(dynamoModel, true, true);
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
