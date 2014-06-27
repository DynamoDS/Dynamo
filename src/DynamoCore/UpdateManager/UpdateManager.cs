using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using Dynamo.Interfaces;
using Dynamo.Utilities;
using Dynamo.UI;
using System.Xml.Linq;
using DynamoCrypto;
using DynamoUtilities;
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
        void HostApplicationBeginQuit();
        void UpdateDataAvailable(IAsynchronousRequest request);
        bool IsVersionCheckInProgress();
        bool CheckNewerDailyBuilds { get; set; }
        bool ForceUpdate { get; set; }
        string UpdateFileLocation { get; }
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
        string SignatureURL { get; set; }
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
        public string SignatureURL { get; set; }
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

        private bool versionCheckInProgress;
        private BinaryVersion productVersion;
        private IAppVersionInfo updateInfo;
        private readonly ILogger logger;
        private const string InstallNameBase = "DynamoInstall";
        private const string OldDailyInstallNameBase = "DynamoDailyInstall";
        private bool checkNewerDailyBuilds;
        private bool forceUpdate;
        private string updateFileLocation;
        private int currentDownloadProgress = -1;

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
                if (null == productVersion)
                {
                    string executingAssemblyPathName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(executingAssemblyPathName);
                    productVersion = BinaryVersion.FromString(myFileVersionInfo.FileVersion);
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
                return updateInfo == null ? 
                    ProductVersion : 
                    updateInfo.Version;
            }
        }

        /// <summary>
        /// Obtains downloaded update file location.
        /// </summary>
        public string UpdateFileLocation
        {
            get { return updateFileLocation; }
            private set
            {
                updateFileLocation = value;
                RaisePropertyChanged("UpdateFileLocation");

                // Save the last downloaded location to the preferences.
                dynSettings.Controller.PreferenceSettings.LastUpdateDownloadPath = updateFileLocation;
            }
        }

        public IAppVersionInfo UpdateInfo
        {
            get { return updateInfo; }
            set
            {
                if (value != null)
                {
                    logger.Log(string.Format("Update available: {0}", value.Version));
                }
                
                updateInfo = value;
                RaisePropertyChanged("UpdateInfo");
            }
        }

        /// <summary>
        /// This flag is available via the debug menu to
        /// allow the update manager to check for newer daily 
        /// builds as well.
        /// </summary>
        public bool CheckNewerDailyBuilds
        {
            get { return checkNewerDailyBuilds; }
            set
            {
                if (!checkNewerDailyBuilds && value)
                {
                    CheckForProductUpdate(new UpdateRequest(new Uri(Configurations.UpdateDownloadLocation), dynSettings.DynamoLogger, UpdateDataAvailable));
                }
                checkNewerDailyBuilds = value;
                RaisePropertyChanged("CheckNewerDailyBuilds");
            }
        }

        /// <summary>
        /// Apply the most recent update, regardless
        /// of whether it is newer than the current version.
        /// </summary>
        public bool ForceUpdate
        {
            get { return forceUpdate; }
            set
            {
                if (!forceUpdate && value)
                {
                    // do a check
                    CheckForProductUpdate(new UpdateRequest(new Uri(Configurations.UpdateDownloadLocation), dynSettings.DynamoLogger, UpdateDataAvailable));
                }
                forceUpdate = value;
                RaisePropertyChanged("ForceUpdate");
            }
        }

        #endregion

        public UpdateManager(ILogger logger)
        {
            this.logger = logger;
            PropertyChanged += UpdateManager_PropertyChanged;
        }

        void UpdateManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "UpdateInfo":
                    if (updateInfo != null)
                    {
                        //When the UpdateInfo property changes, this will be reflected in the UI
                        //by the vsisibility of the download cloud. The most up to date version will
                        //be downloaded asynchronously.
                        logger.Log("Update download started...");

                        var tempPath = Path.GetTempPath();
                        DownloadUpdatePackageAsynchronously(updateInfo.InstallerURL, updateInfo.Version, tempPath);
                        DownloadSignatureFileAsynchronously(updateInfo.SignatureURL, tempPath);
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
            logger.Log("RequestUpdateVersionInfo", LogLevel.File);
            logger.Log("Requesting version update info...");

            if (versionCheckInProgress)
                return;

            versionCheckInProgress = true;
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
                versionCheckInProgress = false;
                return;
            }

            var latestBuildFilePath = GetLatestBuildFromS3(request, CheckNewerDailyBuilds);
            if (string.IsNullOrEmpty(latestBuildFilePath))
            {
                versionCheckInProgress = false;
                return;
            }

            // Strip the build number from the file name.
            // DynamoInstall0.7.0 becomes 0.7.0. Build a version
            // and compare it with the current product version.

            var latestBuildDownloadUrl = Path.Combine(Configurations.UpdateDownloadLocation, latestBuildFilePath);
            var latestBuildSignatureUrl = Path.Combine(
                Configurations.UpdateSignatureLocation,
                Path.GetFileNameWithoutExtension(latestBuildFilePath) + ".sig");

            BinaryVersion latestBuildVersion;
            var latestBuildTime = new DateTime();

            bool useStable = false;
            if (IsStableBuild(InstallNameBase, latestBuildFilePath))
            {
                useStable = true;
                latestBuildVersion = GetBinaryVersionFromFilePath(InstallNameBase, latestBuildFilePath);
            }
            else if (IsDailyBuild(InstallNameBase, latestBuildFilePath) || IsDailyBuild(OldDailyInstallNameBase, latestBuildFilePath))
            {
                latestBuildTime = GetBuildTimeFromFilePath(InstallNameBase, latestBuildFilePath);
                latestBuildVersion = GetCurrentBinaryVersion();
            }
            else
            {
                logger.Log("The specified file path is not recognizable as a stable or a daily build");
                versionCheckInProgress = false;
                return;
            }

            // Check the last downloaded update. If it's the same or newer as the 
            // one found on S3, then just set the update information to that one
            // and bounce.

            //if (ExistingUpdateIsNewer())
            //{
            //    logger.Log(string.Format("Using previously updated download {0}", dynSettings.Controller.PreferenceSettings.LastUpdateDownloadPath));
            //    UpdateDownloaded(this, new UpdateDownloadedEventArgs(null, UpdateFileLocation));
            //    versionCheckInProgress = false;
            //    return;
            //}

            // Install the latest update regardless of whether it
            // is newer than the current build.
            if (ForceUpdate)
            {
                SetUpdateInfo(latestBuildVersion, latestBuildDownloadUrl, latestBuildSignatureUrl);
            }
            else
            {
                if (useStable) //Check stables
                {
                    if (latestBuildVersion > ProductVersion)
                    {
                        SetUpdateInfo(latestBuildVersion, latestBuildDownloadUrl, latestBuildSignatureUrl);
                    }
                    else
                    {
                        logger.Log("Dynamo is up to date.");
                    }
                }
                else // Check dailies
                {
                    if (latestBuildTime > DateTime.Now)
                    {
                        SetUpdateInfo(GetCurrentBinaryVersion(), latestBuildDownloadUrl, latestBuildSignatureUrl);
                    }
                    else
                    {
                        logger.Log("Dynamo is up to date.");
                    }
                }
            }

            versionCheckInProgress = false;
        }

        public void QuitAndInstallUpdate()
        {
            string message = string.Format("An update is available for {0}.\n\n" +
                "Click OK to close {0} and install\nClick CANCEL to cancel the update.", "Dynamo");

            MessageBoxResult result = MessageBox.Show(message, "Install Dynamo", MessageBoxButton.OKCancel);
            bool installUpdate = result == MessageBoxResult.OK;

            if (installUpdate)
            {
                if (ShutdownRequested != null)
                    ShutdownRequested(this, new EventArgs());
            }
            
        }

        public void HostApplicationBeginQuit()
        {
            if (!string.IsNullOrEmpty(UpdateFileLocation))
            {
                var currDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var updater = Path.Combine(currDir, "InstallUpdate.exe");
                if (File.Exists(updater))
                {
                    Process.Start(updater, UpdateFileLocation);
                }
            }
        }

        public bool IsVersionCheckInProgress()
        {
            return versionCheckInProgress;
        }

        #endregion

        #region Private Event Handlers

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            versionCheckInProgress = false;

            if (e == null)
                return;

            string errorMessage = ((null == e.Error) ? "Successful" : e.Error.Message);
            logger.Log("UpdateManager-OnDownloadFileCompleted", LogLevel.File);

            UpdateFileLocation = string.Empty;
            if (e.Error == null)
            {
                UpdateFileLocation = (string)e.UserState;
                logger.Log(string.Format("Update download complete."));
            }

            if (null != UpdateDownloaded)
                UpdateDownloaded(this, new UpdateDownloadedEventArgs(e.Error, UpdateFileLocation));
        }

        #endregion

        #region Private Class Helper Methods

        /// <summary>
        /// Get the file name of the latest build on S3
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetLatestBuildFromS3(IAsynchronousRequest request, bool checkDailyBuilds)
        {
            XNamespace ns = "http://s3.amazonaws.com/doc/2006-03-01/";

            XDocument doc = null;
            using (TextReader td = new StringReader(request.Data))
            {
                try
                {
                    doc = XDocument.Load(td);
                }
                catch (Exception e)
                {
                    dynSettings.DynamoLogger.Log(e);
                    return null;
                }
            }

            // Reads filenames from S3, and pulls out those which include 
            // DynamoInstall, and optionally, those that include DynamoDailyInstall.
            // Order the results according to their LastUpdated field.

            var bucketresult = doc.Element(ns + "ListBucketResult");

            var builds = bucketresult.Descendants(ns + "LastModified").
                OrderByDescending(x => DateTime.Parse(x.Value)).
                Where(x => x.Parent.Value.Contains(InstallNameBase) || x.Parent.Value.Contains(OldDailyInstallNameBase)).
                Select(x => x.Parent);


            var xElements = builds as XElement[] ?? builds.ToArray();
            if (!xElements.Any())
            {
                return null;
            }

            var fileNames = xElements.Select(x => x.Element(ns + "Key").Value);

            string latestBuild = string.Empty;
            latestBuild = checkDailyBuilds ?
                fileNames.FirstOrDefault(x => IsDailyBuild(InstallNameBase, x) || IsDailyBuild(OldDailyInstallNameBase, x)) : 
                fileNames.FirstOrDefault(x=>IsStableBuild(InstallNameBase, x));

            return latestBuild;
        }

        /// <summary>
        /// Get a build time from a file path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>A DateTime or the DateTime MinValue.</returns>
        internal static DateTime GetBuildTimeFromFilePath(string installNameBase, string filePath)
        {
            var version = GetVersionString(installNameBase, filePath);
            var dtStr = version.Split('.').LastOrDefault();

            DateTime dt;
            return DateTime.TryParseExact(
                dtStr,
                "yyyyMMddTHHmm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dt) ? dt : DateTime.MinValue;
        }

        /// <summary>
        /// Find the version string within a file name 
        /// by removing the base install name.
        /// </summary>
        /// <param name="installNameBase"></param>
        /// <param name="filePath"></param>
        /// <returns>A version string like "x.x.x.x" or null if one cannot be found.</returns>
        private static string GetVersionString(string installNameBase, string filePath)
        {
            if (!filePath.Contains(installNameBase))
            {
                return null;
            }

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            return fileName.Replace(installNameBase, "");
        }

        /// <summary>
        /// Get a binary version for the executing assembly
        /// </summary>
        /// <returns>A BinaryVersion</returns>
        internal static BinaryVersion GetCurrentBinaryVersion()
        {
            // If we're looking at dailies, latest build version will simply be
            // the current build version without a build or revision, ex. 0.6
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            return BinaryVersion.FromString(string.Format("{0}.{1}.{2}", v.Major, v.Minor,v.Build));
        }

        /// <summary>
        /// Get a BinaryVersion from a file path.
        /// </summary>
        /// <param name="installNameBase">The base install name.</param>
        /// <param name="filePath">The path name of the file.</param>
        /// <returns>A BinaryVersion or null if one can not be parse from the file path.</returns>
        internal static BinaryVersion GetBinaryVersionFromFilePath(string installNameBase, string filePath)
        {
            // Filename format is DynamoInstall0.7.1.YYYYMMDDT0000.exe

            if (!filePath.Contains(installNameBase))
            {
                return null;
            }

            var fileName = Path.GetFileNameWithoutExtension(filePath).Replace(installNameBase, "");
            var splits = fileName.Split(new string[]{"."},StringSplitOptions.RemoveEmptyEntries);

            if (splits.Count() < 3)
            {
                return null;
            }

            var major = ushort.Parse(splits[0]);
            var minor = ushort.Parse(splits[1]);
            var build = ushort.Parse(splits[2]);

            return BinaryVersion.FromString(string.Format("{0}.{1}.{2}.0", major, minor, build));
        }

        private void SetUpdateInfo(BinaryVersion latestBuildVersion, string latestBuildDownloadUrl, string signatureUrl)
        {
            UpdateInfo = new AppVersionInfo()
            {
                Version = latestBuildVersion,
                VersionInfoURL = Configurations.UpdateDownloadLocation,
                InstallerURL = latestBuildDownloadUrl,
                SignatureURL = signatureUrl
            };
        }

        /// <summary>
        /// Check if a file name is a daily build.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>True if this is a daily build, otherwise false.</returns>
        internal static bool IsDailyBuild(string installNameBase, string fileName)
        {
            if (!fileName.Contains(installNameBase))
            {
                return false;
            }

            var versionStr = GetVersionString(installNameBase, fileName);
            var splits = versionStr.Split('.');

            DateTime dt;
            return DateTime.TryParseExact(
                splits.Last(),
                "yyyyMMddTHHmm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dt);
        }

        /// <summary>
        /// Check if a file name is a stable build.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>True if this is a stable build, otherwise false.</returns>
        internal static bool IsStableBuild(string installNameBase, string fileName)
        {
            if (!fileName.Contains(installNameBase))
            {
                return false;
            }
            return !IsDailyBuild(installNameBase,fileName);
        }

        /// <summary>
        /// Async call to request downloading a file from web.
        /// This call raises UpdateDownloaded event notification.
        /// </summary>
        /// <param name="url">Web URL for file to download.</param>
        /// <param name="version">The version of package that is to be downloaded.</param>
        /// <returns>Request status, it may return false if invalid URL was passed.</returns>
        private bool DownloadUpdatePackageAsynchronously(string url, BinaryVersion version, string tempPath)
        {
            currentDownloadProgress = -1;

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
                downloadedFileName = Path.GetFileName(url);
                downloadedFilePath = Path.Combine(tempPath, downloadedFileName);

                if (File.Exists(downloadedFilePath))
                    File.Delete(downloadedFilePath);
            }
            catch (Exception)
            {
                versionCheckInProgress = false;
                return false;
            }

            var client = new WebClient();
            client.DownloadProgressChanged += client_DownloadProgressChanged;
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(OnDownloadFileCompleted);
            client.DownloadFileAsync(new Uri(url), downloadedFilePath, downloadedFilePath);
            return true;
        }

        /// <summary>
        /// Async call to download the signature file.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool DownloadSignatureFileAsynchronously(string url, string tempPath)
        {
            string downloadedFileName = string.Empty;
            string downloadedFilePath = string.Empty;

            try
            {
                downloadedFileName = Path.GetFileName(url);
                downloadedFilePath = Path.Combine(tempPath, downloadedFileName);

                if (File.Exists(downloadedFilePath))
                    File.Delete(downloadedFilePath);
            }
            catch (Exception)
            {
                versionCheckInProgress = false;
                return false;
            }

            var client = new WebClient();
            client.DownloadFileAsync(new Uri(url), downloadedFilePath, downloadedFilePath);
            return true;
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage % 10 == 0 && 
                e.ProgressPercentage > currentDownloadProgress)
            {
                logger.Log(string.Format("Update download progress: {0}%", e.ProgressPercentage));
                currentDownloadProgress = e.ProgressPercentage;
            }
        }

        #endregion
    }
}
