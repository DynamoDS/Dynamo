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
    }

    /// <summary>
    /// Provides json resource data for all the loaded nodes
    /// </summary>
    class SearchResultDataProvider : NodeItemDataProvider
    {
        private const string serviceIdentifier = "search";
        private IEnumerable<NodeSearchElement> elements;

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
                .Select(e => CreateLoadedTypeItemExtended(e, w++) as LoadedTypeItemExtended).ToList();
            return data;
        }

        /// <summary>
        /// Creates LoadedTypeItem from given node search element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        private LoadedTypeItem CreateLoadedTypeItemExtended(NodeSearchElement element, int w = 0)
        {
            var item = new LoadedTypeItemExtended()
            {
                fullyQualifiedName = element.FullName,
                contextData = element.CreationName,
                iconUrl = new IconUrl(element.IconName, element.Assembly).Url,
                parameters = element.Parameters,
                itemType = element.Group.ToString().ToLower(),
                description = element.Description,
                keywords = element.SearchKeywords.Any()
                        ? element.SearchKeywords.Where(s => !string.IsNullOrEmpty(s)).Aggregate((x, y) => string.Format("{0}, {1}", x, y))
                        : string.Empty,
                weight = w
            };

            InitializeLoadedTypeItem(element, item);
            return item;
        }
    }
}
