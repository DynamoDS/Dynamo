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
using Dynamo.FSchemeInterop;
using HelixToolkit.Wpf;

using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml;

namespace Dynamo.Revit
{
    public abstract class dynRevitTransactionNode : dynNodeModel, IDrawable
    {
        protected object drawableObject = null;
        protected Func<object, RenderDescription> drawMethod = null;

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

        public RenderDescription RenderDescription { get; set; }

        public override void SaveElement(XmlDocument xmlDoc, XmlElement dynEl)
        {
            //Only save elements in the home workspace
            if (WorkSpace is FuncWorkspace)
                return;

            foreach (var run in elements)
            {
                var outEl = xmlDoc.CreateElement("Run");

                foreach (var id in run)
                {
                    Element e;
                    if (dynUtils.TryGetElement(id, out e))
                    {
                        var elementStore = xmlDoc.CreateElement("Element");
                        elementStore.InnerText = e.UniqueId;
                        outEl.AppendChild(elementStore);
                    }
                }
                dynEl.AppendChild(outEl);
            }
        }

        public override void LoadElement(XmlNode elNode)
        {
            var del = new DynElementUpdateDelegate(onDeleted);

            elements.Clear();

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
                            var eid = subNode.InnerText;
                            try
                            {
                                var id = UIDocument.Document.GetElement(eid).Id;
                                runElements.Add(id);
                                dynRevitSettings.Controller.RegisterDeleteHook(id, del);
                            }
                            catch (NullReferenceException)
                            {
                                dynSettings.Controller.DynamoViewModel.Log("Element with UID \"" + eid + "\" not found in Document.");
                            }
                        }
                    }
                }
            }
        }

        internal void RegisterAllElementsDeleteHook()
        {
            var del = new DynElementUpdateDelegate(onDeleted);

            foreach (var eList in elements)
            {
                foreach (var id in eList)
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

        #endregion
        
        //TODO: Move handling of increments to wrappers for eval. Should never have to touch this in subclasses.
        /// <summary>
        /// Implementation detail, records how many times this Element has been executed during this run.
        /// </summary>
        protected int runCount;

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

                dynSettings.Controller.DynamoViewModel.Log("Starting a debug transaction for element: " + NickName);

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

        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            //if this element maintains a collcection of references
            //then clear the collection
            if (this is IClearable)
                (this as IClearable).ClearReferences();

            List<FSharpList<Value>> argSets = new List<FSharpList<Value>>();

            //create a zip of the incoming args and the port data
            //to be used for type comparison
            var portComparison = args.Zip(InPortData, (first, second) => new Tuple<Type, Type>(first.GetType(), second.PortType));

            //if any value is a list whose expectation is a single
            //do an auto map
            //TODO: figure out a better way to do this than using a lot
            //of specific excludes
            if (args.Count() > 0 &&
                portComparison.Any(x => x.Item1 == typeof(Value.List) &&
                x.Item2 != typeof(Value.List)) &&
                !(this.ArgumentLacing == LacingStrategy.Disabled))
            {
                //if the argument is of the expected type, then
                //leave it alone otherwise, wrap it in a list
                int j = 0;
                foreach (var arg in args)
                {
                    //incoming value is list and expecting single
                    if (portComparison.ElementAt(j).Item1 == typeof(Value.List) &&
                        portComparison.ElementAt(j).Item2 != typeof(Value.List))
                    {
                        //leave as list
                        argSets.Add(((Value.List)arg).Item);
                    }
                    //incoming value is list and expecting list
                    else
                    {
                        //wrap in list
                        argSets.Add(Utils.MakeFSharpList(arg));
                    }
                    j++;
                }

                IEnumerable<IEnumerable<Value>> lacedArgs = null;
                switch (this.ArgumentLacing)
                {
                    case LacingStrategy.First:
                        lacedArgs = argSets.SingleSet();
                        break;
                    case LacingStrategy.Shortest:
                        lacedArgs = argSets.ShortestSet();
                        break;
                    case LacingStrategy.Longest:
                        lacedArgs = argSets.LongestSet();
                        break;
                    case LacingStrategy.CrossProduct:
                        lacedArgs = argSets.CartesianProduct();
                        break;
                }

                //setup a list to hold the results
                //each output will have its own results collection
                List<FSharpList<Value>> results = new List<FSharpList<FScheme.Value>>();
                for(int i=0; i<OutPortData.Count(); i++)
                {
                    results.Add(FSharpList<Value>.Empty);
                }
                //FSharpList<Value> result = FSharpList<Value>.Empty;

                //run the evaluate method for each set of 
                //arguments in the la result. do these
                //in reverse order so our cons comes out the right
                //way around
                for (int i = lacedArgs.Count() - 1; i >= 0; i--)
                {
                    var evalResult = Evaluate(Utils.MakeFSharpList(lacedArgs.ElementAt(i).ToArray()));

                    //if the list does not have the same number of items
                    //as the number of output ports, then throw a wobbly
                    if (!evalResult.IsList)
                        throw new Exception("Output value of the node is not a list.");

                    for (int k = 0; k < OutPortData.Count(); k++)
                    {
                        FSharpList<Value> lst = ((Value.List)evalResult).Item;
                        results[k] = FSharpList<Value>.Cons(lst[k], results[k]);
                    }
                    runCount++;
                }

                //the result of evaluation will be a list. we split that result
                //and send the results to the outputs
                for (int i = 0; i < OutPortData.Count(); i++)
                {
                    outPuts[OutPortData[i]] = Value.NewList(results[i]);      
                }
                
            }
            else
            {
                Value evalResult = Evaluate(args);

                if (!evalResult.IsList)
                        throw new Exception("Output value of the node is not a list.");

                FSharpList<Value> lst = ((Value.List)evalResult).Item;

                //the result of evaluation will be a list. we split that result
                //and send the results to the outputs
                for (int i = 0; i < OutPortData.Count(); i++)
                {
                    outPuts[OutPortData[i]] = lst[i];
                }
            }

            ValidateConnections();
        }

        public virtual Value Evaluate(FSharpList<Value> args)
        {
            throw new NotImplementedException();
        }
    }

    public class dynRevitTransactionNodeWithOneOutput : dynRevitTransactionNode
    {
        public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        {
            //THE OLD WAY
            //outPuts[OutPortData[0]] = Evaluate(args);

            //THE NEW WAY
            //if this element maintains a collcection of references
            //then clear the collection
            if (this is IClearable)
                (this as IClearable).ClearReferences();

            List<FSharpList<Value>> argSets = new List<FSharpList<Value>>();

            //create a zip of the incoming args and the port data
            //to be used for type comparison
            var portComparison = args.Zip(InPortData, (first, second) => new Tuple<Type, Type>(first.GetType(), second.PortType));

            //if any value is a list whose expectation is a single
            //do an auto map
            //TODO: figure out a better way to do this than using a lot
            //of specific excludes
            if (args.Count() > 0 &&
                portComparison.Any(x => x.Item1 == typeof(Value.List) &&
                x.Item2 != typeof(Value.List)) &&
                !(this.ArgumentLacing == LacingStrategy.Disabled))
            {
                //if the argument is of the expected type, then
                //leave it alone otherwise, wrap it in a list
                int j = 0;
                foreach (var arg in args)
                {
                    //incoming value is list and expecting single
                    if (portComparison.ElementAt(j).Item1 == typeof(Value.List) &&
                        portComparison.ElementAt(j).Item2 != typeof(Value.List))
                    {
                        //leave as list
                        argSets.Add(((Value.List)arg).Item);
                    }
                    //incoming value is list and expecting list
                    else
                    {
                        //wrap in list
                        argSets.Add(Utils.MakeFSharpList(arg));
                    }
                    j++;
                }

                IEnumerable<IEnumerable<Value>> lacedArgs = null;
                switch (this.ArgumentLacing)
                {
                    case LacingStrategy.First:
                        lacedArgs = argSets.SingleSet();
                        break;
                    case LacingStrategy.Shortest:
                        lacedArgs = argSets.ShortestSet();
                        break;
                    case LacingStrategy.Longest:
                        lacedArgs = argSets.LongestSet();
                        break;
                    case LacingStrategy.CrossProduct:
                        lacedArgs = argSets.CartesianProduct();
                        break;
                }

                //setup an empty list to hold results
                FSharpList<Value> result = FSharpList<Value>.Empty;

                //run the evaluate method for each set of 
                //arguments in the cartesian result. do these
                //in reverse order so our cons comes out the right
                //way around
                for (int i = lacedArgs.Count() - 1; i >= 0; i--)
                {
                    var evalResult = Evaluate(Utils.MakeFSharpList(lacedArgs.ElementAt(i).ToArray()));
                    result = FSharpList<Value>.Cons(evalResult, result);
                    runCount++;
                }

                outPuts[OutPortData[0]] = Value.NewList(result);
            }
            else
            {
                outPuts[OutPortData[0]] = Evaluate(args);
            }
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
