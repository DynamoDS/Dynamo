using System;
using Dynamo.Models;
using Dynamo.PackageManager;

namespace Dynamo.ViewModels
{
    partial class DynamoViewModel
    {
        [Obsolete("This event will be removed, do not use. It does nothing.")]
        public event EventHandler RequestManagePackagesDialog;
        [Obsolete("This method will be removed do not use. It does nothing.")]
        public virtual void OnRequestManagePackagesDialog(Object sender, EventArgs e)
        {
            if (RequestManagePackagesDialog != null)
            {
                RequestManagePackagesDialog(this, e);
            }
        }

        public event RequestPackagePublishDialogHandler RequestPackagePublishDialog;
        public void OnRequestPackagePublishDialog(PublishPackageViewModel vm)
        {
            if (RequestPackagePublishDialog != null)
                RequestPackagePublishDialog(vm);
        }

        public event EventHandler RequestPackageManagerSearchDialog;
        public virtual void OnRequestPackageManagerSearchDialog(Object sender, EventArgs e)
        {
            if (RequestPackageManagerSearchDialog != null)
            {
                RequestPackageManagerSearchDialog(this, e);
            }
        }
        [Obsolete("This event will be removed, do not use. It does nothing.")]
        public event EventHandler RequestPackagePathsDialog;
        [Obsolete("This method will be removed do not use. It does nothing.")]
        public virtual void OnRequestPackagePathsDialog(object sender, EventArgs e)
        {
            var handler = RequestPackagePathsDialog;
            if (handler != null)
                handler(sender, e);
        }

        public event ImageSaveEventHandler RequestSaveImage;
        public virtual void OnRequestSaveImage(Object sender, ImageSaveEventArgs e)
        {
            if (RequestSaveImage != null)
            {
                RequestSaveImage(this, e);
            }
        }

        public event ImageSaveEventHandler RequestSave3DImage;

        public virtual void OnRequestSave3DImage(object sender, ImageSaveEventArgs e)
        {
            if (RequestSave3DImage != null)
            {
                RequestSave3DImage(this, e);
            }
        }
        [Obsolete("This event will be removed later, now the Scaling Factor functionality is implemented in PreferencesViewModel.cs")]
        public event EventHandler RequestScaleFactorDialog;

        [Obsolete("This method will be removed later, now the Scaling Factor functionality is implemented in PreferencesViewModel.cs")]
        public virtual void OnRequestScaleFactorDialog(object sender, EventArgs e)
        {
            var handler = RequestScaleFactorDialog;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        public event EventHandler RequestClose;
        public virtual void OnRequestClose(Object sender, EventArgs e)
        {
            if (RequestClose != null)
            {
                RequestClose(this, e);
            }
        }

        public event EventHandler SidebarClosed;
        public virtual void OnSidebarClosed(Object sender, EventArgs e)
        {
            if (SidebarClosed != null)
            {
                SidebarClosed(this, e);
            }
        }

        public event WorkspaceSaveEventHandler RequestUserSaveWorkflow;
        public virtual void OnRequestUserSaveWorkflow(Object sender, WorkspaceSaveEventArgs e)
        {
            if (RequestUserSaveWorkflow != null)
            {
                RequestUserSaveWorkflow(this, e);
            }
        }

        public event RequestAboutWindowHandler RequestAboutWindow;
        public virtual void OnRequestAboutWindow(DynamoViewModel vm)
        {
            if (RequestAboutWindow != null)
            {
                RequestAboutWindow(vm);
            }
        }

        public event RequestOpenDocumentationLinkHandler RequestOpenDocumentationLink;
        public virtual void OnRequestOpenDocumentationLink(OpenDocumentationLinkEventArgs e)
        {
            // let any registered documentation link handlers process the request
            // if no handlers are registered, we silently ignore the event
            // note we don't fall back to handling the link with the default OS handler due to security concerns
            RequestOpenDocumentationLink?.Invoke(e);
        }

        public event Action RequestShowHideSidebar;
        public virtual void OnShowHideSidebar(bool show)
        {
            if (RequestShowHideSidebar != null)
                RequestShowHideSidebar();
        }

        internal event RequestViewOperationHandler RequestViewOperation;
        private void OnRequestViewOperation(ViewOperationEventArgs e)
        {
            if (RequestViewOperation != null)
            {
                RequestViewOperation(e);
            }
        }

        internal event Action RequestPresetsWarningPrompt;
        private void OnRequestPresetWarningPrompt()
        {
            if (RequestPresetsWarningPrompt != null)
                RequestPresetsWarningPrompt();
        }

        internal event Action RequestPaste;
        private void OnRequestPaste()
        {
            if (RequestPaste != null)
            {
                RequestPaste();
            }
        }

        internal event Action RequestReturnFocusToView;
        internal void OnRequestReturnFocusToView()
        {
            if (RequestReturnFocusToView != null)
                RequestReturnFocusToView();
        }

        public event Action<bool> RequestEnableShortcutBarItems;
        public virtual void OnEnableShortcutBarItems(bool enable)
        {
            if (RequestEnableShortcutBarItems != null)
                RequestEnableShortcutBarItems(enable);
        }

        /// <summary>
        /// Event used to verify that the correct dialog is being showed when saving a graph with unresolved linter issues.
        /// This is only meant to be used for unit testing purposes.
        /// As the GenericTaskDialog is not owned by the DynamoWindow we need another way to verify that it shows up
        /// when doing unit tests.
        /// </summary>
        internal event Action<SaveWarningOnUnresolvedIssuesArgs> SaveWarningOnUnresolvedIssuesShows;
        internal void OnSaveWarningOnUnresolvedIssuesShows(SaveWarningOnUnresolvedIssuesArgs e)
        {
            SaveWarningOnUnresolvedIssuesShows?.Invoke(e);
        }

        /// <summary>
        /// Event raised when there's a request to open the view extension in the side panel.
        /// </summary>
        internal event Action<string> ViewExtensionOpenRequest;
        internal void OnViewExtensionOpenRequest(string extensionName)
        {
            ViewExtensionOpenRequest?.Invoke(extensionName);
        }

        /// <summary>
        /// Event raised when there's a request to open the view extension in the side panel.
        /// </summary>
        internal event Action<string, object> ViewExtensionOpenWithParameterRequest;
        internal void OnViewExtensionOpenWithParameterRequest(string extensionIdentification, object obj)
        {
            ViewExtensionOpenWithParameterRequest?.Invoke(extensionIdentification, obj);
        }
    }
}
