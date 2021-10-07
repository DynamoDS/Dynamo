using System;
using System.Collections.Generic;

namespace DynamoUnits
{
    public class Quantity
    {
        internal readonly ForgeUnitsCLR.Quantity forgeQuantity;

        internal Quantity(ForgeUnitsCLR.Quantity quantity)
        {
            this.forgeQuantity = quantity ?? throw new ArgumentNullException();
        }

        /// <summary>
        /// 
        /// </summary>
        public string TypeId => forgeQuantity.getTypeId();
        
        /// <summary>
        /// 
        /// </summary>
        public string Name => forgeQuantity.getName();

        /// <summary>
        /// Returns a list of all available Units.
        /// </summary>
        public List<Unit> Units
        {
            get
            {
                var units = forgeQuantity.getUnits();
                var dynUnits = new List<Unit>();
                foreach (var unit in units)
                {
                    dynUnits.Add(new Unit(unit));
                }

                return dynUnits;
            }
        }
        /// <summary>
        /// Returns an object of type Quantity from its typeId string.
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public static Quantity ByTypeID(string typeId)
        {
            return new Quantity(Utilities.ForgeUnitsEngine.getQuantity(typeId));
        }

        public override string ToString()
        {
            return Name; //"Quantity" + "(Name = " + Name + ")";
        }

    }
}
