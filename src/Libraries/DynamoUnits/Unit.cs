using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                var units = Utilities.ForgeUnitsEngine.getConvertibleUnits(TypeId);
                return Utilities.ConvertUnitsDictionaryToList(units);
            }
        }

        public List<Quantity> QuantitiesContainingUnit
        {
            get
            {
                var quantities = Utilities.ForgeUnitsEngine.getQuantitiesContainingUnit(TypeId);
                return Utilities.CovertQuantityDictionaryToList(quantities);
            }
        }

        //public ForgeUnitsCLR.UnitSystem UnitSystem => forgeUnit.getUnitSystem();

        /// <summary>
        /// Create a Unit by a TypeID
        /// </summary>
        /// <param name="typeId">string representing the type</param>
        /// <returns>Unit</returns>
        public static Unit ByTypeID(string typeId)
        {
            return new Unit(Utilities.ForgeUnitsEngine.getUnit(typeId));
        }

        //[IsVisibleInDynamoLibrary(false)]
        //public static ForgeUnitsCLR.UnitSystem UnitSystemById(int id)
        //{
        //    return (ForgeUnitsCLR.UnitSystem)id;
        //}

        public static bool AreUnitsConvertible(Unit fromUnit, Unit toUnit)
        {
            return Utilities.ForgeUnitsEngine.areUnitsConvertible(fromUnit.TypeId, toUnit.TypeId);
        }

        public override string ToString()
        {
            return Name; //"Unit" + "(Name = " + Name + ")";
        }
    }
}
