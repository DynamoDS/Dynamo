using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo;
using Dynamo.FSchemeInterop;
using IronPython.Runtime;
using Microsoft.FSharp.Collections;
using Microsoft.Scripting.Hosting;

namespace DynamoPython
{
    internal static class Converters
    {
        internal static FScheme.Value convertToValue(dynamic data, ObjectOperations invoker)
        {
            if (data is FScheme.Value)
                return data;
            else if (data is string)
                return FScheme.Value.NewString(data);
            else if (data is double || data is int || data is float)
                return FScheme.Value.NewNumber(data);
            else if (data is IEnumerable<dynamic>)
            {
                FSharpList<FScheme.Value> result = FSharpList<FScheme.Value>.Empty;

                //data.reverse(); // this breaks under certain circumstances

                var reversalList = new List<dynamic>();

                foreach (var x in data)
                {
                    reversalList.Add(x);
                }

                for (int i = reversalList.Count - 1; i >= 0; --i)
                {
                    var x = reversalList[i];

                    result = FSharpList<FScheme.Value>.Cons(convertToValue(x, invoker), result);
                }

                //foreach (var x in data)
                //{
                //    result = FSharpList<FScheme.Value>.Cons(convertToValue(x), result);
                //}

                return FScheme.Value.NewList(result);
            }
            else if (data is PythonFunction)
            {
                return
                    FScheme.Value.NewFunction(
                        Utils.ConvertToFSchemeFunc(
                            args =>
                                convertToValue(
                                    invoker.Invoke(
                                        data,
                                        args.Select(x => convertFromValue(x, invoker) as object)
                                            .ToArray()),
                                    invoker)));
            }
            else if (data is PyWrapper)
            {
                var func = data as PyWrapper;
                return
                    FScheme.Value.NewFunction(
                        Utils.ConvertToFSchemeFunc(
                            args =>
                                convertToValue(
                                    func(args.Select(a => convertFromValue(a, invoker)).ToArray()),
                                    invoker)));
            }
            else
                return FScheme.Value.NewContainer(data);
        }

        private delegate dynamic PyWrapper(params dynamic[] args);

        internal static dynamic convertFromValue(FScheme.Value exp, ObjectOperations invoker)
        {
            if (exp.IsList)
                return ((FScheme.Value.List)exp).Item.Select(x => convertFromValue(x, invoker)).ToList();
            else if (exp.IsNumber)
                return ((FScheme.Value.Number)exp).Item;
            else if (exp.IsString)
                return ((FScheme.Value.String)exp).Item;
            else if (exp.IsContainer)
                return ((FScheme.Value.Container)exp).Item;
            else if (exp.IsFunction)
            {
                var func = ((FScheme.Value.Function)exp).Item;

                PyWrapper wrapped =
                    args =>
                        convertFromValue(
                            func.Invoke(args.Select(a => convertToValue(a, invoker) as FScheme.Value).ToFSharpList()), invoker);

                return wrapped;
            }
            else
                throw new Exception("Not allowed to pass Functions into a Python Script.");
        }
    }
}
