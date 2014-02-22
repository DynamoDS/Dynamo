using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ComponentModel;
using System.Windows;
using Dynamo.Interfaces;
using Dynamo.UI;
using System.Xml.Linq;
using Microsoft.Practices.Prism.ViewModel;

namespace Dynamo.UpdateManager
{
    public delegate void UpdateDownloadedEventHandler(object sender, UpdateDownloadedEventArgs e);
    public delegate void ShutdownRequestedEventHandler(object sender, EventArgs e);

    public class UpdateDownloadedEventArgs : EventArgs
    {
        public UpdateDownloadedEventArgs(Exception error, string fileLocation)
        {
            Error = error;
            UpdateFileLocation = fileLocation;
            UpdateAvailable = !string.IsNullOrEmpty(fileLocation);
        }

        public bool UpdateAvailable { get; private set; }
        public string UpdateFileLocation { get; private set; }
        public Exception Error { get; private set; }
    }

    /// <summary>
    /// An interface which describes properties and methods for
    /// updating the application.
    /// </summary>
    public interface IUpdateManager
    {
        BinaryVersion ProductVersion { get; }
        BinaryVersion AvailableVersion { get; }
        IAppVersionInfo UpdateInfo { get; set; }
        event UpdateDownloadedEventHandler UpdateDownloaded;
        event ShutdownRequestedEventHandler ShutdownRequested;
        void CheckForProductUpdate(IAsynchronousRequest request);
        void QuitAndInstallUpdate();
        void HostApplicationBeginQuit(object sender, EventArgs e);
        void UpdateDataAvailable(IAsynchronousRequest request);
        bool IsVersionCheckInProgress();
    }

    /// <summary>
    /// An interface to describe available
    /// application update info.
    /// </summary>
    public interface IAppVersionInfo
    {
        BinaryVersion Version { get; set; }
        string VersionInfoURL { get; set; }
        string InstallerURL { get; set; }
    }

    /// <summary>
    /// An interface to describe an asynchronous web
    /// request for update data.
    /// </summary>
    public interface IAsynchronousRequest
    {
        ILogger Logger { get; set; }
        string Data { get; set; }
        string Error { get; set; }
        Uri Path { get; set; }
        Action<IAsynchronousRequest>  OnRequestCompleted { get; set; }
    }

    public class AppVersionInfo : IAppVersionInfo
    {
        public BinaryVersion Version { get; set; }
        public string VersionInfoURL { get; set; }
        public string InstallerURL { get; set; }
    }

    /// <summary>
    /// The UpdateRequest class encapsulates a request for 
    /// getting update information from the web.
    /// </summary>
    internal class UpdateRequest : IAsynchronousRequest
    {
        /// <summary>
        /// An action to be invoked upon completion of the request.
        /// This action is invoked regardless of the success of the request.
        /// </summary>
        public Action<IAsynchronousRequest> OnRequestCompleted { get; set; }

        public ILogger Logger { get; set; }

        /// <summary>
        /// The data returned from the request.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Any error information returned from the request.
        /// </summary>
        public string Error { get; set; }

        public Uri Path { get; set; }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="log">A logger to which to write info.</param>
        /// <param name="onRequestCompleted">A callback which is invoked when data is returned from the request.</param>
        public UpdateRequest(Uri path, ILogger log, Action<IAsynchronousRequest> onRequestCompleted)
        {
            OnRequestCompleted = onRequestCompleted;

            Logger = log;
            Error = string.Empty;
            Data = string.Empty;
            Path = path;

            var client = new WebClient();
            client.OpenReadAsync(path);
            client.OpenReadCompleted += ReadResult;
        }

        /// <summary>
        /// Event handler for the web client's requestion completed event. Reads
        /// the request's result information and subsequently triggers
        /// the UpdateDataAvailable event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadResult(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                if (null == e || e.Error != null)
                {
                    Error = "Unspecified error";
                    if (null != e && (null != e.Error))
                        Error = e.Error.Message;
                }

