namespace System.Collections.Concurrent
{
    /// <summary>
    /// Provides extension methods to ConcurrentBag
    /// </summary>
    internal static class ConcurrentBagExtensions
    {
        /// <summary>
        /// Adds a all the collection values to the ConcurrentBag
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bag"></param>
        /// <param name="data"></param>
        internal static void AddRange<T>(this ConcurrentBag<T> bag, ICollection<T> data)
        {
            foreach (T item in data)
            {
                bag.Add(item);
            }
        }
    }
}
