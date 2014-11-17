using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Structure;

using Dynamo.Applications.Models;
using Dynamo.Models;
using Dynamo.Nodes;

using ProtoCore.AST.AssociativeAST;

using Revit.Elements;
using Revit.Elements.InternalUtilities;

using RevitServices.Elements;
using RevitServices.Persistence;

using Category = Revit.Elements.Category;
using CurveElement = Autodesk.Revit.DB.CurveElement;
using DividedSurface = Autodesk.Revit.DB.DividedSurface;
using Element = Autodesk.Revit.DB.Element;
using FamilySymbol = Revit.Elements.FamilySymbol;
using Level = Revit.Elements.Level;
using ModelText = Autodesk.Revit.DB.ModelText;
using ReferencePlane = Autodesk.Revit.DB.ReferencePlane;
using ReferencePoint = Autodesk.Revit.DB.ReferencePoint;

namespace DSRevitNodesUI
{
    public abstract class ElementsQueryBase : RevitNodeModel
    {
        protected ElementsQueryBase(WorkspaceModel workspaceModel) : base(workspaceModel)
        {
            var u = RevitDynamoModel.RevitServicesUpdater;
            u.ElementsAdded += Updater_ElementsAdded;
            u.ElementsModified += Updater_ElementsModified;
            u.ElementsDeleted += Updater_ElementsDeleted;
        }

        public override void Destroy()
        {
            base.Destroy();

            var u = RevitDynamoModel.RevitServicesUpdater;
            u.ElementsModified -= Updater_ElementsModified;
            u.ElementsDeleted -= Updater_ElementsDeleted;
        }

        private bool forceReExecuteOfNode;

        public override bool ForceReExecuteOfNode
        {
            get { return forceReExecuteOfNode; }
        }

        protected virtual void Updater_ElementsAdded(IEnumerable<string> updated)
        {
            if (!updated.Any()) return;

#if DEBUG
            Debug.WriteLine("There are {0} updated elements", updated.Count());
            DebugElements(updated);
#endif
            forceReExecuteOfNode = true;
            RequiresRecalc = true;
        }


        protected virtual void Updater_ElementsModified(IEnumerable<string> updated)
        {
            if (!updated.Any()) return;
#if DEBUG
            Debug.WriteLine("There are {0} modified elements", updated.Count());
            DebugElements(updated);
#endif
            forceReExecuteOfNode = true;
            RequiresRecalc = true;

        }

        protected virtual void Updater_ElementsDeleted(Document document, IEnumerable<ElementId> deleted)
        {
            if (!deleted.Any()) return;
#if DEBUG
            Debug.WriteLine("There are {0} deleted elements", deleted.Count());
            DebugElements(deleted);
#endif
            forceReExecuteOfNode = true;
            RequiresRecalc = true;

        }

        private static void DebugElements(IEnumerable<string> updated)
        {
            var els = updated.Select(
                id => DocumentManager.Instance.CurrentDBDocument.GetElement(id));
            foreach (var el in els.Where(el => el != null))
                Debug.WriteLine(string.Format("\t{0}", el.Name));
        }

        private static void DebugElements(IEnumerable<ElementId> updated)
        {
            var els = updated.Select(
                id => DocumentManager.Instance.CurrentDBDocument.GetElement(id));
            foreach (var el in els.Where(el => el != null)) {
                Debug.WriteLine(string.Format("\t{0}", el.Name));
            }
        }

        protected override bool ShouldDisplayPreviewCore()
        {
            return false; // Previews are not shown for this node type.
        }
    }

