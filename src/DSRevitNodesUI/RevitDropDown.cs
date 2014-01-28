using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using DSCoreNodesUI;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    /*
    public abstract class DSElementDropDown : DSDropDownBase
    {
        protected DSElementDropDown(string typeName) : base(typeName) { }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node = null;

            node = new FunctionCallNode
            {
                Function = new IdentifierNode("DSRevitNodes.Elements.ElementSelector.ByElementId"),
                FormalArguments = new List<AssociativeNode>
                {
                    new IntNode((Items[SelectedIndex].Item as ElementType).Id.IntegerValue.ToString(CultureInfo.InvariantCulture))
                }
            };

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    public abstract class DSElementsOfTypeDropDown : DSDropDownBase
    {
        protected DSElementsOfTypeDropDown(string typeName) : base(typeName) { }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            //create a document collector to get the elements in the document
            //of the type that are stored in the selected item
            var fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentDBDocument);
            fec.OfClass(Items[SelectedIndex].Item as Type);
            var els = fec.ToElements();

            AssociativeNode node = null;

            var newInputs = els.Select(el => new FunctionCallNode
            {
                Function = new IdentifierNode("DSRevitNodes.Elements.ElementSelector.ByElementId"),
                FormalArguments = new List<AssociativeNode>
                {
                    new IntNode(el.Id.IntegerValue.ToString(CultureInfo.InvariantCulture))
                }
            }).Cast<AssociativeNode>().ToList();

            node = AstFactory.BuildExprList(newInputs);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("Select Family Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a Family Type.")]
    [NodeSearchTags("family", "type")]
    [IsInteractive(true)]
    [IsDesignScriptCompatible]
    public class DSFamilyTypeSelection : DSElementDropDown
    {
        private Type internalType;

        public DSFamilyTypeSelection() : base("Family Type"){}

        protected override void PopulateItems()
        {
            Items.Clear();

            var fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentDBDocument);

            fec.OfClass(typeof(Family));
            if (fec.ToElements().Count == 0)
            {
                Items.Add(new DynamoDropDownItem("No family types available.", null));
                SelectedIndex = 0;
                return;
            }

            foreach (Family family in fec.ToElements())
            {
                foreach (FamilySymbol fs in family.Symbols)
                {
                    Items.Add(new DynamoDropDownItem(string.Format("{0}:{1}", family.Name, fs.Name), fs));
                }
            }
        }
    }

    [NodeName("Select Wall Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a Wall Type.")]
    [NodeSearchTags("wall", "type")]
    [IsInteractive(true)]
    [IsDesignScriptCompatible]
    public class DSWallTypeSelection : DSElementDropDown
    {
        public DSWallTypeSelection(): base("Wall Type"){}

        protected override void PopulateItems()
        {
            Items.Clear();

            var fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentDBDocument);

            fec.OfClass(typeof(WallType));
            if (fec.ToElements().Count == 0)
            {
                Items.Add(new DynamoDropDownItem("No wall types available.", null));
                SelectedIndex = 0;
                return;
            }

            foreach (WallType wt in fec.ToElements())
            {
                Items.Add(new DynamoDropDownItem(wt.Name, wt));
            }
        }
    }

    [NodeName("Floor Types")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a Floor Type.")]
    [NodeSearchTags("floor", "type")]
    [IsInteractive(true)]
    [IsDesignScriptCompatible]
    public class FloorTypes : DSElementDropDown
    {
        public DSFloorTypeSelection() : base("Floor Type") { }

        protected override void PopulateItems()
        {
            Items.Clear();

            var fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentDBDocument);

            fec.OfClass(typeof(FloorType));
            if (fec.ToElements().Count == 0)
            {
                Items.Add(new DynamoDropDownItem("No floor types available.", null));
                SelectedIndex = 0;
                return;
            }

            foreach (FloorType ft in fec.ToElements())
            {
                Items.Add(new DynamoDropDownItem(ft.Name, ft));
            }
        }
    }*/

    [NodeName("Element Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("All element types available in the document.")]
    [IsDesignScriptCompatible]
    public class ElementTypes : DSCoreNodesUI.AllChildrenOfType
    {
        public ElementTypes() : base(typeof(Element)) { }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            return base.BuildOutputAst(inputAstNodes);
        }
    }

    [NodeName("Family Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("All family types available in the document.")]
    [IsDesignScriptCompatible]
    public class FamilyTypes : DSDropDownBase
    {
        public FamilyTypes():base("Family Types"){ }
        
        protected override void PopulateItems()
        {
            Items.Clear();

            var fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentDBDocument);

            fec.OfClass(typeof(Family));
            if (fec.ToElements().Count == 0)
            {
                Items.Add(new DynamoDropDownItem("No family types available.", null));
                SelectedIndex = 0;
                return;
            }

            foreach (Family family in fec.ToElements())
            {
                foreach (FamilySymbol fs in family.Symbols)
                {
                    Items.Add(new DynamoDropDownItem(string.Format("{0}:{1}", family.Name, fs.Name), fs));
                }
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            throw new NotImplementedException();
        }
    }

    [NodeName("Floor Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("All floor types available in the document.")]
    [IsDesignScriptCompatible]
    public class FloorType : DSDropDownBase
    {
        public FloorType() : base("Floor Types") { }

        protected override void PopulateItems()
        {
            Items.Clear();

            var fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentDBDocument);
            fec.OfClass(typeof(FloorType));

            if (fec.ToElements().Count == 0)
            {
                Items.Add(new DynamoDropDownItem("No floor types available.", null));
                SelectedIndex = 0;
                return;
            }

            foreach (var ft in fec.ToElements())
            {
                Items.Add(new DynamoDropDownItem(ft.Name, ft));
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node = null;
            node = new FunctionCallNode
            {
                Function = new IdentifierNode("DSRevitNodes.Elements.DSElementType.ByName"),
                FormalArguments = new List<AssociativeNode>
                {
                    AstFactory.BuildStringNode(((Autodesk.Revit.DB.FloorType)Items[SelectedIndex].Item).Name)
                }
            };

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("Wall Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("All floor types available in the document.")]
    [IsDesignScriptCompatible]
    public class WallType : DSDropDownBase
    {
        public WallType() : base("Wall Types") { }

        protected override void PopulateItems()
        {
            Items.Clear();

            var fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentDBDocument);

            fec.OfClass(typeof(WallType));
            if (fec.ToElements().Count == 0)
            {
                Items.Add(new DynamoDropDownItem("No wall types available.", null));
                SelectedIndex = 0;
                return;
            }

            foreach (var wt in fec.ToElements())
            {
                Items.Add(new DynamoDropDownItem(wt.Name, wt));
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node = null;
            node = new FunctionCallNode
            {
                Function = new IdentifierNode("DSRevitNodes.Elements.DSElementType.ByName"),
                FormalArguments = new List<AssociativeNode>
                {
                    AstFactory.BuildStringNode(((Autodesk.Revit.DB.WallType)Items[SelectedIndex].Item).Name)
                }
            };

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("Category")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("All built-in categories.")]
    [IsDesignScriptCompatible]
    public class Categories : EnumBase
    {
        public Categories():base(typeof(BuiltInCategory)){}

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node = null;
            node = new FunctionCallNode
            {
                Function = new IdentifierNode("DSRevitNodes.DSCategory.ByName"),
                FormalArguments = new List<AssociativeNode>
                {
                    AstFactory.BuildStringNode(((BuiltInCategory)Items[SelectedIndex].Item).ToString())
                }
            };

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }
}
