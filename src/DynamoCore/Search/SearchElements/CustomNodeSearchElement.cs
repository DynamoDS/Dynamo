using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dynamo.Selection;
using Dynamo.UI.Commands;
using Dynamo.Utilities;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.Search.SearchElements
{
    public class CustomNodeSearchElement : NodeSearchElement, IEquatable<CustomNodeSearchElement>
    {
        public DelegateCommand EditCommand { get; set; }

        public Guid Guid { get; internal set; }

        private string _path;
        public string Path
        {
            get { return _path; }
            set { 
                _path = value; 
                RaisePropertyChanged("Path"); 
            }
        }

        public override string Type { get { return "Custom Node"; } }

        public CustomNodeSearchElement(CustomNodeInfo info) : base(info.Name, info.Description, new List<string>())
        {
            this.Node = null;
            this.FullCategoryName = info.Category;
            this.Guid = info.Guid;
            this._path = info.Path;
            this.EditCommand = new DelegateCommand(Edit);
        }

        public override NodeSearchElement Copy()
        {
            return
                new CustomNodeSearchElement(new CustomNodeInfo(this.Guid, this.Name, this.FullCategoryName,
                                                               this.Description, this.Path));
        }

        public void Edit(object _)
        {
            dynSettings.Controller.DynamoViewModel.GoToWorkspaceCommand.Execute(this.Guid);
        }

        public override void Execute()
        {
            string name = this.Guid.ToString();

            // create node
            var guid = Guid.NewGuid();
            dynSettings.Controller.DynamoViewModel.ExecuteCommand(
                new DynCmd.CreateNodeCommand(guid, name, 0, 0, true, true));

            // select node
            var placedNode = dynSettings.Controller.DynamoViewModel.Model.Nodes.Find((node) => node.GUID == guid);
            if (placedNode != null)
            {
                DynamoSelection.Instance.ClearSelection();
                DynamoSelection.Instance.Selection.Add(placedNode);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals(obj as NodeSearchElement);
        }

        public override int GetHashCode()
        {
            return this.Guid.GetHashCode() + this.Type.GetHashCode() + this.Name.GetHashCode() + this.Description.GetHashCode();
        }

        public bool Equals(CustomNodeSearchElement other)
        {
            return other.Guid == this.Guid;
        }

        public new bool Equals(NodeSearchElement other)
        {
            return other is CustomNodeSearchElement && this.Equals(other as CustomNodeSearchElement);
        }
    }
}
