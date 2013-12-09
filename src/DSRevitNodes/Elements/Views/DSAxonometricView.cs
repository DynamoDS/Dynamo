using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSNodeServices;
using DSRevitNodes.GeometryConversion;
using DSRevitNodes.GeometryObjects;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace DSRevitNodes.Elements
{
    /// <summary>
    /// A Revit View3D
    /// </summary>
    [RegisterForTrace]
    public class DSAxonometricView : AbstractView3D
    {

        #region Private constructors

        /// <summary>
        /// Private constructor
        /// </summary>
        private DSAxonometricView(View3D view)
        {
            InternalSetView3D(view);
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private DSAxonometricView(XYZ eye, XYZ target, BoundingBoxXYZ bbox, string name, bool isolate)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldEle =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.View3D>(Document);

            // Rebind to Element
            if (oldEle != null)
            {
                InternalSetView3D(oldEle);
                InternalSetOrientation(BuildOrientation3D(eye, target));
                if (isolate)
                {
                    InternalIsolateInView(bbox);
                }
                else
                {
                    InternalRemoveIsolation();
                }
                InternalSetName(name);
                return;
            }

            //Phase 2 - There was no existing Element, create new one
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var vd = Create3DView(BuildOrientation3D(eye, target), name, false);
            InternalSetView3D(vd);
            if (isolate)
            {
                InternalIsolateInView(bbox);
            }
            else
            {
                InternalRemoveIsolation();
            }
            InternalSetName(name);

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private DSAxonometricView(XYZ eye, XYZ target, Element element, string name, bool isolate)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldEle =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.View3D>(Document);

            // Rebind to Element
            if (oldEle != null)
            {
                InternalSetView3D(oldEle);
                InternalSetOrientation(BuildOrientation3D(eye, target));
                if (isolate)
                {
                    InternalIsolateInView(element);
                }
                else
                {
                    InternalRemoveIsolation();
                }
                InternalSetName(name);
                return;
            }

            //Phase 2 - There was no existing Element, create new one
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var vd = Create3DView(BuildOrientation3D(eye, target), name, false);
            InternalSetView3D(vd);
            if (isolate)
            {
                InternalIsolateInView(element);
            }
            else
            {
                InternalRemoveIsolation();
            }
            InternalSetName(name);

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Perspective View from an Eye position and target position and Element
        /// </summary>
        /// <param name="eyePoint"></param>
        /// <param name="target"></param>
        /// <param name="element"></param>
        /// <param name="name"></param>
        /// <param name="isolateElement"></param>
        /// <returns></returns>
        public static DSAxonometricView ByEyePointTargetAndElement(Autodesk.DesignScript.Geometry.Point eyePoint, Autodesk.DesignScript.Geometry.Point target, AbstractElement element, string name, bool isolateElement)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return new DSAxonometricView(eyePoint.ToXyz(), target.ToXyz(), element.InternalElement, name, isolateElement);
        }

        /// <summary>
        /// Create a Revit Perspective View from an Eye position and target position and Bounding Box
        /// </summary>
        /// <param name="eyePoint"></param>
        /// <param name="target"></param>
        /// <param name="boundingBox"></param>
        /// <param name="name"></param>
        /// <param name="isolateElement"></param>
        /// <returns></returns>
        public static DSAxonometricView ByEyePointTargetAndBoundingBox(Autodesk.DesignScript.Geometry.Point eyePoint, Autodesk.DesignScript.Geometry.Point target, DSBoundingBox boundingBox, string name, bool isolateElement)
        {
            if (boundingBox == null)
            {
                throw new ArgumentNullException("boundingBox");
            }

            return new DSAxonometricView(eyePoint.ToXyz(), target.ToXyz(), boundingBox.InternalBoundingBoxXyz, name, isolateElement);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create from an existing Revit Element
        /// </summary>
        /// <param name="view"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static DSAxonometricView FromExisting(View3D view, bool isRevitOwned)
        {
            return new DSAxonometricView(view)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }
}
