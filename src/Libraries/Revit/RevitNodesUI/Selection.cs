using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.DesignScript.Runtime;
using Autodesk.Revit.DB;

using Dynamo.Interfaces;
using Dynamo.Models;

using ProtoCore.AST.AssociativeAST;

using Revit.Elements;
using Revit.GeometryObjects;
using Revit.Interactivity;
using RevitServices.Persistence;

using Element = Autodesk.Revit.DB.Element;
using RevitDynamoModel = Dynamo.Applications.Models.RevitDynamoModel;

namespace Dynamo.Nodes
{
    public abstract class ElementSelection : SelectionBase
    {
        protected Document selectionOwner;

        protected RevitDynamoModel RevitDynamoModel { get; private set; }

        #region public properties

        /// <summary>
        /// The Element which is selected.
        /// </summary>
        public override List<string> Selection
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

        protected ElementSelection(WorkspaceModel workspaceModel,
            SelectionType selectionType,
            SelectionObjectType selectionObjectType,
            string message,
            string prefix)
            : base(workspaceModel, selectionType, selectionObjectType, message, prefix)
        {
            SelectionHelper = RevitSelectionHelper.Instance;

            RevitDynamoModel = Workspace.DynamoModel as RevitDynamoModel;

            // we need to obtain the dynamo model directly from the workspace model 
            // here, as it is not yet initialized on the base constructor
            var revMod = workspaceModel.DynamoModel as RevitDynamoModel;
            if (revMod == null) return;

            revMod.RevitServicesUpdater.ElementsDeleted += Updater_ElementsDeleted;
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

        void Updater_ElementsDeleted(Document document, IEnumerable<ElementId> deleted)
        {
            if (Selection.Any() && document.Equals(selectionOwner))
            {
                var uuids =
                deleted.Select(document.GetElement)
                    .Where(el => el != null)
                    .Select(el => el.UniqueId);

                Selection = Selection.Where(x => !uuids.Contains(x)).ToList();

                RaisePropertyChanged("Selection");
                RaisePropertyChanged("Text");
                RequiresRecalc = true;
            }
        }

        #endregion

        #region public methods

        public override void Destroy()
        {
            base.Destroy();
            RevitDynamoModel.RevitServicesUpdater.ElementsDeleted -= Updater_ElementsDeleted;
        }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            AssociativeNode node;

            if (Selection == null || !Selection.Any())
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var els = selection;

                var newInputs = els.Select(el =>
                    AstFactory.BuildFunctionCall(
                    new Func<string, bool, Revit.Elements.Element>(ElementSelector.ByUniqueId),
                    new List<AssociativeNode>
                    {
                        AstFactory.BuildStringNode(el),
                        AstFactory.BuildBooleanNode(true)
                    }
                    )).ToList();

                node = AstFactory.BuildExprList(newInputs);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        public override string ToString()
        {
            return selection.Any() ?
                string.Format("{0} : {1}", Prefix, FormatSelectionText(selection)) :
                "Nothing Selected";
        }

        #endregion

        #region private methods

        protected virtual string FormatSelectionText(IEnumerable<string> elements)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;
            var ids = elements.Select(doc.GetElement).Where(el => el != null).Select(el => el.Id).ToArray();
            return ids.Any()
                ? System.String.Join(" ", ids.Take(20))
                : "";
        }

        #endregion

        #region protected methods

        /// <summary>
        /// Callback when selection button is clicked. 
        /// Calls the selection action, and stores the ElementId(s) of the selected objects.
        /// </summary>
        protected override void Select(object parameter)
        {
            try
            {
                CanSelect = false;

                //call the delegate associated with a selection type
                Selection =
                    SelectionHelper.RequestSelectionOfType<Element>(
                        selectionMessage,
                        selectionType,
                        selectionObjectType,
                        RevitDynamoModel.Logger);

                RequiresRecalc = true;

                CanSelect = true;
            }
            catch (OperationCanceledException)
            {
                CanSelect = true;
            }
            catch (Exception e)
            {
                RevitDynamoModel.Logger.Log(e);
            }
        }

        #endregion
    }

    public abstract class SubElementSelection<T> : ElementSelection
    {
        protected Dictionary<string, List<string>> subSelections = new Dictionary<string, List<string>>();
        protected Func<List<string>, Dictionary<string, List<string>>> Update;

        protected SubElementSelection(
            WorkspaceModel workspaceModel,
            SelectionType selectionType,
            SelectionObjectType selectionObjectType,
            string message,
            string prefix)
            : base(workspaceModel, selectionType, selectionObjectType, message, prefix)
        {
            var revMod = workspaceModel.DynamoModel as RevitDynamoModel;
            if (revMod == null) return;
            revMod.RevitServicesUpdater.ElementsModified += Updater_ElementsModified;
        }

        public override void Destroy()
        {
            base.Destroy();
            RevitDynamoModel.RevitServicesUpdater.ElementsModified -= Updater_ElementsModified;
        }

        protected override void Select(object parameter)
        {
            try
            {
                CanSelect = false;

                //call the delegate associated with a selection type
                subSelections = SelectionHelper.RequestSubSelectionOfType<T>(
                    selectionMessage,
                    selectionType,
                    selectionObjectType,
                    RevitDynamoModel.Logger);

                Selection = subSelections.Values.SelectMany(x => x).ToList();

                RequiresRecalc = true;

                CanSelect = true;
            }
            catch (OperationCanceledException)
            {
                CanSelect = true;
            }
            catch (Exception e)
            {
                RevitDynamoModel.Logger.Log(e);
            }
        }

