﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Wpf.Interfaces;
using Dynamo.LibraryViewExtensionWebView2.Handlers;
using Newtonsoft.Json;
using Dynamo.Interfaces;

namespace Dynamo.LibraryViewExtensionWebView2
{
    class LibraryViewCustomization : ILibraryViewCustomization, IDisposable
    {
        public const string DefaultSectionName = "default";
        private LayoutSpecification root = new LayoutSpecification();
        private Dictionary<string, Stream> resources = new Dictionary<string, Stream>();

        /// <summary>
        /// Returns a list of key value pair of all the registered resources
        /// </summary>
        public IEnumerable<KeyValuePair<string, Stream>> Resources { get { return resources; } }

        private void RaiseSepcificationUpdated()
        {
            var handler = SpecificationUpdated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// This event is raised when the specification is updated
        /// </summary>
        public event EventHandler SpecificationUpdated;

        /// <summary>
        /// Allows clients to add a new section to the layout specification. This
        /// operation will fail if the current specification already contains a 
        /// section with the same text as of the given section. The clients must
        /// call AddElements method to add layout elements to a given section.
        /// </summary>
        public bool AddSections(IEnumerable<LayoutSection> sections)
        {
            var spec = GetSpecification();
            spec.sections.AddRange(sections);
            var count = spec.sections.Count;
            if (spec.sections.GroupBy(s => s.text).Count() < count) return false; //some duplicate sections exist

            //Update the specification
            SetSpecification(spec);
            return true;
        }

        /// <summary>
        /// Allows clients to add a list of elements to a given section. This 
        /// operation will fail if the text property of any of the child elements 
        /// of the given section matches with the text property of the given 
        /// elements.
        /// </summary>
        public bool AddElements(IEnumerable<LayoutElement> elements, string sectionText = "")
        {
            return UpdateSpecification(elements, s => s.childElements, e => e.text, sectionText);
        }

        /// <summary>
        /// Allows clients to add a list of include path to a given section. This 
        /// operation will fail if the path property of any of the includes 
        /// of the given section conflicts with the path property of the given 
        /// includes.
        /// </summary>
        public bool AddIncludeInfo(IEnumerable<LayoutIncludeInfo> includes, string sectionText = "")
        {
            return UpdateSpecification(includes, s => s.include, info => info.path, sectionText);
        }

        /// <summary>
        /// Updates the specification with given list of items.
        /// </summary>
        /// <typeparam name="T">Type of items LayoutElement or LayoutIncludeInfo</typeparam>
        /// <param name="items">List of items to add</param>
        /// <param name="getlist">Function to get the list of a given section to update</param>
        /// <param name="keyselector">Function to select key for the given item to identify it uniquely.</param>
        /// <param name="sectionText">Name of the section where the given items will be added</param>
        /// <returns></returns>
        private bool UpdateSpecification<T>(IEnumerable<T> items, Func<LayoutSection, List<T>> getlist, Func<T, string> keyselector, string sectionText)
        {
            var sectionName = DefaultSectionName;
            if (!string.IsNullOrEmpty(sectionText))
            {
                sectionName = sectionText;
            }
            var spec = GetSpecification();
            var section = spec.sections.FirstOrDefault(x => string.Equals(x.text, sectionName));
            if (section == null)
            {
                section = new LayoutSection(sectionName);
                spec.sections.Add(section);
            }

            //obtain the list to update, but make a copy before updating
            var listToUpdate = getlist(section);
            var listCopy = listToUpdate.ToList();
            listCopy.AddRange(items); //add items to the clone list
            //check for dupe categories
            int count = listCopy.Count;
            var groups = listCopy.GroupBy(keyselector).ToList();
            //if there are duplicates we fallback to trying to perform a merge.
            if (groups.Count < count)
            {
                var duplicateNames = groups.Where(g => g.Count() > 1).Select(g=>g.Key);
                var duplicateString = string.Join(", ", duplicateNames);
                Console.WriteLine($"Duplicate entries found, ex: {duplicateString}, attempting to merge");

                //create a fake layout spec containing the items and merge them in.
                //merging should deal with duplicates automatically.
                var newLayoutSpec = new LayoutSpecification();
                var newSection = new LayoutSection(sectionName);
                newLayoutSpec.sections.Add(newSection);
                newSection.childElements.AddRange(items.Cast<LayoutElement>());
                MergeSpecification(newLayoutSpec);
                return true;
            }
            else
            {
                listToUpdate.AddRange(items);
            }

            //Update the specification
            SetSpecification(spec);
            return true;
        }

        /// <summary>
        /// Gets a copy of the current library layout specification
        /// </summary>
        public LayoutSpecification GetSpecification()
        {
            return root.Clone(); //returns clone
        }

        /// <summary>
        /// Sets the given specification as the current library layout 
        /// specification by overwriting the current one.
        /// </summary>
        public void SetSpecification(LayoutSpecification specification)
        {
            root = specification;
            RaiseSepcificationUpdated();
        }

        /// <summary>
        /// Serializes the current specification to JSON stream
        /// </summary>
        public Stream ToJSONStream()
        {
            return root.ToJSONStream();
        }

        /// <summary>
        /// Serializes the current specification to JSON stream and optionally replaces icon urls with images as base64 data
        /// </summary>
        public Stream ToJSONStream(bool replaceIconURLWithData, IconResourceProvider iconResourceProvider = null)
        {
            return ToJSONStream(root, replaceIconURLWithData, iconResourceProvider);
        }

        /// <summary>
        /// Merges the passed spec with the current spec and sets the result as the current library layout.
        /// </summary>
        /// <param name="specToMerge"></param>
        internal void MergeSpecification(LayoutSpecification specToMerge)
        {
            var result = LayoutSpecification.MergeLayoutSpecs(GetSpecification(), specToMerge);
            SetSpecification(result);
        }


        /// <summary>
        /// Notifies when a resource stream is registered. This event is
        /// used by ResourceHandlerFactory to register the resource handler
        /// for provided url path.
        /// </summary>
        public event Action<string, Stream> ResourceStreamRegistered;

        /// <summary>
        /// Registers a given resource stream for a given url. If registered 
        /// successfully the requested resource is returned from the given stream.
        /// </summary>
        /// <param name="urlpath">relative path of url including 
        /// extension of the resource, e.g. /myicons/xicon.png</param>
        /// <param name="resource">resource data stream</param>
        /// <returns>True if the operation was successful</returns>
        public bool RegisterResourceStream(string urlpath, Stream resource)
        {
            var extension = Path.GetExtension(urlpath);
            if (string.IsNullOrEmpty(extension)) return false;

            resources.Add(urlpath, resource);

            var handler = ResourceStreamRegistered;
            if (handler != null)
            {
                handler(urlpath, resource);
            }
            return true;
        }

        /// <summary>
        /// To be called by host application to request CEF shutdown
        /// </summary>
        public void OnAppShutdown()
        {

        }

        /// <summary>
        /// IDisposable implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var item in resources)
                {
                    item.Value.Dispose();
                }
                resources.Clear();
            }
        }

