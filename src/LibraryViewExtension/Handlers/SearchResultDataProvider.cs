using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CefSharp;
using Dynamo.Search;
using Dynamo.Search.SearchElements;

namespace Dynamo.LibraryUI.Handlers
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

        public override Stream GetResource(IRequest request, out string extension)
        {
            var url = request.Url;
            var uri = new Uri(url);
            var pathAndQuery = uri.PathAndQuery;
            var index = url.IndexOf(serviceIdentifier);
            var text = url.Substring(index + serviceIdentifier.Length + 1);

            var elements = model.Search(text);

            extension = "json";
            return GetNodeItemDataStream(elements);
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
