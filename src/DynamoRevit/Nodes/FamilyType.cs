using System;
using System.Linq;
using Autodesk.Revit.DB;
using Dynamo.FSchemeInterop;
using Dynamo.Models;
using Dynamo.Revit;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using RevitServices.Persistence;

namespace Dynamo.Nodes
{
    [NodeName("Select Family Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Select a Family Type from a drop down list.")]
    [NodeSearchTags("type")]
    [IsInteractive(true)]
    public class FamilyTypeSelector : DropDrownBase
    {
        public FamilyTypeSelector()
        {
            OutPortData.Add(new PortData("", "Family type", typeof(FScheme.Value.Container)));
            RegisterAllPorts();

            PopulateItems();
        }

        public override void PopulateItems()
        {
            Items.Clear();

            //load all the currently loaded types into the combo list
            var fec = new FilteredElementCollector(DocumentManager.GetInstance().CurrentUIDocument.Document);
            fec.OfClass(typeof(Family));
            foreach (Family f in fec.ToElements())
            {
                foreach (FamilySymbol fs in f.Symbols)
                {
                    Items.Add(new DynamoDropDownItem(f.Name + ":" + fs.Name, fs));
                }
            }
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            if (SelectedIndex < 0)
                throw new Exception("Nothing selected!");

            return FScheme.Value.NewContainer(Items[SelectedIndex].Item);
        }

    }

