using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;

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
        /// Gets the Forge type schema identifier for a Quantity
        /// </summary>
        /// <returns name="string">Forge TypeId</returns>
        public string TypeId => forgeQuantity.getTypeId();

        /// <summary>
        /// Gets the Name of for a Quantity
        /// </summary>
        /// <returns name="string">Name of Quantity</returns>
        public string Name => forgeQuantity.getName();

        /// <summary>
        /// Gets a list of all available Units associated with a Quantity.
        /// </summary>
        /// <returns name="Unit[]">List of Units</returns>
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
        /// Creates a Quantity object from its Forge type schema identify string.
        /// </summary>
        /// <param name="typeId">Forge TypeId string</param>
        /// <returns name="Quantity">Quantity object</returns>
        public static Quantity ByTypeID(string typeId)
        {
            return new Quantity(Utilities.ForgeUnitsEngine.getQuantity(typeId));
        }

        public override string ToString()
        {
            return "Quantity" + "(Name = " + Name + ")";
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name ?? String.Empty).GetHashCode() +
                       (TypeId ?? String.Empty).GetHashCode();
            }
        }

        public override bool Equals(object obj) => this.Equals(obj as Quantity);

        [IsVisibleInDynamoLibrary(false)]
        public bool Equals(Quantity q)
        {
            if (q is null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, q))
            {
                return true;
            }

            if (this.GetType() != q.GetType())
            {
                return false;
            }

            return (Name == q.Name) && (TypeId == q.TypeId);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static bool operator ==(Quantity lhs, Quantity rhs)
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
        public static bool operator !=(Quantity lhs, Quantity rhs) => !(lhs == rhs);
    }
}
