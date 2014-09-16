using System;
using System.Reactive.Linq;

namespace Dynamo.Utilities
{
    public static class ObservableExtensions
    {
        /// <summary>
        ///     Filters out all nulls from an observable sequence.
        /// </summary>
        /// <typeparam name="T">Type of source elements.</typeparam>
        /// <param name="source">Source observable sequence.</param>
        /// <returns>Source observable sequence but with all null elements skipped.</returns>
        public static IObservable<T> SkipNulls<T>(this IObservable<T> source)
        {
            // ReSharper disable once CompareNonConstrainedGenericWithNull
            return source.Where(t => t != null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<T> SkipNones<T>(this IObservable<IOption<T>> source)
        {
            return source.SelectMany(x => x.Match(Observable.Return, Observable.Empty<T>));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static IObservable<T> Switch<TSource, T>(
            this IObservable<TSource> source, Func<TSource, IObservable<T>> selector)
        {
            return source.Select(selector).Switch();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IDisposable SubscribeExecute(this IObservable<Action> source)
        {
            return source.Subscribe(f => f());
        }
    }
}
