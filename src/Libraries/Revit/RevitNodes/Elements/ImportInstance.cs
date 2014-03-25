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

        public static ImportInstance ByGeometry(Autodesk.DesignScript.Geometry.Geometry geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            // Create a temp file name to export to
            var fn = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".sat";

            var translation = Vector.ByCoordinates(0, 0, 0);
            Robustify(ref geometry, ref translation);

            if (!geometry.ExportToSAT(fn))
            {
                throw new Exception("Failed to import geometry.");
            }

            return new ImportInstance(fn, translation.ToXyz());
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

        #endregion

    }
}
