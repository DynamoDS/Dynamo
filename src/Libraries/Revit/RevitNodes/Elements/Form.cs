using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.GeometryObjects;
using Revit.GeometryReferences;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements
{
    [RegisterForTrace]
    public class Form : Element
    {

        #region Internal Properties

        internal Autodesk.Revit.DB.Form InternalForm
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalForm; }
        }

        #endregion

        #region Private constructor

        /// <summary>
        /// Construct a Revit Form from an existing form.  
        /// </summary>
        /// <param name="form"></param>
        private Form(Autodesk.Revit.DB.Form form)
        {
            InternalSetForm(form);
        }

        /// <summary>
        /// Create a Form by lofting
        /// </summary>
        /// <param name="isSolid"></param>
        /// <param name="curves"></param>
        private Form(bool isSolid, ReferenceArrayArray curves)
        {
            // clean it up
            TransactionManager.Instance.EnsureInTransaction(Document);

            var f = Document.FamilyCreate.NewLoftForm(isSolid, curves);
            InternalSetForm(f);

            TransactionManager.Instance.TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElement);
        }

        #endregion

        #region Private mutator

        private void InternalSetForm(Autodesk.Revit.DB.Form form)
        {
            this.InternalForm = form;
            this.InternalElementId = form.Id;
            this.InternalUniqueId = form.UniqueId;
        }

        #endregion

        #region Private helper methods 



        #endregion

        #region Internal helper methods

        private IEnumerable<Autodesk.Revit.DB.Face> EnumerateFaces()
        {
            return EnumerateSolids(GetGeometryElement()).SelectMany(x => x.Faces.Cast<Autodesk.Revit.DB.Face>());
        }

        private IEnumerable<Autodesk.Revit.DB.Solid> EnumerateSolids()
        {
            return EnumerateSolids(GetGeometryElement());
        }

        private IEnumerable<Autodesk.Revit.DB.Edge> EnumerateEdges()
        {
            return EnumerateSolids(GetGeometryElement()).SelectMany(x => x.Edges.Cast<Autodesk.Revit.DB.Edge>());
        }

        private IEnumerable<Autodesk.Revit.DB.Solid> EnumerateSolids(GeometryElement geomElement)
        {
            // get solid first level elements that are solids
            var solidFaces = geomElement.Where(x => x is Autodesk.Revit.DB.Solid)
                .OfType<Autodesk.Revit.DB.Solid>();

            // get solids from geometry instances
            var geomInstFaces = geomElement.OfType<Autodesk.Revit.DB.GeometryInstance>()
                                            .SelectMany(x => x.GetInstanceGeometry())
                                            .OfType<Autodesk.Revit.DB.Solid>();

            return solidFaces.Concat(geomInstFaces);
        }

        private Autodesk.Revit.DB.GeometryElement GetGeometryElement()
        {
            var geoOptions = new Autodesk.Revit.DB.Options()
            {
                ComputeReferences = true
            };
            var geomObj = InternalForm.get_Geometry(geoOptions);

            if (geomObj == null)
            {
                throw new Exception("Could not obtain geometry from element");
            }

            return geomObj;
        }

        private static IEnumerable<Autodesk.Revit.DB.Face> GetFaces(Autodesk.Revit.DB.GeometryInstance geomInst)
        {
            return geomInst.GetInstanceGeometry()
                .OfType<Autodesk.Revit.DB.Solid>()
                .SelectMany(x => x.Faces.Cast<Autodesk.Revit.DB.Face>());
        }

        private static IEnumerable<Autodesk.Revit.DB.Face> GetFaces(Autodesk.Revit.DB.Solid solid)
        {
            return solid.Faces.Cast<Autodesk.Revit.DB.Face>();
        }

        #endregion

        #region Public static constructors 

        public static Form ByLoftCrossSections(object[] curves, bool isSolid = true)
        {
            if (curves == null) throw new ArgumentNullException("curves");

            // if the arguments are polycurves, explode them
            if (curves.Any(x => x is PolyCurve))
            {
                var ca = curves.Cast<Autodesk.DesignScript.Geometry.Curve>()
                    .Select(x => x is PolyCurve ? ((PolyCurve) x).Curves() : new []{ x } ).ToArray();

                return ByLoftCrossSections(ca, isSolid);
            }

            var refArrArr = new ReferenceArrayArray();

            foreach (var l in curves)
            {
                if (l == null) throw new ArgumentNullException("curves");
                var refArr = new ReferenceArray();

                
                refArr.Append(ElementCurveReference.TryGetCurveReference(l, "Form").InternalReference);
                refArrArr.Append(refArr);
            }

            return new Form(isSolid, refArrArr);
        }

        public static Form ByLoftCrossSections(object[][] curves, bool isSolid = true)
        {
            if (curves.SelectMany(x=>x).Any(x=>x==null))
            {
                throw new ArgumentException("Some of the input curves are null.");    
            }

            if (curves == null) throw new ArgumentNullException("curves");

            var refArrArr = new ReferenceArrayArray();

            foreach (var curveArr in curves)
            {
                var refArr = new ReferenceArray();
                curveArr.ForEach(x => refArr.Append(ElementCurveReference.TryGetCurveReference(x, "Form").InternalReference));
                refArrArr.Append(refArr);
            }

            return new Form(isSolid, refArrArr);

        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Construct the Revit element by selection.  
        /// </summary>
        /// <param name="formElement"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static Form FromExisting(Autodesk.Revit.DB.Form formElement, bool isRevitOwned)
        {
            return new Form(formElement)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}

