using Dynamo.Graph.Nodes;
using Newtonsoft.Json;
using PackingNodeModels.Pack.Validation;
using PackingNodeModels.Properties;
using ProtoCore.AST.AssociativeAST;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PackingNodeModels.Pack
{
    /// <summary>
    /// Creates a node that takes in a TypeDefinition and input values defined by the given TypeDefinition and outputs it as dictionary.
    /// </summary>
    [NodeName("Pack")]
    [NodeCategory(BuiltinNodeCategories.CORE_PACKING)]
    [NodeDescription("PackNodeDescription", typeof(Resource))]
    [OutPortNames("Out")]
    [OutPortTypes("Dictionary")]
    [OutPortDescriptions("Dictionary")]
    [IsDesignScriptCompatible]
    public class Pack : PackingNode
    {
        private List<object> cachedValues;
        private IValidationManager validationManager;

        public Pack()
        {
            validationManager = new ValidationManager(this);
        }

        /// <summary>
        /// Private constructor used for serialization.
        /// </summary>
        /// <param name="inPorts">A collection of <see cref="PortModel"/> objects.</param>
        /// <param name="outPorts">A collection of <see cref="PortModel"/> objects.</param>
        [JsonConstructor]
        protected Pack(IEnumerable<PortModel> inPorts, IEnumerable<PortModel> outPorts)
            : base(inPorts, outPorts)
        {
            validationManager = new ValidationManager(this);
        }

        protected override void RefreshTypeDefinitionPorts()
        {
            cachedValues = null;
            InPorts.Skip(1).ToList().ForEach(port => InPorts.Remove(port));

            if (TypeDefinition != null)
            {
                foreach (var property in TypeDefinition.Properties)
                {
                    InPorts.Add(new PortModel(PortType.Input, this, new PortData(property.Key, property.Value.ToString())));
                }
            }
        }

        protected override void ValidateInputs(List<object> values)
        {
            var wasInWarningState = validationManager.Warnings.Any();
            if (values.Count > 1)
            {
                var valuesByIndex = new Dictionary<int, object>();
                for (int i = 1; i < values.Count; ++i)
                {
                    if (cachedValues == null || cachedValues.Count <= i || values[i] != cachedValues[i])
                    {
                        valuesByIndex[i] = values[i];
                    }
                }

                validationManager.HandleValidation(valuesByIndex);
            }

            cachedValues = values;

            //FIXME This doesn't seem right. Is there a way to build a conditional node that would depend on validation inputs, instead of building the output twice?
            //Right now, it's ran once, then the DataBridge is invoked, which runs the validation and re-trigger the building of the ouput if warnings are gone/new.
            if (wasInWarningState != validationManager.Warnings.Any())
            {
                OnNodeModified(true);
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var baseOutput = base.BuildOutputAst(inputAstNodes).ToList();

            if (inputAstNodes == null || !IsValidInputState(inputAstNodes) || TypeDefinition == null)
            {
                baseOutput.Add(AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()));
                return baseOutput;
            }

            inputAstNodes = inputAstNodes.Skip(1).ToList();
            inputAstNodes.Add(AstFactory.BuildStringNode(TypeDefinition.Name));
            var keys = TypeDefinition.Properties.Select(input => AstFactory.BuildStringNode(input.Key) as AssociativeNode).ToList();
            keys.Add(AstFactory.BuildStringNode("typeid"));
            //FIXME Only way I found to pass in the context to the output function call so it knows how replications should work. 
            //Maybe a tuple of keys,context would be better?
            var isCollectionInputs = TypeDefinition.Properties.Select(input => AstFactory.BuildBooleanNode(input.Value.IsCollection) as AssociativeNode).ToList();

            var functionCall = AstFactory.BuildFunctionCall(
                new Func<List<string>, List<bool>, object, object>(DSCore.PackFunctions.PackOutputAsDictionary),
                new List<AssociativeNode> { AstFactory.BuildExprList(keys), AstFactory.BuildExprList(isCollectionInputs), AstFactory.BuildExprList(inputAstNodes) });

            baseOutput.Add(AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall));

            return baseOutput;
        }
    }
}
