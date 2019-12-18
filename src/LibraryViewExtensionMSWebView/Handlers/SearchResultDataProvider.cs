using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Search;

namespace LibraryViewExtensionMSWebView.Handlers
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
        public const string serviceIdentifier = "/nodeSearch";

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
        public Stream GetResource(string searchText, out string extension)
        {

            var text = Uri.UnescapeDataString(searchText);
            var elements = model.Search(text);

            extension = "json";
            return GetNodeItemDataStream(elements, true);
        }

        /// <summary>
        /// Create a LoadedTypeData object for serialization
        /// </summary>
        /// <param name="searchEntries"></param>
        /// <returns></returns>
        protected override object CreateObjectForSerialization(IEnumerable<NodeSearchElement> searchEntries)
        {
            int w = 0; //represents the weight
            var data = new LoadedTypeData<LoadedTypeItemExtended>();
            data.loadedTypes = searchEntries
                .Select(e => CreateLoadedTypeItem<LoadedTypeItemExtended>(e).setWeight(w++)).ToList();
            return data;
        }
    }
}
