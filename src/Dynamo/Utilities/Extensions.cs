using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Dynamo.Utilities
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// An extension to the ObservableCollection class which allows you 
        /// to remove all objects which don't pass a predicate method.
        /// </summary>
        /// <typeparam name="T">The collection type.</typeparam>
        /// <param name="coll">The observable collection.</param>
        /// <param name="predicate">The predicate method.</param>
        /// <returns></returns>
        public static ObservableCollection<T> RemoveAll<T>(
            this ObservableCollection<T> coll, Predicate<T> predicate )
        {

            for (int i = coll.Count; i == 0; i-- )
            {
                if ( predicate.Invoke( coll[i] ) )
                {
                    coll.RemoveAt(i);
                }
            }

            return coll;
        }

        public static ObservableCollection<T> RemoveRange<T>(this ObservableCollection<T> coll, int index, int count)
        {
            if (index > coll.Count - 1)
            {
                throw new ArgumentException("Starting index is greater than the size of the collection.");
            }

            for (int i = index; i < coll.Count; i++)
            {
                coll.RemoveAt(i);
            }

            return coll;
        }

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            // base case: 
            IEnumerable<IEnumerable<T>> result = new[] { Enumerable.Empty<T>() };
            foreach (var sequence in sequences)
            {
                var s = sequence; // don't close over the loop variable 
                // recursive case: use SelectMany to build the new product out of the old one 
                result =
                  from seq in result
                  from item in s
                  select seq.Concat(new[] { item });
            }
            return result;
        }
        
        /// <summary>
        /// Get the longest list of arguments.
        /// For a set List of Lists like {a} {b1,b2,b3} {c1,c2}
        /// This will return a List of Lists of objects like:
        /// {a,b1,c1} {a,b2,c2} {a,b3,c2}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequences"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> LongestSet<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            //find the longest sequences
            int longest = sequences.Max(x => x.Count());

            //the result is a an enumerable
            List<List<T>> result = new List<List<T>>();

            for (int i = 0; i < longest; i++)
            {
                List<T> inner = new List<T>();

                foreach (var seq in sequences)
                {
                    if (i < seq.Count())
                        inner.Add(seq.ElementAt(i));
                    else
                        inner.Add(seq.Last());
                }
                result.Add(inner);
            }
            
            return result;
        }

        public static IEnumerable<IEnumerable<T>> ShortestSet<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            //find the longest sequences
            int shortest = sequences.Min(x => x.Count());

            //the result is a an enumerable
            List<List<T>> result = new List<List<T>>();

            for (int i = 0; i < shortest; i++)
            {
                List<T> inner = new List<T>();

                foreach (var seq in sequences)
                {
                    if (i < seq.Count())
                        inner.Add(seq.ElementAt(i));
                    else
                        inner.Add(seq.Last());
                }
                result.Add(inner);
            }

            return result;
        }

        public static IEnumerable<IEnumerable<T>> SingleSet<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            //find the longest sequences
            int shortest = sequences.Min(x => x.Count());

            //the result is a an enumerable
            List<List<T>> result = new List<List<T>>();

            List<T> inner = new List<T>();

            foreach (var seq in sequences)
            {
                inner.Add(seq.ElementAt(0));
            }
            result.Add(inner);

            return result;
        } 
    }
}
