namespace Dynamo.Controls
{
    public class Watch3DFullscreenViewModel : dynViewModelBase
    {
        public dynWorkspaceViewModel ParentWorkspace { get; set; }

        public Watch3DFullscreenViewModel(dynWorkspaceViewModel parentWorkspace)
        {
            ParentWorkspace = parentWorkspace;
        }
    }
}
