using System;
using System.Collections.Generic;
using System.Globalization;

using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    public abstract class Double : BasicInteractive<double>
    {
        protected Double(WorkspaceModel workspace) : base(workspace) { }

        public override bool IsConvertible
        {
            get { return true; }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildDoubleNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }

        protected override double DeserializeValue(string val)
        {
            try
            {
                return Convert.ToDouble(val, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        protected override string SerializeValue()
        {
            return Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}