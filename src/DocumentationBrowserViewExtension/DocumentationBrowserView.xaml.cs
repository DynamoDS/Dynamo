using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Utilities;
using Dynamo.ViewModels;
using Dynamo.Wpf.Extensions;

namespace Dynamo.DocumentationBrowser
{
    /// <summary>
    /// Interaction logic for WorkspaceDependencyView.xaml
    /// </summary>
    public partial class DocumentationBrowserView : Window, IDisposable
    {

        private WorkspaceModel currentWorkspace;

        /// <summary>
        /// The hyper link where Dynamo user will be forwarded to for submitting comments.
        /// </summary>
        private readonly string FeedbackLink = "https://forum.dynamobim.com/t/call-for-feedback-on-dynamo-graph-package-dependency-display/37229";

        private ViewLoadedParams loadedParams;
        private DocumentationBrowserViewExtension browserViewExtension;

        /// <summary>
        /// Re-directs to a web link to get the feedback from the user. 
        /// </summary>
        private void ProvideFeedback(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(FeedbackLink);
            }
            catch (Exception ex)
            {
                String message = Dynamo.DocumentationBrowser.Properties.Resources.ProvideFeedbackError + "\n\n" + ex.Message;
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Event handler for workspaceAdded event
        /// </summary>
        /// <param name="obj">workspace model</param>
        internal void OnWorkspaceChanged(IWorkspaceModel obj)
        {
            
        }

        /// <summary>
        /// Event handler for workspaceRemoved event
        /// </summary>
        /// <param name="obj">workspace model</param>
        internal void OnWorkspaceCleared(IWorkspaceModel obj)
        {

        }

        private void OnWorkspacePropertyChanged(object sender, PropertyChangedEventArgs args)
        {

        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="p">ViewLoadedParams</param>
        public DocumentationBrowserView(DocumentationBrowserViewExtension viewExtension, ViewLoadedParams p)
        {
            InitializeComponent();
            this.DataContext = this;
            currentWorkspace = p.CurrentWorkspaceModel as WorkspaceModel;
            p.CurrentWorkspaceChanged += OnWorkspaceChanged;
            p.CurrentWorkspaceCleared += OnWorkspaceCleared;
            currentWorkspace.PropertyChanged += OnWorkspacePropertyChanged;
            loadedParams = p;
            browserViewExtension = viewExtension;
        }



        /// <summary>
        /// Dispose function for WorkspaceDependencyView
        /// </summary>
        public void Dispose()
        {
            loadedParams.CurrentWorkspaceChanged -= OnWorkspaceChanged;
            loadedParams.CurrentWorkspaceCleared -= OnWorkspaceCleared;
        }
    }
}
