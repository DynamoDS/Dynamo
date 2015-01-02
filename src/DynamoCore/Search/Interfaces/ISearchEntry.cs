using System.Collections.Generic;

namespace Dynamo.Search.Interfaces
{
    /// <summary>
    ///     Has a collection of strings that can be used to identifiy the instance
    ///     in a search.
    /// </summary>
    public interface ISearchEntry
    {
        /// <summary>
        ///     Name of this search entry.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Tags which can be used to search for this entry.
        /// </summary>
        ICollection<string> SearchTags { get; }

        /// <summary>
        ///     Description of this search entry.
        /// </summary>
        string Description { get; }
    }
}