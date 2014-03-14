using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using RevitServices.Persistence;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit ImportInstance Element
    /// </summary>
    public class ImportInstance : AbstractElement
    {
        [Browsable(false)]
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { throw new NotImplementedException(); }
        }

        internal Autodesk.Revit.DB.ImportInstance InternalImportInstance { get; private set; }

        internal ImportInstance(string satPath)
        {

            var options = new SATImportOptions()
            {
                AutoCorrectAlmostVHLines = false,
                ThisViewOnly = false
            };

            var id = Document.Import(satPath, options, Document.ActiveView);
            var element = Document.GetElement(id);
            var importInstance = element as Autodesk.Revit.DB.ImportInstance;

            if (importInstance == null)
            {
                throw new Exception("Could not obtain ImportInstance from imported Element");
            }

            InternalSetImportInstance( importInstance );

        }

        private void InternalSetImportInstance(Autodesk.Revit.DB.ImportInstance ele)
        {
            this.InternalUniqueId = ele.UniqueId;
            this.InternalElementId = ele.Id;
            this.InternalImportInstance = ele;
        }

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

    }
}
