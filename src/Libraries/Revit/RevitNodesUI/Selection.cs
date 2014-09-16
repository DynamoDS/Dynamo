using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

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

using Element = Autodesk.Revit.DB.Element;
using RevitDynamoModel = Dynamo.Applications.Models.RevitDynamoModel;
using Point = Autodesk.DesignScript.Geometry.Point;
using UV = Autodesk.DesignScript.Geometry.UV;

namespace Dynamo.Nodes
{
    public abstract class RevitSelection<T1, T2> : SelectionBase<T1, T2>
    {
        protected Document selectionOwner;
        protected RevitDynamoModel RevitDynamoModel { get; private set; }

        #region public properties

        /// <summary>
        /// The Element which is selected.
        /// </summary>
        public override List<T1> Selection
        {
            get { return selection; }
            set
            {
                bool dirty = value != null;

                selection = value;

                if (selection != null)
                {
                    selectionOwner = DocumentManager.Instance.CurrentDBDocument;
                }

                if (dirty)
                {
                    RequiresRecalc = true;
                }

                RaisePropertyChanged("Selection");
                RaisePropertyChanged("Text");
            }
        }

        #endregion

        #region protected constructors

        protected RevitSelection(WorkspaceModel workspaceModel,
            SelectionType selectionType,
            SelectionObjectType selectionObjectType,
            string message,
            string prefix)
            : base(workspaceModel, selectionType, selectionObjectType, message, prefix)
        {
            logger = workspaceModel.DynamoModel.Logger;

            SelectionHelper = RevitSelectionHelper.Instance;

            RevitDynamoModel = Workspace.DynamoModel as RevitDynamoModel;

            // we need to obtain the dynamo model directly from the workspace model 
            // here, as it is not yet initialized on the base constructor
            var revMod = workspaceModel.DynamoModel as RevitDynamoModel;
            if (revMod == null) return;

            revMod.RevitServicesUpdater.ElementsDeleted += Updater_ElementsDeleted;
            revMod.RevitServicesUpdater.ElementsModified += Updater_ElementsModified;
            revMod.RevitDocumentChanged += Controller_RevitDocumentChanged;
        }

        #endregion

        #region ElementSync

        void Controller_RevitDocumentChanged(object sender, EventArgs e)
        {
            Selection.Clear();
            RaisePropertyChanged("Selection");
            RaisePropertyChanged("Text");
        }

        #endregion

        #region public methods

        public override void Destroy()
        {
            base.Destroy();
            RevitDynamoModel.RevitServicesUpdater.ElementsDeleted -= Updater_ElementsDeleted;
            RevitDynamoModel.RevitServicesUpdater.ElementsModified -= Updater_ElementsModified;
        }

        #endregion

        #region protected methods

        protected virtual void Updater_ElementsDeleted(Document document, IEnumerable<ElementId> deleted){}

        protected virtual void Updater_ElementsModified(IEnumerable<string> updated){}

        #endregion
    }

    public abstract class ElementSelection<Tsel> : RevitSelection<Tsel, Element> where Tsel : Element
    {
        protected ElementSelection(WorkspaceModel workspaceModel,
            SelectionType selectionType,
            SelectionObjectType selectionObjectType,
            string message,
            string prefix)
            : base(workspaceModel, selectionType, selectionObjectType, message, prefix) { }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;

            if ((Selection == null || !Selection.Any()) && 
                (!SubSelection.Any() || SubSelection == null))
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var els = SubSelection.Any() ?
                    SubSelection.Cast<Element>():
                    Selection.Cast<Element>();

                var newInputs = els.Select(el =>
                    AstFactory.BuildFunctionCall(
                    new Func<string, bool, Revit.Elements.Element>(ElementSelector.ByUniqueId),
                    new List<AssociativeNode>
                    {
                        AstFactory.BuildStringNode(el.UniqueId),
                        AstFactory.BuildBooleanNode(true)
                    }
                    )).ToList();

                node = AstFactory.BuildExprList(newInputs);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        protected override string FormatSelectionText<T>(List<T> elements)
        {
            if (typeof(T) != typeof(Element)) return "";

            var ids = elements.Cast<Element>().Where(el=>el.IsValidObject).Select(el => el.Id).ToArray();
            return ids.Any()
                ? System.String.Join(" ", ids.Take(20))
                : "";
        }

