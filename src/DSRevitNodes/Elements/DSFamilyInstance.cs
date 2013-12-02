using System;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using DSNodeServices;
using DSRevitNodes.Elements;
using DSRevitNodes.GeometryObjects;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Point = Autodesk.DesignScript.Geometry.Point;
using Curve = Autodesk.DesignScript.Geometry.Curve;
using Face = Autodesk.DesignScript.Geometry.Face;

namespace DSRevitNodes
{
    /// <summary>
    /// A Revit FamilyInstance
    /// </summary>
    [RegisterForTrace]
    public class DSFamilyInstance : AbstractElement
    {

        #region Internal properties

        internal Autodesk.Revit.DB.FamilyInstance InternalFamilyInstance
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        internal override Element InternalElement
        {
            get { return InternalFamilyInstance; }
        }


        #endregion

        #region Private constructors

        /// <summary>
        /// Wrap an existing FamilyInstance. The resulting class is Dynamo owned
        /// </summary>
        /// <param name="instance"></param>
        private DSFamilyInstance(Autodesk.Revit.DB.FamilyInstance instance)
        {
            InternalSetFamilyInstance(instance);
        }

        /// <summary>
        /// Internal constructor for a FamilyInstance
        /// </summary>
        internal DSFamilyInstance(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.XYZ pos)
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
            TransactionManager.GetInstance().EnsureInTransaction(Document);

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

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);
        }

        #endregion

        #region Private mutators

        private void InternalSetFamilyInstance(Autodesk.Revit.DB.FamilyInstance fi)
        {
            this.InternalFamilyInstance = fi;
            this.InternalElementId = fi.Id;
            this.InternalUniqueId = fi.UniqueId;
        }

        private void InternalSetPosition(XYZ fi)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var lp = this.InternalFamilyInstance.Location as LocationPoint;
            lp.Point = fi;

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        private void InternalSetFamilySymbol(Autodesk.Revit.DB.FamilySymbol fs)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            InternalFamilyInstance.Symbol = fs;

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        #endregion

        #region Public properties

        public DSFamilySymbol Symbol
        {
            get
            {
                return DSFamilySymbol.FromExisting(this.InternalFamilyInstance.Symbol, true);
            }
        }

        public Point Location
        {
            get
            {
                var pos = this.InternalFamilyInstance.Location as LocationPoint;
                return Point.ByCoordinates( pos.Point.X, pos.Point.Y, pos.Point.Z );
            }
        }

        #endregion

        #region Static constructors

        /// <summary>
        /// Place a Revit FamilyInstance given the FamilySymbol (also known as the FamilyType) and it's coordinates in world space
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static DSFamilyInstance ByPoint(DSFamilySymbol fs, Point p)
        {
            if (fs == null)
            {
                throw new ArgumentNullException();
            } 
            
            if (p == null)
            {
                throw new ArgumentNullException();
            }

            return new DSFamilyInstance(fs.InternalFamilySymbol, new XYZ(p.X, p.Y, p.Z));
        }

        /// <summary>
        /// Place a Revit FamilyInstance given the FamilySymbol (also known as the FamilyType) and it's coordinates in world space
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static DSFamilyInstance ByCoordinates(DSFamilySymbol fs, double x, double y, double z)
        {
            if (fs == null)
            {
                throw new ArgumentNullException();
            }

            return new DSFamilyInstance(fs.InternalFamilySymbol, new XYZ(x,y,z));
        }


        #endregion

        #region Internal static constructors 

        /// <summary>
        /// Construct a FamilyInstance from the Revit document. 
        /// </summary>
        /// <param name="familyInstance"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static DSFamilyInstance FromExisting(Autodesk.Revit.DB.FamilyInstance familyInstance, bool isRevitOwned)
        {
            return new DSFamilyInstance(familyInstance)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

        static DSFamilyInstance ByCurve(DSFamilySymbol fs, DSCurve c)
        {
            throw new NotImplementedException();
        }

        static DSFamilyInstance ByUvsOnFace(DSFamilySymbol fs, Vector uv, DSFace f)
        {
            throw new NotImplementedException();
        }

        static DSFamilyInstance ByPointAndLevel(Point p, DSLevel l)
        {
            throw new NotImplementedException();
        }


    }
}
