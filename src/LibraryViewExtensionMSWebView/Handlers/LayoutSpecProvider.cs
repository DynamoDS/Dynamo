using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Wpf.Interfaces;

namespace LibraryViewExtensionMSWebView.Handlers
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
        private readonly IconResourceProvider iconProvider;

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

        public LayoutSpecProvider(ILibraryViewCustomization customization, IconResourceProvider iconProvider, string resource, Assembly assembly = null) :
            this(customization, resource, assembly)
        {
            this.iconProvider = iconProvider;
        }

        private void OnSpecificationUpdate(object sender, EventArgs e)
        {
            DisposeResourceStream();
        }

        /// <summary>
        /// Gets the resource for the given request
        /// </summary>
        public override Stream GetResource(string url, out string extension)
        {
            extension = "json";
            if (resourceStream == null)
            {
                resourceStream = (customization as LibraryViewCustomization).ToJSONStream(true, iconProvider);
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