    [NodeName("All Elements of Family Type"), NodeCategory(BuiltinNodeCategories.REVIT_SELECTION),
     NodeDescription("Get all elements of the specified family type from the model."),
     IsDesignScriptCompatible]
    public class ElementsOfFamilyType : ElementsQueryBase
    {
        public ElementsOfFamilyType(WorkspaceModel workspaceModel) : base(workspaceModel)
        {
            InPortData.Add(new PortData("Family Type", "The Family Type."));
            OutPortData.Add(new PortData("Elements", "The list of elements matching the query."));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            var func =
                new Func<FamilySymbol, IList<Revit.Elements.Element>>(ElementQueries.OfFamilyType);

            var functionCall = AstFactory.BuildFunctionCall(func, inputAstNodes);
            return new[]
            { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("All Elements of Type"), NodeCategory(BuiltinNodeCategories.REVIT_SELECTION),
     NodeDescription("All elements in the active document of a given type."),
     IsDesignScriptCompatible]
    public class ElementsOfType : ElementsQueryBase
    {
        public ElementsOfType(WorkspaceModel workspaceModel) : base(workspaceModel)
        {
            InPortData.Add(new PortData("element type", "An element type."));
            OutPortData.Add(
                new PortData("elements", "All elements in the active document of a given type."));
            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            var func = new Func<Type, IList<Revit.Elements.Element>>(ElementQueries.OfElementType);

            var functionCall = AstFactory.BuildFunctionCall(func, inputAstNodes);
            return new[]
            { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("All Elements of Category"), NodeCategory(BuiltinNodeCategories.REVIT_SELECTION),
     NodeDescription("Get all elements of the specified category from the model."),
     IsDesignScriptCompatible]
    public class ElementsOfCategory : ElementsQueryBase
    {
        public ElementsOfCategory(WorkspaceModel workspaceModel) : base(workspaceModel)
        {
            InPortData.Add(new PortData("Category", "The Category"));
            OutPortData.Add(new PortData("Elements", "The list of elements matching the query."));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            var func = new Func<Category, IList<Revit.Elements.Element>>(ElementQueries.OfCategory);

            var functionCall = AstFactory.BuildFunctionCall(func, inputAstNodes);
            return new[]
            { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("All Elements at Level"), NodeCategory(BuiltinNodeCategories.REVIT_SELECTION),
     NodeDescription("Get all the elements at the specified Level from the model."),
     IsDesignScriptCompatible]
    public class ElementsAtLevel : ElementsQueryBase
    {
        public ElementsAtLevel(WorkspaceModel workspaceModel) : base(workspaceModel)
        {
            InPortData.Add(new PortData("Level", "A Level"));
            OutPortData.Add(new PortData("Elements", "Elements at the given level."));

            RegisterAllPorts();
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            var func = new Func<Level, IList<Revit.Elements.Element>>(ElementQueries.AtLevel);
            var functionCall = AstFactory.BuildFunctionCall(func, inputAstNodes);
            return new[]
            { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), functionCall) };
        }
    }

    [NodeName("All Elements In Active View"), NodeCategory(BuiltinNodeCategories.REVIT_VIEW),
     NodeDescription("Get all the elements which are visible in the active view."),
     IsDesignScriptCompatible]
    public class ElementsInView : RevitNodeModel
    {
        private Document doc;
        private HashSet<ElementId> elementIds = new HashSet<ElementId>();
        private HashSet<string> uniqueIds = new HashSet<string>();

        public ElementsInView(WorkspaceModel workspaceModel) : base(workspaceModel)
        {
            OutPortData.Add(new PortData("elements", "All visible elements in the active view."));
            RegisterAllPorts();

            DocumentManager.Instance.CurrentUIApplication.ViewActivated +=
                RevitDynamoModel_RevitDocumentChanged;
            RevitDynamoModel.RevitDocumentChanged += RevitDynamoModel_RevitDocumentChanged;
            RevitDynamoModel.RevitServicesUpdater.ElementsDeleted +=
                RevitServicesUpdaterOnElementsDeleted;
            RevitDynamoModel.RevitServicesUpdater.ElementsModified +=
                RevitServicesUpdaterOnElementsModified;
            RevitDynamoModel.RevitServicesUpdater.ElementsAdded +=
                RevitServicesUpdaterOnElementsAdded;

            RevitDynamoModel_RevitDocumentChanged(null, null);
        }

        public override void Destroy()
        {
            base.Destroy();
            DocumentManager.Instance.CurrentUIApplication.ViewActivated -=
                RevitDynamoModel_RevitDocumentChanged;
            RevitDynamoModel.RevitDocumentChanged -= RevitDynamoModel_RevitDocumentChanged;
            RevitDynamoModel.RevitServicesUpdater.ElementsDeleted -=
                RevitServicesUpdaterOnElementsDeleted;
            RevitDynamoModel.RevitServicesUpdater.ElementsModified -=
                RevitServicesUpdaterOnElementsModified;
            RevitDynamoModel.RevitServicesUpdater.ElementsAdded -=
                RevitServicesUpdaterOnElementsAdded;
        }

        private void RevitServicesUpdaterOnElementsAdded(IEnumerable<string> updated)
        {
            bool recalc = false;
            foreach (var id in updated)
            {
                Element e;
                if (doc.TryGetElement(id, out e))
                {
                    uniqueIds.Add(id);
                    elementIds.Add(e.Id);
                    recalc = true;
                }
            }
            if (recalc)
            {
                ForceReExecuteOfNode = true;
                RequiresRecalc = true;
            }
        }

        #region Get Visible Elements

        public static IList<Element> GetElementsVisibleInActiveView()
        {
            var fec = new FilteredElementCollector(
                DocumentManager.Instance.CurrentDBDocument,
                DocumentManager.Instance.CurrentDBDocument.ActiveView.Id);

            return
                fec.WherePasses(GetVisibleElementFilter())
                    .WhereElementIsNotElementType()
                    .ToElements();
        }

        private static ElementFilter GetVisibleElementFilter()
        {
            var filterList = new List<ElementFilter>();

            var fContinuousRail = new ElementClassFilter(typeof(ContinuousRail));
            var fRailing = new ElementClassFilter(typeof(Railing));
            var fStairs = new ElementClassFilter(typeof(Stairs));
            var fStairsLanding = new ElementClassFilter(typeof(StairsLanding));
            var fTopographySurface = new ElementClassFilter(typeof(TopographySurface));
            var fAssemblyInstance = new ElementClassFilter(typeof(AssemblyInstance));
            var fBaseArray = new ElementClassFilter(typeof(BaseArray));
            var fBeamSystem = new ElementClassFilter(typeof(BeamSystem));
            var fBoundaryConditions = new ElementClassFilter(typeof(BoundaryConditions));
            var fConnectorElement = new ElementClassFilter(typeof(ConnectorElement));
            var fControl = new ElementClassFilter(typeof(Control));
            var fCurveElement = new ElementClassFilter(typeof(CurveElement));
            var fDividedSurface = new ElementClassFilter(typeof(DividedSurface));
            var fCableTrayConduitRunBase = new ElementClassFilter(typeof(CableTrayConduitRunBase));
            var fHostObject = new ElementClassFilter(typeof(HostObject));
            var fInstance = new ElementClassFilter(typeof(Instance));
            var fmepSystem = new ElementClassFilter(typeof(MEPSystem));
            var fModelText = new ElementClassFilter(typeof(ModelText));
            var fOpening = new ElementClassFilter(typeof(Opening));
            var fPart = new ElementClassFilter(typeof(Part));
            var fPartMaker = new ElementClassFilter(typeof(PartMaker));
            var fReferencePlane = new ElementClassFilter(typeof(ReferencePlane));
            var fReferencePoint = new ElementClassFilter(typeof(ReferencePoint));
            var fSpatialElement = new ElementClassFilter(typeof(SpatialElement));
            var fAreaReinforcement = new ElementClassFilter(typeof(AreaReinforcement));
            var fHub = new ElementClassFilter(typeof(Hub));
            var fPathReinforcement = new ElementClassFilter(typeof(PathReinforcement));
            var fRebar = new ElementClassFilter(typeof(Rebar));
            var fTruss = new ElementClassFilter(typeof(Truss));

            filterList.Add(fContinuousRail);
            filterList.Add(fRailing);
            filterList.Add(fStairs);
            filterList.Add(fStairsLanding);
            filterList.Add(fTopographySurface);
            filterList.Add(fAssemblyInstance);
            filterList.Add(fBaseArray);
            filterList.Add(fBeamSystem);
            filterList.Add(fBoundaryConditions);
            filterList.Add(fConnectorElement);
            filterList.Add(fControl);
            filterList.Add(fCurveElement);
            filterList.Add(fDividedSurface);
            filterList.Add(fCableTrayConduitRunBase);
            filterList.Add(fHostObject);
            filterList.Add(fInstance);
            filterList.Add(fmepSystem);
            filterList.Add(fModelText);
            filterList.Add(fOpening);
            filterList.Add(fPart);
            filterList.Add(fPartMaker);
            filterList.Add(fReferencePlane);
            filterList.Add(fReferencePoint);
            filterList.Add(fAreaReinforcement);
            filterList.Add(fHub);
            filterList.Add(fPathReinforcement);
            filterList.Add(fRebar);
            filterList.Add(fTruss);
            filterList.Add(fSpatialElement);

            var cRvtLinks = new ElementCategoryFilter(BuiltInCategory.OST_RvtLinks);
            filterList.Add(cRvtLinks);

            var filters = new LogicalOrFilter(filterList);
            return filters;
        }

        #endregion

        private void RevitServicesUpdaterOnElementsModified(IEnumerable<string> updated)
        {
            if (updated.Any(uniqueIds.Contains))
            {
                ForceReExecuteOfNode = true;
                RequiresRecalc = true;
            }
        }

        private void RevitDynamoModel_RevitDocumentChanged(object sender, EventArgs e)
        {
            doc = DocumentManager.Instance.CurrentDBDocument;
            var elements = GetElementsVisibleInActiveView();
            elementIds = new HashSet<ElementId>(elements.Select(x => x.Id));
            uniqueIds = new HashSet<string>(elements.Select(x => x.UniqueId));
            ForceReExecuteOfNode = true;
            RequiresRecalc = true;
        }

        private void RevitServicesUpdaterOnElementsDeleted(
            Document document, IEnumerable<ElementId> deleted)
        {
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if (doc == document || doc.Equals(document))
            {
                elementIds.RemoveWhere(deleted.Contains);
                uniqueIds =
                    new HashSet<string>(elementIds.Select(id => document.GetElement(id).UniqueId));

                ForceReExecuteOfNode = true;
                RequiresRecalc = true;
            }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            if (uniqueIds == null)
                return new[] { AstFactory.BuildNullNode() };

            Func<string, bool, Revit.Elements.Element> func = ElementSelector.ByUniqueId;

            var elementList =
                AstFactory.BuildExprList(
                    uniqueIds.Select(
                        id =>
                            AstFactory.BuildFunctionCall(
                                func,
                                new List<AssociativeNode>
                                {
                                    AstFactory.BuildStringNode(id),
                                    AstFactory.BuildBooleanNode(true)
                                })).ToList());

            return new[]
            { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), elementList) };
        }
    }
}
