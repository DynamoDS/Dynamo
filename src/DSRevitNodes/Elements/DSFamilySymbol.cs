using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Autodesk.DesignScript.Geometry;
using Autodesk.Revit.DB;
using DSNodeServices;
using DSRevitNodes.Elements;
using RevitServices.Transactions;

namespace DSRevitNodes.Elements
{
    /// <summary>
    /// A Revit FamilySymbol
    /// </summary>
    [RegisterForTrace]
    public class DSFamilySymbol: AbstractElement
    {

        #region Internal Properties

        /// <summary>
        /// Internal wrapper property
        /// </summary>
        internal Autodesk.Revit.DB.FamilySymbol InternalFamilySymbol
        {
            get;
            private set;
        }

        /// <summary>
        /// Reference to the Element
        /// </summary>
        internal override Element InternalElement
        {
            get { return InternalFamilySymbol; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Private constructor for build a DSFamilySymbol
        /// </summary>
        /// <param name="symbol"></param>
        private DSFamilySymbol(Autodesk.Revit.DB.FamilySymbol symbol)
        {
            InternalSetFamilySymbol(symbol);
        }

        #endregion

        #region Private mutators

        /// <summary>
        /// Set the internal model of the family symbol along with its ElementId and UniqueId
        /// </summary>
        /// <param name="symbol"></param>
        private void InternalSetFamilySymbol(Autodesk.Revit.DB.FamilySymbol symbol)
        {
            this.InternalFamilySymbol = symbol;
            this.InternalElementId = symbol.Id;
            this.InternalUniqueId = symbol.UniqueId;
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Get the name of this Family Symbol
        /// </summary>
        public string Name
        {
            get
            {
                return InternalFamilySymbol.Name;
            }
        }

        /// <summary>
        /// Get the parent family of this FamilySymbol
        /// </summary>
        public DSFamily Family
        {
            get
            {
                return DSFamily.FromExisting(this.InternalFamilySymbol.Family, true);
            }
        }

        /// <summary>
        /// Get a list of parameters from this FamilySymbol
        /// </summary>
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

        /// <summary>
        /// Select a FamilySymbol given it's full name including parent family, delimited by a period.
        /// For example, the FamilySymbol Box in the Family Mass would be identified as "Mass.Box"
        /// </summary>
        /// <param name="name">The name of the FamilySymbol as FamilyName.FamilySymbolName </param>
        /// <returns></returns>
        public static DSFamilySymbol ByName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException();
            }

            TransactionManager.GetInstance().EnsureInTransaction(Document);

            // look up the loaded family
            var fec = new Autodesk.Revit.DB.FilteredElementCollector(Document);
            fec.OfClass(typeof(Autodesk.Revit.DB.Family));

            // obtain the family symbol with the provided name
            var symbols = fec.Cast<Autodesk.Revit.DB.Family>()
                            .SelectMany(x => x.Symbols.Cast<Autodesk.Revit.DB.FamilySymbol>());

            var symbol = symbols.FirstOrDefault(x => x.Family.Name + "." + x.Name == name); 

            if (symbol == null)
            {
                throw new Exception("A FamilySymbol with the specified name does not exist in the document");
            }

            TransactionManager.GetInstance().TransactionTaskDone();

            return new DSFamilySymbol(symbol);
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Obtain a FamilySymbol by selection. 
        /// </summary>
        /// <param name="familySymbol"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static DSFamilySymbol FromExisting(Autodesk.Revit.DB.FamilySymbol familySymbol, bool isRevitOwned)
        {
            return new DSFamilySymbol(familySymbol)
            {
                IsRevitOwned = isRevitOwned
            };
        }

        #endregion

        #region ToString override

        public override string ToString()
        {
            return String.Format("FamilySymbol: {0}, Family: {1}", InternalFamilySymbol.Name,
                InternalFamilySymbol.Family.Name);
        }

        #endregion



    }
}
