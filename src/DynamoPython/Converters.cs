using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Dynamo;
using Dynamo.FSchemeInterop;
using IronPython.Runtime;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace DynamoPython
{
    internal static class Converters
    {
        internal static FScheme.Value convertToValue(dynamic data)
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

                List<dynamic> reversal_list = new List<dynamic>();

                foreach (var x in data)
                {
                    reversal_list.Add(x);
                }

                for (int i = reversal_list.Count - 1; i >= 0; --i)
                {
                    var x = reversal_list[i];

                    result = FSharpList<FScheme.Value>.Cons(convertToValue(x), result);
                }

                //foreach (var x in data)
                //{
                //    result = FSharpList<FScheme.Value>.Cons(convertToValue(x), result);
                //}

                return FScheme.Value.NewList(result);
            }

            /*else if (data is PythonFunction)
            {
                var func = data as PythonFunction;
                return FScheme.Value.NewFunction(
                    delegate(FSharpList<FScheme.Value> args)
                    {
                        convertToValue(func.__call__())
                    });
            }*/
            //else if (data is Func<dynamic, dynamic>)
            //{
            //   return Value.NewCurrent(FuncContainer.MakeContinuation(
            //      new Continuation(
            //         exp =>
            //            convertToValue(
            //               data(convertFromValue(exp))
            //            )
            //      )
            //   ));
            //}
            else
                return FScheme.Value.NewContainer(data);
        }

        internal static dynamic convertFromValue(FScheme.Value exp)
        {
            if (exp.IsList)
                return ((FScheme.Value.List)exp).Item.Select(x => convertFromValue(x)).ToList();
            else if (exp.IsNumber)
                return ((FScheme.Value.Number)exp).Item;
            else if (exp.IsString)
                return ((FScheme.Value.String)exp).Item;
            else if (exp.IsContainer)
                return ((FScheme.Value.Container)exp).Item;
            else if (exp.IsFunction)
            {
                var func = ((FScheme.Value.Function)exp).Item;
                Func<IList<dynamic>, dynamic> wrapped =
                    args =>
                        convertFromValue(
                            func.Invoke(args.Select(convertToValue).SequenceToFSharpList()));
                return wrapped;
            }
            else
                throw new Exception("Not allowed to pass Functions into a Python Script.");
        }
    }
}