        protected override object GetModelObjectFromIdentifer(string id)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var e = doc.GetElement(id);
            return e;
        }

        protected override string GetIdentifierFromModelObject(object modelObject)
        {
            var element = (Element)modelObject;
            return modelObject == null ? null : element.UniqueId;
        }

        protected override void Updater_ElementsDeleted(Document document, IEnumerable<ElementId> deleted)
        {
            var elementIds = deleted as ElementId[] ?? deleted.ToArray();
            if (!elementIds.Any()) return;

            if (!Selection.Any() || !document.Equals(selectionOwner)) return;

            var uuids =
                elementIds.Select(document.GetElement)
                    .Where(el => el != null)
                    .Select(el => el.UniqueId);

            Selection = Selection.Where(x => !uuids.Contains(x.UniqueId)).ToList();

            RaisePropertyChanged("Selection");
            RaisePropertyChanged("Text");
            RequiresRecalc = true;

            UpdateSubElements();
        }

        protected override void Updater_ElementsModified(IEnumerable<string> updated)
        {
            var enumerable = updated as string[] ?? updated.ToArray();
            if (!enumerable.Any()) return;

            if (Selection == null || !Selection.Select(x => x.UniqueId).Any(enumerable.Contains))
                return;

            RequiresRecalc = true;

            RaisePropertyChanged("Selection");
            RaisePropertyChanged("Text");
            RequiresRecalc = true;

            UpdateSubElements();
        }
    }

    public abstract class ReferenceSelection : RevitSelection<Reference, Reference>
    {
        protected ReferenceSelection(
            WorkspaceModel workspaceModel,
            SelectionType selectionType,
            SelectionObjectType selectionObjectType,
            string message,
            string prefix)
            : base(workspaceModel, selectionType, selectionObjectType, message, prefix){}

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;

            if (Selection == null || !Selection.Any())
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var stableRefs = Selection.Select(GetIdentifierFromModelObject);

                var newInputs = stableRefs.Select(stableRef =>
                    AstFactory.BuildFunctionCall(
                    new Func<string, object>(GeometryObjectSelector.ByReferenceStableRepresentation),
                    new List<AssociativeNode>
                    {
                        AstFactory.BuildStringNode(stableRef),
                    }
                    )).ToList();

                node = AstFactory.BuildExprList(newInputs);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        protected override string FormatSelectionText<T>(List<T> elements)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var ids = elements.Cast<Reference>().Select(doc.GetElement).Where(el => el != null).Select(el => el.Id).ToArray();

            return ids.Any()
                ? String.Join(" ", ids.Take(20))
                : "";
        }

        protected override object GetModelObjectFromIdentifer(string id)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            try
            {
                var reference = Reference.ParseFromStableRepresentation(doc, id);
                return reference;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        protected override string GetIdentifierFromModelObject(object modelObject)
        {
            if (modelObject == null)
            {
                return null;
            }

            try
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;
                return ((Reference)modelObject).ConvertToStableRepresentation(doc);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        protected override void Updater_ElementsDeleted(Document document, IEnumerable<ElementId> deleted)
        {
            // If an element is deleted, ensure all references which refer
            // to that element are removed from the selection

            if (!Selection.Any() || !document.Equals(selectionOwner)) return;

            Selection = Selection.Where(x => !deleted.Contains(x.ElementId)).ToList();

            RaisePropertyChanged("Selection");
            RaisePropertyChanged("Text");
            RequiresRecalc = true;
        }

        protected override void Updater_ElementsModified(IEnumerable<string> updated)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            // If an element is modified, require recalc
            if (Selection == null || !Selection.Select(r => doc.GetElement(r).UniqueId).Any(updated.Contains))
                return;

            RequiresRecalc = true;
        }
    }
    
    [NodeName("Select Analysis Results")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select analysis results from the document.")]
    [IsDesignScriptCompatible]
    [IsVisibleInDynamoLibrary(false)]
    public class DSAnalysisResultSelection : ElementSelection<Element>
    {
        public DSAnalysisResultSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, 
            SelectionType.One, 
            SelectionObjectType.None,
            "Select an analysis result.", 
            "Analysis Results"){ }
    }

    [NodeName("Select Model Element")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a model element from the document.")]
    [IsDesignScriptCompatible]
    public class DSModelElementSelection : ElementSelection<Element>
    {
        public DSModelElementSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, 
            SelectionType.One, 
            SelectionObjectType.None,
            "Select Model Element", 
            "Element"){ }
    }
    
    [NodeName("Select Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a face.")]
    [IsDesignScriptCompatible]
    public class DSFaceSelection : ReferenceSelection
    {
        public DSFaceSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, 
            SelectionType.One,
            SelectionObjectType.Face,
            "Select a face.", 
            "Face of Element Id") { }
    }

    [NodeName("Select Edge")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select an edge.")]
    [IsDesignScriptCompatible]
    public class DSEdgeSelection : ReferenceSelection
    {
        public DSEdgeSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, 
            SelectionType.One,
            SelectionObjectType.Edge, 
            "Select an edge.", 
            "Edge of Element Id"){ }
    }

    [NodeName("Select Point on Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a point on a face.")]
    [IsDesignScriptCompatible]
    public class DSPointOnElementSelection : ReferenceSelection
    {
        public DSPointOnElementSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, 
            SelectionType.One,
            SelectionObjectType.PointOnFace, 
            "Select a point on a face.",
            "Point on Element"){ }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;

            if (Selection == null || !Selection.Any())
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var newInputs = new List<AssociativeNode>();

                foreach (var reference in Selection)
                {
                    if (reference.GlobalPoint == null)
                    {
                        return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
                    }

                    var pt = reference.GlobalPoint.ToPoint();

                    //this is a selected point on a face
                    var ptArgs = new List<AssociativeNode>()
                    {
                        AstFactory.BuildDoubleNode(pt.X),
                        AstFactory.BuildDoubleNode(pt.Y),
                        AstFactory.BuildDoubleNode(pt.Z)
                    };

                    var functionCallNode = AstFactory.BuildFunctionCall(
                        new Func<double, double, double, Point>(Point.ByCoordinates),
                        ptArgs);

                    newInputs.Add(functionCallNode);
                }

                node = AstFactory.BuildExprList(newInputs);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("Select UV on Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a UV on a face.")]
    [IsDesignScriptCompatible]
    public class DSUvOnElementSelection : ReferenceSelection
    {
        public DSUvOnElementSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, 
            SelectionType.One,
            SelectionObjectType.PointOnFace, 
            "Select a point on a face.",
            "UV on Element"){ }
        
        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;

            if (Selection == null || !Selection.Any())
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var newInputs = new List<AssociativeNode>();

                foreach (var reference in Selection)
                {
                    if (reference.UVPoint == null)
                    {
                        return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), AstFactory.BuildNullNode()) };
                    }

                    var pt = reference.UVPoint;

                    //this is a selected point on a face
                    var ptArgs = new List<AssociativeNode>()
                    {
                        AstFactory.BuildDoubleNode(pt.U),
                        AstFactory.BuildDoubleNode(pt.V),
                    };

                    var functionCallNode = AstFactory.BuildFunctionCall(
                        new Func<double, double, UV>(UV.ByCoordinates),
                        ptArgs);

                    newInputs.Add(functionCallNode);
                }

                node = AstFactory.BuildExprList(newInputs);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }
    }

    [NodeName("Select Divided Surface Families")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a divided surface and get its family instances.")]
    [IsDesignScriptCompatible]
    public class DSDividedSurfaceFamiliesSelection : ElementSelection<Autodesk.Revit.DB.DividedSurface>
    {
        public DSDividedSurfaceFamiliesSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel,
                   SelectionType.One,
                   SelectionObjectType.None,
                   "Select a divided surface.",
                   "Elements")
        {
            // Set an update method. When the target object is modified in
            // Revit, this will cause the sub-elements to be modified.
            SubElementUpdate = RevitSelectionHelper.GetFamilyInstancesFromDividedSurface;
        }
    }

    [NodeName("Select Model Elements")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select multiple elements from the Revit document.")]
    [IsDesignScriptCompatible]
    public class DSModelElementsSelection : ElementSelection<Element>
    {
        public DSModelElementsSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, 
            SelectionType.Many,
            SelectionObjectType.None,
            "Select elements.", 
            "Elements") { }
    }

    [NodeName("Select Faces")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select multiple faces from the Revit document.")]
    [IsDesignScriptCompatible]
    public class SelectFaces : ReferenceSelection
    {
        public SelectFaces(WorkspaceModel workspaceModel)
            : base(workspaceModel,
            SelectionType.Many,
            SelectionObjectType.Face,
            "Select faces.",
            "Faces") { }
    }
}
