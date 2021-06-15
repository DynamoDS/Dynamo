using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using Dynamo.Configuration;
using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Graph;
using Dynamo.Graph.Nodes;
using Dynamo.Graph.Workspaces;
using Dynamo.Migration;
using Dynamo.Nodes;
using Dynamo.UI.Commands;
using Dynamo.UI.Prompts;
using Dynamo.ViewModels;
using Dynamo.Wpf;

using DynamoUnits;
using ProtoCore.AST.AssociativeAST;
using Newtonsoft.Json;
using ProtoCore.AST.ImperativeAST;
using AstFactory = ProtoCore.AST.AssociativeAST.AstFactory;
using DoubleNode = ProtoCore.AST.AssociativeAST.DoubleNode;
using Dynamo.Utilities;
using Dynamo.Engine.CodeGeneration;
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

        [NodeName("Type Id")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("Quantity.TypeIdDescription", typeof(Properties.Resources))]
        [NodeSearchTags("Quantity.TypeIdSearchTags", typeof(Properties.Resources))]
        [IsDesignScriptCompatible]
        public string TypeId => forgeQuantity.getTypeId();

        [NodeName("Name")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("Quantity.NameDescription", typeof(Properties.Resources))]
        [NodeSearchTags("Quantity.NameSearchTags", typeof(Properties.Resources))]
        [IsDesignScriptCompatible]
        public string Name => forgeQuantity.getName();

        [NodeName("Units")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("Quantity.UnitsDescription", typeof(Properties.Resources))]
        [NodeSearchTags("Quantity.UnitsSearchTags", typeof(Properties.Resources))]
        [IsDesignScriptCompatible]
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

        [NodeName("By Type Id")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("Quantity.ByTypeIDDescription", typeof(Properties.Resources))]
        [NodeSearchTags("Quantity.ByTypeIDSearchTags", typeof(Properties.Resources))]
        [IsDesignScriptCompatible]
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
