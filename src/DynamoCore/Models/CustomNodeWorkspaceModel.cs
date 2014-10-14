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
        #region Contructors
        
        public CustomNodeWorkspaceModel(
            string name, string category, string description, double x, double y)
            : this(
                name,
                category,
                description,
                Enumerable.Empty<NodeModel>(),
                Enumerable.Empty<ConnectorModel>(),
                x,
                y) { }

        public CustomNodeWorkspaceModel(
            string name, string category, string description, IEnumerable<NodeModel> e,
            IEnumerable<ConnectorModel> c, double x, double y) 
            : base(name, e, c, x, y)
        {
            WatchChanges = true;
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
            }
        }

        #endregion

        public CustomNodeDefinition CustomNodeDefinition
        {
            get
            {
                return dynamoModel.CustomNodeManager.GetDefinitionFromWorkspace(this);
            }
        }

        protected override void OnModified()
        {
            base.OnModified();

            if (CustomNodeDefinition == null) 
                return;

            CustomNodeDefinition.RequiresRecalc = true;
            CustomNodeDefinition.SyncWithWorkspace(dynamoModel, false, true);
        }

        public List<Function> GetExistingNodes()
        {
            return dynamoModel.AllNodes
                .OfType<Function>()
                .Where(el => el.Definition == CustomNodeDefinition)
                .ToList();
        }

        public override bool SaveAs(string newPath)
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
                CustomNodeDefinition.AddToSearch(this.DynamoModel.SearchModel);
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

            Guid guid = CustomNodeDefinition != null ? CustomNodeDefinition.FunctionId : Guid.NewGuid();

            var root = document.DocumentElement;
            if (root == null)
                return false;
                
            root.SetAttribute("ID", guid.ToString());

            return true;
        }

        protected override void SerializeSessionData(XmlDocument document, object core)
        {
            // Since custom workspace does not have any runtime data to persist,
            // do not allow base class to serialize any session data.
        }
    }
}
