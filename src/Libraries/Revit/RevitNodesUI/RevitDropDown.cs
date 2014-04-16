using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using DSCoreNodesUI;
using Dynamo.Models;
using Dynamo.Nodes;
using Dynamo.Utilities;
using ProtoCore.AST.AssociativeAST;
using RevitServices.Persistence;
using Category = Revit.Elements.Category;

namespace DSRevitNodesUI
{
    [NodeName("Family Types")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("All family types available in the document.")]
    [IsDesignScriptCompatible]
    public class FamilyTypes : DSDropDownBase
    {
        private const string noFamilyTypes = "No family types available.";

        public FamilyTypes():base("Family Type"){ }
        
        protected override void PopulateItems()
        {
            Items.Clear();

            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);

            fec.OfClass(typeof(Family));
            if (fec.ToElements().Count == 0)
            {
                Items.Add(new DynamoDropDownItem(noFamilyTypes, null));
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

            Items = Items.OrderBy(x => x.Name).ToObservableCollection();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                Items[0].Name == noFamilyTypes ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            var args = new List<AssociativeNode>
            {
                AstFactory.BuildStringNode(((FamilySymbol) Items[SelectedIndex].Item).Family.Name),
                AstFactory.BuildStringNode(((FamilySymbol) Items[SelectedIndex].Item).Name)
            };

            var functionCall = AstFactory.BuildFunctionCall
                <System.String, System.String, Revit.Elements.FamilySymbol>
                (Revit.Elements.FamilySymbol.ByFamilyNameAndTypeName, args);

            return new[] {AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }

    }

    [NodeName("Floor Types")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("All floor types available in the document.")]
    [IsDesignScriptCompatible]
    public class FloorTypes : DSDropDownBase
    {
        private const string noFloorTypes = "No floor types available.";

        public FloorTypes() : base("Floor Type") { }

