using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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

    class LoadedTypeData
    {
        public List<LoadedTypeItem> loadedTypes { get; set; }
    }

    /// <summary>
    /// Provides json resource data for all the loaded nodes
    /// </summary>
    class NodeItemDataProvider : ResourceProviderBase
    {
        private NodeSearchModel model;
        private IEventController controller;
        private Timer throttle;
        private long duetime;
        private List<string> items = new List<string>();

        public NodeItemDataProvider(NodeSearchModel model, IEventController controller = null, long dueTime = 200) : base(false)
        {
            this.model = model;
            if (model == null) throw new ArgumentNullException("model");

            if(controller != null)
            {
                model.EntryAdded += OnLibraryDataUpdated;
                model.EntryRemoved += OnLibraryDataUpdated;
                model.EntryUpdated += OnLibraryDataUpdated;

                this.controller = controller;
                this.duetime = dueTime;
                throttle = new Timer(RaiseLibraryDataUpdated, controller, Timeout.Infinite, Timeout.Infinite); //disabled at begining
            }
        }

        private void OnLibraryDataUpdated(NodeSearchElement obj)
        {
            items.Add(FullyQualifiedName(obj));
            //Raise event only after due milliseconds.
            throttle.Change(duetime, 0); //enabled now
        }

        private void RaiseLibraryDataUpdated(object state)
        {
            if(controller != null)
            {
                var text = string.Join(", ", items);
                controller.RaiseEvent("libraryDataUpdated", text);
                //reset items
                items.Clear();
            }
        }

        public override Stream GetResource(IRequest request, out string extension)
        {
            extension = "json";
            return GetNodeItemDataStream(model.SearchEntries);
        }

        private Stream GetNodeItemDataStream(IEnumerable<NodeSearchElement> searchEntries)
        {
            var data = new LoadedTypeData();
            data.loadedTypes = searchEntries
                //.Where(e => !e.ElementType.HasFlag(ElementTypes.Packaged))
                .Select(e => CreateLoadedTypeItem(e)).ToList();

            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            var serializer = new JsonSerializer();
            serializer.Serialize(sw, data);

            sw.Flush();
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Gets fully qualified name for the given node search element
        /// </summary>
        private static string FullyQualifiedName(NodeSearchElement element)
        {
            //If the node search element is part of a package, then we need to prefix pkg:// for it
            if (element.ElementType.HasFlag(ElementTypes.Packaged))
            {
                //Use FullCategory and name as read from _customization.xml file
                return string.Format("{0}{1}.{2}", "pkg://", element.FullCategoryName, element.Name);
            }
            return element.FullName;
        }

        /// <summary>
        /// Creates LoadedTypeItem from given node search element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        internal LoadedTypeItem CreateLoadedTypeItem(NodeSearchElement element)
        {
            //Create LoadedTypeItem with base class
            var item = new LoadedTypeItem()
            {
                fullyQualifiedName = FullyQualifiedName(element),
                contextData = element.CreationName,
                iconUrl = new IconUrl(element.IconName, element.Assembly).Url,
                parameters = element.Parameters,
                itemType = element.Group.ToString().ToLower(),
                keywords = element.SearchKeywords.Any()
                        ? element.SearchKeywords.Where(s => !string.IsNullOrEmpty(s)).Aggregate((x, y) => string.Format("{0}, {1}", x, y))
                        : string.Empty
            };

            //If this element is not a custom node then we are done. The icon url for custom node is different
            if (!element.ElementType.HasFlag(ElementTypes.CustomNode)) return item;

            var customNode = element as CustomNodeSearchElement;
            if (customNode == null) return item;

            item.iconUrl = new IconUrl(customNode.IconName, customNode.Path, true).Url;
            return item;
        }
    }
}
