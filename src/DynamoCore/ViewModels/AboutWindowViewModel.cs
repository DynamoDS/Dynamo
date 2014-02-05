using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.ViewModels
{
    public class AboutWindowViewModel : NotificationObject
    {
        public string Version
        {
            get { return string.Format("Version: {0}", UpdateManager.UpdateManager.Instance.ProductVersion); }
        }

        public bool UpToDate
        {
            get
            {
                return UpdateManager.UpdateManager.Instance.ProductVersion >= UpdateManager.UpdateManager.Instance.AvailableVersion; ;
            }
        }

        public AboutWindowViewModel()
        {
            UpdateManager.UpdateManager.Instance.UpdateDownloaded += Instance_UpdateDownloaded;
        }

        void Instance_UpdateDownloaded(object sender, UpdateManager.UpdateDownloadedEventArgs e)
        {
            RaisePropertyChanged("Version");
            RaisePropertyChanged("UpToDate");
        }
    }
}
