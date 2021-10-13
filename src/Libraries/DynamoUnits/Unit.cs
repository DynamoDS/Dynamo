using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;

namespace DynamoUnits
{
    public class Unit
    {
        internal readonly ForgeUnitsCLR.Unit forgeUnit;

        internal Unit(ForgeUnitsCLR.Unit unit)
        {
            this.forgeUnit = unit ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// Name of Unit
        /// </summary>
        /// <returns name="string">Name of location</returns>
        public string Name => forgeUnit.getName();

        /// <summary>
        /// TypeId of Unit
        /// </summary>
        /// <returns name="string">TypeId of Unit</returns>
        public string TypeId => forgeUnit.getTypeId();

        /// <summary>
        /// Convertible Units associated with this unit
        /// </summary>
        public List<Unit> ConvertibleUnits 
        {
            get
            {
                Dictionary<string, ForgeUnitsCLR.Unit> units = Utilities.ForgeUnitsEngine.getConvertibleUnits(TypeId);
                return Utilities.ConvertUnitsDictionaryToList(units);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public List<Quantity> QuantitiesContainingUnit
        {
            get
            {
                var quantities = Utilities.ForgeUnitsEngine.getQuantitiesContainingUnit(TypeId);
                return Utilities.CovertQuantityDictionaryToList(quantities);
            }
        }

        /// <summary>
        /// Create a Unit by a TypeID
        /// </summary>
        /// <param name="typeId">string representing the type</param>
        /// <returns>Unit</returns>
        public static Unit ByTypeID(string typeId)
        {
            return new Unit(Utilities.ForgeUnitsEngine.getUnit(typeId));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromUnit"></param>
        /// <param name="toUnit"></param>
        /// <returns></returns>
        public static bool AreUnitsConvertible(Unit fromUnit, Unit toUnit)
        {
            return Utilities.ForgeUnitsEngine.areUnitsConvertible(fromUnit.TypeId, toUnit.TypeId);
        }

        public override string ToString()
        {
            return Name; //"Unit" + "(Name = " + Name + ")";
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name ?? String.Empty).GetHashCode() +
                       (TypeId ?? String.Empty).GetHashCode();
            }
        }

        public override bool Equals(object obj) => this.Equals(obj as Unit);

        [IsVisibleInDynamoLibrary(false)]
        public bool Equals(Unit u)
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
            return lhs.Equals(rhs);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static bool operator !=(Unit lhs, Unit rhs) => !(lhs == rhs);
    }
}
