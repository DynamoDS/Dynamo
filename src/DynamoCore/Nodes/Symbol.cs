using System.Linq;
using System.Xml;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    [NodeName("Input")]
    [NodeCategory(BuiltinNodeCategories.CORE_INPUT)]
    [NodeDescription("A function parameter, use with custom nodes")]
    [NodeSearchTags("variable", "argument", "parameter")]
    [IsInteractive(false)]
    [IsDesignScriptCompatible]
    public partial class Symbol : NodeModel
    {
        public Symbol()
        {
            OutPortData.Add(new PortData("", "Symbol", typeof(object)));

            RegisterAllPorts();

            ArgumentLacing = LacingStrategy.Disabled;
        }

        public override bool RequiresRecalc
        {
            get { return false; }
            set { }
        }

        private string _inputSymbol = "";

        public string InputSymbol
        {
            get { return _inputSymbol; }
            set
            {
                _inputSymbol = value;
                ReportModification();
                RaisePropertyChanged("InputSymbol");
            }
        }

        public override IdentifierNode GetAstIdentifierForOutputIndex(int outputIndex)
        {
            return string.IsNullOrEmpty(InputSymbol) ? AstIdentifierForPreview : AstFactory.BuildIdentifier(InputSymbol);
        }

#if !USE_DSENGINE

        protected internal override INode Build(
            Dictionary<NodeModel, Dictionary<int, INode>> preBuilt, int outPort)
        {
            Dictionary<int, INode> result;
            if (!preBuilt.TryGetValue(this, out result))
            {
                result = new Dictionary<int, INode>();
                result[outPort] = new SymbolNode(GUID.ToString());
                preBuilt[this] = result;
            }
            return result[outPort];
        }

#endif

        protected override void SaveNode(
            XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //Debug.WriteLine(pd.Object.GetType().ToString());
            var outEl = xmlDoc.CreateElement("Symbol");
            outEl.SetAttribute("value", InputSymbol);
            nodeElement.AppendChild(outEl);
        }

        protected override void LoadNode(XmlNode nodeElement)
        {
            foreach (var subNode in nodeElement.ChildNodes.Cast<XmlNode>().Where(subNode => subNode.Name == "Symbol"))
            {
                InputSymbol = subNode.Attributes[0].Value;
            }

            ArgumentLacing = LacingStrategy.Disabled;
        }
    }
}