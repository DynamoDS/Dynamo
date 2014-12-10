using System.Collections.Generic;
using System.Web;
using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    public abstract class String : BasicInteractive<string>
    {
        protected String(WorkspaceModel workspace) : base(workspace) { }

        public override string PrintExpression()
        {
            return "\"" + base.PrintExpression() + "\"";
        }

        protected override bool UpdateValueCore(string name, string value)
        {
            if (name == "Value")
            {
                Value = HttpUtility.HtmlEncode(value);
                return true;
            }

            return base.UpdateValueCore(name, value);
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildStringNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }

        protected override string DeserializeValue(string val)
        {
            return val;
        }

        protected override string SerializeValue()
        {
            return this.Value;
        }
    }
}