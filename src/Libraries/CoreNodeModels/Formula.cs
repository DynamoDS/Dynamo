﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using NCalc;
using ProtoCore;
using ProtoCore.AST.AssociativeAST;
using Expression = NCalc.Expression;

namespace DSCoreNodesUI
{
    [NodeName("Formula")]
    [NodeDescription("FormulaDescription", typeof(DSCoreNodesUI.Properties.Resources))]
    [NodeCategory(BuiltinNodeCategories.CORE_SCRIPTING)]
    [IsDesignScriptCompatible]
    public class Formula : NodeModel
    {
        private string formulaString = "";
        public string FormulaString
        {
            get
            {
                return formulaString;
            }

            set
            {
                if (formulaString == null || !formulaString.Equals(value))
                {
                    formulaString = value;
                    if (value != null)
                    {
                        ElementState oldState = State;
                        {
                            ProcessFormula();
                            RaisePropertyChanged("FormulaString");

                            OnNodeModified();
                        }

                        if (oldState != State)
                            RaisePropertyChanged("State");
                    }
                }
            }
        }

        public Formula()
        {
            ArgumentLacing = LacingStrategy.Shortest;
            OutPortData.Add(new PortData("", Properties.Resources.FormulaPortDataResultToolTip));
            RegisterAllPorts();
        }

        protected override bool UpdateValueCore(UpdateValueParams updateValueParams)
        {
            string name = updateValueParams.PropertyName;
            string value = updateValueParams.PropertyValue;

            if (name == "FormulaString")
            {
                FormulaString = value;
                return true; // UpdateValueCore handled.
            }

            return base.UpdateValueCore(updateValueParams);
        }

        #region Serialization/Deserialization methods

        protected override void SerializeCore(XmlElement element, SaveContext context)
        {
            base.SerializeCore(element, context); //Base implementation must be called
            var formStringNode = element.OwnerDocument.CreateElement("FormulaText");
            formStringNode.InnerText = FormulaString;
            element.AppendChild(formStringNode);
        }

        protected override void DeserializeCore(XmlElement nodeElement, SaveContext context)
        {
            base.DeserializeCore(nodeElement, context); //Base implementation must be called

            if (nodeElement.Attributes != null)
            {
                var formulaAttr = nodeElement.Attributes["formula"];
                if (formulaAttr != null)
                {
                    FormulaString = formulaAttr.Value;
                    return;
                }
            }

            var formStringNode = nodeElement.ChildNodes.Cast<XmlNode>().FirstOrDefault(childNode => childNode.Name == "FormulaText");
            FormulaString = formStringNode != null
                ? formStringNode.InnerText
                : nodeElement.InnerText;
        }

        #endregion

/*
        private static readonly HashSet<string> reservedFuncNames = new HashSet<string> { 
            "abs", "acos", "asin", "atan", "ceiling", "cos",
            "exp", "floor", "ieeeremainder", "log", "log10",
            "max", "min", "pow", "round", "sign", "sin", "sqrt",
            "tan", "truncate", "in", "if"
        };
*/

        private static readonly HashSet<string> reservedParamNames = new HashSet<string> {
            "pi", "π"
        };

        private void ProcessFormula()
        {
            Expression e;
            try
            {
                e = new Expression(
                    FormulaString.ToLower()
                        .Replace(" and ", "+").Replace("&&", "+")
                        .Replace(" or ", "+").Replace("||", "+"), 
                    EvaluateOptions.IgnoreCase);
            }
            catch (Exception ex)
            {
                Error(ex.Message);
                return;
            }

            if (e.HasErrors())
            {
                Error(e.Error);
                return;
            }

            var parameters = new List<string>();
            var paramSet = new HashSet<string>();

            e.EvaluateParameter += delegate(string name, ParameterArgs args)
            {
                if (!paramSet.Contains(name) && !reservedParamNames.Contains(name))
                {
                    paramSet.Add(name);
                    parameters.Add(name);
                }

                args.Result = 0;
            };

            e.EvaluateFunction += delegate(string name, FunctionArgs args)
            {
                foreach (var p in args.Parameters)
                {
                    p.Evaluate();
                }

                args.Result = 0;
            };

            try
            {
                e.Evaluate();
            }
            catch { }

            InPortData.Clear();

            foreach (var p in parameters)
            {
                InPortData.Add(new PortData(p, "variable"));
            }

            RegisterInputPorts();
            ClearRuntimeError();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            Func<string, string[], object[], object> backingMethod = DSCore.Formula.Evaluate;

            // Format input names to be used as function parameters
            var inputs = InPortData.Select(x => x.NickName.Replace(' ', '_')).ToList();


            /*  def formula_partial(<params>) {
             *    return = DSCore.Formula.Evaluate(<FormulaString>, <InPortData Names>, <params>);
             *  }
             */

            var functionDef = new FunctionDefinitionNode
            {
                Name = "__formula_" + GUID.ToString().Replace("-", string.Empty),
                Signature =
                    new ArgumentSignatureNode
                    {
                        Arguments = inputs.Select(AstFactory.BuildParamNode).ToList()
                    },
                ReturnType = TypeSystem.BuildPrimitiveTypeObject(PrimitiveType.kTypeVar),
                FunctionBody =
                    new CodeBlockNode
                    {
                        Body =
                            new List<AssociativeNode>
                                {
                                    AstFactory.BuildReturnStatement(
                                        AstFactory.BuildFunctionCall(
                                            backingMethod,
                                            new List<AssociativeNode>
                                            {
                                                AstFactory.BuildStringNode(FormulaString),
                                                AstFactory.BuildExprList(
                                                    InPortData.Select(
                                                        x =>
                                                            AstFactory.BuildStringNode(x.NickName) as
                                                            AssociativeNode).ToList()),
                                                AstFactory.BuildExprList(
                                                    inputs.Select(AstFactory.BuildIdentifier)
                                                        .Cast<AssociativeNode>()
                                                        .ToList())
                                            }))
                                }
                    }
            };

            if (IsPartiallyApplied)
            {
                return new AssociativeNode[]
                {
                    functionDef,
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                        AstFactory.BuildFunctionObject(
                            functionDef.Name,
                            InPortData.Count,
                            Enumerable.Range(0, InPortData.Count).Where(HasConnectedInput),
                            inputAstNodes))
                };
            }
            else
            {
                AppendReplicationGuides(inputAstNodes);

                return new AssociativeNode[]
                {
                    functionDef,
                    AstFactory.BuildAssignment(
                        GetAstIdentifierForOutputIndex(0),
                        AstFactory.BuildFunctionCall(
                            functionDef.Name,
                            inputAstNodes))
                };
            }
        }
    }
}
