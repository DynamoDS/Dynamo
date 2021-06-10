using Dynamo.Models;
using Dynamo.PackageManager;

namespace Dynamo.ViewModels
{
    public delegate void ImageSaveEventHandler(object sender, ImageSaveEventArgs e);

    public delegate void WorkspaceSaveEventHandler(object sender, WorkspaceSaveEventArgs e);

    public delegate void RequestPackagePublishDialogHandler(PublishPackageViewModel publishViewModel);

    public delegate void RequestAboutWindowHandler(DynamoViewModel aboutViewModel);

    public delegate void RequestShowHideGalleryHandler(bool showGallery);

    internal delegate void RequestViewOperationHandler(ViewOperationEventArgs e);

    public delegate void RequestBitmapSourceHandler(IconRequestEventArgs e);

    public delegate void RequestOpenDocumentationLinkHandler(OpenDocumentationLinkEventArgs e);

    /// <summary>
    /// Eventhandler for verifying that the correct dialog is being showed when saving a graph with unresolved linter issues.
    /// This is only meant to be used for unit testing purposes.
    /// </summary>
    /// <param name="e"></param>
    internal delegate void SaveWarningOnUnresolvedIssuesShows(SaveWarningOnUnresolvedIssuesArgs e);
}
