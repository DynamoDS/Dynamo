using System;
using System.Collections.Generic;

namespace Dynamo.Search.SearchElements
{
    public class CustomNodeSearchElement : NodeModelSearchElement, IEquatable<CustomNodeSearchElement>
    {
        public Guid Guid { get; internal set; }

        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                RaisePropertyChanged("Path");
            }
        }

        public override string Type { get { return "Custom Node"; } }
        
        public CustomNodeSearchElement(CustomNodeInfo info)
            : base(info.Name, info.Description, new List<string>())
        {
            Node = null;
            FullCategoryName = info.Category;
            Guid = info.FunctionId;
            path = info.Path;
        }

        public override NodeModelSearchElement Copy()
        {
            return
                new CustomNodeSearchElement(new CustomNodeInfo(Guid, Name, FullCategoryName,
                                                               Description, Path));
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals(obj as NodeModelSearchElement);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode() + Type.GetHashCode() + Name.GetHashCode() + Description.GetHashCode();
        }

        public bool Equals(CustomNodeSearchElement other)
        {
            return other.Guid == Guid;
        }

        public new bool Equals(NodeModelSearchElement other)
        {
            return other is CustomNodeSearchElement && Equals(other as CustomNodeSearchElement);
        }
    }
}