using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml;
using Dynamo.UI;
using Dynamo.Utilities;

namespace Dynamo.Search.SearchElements
{
    public class CustomNodeSearchElement : NodeSearchElement, IEquatable<CustomNodeSearchElement>
    {
        public Guid Guid { get; internal set; }

        private string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                RaisePropertyChanged("Path");
            }
        }

        public override string Type { get { return "Custom Node"; } }

        List<Tuple<string, string>> inputParameters;
        List<string> outputParameters;

        protected override List<Tuple<string, string>> GenerateInputParameters()
        {
            TryLoadDocumentation();

            if (!inputParameters.Any())
                inputParameters.Add(Tuple.Create("", "none"));

            return inputParameters;
        }

        protected override List<string> GenerateOutputParameters()
        {
            TryLoadDocumentation();

            if (!outputParameters.Any())
                outputParameters.Add("none");

            return outputParameters;
        }

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

        private void TryLoadDocumentation()
        {
            if (inputParameters != null || (outputParameters != null))
                return;

            inputParameters = new List<Tuple<string, string>>();
            outputParameters = new List<string>();

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
                            .Where(subNode =>(subNode.Name == "Symbol")))
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

        protected override BitmapSource LoadDefaultIcon(ResourceType resourceType)
        {   
            string postfix = resourceType == ResourceType.SmallIcon ?
                Configurations.SmallIconPostfix : Configurations.LargeIconPostfix;

            return GetIcon(Configurations.DefaultCustomNodeIcon + postfix);
        }
    }
}
