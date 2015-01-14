using System;
using Dynamo.Interfaces;
using Dynamo.Models;

namespace Dynamo.Search.SearchElements
{
    /// <summary>
    ///     Search element for custom nodes.
    /// </summary>
    public class CustomNodeSearchElement : NodeSearchElement
    {
        private readonly ICustomNodeSource customNodeManager;
        public Guid ID { get; private set; }
        private string path;

        /// <summary>
        ///     Path to this custom node in disk, used in the Edit context menu.
        /// </summary>
        public string Path
        {
            get { return path; }
            private set
            {
                if (value == path) return;
                path = value;
                OnPropertyChanged("Path");
            }
        }

        public CustomNodeSearchElement(ICustomNodeSource customNodeManager, CustomNodeInfo info)
        {
            this.customNodeManager = customNodeManager;
            SyncWithCustomNodeInfo(info);
        }

        /// <summary>
        ///     Updates the properties of this search element.
        /// </summary>
        /// <param name="info"></param>
        public void SyncWithCustomNodeInfo(CustomNodeInfo info)
        {
            ID = info.FunctionId;
            Name = info.Name;
            FullCategoryName = info.Category;
            Description = info.Description;
            Path = info.Path;
        }

        protected override NodeModel ConstructNewNodeModel()
        {
            return customNodeManager.CreateCustomNodeInstance(ID);
        }
    }
}