using System;
using System.Linq;
using Autodesk.Revit.DB;
using DSNodeServices;
using RevitServices.Persistence;

namespace Revit.Elements
{
    /// <summary>
    /// A Revit FamilySymbol
    /// </summary>
    [RegisterForTrace]
    public class FamilySymbol: AbstractElement
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
        public override Autodesk.Revit.DB.Element InternalElement
        {
            get { return InternalFamilySymbol; }
        }

        #endregion

        #region Private constructors

        /// <summary>
        /// Private constructor for build a DSFamilySymbol
        /// </summary>
        /// <param name="symbol"></param>
        private FamilySymbol(Autodesk.Revit.DB.FamilySymbol symbol)
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
        public Family Family
        {
            get
            {
                return Family.FromExisting(this.InternalFamilySymbol.Family, true);
            }
        }

        #endregion

        #region Public static constructors

        /// <summary>
        /// Select a FamilySymbol given it's parent Family and the FamilySymbol's name.
        /// </summary>
        /// <param name="family">The FamilySymbol's parent Family</param>
        /// <param name="name">The name of the FamilySymbol</param>
        /// <returns></returns>
        public static FamilySymbol ByFamilyAndName(Family family, string name)
        {
            if (family == null)
            {
                throw new ArgumentNullException("family");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            // obtain the family symbol with the provided name
            var symbol =
                family.InternalFamily.Symbols.Cast<Autodesk.Revit.DB.FamilySymbol>().FirstOrDefault(x => x.Name == name);

            if (symbol == null)
            {
               throw new Exception(String.Format("A FamilySymbol with the specified name, {0}, does not exist in the Family", name));
            }

            return new FamilySymbol(symbol)
            {
                IsRevitOwned = true
            };
        }

        /// <summary>
        /// Select a FamilySymbol give it's family name and type name.
        /// </summary>
        /// <param name="familyName">The FamilySymbol's parent Family name.</param>
        /// <param name="typeName">The name of the FamilySymbol.</param>
        /// <returns></returns>
        public static FamilySymbol ByFamilyNameAndTypeName(string familyName, string typeName)
        {
            if (familyName == null)
            {
                throw new ArgumentNullException("familyName");
            }

            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }

            //find the family
            var collector = new FilteredElementCollector(DocumentManager.GetInstance().CurrentDBDocument);
            collector.OfClass(typeof (Autodesk.Revit.DB.Family));
            var family = (Autodesk.Revit.DB.Family)collector.ToElements().FirstOrDefault(x => x.Name == familyName);

            if (family == null)
            {
                throw new Exception(string.Format("A family with the specified name, {0}, could not be found in the document.", familyName));
            }

            // obtain the family symbol with the provided name
            var symbol =
                family.Symbols.Cast<Autodesk.Revit.DB.FamilySymbol>().FirstOrDefault(x => x.Name == typeName);

            if (symbol == null)
            {
                throw new Exception(String.Format("A FamilySymbol with the specified name, {0}, does not exist in the Family", typeName));
            }

            return new FamilySymbol(symbol)
            {
                IsRevitOwned = true
            };
        }

        /// <summary>
        /// Select a FamilySymbol given it's name.  This method will return the first FamilySymbol it finds if there are
        /// two or more FamilySymbol's with the same name.
        /// </summary>
        /// <param name="name">The name of the FamilySymbol</param>
        /// <returns></returns>
        public static FamilySymbol ByName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException();
            }

            // look up the loaded family
            var symbol = DocumentManager.GetInstance()
                .ElementsOfType<Autodesk.Revit.DB.FamilySymbol>()
                .FirstOrDefault(x => x.Name == name);

            if (symbol == null)
            {
                throw new Exception(String.Format("A FamilySymbol with the specified name, {0}, does not exist in the document", name));
            }

            return new FamilySymbol(symbol)
            {
                IsRevitOwned = true
            };
        }

        #endregion

        #region Internal static constructors

        /// <summary>
        /// Obtain a FamilySymbol by selection. 
        /// </summary>
        /// <param name="familySymbol"></param>
        /// <param name="isRevitOwned"></param>
        /// <returns></returns>
        internal static FamilySymbol FromExisting(Autodesk.Revit.DB.FamilySymbol familySymbol, bool isRevitOwned)
        {
            return new FamilySymbol(familySymbol)
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
