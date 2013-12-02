using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSNodeServices;
using DSRevitNodes.References;

namespace DSRevitNodes.Elements
{
    [RegisterForTrace]
    public class DSForm : AbstractElement
    {

        #region Internal Properties

        internal Autodesk.Revit.DB.Form InternalForm
        {
            get; private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        internal override Element InternalElement
        {
            get { return InternalForm; }
        }

        #endregion

        #region Private constructor

        /// <summary>
        /// Construct a Revit Form from an existing form.  
        /// </summary>
        /// <param name="form"></param>
        private DSForm(Autodesk.Revit.DB.Form form)
        {
            InternalSetForm(form);
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

        #region Public properties

        public DSFaceReference[] FaceReferences
        {
            get
            {
                return EnumerateFaces().Select(x => new DSFaceReference(x)).ToArray();
            }
        }

        public DSSolid[] Solids
        {
            get
            {
                return EnumerateSolids().Select(x => new DSSolid(x)).ToArray();
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

        #region Internal static constructors

        /// <summary>
        /// Construct the Revit element by selection.  
        /// </summary>
        /// <param name="formElement"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static DSForm FromExisting(Autodesk.Revit.DB.Form formElement, bool isRevitOwned)
        {
            return new DSForm(formElement)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

    }
}


