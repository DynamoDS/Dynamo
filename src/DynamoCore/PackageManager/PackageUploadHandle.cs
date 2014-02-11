using Greg.Requests;
using Greg.Responses;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.PackageManager
{
    public class PackageUploadHandle : NotificationObject
    {
        public enum State
        {
            Ready, Copying, Compressing, Uploading, Uploaded, Error
        }

        private string _errorString = "";
        public string ErrorString { get { return _errorString; } set { _errorString = value; RaisePropertyChanged("ErrorString"); } }

        private State _uploadState = State.Ready;

        public State UploadState
        {
            get { return _uploadState; }
            set
            {
                _uploadState = value;
                RaisePropertyChanged("UploadState");
            }
        }

        public PackageUploadRequestBody Header { get; private set; }
        public PackageVersionUploadRequestBody VersionHeader { get; private set; }
        public string Name { get { return Header.name; } }
        public PackageHeader CompletedHeader { get; set; }

        public string VersionName { get { return Header != null ? Header.version : VersionHeader.version; } }

        public PackageUploadHandle(PackageUploadRequestBody header)
        {
            Header = header;
        }

        public PackageUploadHandle(PackageVersionUploadRequestBody header)
        {
            VersionHeader = header;
        }

        public void Error(string errorString)
        {
            ErrorString = errorString;
            UploadState = State.Error;
        }

        public void Done(PackageHeader ph)
        {
            CompletedHeader = ph;
            UploadState = State.Uploaded;
        }

    }

}
