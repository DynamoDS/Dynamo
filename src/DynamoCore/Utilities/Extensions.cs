using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using ICSharpCode.AvalonEdit.Utils;

namespace Dynamo.Utilities
{
    /// <summary>
    ///     A collection of recursive groupings that have a common key.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRecursiveGrouping<out T> : IGrouping<T, IRecursiveGrouping<T>> { }

    /// <summary>
    ///     A tree that has a tag, leaves, and subtrees.
    /// </summary>
    /// <typeparam name="TNodeTag"></typeparam>
    /// <typeparam name="TLeaf"></typeparam>
    public interface ITree<out TNodeTag, out TLeaf>
    {
        TNodeTag Tag { get; }
        IEnumerable<TLeaf> Leaves { get; }
        IEnumerable<ITree<TNodeTag, TLeaf>> SubTrees { get; }
    }

    public static class ExtensionMethods
    {
        /// <summary>
        ///     Gets all the tags of an ITree.
        /// </summary>
        /// <typeparam name="TNodeTag"></typeparam>
        /// <typeparam name="TLeaf"></typeparam>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static IEnumerable<TNodeTag> GetAllTags<TNodeTag, TLeaf>(
            this ITree<TNodeTag, TLeaf> tree)
        {
            yield return tree.Tag;
            foreach (var tag in tree.SubTrees.SelectMany(sub => sub.GetAllTags()))
                yield return tag;
        }

