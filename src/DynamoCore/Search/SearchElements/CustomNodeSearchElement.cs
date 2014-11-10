using System;
using System.Collections.Generic;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Utilities;

namespace Dynamo.Search.SearchElements
{
    public class CustomNodeSource : ICustomNodeSource
    {
        private readonly CustomNodeManager manager;
        private readonly Guid customNodeId;

        public CustomNodeSource(CustomNodeManager manager, Guid customNodeId)
        {
            this.manager = manager;
            this.customNodeId = customNodeId;
        }

        public NodeModel NewInstance()
        {
            return manager.CreateCustomNodeInstance(customNodeId, TODO);
        }
    }

    public class CustomNodeSearchElement : NodeSearchElement, IEquatable<CustomNodeSearchElement>
    {
        private readonly ICustomNodeSource nodeSource;

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

        public override NodeModel GetSearchResult()
        {
            return nodeSource.NewInstance();
        }

        public CustomNodeSearchElement(CustomNodeInfo info, ICustomNodeSource nodeSource)
            : base(info.Name, info.Description, new List<string>())
        {
            this.nodeSource = nodeSource;
            Node = null;
            FullCategoryName = info.Category;
            Guid = info.Guid;
            path = info.Path;
        }

        public override NodeSearchElement Copy()
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