using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoUnits
{
    public class Quantity
    {
        internal readonly ForgeUnitsCLR.Quantity forgeQuantity;

        internal Quantity(ForgeUnitsCLR.Quantity quantity)
        {
            this.forgeQuantity = quantity ?? throw new ArgumentNullException();
        }

        public string TypeId => forgeQuantity.getTypeId();

        public string Name => forgeQuantity.getName();

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
