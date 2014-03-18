using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Dynamo.Nodes;
using Dynamo.Utilities;
using String = System.String;

namespace Dynamo.Models
{
    public class CustomNodeWorkspaceModel : WorkspaceModel
    {
        #region Contructors

        public CustomNodeWorkspaceModel()
            : this("", "", "", new List<NodeModel>(), new List<ConnectorModel>(), 0, 0)
        {
        }

        public CustomNodeWorkspaceModel(String name, String category)
            : this(name, category, "", new List<NodeModel>(), new List<ConnectorModel>(), 0, 0)
        {
        }

        public CustomNodeWorkspaceModel(String name, String category, string description, double x, double y)
            : this(name, category, description, new List<NodeModel>(), new List<ConnectorModel>(), x, y)
        {
        }

        public CustomNodeWorkspaceModel(
            String name, String category, string description, IEnumerable<NodeModel> e, IEnumerable<ConnectorModel> c, double x, double y)
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
            get { return dynSettings.Controller.CustomNodeManager.GetDefinitionFromWorkspace(this); }
        }

        public override void Modified()
        {
            base.Modified();

            if (CustomNodeDefinition == null) 
                return;

            CustomNodeDefinition.RequiresRecalc = true;
            CustomNodeDefinition.SyncWithWorkspace(false, true);
        }

        public List<Function> GetExistingNodes()
        {
            return dynSettings.Controller.DynamoModel.AllNodes
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
                CustomNodeDefinition.AddToSearch();
                dynSettings.Controller.SearchViewModel.SearchAndUpdateResultsSync();
                CustomNodeDefinition.UpdateCustomNodeManager();
            }

            // A SaveAs to an existing function id prompts the creation of a new 
            // custom node with a new function id
            if ( doRefactor )
            {
                // if the original path does not exist
                if ( !File.Exists(originalPath) )
                {
                    CustomNodeDefinition.FunctionId = newGuid;
                    CustomNodeDefinition.SyncWithWorkspace(true, true);
                    return false;
                }

                var newDef = CustomNodeDefinition;

                // reload the original funcdef from its path
                dynSettings.CustomNodeManager.Remove(originalGuid);
                dynSettings.CustomNodeManager.AddFileToPath(originalPath);
                var origDef = dynSettings.CustomNodeManager.GetFunctionDefinition(originalGuid);
                if (origDef == null)
                {
                    return false;
                }

                // reassign existing nodes to point to newly deserialized function def
                dynSettings.Controller.DynamoModel.AllNodes
                        .OfType<Function>()
                        .Where(el => el.Definition.FunctionId == originalGuid)
                        .ToList()
                        .ForEach(node =>
                            {
                                node.Definition = origDef;
                            });

                // update this workspace with its new id
                newDef.FunctionId = newGuid;
                newDef.SyncWithWorkspace(true, true);
            }

            return true;
        }

        public override void OnDisplayed()
        {

        }

        protected override bool PopulateXmlDocument(XmlDocument document)
        {
            if (!base.PopulateXmlDocument(document))
                return false;

            Guid guid;
            if (CustomNodeDefinition != null)
            {
                guid = CustomNodeDefinition.FunctionId;
            }
            else
            {
                guid = Guid.NewGuid();
            }

            var root = document.DocumentElement;
            root.SetAttribute("ID", guid.ToString());

            return true;
        }
    }
}
