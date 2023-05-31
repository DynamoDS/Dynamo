using System.Collections.Concurrent;

namespace DynamoAnalyzer.Extensions
{
    public static class ConcurrentBagExtensions
    {
        public static void AddRange<T>(this ConcurrentBag<T> bag, ICollection<T> data)
        {
            foreach (T item in data)
            {
                bag.Add(item);
            }
        }
    }
}
