using Dynamo.Extensions;
using Dynamo.Wpf.Extensions;
using System.Windows.Controls;

namespace Dynamo.PackageDependency
{
    /// <summary>
    /// The Extension framework for Dynamo allows you to extend
    /// Dynamo by loading your own classes that can interact with Dynamo's API.
    /// An Extension has two components, an assembly containing your class and
    /// an xml manifest file telling Dynamo where to find your assembly. Extension
    /// manifests are loaded from the Dynamo/Extensions folder or from package/extra
    /// folders.
    /// 
    /// This sample demonstrates a simple IExtension which tracks nodes added to the workspace.
    /// </summary>
    public class PackageDependencyViewExtension : IViewExtension
    {
        /// <summary>
        /// Extension Name
        /// </summary>
        public string Name
        {
            get
            {
                return "Package Dependency ViewExtension";
            }
        }

        /// <summary>
        /// GUID of the extension
        /// </summary>
        public string UniqueId
        {
            get
            {
                return "A6706BF5-11C2-458F-B7C8-B745A77EF7FD";
            }
        }

        private PackageDependencyView DependencyView
        {
            get;
            set;
        }

        private ReadyParams ReadyParams;

        /// <summary>
        /// Dispose function after extension is closed
        /// </summary>
        public void Dispose()
        {

        }

        /// <summary>
        /// Ready is called when the DynamoModel is finished being built, or when the extension is installed
        /// sometime after the DynamoModel is already built. ReadyParams provide access to references like the
        /// CurrentWorkspace.
        /// </summary>
        /// <param name="sp"></param>
        public void Ready(ReadyParams readyParams)
        {
            ReadyParams = readyParams;
        }

        public void Shutdown()
        {
            ReadyParams.CurrentWorkspaceChanged -= DependencyView.OnWorkspaceChanged;
            ReadyParams.CurrentWorkspaceCleared -= DependencyView.OnWorkspaceCleared;
            this.Dispose();
        }

        public void Startup(ViewStartupParams viewLoadedParams)
        {

        }

        public void Loaded(ViewLoadedParams viewLoadedParams)
        {
            DependencyView = new PackageDependencyView(viewLoadedParams);

            // Replace with extension view inject API
            var sidebarGrid = viewLoadedParams.DynamoWindow.FindName("sidebarExtensionsGrid") as Grid;
            sidebarGrid.Children.Add(DependencyView);
        }
    }
}
