using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Dynamo.Utilities;

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

        public override void Modified()
        {
            base.Modified();

            //add a check if any loaded defs match this workspace
            // unnecessary given the next lines --SJE
            //if (dynSettings.Controller.CustomNodeManager.GetLoadedDefinitions().All(x => x.Workspace != this))
            //    return;

            var def =
                dynSettings.Controller.CustomNodeManager
                           .GetLoadedDefinitions()
                           .FirstOrDefault(x => x.WorkspaceModel == this);

            if (def == null) return;

            def.RequiresRecalc = true;

            try
            {
               def.Save(false, true);
            }
            catch { }
        }

        public override bool SaveAs(string path)
        {
            if (String.IsNullOrEmpty(path)) return false;

            var def = dynSettings.Controller.CustomNodeManager.GetDefinitionFromWorkspace(this);
            if (def == null) return false;
            
            // check if FilePath and path differ and FilePath is not null
            def.WorkspaceModel.FileName = path;

            if (def != null)
            {
                def.Save(true);
                this.FileName = path;
            }

            return true;
        }

        public override void OnDisplayed()
        {

        }
    }
}
