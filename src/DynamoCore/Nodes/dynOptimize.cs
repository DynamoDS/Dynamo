using System;
using System.Collections.Generic;
using Dynamo.Models;
using Microsoft.FSharp.Collections;

namespace Dynamo.Nodes
{
    [NodeName("Find Root")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_OPTIMIZE)]
    [NodeSearchTags("optimize", "newton")]
    [NodeDescription("Find the minimum of a 1 dimensional function without providing a derivative using Newton's method.")]
    public class NewtonRootFind1DNoDeriv : NodeWithOneOutput
    {
        public NewtonRootFind1DNoDeriv()
        {
            InPortData.Add(new PortData("f(x)", "Objective Function", typeof(FScheme.Value.Function)));
            InPortData.Add(new PortData("x", "Starting value for x", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("max_iterations", "Number of iterations", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(100)));

            OutPortData.Add(new PortData("result", "Result of function application.", typeof(object)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var f = ((FScheme.Value.Function)args[0]).Item;
            var x = ((FScheme.Value.Number)args[1]).Item;
            var max_its = (int) Math.Max(1, ((FScheme.Value.Number) args[2]).Item);

            var count = 0;
            var change = 1e10;
            var max_change = 1e-5;
            var h = 1e-5;

            // use newton's method 
            while (count < max_its && change > max_change)
            {
                var fx = InvokeFunction(f, x);
                var fxh = InvokeFunction(f, x + h);
                var dfx = (fxh - fx)/h;

                var x1 = x - fx/dfx;
                change = x - x1;
                x = x1;

                count++;
            }

            return FScheme.Value.NewNumber(x);
        }

        public static double InvokeFunction( Microsoft.FSharp.Core.FSharpFunc< FSharpList<FScheme.Value>, FScheme.Value > f, double x )
        {
            return ((FScheme.Value.Number) f.Invoke(DoubleToFSharpList(x))).Item;
        }

        public static FSharpList<FScheme.Value> DoubleToFSharpList(double x)
        {
            return FSchemeInterop.Utils.ToFSharpList(new List<FScheme.Value>() {FScheme.Value.NewNumber(x)});
        }

    }

    [NodeName("Find Root With Derivative")]
    [NodeCategory(BuiltinNodeCategories.LOGIC_MATH_OPTIMIZE)]
    [NodeSearchTags("optimize", "newton")]
    [NodeDescription("Find the minimum of a 1 dimensional function while providing a derivative using Newton's method.")]
    public class NewtonRootFind1DWithDeriv : NodeWithOneOutput
    {
        public NewtonRootFind1DWithDeriv()
        {
            InPortData.Add(new PortData("f(x)", "Objective Function", typeof(FScheme.Value.Function)));
            InPortData.Add(new PortData("df(x)/dx", "Derivative Function", typeof(FScheme.Value.Function)));
            InPortData.Add(new PortData("start", "Start value", typeof(FScheme.Value.Number)));
            InPortData.Add(new PortData("max_iterations", "Number of iterations", typeof(FScheme.Value.Number), FScheme.Value.NewNumber(100)));

            OutPortData.Add(new PortData("result", "Result of function application.", typeof(object)));

            RegisterAllPorts();
        }

        public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
        {
            var f = ((FScheme.Value.Function)args[0]).Item;
            var df = ((FScheme.Value.Function)args[1]).Item;
            var x = ((FScheme.Value.Number)args[2]).Item;
            var max_its = (int)Math.Max(1, ((FScheme.Value.Number)args[3]).Item);

            var count = 0;
            var change = 1e10;
            var max_change = 1e-5;

            // use newton's method 
            while (count < max_its && change > max_change)
            {
                var fx = NewtonRootFind1DNoDeriv.InvokeFunction(f, x);
                var dfx = NewtonRootFind1DNoDeriv.InvokeFunction(df, x);

                var x1 = x - fx / dfx;
                change = x - x1;
                x = x1;

                count++;
            }

            return FScheme.Value.NewNumber(x);
        }

    }

}


