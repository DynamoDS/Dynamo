namespace Dynamo.ViewModels
{
    public class Watch3DFullscreenViewModel : ViewModelBase
    {
        public WorkspaceViewModel ParentWorkspace { get; set; }

        public Watch3DFullscreenViewModel(WorkspaceViewModel parentWorkspace)
        {
            ParentWorkspace = parentWorkspace;
        }
    }
}
