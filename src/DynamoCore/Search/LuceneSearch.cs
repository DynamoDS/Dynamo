using System;
using Dynamo.Utilities;

namespace Dynamo.Search
{
    /// <summary>
    /// Singleton class that has access to all the lucene search utility classes.
    /// </summary>
    public sealed class LuceneSearch
    {
        /// <summary>
        /// Use Lazy&lt;LuceneSearch&gt; to make sure the Singleton class is only initialized once
        /// </summary>
        internal static readonly Lazy<LuceneSearch> lazy = new Lazy<LuceneSearch>(() => new LuceneSearch());

        #region Public members
        /// <summary>
        /// The actual instance stored in the Singleton class
        /// </summary>
        public static LuceneSearch Instance { get { return lazy.Value; } }
        #endregion

        /// <summary>
        /// Singleton class initialization logic which will be run in a lazy way the first time Dynamo try to access Lucene search utilities.
        /// </summary>
        private LuceneSearch()
        {
            
        }

        /// <summary>
        /// LuceneSearchUtility for nodes
        /// </summary>
        internal static LuceneSearchUtility LuceneSearchUtility { get; set; }

        /// <summary>
        /// LuceneSearchUtility for nodeaautocomplete.
        /// </summary>
        internal static LuceneSearchUtility LuceneUtilityNodeAutocomplete { get; set; }

        /// <summary>
        /// LuceneSearchUtility for packages in package manager.
        /// </summary>

        internal static LuceneSearchUtility LuceneUtilityPackageManager { get; set; }
    }
}