    [NodeName("Set Family Type Parameter")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Modifies a parameter on a family type.")]
    public class FamilyTypeParameterSetter : RevitTransactionNodeWithOneOutput
    {
        public FamilyTypeParameterSetter()
        {
            InPortData.Add(new PortData("ft", "Family type.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("param", "Parameter to modify.", typeof(FScheme.Value.String)));
            InPortData.Add(new PortData("value", "Value to set the parameter to.", typeof(object)));
            OutPortData.Add(new PortData("ft", "Modified family type.", typeof(FScheme.Value.Container)));

            RegisterAllPorts();
        }

        private static FScheme.Value setParam(FamilySymbol fi, string paramName, FScheme.Value valueExpr)
        {
            var p = fi.get_Parameter(paramName);
            if (p != null)
            {
                return _setParam(fi, p, valueExpr);
            }
            throw new Exception("Parameter \"" + paramName + "\" was not found!");
        }

        private static FScheme.Value setParam(FamilySymbol fi, Definition paramDef, FScheme.Value valueExpr)
        {
            var p = fi.get_Parameter(paramDef);
            if (p != null)
            {
                return _setParam(fi, p, valueExpr);
            }
            throw new Exception("Parameter \"" + paramDef.Name + "\" was not found!");
        }

        private static FScheme.Value _setParam(FamilySymbol ft, Parameter p, FScheme.Value valueExpr)
        {
            if (p.StorageType == StorageType.Double)
            {
                p.Set(((FScheme.Value.Number)valueExpr).Item);
            }
            else if (p.StorageType == StorageType.Integer)
            {
                p.Set((int)((FScheme.Value.Number)valueExpr).Item);
            }
            else if (p.StorageType == StorageType.String)
            {
                p.Set(((FScheme.Value.String)valueExpr).Item);
            }
            else if (valueExpr.IsNumber)
            {
                p.Set(new ElementId((int)(valueExpr as FScheme.Value.Number).Item));
            }
            else
            {
                p.Set((ElementId)((FScheme.Value.Container)valueExpr).Item);
            }
            return FScheme.Value.NewContainer(ft);
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var valueExpr = args[2];

            var param = args[1];
            if (param.IsString)
            {
                var paramName = ((FScheme.Value.String)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as FScheme.Value.List).Item;
                    return FScheme.Value.NewList(
                        Utils.SequenceToFSharpList(
                            fiList.Select(
                                x =>
                                setParam(
                                    (FamilySymbol)((FScheme.Value.Container)x).Item,
                                    paramName,
                                    valueExpr
                                    )
                                )
                            )
                        );
                }
                else
                {
                    var fs = (FamilySymbol)((FScheme.Value.Container)input).Item;

                    return setParam(fs, paramName, valueExpr);
                }
            }
            else
            {
                var paramDef = (Definition)((FScheme.Value.Container)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as FScheme.Value.List).Item;
                    return FScheme.Value.NewList(
                        Utils.SequenceToFSharpList(
                            fiList.Select(
                                x =>
                                setParam(
                                    (FamilySymbol)((FScheme.Value.Container)x).Item,
                                    paramDef,
                                    valueExpr
                                    )
                                )
                            )
                        );
                }
                else
                {
                    var fs = (FamilySymbol)((FScheme.Value.Container)input).Item;

                    return setParam(fs, paramDef, valueExpr);
                }
            }
        }
    }

    [NodeName("Get Family Type Parameter")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Fetches the value of a parameter of a Family Type.")]
    public class FamilyTypeParameterGetter : RevitTransactionNodeWithOneOutput
    {
        public FamilyTypeParameterGetter()
        {
            InPortData.Add(new PortData("ft", "Family type.", typeof(FScheme.Value.Container)));
            InPortData.Add(new PortData("param", "Parameter to fetch (string).", typeof(FScheme.Value.String)));

            OutPortData.Add(new PortData("val", "Parameter value.", typeof(object)));

            RegisterAllPorts();
        }

        private static FScheme.Value getParam(FamilySymbol fi, string paramName)
        {
            var p = fi.get_Parameter(paramName);
            if (p != null)
            {
                return _getParam(fi, p);
            }
            throw new Exception("Parameter \"" + paramName + "\" was not found!");
        }

        private static FScheme.Value getParam(FamilySymbol fi, Definition paramDef)
        {
            var p = fi.get_Parameter(paramDef);
            if (p != null)
            {
                return _getParam(fi, p);
            }
            throw new Exception("Parameter \"" + paramDef.Name + "\" was not found!");
        }

        private static FScheme.Value _getParam(FamilySymbol fi, Parameter p)
        {
            if (p.StorageType == StorageType.Double)
            {
                return FScheme.Value.NewNumber(p.AsDouble());
            }
            else if (p.StorageType == StorageType.Integer)
            {
                return FScheme.Value.NewNumber(p.AsInteger());
            }
            else if (p.StorageType == StorageType.String)
            {
                return FScheme.Value.NewString(p.AsString());
            }
            else
            {
                return FScheme.Value.NewContainer(p.AsElementId());
            }
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var param = args[1];
            if (param.IsString)
            {
                var paramName = ((FScheme.Value.String)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as FScheme.Value.List).Item;
                    return FScheme.Value.NewList(
                        Utils.SequenceToFSharpList(
                            fiList.Select(
                                x =>
                                getParam(
                                    (FamilySymbol)((FScheme.Value.Container)x).Item,
                                    paramName
                                    )
                                )
                            )
                        );
                }
                else
                {
                    var fi = (FamilySymbol)((FScheme.Value.Container)input).Item;

                    return getParam(fi, paramName);
                }
            }
            else
            {
                var paramDef = (Definition)((FScheme.Value.Container)param).Item;

                var input = args[0];
                if (input.IsList)
                {
                    var fiList = (input as FScheme.Value.List).Item;
                    return FScheme.Value.NewList(
                        Utils.SequenceToFSharpList(
                            fiList.Select(
                                x =>
                                getParam(
                                    (FamilySymbol)((FScheme.Value.Container)x).Item,
                                    paramDef
                                    )
                                )
                            )
                        );
                }
                else
                {
                    var fi = (FamilySymbol)((FScheme.Value.Container)input).Item;

                    return getParam(fi, paramDef);
                }
            }
        }
    }

    [NodeName("Get Family Instances by Type")]
    [NodeCategory(BuiltinNodeCategories.REVIT_FAMILIES)]
    [NodeDescription("Returns all family instances of the selected type in the active model.")]
    public class GetFamilyInstancesByType : NodeWithOneOutput
    {
        public GetFamilyInstancesByType()
        {
            InPortData.Add(new PortData("type", "The type of the family you want to find.", typeof(FScheme.Value.Container)));
            OutPortData.Add(new PortData("instances", "The instance(s) of the selected type found in the active model.",
                                         typeof(FScheme.Value.List)));
            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var symbol = (FamilySymbol)((FScheme.Value.Container)args[0]).Item;
            var collector = new FilteredElementCollector(DocumentManager.GetInstance().CurrentUIDocument.Document);
            collector.OfClass(typeof(FamilyInstance));
            var fis = collector.ToElements().Where(x => x is FamilyInstance).Cast<FamilyInstance>().Where(x => x.Symbol.Name == symbol.Name);
            var results = fis.Aggregate(FSharpList<FScheme.Value>.Empty,
                                        (current, fi) => FSharpList<FScheme.Value>.Cons(FScheme.Value.NewContainer(fi), current));

            return FScheme.Value.NewList(Utils.SequenceToFSharpList(results.Reverse()));
        }
    }
}
