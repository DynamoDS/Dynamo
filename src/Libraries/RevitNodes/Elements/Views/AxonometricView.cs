using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.GeometryConversion;
using Revit.GeometryObjects;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements.Views
{
    /// <summary>
    /// A Revit View3D
    /// </summary>
    [RegisterForTrace]
    public class AxonometricView : View3D
    {
        #region Private constructors

        /// <summary>
        /// Private constructor
        /// </summary>
        private AxonometricView(Autodesk.Revit.DB.View3D view)
        {
            InternalSetView3D(view);
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private AxonometricView(XYZ eye, XYZ target, BoundingBoxXYZ bbox, string name = DEFAULT_VIEW_NAME, bool isolate = false)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldEle =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.View3D>(Document);

            //Phase 2 - There was no existing Element, create new one
            TransactionManager.Instance.EnsureInTransaction(Document);

            // Rebind to Element
            if (oldEle != null)
            {
                InternalSetView3D(oldEle);
                InternalSetOrientation(BuildOrientation3D(eye, target));
                InternalSetIsolation(bbox, isolate);
                InternalSetName(name);
                return;
            }

            var vd = Create3DView(BuildOrientation3D(eye, target), name, false);
            InternalSetView3D(vd);
            InternalSetIsolation(bbox, isolate);
            InternalSetName(name);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElement);
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private AxonometricView(XYZ eye, XYZ target, string name = DEFAULT_VIEW_NAME, Autodesk.Revit.DB.Element element = null, bool isolate = false)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldEle =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.View3D>(Document);

            //Phase 2 - There was no existing Element, create new one
            TransactionManager.Instance.EnsureInTransaction(Document);

            // Rebind to Element
            if (oldEle != null)
            {
                InternalSetView3D(oldEle);
                InternalSetOrientation(BuildOrientation3D(eye, target));
                InternalSetIsolation(element, isolate);
                InternalSetName(name);
                return;
            }

            var vd = Create3DView(BuildOrientation3D(eye, target), name, false);
            InternalSetView3D(vd);
            InternalSetIsolation(element, isolate);
            InternalSetName(name);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElement);
        }

        private void InternalSetIsolation(Autodesk.Revit.DB.Element element, bool isolate)
        {
            if (isolate && element != null)
                InternalIsolateInView(element);
            else
                InternalRemoveIsolation();
        }

        private void InternalSetIsolation(Autodesk.Revit.DB.BoundingBoxXYZ bbox, bool isolate)
        {
            if (isolate && bbox != null)
                InternalIsolateInView(bbox);
            else
                InternalRemoveIsolation();
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Axonometric (isometric) View from an eye position
        /// and a target position.
        /// </summary>
        /// <param name="eyePoint">A Point representing the eye point in meters.</param>
        /// <param name="target">A Point representing the target of view in meters.</param>
        /// <param name="name">The name of the view.</param>
        /// <returns>An AxonometricView object.</returns>
        public static AxonometricView ByEyePointAndTarget(
            Autodesk.DesignScript.Geometry.Point eyePoint,
            Autodesk.DesignScript.Geometry.Point target, 
            string name = DEFAULT_VIEW_NAME)
        {
            if (eyePoint == null)
                throw new ArgumentNullException("eyePoint");

            if (target == null)
                throw new ArgumentNullException("target");

            if (name == null)
            {
                name = DEFAULT_VIEW_NAME;
            }

            return ByEyePointTargetAndElement(eyePoint,
                    target, name);
        }

        /// <summary>
        /// Create a Revit Axonometric (isometric) View from an Eye position and target position and Element
        /// </summary>
        /// <param name="eyePoint">A Point representing the eye point.</param>
        /// <param name="target">A Point representing the target of view.</param>
        /// <param name="element">This argument cannot be null, and it has to be either a 
        /// Revit.Elements.Element or  Revit.GeometryObjectsBoundingBox.</param>
        /// <param name="name">The name of the view.</param>
        /// <param name="isolateElement">If this argument is set to true, the element or 
        /// bounding box will be isolated in the current view by creating a minimum size
        /// crop box around it.</param>
        /// <returns>An AxonometricView object.</returns>
        public static AxonometricView ByEyePointTargetAndElement(
            Autodesk.DesignScript.Geometry.Point eyePoint, 
            Autodesk.DesignScript.Geometry.Point target,
            string name = DEFAULT_VIEW_NAME, 
            Element element = null, 
            bool isolateElement = false)
        {
            if (eyePoint == null)
                throw new ArgumentNullException("eyePoint");

            if (target == null)
                throw new ArgumentNullException("target");

            if (name == null)
            {
                name = DEFAULT_VIEW_NAME;
            }

            if (element == null)
            {
                return new AxonometricView(
                    eyePoint.ToXyz(true),
                    target.ToXyz(true),
                    name,
                    null,
                    isolateElement);
            }
            else
            {
                return new AxonometricView(
                    eyePoint.ToXyz(true),
                    target.ToXyz(true),
                    name,
                    element.InternalElement,
                    isolateElement);
            }
            
        }

        /// <summary>
        /// Create a Revit Axonometric (isometric) View from an Eye position and target position and Bounding Box
        /// </summary>
        /// <param name="eyePoint">A Point representing the eye point.</param>
        /// <param name="target">A Point representing the target of view.</param>
        /// <param name="boundingBox">A BoundingBox. The view will be cropped to this bounding box</param>
        /// <param name="name">The name of the view.</param>
        /// <param name="isolateElement">If this argument is set to true, the element or 
        /// bounding box will be isolated in the current view by creating a minimum size
        /// crop box around it.</param>
        /// <returns>An AxonometricView object.</returns>
        public static AxonometricView ByEyePointTargetAndBoundingBox(Autodesk.DesignScript.Geometry.Point eyePoint, 
            Autodesk.DesignScript.Geometry.Point target, 
            Autodesk.DesignScript.Geometry.BoundingBox boundingBox, 
            string name = DEFAULT_VIEW_NAME, 
            bool isolateElement = false)
        {
            if (boundingBox == null)
            {
                throw new ArgumentNullException("boundingBox");
            }

            if (eyePoint == null)
                throw new ArgumentNullException("eyePoint");

            if (target == null)
                throw new ArgumentNullException("target");

            if (name == null)
            {
                name = DEFAULT_VIEW_NAME;
            }

            return new AxonometricView(eyePoint.ToXyz(true), target.ToXyz(true), boundingBox.ToRevitType(true), name, isolateElement);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create from an existing Revit Element
        /// </summary>
        /// <param name="view"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static AxonometricView FromExisting(Autodesk.Revit.DB.View3D view, bool isRevitOwned)
        {
            return new AxonometricView(view)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }
}
