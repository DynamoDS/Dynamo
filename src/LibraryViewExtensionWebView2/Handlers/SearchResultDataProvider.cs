using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Search;
using Dynamo.Search.SearchElements;

namespace Dynamo.LibraryViewExtensionWebView2.Handlers
{
    class LoadedTypeItemExtended : LoadedTypeItem
    {
        public int weight { get; set; }
        public LoadedTypeItemExtended setWeight(int w)
        {
            this.weight = w;
            return this;
        }
    }

    /// <summary>
    /// Provides json resource data for all the loaded nodes
    /// </summary>
    class SearchResultDataProvider : NodeItemDataProvider
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items"></param>
        public SearchResultDataProvider(NodeSearchModel model) : base(model)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items"></param>
        public SearchResultDataProvider(NodeSearchModel model, IconResourceProvider iconProvider) : base(model, iconProvider)
        {
        }

        /// <summary>
        /// Get the results of a search.
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public override Stream GetResource(string searchText, out string extension)
        {
            var text = Uri.UnescapeDataString(searchText);
            var elements = string.IsNullOrWhiteSpace(text) ? new List<NodeSearchElement>() : model.Search(text, LuceneSearch.LuceneUtilityNodeSearch);
            extension = "json";
            return GetNodeItemDataStream(elements, true);
        }

        /// <summary>
        /// Create a LoadedTypeData object for serialization
        /// </summary>
        /// <param name="entries"></param>
        /// <returns></returns>
        protected override object CreateObjectForSerialization(IEnumerable<NodeSearchElement> entries)
        {
            int w = 0; //represents the weight
            var data = new LoadedTypeData<LoadedTypeItemExtended>();
            data.loadedTypes = entries
                .Select(e => CreateLoadedTypeItem<LoadedTypeItemExtended>(e).setWeight(w++)).ToList();
            return data;
        }
    }
}
