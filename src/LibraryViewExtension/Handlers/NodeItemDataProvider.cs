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
    class NodeItemDataProvider : IResourceProvider
    {
        private NodeSearchModel model;

        public static readonly string IconUrlServiceEndpoint = "http://54.169.171.233:3456/src/resources/icons/";

        public NodeItemDataProvider(NodeSearchModel model)
        {
            this.model = model;
        }

        public bool IsStaticResource { get { return false; } }

        public string Scheme { get { return "http"; } }

        public Stream GetResource(IRequest request, out string extension)
        {
            extension = "json";
            return GetNodeItemDataStream(model.SearchEntries);
        }

        private Stream GetNodeItemDataStream(IEnumerable<NodeSearchElement> searchEntries)
        {
            var data = new LoadedTypeData();
            data.loadedTypes = searchEntries
                //.Where(e => !e.ElementType.HasFlag(ElementTypes.Packaged))
                .Select(
                e => new LoadedTypeItem()
                {
                    fullyQualifiedName = GetFullyQualifiedName(e),
                    contextData = e.CreationName,
                    iconUrl = string.Format("{0}{1}.png", IconUrlServiceEndpoint, e.IconName),
                    itemType = e.Group.ToString().ToLower(),
                    keywords = e.SearchKeywords.Any() 
                        ? e.SearchKeywords.Where(s => !string.IsNullOrEmpty(s)).Aggregate((x, y) => string.Format("{0}, {1}", x, y)) 
                        : string.Empty
                }).ToList();

            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            var serializer = new JsonSerializer();
            serializer.Serialize(sw, data);

            sw.Flush();
            ms.Position = 0;
            return ms;
        }

        private string GetFullyQualifiedName(NodeSearchElement element)
        {
            var pkgprefix = element.ElementType.HasFlag(ElementTypes.Packaged) ? "pkg://" : "";
            return string.Format("{0}{1}", pkgprefix, element.FullName);
        }
    }
}