        private void Updater_ElementsModified(IEnumerable<string> updated)
        {
            if (Selection != null && Selection.Any(updated.Contains))
            {
                RequiresRecalc = true;

                if (Update != null)
                {
                    Selection.Clear();
                    subSelections = Update.Invoke(Selection);
                    Selection = subSelections.Values.SelectMany(x => x).ToList();
                }
            }
        }
    }

    public abstract class ReferenceSelection : ElementSelection
    {
        protected Dictionary<string, List<string>> subSelections = new Dictionary<string, List<string>>();

        protected ReferenceSelection(
            WorkspaceModel workspaceModel,
            SelectionType selectionType,
            SelectionObjectType selectionObjectType,
            string message,
            string prefix)
            : base(workspaceModel, selectionType, selectionObjectType, message, prefix) { }

        public override IEnumerable<AssociativeNode> BuildOutputAst(List<AssociativeNode> inputAstNodes)
        {
            var newInputs = new List<AssociativeNode>();

            AssociativeNode node;

            if (Selection == null || !Selection.Any())
            {
                node = AstFactory.BuildNullNode();
            }
            else
            {
                var doc = DocumentManager.Instance.CurrentDBDocument;

                foreach (var stableRef in Selection)
                {
                    var elRef =
                    Reference.ParseFromStableRepresentation(doc, stableRef);

                    // POINT ON FACE
                    var pt = elRef.GlobalPoint;
                    if (pt != null)
                    {
                        //this is a selected point on a face
                        var ptArgs = new List<AssociativeNode>()
                        {
                            AstFactory.BuildDoubleNode(pt.X),
                            AstFactory.BuildDoubleNode(pt.Y),
                            AstFactory.BuildDoubleNode(pt.Z)
                        };

                        var ptNode = AstFactory.BuildFunctionCall
                        (
                            "Autodesk.DesignScript.Geometry.Point",
                            "ByCoordinates",
                            ptArgs
                        );

                        newInputs.Add(ptNode);
                        continue;
                    }

                    // UV POINT ON FACE
                    var uv = elRef.UVPoint;
                    if (uv != null)
                    {
                        //this is a selected point on a face
                        var ptArgs = new List<AssociativeNode>()
                        {
                            AstFactory.BuildDoubleNode(uv.U),
                            AstFactory.BuildDoubleNode(uv.V)
                        };

                        var uvNode = AstFactory.BuildFunctionCall
                        (
                            "Autodesk.DesignScript.Geometry.UV",
                            "ByCoordinates",
                            ptArgs
                        );

                        newInputs.Add(uvNode);
                        continue;
                    }


                    var refNode = AstFactory.BuildFunctionCall(
                        new Func<string, object>(
                            GeometryObjectSelector.ByReferenceStableRepresentation),
                        new List<AssociativeNode>
                        {
                            AstFactory.BuildStringNode(stableRef),
                        }
                        );
                    newInputs.Add(refNode);

                }

                node = AstFactory.BuildExprList(newInputs);
            }

            return new[] { AstFactory.BuildAssignment(GetAstIdentifierForOutputIndex(0), node) };
        }

        protected override void Select(object parameter)
        {
            try
            {
                CanSelect = false;

                //call the delegate associated with a selection type
                subSelections = SelectionHelper.RequestSelection(
                    selectionMessage,
                    selectionType,
                    selectionObjectType,
                    RevitDynamoModel.Logger);

                Selection = subSelections.Values.SelectMany(x => x).ToList();

                RequiresRecalc = true;

                CanSelect = true;
            }
            catch (OperationCanceledException)
            {
                CanSelect = true;
            }
            catch (Exception e)
            {
                RevitDynamoModel.Logger.Log(e);
            }
        }

        protected override string FormatSelectionText(IEnumerable<string> elements)
        {
            var doc = DocumentManager.Instance.CurrentDBDocument;

            var refs = Selection.Select(r => Reference.ParseFromStableRepresentation(doc, r));
            var ids = refs.Select(doc.GetElement).Where(el => el != null).Select(el => el.Id).ToArray();

            return ids.Any()
                ? System.String.Join(" ", ids.Take(20))
                : "";
        }
    }
    
    [NodeName("Select Analysis Results")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select analysis results from the document.")]
    [IsDesignScriptCompatible]
    [IsVisibleInDynamoLibrary(false)]
    public class DSAnalysisResultSelection : ElementSelection
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
    public class DSModelElementSelection : ElementSelection
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
    }

    [NodeName("Select UV on Face")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a UV on a face.")]
    [IsDesignScriptCompatible]
    public class DSUVOnElementSelection : ReferenceSelection
    {
        public DSUVOnElementSelection(WorkspaceModel workspaceModel)
            : base(workspaceModel, 
            SelectionType.One,
            SelectionObjectType.PointOnFace, 
            "Select a point on a face.",
            "UV on Element"){ }
    }

    [NodeName("Select Divided Surface Families")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select a divided surface and get its family instances.")]
    [IsDesignScriptCompatible]
    public class DSDividedSurfaceFamiliesSelection : SubElementSelection<Autodesk.Revit.DB.DividedSurface>
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
            Update = RevitSelectionHelper.GetFamilyInstancesFromDividedSurface;
        }
    }

    [NodeName("Select Model Elements")]
    [NodeCategory(BuiltinNodeCategories.REVIT_SELECTION)]
    [NodeDescription("Select multiple elements from the Revit document.")]
    [IsDesignScriptCompatible]
    public class DSModelElementsSelection : ElementSelection
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
