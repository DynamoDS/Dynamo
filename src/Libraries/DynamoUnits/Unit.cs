using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
#if NET6_0_OR_GREATER
using ForgeUnitsCLR = Autodesk.ForgeUnits;
#endif

namespace DynamoUnits
{
    /// <summary>
    /// An object representing a standard system for measuring a quantity.
    /// </summary>
    public class Unit
    {
        internal readonly ForgeUnitsCLR.Unit forgeUnit;

        internal Unit(ForgeUnitsCLR.Unit unit)
        {
            this.forgeUnit = unit ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// Gets the Name of for a Unit
        /// </summary>
        /// <returns name="string">Name of Unit</returns>
        public string Name => forgeUnit.getName();

        /// <summary>
        /// Gets the Forge type schema identifier for a Unit
        /// </summary>
        /// <returns name="string">Forge TypeId</returns>
        public string TypeId => forgeUnit.getTypeId();

        /// <summary>
        /// Gets all Units which are convertible from a Unit.
        /// </summary>
        /// <returns name="Unit[]">List of Units</returns>
        public IEnumerable<Unit> ConvertibleUnits 
        {
            get
            {
                Dictionary<string, ForgeUnitsCLR.Unit> units = Utilities.ForgeUnitsEngine.getConvertibleUnits(TypeId);
                return Utilities.ConvertForgeUnitDictionaryToCollection(units);
            }
        }

        /// <summary>
        /// Gets all Quantity objects which contain a Unit. 
        /// </summary>
        /// /// <returns name="Quantity[]">List of Quantities</returns>
        public IEnumerable<Quantity> QuantitiesContainingUnit
        {
            get
            {
                var quantities = Utilities.ForgeUnitsEngine.getQuantitiesContainingUnit(TypeId);
                return Utilities.CovertForgeQuantityDictionaryToCollection(quantities);
            }
        }

        /// <summary>
        /// Creates a Unit object from its Forge type schema identifier string.
        /// </summary>
        /// <param name="typeId">Forge TypeId string</param>
        /// <returns name="Unit">Unit object</returns>
        public static Unit ByTypeID(string typeId)
        {
            try
            {
                return new Unit(Utilities.ForgeUnitsEngine.getUnit(typeId));
            }
            catch (Exception)
            {
                //The exact match for the Forge TypeID failed.  Test for a fallback.  This can be either earlier or later version number.
                if (Utilities.TryParseTypeId(typeId, out string typeName, out Version version))
                {
                    var versionDictionary = Utilities.GetAllLatestRegisteredUnitVersions();
                    if (versionDictionary.TryGetValue(typeName, out var existingVersion))
                    {
                        return new Unit(Utilities.ForgeUnitsEngine.getUnit(typeName + "-" + existingVersion.ToString()));
                    }
                }

                //else re-throw existing exception as there is no fallback
                throw;
            }
        }

        /// <summary>
        /// Determine whether two Unit objects are convertible
        /// </summary>
        /// <param name="fromUnit">Unit Object</param>
        /// <param name="toUnit">Unit Object</param>
        public static bool AreUnitsConvertible(Unit fromUnit, Unit toUnit)
        {
            return Utilities.ForgeUnitsEngine.areUnitsConvertible(fromUnit.TypeId, toUnit.TypeId);
        }

        public override string ToString()
        {
            return "Unit" + "(Name = " + Name + ")";
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name ?? String.Empty).GetHashCode() ^
                       (TypeId ?? String.Empty).GetHashCode();
            }
        }

        public override bool Equals(object obj) => this.EqualsImpl(obj as Unit);

        internal bool EqualsImpl(Unit u)
        {
            if (u is null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, u))
            {
                return true;
            }

            if (this.GetType() != u.GetType())
            {
                return false;
            }

            return (Name == u.Name) && (TypeId == u.TypeId);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static bool operator ==(Unit lhs, Unit rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles case of null on right side.
            return lhs.EqualsImpl(rhs);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static bool operator !=(Unit lhs, Unit rhs) => !(lhs == rhs);
    }
}
