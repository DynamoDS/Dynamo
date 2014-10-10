using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;

using Dynamo.Interfaces;
using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

using Revit.Elements;
using Revit.GeometryConversion;
using Revit.GeometryObjects;
using Revit.Interactivity;
using RevitServices.Persistence;

using DividedSurface = Autodesk.Revit.DB.DividedSurface;
using Element = Autodesk.Revit.DB.Element;
using RevitDynamoModel = Dynamo.Applications.Models.RevitDynamoModel;
using Point = Autodesk.DesignScript.Geometry.Point;
using UV = Autodesk.DesignScript.Geometry.UV;

namespace Dynamo.Nodes
{
    public abstract class RevitSelection<TSelection, TResult> : SelectionBase<TSelection, TResult>
    {
        protected Document SelectionOwner { get; private set; }
        protected RevitDynamoModel RevitDynamoModel { get; private set; }

        #region public properties

        public override bool CanSelect
        {
            get { return base.CanSelect && RevitDynamoModel.RunEnabled; }
            set { base.CanSelect = value; }
        }

        public override string SelectionSuggestion
        {
            get
            {
                return RevitDynamoModel.RunEnabled
                    ? base.SelectionSuggestion
                    : "Selection is disabled when Dynamo run is disabled.";
            }
        }

        #endregion

        #region protected constructors

        protected RevitSelection(
            WorkspaceModel workspaceModel, SelectionType selectionType,
            SelectionObjectType selectionObjectType, string message, string prefix)
            : base(workspaceModel, selectionType, selectionObjectType, message, prefix)
        {
            Logger = workspaceModel.DynamoModel.Logger;

            RevitDynamoModel = Workspace.DynamoModel as RevitDynamoModel;

            // we need to obtain the dynamo model directly from the workspace model 
            // here, as it is not yet initialized on the base constructor
            var revMod = workspaceModel.DynamoModel as RevitDynamoModel;
            if (revMod == null) return;

            revMod.RevitServicesUpdater.ElementsDeleted += Updater_ElementsDeleted;
            revMod.RevitServicesUpdater.ElementsModified += Updater_ElementsModified;
            revMod.RevitDocumentChanged += Controller_RevitDocumentChanged;
            revMod.PropertyChanged += revMod_PropertyChanged;
        }

        /// <summary>
        /// Handler for the RevitDynamoModel's PropertyChanged event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void revMod_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Use the RunEnabled flag on the dynamo model
            // to set the CanSelect flag, enabling or disabling
            // any bound UI when the dynamo model is not
            // in a runnable state.
            if (e.PropertyName == "RunEnabled")
            {
                RaisePropertyChanged("CanSelect");
                RaisePropertyChanged("SelectionSuggestion");
            }
        }

        #endregion

        #region ElementSync

        private void Controller_RevitDocumentChanged(object sender, EventArgs e)
        {
            ClearSelections();
        }

        #endregion

        #region public methods

        public override void Destroy()
        {
            base.Destroy();
            RevitDynamoModel.RevitServicesUpdater.ElementsDeleted -= Updater_ElementsDeleted;
            RevitDynamoModel.RevitServicesUpdater.ElementsModified -= Updater_ElementsModified;
        }

        public override void UpdateSelection(IEnumerable<TSelection> rawSelection)
        {
            base.UpdateSelection(rawSelection);
            SelectionOwner = DocumentManager.Instance.CurrentDBDocument;
        }

        #endregion

        #region protected methods

        protected virtual void Updater_ElementsDeleted(
            Document document, IEnumerable<ElementId> deleted) { }

        protected virtual void Updater_ElementsModified(IEnumerable<string> updated) { }

