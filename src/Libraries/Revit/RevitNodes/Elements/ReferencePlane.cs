using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI;
using DSNodeServices;
using Revit.Elements;
using Revit.GeometryConversion;
using Revit.References;
using RevitServices.Persistence;
using RevitServices.Transactions;
using ArgumentException = System.ArgumentException;
using ArgumentNullException = System.ArgumentNullException;
using Line = Autodesk.DesignScript.Geometry.Line;
using Plane = Autodesk.DesignScript.Geometry.Plane;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit ReferencePlane
    /// </summary>
    [RegisterForTrace]
    public class ReferencePlane : AbstractElement
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
        private ReferencePlane( Autodesk.Revit.DB.ReferencePlane referencePlane)
        {
            this.InternalReferencePlane = referencePlane;
        }

        /// <summary>
        /// Constructor used internally by public static constructors
        /// </summary>
        /// <param name="bubbleEnd"></param>
        /// <param name="freeEnd"></param>
        private ReferencePlane(XYZ bubbleEnd, XYZ freeEnd, XYZ normal, Autodesk.Revit.DB.View view )
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
                DocumentManager.Instance.DeleteElement(oldEle.Id);
            }

            //Phase 2- There was no existing element, create new
            TransactionManager.Instance.EnsureInTransaction(Document);

            Autodesk.Revit.DB.ReferencePlane refPlane;

            if (Document.IsFamilyDocument)
            {
                refPlane = Document.FamilyCreate.NewReferencePlane(
                    bubbleEnd,
                    freeEnd,
                    normal, 
                    view );
            }
            else
            {
                refPlane = Document.Create.NewReferencePlane(
                    bubbleEnd,
                    freeEnd,
                    normal,
                    view );
            }

            InternalSetReferencePlane(refPlane);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElement);

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
            TransactionManager.Instance.EnsureInTransaction(Document);

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

            TransactionManager.Instance.TransactionTaskDone();

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
        public PlaneReference PlaneReference
        {
            get
            {
                return new PlaneReference(InternalReferencePlane.Reference);
            }
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Form a ReferencePlane from a line in the Active view.  The cut vector is the Z Axis.
        /// </summary>
        /// <param name="line">The line where the bubble wil be located at the start</param>
        /// <returns></returns>
        public static ReferencePlane ByLine( Autodesk.DesignScript.Geometry.Line line )
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            var start = line.StartPoint.ToXyz();
            var end = line.EndPoint.ToXyz();
            var norm = (end - start).GetParallel();

            return new ReferencePlane(  start, 
                                        end,
                                        norm, 
                                        Document.ActiveView );
        }

        /// <summary>
        /// Form a Refernece plane from two end points in the Active view.  The cut vector is the Z Axis.
        /// </summary>
        /// <param name="start">The location where the bubble will be located</param>
        /// <param name="end">The other end</param>
        /// <returns></returns>
        public static ReferencePlane ByStartPointEndPoint( Point start, Point end )
        {
            if (start == null)
            {
                throw new ArgumentNullException("start");
            }

            if (end == null)
            {
                throw new ArgumentNullException("end");
            }

            return new ReferencePlane(  start.ToXyz(), 
                                        end.ToXyz(),
                                        (end.ToXyz() - start.ToXyz()).GetParallel(),
                                        Document.ActiveView);
        }

        #endregion

        #region Internal static constructors

        internal static ReferencePlane FromExisting(Autodesk.Revit.DB.ReferencePlane ele, bool isRevitOwned)
        {
            return new ReferencePlane(ele)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
