using System;
using System.Collections.Generic;
using System.Linq;
using Dynamo.Units;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Expression = Dynamo.FScheme.Expression;

namespace Dynamo.FSchemeInterop
{
    /// <summary>
    /// Miscellaneous helper and convenience methods.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Makes an FScheme Expression representing an anonymous function.
        /// </summary>
        public static Expression MakeAnon(IEnumerable<string> inputSyms, Expression body)
        {
            return Expression.NewFun(
                ToFSharpList(inputSyms.Select(FScheme.Parameter.NewNormal)),
                body);
        }

        /// <summary>
        /// Makes an FScheme Expression representing an anonymous function, where all extra
        /// arguments are packed into the last parameter.
        /// </summary>
        /// <param name="inputSyms">List of parameters</param>
        /// <param name="body">Body of the function</param>
        /// <returns></returns>
        public static Expression MakeVarArgAnon(IEnumerable<string> inputSyms, Expression body)
        {
            var cnt = inputSyms.Count();

            return Expression.NewFun(
                ToFSharpList(inputSyms.Select(
                    (x, i) => 
                        i == cnt 
                        ? FScheme.Parameter.NewTail(x) 
                        : FScheme.Parameter.NewNormal(x))),
                body);
        }

        /// <summary>
        ///     A utility function to obtain the CSharp number type from a Value
        /// </summary>
        /// <param name="value">A Value object that returns true for IsNumber</param>
        /// <param name="convertedValue">The holder for the obtained value</param>
        /// <returns>False if the first param returns false for IsNumber</returns>
        public static bool Convert(Value value, ref double convertedValue)
        {
            if (!value.IsNumber)
                return false;
            convertedValue = (value as Value.Number).Item;
            return true;
        }