        #endregion
    }

    /// <summary>
    /// A selection type where the derived class specifies the type to select,
    /// and returns an element.
    /// </summary>
    /// <typeparam name="TSelection"></typeparam>
    public abstract class ElementSelection<TSelection> : RevitSelection<TSelection, Element>
        where TSelection : Element
    {
        protected ElementSelection(
            WorkspaceModel workspaceModel, SelectionType selectionType,
            SelectionObjectType selectionObjectType, string message, string prefix)
            : base(workspaceModel, selectionType, selectionObjectType, message, prefix) { }

        public override IModelSelectionHelper<TSelection> SelectionHelper
        {
            get { return RevitElementSelectionHelper<TSelection>.Instance; }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;
            Func<string, bool, Revit.Elements.Element> func = ElementSelector.ByUniqueId;

            var results = SelectionResults.ToList();

            if (SelectionResults == null || !results.Any())
            {
                node = AstFactory.BuildNullNode();
            }
            else if (results.Count == 1)
            {
                var el = results.First();

                // If there is only one object in the list,
                // return a single item.
                node = AstFactory.BuildFunctionCall(
                    func,
                    new List<AssociativeNode>
                    {
                        AstFactory.BuildStringNode(el.UniqueId),
                        AstFactory.BuildBooleanNode(true)
                    });
            }
            else
            {
                var newInputs =
                    results.Select(
                        el =>
                            AstFactory.BuildFunctionCall(
                                func,
                                new List<AssociativeNode>
                                {
                                    AstFactory.BuildStringNode(el.UniqueId),
                                    AstFactory.BuildBooleanNode(true)
                                })).ToList();

                node = AstFactory.BuildExprList(newInputs);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        protected override string FormatSelectionText<T>(IEnumerable<T> elements)
        {
            if (typeof(T) != typeof(Element)) return "";

            var ids =
                elements.Cast<Element>().Where(el => el.IsValidObject).Select(el => el.Id).ToArray();
            return ids.Any() ? String.Join(" ", ids.Take(20)) : "";
        }

        protected override TSelection GetModelObjectFromIdentifer(string id)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var e = doc.GetElement(id);
            return e as TSelection;
        }

        protected override string GetIdentifierFromModelObject(TSelection modelObject)
        {
            return modelObject == null ? null : modelObject.UniqueId;
        }

        protected override void Updater_ElementsDeleted(
            Document document, IEnumerable<ElementId> deleted)
        {
            if (!SelectionResults.Any() || !document.Equals(SelectionOwner)) return;

            var uuids =
                deleted.Select(document.GetElement)
                    .Where(el => el != null)
                    .Select(el => el.UniqueId);

            UpdateSelection(Selection.Where(x => !uuids.Contains(x.UniqueId)));
        }

        protected override void Updater_ElementsModified(IEnumerable<string> updated)
        {
            if (!updated.Any())
                return;

            var updatedSet = new HashSet<string>(updated);

            if (!SelectionResults.Select(x => x.UniqueId).Any(updatedSet.Contains))
                return;

            UpdateSelection(Selection);
        }

        protected override IEnumerable<Element> ExtractSelectionResults(TSelection selection)
        {
            yield return selection;
        }
    }

    /// <summary>
    /// A selection type for selecting reference objects in Revit (Faces, Edges, etc.)
    /// </summary>
    public abstract class ReferenceSelection : RevitSelection<Reference, Reference>
    {
        protected ReferenceSelection(
            WorkspaceModel workspaceModel, SelectionType selectionType,
            SelectionObjectType selectionObjectType, string message, string prefix)
            : base(workspaceModel, selectionType, selectionObjectType, message, prefix) { }

        public override IModelSelectionHelper<Reference> SelectionHelper
        {
            get { return RevitReferenceSelectionHelper.Instance; }
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;
            Func<string, object> func = GeometryObjectSelector.ByReferenceStableRepresentation;

            var results = SelectionResults.ToList();

            if (SelectionResults == null || !results.Any())
            {
                node = AstFactory.BuildNullNode();
            }
            else if (results.Count == 1)
            {
                var stableRef = GetIdentifierFromModelObject(results.First());

                node = AstFactory.BuildFunctionCall(
                    func,
                    new List<AssociativeNode> { AstFactory.BuildStringNode(stableRef), });
            }
            else
            {
                var stableRefs = results.Select(GetIdentifierFromModelObject);

                var newInputs =
                    stableRefs.Select(
                        stableRef =>
                            AstFactory.BuildFunctionCall(
                                func,
                                new List<AssociativeNode> { AstFactory.BuildStringNode(stableRef), }))
                        .ToList();

                node = AstFactory.BuildExprList(newInputs);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        protected override string FormatSelectionText<T>(IEnumerable<T> elements)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var ids =
                elements.Cast<Reference>()
                    .Select(doc.GetElement)
                    .Where(el => el != null)
                    .Select(el => el.Id);

            return ids.Any() ? String.Join(" ", ids.Take(20)) : "";
        }

        protected override Reference GetModelObjectFromIdentifer(string id)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            try
            {
                var reference = Reference.ParseFromStableRepresentation(doc, id);
                return reference;
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected override string GetIdentifierFromModelObject(Reference modelObject)
        {
            if (modelObject == null)
            {
                return null;
            }

            try
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                return modelObject.ConvertToStableRepresentation(doc);
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected override void Updater_ElementsDeleted(
            Document document, IEnumerable<ElementId> deleted)
        {
            // If an element is deleted, ensure all references which refer
            // to that element are removed from the selection

            if (!SelectionResults.Any() || !document.Equals(SelectionOwner)) return;

            UpdateSelection(SelectionResults.Where(x => !deleted.Contains(x.ElementId)));

            RaisePropertyChanged("SelectionResults");
            RaisePropertyChanged("Text");
            RequiresRecalc = true;
        }

        protected override void Updater_ElementsModified(IEnumerable<string> updated)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            // If an element is modified, require recalc
            if (SelectionResults == null
                || !SelectionResults.Select(r => doc.GetElement(r).UniqueId).Any(updated.Contains))
                return;

            RequiresRecalc = true;
        }

        protected override IEnumerable<Reference> ExtractSelectionResults(Reference selection)
        {
            yield return selection;
        }
    }

    [NodeName("Select Analysis Results"), NodeCategory(BuiltinNodeCategories.REVIT_SELECTION),
     NodeDescription("Select analysis results from the document."), IsDesignScriptCompatible,
     IsVisibleInDynamoLibrary(false)]
    public class DSAnalysisResultSelection : ElementSelection<Element>
    {
        public DSAnalysisResultSelection(WorkspaceModel workspaceModel)
            : base(
                workspaceModel,
                SelectionType.One,
                SelectionObjectType.None,
                "Select an analysis result.",
                "Analysis Results") { }
    }

    [NodeName("Select Model Element"), NodeCategory(BuiltinNodeCategories.REVIT_SELECTION),
     NodeDescription("Select a model element from the document."), IsDesignScriptCompatible]
    public class DSModelElementSelection : ElementSelection<Element>
    {
        public DSModelElementSelection(WorkspaceModel workspaceModel)
            : base(
                workspaceModel,
                SelectionType.One,
                SelectionObjectType.None,
                "Select Model Element",
                "Element") { }
    }

    [NodeName("Select Face"), NodeCategory(BuiltinNodeCategories.REVIT_SELECTION),
     NodeDescription("Select a face."), IsDesignScriptCompatible]
    public class DSFaceSelection : ReferenceSelection
    {
        public DSFaceSelection(WorkspaceModel workspaceModel)
            : base(
                workspaceModel,
                SelectionType.One,
                SelectionObjectType.Face,
                "Select a face.",
                "Face of Element Id") { }
    }

    [NodeName("Select Edge"), NodeCategory(BuiltinNodeCategories.REVIT_SELECTION),
     NodeDescription("Select an edge."), IsDesignScriptCompatible]
    public class DSEdgeSelection : ReferenceSelection
    {
        public DSEdgeSelection(WorkspaceModel workspaceModel)
            : base(
                workspaceModel,
                SelectionType.One,
                SelectionObjectType.Edge,
                "Select an edge.",
                "Edge of Element Id") { }
    }

    [NodeName("Select Point on Face"), NodeCategory(BuiltinNodeCategories.REVIT_SELECTION),
     NodeDescription("Select a point on a face."), IsDesignScriptCompatible]
    public class DSPointOnElementSelection : ReferenceSelection
    {
        public DSPointOnElementSelection(WorkspaceModel workspaceModel)
            : base(
                workspaceModel,
                SelectionType.One,
                SelectionObjectType.PointOnFace,
                "Select a point on a face.",
                "Point on Element") { }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;

            if (SelectionResults == null || !SelectionResults.Any())
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var newInputs = new List<AssociativeNode>();

                foreach (var reference in SelectionResults)
                {
                    if (reference.GlobalPoint == null)
                    {
                        return new[]
                        {
                            AstFactory.BuildAssignment(
                                GetAstIdentifierForOutputIndex(0),
                                AstFactory.BuildNullNode())
                        };
                    }

                    var pt = reference.GlobalPoint.ToPoint();

                    //this is a selected point on a face
                    var ptArgs = new List<AssociativeNode>
                    {
                        AstFactory.BuildDoubleNode(pt.X),
                        AstFactory.BuildDoubleNode(pt.Y),
                        AstFactory.BuildDoubleNode(pt.Z)
                    };

                    var functionCallNode =
                        AstFactory.BuildFunctionCall(
                            new Func<double, double, double, Point>(Point.ByCoordinates),
                            ptArgs);

                    newInputs.Add(functionCallNode);
                }

                node = AstFactory.BuildExprList(newInputs);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("Select UV on Face"), NodeCategory(BuiltinNodeCategories.REVIT_SELECTION),
     NodeDescription("Select a UV on a face."), IsDesignScriptCompatible]
    public class DSUvOnElementSelection : ReferenceSelection
    {
        public DSUvOnElementSelection(WorkspaceModel workspaceModel)
            : base(
                workspaceModel,
                SelectionType.One,
                SelectionObjectType.PointOnFace,
                "Select a point on a face.",
                "UV on Element") { }

        public override IEnumerable<AssociativeNode> BuildOutputAst(
            List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;

            if (SelectionResults == null || !SelectionResults.Any())
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var newInputs = new List<AssociativeNode>();

                foreach (var reference in SelectionResults)
                {
                    if (reference.UVPoint == null)
                    {
                        return new[]
                        {
                            AstFactory.BuildAssignment(
                                GetAstIdentifierForOutputIndex(0),
                                AstFactory.BuildNullNode())
                        };
                    }

                    var pt = reference.UVPoint;

                    //this is a selected point on a face
                    var ptArgs = new List<AssociativeNode>
                    {
                        AstFactory.BuildDoubleNode(pt.U),
                        AstFactory.BuildDoubleNode(pt.V),
                    };

                    var functionCallNode =
                        AstFactory.BuildFunctionCall(
                            new Func<double, double, UV>(UV.ByCoordinates),
                            ptArgs);

                    newInputs.Add(functionCallNode);
                }

                node = AstFactory.BuildExprList(newInputs);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("Select Divided Surface Families"),
     NodeCategory(BuiltinNodeCategories.REVIT_SELECTION),
     NodeDescription("Select a divided surface and get its family instances."),
     IsDesignScriptCompatible]
    public class DSDividedSurfaceFamiliesSelection : ElementSelection<DividedSurface>
    {
        public DSDividedSurfaceFamiliesSelection(WorkspaceModel workspaceModel)
            : base(
                workspaceModel,
                SelectionType.One,
                SelectionObjectType.None,
                "Select a divided surface.",
                "Elements") { }

        // Set an update method. When the target object is modified in
        // Revit, this will cause the sub-elements to be modified.
        protected override IEnumerable<Element> ExtractSelectionResults(DividedSurface selection)
        {
            return
                RevitElementSelectionHelper<DividedSurface>.GetFamilyInstancesFromDividedSurface(
                    selection);
        }
    }

    [NodeName("Select Model Elements"), NodeCategory(BuiltinNodeCategories.REVIT_SELECTION),
     NodeDescription("Select multiple elements from the Revit document."), IsDesignScriptCompatible]
    public class DSModelElementsSelection : ElementSelection<Element>
    {
        public DSModelElementsSelection(WorkspaceModel workspaceModel)
            : base(
                workspaceModel,
                SelectionType.Many,
                SelectionObjectType.None,
                "Select elements.",
                "Elements") { }
    }

    [NodeName("Select Faces"), NodeCategory(BuiltinNodeCategories.REVIT_SELECTION),
     NodeDescription("Select multiple faces from the Revit document."), IsDesignScriptCompatible]
    public class SelectFaces : ReferenceSelection
    {
        public SelectFaces(WorkspaceModel workspaceModel)
            : base(
                workspaceModel,
                SelectionType.Many,
                SelectionObjectType.Face,
                "Select faces.",
                "Faces") { }
    }
}
