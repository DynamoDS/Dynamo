using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;

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

        private XmlNode xmlNode;

        protected override List<Tuple<string, string>> GenerateInputParameters()
        {
            List<Tuple<string, string>> inputPar = new List<Tuple<string, string>>();
            List<string> inputs = new List<string>();

            if (xmlNode == null)
            {
                xmlNode = TryToLoadXmlNode();

                // If we couldn't load xml with information about node, just return none.
                if (xmlNode == null)
                {
                    inputPar.Add(Tuple.Create("", "none"));
                    return inputPar;
                }
            }

            foreach (XmlNode elNode in xmlNode.ChildNodes)
            {
                foreach (var subNode in
                    elNode.ChildNodes.Cast<XmlNode>()
                        .Where(subNode =>
                            ((subNode.Name == "Symbol") && 
                            (subNode.ParentNode.Name == "Dynamo.Nodes.Symbol")||
                            (subNode.ParentNode.Name == "Dynamo.Nodes.dynSymbol"))))
                {
                    inputs.Add(subNode.Attributes[0].Value);
                }
            }

            foreach (var parameter in inputs)
            {
                if (parameter != string.Empty)
                    inputPar.Add(Tuple.Create(parameter, ""));
            }

            return inputPar;
        }

        protected override List<string> GenerateOutputParameters()
        {
            List<string> outputPar = new List<string>();

            if (xmlNode == null)
            {
                xmlNode = TryToLoadXmlNode();

                // If we couldn't load xml with information about node, just return none.
                if (xmlNode == null)
                {
                    outputPar.Add("none");
                    return outputPar;
                }
            }

            foreach (XmlNode elNode in xmlNode.ChildNodes)
            {
                foreach (var subNode in
                    elNode.ChildNodes.Cast<XmlNode>()
                        .Where(subNode =>
                            ((subNode.Name == "Symbol") &&
                            (subNode.ParentNode.Name == "Dynamo.Nodes.Output") ||
                            (subNode.ParentNode.Name == "Dynamo.Nodes.dynOutput"))))
                {
                    if (subNode.Attributes[0].Value != string.Empty)
                        outputPar.Add(subNode.Attributes[0].Value);
                }
            }

            return outputPar;
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

        private XmlNode TryToLoadXmlNode()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(Path);
                XmlNodeList elNodes = xmlDoc.GetElementsByTagName("Elements");

                if (elNodes.Count == 0)
                    elNodes = xmlDoc.GetElementsByTagName("dynElements");

                XmlNode elNodesList = elNodes[0];
                return elNodesList;
            }
            catch
            {
                return null;
            }
        }

    }
}
