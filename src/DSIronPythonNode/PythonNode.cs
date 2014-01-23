using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSCoreNodesUI;
using Dynamo.Controls;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.UI;
using ProtoCore.AST.AssociativeAST;

namespace DSIronPythonNode
{
    public class IronPythonEvaluator
    {
        public static object EvaluateIronPythonScript(string code, IList<Tuple<string, object>> bindings)
        {
            throw new NotImplementedException();
        }
    }

    [Browsable(false)]
    public class PythonNode : VariableInputNode
    {
        public PythonNode()
        {

        }

        public void SetupCustomUIElements(dynNodeView view)
        {
            throw new NotImplementedException();
        }

        protected override string InputRootName
        {
            get { return "IN"; }
        }

        protected override string TooltipRootName
        {
            get { return "Input #"; }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return new[] { ProtoCore.AST.AssociativeAST.AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), new NullNode()) };
        }
    }
}
