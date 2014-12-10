using System;
using System.Collections.Generic;
using System.Globalization;

using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

namespace DSCoreNodesUI
{
    public abstract class Integer : BasicInteractive<int>
    {
        protected Integer(WorkspaceModel workspace) : base(workspace) { }

        protected override int DeserializeValue(string val)
        {
            try
            {
                return Convert.ToInt32(val, CultureInfo.InvariantCulture);
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

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var rhs = AstFactory.BuildIntNode(Value);
            var assignment = AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), rhs);

            return new[] { assignment };
        }
    }
}