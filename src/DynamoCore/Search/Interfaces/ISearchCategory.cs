using System.Collections.Generic;

namespace Dynamo.Search.Interfaces
{
    /// <summary>
    ///     A search category.
    /// </summary>
    /// <typeparam name="TEntry"></typeparam>
    public interface ISearchCategory<out TEntry>
    {
        /// <summary>
        ///     The name of this category.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Entries contained in this category.
        /// </summary>
        IEnumerable<TEntry> Entries { get; }

        /// <summary>
        ///     Sub-categories contained in this category
        /// </summary>
        IEnumerable<ISearchCategory<TEntry>> SubCategories { get; }
    }
}