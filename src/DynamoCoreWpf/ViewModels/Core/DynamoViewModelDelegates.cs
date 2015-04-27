using Dynamo.Models;
using Dynamo.PackageManager;

namespace Dynamo.ViewModels
{
    public delegate void ImageSaveEventHandler(object sender, ImageSaveEventArgs e);

    public delegate void WorkspaceSaveEventHandler(object sender, WorkspaceSaveEventArgs e);

    public delegate void RequestPackagePublishDialogHandler(PublishPackageViewModel publishViewModel);

    public delegate void RequestAboutWindowHandler(DynamoViewModel aboutViewModel);

    public delegate void RequestShowHideGalleryHandler(bool showGallery);

    public delegate void RequestViewOperationHandler(ViewOperationEventArgs e);

    public delegate void RequestBitmapSourceHandler(IconRequestEventArgs e);
}
