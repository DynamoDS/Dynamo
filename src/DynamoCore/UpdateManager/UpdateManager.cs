using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ComponentModel;
using System.Windows;
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
        event UpdateDownloadedEventHandler UpdateDownloaded;
        event ShutdownRequestedEventHandler ShutdownRequested;
        void CheckForProductUpdate();
        void QuitAndInstallUpdate();
        void HostApplicationBeginQuit(object sender, EventArgs e);
    }

    public interface IAppVersionInfo
    {
        BinaryVersion Version { get; set; }
        string VersionInfoURL { get; set; }
        string InstallerURL { get; set; }
    }

    public interface IAsynchronousRequest
    {

    }

    public class AppVersionInfo:IAppVersionInfo
    {
        public BinaryVersion Version { get; set; }
        public string VersionInfoURL { get; set; }
        public string InstallerURL { get; set; }
    }

    /// <summary>
    /// This class provides services for product update management.
    /// </summary>
    public class UpdateManager: NotificationObject, IUpdateManager
    {
        #region Private Class Data Members

        private bool _versionCheckInProgress = false;
        private BinaryVersion _productVersion = null;
        private IAppVersionInfo _updateInfo;
        private DynamoLogger logger = null;

        #endregion

        #region Public Event Handlers

        /// <summary>
        /// Occurs when RequestUpdateDownload operation completes.
        /// </summary>
        public event UpdateDownloadedEventHandler UpdateDownloaded;
        public event ShutdownRequestedEventHandler ShutdownRequested;

        #endregion

        #region Public Class Properties

        public UpdateManager()
        {
            logger = DynamoLogger.Instance;
        }

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

                if (_updateInfo != null)
                {
                    DownloadUpdatePackageAsynchronously(_updateInfo.InstallerURL, _updateInfo.Version); 
                }
            }
        }

        #endregion

        #region Public Class Operational Methods

        /// <summary>
        /// Async call to request the update version info from the web. 
        /// This call raises UpdateFound event notification, if an update is
        /// found.
        /// </summary>
        public void CheckForProductUpdate()
        {
            logger.Log("RequestUpdateVersionInfo", "RequestUpdateVersionInfo");

            try
            {
                if (_versionCheckInProgress)
                    return;

                _versionCheckInProgress = true;

                var client = new WebClient();
                client.OpenReadAsync(new Uri(Configurations.UpdateDownloadLocation));
                client.OpenReadCompleted += client_OpenReadCompleted;
            }
            catch (Exception ex)
            {
                _versionCheckInProgress = false;
                DynamoLogger.Instance.LogError("UpdateRequest", string.Format("Could not complete product update request:\n {0}", ex.Message));
            }
        }

        public void QuitAndInstallUpdate()
        {
            string message = string.Format("An update is available for {0}.\n\n" +
                "Click OK to close {0} and install\nClick CANCEL to cancel the update.", "Dynamo");

            MessageBoxResult result = MessageBox.Show(message, "Install Dynamo", MessageBoxButton.OKCancel);
            bool installUpdate = result == MessageBoxResult.OK;

            logger.LogInfo("UpdateManager-QuitAndInstallUpdate",
                (installUpdate ? "Install button clicked" : "Cancel button clicked"));

            if (false != installUpdate)
            {
                if (this.ShutdownRequested != null)
                    this.ShutdownRequested(this, new EventArgs());
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
            logger.LogInfo("UpdateManager-OnDownloadFileCompleted", errorMessage);

            UpdateFileLocation = string.Empty;
            if (e.Error == null)
                UpdateFileLocation = (string)e.UserState;

            if (null != UpdateDownloaded)
                UpdateDownloaded(this, new UpdateDownloadedEventArgs(e.Error, UpdateFileLocation));
        }

        private void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            UpdateInfo = null;

            if (null == e || e.Error != null)
            {
                string errorMessage = "Unspecified error";
                if (null != e && (null != e.Error))
                    errorMessage = e.Error.Message;

                logger.LogError("UpdateManager-OnUpdateVersionRequested",
                    string.Format("Request failure: {0}", errorMessage));

                _versionCheckInProgress = false;
                return;
            }

            string data;
            using (var sr = new StreamReader(e.Result))
            {
                data = sr.ReadToEnd();
            }

            if (string.IsNullOrEmpty(data))
            {
                _versionCheckInProgress = false;
                return;
            }

            XNamespace ns = "http://s3.amazonaws.com/doc/2006-03-01/";

            XDocument doc = null;
            using (TextReader td = new StringReader(data))
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
