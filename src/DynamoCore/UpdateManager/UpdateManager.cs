using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ComponentModel;
using System.Windows;
using Dynamo.UI;
using System.Xml.Linq;

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
    /// This class provides services for product update management.
    /// </summary>
    public class UpdateManager
    {
        #region Private Class Data Members

        struct AppVersionInfo
        {
            public BinaryVersion Version;
            public string VersionInfoURL;
            public string InstallerURL;
        }

        private UpdateManager(DynamoLogger logger)
        {
            this.logger = logger;
        }

        private static UpdateManager self = null;
        private bool versionCheckInProgress = false;
        private BinaryVersion productVersion = null;
        private AppVersionInfo? updateInfo;
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

        public static UpdateManager CreateInstance(DynamoLogger logger)
        {
            if (self != null) return self;

            if (null == logger)
                throw new ArgumentNullException("logger", "Unspecified logger (61578808A807)");

            self = new UpdateManager(logger);

            return self;
        }

        /// <summary>
        /// Obtains singleton object instance of UpdateManager class
        /// </summary>
        public static UpdateManager Instance
        {
            get { return UpdateManager.self; }
        }

        /// <summary>
        /// Obtains product version string
        /// </summary>
        public BinaryVersion ProductVersion
        {
            get
            {
                if (null == productVersion)
                {
                    string executingAssemblyPathName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(executingAssemblyPathName);
                    productVersion = BinaryVersion.FromString(myFileVersionInfo.FileVersion.ToString());
                }

                return productVersion;
            }
        }

        /// <summary>
        /// Obtains available update version string 
        /// </summary>
        public BinaryVersion AvailableVersion
        {
            get
            {
                if (!updateInfo.HasValue)
                {
                    CheckForProductUpdate();
                    return ProductVersion;
                }

                return updateInfo.Value.Version;
            }
        }

        /// <summary>
        /// Obtains downloaded update file location.
        /// </summary>
        public string UpdateFileLocation { get; private set; }

        #endregion

        #region Public Class Operational Methods

        /// <summary>
        /// Async call to request the update version info from the web. 
        /// This call raises UpdateFound event notification, if an update is
        /// found.
        /// </summary>
        public void CheckForProductUpdate()
        {
            if (versionCheckInProgress != false)
                return;

            logger.Log("RequestUpdateVersionInfo", "RequestUpdateVersionInfo");

            versionCheckInProgress = true;
            var client = new WebClient();
            client.OpenReadCompleted += OnUpdateVersionRequested;
            client.OpenReadAsync(new Uri(Configurations.UpdateDownloadLocation));
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

        private void OnUpdateVersionRequested(object sender, OpenReadCompletedEventArgs e)
        {
            if (null == e || e.Error != null)
            {
                string errorMessage = "Unspecified error";
                if (null != e && (null != e.Error))
                    errorMessage = e.Error.Message;

                logger.LogError("UpdateManager-OnUpdateVersionRequested",
                    string.Format("Request failure: {0}", errorMessage));

                versionCheckInProgress = false;
                return;
            }

            //element in the response from s3 will be of the form:
            /*
            <Contents>
                <Key>DynamoDailyInstall20130622T1917.exe</Key>
                <LastModified>2013-06-22T23:17:54.000Z</LastModified>
                <ETag>"f143035f2ef294f928a1e9a197d2d5af"</ETag>
                <Size>32389918</Size>
                <StorageClass>STANDARD</StorageClass>
            </Contents>
             */

            XNamespace ns = "http://s3.amazonaws.com/doc/2006-03-01/";
            var doc = XDocument.Load(e.Result);

            var bucketresult = doc.Element(ns + "ListBucketResult");
            var builds = bucketresult.Descendants(ns + "LastModified").
                OrderByDescending(x => DateTime.Parse(x.Value)).
                Where(x => x.Parent.Value.Contains("DynamoInstall")).
                Select(x => x.Parent);

            var xElements = builds as XElement[] ?? builds.ToArray();
            if (!xElements.Any())
            {
                versionCheckInProgress = false;
                return;
            }

            var latestBuild = xElements.First();
            var latestBuildFileName = latestBuild.Element(ns + "Key").Value;
            var latestBuildDownloadUrl = Path.Combine(Configurations.UpdateDownloadLocation, latestBuildFileName);
            var latestBuildVersion = BinaryVersion.FromString(Path.GetFileNameWithoutExtension(latestBuildFileName).Remove(0, 13));

            updateInfo = new AppVersionInfo()
            {
                Version = latestBuildVersion,
                VersionInfoURL = Configurations.UpdateDownloadLocation,
                InstallerURL = latestBuildDownloadUrl
            };

            logger.LogInfo("UpdateManager-OnUpdateVersionRequested",
                string.Format("Product Version: {0} Available Version : {1}",
                ProductVersion.ToString(), latestBuildVersion.ToString()));

            if (updateInfo.Value.Version <= this.ProductVersion)
            {
                versionCheckInProgress = false;
                return; // Up-to-date, no download required.
            }

            DownloadUpdatePackage(updateInfo.Value.InstallerURL, updateInfo.Value.Version);
        }

        private void OnDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            versionCheckInProgress = false;

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

        #endregion

        #region Private Class Helper Methods

        /// <summary>
        /// Async call to request downloading a file from web.
        /// This call raises UpdateDownloaded event notification.
        /// </summary>
        /// <param name="url">Web URL for file to download.</param>
        /// <param name="version">The version of package that is to be downloaded.</param>
        /// <returns>Request status, it may return false if invalid URL was passed.</returns>
        private bool DownloadUpdatePackage(string url, BinaryVersion version)
        {
            if (string.IsNullOrEmpty(url) || (null == version))
            {
                versionCheckInProgress = false;
                return false;
            }

            UpdateFileLocation = string.Empty;
            string downloadedFileName = string.Empty;
            string downloadedFilePath = string.Empty;

            try
            {
                //downloadedFileName = Path.GetFileNameWithoutExtension(url);
                //downloadedFileName += "." + version.ToString() + Path.GetExtension(url);
                downloadedFileName = Path.GetFileName(url);
                downloadedFilePath = Path.Combine(Path.GetTempPath(), downloadedFileName);

                if (File.Exists(downloadedFilePath))
                    File.Delete(downloadedFilePath);
            }
            catch (Exception)
            {
                versionCheckInProgress = false;
                return false;
            }

            WebClient client = new WebClient();
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(OnDownloadFileCompleted);
            client.DownloadFileAsync(new Uri(url), downloadedFilePath, downloadedFilePath);
            return true;
        }

        #endregion
    }
}
