using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Xml;

using Autodesk.Revit.DB;

using Microsoft.FSharp.Collections;

using Dynamo.Utilities;
using Value = Dynamo.FScheme.Value;
using Dynamo.Connectors;
using Dynamo.Controls;
using Dynamo.Nodes;

using HelixToolkit.Wpf;

namespace Dynamo.Revit
{
    public abstract partial class dynRevitTransactionNode : dynNodeModel, IDrawable
    {
        protected object DrawableObject = null;
        protected Func<object, RenderDescription> DrawMethod = null;

        //private Type base_type = null;

        //TODO: Move from dynElementSettings to another static area in DynamoRevit
        protected Autodesk.Revit.UI.UIDocument UIDocument
        {
            get { return dynRevitSettings.Doc; }
        }

        // this contains a list of all the elements created over all previous
        // recursive runs over the node. subsequest runs executed via the 'Run'
        // button or 'Run Automatically' are stored in an external map
        // To get all the Elements associated with this node, flatten this list
        private List<List<ElementId>> elements
        {
            get
            {
                return dynRevitSettings.ElementsContainers.Peek()[GUID];
            }
        }

        // This list contains the elements of the current recurvise execution
        public List<ElementId> Elements
        {
            get
            {
                while (elements.Count <= _runCount)
                    elements.Add(new List<ElementId>());
                return elements[_runCount];
            }
        }

        public IEnumerable<ElementId> AllElements
        {
            get
            {
                return elements.SelectMany(x => x);
            }
        }

        public RenderDescription RenderDescription { get; set; }

        protected dynRevitTransactionNode()
        {
            ArgumentLacing = LacingStrategy.Longest;
            RegisterAllElementsDeleteHook();
        }

        protected override void SaveNode(XmlDocument xmlDoc, XmlElement dynEl, SaveContext context)
        {
            //Don't copy over stored references
            if (context == SaveContext.Copy)
                return;

            //Only save elements in the home workspace
            if (WorkSpace is FuncWorkspace)
                return;

            foreach (var run in elements)
            {
                var outEl = xmlDoc.CreateElement("Run");

                foreach (var id in run)
                {
                    Element e;
                    if (dynUtils.TryGetElement(id,typeof(object), out e))
                    {
                        var elementStore = xmlDoc.CreateElement("Element");
                        elementStore.InnerText = e.UniqueId;
                        outEl.AppendChild(elementStore);
                    }
                }
                dynEl.AppendChild(outEl);
            }
        }

        protected override void LoadNode(XmlNode elNode)
        {
            var del = new DynElementUpdateDelegate(onDeleted);

            elements.Clear();

            var sb = new StringBuilder();
            
            foreach (XmlNode subNode in elNode.ChildNodes)
            {
                if (subNode.Name == "Run")
                {
                    var runElements = new List<ElementId>();
                    elements.Add(runElements);

                    foreach (XmlNode element in subNode.ChildNodes)
                    {
                        if (element.Name == "Element")
                        {
                            var eid = element.InnerText;
                            try
                            {
                                var id = UIDocument.Document.GetElement(eid).Id;
                                runElements.Add(id);
                                dynRevitSettings.Controller.RegisterDeleteHook(id, del);
                            }
                            catch (NullReferenceException)
                            {
                                //dynSettings.Controller.DynamoViewModel.Log("Element with UID \"" + eid + "\" not found in Document.");
                                sb.AppendLine("Element with UID \"" + eid + "\" not found in Document.");
                            }
                        }
                    }
                }
            }

            dynSettings.Controller.DynamoViewModel.Log(sb.ToString());
        }

        internal void RegisterAllElementsDeleteHook()
        {
            var del = new DynElementUpdateDelegate(onDeleted);

            foreach (var id in elements.SelectMany(eList => eList)) 
            {
                dynRevitSettings.Controller.RegisterDeleteHook(id, del);
            }
        }

        #region Watch 3D Rendering

        public virtual void Draw()
        {
            if (this.RenderDescription == null)
                this.RenderDescription = new Nodes.RenderDescription();
            else
                this.RenderDescription.ClearAll();

            var drawaableRevitElements = elements.SelectMany(x => x.Select(y => dynRevitSettings.Doc.Document.GetElement(y)));

            Debug.WriteLine(string.Format("Drawing {0} elements of type : {1}", drawaableRevitElements.Count(),
                                          this.GetType()));
            
            foreach (Element e in drawaableRevitElements)
            {
                Draw(this.RenderDescription, e);
            }
        }

