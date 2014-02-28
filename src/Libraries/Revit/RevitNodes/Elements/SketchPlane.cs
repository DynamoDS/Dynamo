﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.GeometryConversion;
using Revit.References;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit SketchPlane
    /// </summary>
    [RegisterForTrace]
    public class SketchPlane : AbstractElement
    {
        #region Internal properties

        /// <summary>
        /// Internal reference to the Revit Element
        /// </summary>
        internal Autodesk.Revit.DB.SketchPlane InternalSketchPlane
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalSketchPlane; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Set the Sketch plane from an existing one
        /// </summary>
        /// <param name="existingSketchPlane"></param>
        private SketchPlane(Autodesk.Revit.DB.SketchPlane existingSketchPlane)
        {
            InternalSetSketchPlane(existingSketchPlane);
        }

        /// <summary>
        /// Make a SketchPlane from a plane
        /// </summary>
        /// <param name="p"></param>
        private SketchPlane(Autodesk.Revit.DB.Plane p)
        {

            //Phase 1 - Check to see if the object exists and should be rebound
            var oldEle =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.SketchPlane>(Document);

            //There was an element, bind & mutate
            if (oldEle != null)
            {
                InternalSetSketchPlane(oldEle);
                if (InternalSetPlane(p))
                {
                    return;
                }

                // if setting the plane fails, we delete the old Element
                // in order to create a new one
                DocumentManager.GetInstance().DeleteElement(oldEle.Id);

            }

            //Phase 2- There was no existing element, create new
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            Autodesk.Revit.DB.SketchPlane sp;

            if (Document.IsFamilyDocument)
            {
                sp = Document.FamilyCreate.NewSketchPlane(p);
            }
            else
            {
                sp = Document.Create.NewSketchPlane(p);
            }

            InternalSetSketchPlane(sp);

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);

        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Attempt to set the plane of an existing SketchPlane
        /// </summary>
        /// <param name="p"></param>
        /// <returns>False if the new sketch plane is not parallel to the existing one</returns>
        private bool InternalSetPlane(Autodesk.Revit.DB.Plane p)
        {
            var sp = this.InternalSketchPlane;

            XYZ newOrigin = p.Origin;
            XYZ newNorm = p.Normal;
            var oldP = sp.Plane;
            XYZ oldOrigin = oldP.Origin;
            XYZ oldNorm = oldP.Normal;

            if (oldNorm.IsAlmostEqualTo(newNorm))
            {
                XYZ moveVec = newOrigin - oldOrigin;
                if (moveVec.GetLength() > 0.000000001)
                    ElementTransformUtils.MoveElement(Document, sp.Id, moveVec);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Set the Element, ElementId, and UniqueId
        /// </summary>
        /// <param name="existingSketchPlane"></param>
        private void InternalSetSketchPlane(Autodesk.Revit.DB.SketchPlane existingSketchPlane)
        {
            this.InternalSketchPlane = existingSketchPlane;
            this.InternalElementId = existingSketchPlane.Id;
            this.InternalUniqueId = existingSketchPlane.UniqueId;
        }

        #endregion

        #region Public properties

        public PlaneReference PlaneReference
        {
            get
            {
                return new PlaneReference(this.InternalSketchPlane.GetPlaneReference());
            }
        }

        public Autodesk.DesignScript.Geometry.Plane Plane
        {
            get
            {
                return this.InternalSketchPlane.GetPlane().ToPlane();
            }
        }

        #endregion

        #region Public static constructor

        /// <summary>
        /// Make a Revit SketchPlane given a plane
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        public static SketchPlane ByPlane(Autodesk.DesignScript.Geometry.Plane plane)
        {
            if (plane == null)
            {
                throw new ArgumentNullException("plane");
            }
            
            return new SketchPlane(plane.ToPlane());
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Create from an existing element
        /// </summary>
        /// <param name="existingSketchPlane"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static SketchPlane FromExisting(Autodesk.Revit.DB.SketchPlane existingSketchPlane, bool isRevitOwned)
        {
            return new SketchPlane(existingSketchPlane)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion
    }
}
