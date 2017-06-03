using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CefSharp;
using Newtonsoft.Json;

namespace Dynamo.LibraryUI.Handlers
{
    class LayoutDataTypePath
    {
        public string path { get; set; }
        public string iconUrl { get; set; }
    }

    class LayoutElement
    {
        public string text { get; set; }
        public string iconUrl { get; set; }
        public string elementType { get; set; }
        public List<LayoutDataTypePath> include { get; set; }
        public List<LayoutElement> childElements { get; set; }
    }

    class LayoutSection : LayoutElement
    {
        public string showHeader { get; set; }
    }

    class LayoutSpec
    {
        public List<LayoutSection> sections { get; set; }
    }

    class LayoutSpecProvider : ResourceProviderBase
    {
        private const string resource = "Dynamo.LibraryUI.web.library.layoutSpecs.json";
        private LayoutSpec spec = null;
        private Stream resourceStream = null;

        public LayoutSpecProvider() : base(false)
        {
            InitializeSpec();
        }

        private void InitializeSpec()
        {
            var assembly = Assembly.GetExecutingAssembly();
            resourceStream = assembly.GetManifestResourceStream(resource);
            var sr = new StreamReader(resourceStream);
            var serializer = new JsonSerializer() { NullValueHandling = NullValueHandling.Ignore };
            spec = (LayoutSpec)serializer.Deserialize(sr, typeof(LayoutSpec));
        }

        public void UpdateAddOnsSpec(string nodes)
        {
            if (string.IsNullOrEmpty(nodes)) return;

            var categories = nodes.Split(',')
                .Select(x => { var id = x.IndexOf('.'); return x.Substring(0, id); })
                .Distinct()
                .SkipWhile(x => x.StartsWith("pkg://"));

            foreach (var item in spec.sections)
            {
                if (!item.text.Equals("Add-ons")) continue;

                item.include.AddRange(categories.Select(c => new LayoutDataTypePath() { path = c }));
            }

            //Reset the resource stream, so that the next query will serailize updated spec.
            resourceStream.Dispose();
            resourceStream = null;
        }

        public override Stream GetResource(IRequest request, out string extension)
        {
            extension = "json";
            if (resourceStream != null) return resourceStream;

            resourceStream = new MemoryStream();
            var sw = new StreamWriter(resourceStream);
            var serializer = new JsonSerializer();
            serializer.Serialize(sw, spec);

            sw.Flush();
            resourceStream.Position = 0;
            return resourceStream;
        }
    }
}