        public static void DrawUndrawable(RenderDescription description, object obj)
        {
            //TODO: write a message, throw an exception, draw a question mark
        }

        public static void DrawReferencePoint(RenderDescription description, object obj)
        {
            var point = obj as ReferencePoint;

            if (point == null)
                return;

            description.points.Add(new Point3D(point.GetCoordinateSystem().Origin.X,
                point.GetCoordinateSystem().Origin.Y,
                point.GetCoordinateSystem().Origin.Z));
        }

        public static void DrawXYZ(RenderDescription description, object obj)
        {
            var point = obj as XYZ;
            if (point == null)
                return;

            description.points.Add(new Point3D(point.X, point.Y, point.Z));
        }

        public static void DrawCurve(RenderDescription description, object obj)
        {
            var curve = obj as Curve;

            if (curve == null)
                return;

            IList<XYZ> points = curve.Tessellate();

            for (int i = 0; i < points.Count; ++i)
            {
                XYZ xyz = points[i];

                description.lines.Add(new Point3D(xyz.X, xyz.Y, xyz.Z));

                if (i == 0 || i == (points.Count - 1))
                    continue;

                description.lines.Add(new Point3D(xyz.X, xyz.Y, xyz.Z));
            }
        }

        public static void DrawCurveElement(RenderDescription description, object obj)
        {
            var elem = obj as CurveElement;

            if (elem == null)
                return;

            DrawCurve(description, elem.GeometryCurve);
        }

        public static void DrawSolid(RenderDescription description, object obj)
        {
            var solid = obj as Solid;

            if (solid == null)
                return;

            foreach (Face f in solid.Faces)
            {
                DrawFace(description, f);
            }

            foreach (Edge edge in solid.Edges)
            {
                DrawCurve(description, edge.AsCurve());
            }
        }

        public static Point3D RevitPointToWindowsPoint(XYZ xyz)
        {
            return new Point3D(xyz.X, xyz.Y, xyz.Z);
        }

        // must return an array to make mesh double sided
        public static Mesh3D[] RevitMeshToHelixMesh(Mesh rmesh)
        {
            var indicesFront = new List<int>();
            var indicesBack = new List<int>();
            var vertices = new List<Point3D>();

            for (int i = 0; i < rmesh.NumTriangles; ++i)
            {
                MeshTriangle tri = rmesh.get_Triangle(i);

                for (int k = 0; k < 3; ++k)
                {
                    Point3D newPoint = RevitPointToWindowsPoint(tri.get_Vertex(k));

                    bool newPointExists = false;
                    for (int l = 0; l < vertices.Count; ++l)
                    {
                        Point3D p = vertices[l];
                        if ((p.X == newPoint.X) && (p.Y == newPoint.Y) && (p.Z == newPoint.Z))
                        {
                            indicesFront.Add(l);
                            newPointExists = true;
                            break;
                        }
                    }

                    if (newPointExists)
                        continue;

                    indicesFront.Add(vertices.Count);
                    vertices.Add(newPoint);
                }

                int a = indicesFront[indicesFront.Count - 3];
                int b = indicesFront[indicesFront.Count - 2];
                int c = indicesFront[indicesFront.Count - 1];

                indicesBack.Add(c);
                indicesBack.Add(b);
                indicesBack.Add(a);
            }

            var meshes = new List<Mesh3D>
            {
                new Mesh3D(vertices, indicesFront),
                new Mesh3D(vertices, indicesBack)
            };

            return meshes.ToArray();
        }

        public static void DrawFace(RenderDescription description, object obj)
        {
            var face = obj as Face;

            if (face == null)
                return;

            Mesh3D[] meshes = RevitMeshToHelixMesh(face.Triangulate(0.2));

            foreach (Mesh3D mesh in meshes)
            {
                description.meshes.Add(mesh);
            }
        }

        public static void DrawForm(RenderDescription description, object obj)
        {
            var form = obj as Form;

            if (form == null)
                return;

            DrawGeometryElement(description, form.get_Geometry(new Options()));
        }

        public static void DrawGeometryElement(RenderDescription description, object obj)
        {
            try
            {
                var gelem = obj as GeometryElement;

                foreach (GeometryObject go in gelem)
                {
                    DrawGeometryObject(description, go);
                }
            }
            catch (Exception ex)
            {
                dynSettings.Controller.DynamoViewModel.Log(ex.Message);
                dynSettings.Controller.DynamoViewModel.Log(ex.StackTrace);
            }

        }

        // Why the if/else statements? Most dynRevitTransactionNode are created
        // via a Python script. This keeps logic in the main C# code base.

