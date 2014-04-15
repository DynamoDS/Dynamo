using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;

namespace DSRevitNodesUI
{
    public abstract class ElementsQueryBase : NodeModel
    {
        protected ElementsQueryBase()
        {
            var u = dynRevitSettings.Controller.Updater;
            u.ElementsModified += Updater_ElementsModified;
            u.ElementsDeleted += Updater_ElementsDeleted;
        }

        public override void Destroy()
        {
            base.Destroy();

            var u = dynRevitSettings.Controller.Updater;
            u.ElementsModified -= Updater_ElementsModified;
            u.ElementsDeleted -= Updater_ElementsDeleted;
        }

        protected void Updater_ElementsModified(IEnumerable<string> updated)
        {
            RequiresRecalc = true;
        }

        protected void Updater_ElementsDeleted(Document document, IEnumerable<ElementId> deleted)
        {
            RequiresRecalc = true;
        }
    }

    [NodeName("All Elements of Family Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Get all elements of the specified family type from the model.")]
    [IsDesignScriptCompatible]
    public class ElementsOfFamilyType : ElementsQueryBase
    {
        public ElementsOfFamilyType()
        {
            InPortData.Add(new PortData("Family Type", "The Family Type.", typeof(object)));
            OutPortData.Add(new PortData("Elements", "The list of elements matching the query.", typeof(object)));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var functionCall = AstFactory.BuildFunctionCall("ElementQueries", "OfFamilyType", inputAstNodes);

            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall)
            };
        }
    }

    [NodeName("All Elements of Category")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Get all elements of the specified category from the model.")]
    [IsDesignScriptCompatible]
    public class ElementsOfCategory : ElementsQueryBase
    {
        public ElementsOfCategory()
        {
            InPortData.Add(new PortData("Category", "The Category", typeof(object)));
            OutPortData.Add(new PortData("Elements", "The list of elements matching the query.", typeof(object)));
            
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var functionCall = AstFactory.BuildFunctionCall("ElementQueries", "OfCategory", inputAstNodes);

            return new[]
            {
                AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall)
            };
        }
    }
}
