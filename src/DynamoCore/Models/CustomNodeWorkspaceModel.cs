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
                this.HasUnsavedChanges = true;
            }
        }

        #endregion

        public FunctionDefinition FunctionDefinition
        {
            get { return dynSettings.Controller.CustomNodeManager.GetDefinitionFromWorkspace(this); }
        }

        public override void Modified()
        {
            base.Modified();

            if (this.FunctionDefinition == null) return;
            this.FunctionDefinition.RequiresRecalc = true;
            this.FunctionDefinition.SyncWithWorkspace(false, true);
        }

        public List<Function> GetExistingNodes()
        {
            return dynSettings.Controller.DynamoModel.AllNodes
                .OfType<Function>()
                .Where(el => el.Definition == this.FunctionDefinition)
                .ToList();
        }

        public override bool SaveAs(string path)
        {
            var oldPath = this.FileName;
            if (!base.SaveAs(path)) return false;

            if (oldPath == null) this.FunctionDefinition.AddToSearch();

            // A SaveAs to an existing function id prompts the creation of a new 
            // custom node with a new function id
            if ( oldPath != path && oldPath != null )
            {
                if (!File.Exists(oldPath))
                {
                    this.FunctionDefinition.SyncWithWorkspace(true, true);
                    return false;
                }

                var newDef = this.FunctionDefinition;

                // unload existing custom node from customnodemanager
                var originalGuid = this.FunctionDefinition.FunctionId;
                var customNodeManager = dynSettings.CustomNodeManager;
                customNodeManager.RemoveFromCustomNodeManager(originalGuid);

                // get function definition from old path with old id
                customNodeManager.AddFileToPath(oldPath);
                var origDef = customNodeManager.GetFunctionDefinition(originalGuid);

                // reassign existing nodes to point to deserialized function def
                GetExistingNodes().ForEach(node =>
                    {
                        node.Definition = origDef;
                    });

                // this workspace points to itself
                newDef.FunctionId = Guid.NewGuid();
                newDef.SyncWithWorkspace(true, true);
            }

            return true;
        }

        public override void OnDisplayed()
        {

        }

        public override XmlDocument GetXml()
        {
            var doc = base.GetXml();

            Guid guid;
            if (this.FunctionDefinition != null)
            {
                guid = this.FunctionDefinition.FunctionId;
            }
            else
            {
                guid = Guid.NewGuid();
            }

            var root = doc.DocumentElement;
            root.SetAttribute("ID", guid.ToString());

            return doc;
        }
    }
}
