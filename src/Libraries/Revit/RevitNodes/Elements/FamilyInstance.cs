using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.GeometryConversion;
using Revit.GeometryObjects;
using Revit.References;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Face =  Revit.GeometryObjects.Face;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit FamilyInstance
    /// </summary>
    [RegisterForTrace]
    public class FamilyInstance : AbstractFamilyInstance
    {

        #region Private constructors

        /// <summary>
        /// Wrap an existing FamilyInstance.
        /// </summary>
        /// <param name="instance"></param>
        protected FamilyInstance(Autodesk.Revit.DB.FamilyInstance instance)
        {
            InternalSetFamilyInstance(instance);
        }

        /// <summary>
        /// Internal constructor for a FamilyInstance
        /// </summary>
        internal FamilyInstance(Autodesk.Revit.DB.FamilySymbol fs, XYZ pos,
            Autodesk.Revit.DB.Level level)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldFam =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(Document);

            //There was a point, rebind to that, and adjust its position
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetLevel(level);
                InternalSetFamilySymbol(fs);
                InternalSetPosition(pos);
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.Instance.EnsureInTransaction(Document);

            Autodesk.Revit.DB.FamilyInstance fi;

            if (Document.IsFamilyDocument)
            {
                fi = Document.FamilyCreate.NewFamilyInstance(pos, fs, level,
                    Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            }
            else
            {
                fi = Document.Create.NewFamilyInstance(
                    pos, fs, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            }

            InternalSetFamilyInstance(fi);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        /// <summary>
        /// Internal constructor for a FamilyInstance
        /// </summary>
        internal FamilyInstance(Autodesk.Revit.DB.FamilySymbol fs, XYZ pos)
        {
            //Phase 1 - Check to see if the object exists and should be rebound
            var oldFam =
                ElementBinder.GetElementFromTrace<Autodesk.Revit.DB.FamilyInstance>(Document);

            //There was a point, rebind to that, and adjust its position
            if (oldFam != null)
            {
                InternalSetFamilyInstance(oldFam);
                InternalSetFamilySymbol(fs);
                InternalSetPosition(pos);
                return;
            }

            //Phase 2- There was no existing point, create one
            TransactionManager.Instance.EnsureInTransaction(Document);

            Autodesk.Revit.DB.FamilyInstance fi;

            if (Document.IsFamilyDocument)
            {
                fi = Document.FamilyCreate.NewFamilyInstance(pos, fs,
                    Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            }
            else
            {
                fi = Document.Create.NewFamilyInstance(
                    pos, fs, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
            }

            InternalSetFamilyInstance(fi);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.SetElementForTrace(InternalElement);
        }

        #endregion

        #region Private mutators

        private void InternalSetLevel(Autodesk.Revit.DB.Level level)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            // http://thebuildingcoder.typepad.com/blog/2011/01/family-instance-missing-level-property.html
            InternalFamilyInstance.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM).Set(level.Id);

            TransactionManager.Instance.TransactionTaskDone();
        }

        private void InternalSetPosition(XYZ fi)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            var lp = InternalFamilyInstance.Location as LocationPoint;
            lp.Point = fi;

            TransactionManager.Instance.TransactionTaskDone();
        }

        #endregion

        #region Public properties

        public FamilySymbol Symbol
        {
            get
            {
                return FamilySymbol.FromExisting(InternalFamilyInstance.Symbol, true);
            }
        }

        public Point Location
        {
            get
            {
                TransactionManager.Instance.EnsureInTransaction(DocumentManager.Instance.CurrentDBDocument);
                DocumentManager.Regenerate();
                var pos = InternalFamilyInstance.Location as LocationPoint;
                TransactionManager.Instance.TransactionTaskDone();
                return Point.ByCoordinates(pos.Point.X, pos.Point.Y, pos.Point.Z);
            }
        }

        public Curve[] Curves {
            get
            {
                var curves = GetCurves(new Options()
                {
                    ComputeReferences = true
                });

                return curves.Select(x => x.ToProtoType()).ToArray();
            }
        }

        public CurveReference[] CurveReferences
        {
            get
            {
                var curves = GetCurves(new Options()
                {
                    ComputeReferences = true
                });

                return curves.Select(CurveReference.FromExisting).ToArray();
            }
        }

        public Face[] Faces
        {
            get
            {
                var faces = GetFaces(new Options()
                {
                    ComputeReferences = true
                });

                return faces.Select(Face.FromExisting).ToArray();
            }
        }

        public FaceReference[] FaceReferences
        {
            get
            {
                var faces = GetFaces(new Options()
                {
                    ComputeReferences = true
                });

                return faces.Select(FaceReference.FromExisting).ToArray();
            }
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Place a Revit FamilyInstance given the FamilySymbol (also known as the FamilyType) and it's coordinates in world space
        /// </summary>
        /// <param name="familySymbol"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static FamilyInstance ByPoint(FamilySymbol familySymbol, Point point)
        {
            if (familySymbol == null)
            {
                throw new ArgumentNullException();
            } 
            
            if (point == null)
            {
                throw new ArgumentNullException();
            }

            return new FamilyInstance(familySymbol.InternalFamilySymbol, new XYZ(point.X, point.Y, point.Z));
        }

        /// <summary>
        /// Place a Revit FamilyInstance given the FamilySymbol (also known as the FamilyType) and it's coordinates in world space
        /// </summary>
        /// <param name="familySymbol"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static FamilyInstance ByCoordinates(FamilySymbol familySymbol, double x, double y, double z)
        {
            if (familySymbol == null)
            {
                throw new ArgumentNullException("familySymbol");
            }

            return new FamilyInstance(familySymbol.InternalFamilySymbol, new XYZ(x,y,z));
        }

        /// <summary>
        /// Place a Revit FamilyInstance given the FamilySymbol (also known as the FamilyType), it's coordinates in world space, and the Level
        /// </summary>
        /// <param name="familySymbol"></param>
        /// <param name="point"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static FamilyInstance ByPointAndLevel(FamilySymbol familySymbol, Point point, Level level)
        {
            if (familySymbol == null)
            {
                throw new ArgumentNullException("familySymbol");
            }

            return new FamilyInstance(familySymbol.InternalFamilySymbol, point.ToXyz(), level.InternalLevel);
        }

        /// <summary>
        /// Obtain a collection of FamilyInstances from the Revit Document and use them in the Dynamo graph
        /// </summary>
        /// <param name="familySymbol"></param>
        /// <returns></returns>
        public static FamilyInstance[] ByFamilySymbol(FamilySymbol familySymbol)
        {
            if (familySymbol == null)
            {
                throw new ArgumentNullException("familySymbol");
            }

            return DocumentManager.Instance
                .ElementsOfType<Autodesk.Revit.DB.FamilyInstance>()
                .Where(x => x.Symbol.Id == familySymbol.InternalFamilySymbol.Id)
                .Select(x => FromExisting(x, true))
                .ToArray();
        }

        #endregion

        #region Internal static constructors 

        /// <summary>
        /// Construct a FamilyInstance from the Revit document. 
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static FamilyInstance FromExisting(Autodesk.Revit.DB.FamilyInstance familyInstance, bool isRevitOwned)
        {
            return new FamilyInstance(familyInstance)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

        #region Incomplete Static constructors

        static FamilyInstance ByCurve(FamilySymbol fs, Curve c)
        {
            throw new NotImplementedException();
        }

        static FamilyInstance ByUvsOnFace(FamilySymbol fs, Vector uv, Face f)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override string ToString()
        {
            return InternalFamilyInstance.Name;
        }

    }
}
