using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    public class PerspectiveView : AbstractView3D
    {

        #region Private constructors

        /// <summary>
        /// Private constructor
        /// </summary>
        private PerspectiveView(View3D view)
        {
            InternalSetView3D(view);
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private PerspectiveView(XYZ eye, XYZ target, BoundingBoxXYZ bbox, string name, bool isolate)
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

            var vd = Create3DView(BuildOrientation3D(eye, target), name, true);
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

            ElementBinder.SetElementForTrace(this.InternalElementId);
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private PerspectiveView(XYZ eye, XYZ target, Autodesk.Revit.DB.Element element, string name, bool isolate)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldEle =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.View3D>(Document);

            // Rebind to Element
            if (oldEle != null)
            {
                InternalSetView3D(oldEle);
                InternalSetOrientation( BuildOrientation3D(eye, target) );
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

            var vd = Create3DView(BuildOrientation3D(eye, target), name, true);
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

            ElementBinder.SetElementForTrace(this.InternalElementId);
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Perspective View from an Eye position, a target position, and 
        /// either an AbstractElement or BoundingBox.
        /// </summary>
        /// <param name="eyePoint">A Point representing the eye point.</param>
        /// <param name="target">A Point representing the target of view.</param>
        /// <param name="element">This argument cannot be null, and it has to be either a 
        /// Revit.Elements.AbstractElement or Revit.GeometryObjects.BoundingBox.</param>
        /// <param name="name">The name of the view.</param>
        /// <param name="isolateElement">If this argument is set to true, the element or 
        /// bounding box will be isolated in the current view by creating a minimum size
        /// crop box around it.</param>
        /// <returns>Returns the resulting PerspectiveView object.</returns>
        /// 
        public static PerspectiveView ByEyePointAndTarget(
            Autodesk.DesignScript.Geometry.Point eyePoint,
            Autodesk.DesignScript.Geometry.Point target,
            object element, string name, bool isolateElement)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            AbstractElement abstractElement = element as AbstractElement;
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
                "'Revit.Elements.AbstractElement' or 'Revit.GeometryObjects.BoundingBox', " +
                "but found to be of type '{0}'", element.GetType());

            throw new ArgumentException(message, "element");
        }

        /// <summary>
        /// Create a Revit Perspective View from an Eye position and target position and Element
        /// </summary>
        /// <param name="eyePoint"></param>
        /// <param name="target"></param>
        /// <param name="element"></param>
        /// <param name="name"></param>
        /// <param name="isolateElement"></param>
        /// <returns></returns>
        public static PerspectiveView ByEyePointTargetAndElement(Autodesk.DesignScript.Geometry.Point eyePoint, Autodesk.DesignScript.Geometry.Point target, AbstractElement element, string name, bool isolateElement)
        {
            if (eyePoint == null)
            {
                throw new ArgumentNullException("eyePoint");
            }

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            return new PerspectiveView(eyePoint.ToXyz(), target.ToXyz(), element.InternalElement, name, isolateElement);
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
        public static PerspectiveView ByEyePointTargetAndBoundingBox(Autodesk.DesignScript.Geometry.Point eyePoint, Autodesk.DesignScript.Geometry.Point target, BoundingBox boundingBox, string name, bool isolateElement)
        {
            if (boundingBox == null)
            {
                throw new ArgumentNullException("boundingBox");
            }

            if (eyePoint == null)
            {
                throw new ArgumentNullException("eyePoint");
            }

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            return new PerspectiveView(eyePoint.ToXyz(), target.ToXyz(), boundingBox.InternalBoundingBoxXyz, name, isolateElement);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create from an existing Revit Element
        /// </summary>
        /// <param name="view"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static PerspectiveView FromExisting( View3D view, bool isRevitOwned )
        {
            return new PerspectiveView(view)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }
}


