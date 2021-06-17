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
