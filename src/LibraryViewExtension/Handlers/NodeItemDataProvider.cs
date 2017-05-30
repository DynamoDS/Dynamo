using System.Collections.Generic;
using System.IO;
using System.Linq;
using CefSharp;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Newtonsoft.Json;

namespace Dynamo.LibraryUI.Handlers
{
    class NodeData
    {
        public string creationName { get; set; }
        public string module { get; set; }
    }

    class LoadedTypeItem
    {
        public string fullyQualifiedName { get; set; }
        public string iconUrl { get; set; }
        public string contextData { get; set; }
        public string parameters { get; set; }
        public string itemType { get; set; }
        public string keywords { get; set; }
    }

    class LoadedTypeItemExtended : LoadedTypeItem
    {
        public int weight { get; set; }
    }

    class LoadedTypeData<T>
    {
        public List<T> loadedTypes { get; set; }
    }

    /// <summary>
    /// Provides json resource data for all the loaded nodes
    /// </summary>
    class NodeItemDataProvider : ResourceProviderBase
    {
        private IEnumerable<NodeSearchElement> elements;
        private bool isElementWeighted = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items"></param>
        /// <param name="weighted">This means the items are associated with weights based on the order</param>
        public NodeItemDataProvider(IEnumerable<NodeSearchElement> items, bool weighted = false) : base(false)
        {
            this.elements = items;
            this.isElementWeighted = weighted;
        }

        public override Stream GetResource(IRequest request, out string extension)
        {
            extension = "json";
            return GetNodeItemDataStream(elements);
        }

        private Stream GetNodeItemDataStream(IEnumerable<NodeSearchElement> searchEntries)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            var serializer = new JsonSerializer();

            if (isElementWeighted)
            {
                int w = 0; //represents the weight
                var data = new LoadedTypeData<LoadedTypeItemExtended>();
                data.loadedTypes = searchEntries
                    .Select(e => CreateLoadedTypeItem(e, w++) as LoadedTypeItemExtended).ToList();
                serializer.Serialize(sw, data);
            }
            else
            {
                var data = new LoadedTypeData<LoadedTypeItem>();
                data.loadedTypes = searchEntries
                    //.Where(e => !e.ElementType.HasFlag(ElementTypes.Packaged))
                    .Select(e => CreateLoadedTypeItem(e)).ToList();
                serializer.Serialize(sw, data);
            }

            sw.Flush();
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Creates LoadedTypeItem from given node search element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        internal LoadedTypeItem CreateLoadedTypeItem(NodeSearchElement element, int w = 0)
        {
            //Create LoadedTypeItem with base class
            LoadedTypeItem item = null;
            if (isElementWeighted)
            {
                item = new LoadedTypeItemExtended()
                {
                    fullyQualifiedName = element.FullName,
                    contextData = element.CreationName,
                    iconUrl = new IconUrl(element.IconName, element.Assembly).Url,
                    parameters = element.Parameters,
                    itemType = element.Group.ToString().ToLower(),
                    keywords = element.SearchKeywords.Any()
                            ? element.SearchKeywords.Where(s => !string.IsNullOrEmpty(s)).Aggregate((x, y) => string.Format("{0}, {1}", x, y))
                            : string.Empty,
                    weight = w
                };
            }
            else
            {
                item = new LoadedTypeItem()
                {
                    fullyQualifiedName = element.FullName,
                    contextData = element.CreationName,
                    iconUrl = new IconUrl(element.IconName, element.Assembly).Url,
                    parameters = element.Parameters,
                    itemType = element.Group.ToString().ToLower(),
                    keywords = element.SearchKeywords.Any()
                            ? element.SearchKeywords.Where(s => !string.IsNullOrEmpty(s)).Aggregate((x, y) => string.Format("{0}, {1}", x, y))
                            : string.Empty
                };
            }

            //If the node search element is part of a package, then we need to prefix pkg:// for it
            var packaged = element.ElementType.HasFlag(ElementTypes.Packaged);
            if (packaged)
            {
                //Use FullCategory and name as read from _customization.xml file
                item.fullyQualifiedName = string.Format("{0}{1}.{2}", "pkg://", element.FullCategoryName, element.Name);
            }

            //If this element is not a custom node then we are done. The icon url for custom node is different
            if (!element.ElementType.HasFlag(ElementTypes.CustomNode)) return item;

            var customNode = element as CustomNodeSearchElement;
            if (customNode == null) return item;

            item.iconUrl = new IconUrl(customNode.IconName, customNode.Path, true).Url;
            return item;
        }
    }
}
