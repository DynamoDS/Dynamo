using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.LibraryUI
{
    class LibraryViewCustomization : ILibraryViewCustomization
    {
        public const string DefaultSectionName = "default";
        private LayoutSpecification root = new LayoutSpecification();

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
            var text = DefaultSectionName;
            if (!string.IsNullOrEmpty(sectionText))
            {
                text = sectionText;
            }
            var spec = GetSpecification();
            var section = spec.sections.FirstOrDefault(x => string.Equals(x.text, text));
            if (section == null)
            {
                section = new LayoutSection(text);
                spec.sections.Add(section);
            }

            //obtain the list to update
            var list = getlist(section);
            list.AddRange(items); //add items to the list
            int count = list.Count;
            var groups = list.GroupBy(keyselector).ToList();
            if (groups.Count < count)
            {
                var duplicates = string.Join(", ", groups.Where(g => g.Count() > 1).Select(g => g.Key));
                throw new InvalidOperationException(string.Format("Duplicate entries found, ex: {0}", duplicates));
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
    }
}
