using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSNodeServices;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryConversion;
using DSRevitNodes.References;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Line = Autodesk.DesignScript.Geometry.Line;
using Plane = Autodesk.DesignScript.Geometry.Plane;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodes.Elements
{
    /// <summary>
    /// A Revit ReferencePlane
    /// </summary>
    [RegisterForTrace]
    public class DSReferencePlane : AbstractElement
    {
        #region Internal properties

        /// <summary>
        /// Internal handle for the Revit object
        /// </summary>
        internal Autodesk.Revit.DB.ReferencePlane InternalReferencePlane
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalReferencePlane; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Internal reference plane
        /// </summary>
        /// <param name="referencePlane"></param>
        private DSReferencePlane( Autodesk.Revit.DB.ReferencePlane referencePlane)
        {
            this.InternalReferencePlane = referencePlane;
        }

        /// <summary>
        /// Constructor used internally by public static constructors
        /// </summary>
        /// <param name="bubbleEnd"></param>
        /// <param name="freeEnd"></param>
        private DSReferencePlane(XYZ bubbleEnd, XYZ freeEnd)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldEle =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.ReferencePlane>(Document);

            //There was an element, bind & mutate
            if (oldEle != null)
            {
                InternalSetReferencePlane(oldEle);
                if (InternalSetEndpoints(bubbleEnd, freeEnd))
                {
                    return;
                }

                // delete the old element, we couldn't update it for some reason
                DocumentManager.GetInstance().DeleteElement(oldEle.Id);
            }

            //Phase 2- There was no existing element, create new
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            Autodesk.Revit.DB.ReferencePlane refPlane;

            if (Document.IsFamilyDocument)
            {
                refPlane = Document.FamilyCreate.NewReferencePlane(
                    bubbleEnd,
                    freeEnd,
                    XYZ.BasisZ,
                    Document.ActiveView);
            }
            else
            {
                refPlane = Document.Create.NewReferencePlane(
                    bubbleEnd,
                    freeEnd,
                    XYZ.BasisZ,
                    Document.ActiveView );
            }

            InternalSetReferencePlane(refPlane);

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);

        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the InternalReferencePlane and update the element id and unique id
        /// </summary>
        /// <param name="rp"></param>
        private void InternalSetReferencePlane(Autodesk.Revit.DB.ReferencePlane rp)
        {
            this.InternalReferencePlane = rp;
            this.InternalElementId = rp.Id;
            this.InternalUniqueId = rp.UniqueId;
        }

        /// <summary>
        /// Mutate the two end points of the ReferencePlane 
        /// </summary>
        /// <param name="bubbleEnd"></param>
        /// <param name="freeEnd"></param>
        /// <returns>False if the operation failed</returns>
        private bool InternalSetEndpoints(XYZ bubbleEnd, XYZ freeEnd)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var refPlane = this.InternalReferencePlane;

            XYZ oldBubbleEnd = refPlane.BubbleEnd;
            XYZ oldFreeEnd = refPlane.FreeEnd;
            XYZ midPointOld = 0.5 * (oldBubbleEnd + oldFreeEnd);

            XYZ midPoint = 0.5 * (bubbleEnd + freeEnd);
            XYZ moveVec = XYZ.BasisZ.DotProduct(midPoint - midPointOld) * XYZ.BasisZ;

            // (sic) From Dynamo Legacy
            var success = true;
            try
            {
                ElementTransformUtils.MoveElement(Document, refPlane.Id, moveVec);
                refPlane.BubbleEnd = bubbleEnd;
                refPlane.FreeEnd = freeEnd;
            }
            catch
            {
                success = false;
            }

            TransactionManager.GetInstance().TransactionTaskDone();

            return success;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Get the internal Geometric Plane
        /// </summary>
        public Autodesk.DesignScript.Geometry.Plane Plane
        {
            get
            {
                return InternalReferencePlane.Plane.ToPlane();
            }
        }

        /// <summary>
        /// Get a reference to this plane for downstream Elements requiring it
        /// </summary>
        public DSPlaneReference PlaneReference
        {
            get
            {
                return new DSPlaneReference(InternalReferencePlane.Reference);
            }
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Form a ReferencePlane from a line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static DSReferencePlane ByLine(Autodesk.DesignScript.Geometry.Line line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            return new DSReferencePlane(line.StartPoint.ToXyz(), line.EndPoint.ToXyz());
        }

        /// <summary>
        /// Form a Refernece plane from two end points
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static DSReferencePlane ByStartPointEndPoint(Point start, Point end)
        {
            if (start == null)
            {
                throw new ArgumentNullException("start");
            }

            if (end == null)
            {
                throw new ArgumentNullException("end");
            }

            return new DSReferencePlane(start.ToXyz(), end.ToXyz());
        }

        #endregion

        #region Internal static constructors

        internal static DSReferencePlane FromExisting(ReferencePlane ele, bool isRevitOwned)
        {
            return new DSReferencePlane(ele)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
