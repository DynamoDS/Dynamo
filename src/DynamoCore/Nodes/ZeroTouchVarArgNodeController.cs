using System.Collections.Generic;
using System.Linq;

using Dynamo.DSEngine;
using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

namespace Dynamo.Nodes
{
    public class ZeroTouchVarArgNodeController : ZeroTouchNodeController
    {
        public ZeroTouchVarArgNodeController(FunctionDescriptor zeroTouchDef) : base(zeroTouchDef) { }

        protected override void InitializeFunctionParameters(NodeModel model, IEnumerable<TypedParameter> parameters)
        {
            var typedParameters = parameters as IList<TypedParameter> ?? parameters.ToList();
            base.InitializeFunctionParameters(model, typedParameters.Take(typedParameters.Count() - 1));
        }

        protected override void BuildOutputAst(NodeModel model, List<AssociativeNode> inputAstNodes, List<AssociativeNode> resultAst)
        {
            // All inputs are provided, then we should pack all inputs that
            // belong to variable input parameter into a single array. 
            if (!model.IsPartiallyApplied)
            {
                var paramCount = Definition.Parameters.Count();
                var packId = "__var_arg_pack_" + model.GUID;
                resultAst.Add(
                    AstFactory.BuildAssignment(
                        AstFactory.BuildIdentifier(packId),
                        AstFactory.BuildExprList(inputAstNodes.Skip(paramCount - 1).ToList())));

                inputAstNodes =
                    inputAstNodes.Take(paramCount - 1)
                        .Concat(new[] { AstFactory.BuildIdentifier(packId) })
                        .ToList();
            }

            base.BuildOutputAst(model, inputAstNodes, resultAst);
        }
    }
}