        /// <summary>
        ///     A utility function to obtain the CSharp number type from a Value
        /// </summary>
        /// <param name="value">A Value object that returns true for IsNumber</param>
        /// <param name="convertedValue">The holder for the obtained value</param>
        /// <returns>False if the first param returns false for IsNumber</returns>
        public static bool Convert(Value value, ref string convertedValue)
        {
            if (value.IsString)
            {
                convertedValue = ((Value.String)value).Item;
                return true;
            }

            if (value.IsSymbol)
            {
                convertedValue = ((Value.Symbol)value).Item;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     A utility function to obtain the CSharp number type from a Value
        /// </summary>
        /// <param name="value">A Value object that returns true for IsNumber</param>
        /// <param name="convertedValue">The holder for the obtained value</param>
        /// <returns>False if the first param returns false for IsNumber</returns>
        public static bool Convert(Value value, ref FSharpList<Value> convertedValue)
        {
            if (!value.IsList)
                return false;
            convertedValue = ((Value.List)value).Item;
            return true;
        }

        /// <summary>
        ///     A utility function to obtain the Geometry from a Value
        /// </summary>
        /// <param name="value">A Value object that returns Container</param>
        /// <param name="convertedValue">The holder for the obtained value</param>
        /// <returns>False if the first param returns false for IsContainer</returns>
        /*public static bool Convert(Value value, ref Geometry convertedValue)
        {
            convertedValue = null;
            if (!value.IsContainer)
                return false;
            object itemValue = (value as Value.Container).Item;
            convertedValue = itemValue as Geometry;
            return (convertedValue != null);
        }*/

        /// <summary>
        /// Converts a Func to an FSharpFunc.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static FSharpFunc<T, U> ConvertToFSharpFunc<T, U>(Converter<T, U> f)
        {
            return FSharpFunc<T, U>.FromConverter(f);
        }

        /// <summary>
        /// Converts a function that accepts a FSharpList of Values and returns a Value
        /// into a FScheme compatible version.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static FSharpFunc<FSharpList<Value>, Value> ConvertToFSchemeFunc(Converter<FSharpList<Value>, Value> f)
        {
            return ConvertToFSharpFunc(f);
        }

        /// <summary>
        /// Makes an FSharp list from all given arguments.
        /// </summary>
        public static FSharpList<T> MakeFSharpList<T>(params T[] ar)
        {
            return ToFSharpList(ar);
        }

        /// <summary>
        /// Converts the given IEnumerable into an FSharp list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <returns></returns>
        public static FSharpList<T> ToFSharpList<T>(this IEnumerable<T> seq)
        {
            return ListModule.OfSeq(seq);
        }

        public static FSharpList<T> SequenceToFSharpList<T>(this IEnumerable<T> seq)
        {
            return seq.ToFSharpList();
        }

        /// <summary>
        /// A better ToString() for Values.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string Print(this Value v)
        {
            return v.IsString 
                ? (v as Value.String).Item 
                : FScheme.print(v);
        }

        /// <summary>
        /// Determine whether the given list is a list of lists.
        /// </summary>
        /// 
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsListOfLists(Value value)
        {
            if (value.IsList)
            {
                FSharpList<Value> vals = ((Value.List) value).Item;

                if (!vals.Any())
                    return false;

                if (vals[0].IsList)
                    return true;
            }

            return false;
        }
        
        public static bool IsListOfListsOfLists(Value value)
        {
            if (value.IsList)
            {
                FSharpList<Value> vals = ((Value.List)value).Item;

                if (!vals.Any())
                    return false;

                if (vals[0].IsList)
                {
                    FSharpList<Value> vals2 = ((Value.List)vals[0]).Item;
                    if (vals2.Any() && vals2[0].IsList)
                        return true;
                }
            }

            return false;
        }

        public static dynamic ToDynamic(this Value value)
        {
            if (value.IsContainer)
            {
                var item = ((Value.Container) value).Item;
                return item;
            }
            
            if (value.IsNumber)
            {
                var item = ((Value.Number)value).Item;
                return item;
            }

            if (value.IsString)
            {
                var item = ((Value.String) value).Item;
                return item;
            }

            if (value.IsSymbol)
            {
                var item = ((Value.String) value).Item;
                return item;
            }

            if (value.IsFunction)
            {
                var item = ((Value.Function) value).Item;
                return item;
            }

            return null;
        }

        public static Value ToValue(SIUnit input)
        {
            return Value.NewContainer(input);
        }

        public static Value ToValue(double input)
        {
            return Value.NewNumber(input);
        }

        public static Value ToValue(string input)
        {
            return Value.NewString(input);
        }

        public static Value ToValue(object input)
        {
            return Value.NewContainer(input);
        }
        
        /// <summary>
        /// Unwrap an FScheme value containing a number or a unit to a double.
        /// If the value contains a unit object, convert the internal value of the
        /// unit object to the units required by the host application as specified
        /// in the preference settings. If the value contains a number, do not 
        /// apply a conversion.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Value UnwrapToDoubleWithHostUnitConversion(Value value)
        {
            if (value.IsList)
            {
                //recursively convert items in list
                return ConvertListToHostUnits((Value.List)value);
            }

            if (value.IsContainer)
            {
                var unit = ((Value.Container)value).Item as SIUnit;
                if (unit != null)
                {
                    return Value.NewNumber(unit.ConvertToHostUnits());
                }
            }

            return value;
        }

        private static Value ConvertListToHostUnits(Value.List value)
        {
            var list = value.Item;
            return Value.NewList(ToFSharpList(list.Select(UnwrapToDoubleWithHostUnitConversion)));
        }

        public static SIUnit UnwrapToSIUnit(Value value)
        {
            if (value.IsContainer)
            {
                var measure = ((Value.Container)value).Item as SIUnit;
                if (measure != null)
                {
                    return measure;
                }
            }

            throw new Exception("The value was not convertible to a unit of measure.");
        }

        public static IComparable ToComparable(Value value)
        {
            if (value.IsNumber)
                return (value as Value.Number).Item;

            if (value.IsString)
                return (value as Value.String).Item;

            if (value.IsContainer)
            {
                var unboxed = (value as Value.Container).Item;
                if (unboxed is IComparable)
                    return unboxed as IComparable;
            }

            throw new Exception(
                string.Format(
                    "Key mapper result {0} is not Comparable, and thus cannot be sorted.",
                    (value as dynamic).Item));
        }
    }
}
