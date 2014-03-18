using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using DSNodeServices;
using RevitServices.Persistence;
using RevitServices.Transactions;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit ImportInstance Element
    /// </summary>
    [RegisterForTrace]
    public class ImportInstance : AbstractElement
    {
        [Browsable(false)]
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalImportInstance; }
        }

        internal Autodesk.Revit.DB.ImportInstance InternalImportInstance { get; private set; }

        internal ImportInstance(string satPath)
        {
            TransactionManager.Instance.EnsureInTransaction(Document);

            var options = new SATImportOptions()
            {

            };

            var id = Document.Import(satPath, options, Document.ActiveView);
            var element = Document.GetElement(id);
            var importInstance = element as Autodesk.Revit.DB.ImportInstance;

            if (importInstance == null)
            {
                throw new Exception("Could not obtain ImportInstance from imported Element");
            }

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


        public static ImportInstance ByGeometry(Autodesk.DesignScript.Geometry.Geometry geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            // Create a temp file name to export to
            var fn = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".sat";

            if (!geometry.ExportToSAT(fn))
            {
                throw new Exception("Failed to import geometry.");
            }

            return new ImportInstance(fn);
        }

    }
}