        public static void DrawGeometryObject(RenderDescription description, object obj)
        {

            if (obj == null)
                return;

            if (obj is XYZ)
            {
                DrawXYZ(description, obj);
            }
            if (obj is Curve)
            {
                DrawCurve(description, obj);
            }
            else if (obj is Solid)
            {
                DrawSolid(description, obj);
            }
            else if (obj is Face)
            {
                DrawFace(description, obj);
            }
            else
            {
                DrawUndrawable(description, obj);
            }
        }

        // Elements can cantain many Geometry
        public static void DrawElement(RenderDescription description, object obj)
        {
            if (obj == null)
                return;

            if (obj is CurveElement)
            {
                DrawCurveElement(description, obj);
            }
            else if (obj is ReferencePoint)
            {
                DrawReferencePoint(description, obj);
            }
            else if (obj is Form)
            {
                DrawForm(description, obj);
            }
            else if (obj is GeometryElement)
            {
                DrawGeometryElement(description, obj);
            }
            else if (obj is GeometryObject)
            {
                DrawGeometryObject(description, obj);
            }
            else
            {
                var elem = obj as Element;
                if (elem != null)
                {
                    var o = new Options { DetailLevel = ViewDetailLevel.Medium };
                    GeometryElement geom = elem.get_Geometry(o);

                    if (geom != null)
                    {
                        DrawGeometryObject(description, geom);
                    }
                }
            }
        }

        public void Draw(RenderDescription description, object obj)
        {
            DrawElement(description, obj);
        }

        #endregion
        
        //TODO: Move handling of increments to wrappers for eval. Should never have to touch this in subclasses.
        /// <summary>
        /// Implementation detail, records how many times this Element has been executed during this run.
        /// </summary>
        private int _runCount;

        internal void ResetRuns()
        {
            if (_runCount > 0)
            {
                PruneRuns(_runCount);
                _runCount = 0;
            }
        }

        protected override void OnEvaluate()
        {
            base.OnEvaluate();

            _runCount++;
        }

        internal void PruneRuns(int numRuns)
        {
            Debug.WriteLine(string.Format("Pruning runs from {0} to {1}", elements.Count, numRuns));

            for (int i = elements.Count - 1; i >= numRuns; i--)
            {
                var elems = elements[i];
                var query = from e in elems
                            let el = UIDocument.Document.GetElement(e)
                            where el != null
                            select e;

                foreach (var e in query)
                    UIDocument.Document.Delete(e);

                elems.Clear();
            }

            if (elements.Count > numRuns)
            {
                elements.RemoveRange(
                   numRuns,
                   elements.Count - numRuns);
            }
        }

        protected override void __eval_internal(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            var controller = dynRevitSettings.Controller;

            bool debug = controller.DynamoViewModel.RunInDebug;

            if (!debug)
            {
                #region no debug

                if ((controller.DynamoViewModel as DynamoRevitViewModel).TransMode == 
                    DynamoRevitViewModel.TransactionMode.Manual && 
                    !controller.IsTransactionActive())
                {
                    throw new Exception("A Revit transaction is required in order evaluate this element.");
                }

                controller.InitTransaction();

                base.__eval_internal(args, outPuts);

                foreach (ElementId eid in _deletedIds)
                {
                    controller.RegisterSuccessfulDeleteHook(
                       eid,
                       onSuccessfulDelete);
                }
                _deletedIds.Clear();

                #endregion
            }
            else
            {
                #region debug

                dynSettings.Controller.DynamoViewModel.Log("Starting a debug transaction for element: " + NickName);

                IdlePromise.ExecuteOnIdle(
                   delegate
                   {
                       controller.InitTransaction();

                       try
                       {
                           base.__eval_internal(args, outPuts);

                           foreach (ElementId eid in _deletedIds)
                           {
                               controller.RegisterSuccessfulDeleteHook(
                                  eid,
                                  onSuccessfulDelete);
                           }
                           _deletedIds.Clear();

                           controller.EndTransaction();

                           ValidateConnections();
                       }
                       catch (Exception)
                       {
                           controller.CancelTransaction();
                           throw;
                       }
                   },
                   false
                );

                #endregion
            }

            #region Register Elements w/ DMU

            var del = new DynElementUpdateDelegate(onDeleted);

            foreach (ElementId id in Elements)
                controller.RegisterDeleteHook(id, del);

            #endregion
        }

        private readonly List<ElementId> _deletedIds = new List<ElementId>();

