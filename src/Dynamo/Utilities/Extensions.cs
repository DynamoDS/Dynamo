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
    }
}
