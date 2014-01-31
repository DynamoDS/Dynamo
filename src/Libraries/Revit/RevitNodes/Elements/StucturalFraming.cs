using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using DSNodeServices;
using Revit.Elements;
using Revit.GeometryConversion;
using Revit.GeometryObjects;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Curve = Autodesk.Revit.DB.Curve;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit FamilyInstance
    /// </summary>
    [RegisterForTrace]
    public class StructuralFraming : AbstractFamilyInstance
    {

        #region Private constructors

        /// <summary>
        /// Wrap an existing FamilyInstance. 
        /// </summary>
        /// <param name="instance"></param>
        private StructuralFraming(Autodesk.Revit.DB.FamilyInstance instance)
        {
            InternalSetFamilyInstance(instance);
        }

        /// <summary>
        /// Internal constructor - creates a single StructuralFraming instance
        /// </summary>
        internal StructuralFraming(Autodesk.Revit.DB.Curve curve, Autodesk.Revit.DB.XYZ upVector, Autodesk.Revit.DB.Level level, Autodesk.Revit.DB.Structure.StructuralType structuralType, Autodesk.Revit.DB.FamilySymbol symbol)
        {

            //Phase 1 - Check to see if the object exists and should be rebound
            var oldFam =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(Document);

            //There was a point, rebind to that, and adjust its position
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetFamilySymbol(symbol);
                InternalSetCurve(curve);
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var creationData = GetCreationData(curve, upVector, level, structuralType, symbol);
            
            Autodesk.Revit.DB.FamilyInstance fi;
            if (Document.IsFamilyDocument)
            {
                var elementIds = Document.FamilyCreate.NewFamilyInstances2(new List<FamilyInstanceCreationData>() { creationData });

                if (elementIds.Count == 0)
                {
                    throw new Exception("Could not create the FamilyInstance");
                }

                fi = (Autodesk.Revit.DB.FamilyInstance)Document.GetElement(elementIds.First());
            }
            else
            {
                var elementIds = Document.Create.NewFamilyInstances2(new List<FamilyInstanceCreationData>() { creationData });

                if (elementIds.Count == 0)
                {
                    throw new Exception("Could not create the FamilyInstance");
                }

                fi = (Autodesk.Revit.DB.FamilyInstance) Document.GetElement(elementIds.First());
            }

            InternalSetFamilyInstance(fi);

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);
        }

        #endregion

        #region Private helper methods

        private FamilyInstanceCreationData GetCreationData(Autodesk.Revit.DB.Curve curve, Autodesk.Revit.DB.XYZ upVector, Autodesk.Revit.DB.Level level, Autodesk.Revit.DB.Structure.StructuralType structuralType, Autodesk.Revit.DB.FamilySymbol symbol)
        {

            //calculate the desired rotation
            //we do this by finding the angle between the z axis
            //and vector between the start of the beam and the target point
            //both projected onto the start plane of the beam.
            var zAxis = new XYZ(0, 0, 1);
            var yAxis = new XYZ(0, 1, 0);

            //flatten the beam line onto the XZ plane
            //using the start's z coordinate
            var start = curve.get_EndPoint(0);
            var end = curve.get_EndPoint(1);
            var newEnd = new XYZ(end.X, end.Y, start.Z); //drop end point to plane

            //catch the case where the end is directly above
            //the start, creating a normal with zero length
            //in that case, use the Z axis
            XYZ planeNormal = newEnd.IsAlmostEqualTo(start) ? zAxis : (newEnd - start).Normalize();

            double gamma = upVector.AngleOnPlaneTo(zAxis.IsAlmostEqualTo(planeNormal) ? yAxis : zAxis, planeNormal);

            return new FamilyInstanceCreationData(curve, symbol, level, structuralType)
            {
                RotateAngle = gamma
            };

        }

        #endregion

        #region Private mutators

        private void InternalSetCurve(Autodesk.Revit.DB.Curve crv)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            //update the curve
            var locCurve = InternalFamilyInstance.Location as LocationCurve;
            locCurve.Curve = crv;

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        #endregion

        #region Public properties

        public FamilySymbol Symbol
        {
            get
            {
                return FamilySymbol.FromExisting(this.InternalFamilyInstance.Symbol, true);
            }
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Create a Revit Structural Member - a special FamilyInstance
        /// </summary>
        /// <param name="curve">The curve path for the structural member</param>
        /// <param name="upVector">The up vector for the element - this is required to determine the orientation of the element</param>
        /// <param name="level">The level on which the member should appear</param>
        /// <param name="structuralType">The type of the structural element - a beam, column, etc</param>
        /// <param name="structuralFamilySymbol">The FamilySymbol representing the structural type</param>
        /// <returns></returns>
        public static StructuralFraming ByCurveLevelUpVectorAndType(Autodesk.DesignScript.Geometry.Curve curve, Level level, Autodesk.DesignScript.Geometry.Vector upVector, StructuralType structuralType, FamilySymbol structuralFamilySymbol)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }

            if (level == null)
            {
                throw new ArgumentNullException("level");
            }

            if (upVector == null)
            {
                throw new ArgumentNullException("upVector");
            }

            if (structuralFamilySymbol == null)
            {
                throw new ArgumentNullException("structuralFamilySymbol");
            }            

            return new StructuralFraming(curve.ToRevitType(), upVector.ToXyz(), level.InternalLevel,
                structuralType.ToRevitType(), structuralFamilySymbol.InternalFamilySymbol);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Construct a FamilyInstance from the Revit document. 
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static StructuralFraming FromExisting(Autodesk.Revit.DB.FamilyInstance familyInstance, bool isRevitOwned)
        {
            return new StructuralFraming(familyInstance)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}