        /// <summary>
        /// Deletes an Element from the Document and removes all Dynamo regen hooks. If the second
        /// argument is true, then it will not delete from the Document, but will still remove all
        /// regen hooks.
        /// </summary>
        /// <param name="id">ID belonging to the element to be deleted.</param>
        /// <param name="hookOnly">Whether or not to only remove the regen hooks.</param>
        protected void DeleteElement(ElementId id, bool hookOnly = false)
        {
            if (!hookOnly)
                UIDocument.Document.Delete(id);
            _deletedIds.Add(id);
        }

        /// <summary>
        /// Destroy all elements belonging to this dynElement
        /// </summary>
        public override void Destroy()
        {
            var controller = dynRevitSettings.Controller;

            IdlePromise.ExecuteOnIdle(
               delegate
               {
                   controller.InitTransaction();
                   try
                   {
                       _runCount = 0;

                       var query = controller.DynamoViewModel.Model.HomeSpace.Nodes
                           .OfType<dynFunctionWithRevit>()
                           .Select(x => x.ElementsContainer)
                           .Where(c => c.HasElements(GUID))
                           .SelectMany(c => c[GUID]);

                       foreach (var els in query)
                       {
                           foreach (ElementId e in els)
                           {
                               try
                               {
                                   dynRevitSettings.Doc.Document.Delete(e);
                               }
                               catch (Autodesk.Revit.Exceptions.InvalidOperationException)
                               {
                                   //TODO: Flesh out?
                               }
                           }
                           els.Clear();
                       }
                   }
                   catch (Exception ex)
                   {
                       dynSettings.Controller.DynamoViewModel.Log(
                          "Error deleting elements: "
                          + ex.GetType().Name
                          + " -- " + ex.Message
                       );
                   }
                   controller.EndTransaction();
                   WorkSpace.Modified();
               });
        }

        void onDeleted(List<ElementId> deleted)
        {
            int count = elements.Sum(els => els.RemoveAll(deleted.Contains));

            if (!isDirty)
                isDirty = count > 0;
        }

        void onSuccessfulDelete(List<ElementId> deleted)
        {
            foreach (var els in elements)
                els.RemoveAll(deleted.Contains);
        }
    }

    public abstract class dynRevitTransactionNodeWithOneOutput : dynRevitTransactionNode
    {
        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            outPuts[OutPortData[0]] = Evaluate(args);
        }

        public abstract Value Evaluate(FSharpList<Value> args);
    }

    namespace SyncedNodeExtensions
    {
        public static class ElementSync
        {
            /// <summary>
            /// Registers the given element id with the DMU such that any change in the element will
            /// trigger a workspace modification event (dynamic running and saving).
            /// </summary>
            /// <param name="id">ElementId of the element to watch.</param>
            public static void RegisterEvalOnModified(this dynNodeModel node, ElementId id, Action modAction = null, Action delAction = null)
            {
                var u = dynRevitSettings.Controller.Updater;
                u.RegisterChangeHook(
                   id,
                   ChangeTypeEnum.Modify,
                   ReEvalOnModified(node, modAction)
                );
                u.RegisterChangeHook(
                   id,
                   ChangeTypeEnum.Delete,
                   UnRegOnDelete(delAction)
                );
            }

            /// <summary>
            /// Unregisters the given element id with the DMU. Should not be called unless it has already
            /// been registered with RegisterEvalOnModified
            /// </summary>
            /// <param name="id">ElementId of the element to stop watching.</param>
            public static void UnregisterEvalOnModified(this dynNodeModel node, ElementId id)
            {
                var u = dynRevitSettings.Controller.Updater;
                u.UnRegisterChangeHook(
                   id, ChangeTypeEnum.Modify
                );
                u.UnRegisterChangeHook(
                   id, ChangeTypeEnum.Delete
                );
            }

            static DynElementUpdateDelegate UnRegOnDelete(Action deleteAction)
            {
                return delegate(List<ElementId> deleted)
                {
                    foreach (var d in deleted)
                    {
                        var u = dynRevitSettings.Controller.Updater;
                        u.UnRegisterChangeHook(d, ChangeTypeEnum.Delete);
                        u.UnRegisterChangeHook(d, ChangeTypeEnum.Modify);
                    }
                    if (deleteAction != null)
                        deleteAction();
                };
            }

            static DynElementUpdateDelegate ReEvalOnModified(dynNodeModel node, Action modifiedAction)
            {
                return delegate(List<ElementId> modified)
                {
                    if (!node.RequiresRecalc && !dynRevitSettings.Controller.Running)
                    {
                        if (modifiedAction != null)
                            modifiedAction();
                        node.RequiresRecalc = true;
                    }
                };
            }
        }
    }
}