        protected override void PopulateItems()
        {
            Items.Clear();

            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            fec.OfClass(typeof(Autodesk.Revit.DB.FloorType));

            if (fec.ToElements().Count == 0)
            {
                Items.Add(new DynamoDropDownItem(noFloorTypes, null));
                SelectedIndex = 0;
                return;
            }

            foreach (var ft in fec.ToElements())
            {
                Items.Add(new DynamoDropDownItem(ft.Name, ft));
            }

            Items = Items.OrderBy(x => x.Name).ToObservableCollection();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 || 
                Items[0].Name == noFloorTypes ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            var args = new List<AssociativeNode>
            {
                AstFactory.BuildStringNode(((Autodesk.Revit.DB.FloorType) Items[SelectedIndex].Item).Name)
            };

            var functionCall = AstFactory.BuildFunctionCall
                <System.String, Revit.Elements.FloorType>
                (Revit.Elements.FloorType.ByName, args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("Wall Types")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("All floor types available in the document.")]
    [IsDesignScriptCompatible]
    public class WallTypes : DSDropDownBase
    {
        private const string noWallTypes = "No wall types available.";

        public WallTypes() : base("Wall Type") { }

        protected override void PopulateItems()
        {
            Items.Clear();

            var fec = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);

            fec.OfClass(typeof(Autodesk.Revit.DB.WallType));
            if (fec.ToElements().Count == 0)
            {
                Items.Add(new DynamoDropDownItem(noWallTypes, null));
                SelectedIndex = 0;
                return;
            }

            foreach (var wt in fec.ToElements())
            {
                Items.Add(new DynamoDropDownItem(wt.Name, wt));
            }

            Items = Items.OrderBy(x => x.Name).ToObservableCollection();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                Items[0].Name == noWallTypes ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            var args = new List<AssociativeNode>
            {
                AstFactory.BuildStringNode(((Autodesk.Revit.DB.WallType) Items[SelectedIndex].Item).Name)
            };
            var functionCall = AstFactory.BuildFunctionCall("Revit.Elements.WallType",
                                                            "ByName",
                                                            args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("Categories")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("All built-in categories.")]
    [IsDesignScriptCompatible]
    public class Categories : EnumBase<BuiltInCategory>
    {
        protected override void PopulateItems()
        {
            Items.Clear();
            foreach (var constant in Enum.GetValues(typeof(BuiltInCategory)))
            {
                Items.Add(new DynamoDropDownItem(constant.ToString().Substring(4), constant));
            }

            Items = Items.OrderBy(x => x.Name).ToObservableCollection();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var args = new List<AssociativeNode>
            {
                AstFactory.BuildStringNode(((BuiltInCategory) Items[SelectedIndex].Item).ToString())
            };

            var func = new Func<string, Category>(Revit.Elements.Category.ByName);
            var functionCall = AstFactory.BuildFunctionCall(func, args);

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("Levels")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a level in the active document")]
    [IsDesignScriptCompatible]
    public class Levels : DropDrownBase
    {
        private const string noLevels = "No levels available.";

        public Levels()
        {
            OutPortData.Add(new PortData("Level", "The level.", typeof(object)));

            RegisterAllPorts();

            PopulateItems();
        }

        public override void PopulateItems()
        {
            Items.Clear();

            //find all levels in the project
            var levelColl = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);
            levelColl.OfClass(typeof(Level));

            if (levelColl.ToElements().Count == 0)
            {
                Items.Add(new DynamoDropDownItem(noLevels, null));
                SelectedIndex = 0;
                return;
            }

            levelColl.ToElements().ToList().ForEach(x => Items.Add(new DynamoDropDownItem(x.Name, x)));

            Items = Items.OrderBy(x => x.Name).ToObservableCollection<DynamoDropDownItem>();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                Items[0].Name == noLevels ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            var node = AstFactory.BuildFunctionCall(
                "Revit.Elements.ElementSelector",
                "ByElementId",
                new List<AssociativeNode>
                {
                    AstFactory.BuildIntNode(((Level)Items[SelectedIndex].Item).Id.IntegerValue)
                });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("Structural Framing Types")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a level in the active document")]
    [IsDesignScriptCompatible]
    public class StructuralFramingTypes : DropDrownBase
    {
        private const string noFraming = "No structural framing types available.";

        public StructuralFramingTypes()
        {
            OutPortData.Add(new PortData("type", "The selected structural framing type.", typeof(object)));

            RegisterAllPorts();

            PopulateItems();
        }

        public override void PopulateItems()
        {
            Items.Clear();

            //find all the structural framing family types in the project
            var collector = new FilteredElementCollector(DocumentManager.Instance.CurrentDBDocument);

            var catFilter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming);
            collector.OfClass(typeof(FamilySymbol)).WherePasses(catFilter);

            if (collector.ToElements().Count == 0)
            {
                Items.Add(new DynamoDropDownItem(noFraming, null));
                SelectedIndex = 0;
                return;
            }

            foreach (var e in collector.ToElements())
                Items.Add(new DynamoDropDownItem(e.Name, e));

            Items = Items.OrderBy(x => x.Name).ToObservableCollection<DynamoDropDownItem>();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            if (Items.Count == 0 ||
                Items[0].Name == noFraming ||
                SelectedIndex == -1)
            {
                return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
            }

            var node = AstFactory.BuildFunctionCall(
                "Revit.Elements.ElementSelector",
                "ByElementId",
                new List<AssociativeNode>
                {
                    AstFactory.BuildIntNode(((Element)Items[SelectedIndex].Item).Id.IntegerValue)
                });

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("Spacing Rule Layout")]
    [NodeCategory(BuiltinNodeCategories.GEOMETRY_CURVE_DIVIDE)]
    [NodeDescription("A spacing rule layout for calculating divided paths.")]
    public class SpacingRuleLayouts : EnumAsInt<SpacingRuleLayout> { }
}
