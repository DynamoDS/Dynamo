using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.SyntaxAnalysis;
using ProtoCore.Utils;

namespace Dynamo.Engine
{
    internal class MigrateFormulaToDS : AstReplacer
    {
        private static readonly TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;

        /// <summary>
        /// Regex to match "if(<condition>, <true>, <false>)" pattern in Formula node and extract the parts of the
        /// "if" statement to construct the corresponding conditional AST in DS. This is because the DS parser does not
        /// recognize this "if" syntax.
        /// </summary>
        private const string ifCond = @"(if\s*)\(\s*(.*?)\s*\)$";
        private static readonly Regex ifRgx = new Regex(ifCond, RegexOptions.IgnoreCase);


        internal string ConvertFormulaToDS(string formula)
        {
            CodeBlockNode cbn = null;
            var match = ifRgx.Match(formula);

            List<AssociativeNode> dsAst = new List<AssociativeNode>();
            if (match.Success)
            {
                var cond = match.Groups[2].Value;
                var expArray = $"[{cond}];";

                cbn = ParserUtils.Parse(expArray);
                var asts = MigrateFormulaToCodeBlockNode(cbn.Body).ToList();
                var exprList = (asts[0] as BinaryExpressionNode).RightNode as ExprListNode;
                var condAst = exprList.Exprs[0];
                var trueAst = exprList.Exprs[1];
                var falseAst = exprList.Exprs[2];

                dsAst.Add(AstFactory.BuildConditionalNode(condAst, trueAst, falseAst));
            }
            else
            {
                cbn = ParserUtils.Parse(formula + ";");
                dsAst.AddRange(MigrateFormulaToCodeBlockNode(cbn.Body));
            }
            var codegen = new CodeGenDS(dsAst);
            return codegen.GenerateCode();
        }

        private IEnumerable<AssociativeNode> MigrateFormulaToCodeBlockNode(IEnumerable<AssociativeNode> nodes)
        {
            return nodes.Select(node => node.Accept(new MigrateFormulaToDS()));
        }

        public override AssociativeNode VisitFunctionCallNode(FunctionCallNode node)
        {
            node = base.VisitFunctionCallNode(node) as FunctionCallNode;
            var funcName = textInfo.ToTitleCase(node.Function.Name);

            if (funcName == "Sin" || funcName == "Cos" || funcName == "Tan")
            {
                funcName = $"DSCore.Math.{funcName}";
                node.Function.Name = funcName;
                (node.Function as IdentifierNode).Value = funcName;

                var arg = node.FormalArguments.FirstOrDefault();

                var newArg = AstFactory.BuildFunctionCall("DSCore.Math", "RadiansToDegrees", new List<AssociativeNode> { arg });
                node.FormalArguments = new List<AssociativeNode> { newArg };
            }
            else if (funcName == "Asin" || funcName == "Acos" || funcName == "Atan")
            {
                funcName = $"DSCore.Math.{funcName}";
                node.Function.Name = funcName;
                (node.Function as IdentifierNode).Value = funcName;

                node = AstFactory.BuildFunctionCall(
                    "DSCore.Math", "DegreesToRadians", new List<AssociativeNode> { node }) as FunctionCallNode;
            }
            else if(funcName == "Exp" || funcName == "Log10" || funcName == "Pow" || funcName == "Sqrt" || funcName == "Abs"
                || funcName == "Floor" || funcName == "Ceiling" || funcName == "Round")
            {
                funcName = $"DSCore.Math.{funcName}";
                node.Function.Name = funcName;
                (node.Function as IdentifierNode).Value = funcName;
            }
            return node;
        }
    }
}
