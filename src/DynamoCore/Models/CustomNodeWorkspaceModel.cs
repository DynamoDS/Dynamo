using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using Dynamo.Nodes;

using String = System.String;

namespace Dynamo.Models
{
    public class CustomNodeWorkspaceModel : WorkspaceModel
    {
        #region Contructors

        public CustomNodeWorkspaceModel(DynamoModel dynamoModel)
            : this(dynamoModel, "", "", "", new List<NodeModel>(), new List<ConnectorModel>(), 0, 0)
        {
        }

        public CustomNodeWorkspaceModel(DynamoModel dynamoModel, String name, String category)
            : this(dynamoModel, name, category, "", new List<NodeModel>(), new List<ConnectorModel>(), 0, 0)
        {
        }

        public CustomNodeWorkspaceModel(DynamoModel dynamoModel, String name, String category, string description, double x, double y)
            : this(dynamoModel, name, category, description, new List<NodeModel>(), new List<ConnectorModel>(), x, y)
        {
        }

        public CustomNodeWorkspaceModel(DynamoModel dynamoModel, 
            String name, String category, string description, IEnumerable<NodeModel> e, IEnumerable<ConnectorModel> c, double x, double y)
            : base(dynamoModel, name, e, c, x, y)
        {
            WatchChanges = true;
            HasUnsavedChanges = false;
            Category = category;
            Description = description;

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == /*NXLT*/"Name" || args.PropertyName == /*NXLT*/"Category" || args.PropertyName == /*NXLT*/"Description")
            {
                HasUnsavedChanges = true;
            }
        }

        #endregion

        public CustomNodeDefinition CustomNodeDefinition
        {
            get { return DynamoModel.CustomNodeManager.GetDefinitionFromWorkspace(this); }
        }

        public override void Modified()
        {
            base.Modified();

            if (CustomNodeDefinition == null) 
                return;

            CustomNodeDefinition.RequiresRecalc = true;
            CustomNodeDefinition.SyncWithWorkspace(DynamoModel, false, true);
        }

        public List<Function> GetExistingNodes()
        {
            return DynamoModel.AllNodes
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
                DynamoModel.SearchModel.OnRequestSync();
                CustomNodeDefinition.UpdateCustomNodeManager(DynamoModel.CustomNodeManager);
            }

            // A SaveAs to an existing function id prompts the creation of a new 
            // custom node with a new function id
            if ( doRefactor )
            {
                // if the original path does not exist
                if ( !File.Exists(originalPath) )
                {
                    CustomNodeDefinition.FunctionId = newGuid;
                    CustomNodeDefinition.SyncWithWorkspace(DynamoModel, true, true);
                    return false;
                }

                var newDef = CustomNodeDefinition;

                // reload the original funcdef from its path
                DynamoModel.CustomNodeManager.Remove(originalGuid);
                DynamoModel.CustomNodeManager.AddFileToPath(originalPath);
                var origDef = DynamoModel.CustomNodeManager.GetFunctionDefinition(originalGuid);
                if (origDef == null)
                {
                    return false;
                }

                // reassign existing nodes to point to newly deserialized function def
                var instances = DynamoModel.AllNodes
                        .OfType<Function>()
                        .Where(el => el.Definition.FunctionId == originalGuid);

                foreach (var node in instances)
                    node.ResyncWithDefinition(origDef);

                // update this workspace with its new id
                newDef.FunctionId = newGuid;
                newDef.SyncWithWorkspace(DynamoModel, true, true);
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

            root.SetAttribute(/*NXLT*/"ID", guid.ToString());

            return true;
        }

        protected override void SerializeSessionData(XmlDocument document)
        {
            // Since custom workspace does not have any runtime data to persist,
            // do not allow base class to serialize any session data.
        }
    }
}
