using System;
using System.Collections.Generic;
using System.Xml;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    [NodeName("Output")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A function output, use with custom nodes")]
    [IsInteractive(false)]
    [IsDesignScriptCompatible]
    public partial class Output : NodeModel
    {
        public Output()
        {
            InPortData.Add(new PortData("", "", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override bool RequiresRecalc
        {
            get { return false; }
            set { }
        }

        private string _symbol = "";

        public string Symbol
        {
            get { return _symbol; }
            set
            {
                _symbol = value;
                ReportModification();
                RaisePropertyChanged("Symbol");
            }
        }

        public override IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            if (outputIndex < 0 || outputIndex > OutPortData.Count)
                throw new ArgumentOutOfRangeException("outputIndex", @"Index must correspond to an OutPortData index.");

            return AstIdentifierForPreview;
        }

        internal override IEnumerable<AssociativeNode> BuildAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode assignment;
            if (null == inputAstNodes || inputAstNodes.Count == 0)
                assignment = AstFactory.BuildAssignment(AstIdentifierForPreview, AstFactory.BuildNullNode());
            else
                assignment = AstFactory.BuildAssignment(AstIdentifierForPreview, inputAstNodes[0]);

            return new AssociativeNode[] { assignment };
        }

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            XmlElement outEl = xmlDoc.CreateElement("Symbol");
            outEl.SetAttribute("value", Symbol);
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (XmlNode subNode in nodeElement.ChildNodes)
            {
                if (subNode.Name == "Symbol")
                {
                    Symbol = subNode.Attributes[0].Value;
                }
            }

            ArgumentLacing = LacingStrategy.Disabled;
        }
    }
}