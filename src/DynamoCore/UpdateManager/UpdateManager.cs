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
    }

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
        void ReadResult(object sender, OpenReadCompletedEventArgs e);
        event EventHandler UpdateDataAvailable;
    }

    public class AppVersionInfo : IAppVersionInfo
    {
        public BinaryVersion Version { get; set; }
        public string VersionInfoURL { get; set; }
        public string InstallerURL { get; set; }
    }

    /// <summary>
    /// The UpdateRequest class encapsulates a request for 
    /// update information from the web.
    /// </summary>
    internal class UpdateRequest : IAsynchronousRequest
    {
        public ILogger Logger { get; set; }

        /// <summary>
        /// The data returned from the request.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// Any error information returned from the request.
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Event triggered when data is available from the request
        /// </summary>
        public event EventHandler UpdateDataAvailable;
        protected virtual void OnUpdateDataAvailable(EventArgs e)
        {
            if (UpdateDataAvailable != null)
                UpdateDataAvailable(this, e);
        }

        public UpdateRequest(ILogger log)
        {
            Logger = log;
            Error = string.Empty;
            Data = string.Empty;

            var client = new WebClient();
            client.OpenReadAsync(new Uri(Configurations.UpdateDownloadLocation));
            client.OpenReadCompleted += ReadResult;
        }

        /// <summary>
        /// Event handler for the web client's requestion completed event. Reads
        /// the request's result information and subsequently triggers
        /// the UpdateDataAvailable event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReadResult(object sender, OpenReadCompletedEventArgs e)
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

                OnUpdateDataAvailable(EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Logger.Log("UpdateRequest", "The update request could not be completed.\n" + ex.Message);
            }
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

            try
            {
                if (_versionCheckInProgress)
                    return;

                _versionCheckInProgress = true;

                request.UpdateDataAvailable += request_UpdateDataAvailable;
            }
            catch (Exception ex)
            {
                _versionCheckInProgress = false;
                DynamoLogger.Instance.LogError("UpdateRequest", string.Format("Could not complete product update request:\n {0}", ex.Message));
            }
        }

        /// <summary>
        /// Handler for the UpdateRequest's UpdateDataAvailable event.
        /// Reads the request's data, and parses for available versions. 
        /// If a more recent version is available, the UpdateInfo object 
        /// will be set. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void request_UpdateDataAvailable(object sender, EventArgs e)
        {
            UpdateInfo = null;

            var request = sender as IAsynchronousRequest;

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
