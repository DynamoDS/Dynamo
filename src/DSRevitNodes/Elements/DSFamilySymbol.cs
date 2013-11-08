using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using DSRevitNodes.Elements;
using RevitServices.Transactions;

namespace DSRevitNodes
{
    /// <summary>
    /// A Revit FamilySymbol
    /// </summary>
    [RegisterForTrace]
    class DSFamilySymbol: AbstractElement
    {

        #region internal Properties

        protected Autodesk.Revit.DB.FamilySymbol InternalFamilySymbol
        {
            get;
            private set;
        }

        #endregion

        #region Private constructors

        internal DSFamilySymbol(Autodesk.Revit.DB.FamilySymbol symbol)
        {
            InternalSetFamilySymbol(symbol);
        }

        #endregion

        #region Private mutators

        private void InternalSetFamilySymbol(Autodesk.Revit.DB.FamilySymbol symbol)
        {
            this.InternalFamilySymbol = symbol;
            this.InternalId = symbol.Id;
            this.InternalUniqueId = symbol.UniqueId;
        }

        #endregion

        #region Public properties

        public string Name
        {
            get
            {
                return InternalFamilySymbol.Name;
            }
        }

        public DSFamily Family
        {
            get
            {
                return new DSFamily(this.InternalFamilySymbol.Family);
            }
        }

        public DSParameter[] Parameters
        {
            get
            {
                var parms = this.InternalFamilySymbol.Parameters;
                var parmsOut = new DSParameter[parms.Size];
                var count = 0;
                foreach (var param in parms)
                {
                    parmsOut[count++] = new DSParameter(param);
                }

                return parmsOut;
            }
        }

        #endregion

        #region Public static constructors

        public DSFamilySymbol ByName(string name)
        {
            // look up the symbol
            TransactionManager.GetInstance().EnsureInTransaction(Document);

            var fec = new Autodesk.Revit.DB.FilteredElementCollector(Document);
            fec.OfClass(typeof(Autodesk.Revit.DB.Family));

            var symbol = fec.ToElements().FirstOrDefault(x => x.Name == name);

            if (symbol == null)
            {
                throw new Exception("A FamilySymbol with the specified name does not exist in the document");
            }

            TransactionManager.GetInstance().TransactionTaskDone();

            return new DSFamilySymbol((Autodesk.Revit.DB.FamilySymbol) symbol);
        }

        #endregion

    }
}
