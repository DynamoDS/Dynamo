using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
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
            inputParameters = new List<Tuple<string, string>>();
            outputParameters = new List<string>();
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
            if (info.IsPackageMember)
                ElementType = ElementTypeEnum.Package;
            else
                ElementType = ElementTypeEnum.CustomNode;
        }

        protected override NodeModel ConstructNewNodeModel()
        {
            return customNodeManager.CreateCustomNodeInstance(ID);
        }

        private void TryLoadDocumentation()
        {
            if (inputParameters.Any() || outputParameters.Any())
                return;

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(Path);
                XmlNodeList elNodes = xmlDoc.GetElementsByTagName("Elements");

                if (elNodes.Count == 0)
                    elNodes = xmlDoc.GetElementsByTagName("dynElements");

                XmlNode elNodesList = elNodes[0];

                foreach (XmlNode elNode in elNodesList.ChildNodes)
                {
                    foreach (var subNode in
                        elNode.ChildNodes.Cast<XmlNode>()
                            .Where(subNode => (subNode.Name == "Symbol")))
                    {
                        var parameter = subNode.Attributes[0].Value;
                        if (parameter != string.Empty)
                        {
                            if ((subNode.ParentNode.Name == "Dynamo.Nodes.Symbol") ||
                                (subNode.ParentNode.Name == "Dynamo.Nodes.dynSymbol"))
                                inputParameters.Add(Tuple.Create(parameter, ""));

                            if ((subNode.ParentNode.Name == "Dynamo.Nodes.Output") ||
                                (subNode.ParentNode.Name == "Dynamo.Nodes.dynOutput"))
                                outputParameters.Add(parameter);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        protected override IEnumerable<Tuple<string, string>> GenerateInputParameters()
        {
            TryLoadDocumentation();

            if (!inputParameters.Any())
                inputParameters.Add(Tuple.Create("", "none"));

            return inputParameters;
        }

        protected override IEnumerable<string> GenerateOutputParameters()
        {
            TryLoadDocumentation();

            if (!outputParameters.Any())
                outputParameters.Add("none");

            return outputParameters;
        }
    }
}
