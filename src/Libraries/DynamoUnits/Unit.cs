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
    public class Unit
    {
        internal readonly ForgeUnitsCLR.Unit forgeUnit;

        internal Unit(ForgeUnitsCLR.Unit unit)
        {
            this.forgeUnit = unit ?? throw new ArgumentNullException();
        }


        [NodeName("Name")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("Unit.NameDescription", typeof(Properties.Resources))]
        [NodeSearchTags("Unit.NameSearchTags", typeof(Properties.Resources))]
        [IsDesignScriptCompatible]
        /// <summary>
        /// Name of Unit
        /// </summary>
        /// <returns name="string">Name of location</returns>
        public string Name => forgeUnit.getName();


        [NodeName("Type Id")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("Unit.TypeIdDescription", typeof(Properties.Resources))]
        [NodeSearchTags("Unit.TypeIdSearchTags", typeof(Properties.Resources))]
        [IsDesignScriptCompatible]
        /// <summary>
        /// TypeId of Unit
        /// </summary>
        /// <returns name="string">TypeId of Unit</returns>
        public string TypeId => forgeUnit.getTypeId();


        [NodeName("Convertible Units")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("Unit.ConvertibleUnitsDescription", typeof(Properties.Resources))]
        [NodeSearchTags("Unit.ConvertibleUnitsSearchTags", typeof(Properties.Resources))]
        [IsDesignScriptCompatible]
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

        [NodeName("Quantities Containing Unit")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("Unit.QuantitiesContainingUnitDescription", typeof(Properties.Resources))]
        [NodeSearchTags("Unit.QuantitiesContainingUnitSearchTags", typeof(Properties.Resources))]
        [IsDesignScriptCompatible]
        public List<Quantity> QuantitiesContainingUnit
        {
            get
            {
                var quantities = Utilities.ForgeUnitsEngine.getQuantitiesContainingUnit(TypeId);
                return Utilities.CovertQuantityDictionaryToList(quantities);
            }
        }

        //public ForgeUnitsCLR.UnitSystem UnitSystem => forgeUnit.getUnitSystem();

        [NodeName("By Type Id")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("Unit.ByTypeIDDescription", typeof(Properties.Resources))]
        [NodeSearchTags("Unit.ByTypeIDSearchTags", typeof(Properties.Resources))]
        [IsDesignScriptCompatible]
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
        [NodeName("Are Units Convertible")]
        [NodeCategory(BuiltinNodeCategories.CORE_UNITS)]
        [NodeDescription("Unit.AreUnitsConvertibleDescription", typeof(Properties.Resources))]
        [NodeSearchTags("Unit.AreUnitsConvertibleSearchTags", typeof(Properties.Resources))]
        [IsDesignScriptCompatible]
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
