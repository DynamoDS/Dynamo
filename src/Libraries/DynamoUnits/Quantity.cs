using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DynamoUnits;
using ProtoCore.AST.AssociativeAST;
using Newtonsoft.Json;
using ProtoCore.AST.ImperativeAST;
using AstFactory = ProtoCore.AST.AssociativeAST.AstFactory;
using DoubleNode = ProtoCore.AST.AssociativeAST.DoubleNode;
using System.Collections;
using DynamoUnits.Properties;

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