        /// <summary>
        ///     Contructs a tree by recursively grouping elements from a sequence. Essentially, performs
        ///     a GroupBy operation, and then for each Grouping, performs another GroupBy, assuming there
        ///     is a GroupBy key available for the sub-group.
        /// </summary>
        /// <typeparam name="TLeaf"></typeparam>
        /// <typeparam name="TNodeKey"></typeparam>
        /// <param name="allLeaves"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<IRecursiveGrouping<IEither<TNodeKey, IEnumerable<TLeaf>>>> GroupByRecursive<TLeaf, TNodeKey>(
            this IEnumerable<TLeaf> allLeaves, Func<TLeaf, ICollection<TNodeKey>> keySelector)
        {
            var query =
                allLeaves.Select(
                    x =>
                        new KeyValuePair<TLeaf, ImmutableStack<TNodeKey>>(
                            x,
                            keySelector(x)
                                .Reverse()
                                .Aggregate(
                                    ImmutableStack<TNodeKey>.Empty,
                                    (keys, key) => keys.Push(key))));
            return GroupByRecursive(query);
        }

        /// <summary>
        ///     Contructs a tree by recursively grouping elements from a sequence. Essentially, performs
        ///     a GroupBy operation, and then for each Grouping, performs another GroupBy, assuming there
        ///     is a GroupBy key available for the sub-group.
        /// </summary>
        /// <typeparam name="TNodeTag"></typeparam>
        /// <typeparam name="TLeaf"></typeparam>
        /// <param name="entries"></param>
        /// <param name="categorySelector"></param>
        /// <param name="rootTag"></param>
        /// <returns></returns>
        public static ITree<TNodeTag, TLeaf> GroupByRecursive<TNodeTag, TLeaf>(
            this IEnumerable<TLeaf> entries, Func<TLeaf, ICollection<TNodeTag>> categorySelector,
            TNodeTag rootTag)
        {
            return entries.GroupByRecursive<TLeaf, TNodeTag, Tree<TNodeTag, TLeaf>>(
                categorySelector,
                Tree<TNodeTag, TLeaf>.Create,
                rootTag);
        }

        /// <summary>
        ///     Contructs a tree by recursively grouping elements from a sequence. Essentially, performs
        ///     a GroupBy operation, and then for each Grouping, performs another GroupBy, assuming there
        ///     is a GroupBy key available for the sub-group.
        /// </summary>
        /// <typeparam name="TLeaf"></typeparam>
        /// <typeparam name="TNodeKey"></typeparam>
        /// <typeparam name="TTree"></typeparam>
        /// <param name="allLeaves"></param>
        /// <param name="keySelector"></param>
        /// <param name="treeCreator"></param>
        /// <param name="rootKey"></param>
        /// <returns></returns>
        public static TTree GroupByRecursive<TLeaf, TNodeKey, TTree>(
            this IEnumerable<TLeaf> allLeaves, Func<TLeaf, ICollection<TNodeKey>> keySelector,
            Func<TNodeKey, IEnumerable<TTree>, IEnumerable<TLeaf>, TTree> treeCreator, TNodeKey rootKey)
        {
            return GroupByRecursive(allLeaves.GroupByRecursive(keySelector), treeCreator, rootKey);
        }
        
        #region GroupByRecursive helpers

        private sealed class Tree<TNode, TEntry> : ITree<TNode, TEntry>
        {
            private Tree(TNode name, IEnumerable<TEntry> entries, IEnumerable<Tree<TNode, TEntry>> subCategories)
            {
                SubTrees = subCategories;
                Leaves = entries;
                Tag = name;
            }

            public static Tree<TNode, TEntry> Create(TNode categoryName, IEnumerable<Tree<TNode, TEntry>> subCategories, IEnumerable<TEntry> entries)
            {
                return new Tree<TNode, TEntry>(categoryName, entries, subCategories);
            }

            public TNode Tag { get; private set; }
            public IEnumerable<TEntry> Leaves { get; private set; }
            public IEnumerable<ITree<TNode, TEntry>> SubTrees { get; private set; }
        }


        private static V GroupByRecursive<TLeaf, TNodeKey, V>(
            IEnumerable<IRecursiveGrouping<IEither<TNodeKey, IEnumerable<TLeaf>>>> grouped,
            Func<TNodeKey, IEnumerable<V>, IEnumerable<TLeaf>, V> grouper, TNodeKey rootKey)
        {
            var leaves = new List<TLeaf>();
            var subCategories = new List<V>();

            foreach (var grouping in grouped)
            {
                IRecursiveGrouping<IEither<TNodeKey, IEnumerable<TLeaf>>> grouping1 = grouping;
                grouping.Key
                    .SelectLeft(categoryName => GroupByRecursive(grouping1, grouper, categoryName))
                    .Match(subCategories.Add, leaves.AddRange);
            }

            return grouper(rootKey, subCategories, leaves);
        }

        private static IEnumerable<IRecursiveGrouping<IEither<TNodeKey, IEnumerable<TLeaf>>>> GroupByRecursive
            <TLeaf, TNodeKey>(IEnumerable<KeyValuePair<TLeaf, ImmutableStack<TNodeKey>>> entries)
        {
            return
                entries.GroupBy(
                    entry => entry.Value.IsEmpty ? Option.None<TNodeKey>() : Option.Some(entry.Value.Peek()))
                    .Select(
                        g =>
                            g.Key.Match(
                                key =>
                                    new RecursiveGrouping<IEither<TNodeKey, IEnumerable<TLeaf>>>(
                                        Either.Left<TNodeKey, IEnumerable<TLeaf>>(key),
                                        GroupByRecursive(
                                            g.Select(
                                                kv =>
                                                    new KeyValuePair<TLeaf, ImmutableStack<TNodeKey>>(
                                                    kv.Key,
                                                    kv.Value.Pop())))),
                                () =>
                                    new RecursiveGrouping<IEither<TNodeKey, IEnumerable<TLeaf>>>(
                                        Either.Right<TNodeKey, IEnumerable<TLeaf>>(g.Select(x => x.Key)),
                                        Enumerable
                                            .Empty<
                                                IRecursiveGrouping<
                                                    IEither<TNodeKey, IEnumerable<TLeaf>>>>())));
        }

        private class RecursiveGrouping<T> : IRecursiveGrouping<T>
        {
            private readonly IEnumerable<IRecursiveGrouping<T>> source;

            public RecursiveGrouping(T key, IEnumerable<IRecursiveGrouping<T>> source)
            {
                Key = key;
                this.source = source;
            }

            public IEnumerator<IRecursiveGrouping<T>> GetEnumerator()
            {
                return source.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public T Key { get; private set; }
        }
        #endregion

        public static IEnumerable<T> AsSingleton<T>(this T o)
        {
            yield return o;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public interface IIndexed<out T>
        {
            T Element { get; }
            int Index { get; }
        }

        private struct EnumerationIndex<T> : IIndexed<T>
        {
            public int Index { get; private set; }
            public T Element { get; private set; }

            public static EnumerationIndex<T> Create(T element, int index)
            {
                return new EnumerationIndex<T> { Element = element, Index = index };
            }
        }

        public static Collection<T> AddRange<T>(this Collection<T> collection, IEnumerable<T> items)
        {
            if (collection == null) throw new ArgumentNullException("collection");
            if (items == null) throw new ArgumentNullException("items");

            foreach (var each in items)
            {
                collection.Add(each);
            }

            return collection;
        }

        public static IEnumerable<IIndexed<T>> Enumerate<T>(this IEnumerable<T> seq)
        {
            return seq.Select(EnumerationIndex<T>.Create).Cast<IIndexed<T>>();
        }

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
                throw new ArgumentException("Starting index is greater than the size of the collection.");

            if (index + count > coll.Count)
                throw new ArgumentException("Range extends beyond the end of the list.");

            if (count < 0)
                throw new ArgumentException("Cannot have negative count.");

            if (index < 0)
                throw new ArgumentException("Cannot have negative index.");

            for (int i = 0; i < count; i++)
            {
                coll.RemoveAt(index);
            }

            return coll;
        }

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            // base case: 
            IEnumerable<IEnumerable<T>> result = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                result,
                (current, s) =>
                    (from seq in current from item in s select seq.Concat(new[] { item })));
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

        public static IEnumerable<IEnumerable<T>> ShortestSet<T>(
            this IEnumerable<IEnumerable<T>> sequences)
        {
            //find the shortest sequences
            int shortest = sequences.Min(x => x.Count());

            //the result is a an enumerable
            var result = new List<List<T>>();

            for (int i = 0; i < shortest; i++)
            {
                var inner = new List<T>();

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

        public static IEnumerable<IEnumerable<T>> SingleSet<T>(
            this IEnumerable<IEnumerable<T>> sequences)
        {
            //the result is a an enumerable
            var result = new List<List<T>>();

            var inner = sequences.Select(seq => seq.ElementAt(0)).ToList();

            result.Add(inner);

            return result;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> coll)
        {
            var c = new ObservableCollection<T>();
            foreach (var e in coll)
                c.Add(e);
            return c;
        }

        public static ThreadSafeList<TSource> ToThreadSafeList<TSource>(this IEnumerable<TSource> source)
        {
            var tsl = new ThreadSafeList<TSource>();
            foreach (var item in source)
            {
                tsl.Add(item);
            }
            return tsl;
        }

        public static string GetFullName(this Delegate del)
        {
            if (del.Method.DeclaringType == null)
                throw new ArgumentException(@"Delegate has no declaring type.", @"del");

            return String.Format("{0}.{1}", del.Method.DeclaringType.FullName, del.Method.Name);
        }

        public static string GetChildNodeStringValue(XmlNode nodeElement)
        {
            return GetChildNodeValue(nodeElement, typeof(string).FullName);
        }

        public static string GetChildNodeDoubleValue(XmlNode nodeElement)
        {
            return GetChildNodeValue(nodeElement, typeof(double).FullName);
        }

        private static string GetChildNodeValue(XmlNode nodeElement, string typeName)
        {
            var query = from XmlNode childNode in nodeElement.ChildNodes
                        where childNode.Name.Equals(typeName)
                        from XmlAttribute attribute in childNode.Attributes
                        where attribute.Name.Equals("value")
                        select attribute;

            foreach (XmlAttribute attribute in query)
                return attribute.Value;

            return string.Empty;
        }
    }
}
