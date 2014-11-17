using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;

using DSNodeServices;

using Revit.GeometryConversion;

using RevitServices.Persistence;
using RevitServices.Transactions;

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
            TransactionManager.Instance.EnsureInTransaction(Document);

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

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElement);
        }

        /// <summary>
        /// Internal constructor - creates a single StructuralFraming instance
        /// </summary>
        internal StructuralFraming(Autodesk.Revit.DB.Curve curve, Autodesk.Revit.DB.Level level, Autodesk.Revit.DB.Structure.StructuralType structuralType, Autodesk.Revit.DB.FamilySymbol symbol)
        {

            //Phase 1 - Check to see if the object exists and should be rebound
            var oldFam =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(Document);

            //There's an existing structural framing element, rebind to that, and adjust its position
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetFamilySymbol(symbol);
                InternalSetCurve(curve);
                return;
            }

            //Phase 2- There was no existing structural framing element, create one
            TransactionManager.Instance.EnsureInTransaction(Document);

            var creationData = GetCreationData(curve, level, structuralType, symbol);

            Autodesk.Revit.DB.FamilyInstance fi;

            if (Document.IsFamilyDocument)
            {
                var elementIds =
                    Document.FamilyCreate.NewFamilyInstances2(
                        new List<FamilyInstanceCreationData>() { creationData });

                if (elementIds.Count == 0)
                {
                    throw new Exception("Could not create the FamilyInstance");
                }

                fi = (Autodesk.Revit.DB.FamilyInstance)Document.GetElement(elementIds.First());
            }
            else
            {
                var elementIds =
                    Document.Create.NewFamilyInstances2(
                        new List<FamilyInstanceCreationData>() { creationData });

                if (elementIds.Count == 0)
                {
                    throw new Exception("Could not create the FamilyInstance");
                }

                fi = (Autodesk.Revit.DB.FamilyInstance)Document.GetElement(elementIds.First());
            }


            InternalSetFamilyInstance(fi);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElement);
        }
        
        #endregion

        #region Private helper methods

        private static FamilyInstanceCreationData GetCreationData(Autodesk.Revit.DB.Curve curve, Autodesk.Revit.DB.XYZ upVector, Autodesk.Revit.DB.Level level, Autodesk.Revit.DB.Structure.StructuralType structuralType, Autodesk.Revit.DB.FamilySymbol symbol)
        {

            //calculate the desired rotation
            //we do this by finding the angle between the z axis
            //and vector between the start of the beam and the target point
            //both projected onto the start plane of the beam.
            var zAxis = new XYZ(0, 0, 1);
            var yAxis = new XYZ(0, 1, 0);

            //flatten the beam line onto the XZ plane
            //using the start's z coordinate
            var start = curve.GetEndPoint(0);
            var end = curve.GetEndPoint(1);
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

        private static FamilyInstanceCreationData GetCreationData(Autodesk.Revit.DB.Curve curve, Autodesk.Revit.DB.Level level, Autodesk.Revit.DB.Structure.StructuralType structuralType, Autodesk.Revit.DB.FamilySymbol symbol)
        {
            return new FamilyInstanceCreationData(curve, symbol, level, structuralType);
        }

        #endregion

        #region Private mutators

        private void InternalSetCurve(Autodesk.Revit.DB.Curve crv)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            //update the curve
            var locCurve = InternalFamilyInstance.Location as LocationCurve;
            locCurve.Curve = crv;

            TransactionManager.Instance.TransactionTaskDone();
        }

        #endregion

        #region Public properties

        new public Autodesk.DesignScript.Geometry.Curve Location
        {
            get
            {
                var location = this.InternalFamilyInstance.Location;
                var crv = location as LocationCurve;
                if (null != crv && null != crv.Curve)
                    return crv.Curve.ToProtoType();
                throw new Exception("The location of the structural element is not a valid curve!");
            }
        }

        public new FamilySymbol Symbol
        {    
            // NOTE: Because AbstractFamilyInstance is not visible in the library
            //       we redefine this method on FamilyInstance
            get { return base.Symbol; }
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
        /// <param name="structuralFramingType">The structural framing type representing the structural type</param>
        /// <returns></returns>
        public static StructuralFraming ByCurveLevelUpVectorAndType(Autodesk.DesignScript.Geometry.Curve curve, Level level, 
            Autodesk.DesignScript.Geometry.Vector upVector, StructuralType structuralType, FamilySymbol structuralFramingType)
        {
            if (curve == null)
            {
                throw new System.ArgumentNullException("curve");
            }

            if (level == null)
            {
                throw new System.ArgumentNullException("level");
            }

            if (upVector == null)
            {
                throw new System.ArgumentNullException("upVector");
            }

            if (structuralFramingType == null)
            {
                throw new System.ArgumentNullException("structuralFramingType");
            }            

            return new StructuralFraming(curve.ToRevitType(), upVector.ToXyz(), level.InternalLevel,
                structuralType.ToRevitType(), structuralFramingType.InternalFamilySymbol);
        }

        /// <summary>
        /// Create a beam.
        /// </summary>
        /// <param name="curve">The curve which defines the center line of the beam.</param>
        /// <param name="level">The level with which you'd like the beam to be associated.</param>
        /// <param name="structuralFramingType">The structural framing type representing the beam.</param>
        /// <returns></returns>
        public static StructuralFraming BeamByCurve(Autodesk.DesignScript.Geometry.Curve curve, Revit.Elements.Level level, Revit.Elements.FamilySymbol structuralFramingType)
        {
            if (curve == null)
            {
                throw new System.ArgumentNullException("curve");
            }

            if (level == null)
            {
                throw new System.ArgumentNullException("level");
            }

            if (structuralFramingType == null)
            {
                throw new System.ArgumentNullException("structuralFramingType");
            }

            return new StructuralFraming(curve.ToRevitType(), level.InternalLevel, Autodesk.Revit.DB.Structure.StructuralType.Beam, structuralFramingType.InternalFamilySymbol);
        }

        /// <summary>
        /// Create a brace.
        /// </summary>
        /// <param name="curve">The cruve which defines the center line of the brace.</param>
        /// <param name="level">The level with which you'd like the brace to be associated.</param>
        /// <param name="structuralFramingType">The structural framing type representing the brace.</param>
        /// <returns></returns>
        public static StructuralFraming BraceByCurve(Autodesk.DesignScript.Geometry.Curve curve, Revit.Elements.Level level, Revit.Elements.FamilySymbol structuralFramingType)
        {
            if (curve == null)
            {
                throw new System.ArgumentNullException("curve");
            }

            if (level == null)
            {
                throw new System.ArgumentNullException("level");
            }

            if (structuralFramingType == null)
            {
                throw new System.ArgumentNullException("structuralFramingType");
            }

            return new StructuralFraming(curve.ToRevitType(), level.InternalLevel, Autodesk.Revit.DB.Structure.StructuralType.Brace, structuralFramingType.InternalFamilySymbol);
        }

        /// <summary>
        /// Create a column.
        /// </summary>
        /// <param name="curve">The curve which defines the center line of the column.</param>
        /// <param name="level">The level with which you'd like the column to be associated.</param>
        /// <param name="structuralColumnType">The structural column type representing the column.</param>
        /// <returns></returns>
        public static StructuralFraming ColumnByCurve(
            Autodesk.DesignScript.Geometry.Curve curve, Revit.Elements.Level level, Revit.Elements.FamilySymbol structuralColumnType)
        {
            if (curve == null)
            {
                throw new System.ArgumentNullException("curve");
            }

            if (level == null)
            {
                throw new System.ArgumentNullException("level");
            }

            if (structuralColumnType == null)
            {
                throw new System.ArgumentNullException("structuralColumnType");
            }

            var start = curve.PointAtParameter(0);
            var end = curve.PointAtParameter(1);

            // Revit will throw an exception if you attempt to create a column whose 
            // base is above its top. 
            if (end.Z <= start.Z)
            {
                throw new Exception("The end of the curve for creating a column should be above the start of the curve.");
            }

            return new StructuralFraming(curve.ToRevitType(), level.InternalLevel, Autodesk.Revit.DB.Structure.StructuralType.Column, structuralColumnType.InternalFamilySymbol);
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
