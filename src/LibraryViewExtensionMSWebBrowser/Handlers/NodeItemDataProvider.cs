using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Search;
using Dynamo.Search.SearchElements;
using Dynamo.LibraryViewExtensionMSWebBrowser.Handlers;
using Newtonsoft.Json;

// TODO there are many clases in this file which are duplicates in LibraryViewExtension - 
// these can be refactored out into a shared core.

namespace Dynamo.LibraryViewExtensionMSWebBrowser
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
        public string description { get; set; }
        public string keywords { get; set; }
    }

    class LoadedTypeData<T> where T : LoadedTypeItem
    {
        public List<T> loadedTypes { get; set; }
    }

    /// <summary>
    /// Provides json resource data for all the loaded nodes
    /// </summary>
    class NodeItemDataProvider : ResourceProviderBase
    {
        protected NodeSearchModel model;
        private IconResourceProvider iconProvider;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="items"></param>
        public NodeItemDataProvider(NodeSearchModel model) : base(false)
        {
            this.model = model;
        }

        public NodeItemDataProvider(NodeSearchModel model, IconResourceProvider iconProvider)
        {
            this.model = model;
            this.iconProvider = iconProvider;
        }

        public override Stream GetResource(string url, out string extension)
        {
            extension = "json";
            //pass false to keep original icon urls
            return GetNodeItemDataStream(model.SearchEntries, false);
        }

        /// <summary>
        /// main nodeItem data lookup method.
        /// </summary>
        /// <param name="searchEntries"></param>
        /// <param name="replaceIconURLWithData">the option to replace the nodeItems iconUrl property with the base64Data 
        /// imagedata before it gets sent to the library UI. If this parameter is false, the urls will remain unchanged
        /// and the librayUI will need to resolve them.</param>
        /// <returns></returns>
        protected Stream GetNodeItemDataStream(IEnumerable<NodeSearchElement> searchEntries, bool replaceIconURLWithData)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            var serializer = new JsonSerializer();
            var stringBuilder = new StringBuilder();
            var data = CreateObjectForSerialization(searchEntries);

            if (replaceIconURLWithData)
            {
                var ext = string.Empty;
                IEnumerable<LoadedTypeItem> loadedTypes = new List<LoadedTypeItem>();
                if (data is LoadedTypeData<LoadedTypeItem>)
                {
                    loadedTypes = (data as LoadedTypeData<LoadedTypeItem>).loadedTypes;

                }
                else if (data is LoadedTypeData<LoadedTypeItemExtended>)
                {
                    loadedTypes = (data as LoadedTypeData<LoadedTypeItemExtended>).loadedTypes;
                }
                //lookup each loaded type and gets its icon, and embed that string in the type
                foreach (var item in loadedTypes)
                {
                    var iconAsBase64 = iconProvider.GetResourceAsString(item.iconUrl, out ext);
                    if (ext == "svg")
                    {
                        ext = "svg+xml";
                    }
                    stringBuilder.Append("data:image/");
                    stringBuilder.Append(ext);
                    stringBuilder.Append(";base64, ");
                    stringBuilder.Append(iconAsBase64);
                    item.iconUrl = stringBuilder.ToString();
                    stringBuilder.Clear();
                    //item.iconUrl = $"data:image/{ext};base64, {iconAsBase64}";
                }
            }
            serializer.Serialize(sw, data);

            sw.Flush();
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Create a LoadedTypeData object for serialization
        /// </summary>
        /// <param name="searchEntries"></param>
        /// <returns></returns>
        protected virtual object CreateObjectForSerialization(IEnumerable<NodeSearchElement> searchEntries)
        {
            var data = new LoadedTypeData<LoadedTypeItem>();

            // Converting searchEntries to another list so as to avoid modifying the actual searchEntries list when iterating through it. 
            data.loadedTypes = searchEntries.ToList().Select(e => CreateLoadedTypeItem<LoadedTypeItem>(e)).ToList();
            return data;
        }

        /// Gets fully qualified name for the given node search element
        /// </summary>
        public static string GetFullyQualifiedName(NodeSearchElement element)
        {
            //If the node search element is part of a package, then we need to prefix pkg:// for it
            if (element.ElementType.HasFlag(ElementTypes.Packaged))
            {
                //Use FullCategory and name as read from _customization.xml file
                return string.Format("{0}{1}.{2}", "pkg://", element.FullCategoryName, element.Name);
            }
            else if (element.ElementType.HasFlag(ElementTypes.CustomNode))
            {
                //Use FullCategory and name as read from _customization.xml file
                return string.Format("{0}{1}", "dyf://", element.FullName);
            }
            return element.FullName;
        }

        /// <summary>
        /// Creates LoadedTypeItem from given node search element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        internal T CreateLoadedTypeItem<T>(NodeSearchElement element) where T : LoadedTypeItem, new()
        {
            var item = new T()
            {
                fullyQualifiedName = GetFullyQualifiedName(element),
                contextData = element.CreationName,
                iconUrl = new IconUrl(element.IconName, element.Assembly).Url,
                parameters = element.Parameters,
                itemType = element.Group.ToString().ToLower(),
                description = element.Description,
                keywords = element.SearchKeywords.Any()
                        ? element.SearchKeywords.Where(s => !string.IsNullOrEmpty(s)).Aggregate((x, y) => string.Format("{0}, {1}", x, y))
                        : string.Empty
            };

            //If this element is not a custom node then we are done. The icon url for custom node is different
            if (!element.ElementType.HasFlag(ElementTypes.CustomNode)) return item;

            var customNode = element as CustomNodeSearchElement;
            if (customNode != null)
            {
                item.iconUrl = new IconUrl(customNode.IconName, customNode.Path, true).Url;
            }

            return item;
        }
    }
}
