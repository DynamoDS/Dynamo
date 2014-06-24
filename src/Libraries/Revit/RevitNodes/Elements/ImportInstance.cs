using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSNodeServices;
using Revit.GeometryConversion;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit ImportInstance Element
    /// </summary>
    [RegisterForTrace]
    public class ImportInstance : Element
    {
        [Browsable(false)]
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalImportInstance; }
        }

        internal Autodesk.Revit.DB.ImportInstance InternalImportInstance { get; private set; }

        internal ImportInstance(string satPath, XYZ translation = null)
        {
            translation = translation ?? XYZ.Zero;

            TransactionManager.Instance.EnsureInTransaction(Document);

            var options = new SATImportOptions()
            {
                Unit = ImportUnit.Foot
            };

            var id = Document.Import(satPath, options, Document.ActiveView);
            var element = Document.GetElement(id);
            var importInstance = element as Autodesk.Revit.DB.ImportInstance;

            if (importInstance == null)
            {
                throw new Exception("Could not obtain ImportInstance from imported Element");
            }

            InternalSetImportInstance(importInstance);
            InternalUnpinAndTranslateImportInstance(translation);

            this.Path = satPath;

            TransactionManager.Instance.TransactionTaskDone();
        }

        private void InternalUnpinAndTranslateImportInstance(Autodesk.Revit.DB.XYZ translation)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            // the element must be unpinned to translate
            InternalImportInstance.Pinned = false;

            if (!translation.IsZeroLength()) ElementTransformUtils.MoveElement(Document, InternalImportInstance.Id, translation);

            TransactionManager.Instance.TransactionTaskDone();
        }

        private void InternalSetImportInstance(Autodesk.Revit.DB.ImportInstance ele)
        {
            this.InternalUniqueId = ele.UniqueId;
            this.InternalElementId = ele.Id;
            this.InternalImportInstance = ele;
        }

        #region Public properties

        public string Path { get; private set; }

        #endregion

        /// <summary>
        /// Import Geometry from a SAT file.  The SAT file is assumed to be in Feet.
        /// </summary>
        /// <param name="pathToFile">The path to the SAT file</param>
        /// <returns></returns>
        public static ImportInstance BySATFile(string pathToFile)
        {

            if (pathToFile == null)
            {
                throw new ArgumentNullException("pathToFile");
            }

            if (!File.Exists(pathToFile))
            {
                throw new ArgumentException("The file could not be found at: " + pathToFile );
            }

            return new ImportInstance(pathToFile);
        }

        /// <summary>
        /// Import a collection of Geometry (Solid, Curve, Surface, etc) into Revit as an ImportInstance.  This variant is much faster than
        /// ImportInstance.ByGeometry as it uses a batch method.
        /// </summary>
        /// <param name="geometries">A collection of Geometry</param>
        /// <returns></returns>
        public static ImportInstance ByGeometries(Autodesk.DesignScript.Geometry.Geometry[] geometries)
        {
            if (geometries == null)
            {
                throw new ArgumentNullException("geometries");
            }

            // transform geometry from dynamo unit system (m) to revit (ft)
            geometries = geometries.Select(x => x.InHostUnits()).ToArray();

            var translation = Vector.ByCoordinates(0, 0, 0);
            Robustify(ref geometries, ref translation);

            // Export to temporary file
            var fn = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".sat";
            var exported_fn = Autodesk.DesignScript.Geometry.Geometry.ExportToSAT(geometries, fn);

            return new ImportInstance(exported_fn, translation.ToXyz());
        }

        /// <summary>
        /// Import a collection of Geometry (Solid, Curve, Surface, etc) into Revit as an ImportInstance.
        /// </summary>
        /// <param name="geometry">A single piece of geometry</param>
        /// <returns></returns>
        public static ImportInstance ByGeometry(Autodesk.DesignScript.Geometry.Geometry geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            // transform geometry from dynamo unit system (m) to revit (ft)
            geometry = geometry.InHostUnits();

            var translation = Vector.ByCoordinates(0, 0, 0);
            Robustify(ref geometry, ref translation);

            // Export to temporary file
            var fn = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".sat";
            var exported_fn = geometry.ExportToSAT(fn);

            return new ImportInstance(exported_fn, translation.ToXyz());
        }


        #region Helper methods
        
        /// <summary>
        /// This method contains workarounds for increasing the robustness of input geometry
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="translation"></param>
        private static void Robustify(ref Autodesk.DesignScript.Geometry.Geometry geometry,
            ref Autodesk.DesignScript.Geometry.Vector translation)
        {
            // translate centroid of the solid to the origin
            // export, then move back 
            if (geometry is Autodesk.DesignScript.Geometry.Solid)
            {
                var solid = geometry as Autodesk.DesignScript.Geometry.Solid;

                translation = solid.Centroid().AsVector();
                var tranGeo = solid.Translate(translation.Reverse());

                geometry = tranGeo;
            }
        }

        /// <summary>
        /// This method contains workarounds for increasing the robustness of input geometry
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="translation"></param>
        private static void Robustify(ref Autodesk.DesignScript.Geometry.Geometry[] geometry,
            ref Autodesk.DesignScript.Geometry.Vector translation)
        {
            // translate all geom to centroid of bbox, then translate back
            var bb = Autodesk.DesignScript.Geometry.BoundingBox.ByGeometry(geometry);

            // get center of bbox
            var trans = ((bb.MinPoint.ToXyz() + bb.MaxPoint.ToXyz())/2).ToVector().Reverse();

            // translate all geom so that it is centered by bb
            geometry = geometry.Select(x => x.Translate(trans)).ToArray();

            // so that we can move it all back
            translation = trans.Reverse();
        }

        #endregion

    }
}
