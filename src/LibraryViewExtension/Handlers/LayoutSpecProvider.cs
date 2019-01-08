using System;
using System.IO;
using System.Reflection;
using CefSharp;
using Dynamo.Wpf.Interfaces;

namespace Dynamo.LibraryUI.Handlers
{
    /// <summary>
    /// Implements LayoutSepc resource provider, by default it reads the spec
    /// from a given json resource. It also allows to update certain specific
    /// sections from the layout spec for a given set of NodeSearchElements
    /// </summary>
    class LayoutSpecProvider : ResourceProviderBase
    {
        private Stream resourceStream;
        private readonly ILibraryViewCustomization customization;

        /// <summary>
        /// Creates the LayoutSpecProvider
        /// </summary>
        /// <param name="resource">The resource name of json resource in 
        /// the given assembly.</param>
        /// <param name="assembly">Assembly which contains the specified resource</param>
        public LayoutSpecProvider(ILibraryViewCustomization customization, string resource, Assembly assembly = null) : base(false)
        {
            assembly = assembly == null ? Assembly.GetExecutingAssembly() : assembly;
            var stream = assembly.GetManifestResourceStream(resource);

            //Get the spec from the stream
            var spec = LayoutSpecification.FromJSONStream(stream);
            customization.AddSections(spec.sections);
            this.customization = customization;
            this.customization.SpecificationUpdated += OnSpecificationUpdate;
        }

        private void OnSpecificationUpdate(object sender, EventArgs e)
        {
            DisposeResourceStream();
        }

        /// <summary>
        /// Gets the resource for the given request
        /// </summary>
        public override Stream GetResource(IRequest request, out string extension)
        {
            extension = "json";
            if (resourceStream == null)
            {
                resourceStream = customization.ToJSONStream();
            }

            return resourceStream;
        }

        private void DisposeResourceStream()
        {
            if (resourceStream != null)
            {
                resourceStream.Dispose();
                resourceStream = null;
            }
        }
    }
}
