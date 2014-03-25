using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
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


            importInstance.Pinned = false;
            ElementTransformUtils.MoveElement(Document, importInstance.Id, translation);
            InternalSetImportInstance( importInstance );

            this.Path = satPath;

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

        public static ImportInstance BySolid(Autodesk.DesignScript.Geometry.Solid geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            // Create a temp file name to export to
            var fn = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".sat";

            var tran = geometry.Centroid().AsVector();
            var tranGeo = geometry.Translate(tran.Reverse());

            if (!tranGeo.ExportToSAT(fn))
            {
                throw new Exception("Failed to import geometry.");
            }

            return new ImportInstance(fn, tran.ToXyz());
        }


        //public static ImportInstance ByGeometry(Autodesk.DesignScript.Geometry.Geometry geometry)
        //{
        //    if (geometry == null)
        //    {
        //        throw new ArgumentNullException("geometry");
        //    }

        //    // Create a temp file name to export to
        //    var fn = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".sat";

        //    if (!geometry.ExportToSAT(fn))
        //    {
        //        throw new Exception("Failed to import geometry.");
        //    }

        //    return new ImportInstance(fn);
        //}

    }
}
