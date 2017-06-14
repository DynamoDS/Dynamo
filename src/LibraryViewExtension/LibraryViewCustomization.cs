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
        /// Gets a copy of the current library layout specification
        /// </summary>
        public LayoutSpecification Specification
        {
            get
            {
                return root.Clone();
            }
            private set
            {
                root = value;
                RaiseSepcificationUpdated();
            }
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
            var spec = Specification;
            spec.sections.AddRange(sections);
            var count = spec.sections.Count;
            if (spec.sections.GroupBy(s => s.text).Count() < count) return false; //some duplicate sections exist
            
            //Update the specification
            Specification = spec;
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
            var text = DefaultSectionName;
            if (!string.IsNullOrEmpty(sectionText))
            {
                text = sectionText;
            }
            var spec = Specification;
            var section = spec.sections.FirstOrDefault(x => string.Equals(x.text, text));
            if (section == null)
            {
                section = new LayoutSection(text);
                spec.sections.Add(section);
            }
            section.childElements.AddRange(elements);
            int count = section.childElements.Count;
            if (section.childElements.GroupBy(e => e.text).Count() < count) return false;

            //Update the specification
            Specification = spec;
            return true;
        }

        /// <summary>
        /// Allows clients to add a list of include path to a given section. This 
        /// operation will fail if the path property of any of the includes 
        /// of the given section conflicts with the path property of the given 
        /// includes.
        /// </summary>
        public bool AddIncludeInfo(IEnumerable<LayoutIncludeInfo> includes, string sectionText = "")
        {
            var text = DefaultSectionName;
            if (!string.IsNullOrEmpty(sectionText))
            {
                text = sectionText;
            }
            var spec = Specification;
            var section = spec.sections.FirstOrDefault(x => string.Equals(x.text, text));
            if (section == null)
            {
                section = new LayoutSection(text);
                spec.sections.Add(section);
            }
            section.include.AddRange(includes);
            int count = section.include.Count;
            if (section.include.GroupBy(e => e.path).Count() < count) return false;

            //Update the specification
            Specification = spec;
            return true;
        }

        /// <summary>
        /// Gets a copy of the current library layout specification
        /// </summary>
        public LayoutSpecification GetSpecification()
        {
            return Specification; //returns clone
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
