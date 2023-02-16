using System;
using System.Collections.Generic;
using Autodesk.DesignScript.Runtime;
#if NET6_0_OR_GREATER
using ForgeUnitsCLR = Autodesk.ForgeUnits;
#endif

namespace DynamoUnits
{
    /// <summary>
    /// An object representing a property which is measurable.  A Quantity can be defined or derived from other quantities.
    /// </summary>
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
        /// Gets all available Units associated with a Quantity.
        /// </summary>
        /// <returns name="Unit[]">List of Units</returns>
        public IEnumerable<Unit> Units
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
        /// Creates a Quantity object from its Forge type schema identifier string.
        /// </summary>
        /// <param name="typeId">Forge TypeId string</param>
        /// <returns name="Quantity">Quantity object</returns>
        public static Quantity ByTypeID(string typeId)
        {
            try
            {
                return new Quantity(Utilities.ForgeUnitsEngine.getQuantity(typeId));
            }
            catch (Exception e)
            {
                //The exact match for the Forge TypeID failed.  Test for a fallback.  This can be either earlier or later version number.
                if (Utilities.TryParseTypeId(typeId, out string typeName, out Version version))
                {
                    var versionDictionary = Utilities.GetAllRegisteredQuantityVersions();
                    if (versionDictionary.TryGetValue(typeName, out var existingVersion))
                    {
                        return new Quantity(Utilities.ForgeUnitsEngine.getQuantity(typeName + "-" + existingVersion.ToString()));
                    }
                }

                //else re-throw existing exception as there is no fallback
                throw;
            }
        }

        public override string ToString()
        {
            return "Quantity" + "(Name = " + Name + ")";
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name ?? String.Empty).GetHashCode() ^
                       (TypeId ?? String.Empty).GetHashCode();
            }
        }

        public override bool Equals(object obj) => this.EqualsImpl(obj as Quantity);

        internal bool EqualsImpl(Quantity q)
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
            return lhs.EqualsImpl(rhs);
        }

        [IsVisibleInDynamoLibrary(false)]
        public static bool operator !=(Quantity lhs, Quantity rhs) => !(lhs == rhs);
    }
}
