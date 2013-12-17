using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSNodeServices;
using DSRevitNodes.GeometryConversion;
using DSRevitNodes.GeometryObjects;
using DSRevitNodes.References;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSRevitNodes.Elements
{
    /// <summary>
    /// A Revit FamilyInstance
    /// </summary>
    [RegisterForTrace]
    public class DSFamilyInstance : AbstractFamilyInstance
    {

        #region Private constructors

        /// <summary>
        /// Wrap an existing FamilyInstance.
        /// </summary>
        /// <param name="instance"></param>
        protected DSFamilyInstance(Autodesk.Revit.DB.FamilyInstance instance)
        {
            InternalSetFamilyInstance(instance);
        }

        /// <summary>
        /// Internal constructor for a FamilyInstance
        /// </summary>
        internal DSFamilyInstance(Autodesk.Revit.DB.FamilySymbol fs, Autodesk.Revit.DB.XYZ pos,
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
            TransactionManager.GetInstance().EnsureInTransaction(Document);

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

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.SetElementForTrace(this.InternalElementId);
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

        private void InternalSetLevel(Level level)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            // http://thebuildingcoder.typepad.com/blog/2011/01/family-instance-missing-level-property.html
            InternalFamilyInstance.get_Parameter(BuiltInParameter.FAMILY_LEVEL_PARAM).Set(level.Id);

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        private void InternalSetPosition(XYZ fi)
        {
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var lp = this.InternalFamilyInstance.Location as LocationPoint;
            lp.Point = fi;

            TransactionManager.GetInstance().TransactionTaskDone();
        }

        #endregion

        #region Public properties

        public Autodesk.DesignScript.Geometry.Curve[] Curves {
            get
            {
                var curves = this.GetCurvesFromFamily(InternalFamilyInstance, new Options()
                {
                    ComputeReferences = true
                });

                return curves.Select(x => x.ToProtoType()).ToArray();
            }
        }

        public DSCurveReference[] CurveReferences
        {
            get
            {
                var curves = this.GetCurvesFromFamily(InternalFamilyInstance, new Options()
                {
                    ComputeReferences = true
                });

                return curves.Select(x => new DSCurveReference(x)).ToArray();
            }
        }

        #endregion

        #region Public static constructors

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
        /// <param name="familySymbol"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static DSFamilyInstance ByCoordinates(DSFamilySymbol familySymbol, double x, double y, double z)
        {
            if (familySymbol == null)
            {
                throw new ArgumentNullException("familySymbol");
            }

            return new DSFamilyInstance(familySymbol.InternalFamilySymbol, new XYZ(x,y,z));
        }

        /// <summary>
        /// Place a Revit FamilyInstance given the FamilySymbol (also known as the FamilyType), it's coordinates in world space, and the Level
        /// </summary>
        /// <param name="familySymbol"></param>
        /// <param name="p"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static DSFamilyInstance ByPointAndLevel(DSFamilySymbol familySymbol, Point p, DSLevel level)
        {
            if (familySymbol == null)
            {
                throw new ArgumentNullException("familySymbol");
            }

            return new DSFamilyInstance(familySymbol.InternalFamilySymbol, p.ToXyz(), level.InternalLevel);
        }

        /// <summary>
        /// Obtain a collection of FamilyInstances from the Revit Document and use them in the Dynamo graph
        /// </summary>
        /// <param name="familySymbol"></param>
        /// <returns></returns>
        public static DSFamilyInstance[] ByFamilySymbol(DSFamilySymbol familySymbol)
        {
            if (familySymbol == null)
            {
                throw new ArgumentNullException("familySymbol");
            }

            return DocumentManager.GetInstance()
                .ElementsOfType<FamilyInstance>()
                .Where(x => x.Symbol.Id == familySymbol.InternalFamilySymbol.Id)
                .Select(x => DSFamilyInstance.FromExisting(x, true))
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
        internal static DSFamilyInstance FromExisting(Autodesk.Revit.DB.FamilyInstance familyInstance, bool isRevitOwned)
        {
            return new DSFamilyInstance(familyInstance)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

        #region Incomplete Static constructors

        static DSFamilyInstance ByCurve(DSFamilySymbol fs, DSCurve c)
        {
            throw new NotImplementedException();
        }

        static DSFamilyInstance ByUvsOnFace(DSFamilySymbol fs, Vector uv, DSFace f)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private helper methods

        private IEnumerable<Autodesk.Revit.DB.Curve> GetCurvesFromFamily(Autodesk.Revit.DB.FamilyInstance fi, Autodesk.Revit.DB.Options options)
        {
            var geomElem = fi.get_Geometry(options);

            var curves = new CurveArray();

            //Find all curves and insert them into curve array
            AddCurves(fi, geomElem, ref curves);

            return curves.Cast<Autodesk.Revit.DB.Curve>();

        }

        /// <summary>
        /// Retrieve the first curve found for 
        /// the given element. In case the element is a 
        /// family instance, it may have its own non-empty
        /// solid, in which case we use that. Otherwise we 
        /// search the symbol geometry. If we use the 
        /// symbol geometry, we have to keep track of the 
        /// instance transform to map it to the actual
        /// instance project location.
        /// </summary>
        private Autodesk.Revit.DB.Curve GetCurve(Element e, Options opt)
        {
            GeometryElement geo = e.get_Geometry(opt);

            Autodesk.Revit.DB.Curve curve = null;
            GeometryInstance inst = null;
            Transform t = Transform.Identity;

            // Some columns have no solids, and we have to 
            // retrieve the geometry from the symbol; 
            // others do have solids on the instance itself 
            // and no contents in the instance geometry 
            // (e.g. in rst_basic_sample_project.rvt).

            foreach (GeometryObject obj in geo)
            {
                curve = obj as Autodesk.Revit.DB.Curve;

                if (null != curve)
                {
                    break;
                }

                inst = obj as GeometryInstance;
            }

            if (null == curve && null != inst)
            {
                geo = inst.GetSymbolGeometry();
                t = inst.Transform;

                foreach (GeometryObject obj in geo)
                {
                    curve = obj as Autodesk.Revit.DB.Curve;

                    if (null != curve)
                    {
                        break;
                    }
                }
            }
            return curve;
        }

        private void AddCurves(FamilyInstance fi, GeometryElement geomElem, ref CurveArray curves)
        {
            foreach (GeometryObject geomObj in geomElem)
            {
                Autodesk.Revit.DB.Curve curve = geomObj as Autodesk.Revit.DB.Curve;
                if (null != curve)
                {
                    curves.Append(curve);
                    continue;
                }

                //If this GeometryObject is Instance, call AddCurve
                GeometryInstance geomInst = geomObj as GeometryInstance;
                if (null != geomInst)
                {
                    GeometryElement transformedGeomElem // curves transformed into project coords
                        = geomInst.GetInstanceGeometry(geomInst.Transform.Inverse);
                    AddCurves(fi, transformedGeomElem, ref curves);
                }
            }
        }

        #endregion

    }
}
