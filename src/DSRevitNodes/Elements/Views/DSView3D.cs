using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSNodeServices;
using DSRevitNodes.GeometryConversion;
using DSRevitNodes.GeometryObjects;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace DSRevitNodes.Elements.Views
{
    /// <summary>
    /// A Revit View3D
    /// </summary>
    [RegisterForTrace]
    public class DSView3D : AbstractView
    {

        #region Internal properties

        /// <summary>
        /// An internal handle on the Revit element
        /// </summary>
        internal View3D InternalView3D
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        internal override Element InternalElement
        {
            get { return InternalView3D; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Private constructor
        /// </summary>
        private DSView3D(View3D view)
        {
            InternalSetView3D(view);
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private DSView3D(XYZ eye, XYZ target, Element element, string name, bool isolate, bool isPerspective)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var globalUp = XYZ.BasisZ;
            var direction = target.Subtract(eye);
            var up = direction.CrossProduct(globalUp).CrossProduct(direction);
            var orient = new ViewOrientation3D(eye, up, direction);

            View3D vd = Create3DView(orient, name, isPerspective);

            if (isolate)
            {
                IsolateElementInView( vd, element );
            }

            InternalSetView3D(vd);

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElementId);
        }

        #endregion

        #region Private helper methods

        /// <summary>
        /// Obtain a sparse point collection outlining a Revit element bt traversing it's
        /// GeometryObject representation
        /// </summary>
        /// <param name="e"></param>
        /// <param name="pts"></param>
        private static void GetPointCloud(Element e, List<XYZ> pts)
        {
            var options = new Options()
            {
                ComputeReferences = true,
                DetailLevel = ViewDetailLevel.Coarse,
                IncludeNonVisibleObjects = false
            };

            foreach (var gObj in e.get_Geometry(options))
            {
                if (gObj is Solid)
                {
                    GetPointCloud(gObj as Solid, pts);
                }
                else if (gObj is GeometryInstance)
                {
                    GetPointCloud(gObj as GeometryInstance, pts);
                }
            }
        }

        /// <summary>
        /// Obtain a point collection outlining a GeometryObject
        /// </summary>
        /// <param name="geomInst"></param>
        /// <param name="pts"></param>
        private static void GetPointCloud(GeometryInstance geomInst, List<XYZ> pts)
        {
            foreach (var gObj in geomInst.GetInstanceGeometry())
            {
                if (gObj is Solid)
                {
                    GetPointCloud(gObj as Solid, pts);
                }
                else if (gObj is GeometryInstance)
                {
                    GetPointCloud(gObj as GeometryInstance, pts);
                }
            }
        }

        /// <summary>
        /// Obtain a point collection outlining a Solid GeometryObject
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="pts"></param>
        private static void GetPointCloud(Solid solid, List<XYZ> pts)
        {
            foreach (Edge gEdge in solid.Edges)
            {
                var c = gEdge.AsCurve();
                if (c is Line)
                {
                    pts.Add(c.Evaluate(0, true));
                    pts.Add(c.Evaluate(1, true));
                }
                else
                {
                    IList<XYZ> xyzArray = gEdge.Tessellate();
                    pts.AddRange(xyzArray);
                }
            }
        }

        /// <summary>
        /// Make a single element appear in a particular view
        /// </summary>
        /// <param name="view"></param>
        /// <param name="element"></param>
        private static void IsolateElementInView(View3D view, Element element)
        {
            var fec = GetVisibleElementFilter();

            view.CropBoxActive = true;
                
            var all = fec.ToElements();
            var toHide =
                fec.ToElements().Where(x => !x.IsHidden(view) && x.CanBeHidden(view) && x.Id != element.Id).Select(x => x.Id).ToList();

            if (toHide.Count > 0)
                view.HideElements(toHide);

            // (sic)
            Document.Regenerate();

            if (view.IsPerspective)
            {
                var farClip = view.get_Parameter("Far Clip Active");
                farClip.Set(0);
            }
            else
            {
                var pts = new List<XYZ>();

                GetPointCloud(element, pts);

                var bounding = view.CropBox;
                var transInverse = bounding.Transform.Inverse;
                var transPts = pts.Select(transInverse.OfPoint).ToList();

                //ingore the Z coordindates and find
                //the max X ,Y and Min X, Y in 3d view.
                double dMaxX = 0, dMaxY = 0, dMinX = 0, dMinY = 0;

                bool bFirstPt = true;
                foreach (var pt1 in transPts)
                {
                    if (true == bFirstPt)
                    {
                        dMaxX = pt1.X;
                        dMaxY = pt1.Y;
                        dMinX = pt1.X;
                        dMinY = pt1.Y;
                        bFirstPt = false;
                    }
                    else
                    {
                        if (dMaxX < pt1.X)
                            dMaxX = pt1.X;
                        if (dMaxY < pt1.Y)
                            dMaxY = pt1.Y;
                        if (dMinX > pt1.X)
                            dMinX = pt1.X;
                        if (dMinY > pt1.Y)
                            dMinY = pt1.Y;
                    }
                }

                bounding.Max = new XYZ(dMaxX, dMaxY, bounding.Max.Z);
                bounding.Min = new XYZ(dMinX, dMinY, bounding.Min.Z);
                view.CropBox = bounding;
            }

            view.CropBoxVisible = false;

        }

        /// <summary>
        /// Create a Revit 3D View
        /// </summary>
        /// <param name="orient"></param>
        /// <param name="name"></param>
        /// <param name="isPerspective"></param>
        /// <returns></returns>
        private static View3D Create3DView(ViewOrientation3D orient, string name, bool isPerspective)
        {
            // (sic) From the Dynamo legacy implementation
            var viewFam = DocumentManager.GetInstance().ElementsOfType<ViewFamilyType>()
                .FirstOrDefault(x => x.ViewFamily == ViewFamily.ThreeDimensional);

            if (viewFam == null)
            {
                throw new Exception("There is no three dimensional view family int he document");
            }

            Autodesk.Revit.DB.View3D view;
            if (isPerspective)
            {
                view = View3D.CreatePerspective(Document, viewFam.Id);
            }
            else
            {
                view = View3D.CreateIsometric(Document, viewFam.Id);
            }

            view.SetOrientation(orient);
            view.SaveOrientationAndLock();

            try
            {
                //will fail if name is not unique
                view.Name = name;
            }
            catch
            {
                view.Name = CreateUniqueViewName(name);
            }


            return view;
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the InternalView3D property and the associated element id and unique id
        /// </summary>
        /// <param name="view"></param>
        private void InternalSetView3D(View3D view)
        {
            this.InternalView3D = view;
            this.InternalElementId = view.Id;
            this.InternalUniqueId = view.UniqueId;
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eyePoint"></param>
        /// <param name="target"></param>
        /// <param name="geomBoundingBox"></param>
        /// <param name="name"></param>
        /// <param name="isPerspective"></param>
        /// <returns></returns>
        public static DSView3D ByEyePointTargetAndElement(Autodesk.DesignScript.Geometry.Point eyePoint, Autodesk.DesignScript.Geometry.Point target, Element element, string name, bool isolateElement, bool isPerspective)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return new DSView3D(eyePoint.ToXyz(), target.ToXyz(), element, name, isolateElement, isPerspective);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create a View from a user selected Element.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static DSView3D FromExisting( View3D view, bool isRevitOwned )
        {
            return new DSView3D(view)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }
}


