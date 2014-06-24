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
        private AxonometricView(XYZ eye, XYZ target, BoundingBoxXYZ bbox, string name, bool isolate)
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
            TransactionManager.Instance.EnsureInTransaction(Document);

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

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElement);
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private AxonometricView(XYZ eye, XYZ target, Autodesk.Revit.DB.Element element, string name, bool isolate)
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
            TransactionManager.Instance.EnsureInTransaction(Document);

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

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElement);
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Axonometric (isometric) View from an Eye position, 
        /// a target position, and either an Element or BoundingBox.
        /// </summary>
        /// <param name="eyePoint">A Point representing the eye point in meters.</param>
        /// <param name="target">A Point representing the target of view in meters.</param>
        /// <param name="element">This argument cannot be null, and it has to be either a 
        /// Revit.Elements.Element or  Revit.GeometryObjectsBoundingBox.</param>
        /// <param name="name">The name of the view.</param>
        /// <param name="isolateElement">If this argument is set to true, the element or 
        /// bounding box will be isolated in the current view by creating a minimum size
        /// crop box around it.</param>
        /// <returns>Returns the resulting AxonometricView object.</returns>
        /// 
        public static AxonometricView ByEyePointAndTarget(
            Autodesk.DesignScript.Geometry.Point eyePoint,
            Autodesk.DesignScript.Geometry.Point target,
            object element, string name, bool isolateElement)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (eyePoint == null)
                throw new ArgumentNullException("eyePoint");

            if (target == null)
                throw new ArgumentNullException("target");

            if (element == null)
                throw new ArgumentNullException("element");

            if (name == null)
                throw new ArgumentNullException("name");

            Element abstractElement = element as Element;
            if (abstractElement != null)
            {
                return ByEyePointTargetAndElement(eyePoint,
                    target, abstractElement, name, isolateElement);
            }

            BoundingBox boundingBox = element as BoundingBox;
            if (boundingBox != null)
            {
                return ByEyePointTargetAndBoundingBox(eyePoint,
                    target, boundingBox, name, isolateElement);
            }

            string message = string.Format("Argument is expected to be of type " +
                "'Revit.Elements.AbstractElement' or ' Revit.GeometryObjectsBoundingBox', " +
                "but found to be of type '{0}'", element.GetType());

            throw new ArgumentException(message, "element");
        }

        /// <summary>
        /// Create a Revit Axonometric (isometric) View from an Eye position and target position and Element
        /// </summary>
        /// <param name="eyePoint">Eye point in meters</param>
        /// <param name="target">Target of view in meters</param>
        /// <param name="element"></param>
        /// <param name="name"></param>
        /// <param name="isolateElement"></param>
        /// <returns></returns>
        public static AxonometricView ByEyePointTargetAndElement(
            Autodesk.DesignScript.Geometry.Point eyePoint, 
            Autodesk.DesignScript.Geometry.Point target, 
            Element element, 
            string name, bool isolateElement)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (eyePoint == null)
                throw new ArgumentNullException("eyePoint");

            if (target == null)
                throw new ArgumentNullException("target");

            if (element == null)
                throw new ArgumentNullException("element");

            if (name == null)
                throw new ArgumentNullException("name");


            return new AxonometricView(eyePoint.ToXyz(true), target.ToXyz(true), element.InternalElement, name, isolateElement);
        }

        /// <summary>
        /// Create a Revit Axonometric (isometric) View from an Eye position and target position and Bounding Box
        /// </summary>
        /// <param name="eyePoint">Eye point in meters</param>
        /// <param name="target">Target of view in meters</param>
        /// <param name="boundingBox">Bounding box represented in meters</param>
        /// <param name="name"></param>
        /// <param name="isolateElement"></param>
        /// <returns></returns>
        public static AxonometricView ByEyePointTargetAndBoundingBox(Autodesk.DesignScript.Geometry.Point eyePoint, Autodesk.DesignScript.Geometry.Point target, Autodesk.DesignScript.Geometry.BoundingBox boundingBox, string name, bool isolateElement)
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
                throw new ArgumentNullException("name");

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
