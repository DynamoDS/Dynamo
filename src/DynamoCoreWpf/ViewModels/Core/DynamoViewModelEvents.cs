using System;

using Dynamo.Models;
using Dynamo.PackageManager;

namespace Dynamo.ViewModels
{
    partial class DynamoViewModel
    {
        public event EventHandler RequestManagePackagesDialog;
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

        public event EventHandler RequestPackagePathsDialog;
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

        public event EventHandler RequestScaleFactorDialog;
        public virtual void OnRequestScaleFactorDialog(object sender, EventArgs e)
        {
            var handler = RequestScaleFactorDialog;
            if (handler != null)
                handler(sender, e);
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

        public event RequestShowHideGalleryHandler RequestShowHideGallery;
        public virtual void OnRequestShowHideGallery(bool showGallery)
        {
            if (RequestShowHideGallery != null)
            {
                RequestShowHideGallery(showGallery);
            }
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
    }
}
