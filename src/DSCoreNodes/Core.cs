using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace DSCoreNodes
{
    /// <summary>
    /// 
    /// </summary>
    class Function
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        public static Func<object, object> Compose(params Func<object, object>[] funcs)
        {
            if (!funcs.Any())
            {
                throw new ArgumentException("Need at least one function to perform composition.");
            }

            return funcs.Skip(1).Aggregate(funcs[0], (func, a) => x => func(a(x)));
        }

        /// <summary>
        /// Returns whatever is passed in.
        /// </summary>
        /// <param name="x">Anything.</param>
        /// <returns>The input.</returns>
        public static object Identity(object x)
        {
            return x;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    class List
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<T> Reverse<T>(IEnumerable<T> list)
        {
            return list.Reverse();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static IEnumerable<T> NewList<T>(params T[] elements)
        {
            return elements;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<T> Sort<T>(IEnumerable<T> list)
        {
            return SeqModule.Sort(list);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="keyProjection"></param>
        /// <returns></returns>
        public static IEnumerable<T> SortByKey<T, TKey>(
            IEnumerable<T> list, Converter<T, TKey> keyProjection)
        {
            return SeqModule.SortBy(FSharpFunc<T, TKey>.FromConverter(keyProjection), list);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public static IEnumerable<T> SortByComparison<T>(
            IEnumerable<T> list, Comparison<T> comparison)
        {
            var rtn = list.ToList();
            rtn.Sort(comparison);
            return rtn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="?"></param>
        /// <returns></returns>
        public static T MinimumValue<T>(IEnumerable<T> list)
        {
            return list.Min();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="list"></param>
        /// <param name="keyProjection"></param>
        /// <returns></returns>
        public static T MinimumValueByKey<T, TKey>(
            IEnumerable<T> list, Converter<T, TKey> keyProjection)
        {
            return SeqModule.MinBy(FSharpFunc<T, TKey>.FromConverter(keyProjection), list);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T MaximumValue<T>(IEnumerable<T> list)
        {
            return list.Max();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="list"></param>
        /// <param name="keyProjection"></param>
        /// <returns></returns>
        public static T MaximumValueByKey<T, TKey>(
            IEnumerable<T> list, Converter<T, TKey> keyProjection)
        {
            return SeqModule.MaxBy(FSharpFunc<T, TKey>.FromConverter(keyProjection), list);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TState"></typeparam>
        /// <param name="list"></param>
        /// <param name="seed"></param>
        /// <param name="reducer"></param>
        /// <returns></returns>
        public static TState Reduce<T, TState>(
            IEnumerable<T> list, TState seed, Func<T, TState, TState> reducer)
        {
            return
                SeqModule.Fold(
                    FSharpFunc<TState, FSharpFunc<T, TState>>.FromConverter(
                        state =>
                            FSharpFunc<T, TState>.FromConverter(element => reducer(element, state))),
                    seed,
                    list);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<T> Filter<T>(IEnumerable<T> list, Func<T, bool> predicate)
        {
            return list.Where(predicate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<T> FilterOut<T>(IEnumerable<T> list, Func<T, bool> predicate)
        {
            return list.Where(x => !predicate(x));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public delegate object MapDelegate(params object[] args);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projection"></param>
        /// <param name="lists"></param>
        /// <returns></returns>
        public static IEnumerable<object> Map(
            MapDelegate projection, params IEnumerable<object>[] lists)
        {
            if (!lists.Any())
                throw new ArgumentException("Need at least one list to map.");

            IEnumerable<List<object>> argList = lists[0].Select(x => new List<object> { x });

            foreach (var pair in
                lists.Skip(1)
                     .SelectMany(list => list.Zip(argList, (o, objects) => new { o, objects })))
            {
                pair.objects.Add(pair.o);
            }

            return argList.Select(x => projection(x.ToArray()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="projection"></param>
        /// <param name="lists"></param>
        /// <returns></returns>
        public static IEnumerable<object> CartesianProduce(
            MapDelegate projection, IEnumerable<object>[] lists)
        {
            if (!lists.Any())
                throw new ArgumentException("Need at least one list to map.");

            throw new NotImplementedException();
        }
    }
}
