using System;
using System.Collections.Generic;

using Dynamo.UI.Commands;
using Dynamo.Utilities;
using DynCmd = Dynamo.ViewModels.DynamoViewModel;

namespace Dynamo.Search.SearchElements
{
    public class CustomNodeSearchElement : NodeSearchElement, IEquatable<CustomNodeSearchElement>
    {
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

        public CustomNodeSearchElement(CustomNodeInfo info, SearchElementGroup group)
            : base(info.Name, info.Description, new List<string>(), group)
        {
            this.Node = null;
            this.FullCategoryName = info.Category;
            this.ElementType = SearchModel.ElementType.CustomNode;
            this.Guid = info.Guid;
            this._path = info.Path;
        }

        public override NodeSearchElement Copy()
        {
            var copiedNode = new CustomNodeSearchElement(new CustomNodeInfo(this.Guid, this.Name,
                this.FullCategoryName, this.Description, this.Path), Group);
            copiedNode.ElementType = this.ElementType;

            return copiedNode;
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
