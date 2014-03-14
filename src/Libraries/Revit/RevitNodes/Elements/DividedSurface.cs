using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.References;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Face = Autodesk.DesignScript.Geometry.Face;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit DividedSurface
    /// </summary>
    [RegisterForTrace]
    public class DividedSurface : AbstractElement
    {
        #region Private Properties

        /// <summary>
        /// Internal variable containing the wrapped Revit object
        /// </summary>
        internal Autodesk.Revit.DB.DividedSurface InternalDividedSurface
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalDividedSurface; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Construct from an existing Element.  The resulting object is Dynamo owned.
        /// </summary>
        /// <param name="divSurf"></param>
        private DividedSurface(Autodesk.Revit.DB.DividedSurface divSurf)
        {
            InternalSetDividedSurface(divSurf);
        }

        /// <summary>
        /// Private constructor for creating a divided surface
        /// </summary>
        /// <param name="face"></param>
        /// <param name="uDivs"></param>
        /// <param name="vDivs"></param>
        /// <param name="rotation"></param>
        private DividedSurface(FaceReference face, int uDivs, int vDivs, double rotation)
        {
            // if the family instance is present in trace...
            var oldEle =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.DividedSurface>(Document);

            // just mutate it...
            if (oldEle != null)
            {
                InternalSetDividedSurface(oldEle);
                InternalSetDivisions(uDivs, vDivs);
                InternalSetRotation(rotation);
                return;
            }

            // otherwise create a new family instance...
            TransactionManager.Instance.EnsureInTransaction(Document);

            var divSurf = Document.FamilyCreate.NewDividedSurface(face.InternalReference);

            InternalSetDividedSurface(divSurf);
            InternalSetDivisions(uDivs, vDivs);
            InternalSetRotation(rotation);

            TransactionManager.Instance.TransactionTaskDone();

            // remember this new value
            ElementBinder.SetElementForTrace(this.InternalElement);
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Number of divisions in U direction
        /// </summary>
        public int UDivisions
        {
            get
            {
                return InternalDividedSurface.USpacingRule.Number;
            }
        }

        /// <summary>
        /// Number of divisions in V direction
        /// </summary>
        public int VDivisions
        {
            get
            {
                return InternalDividedSurface.VSpacingRule.Number;
            }
        }

        /// <summary>
        /// Rotation of the grid lines with respect to the UV parameterization
        /// of the face
        /// </summary>
        public double Rotation
        {
            get
            {
                return InternalDividedSurface.AllGridRotation;
            }
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Method to set the internal divided surface, id, and unique id
        /// </summary>
        /// <param name="divSurf"></param>
        private void InternalSetDividedSurface(Autodesk.Revit.DB.DividedSurface divSurf)
        {
            this.InternalDividedSurface = divSurf;
            this.InternalElementId = divSurf.Id;
            this.InternalUniqueId = divSurf.UniqueId;
        }

        /// <summary>
        /// Method to mutate the number of divisions of the internal divided surface.  Will
        /// fail if the divided surface is not set
        /// </summary>
        /// <param name="uDivs"></param>
        /// <param name="vDivs"></param>
        private void InternalSetDivisions(int uDivs, int vDivs)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            if (InternalDividedSurface.USpacingRule.Number != uDivs)
                InternalDividedSurface.USpacingRule.Number = uDivs;
            if (InternalDividedSurface.VSpacingRule.Number != vDivs)
                InternalDividedSurface.VSpacingRule.Number = vDivs;

            TransactionManager.Instance.TransactionTaskDone();
        }

        /// <summary>
        /// Method to set the grid rotation of the internal divided surface
        /// </summary>
        /// <param name="uDivs"></param>
        /// <param name="vDivs"></param>
        private void InternalSetRotation(double rotation)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            InternalDividedSurface.AllGridRotation = rotation;

            TransactionManager.Instance.TransactionTaskDone();
        }

        #endregion

        #region Static constructors

        /// <summary>
        /// Create a Revit DividedSurface on a face given the face and number of divisions in u and v directon
        /// </summary>
        /// <param name="face"></param>
        /// <param name="uDivs"></param>
        /// <param name="vDivs"></param>
        /// <returns></returns>
        public static DividedSurface ByFaceAndUVDivisions(FaceReference face, int uDivs, int vDivs)
        {
            return ByFaceUVDivisionsAndRotation(face, uDivs, vDivs, 0.0);
        }

        /// <summary>
        /// Create a Revit DividedSurface on a face given the face and number of divisions in u and v directon
        /// and the rotation of the grid lines with respect to the natural UV parameterization of the face
        /// </summary>
        /// <param name="face"></param>
        /// <param name="uDivs"></param>
        /// <param name="vDivs"></param>
        /// <param name="gridRotation"></param>
        /// <returns></returns>
        public static DividedSurface ByFaceUVDivisionsAndRotation(FaceReference face, int uDivs, int vDivs, double gridRotation)
        {

            if (face == null)
            {
                throw new ArgumentNullException("face");
            }

            if (uDivs <= 0)
            {
                throw new Exception("uDivs must be a positive integer");
            }

            if (vDivs <= 0)
            {
                throw new Exception("vDivs must be a positive integer");
            }

            return new DividedSurface(face, uDivs, vDivs, gridRotation);
        }

        #endregion

        #region Internal static constructor

        /// <summary>
        /// Construct this type from an existing Revit element.
        /// </summary>
        /// <param name="dividedSurface"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static DividedSurface FromExisting(Autodesk.Revit.DB.DividedSurface dividedSurface, bool isRevitOwned)
        {
            if (dividedSurface == null)
            {
                throw new ArgumentNullException("dividedSurface");
            }

            return new DividedSurface(dividedSurface)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}

