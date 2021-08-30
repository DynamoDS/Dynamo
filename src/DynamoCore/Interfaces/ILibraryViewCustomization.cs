using System;
using System.Collections.Generic;
using System.IO;

namespace Dynamo.Wpf.Interfaces
{
    /// <summary>
    /// Provides methods to customize library view
    /// </summary>
    public interface ILibraryViewCustomization
    {
        /// <summary>
        /// This event is raised when the specification is updated
        /// </summary>
        event EventHandler SpecificationUpdated;

        /// <summary>
        /// Gets a copy of the current library layout specification
        /// </summary>
        LayoutSpecification GetSpecification();

        /// <summary>
        /// Sets the given specification as the current library layout 
        /// specification by overwriting the current one.
        /// </summary>
        /// <param name="specification">New layout specification</param>
        void SetSpecification(LayoutSpecification specification);

        /// <summary>
        /// Serializes the current specification to JSON stream
        /// </summary>
        Stream ToJSONStream();

        /// <summary>
        /// Allows clients to add a new section to the layout specification. This
        /// operation will fail if the current specification already contains a 
        /// section with the same text as of the given section. The clients must
        /// call AddElements method to add layout elements to a given section.
        /// </summary>
        /// <param name="sections">A list of new sections.</param>
        /// <returns>True if the operation was successful</returns>
        bool AddSections(IEnumerable<LayoutSection> sections);

        /// <summary>
        /// Allows clients to add a list of elements to a given section. This 
        /// operation will fail if the text property of any of the child elements 
        /// of the given section matches with the text property of the given 
        /// elements.
        /// </summary>
        /// <param name="elements">List of layout elements to add</param>
        /// <param name="sectionText">The name of the section to be updated</param>
        /// <returns>True if the operation was successful</returns>
        bool AddElements(IEnumerable<LayoutElement> elements, string sectionText = "");

        /// <summary>
        /// Allows clients to add a list of layout include info to a given section. This 
        /// operation will fail if the path property of any of the includes 
        /// of the given section conflicts with the path property of the given 
        /// includes.
        /// </summary>
        /// <param name="includes">List of layout include info to add</param>
        /// <param name="sectionText">The name of the section to be updated</param>
        /// <returns>True if the operation was successful</returns>
        bool AddIncludeInfo(IEnumerable<LayoutIncludeInfo> includes, string sectionText = "");

        /// <summary>
        /// Registers a given resource stream for a given url. If registered 
        /// successfully the requested resource is returned from the given stream.
        /// </summary>
        /// <param name="urlpath">relative path of url including 
        /// extension of the resource, e.g. /myicons/xicon.png</param>
        /// <param name="resource">resource data stream</param>
        /// <returns>True if the operation was successful</returns>
        bool RegisterResourceStream(string urlpath, Stream resource);

        /// <summary>
        /// This method can be called by host application to notify the aplication
        /// shutdown so that the customization service can do the cleanup of resources.
        /// </summary>
        void OnAppShutdown();
    }
}