        //layoutSpec helpers
        /// <summary>
        /// Serializes the layout specification to json stream and
        /// optionally replaces icon urls with image data as base64 encoded strings
        /// </summary>
        /// <returns></returns>
        private Stream ToJSONStream(LayoutSpecification spec, bool replaceIconURLWithData, IconResourceProvider iconProvider = null)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            if (replaceIconURLWithData && iconProvider != null)
            {
                //foreach section update all nested children and nested includes, and root iconurls to image data.
                spec.sections.ForEach(section => {
                    section.iconUrl = getBase64ImageString(section, iconProvider);
                    section.EnumerateChildren().ToList().ForEach(child => child.iconUrl = getBase64ImageString(child, iconProvider));
                    section.EnumerateIncludes().ToList().ForEach(child => child.iconUrl = getBase64ImageString(child, iconProvider));
                });
            }

            var serializer = new JsonSerializer() { Formatting = Formatting.Indented, NullValueHandling = NullValueHandling.Ignore };
            serializer.Serialize(sw, spec);
            
            sw.Flush();
            ms.Position = 0;
            return ms;
        }

        private string getBase64ImageString(LayoutElement item, IconResourceProvider iconProvider)
        {
            var ext = string.Empty;
            var iconAsBase64 = iconProvider.GetResourceAsString(item.iconUrl, out ext);
            if (string.IsNullOrEmpty(iconAsBase64))
            {
                return "";
            }
            if (ext.Contains("svg"))
            {
                ext = "svg+xml";
            }
            return $"data:image/{ext};base64, {iconAsBase64}";
        }
        private string getBase64ImageString(LayoutIncludeInfo item, IconResourceProvider iconProvider)
        {
            var ext = string.Empty;
            var iconAsBase64 = iconProvider.GetResourceAsString(item.iconUrl, out ext);
            if (string.IsNullOrEmpty(iconAsBase64))
            {
                return "";
            }
            if (ext.Contains("svg"))
            {
                ext = "svg+xml";
            }
            return $"data:image/{ext};base64, {iconAsBase64}";
        }
    }
}
