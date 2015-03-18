using System.Collections.Generic;
using System.Web;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    public abstract class String : BasicInteractive<string>
    {
        public override string PrintExpression()
        {
            return "\"" + base.PrintExpression() + "\"";
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;
            if (name == "Value")
            {
                Value = value; 
                return true;
            }

            return base.UpdateValueCore(updateValueParams);
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