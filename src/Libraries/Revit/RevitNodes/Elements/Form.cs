using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.GeometryObjects;
using Revit.References;
using RevitServices.Persistence;
using RevitServices.Transactions;
using Face = Revit.GeometryObjects.Face;

namespace Revit.Elements
{
    [RegisterForTrace]
    public class Form : AbstractElement
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
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var f = Document.FamilyCreate.NewLoftForm(isSolid, curves);
            InternalSetForm(f);

            TransactionManager.GetInstance().TransactionTaskDone();

            ElementBinder.CleanupAndSetElementForTrace(Document, this.InternalElementId);
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

        #region Public properties

        /// <summary>
        /// Get the FaceReferences from this Element
        /// </summary>
        public FaceReference[] FaceReferences
        {
            get
            {
                return EnumerateFaces().Select(x => new FaceReference(x)).ToArray();
            }
        }

        /// <summary>
        /// Get the Faces from this Element
        /// </summary>
        public Face[] Faces
        {
            get
            {
                return EnumerateFaces().Select(Face.FromExisting).ToArray();
            }
        }

        public Solid[] Solids
        {
            get
            {
                return EnumerateSolids().Select(x => new Solid(x)).ToArray();
            }
        }

        #endregion

        // need a way to enumerate solid edges as curves
        //public DSCurveReference[] CurveReferences
        //{
        //    get
        //    {
        //        return EnumerateFaces().Select(x => new DSFaceReference(x)).ToArray();
        //    }
        //}

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

        public static Form ByLoftingCurveReferences( CurveReference[] curves, bool isSolid )
        {
            // build references
            var refArrArr = new ReferenceArrayArray();

            foreach (var l in curves)
            {
                var refArr = new ReferenceArray();
                refArr.Append(l.InternalReference);
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

