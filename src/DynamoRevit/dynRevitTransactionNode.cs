using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dynamo.Controls;
using Dynamo.Nodes;
using Dynamo.Utilities;
using Autodesk.Revit.DB;

using Value = Dynamo.FScheme.Value;
using Microsoft.FSharp.Collections;
using Dynamo.Connectors;

using HelixToolkit.Wpf;

using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Dynamo.Revit
{
    public abstract class dynRevitTransactionNode : dynNodeModel, IDrawable
    {
        protected object drawableObject = null;
        protected Func<object, RenderDescription> drawMethod = null;

        private Type base_type = null;

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
                return dynRevitSettings.ElementsContainers.Peek()[this];
            }
        }

        // This list contains the elements of the current recurvise execution
        public List<ElementId> Elements
        {
            get
            {
                while (elements.Count <= runCount)
                    elements.Add(new List<ElementId>());
                return elements[runCount];
            }
            private set
            {
                elements[runCount] = value;
            }
        }

        public virtual RenderDescription Draw()
        {
            RenderDescription description = new RenderDescription();

            var drawaableRevitElements = elements.SelectMany(x => x.Select(y => dynRevitSettings.Doc.Document.GetElement(y)));

            Debug.WriteLine(string.Format("Drawing {0} elements of type : {1}", drawaableRevitElements.Count(),
                                          this.GetType()));
            
            foreach (Element e in drawaableRevitElements)
            {
                Draw(description, e);
            }
            return description;
        }

        public static void DrawUndrawable(RenderDescription description, object obj)
        {
            //TODO: write a message, throw an exception, draw a question mark
        }

        public static void DrawReferencePoint(RenderDescription description, object obj)
        {
            ReferencePoint point = obj as ReferencePoint;
            description.points.Add(new Point3D(point.GetCoordinateSystem().Origin.X,
                point.GetCoordinateSystem().Origin.Y,
                point.GetCoordinateSystem().Origin.Z));
        }

        public static void DrawXYZ(RenderDescription description, object obj)
        {
            XYZ point = obj as XYZ;
            description.points.Add(new Point3D(point.X, point.Y, point.Z));
        }

        public static void DrawCurve(RenderDescription description, object obj)
        {
            Autodesk.Revit.DB.Curve curve = obj as Autodesk.Revit.DB.Curve;

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
            Autodesk.Revit.DB.CurveElement elem = obj as Autodesk.Revit.DB.CurveElement;

            DrawCurve(description, elem.GeometryCurve);
        }

        public static void DrawSolid(RenderDescription description, object obj)
        {
            Autodesk.Revit.DB.Solid solid = obj as Autodesk.Revit.DB.Solid;

            foreach (Face f in solid.Faces)
            {
                DrawFace(description, f);
            }

            foreach (Edge edge in solid.Edges)
            {
                DrawCurve(description, edge.AsCurve());
            }
        }

        public static Point3D RevitPointToWindowsPoint(Autodesk.Revit.DB.XYZ xyz)
        {
            return new Point3D(xyz.X, xyz.Y, xyz.Z);
        }

        // must return an array to make mesh double sided
        public static Mesh3D[] RevitMeshToHelixMesh(Autodesk.Revit.DB.Mesh rmesh)
        {
            List<int> indices_front = new List<int>();
            List<int> indices_back = new List<int>();
            List<Point3D> vertices = new List<Point3D>();

            for (int i = 0; i < rmesh.NumTriangles; ++i)
            {
                MeshTriangle tri = rmesh.get_Triangle(i);

                for (int k = 0; k < 3; ++k)
                {
                    Point3D new_point = RevitPointToWindowsPoint(tri.get_Vertex(k));

                    bool new_point_exists = false;
                    for (int l = 0; l < vertices.Count; ++l)
                    {
                        Point3D p = vertices[l];
                        if ((p.X == new_point.X) && (p.Y == new_point.Y) && (p.Z == new_point.Z))
                        {
                            indices_front.Add(l);
                            new_point_exists = true;
                            break;
                        }
                    }

                    if (new_point_exists)
                        continue;

                    indices_front.Add(vertices.Count);
                    vertices.Add(new_point);
                }

                int a = indices_front[indices_front.Count - 3];
                int b = indices_front[indices_front.Count - 2];
                int c = indices_front[indices_front.Count - 1];

                indices_back.Add(c);
                indices_back.Add(b);
                indices_back.Add(a);
            }

            List<Mesh3D> meshes = new List<Mesh3D>();
            meshes.Add(new Mesh3D(vertices, indices_front));
            meshes.Add(new Mesh3D(vertices, indices_back));

            return meshes.ToArray();
        }

        public static void DrawFace(RenderDescription description, object obj)
        {
            Autodesk.Revit.DB.Face face = obj as Autodesk.Revit.DB.Face;

            Mesh3D[] meshes = RevitMeshToHelixMesh(face.Triangulate(0.2));

            foreach (Mesh3D mesh in meshes)
            {
                description.meshes.Add(mesh);
            }
        }

        public static void DrawForm(RenderDescription description, object obj)
        {
            Autodesk.Revit.DB.Form form = obj as Autodesk.Revit.DB.Form;

            DrawGeometryElement(description, form.get_Geometry(new Options()));
        }

        public static void DrawGeometryElement(RenderDescription description, object obj)
        {
            try
            {
                GeometryElement gelem = obj as GeometryElement;

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
            // Debugging code
            //string path = @"C:\Temp\" + System.Guid.NewGuid().ToString() + ".txt";
            //System.IO.File.WriteAllText(path, obj.GetType().Name);

            if (typeof(Autodesk.Revit.DB.XYZ).IsAssignableFrom(obj.GetType()))
            {
                DrawXYZ(description, obj);
            }
            if (typeof(Autodesk.Revit.DB.Curve).IsAssignableFrom(obj.GetType()))
            {
                DrawCurve(description, obj);
            }
            else if (typeof(Autodesk.Revit.DB.Solid).IsAssignableFrom(obj.GetType()))
            {
                DrawSolid(description, obj);
            }
            else if (typeof(Autodesk.Revit.DB.Face).IsAssignableFrom(obj.GetType()))
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
            if (typeof(Autodesk.Revit.DB.CurveElement).IsAssignableFrom(obj.GetType()))
            {
                DrawCurveElement(description, obj);
            }
            else if (typeof(Autodesk.Revit.DB.ReferencePoint).IsAssignableFrom(obj.GetType()))
            {
                DrawReferencePoint(description, obj);
            }
            else if (typeof(Autodesk.Revit.DB.Form).IsAssignableFrom(obj.GetType()))
            {
                DrawForm(description, obj);
            }
            else if (typeof(Autodesk.Revit.DB.GeometryElement).IsAssignableFrom(obj.GetType()))
            {
                DrawGeometryElement(description, obj);
            }
            else if (typeof (Autodesk.Revit.DB.GeometryObject).IsAssignableFrom(obj.GetType()))
            {
                DrawGeometryObject(description, obj);
            }
            else
            {
                Element elem = obj as Element;
                DrawGeometryElement(description, elem.get_Geometry(new Options()));
            }
        }

        public void Draw(RenderDescription description, object obj)
        {
            //string path = @"C:\Temp\" + System.Guid.NewGuid().ToString() + ".txt";
            //System.IO.File.WriteAllText(path, obj.GetType().Name);

            DrawElement(description, obj);
        }
        
        //TODO: Move handling of increments to wrappers for eval. Should never have to touch this in subclasses.
        /// <summary>
        /// Implementation detail, records how many times this Element has been executed during this run.
        /// </summary>
        private int runCount;

        internal void ResetRuns()
        {
            if (runCount > 0)
            {
                PruneRuns(runCount);
                runCount = 0;
            }
        }

        protected override void OnEvaluate()
        {
            base.OnEvaluate();

            runCount++;
        }

        internal void PruneRuns(int runCount)
        {
            Debug.WriteLine(string.Format("Pruning runs from {0} to {1}", elements.Count, runCount));

            for (int i = elements.Count - 1; i >= runCount; i--)
            {
                var elems = elements[i];
                foreach (var e in elems)
                {
                    UIDocument.Document.Delete(e);
                }
                elems.Clear();
            }

            if (elements.Count > runCount)
            {
                elements.RemoveRange(
                   runCount,
                   elements.Count - runCount
                );
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

                Evaluate(args, outPuts);

                foreach (ElementId eid in deletedIds)
                {
                    controller.RegisterSuccessfulDeleteHook(
                       eid,
                       onSuccessfulDelete
                    );
                }
                deletedIds.Clear();

                #endregion
            }
            else
            {
                #region debug

                Bench.Dispatcher.Invoke(new Action(
                   () =>
                      dynSettings.Controller.DynamoViewModel.Log("Starting a debug transaction for element: " + NickName)
                ));

                IdlePromise.ExecuteOnIdle(
                   delegate
                   {
                       controller.InitTransaction();

                       try
                       {
                           Evaluate(args, outPuts);

                           foreach (ElementId eid in deletedIds)
                           {
                               controller.RegisterSuccessfulDeleteHook(
                                  eid,
                                  onSuccessfulDelete
                               );
                           }
                           deletedIds.Clear();

                           controller.EndTransaction();

                           ValidateConnections();
                       }
                       catch (Exception ex)
                       {
                           controller.CancelTransaction();
                           throw ex;
                       }
                   }
                );

                #endregion
            }

            #region Register Elements w/ DMU

            var del = new DynElementUpdateDelegate(onDeleted);

            foreach (ElementId id in Elements)
                controller.RegisterDeleteHook(id, del);

            #endregion
        }

        private List<ElementId> deletedIds = new List<ElementId>();
        protected void DeleteElement(ElementId id, bool hookOnly = false)
        {
            if (!hookOnly)
                UIDocument.Document.Delete(id);
            deletedIds.Add(id);
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
                       runCount = 0;

                       var query = controller.DynamoViewModel.Model.HomeSpace.Nodes
                           .Where(x => x is dynFunctionWithRevit)
                           .Select(x => (x as dynFunctionWithRevit).ElementsContainer)
                           .Where(c => c.HasElements(this))
                           .SelectMany(c => c[this]);

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
               },
               true
            );
        }

        void onDeleted(List<ElementId> deleted)
        {
            int count = 0;
            foreach (var els in elements)
            {
                count += els.RemoveAll(deleted.Contains);
            }

            if (!isDirty)
                isDirty = count > 0;
        }

        void onSuccessfulDelete(List<ElementId> deleted)
        {
            foreach (var els in elements)
                els.RemoveAll(x => deleted.Contains(x));
        }
    }

    public class dynRevitTransactionNodeWithOneOutput : dynRevitTransactionNode
    {
        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            outPuts[OutPortData[0]] = Evaluate(args);
        }

        public virtual Value Evaluate(FSharpList<Value> args)
        {
            throw new NotImplementedException();
        }
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