                using (var sr = new StreamReader(e.Result))
                {
                    Data = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Error = string.Empty;
                Data = string.Empty;

                Logger.Log("UpdateRequest", "The update request could not be completed.\n" + ex.Message);
            }

            //regardless of the success of the above logic
            //invoke the completion callback
            OnRequestCompleted.Invoke(this);
        }
    }

    /// <summary>
    /// This class provides services for product update management.
    /// </summary>
    public class UpdateManager: NotificationObject, IUpdateManager
    {
        #region Private Class Data Members

        private bool _versionCheckInProgress;
        private BinaryVersion _productVersion;
        private IAppVersionInfo _updateInfo;
        private ILogger _logger;

        #endregion

        #region Public Event Handlers

        /// <summary>
        /// Occurs when RequestUpdateDownload operation completes.
        /// </summary>
        public event UpdateDownloadedEventHandler UpdateDownloaded;
        public event ShutdownRequestedEventHandler ShutdownRequested;

        #endregion

        #region Public Class Properties

        /// <summary>
        /// Obtains product version string
        /// </summary>
        public BinaryVersion ProductVersion
        {
            get
            {
                if (null == _productVersion)
                {
                    string executingAssemblyPathName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(executingAssemblyPathName);
                    _productVersion = BinaryVersion.FromString(myFileVersionInfo.FileVersion);
                }

                return _productVersion;
            }
        }

        /// <summary>
        /// Obtains available update version string 
        /// </summary>
        public BinaryVersion AvailableVersion
        {
            get
            {
                return _updateInfo == null ? 
                    ProductVersion : 
                    _updateInfo.Version;
            }
        }

        /// <summary>
        /// Obtains downloaded update file location.
        /// </summary>
        public string UpdateFileLocation { get; private set; }

        public IAppVersionInfo UpdateInfo
        {
            get { return _updateInfo; }
            set
            {
                _updateInfo = value;
                RaisePropertyChanged("UpdateInfo");
            }
        }

        #endregion

        public UpdateManager()
        {
            _logger = DynamoLogger.Instance;
            PropertyChanged += UpdateManager_PropertyChanged;
        }

        void UpdateManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "UpdateInfo":
                    if (_updateInfo != null)
                    {
                        //HOLD for post 0.7.0
                        //When the UpdateInfo property changes, this will be reflected in the UI
                        //by the vsisibility of the download cloud. The most up to date version will
                        //be downloaded asynchronously.
                        //DownloadUpdatePackageAsynchronously(_updateInfo.InstallerURL, _updateInfo.Version);
                    }
                    break;
            }
        }

        #region Public Class Operational Methods

        /// <summary>
        /// Async call to request the update version info from the web. 
        /// This call raises UpdateFound event notification, if an update is
        /// found.
        /// </summary>
        public void CheckForProductUpdate(IAsynchronousRequest request)
        {
            _logger.Log("RequestUpdateVersionInfo", "RequestUpdateVersionInfo");

            if (_versionCheckInProgress)
                return;

            _versionCheckInProgress = true;
        }

        /// <summary>
        /// Callback for the UpdateRequest's UpdateDataAvailable event.
        /// Reads the request's data, and parses for available versions. 
        /// If a more recent version is available, the UpdateInfo object 
        /// will be set. 
        /// </summary>
        /// <param name="request">An instance of an update request.</param>
        public void UpdateDataAvailable(IAsynchronousRequest request)
        {
            UpdateInfo = null;

            //If there is error data or the request data is empty
            //bail out.
            if (!string.IsNullOrEmpty(request.Error) || 
                string.IsNullOrEmpty(request.Data))
            {
                _versionCheckInProgress = false;
                return;
            }

            XNamespace ns = "http://s3.amazonaws.com/doc/2006-03-01/";

            XDocument doc = null;
            using (TextReader td = new StringReader(request.Data))
            {
                doc = XDocument.Load(td);
            }

            var bucketresult = doc.Element(ns + "ListBucketResult");
            var builds = bucketresult.Descendants(ns + "LastModified").
                OrderByDescending(x => DateTime.Parse(x.Value)).
                Where(x => x.Parent.Value.Contains("DynamoInstall")).
                Select(x => x.Parent);

            var xElements = builds as XElement[] ?? builds.ToArray();
            if (!xElements.Any())
            {
                _versionCheckInProgress = false;
                return;
            }

            var latestBuild = xElements.First();
            var latestBuildFileName = latestBuild.Element(ns + "Key").Value;

            var latestBuildDownloadUrl = Path.Combine(Configurations.UpdateDownloadLocation, latestBuildFileName);
            var latestBuildVersion = BinaryVersion.FromString(Path.GetFileNameWithoutExtension(latestBuildFileName).Remove(0, 13));

            if (latestBuildVersion > ProductVersion)
            {
                UpdateInfo = new AppVersionInfo()
                {
                    Version = latestBuildVersion,
                    VersionInfoURL = Configurations.UpdateDownloadLocation,
                    InstallerURL = latestBuildDownloadUrl
                };
            }

            _versionCheckInProgress = false;
        }

        public void QuitAndInstallUpdate()
        {
            string message = string.Format("An update is available for {0}.\n\n" +
                "Click OK to close {0} and install\nClick CANCEL to cancel the update.", "Dynamo");

            MessageBoxResult result = MessageBox.Show(message, "Install Dynamo", MessageBoxButton.OKCancel);
            bool installUpdate = result == MessageBoxResult.OK;

            _logger.Log("UpdateManager-QuitAndInstallUpdate",
                (installUpdate ? "Install button clicked" : "Cancel button clicked"));

            if (installUpdate)
            {
                if (ShutdownRequested != null)
                    ShutdownRequested(this, new EventArgs());
            }
        }

        public void HostApplicationBeginQuit(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(UpdateFileLocation))
            {
                if (File.Exists(UpdateFileLocation))
                    Process.Start(UpdateFileLocation);
            }
        }

        public bool IsVersionCheckInProgress()
        {
            return _versionCheckInProgress;
        }

        #endregion

        #region Private Event Handlers

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            _versionCheckInProgress = false;

            if (e == null)
                return;

            string errorMessage = ((null == e.Error) ? "Successful" : e.Error.Message);
            _logger.Log("UpdateManager-OnDownloadFileCompleted", errorMessage);

            UpdateFileLocation = string.Empty;
            if (e.Error == null)
                UpdateFileLocation = (string)e.UserState;

            if (null != UpdateDownloaded)
                UpdateDownloaded(this, new UpdateDownloadedEventArgs(e.Error, UpdateFileLocation));
        }

        #endregion

        #region Private Class Helper Methods

        /// <summary>
        /// Async call to request downloading a file from web.
        /// This call raises UpdateDownloaded event notification.
        /// </summary>
        /// <param name="url">Web URL for file to download.</param>
        /// <param name="version">The version of package that is to be downloaded.</param>
        /// <returns>Request status, it may return false if invalid URL was passed.</returns>
        private bool DownloadUpdatePackageAsynchronously(string url, BinaryVersion version)
        {
            if (string.IsNullOrEmpty(url) || (null == version))
            {
                _versionCheckInProgress = false;
                return false;
            }

            UpdateFileLocation = string.Empty;
            string downloadedFileName = string.Empty;
            string downloadedFilePath = string.Empty;

            try
            {
                downloadedFileName = Path.GetFileName(url);
                downloadedFilePath = Path.Combine(Path.GetTempPath(), downloadedFileName);

                if (File.Exists(downloadedFilePath))
                    File.Delete(downloadedFilePath);
            }
            catch (Exception)
            {
                _versionCheckInProgress = false;
                return false;
            }

            var client = new WebClient();
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(OnDownloadFileCompleted);
            client.DownloadFileAsync(new Uri(url), downloadedFilePath, downloadedFilePath);
            return true;
        }

        #endregion
    }
}
