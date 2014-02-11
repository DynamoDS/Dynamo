using System;
using System.Collections.Generic;
using Dynamo.Core;
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
            Node = null;
            FullCategoryName = info.Category;
            Guid = info.Guid;
            _path = info.Path;
            EditCommand = new DelegateCommand(Edit);
        }

        public override NodeSearchElement Copy()
        {
            return
                new CustomNodeSearchElement(new CustomNodeInfo(Guid, Name, FullCategoryName,
                                                               Description, Path));
        }

        public void Edit(object _)
        {
            DynamoSettings.Controller.DynamoViewModel.GoToWorkspaceCommand.Execute(Guid);
        }

        public override void Execute()
        {
            string name = Guid.ToString();

            // create node
            var guid = Guid.NewGuid();
            DynamoSettings.Controller.DynamoViewModel.ExecuteCommand(
                new DynCmd.CreateNodeCommand(guid, name, 0, 0, true, true));

            // select node
            var placedNode = DynamoSettings.Controller.DynamoViewModel.Model.Nodes.Find((node) => node.GUID == guid);
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

            return Equals(obj as NodeSearchElement);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode() + Type.GetHashCode() + Name.GetHashCode() + Description.GetHashCode();
        }

        public bool Equals(CustomNodeSearchElement other)
        {
            return other.Guid == Guid;
        }

        public new bool Equals(NodeSearchElement other)
        {
            return other is CustomNodeSearchElement && Equals(other as CustomNodeSearchElement);
        }
    }
}
