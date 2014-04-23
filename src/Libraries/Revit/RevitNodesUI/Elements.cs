using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using Revit.Elements.InternalUtilities;

namespace DSRevitNodesUI
{
    public abstract class ElementsQueryBase : NodeModel
    {



        protected ElementsQueryBase()
        {
            var u = dynRevitSettings.Controller.Updater;
            u.ElementsAdded += Updater_ElementsAdded;
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

        private bool forceReExecuteOfNode = false;
        public override bool ForceReExecuteOfNode
        {
            get { return forceReExecuteOfNode; }
        }

        private void Updater_ElementsAdded(IEnumerable<string> updated)
        {
            forceReExecuteOfNode = true;
            RequiresRecalc = true;
        }


        protected void Updater_ElementsModified(IEnumerable<string> updated)
        {
            forceReExecuteOfNode = true;
            RequiresRecalc = true;

        }

        protected void Updater_ElementsDeleted(Document document, IEnumerable<ElementId> deleted)
        {
            forceReExecuteOfNode = true;
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
            InPortData.Add(new PortData("Family Type", "The Family Type."));
            OutPortData.Add(new PortData("Elements", "The list of elements matching the query."));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var func =
                new Func<Revit.Elements.FamilySymbol, IList<Revit.Elements.Element>>(
                    ElementQueries.OfFamilyType);

            var functionCall = AstFactory.BuildFunctionCall(func, inputAstNodes);
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
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
            InPortData.Add(new PortData("Category", "The Category"));
            OutPortData.Add(new PortData("Elements", "The list of elements matching the query."));
            
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var func =
                new Func<Revit.Elements.Category, IList<Revit.Elements.Element>>(ElementQueries.OfCategory);

            var functionCall = AstFactory.BuildFunctionCall(func, inputAstNodes);
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("All Elements at Level")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Get all the elements oatthe specified Level from the model.")]
    [IsDesignScriptCompatible]
    public class ElementsAtLevel : ElementsQueryBase
    {
        public ElementsAtLevel()
        {
            InPortData.Add(new PortData("Level", "A Level"));
            OutPortData.Add(new PortData("Elements", "Elements at the given level."));
            
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var func = new Func<Revit.Elements.Level, IList<Revit.Elements.Element>>(ElementQueries.AtLevel);
            var functionCall = AstFactory.BuildFunctionCall(func, inputAstNodes);
            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }
}
