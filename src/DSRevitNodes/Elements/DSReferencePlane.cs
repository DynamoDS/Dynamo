using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DSNodeServices;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Line = Autodesk.DesignScript.Geometry.Line;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodes
{
    [RegisterForTrace]
    public class DSReferencePlane : AbstractElement
    {

        private Autodesk.Revit.DB.ReferencePlane InternalReferencePlane;

        #region Private constructors

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

        private void InternalSetReferencePlane(Autodesk.Revit.DB.ReferencePlane rp)
        {
            this.InternalReferencePlane = rp;
            this.InternalElementId = rp.Id;
            this.InternalUniqueId = rp.UniqueId;
        }

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

        #region Public static constructors

        public static DSReferencePlane ByLine(Autodesk.DesignScript.Geometry.Line line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

            return new DSReferencePlane(line.StartPoint.ToXyz(), line.EndPoint.ToXyz());
        }

        public static DSReferencePlane ByStartPtEndPt(Point start, Point end)
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

    }
}
