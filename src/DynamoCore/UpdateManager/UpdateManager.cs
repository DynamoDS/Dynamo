using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using Dynamo.Core;
using Microsoft.Win32;

namespace Dynamo.UpdateManager
{
    public delegate void UpdateDownloadedEventHandler(object sender, UpdateDownloadedEventArgs e);
    public delegate void ShutdownRequestedEventHandler(IUpdateManager updateManager);

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
        bool IsUpdateAvailable { get; }
        event UpdateDownloadedEventHandler UpdateDownloaded;
        event ShutdownRequestedEventHandler ShutdownRequested;
        void CheckForProductUpdate(IAsynchronousRequest request);
        void QuitAndInstallUpdate();
        void HostApplicationBeginQuit();
        void UpdateDataAvailable(IAsynchronousRequest request);
        bool CheckNewerDailyBuilds { get; set; }
        bool ForceUpdate { get; set; }
        IUpdateManagerConfiguration Configuration { get; }
        event LogEventHandler Log;
        void OnLog(LogEventArgs args);
        void RegisterExternalApplicationProcessId(int id);
    }

    public interface IDynamoLookUp
    {
        /// <summary>
        /// Gets installation path for all version of this Dynamo Product
        /// installed on this system.
        /// </summary>
        IEnumerable<string> GetDynamoInstallLocations();

        /// <summary>
        /// Gets the version of latest installed product
        /// </summary>
        BinaryVersion LatestProduct { get; }
    }

    public interface IUpdateManagerConfiguration
    {
        /// <summary>
        /// Defines download location for new installer
        /// </summary>
        string DownloadSourcePath { get; set; }

        /// <summary>
        /// Defines location for signature file to validate the new installer.
        /// </summary>
        string SignatureSourcePath { get; set; }

        /// <summary>
        /// Defines whether to consider daily builds for update, default is false.
        /// </summary>
        bool CheckNewerDailyBuild { get; set; }

        /// <summary>
        /// Defines whether to force update, default vlaue is false.
        /// </summary>
        bool ForceUpdate { get; set; }

        /// <summary>
        /// Gets the base name of the installer to be used for upgrade.
        /// </summary>
        string InstallerNameBase { get; set; }

        /// <summary>
        /// Gets IDynamoLookUp interface to search Dynamo installations on the system.
        /// </summary>
        IDynamoLookUp DynamoLookUp { get; set; }
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
        string Data { get; set; }
        string Error { get; set; }
        Uri Path { get; set; }
        Action<IAsynchronousRequest> OnRequestCompleted { get; set; }
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
        /// UpdateManager instance that created this request.
        /// </summary>
        private readonly IUpdateManager manager = null;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="path">Uri that needs to be read to get the update information.</param>
        /// <param name="manager">The update manager which is making this request.</param>
        public UpdateRequest(Uri path, IUpdateManager manager)
        {
            OnRequestCompleted = manager.UpdateDataAvailable;
            this.manager = manager;

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

                manager.OnLog(new LogEventArgs("The update request could not be completed.", LogLevel.File));
                manager.OnLog(new LogEventArgs(ex, LogLevel.File));
            }

            //regardless of the success of the above logic
            //invoke the completion callback
            OnRequestCompleted.Invoke(this);
        }
    }

    /// <summary>
    /// Defines Update Manager Configuration settings.
    /// </summary>
    public class UpdateManagerConfiguration : IUpdateManagerConfiguration
    {
        private const string PRODUCTION_SOURCE_PATH_S = "http://dyn-builds-data.s3.amazonaws.com/";
        private const string PRODUCTION_SIG_SOURCE_PATH_S = "http://dyn-builds-data-sig.s3.amazonaws.com/";
        private const string DEFAULT_CONFIG_FILE_S = "UpdateManagerConfig.xml";
        private const string INSTALL_NAME_BASE = "DynamoInstall";

        /// <summary>
        /// Defines download location for new installer
        /// </summary>
        public string DownloadSourcePath { get; set; }
        
        /// <summary>
        /// Defines location for signature file to validate the new installer.
        /// </summary>
        public string SignatureSourcePath { get; set; }

        /// <summary>
        /// Defines whether to consider daily builds for update, default is false.
        /// </summary>
        public bool CheckNewerDailyBuild { get; set; }

        /// <summary>
        /// Defines whether to force update, default vlaue is false.
        /// </summary>
        public bool ForceUpdate { get; set; }

        /// <summary>
        /// Gets the base name of the installer to be used for upgrade.
        /// </summary>
        public string InstallerNameBase { get; set; }

        /// <summary>
        /// Return file path for the overriding config file.
        /// </summary>
        [XmlIgnore]
        public string ConfigFilePath { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public UpdateManagerConfiguration()
        {
            DownloadSourcePath = PRODUCTION_SOURCE_PATH_S;
            SignatureSourcePath = PRODUCTION_SIG_SOURCE_PATH_S;
            CheckNewerDailyBuild = false;
            ForceUpdate = false;
            InstallerNameBase = INSTALL_NAME_BASE;
        }

        /// <summary>
        /// Loads the configurations from given xml file.
        /// </summary>
        /// <param name="filePath">Xml file path that contains configuration details.</param>
        /// <param name="updateManager"></param>
        /// <returns>UpdateManagerConfiguration</returns>
        public static UpdateManagerConfiguration Load(string filePath, IUpdateManager updateManager)
        {
            if(string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                return null;

            try
            {
                var serializer = new XmlSerializer(typeof(UpdateManagerConfiguration));
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var config = serializer.Deserialize(fs) as UpdateManagerConfiguration;
                    if(null != config)
                        config.ConfigFilePath = filePath;
                    return config;
                }
            }
            catch (Exception ex)
            {
                if (null != updateManager)
                    updateManager.OnLog(
                        new LogEventArgs(
                            string.Format(
                                Properties.Resources.FailedToLoad,
                                filePath,
                                ex.Message),
                            LogLevel.Console));
                else throw;
            }
            return null;
        }

        /// <summary>
        /// Saves this configuration to a given file in xml format.
        /// </summary>
        /// <param name="filePath">File path to save this configuration.</param>
        /// <param name="updateManager"></param>
        public void Save(string filePath, IUpdateManager updateManager)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(UpdateManagerConfiguration));
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    serializer.Serialize(fs, this);
                }
            }
            catch (Exception ex)
            {
                if (null != updateManager)
                    updateManager.OnLog(
                        new LogEventArgs(
                            string.Format(
                                Properties.Resources.FailedToSave,
                                filePath,
                                ex.Message),
                            LogLevel.Console));
                else throw;
            }
        }

        /// <summary>
        /// Utility method to get the settings
        /// </summary>
        /// <param name="lookUp">IDynamoLookUp instance</param>
        /// <param name="updateManager"></param>
        /// <returns></returns>
        public static UpdateManagerConfiguration GetSettings(IDynamoLookUp lookUp, IUpdateManager updateManager = null)
        {
            string filePath;
            var exists = TryGetConfigFilePath(out filePath);
#if DEBUG
            //This code is just to create the default config file to
            //save the default settings, which later on can be modified
            //to re-direct it to other download target for testing.
            if (!exists)
            {
                var umConfig = new UpdateManagerConfiguration();
                umConfig.Save(filePath, updateManager);
            }
#endif
            if (!exists) 
                return new UpdateManagerConfiguration() { DynamoLookUp = lookUp };

            var config = Load(filePath, updateManager);
            if (null != config)
                config.DynamoLookUp = lookUp;

            return config;
        }

        /// <summary>
        /// Gets the update manager config file path.
        /// </summary>
        /// <param name="filePath">Full path for the config file</param>
        /// <returns>True if file exists.</returns>
        public static bool TryGetConfigFilePath(out string filePath)
        {
            string location = Assembly.GetExecutingAssembly().Location;
            // ReSharper disable once AssignNullToNotNullAttribute, location is always available
            filePath = Path.Combine(Path.GetDirectoryName(location), DEFAULT_CONFIG_FILE_S);
            return File.Exists(filePath);
        }

        [XmlIgnore]
        public IDynamoLookUp DynamoLookUp { get; set; }
    }

    /// <summary>
    /// This class provides services for product update management.
    /// </summary>
    public sealed class UpdateManager : NotificationObject, IUpdateManager
    {
        #region Private Class Data Members

        private bool versionCheckInProgress;
        private static BinaryVersion productVersion;
        private IAppVersionInfo updateInfo;
        private const string OLD_DAILY_INSTALL_NAME_BASE = "DynamoDailyInstall";
        private const string INSTALLUPDATE_EXE = "InstallUpdate.exe";
        private string updateFileLocation;
        private int currentDownloadProgress = -1;
        private IAppVersionInfo downloadedUpdateInfo;
        private IUpdateManagerConfiguration configuration = null;
        private int hostApplicationProcessId = -1;

        #endregion

        #region Public Event Handlers

        /// <summary>
        /// Occurs when RequestUpdateDownload operation completes.
        /// </summary>
        public event UpdateDownloadedEventHandler UpdateDownloaded;
        public event ShutdownRequestedEventHandler ShutdownRequested;
        public event LogEventHandler Log;

        #endregion

        #region Public Class Properties

        /// <summary>
        /// Obtains product version string
        /// </summary>
        public BinaryVersion ProductVersion
        {
            get
            {
                return GetProductVersion();
            }
        }

        public static BinaryVersion GetProductVersion()
        {
            if (null != productVersion) return productVersion;

            var executingAssemblyName = Assembly.GetExecutingAssembly().GetName();
            productVersion = BinaryVersion.FromString(executingAssemblyName.Version.ToString());

            return productVersion;
        }

        /// <summary>
        ///     Obtains available update version string 
        /// </summary>
        public BinaryVersion AvailableVersion
        {
            get
            {
                // Dirty patch: A version is available only when the update has been downloaded.
                // This causes the UI to display the update button only after the download has
                // completed.
                return downloadedUpdateInfo == null
                    ? ProductVersion : updateInfo.Version;
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
            }
        }

        public IAppVersionInfo UpdateInfo
        {
            get { return updateInfo; }
            set
            {
                if (value != null)
                {
                    OnLog(new LogEventArgs(string.Format(Properties.Resources.UpdateAvailable, value.Version), LogLevel.Console));
                }

                updateInfo = value;
                RaisePropertyChanged("UpdateInfo");
            }
        }

        /// <summary>
        ///     Dirty patch: Set to the value of UpdateInfo once the new update installer has been
        ///     downloaded.
        /// </summary>
        public IAppVersionInfo DownloadedUpdateInfo
        {
            get { return downloadedUpdateInfo; }
            set
            {
                downloadedUpdateInfo = value;
                RaisePropertyChanged("DownloadedUpdateInfo");
            }
        }

        public bool IsUpdateAvailable
        {
            get
            {
                //Update is not available unitl it's downloaded
                if(DownloadedUpdateInfo==null)
                    return false;

                return ForceUpdate || AvailableVersion > ProductVersion;
            }
        }

        /// <summary>
        /// This flag is available via the debug menu to
        /// allow the update manager to check for newer daily 
        /// builds as well.
        /// </summary>
        public bool CheckNewerDailyBuilds
        {
            get { return Configuration.CheckNewerDailyBuild; }
            set
            {
                if (!Configuration.CheckNewerDailyBuild && value)
                {
                    CheckForProductUpdate(new UpdateRequest(new Uri(Configuration.DownloadSourcePath), this));
                }
                Configuration.CheckNewerDailyBuild = value;
                RaisePropertyChanged("CheckNewerDailyBuilds");
            }
        }

        /// <summary>
        /// Apply the most recent update, regardless
        /// of whether it is newer than the current version.
        /// </summary>
        public bool ForceUpdate
        {
            get { return Configuration.ForceUpdate; }
            set
            {
                if (!Configuration.ForceUpdate && value)
                {
                    // do a check
                    CheckForProductUpdate(new UpdateRequest(new Uri(Configuration.DownloadSourcePath), this));
                }
                Configuration.ForceUpdate = value;
                RaisePropertyChanged("ForceUpdate");
            }
        }

        /// <summary>
        /// Returns the configuration settings.
        /// </summary>
        public IUpdateManagerConfiguration Configuration
        {
            get 
            {
                return configuration ?? (configuration = UpdateManagerConfiguration.GetSettings(null, this));
            }
        }

        #endregion

        public UpdateManager(IUpdateManagerConfiguration configuration)
        {
            this.configuration = configuration;
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
                        OnLog(new LogEventArgs(Properties.Resources.UpdateDownloadStarted, LogLevel.Console));

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
            OnLog(new LogEventArgs("RequestUpdateVersionInfo", LogLevel.File));
            OnLog(new LogEventArgs(Properties.Resources.RequestingVersionUpdate, LogLevel.Console));

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
                OnLog(new LogEventArgs(String.Format(Properties.Resources.CouldNotGetUpdateData, request.Path), LogLevel.Console));
                versionCheckInProgress = false;
                return;
            }

            var latestBuildFilePath = GetLatestBuildFromS3(request, CheckNewerDailyBuilds);
            if (string.IsNullOrEmpty(latestBuildFilePath))
            {
                OnLog(new LogEventArgs(Properties.Resources.CouldNotGetLatestBuild, LogLevel.Console));
                versionCheckInProgress = false;
                return;
            }

            // Strip the build number from the file name.
            // DynamoInstall0.7.0 becomes 0.7.0. Build a version
            // and compare it with the current product version.

            var latestBuildDownloadUrl = Path.Combine(Configuration.DownloadSourcePath, latestBuildFilePath);
            var latestBuildSignatureUrl = Path.Combine(
                Configuration.SignatureSourcePath,
                Path.GetFileNameWithoutExtension(latestBuildFilePath) + ".sig");

            BinaryVersion latestBuildVersion;
            var latestBuildTime = new DateTime();

            bool useStable = false;
            if (IsStableBuild(Configuration.InstallerNameBase, latestBuildFilePath))
            {
                useStable = true;
                latestBuildVersion = GetBinaryVersionFromFilePath(Configuration.InstallerNameBase, latestBuildFilePath);
            }
            else if (IsDailyBuild(Configuration.InstallerNameBase, latestBuildFilePath) || IsDailyBuild(OLD_DAILY_INSTALL_NAME_BASE, latestBuildFilePath))
            {
                latestBuildTime = GetBuildTimeFromFilePath(Configuration.InstallerNameBase, latestBuildFilePath);
                latestBuildVersion = GetCurrentBinaryVersion();
            }
            else
            {
                OnLog(new LogEventArgs(Properties.Resources.PathNotRegconizableAsStableOrDailyBuild, LogLevel.Console));
                versionCheckInProgress = false;
                return;
            }

            // Check the last downloaded update. If it's the same or newer as the 
            // one found on S3, then just set the update information to that one
            // and bounce.

            //if (ExistingUpdateIsNewer())
            //{
            //    logger.Log(string.Format("Using previously updated download {0}", dynamoModel.PreferenceSettings.LastUpdateDownloadPath));
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
                        OnLog(new LogEventArgs(Properties.Resources.DynamoUpToDate, LogLevel.Console));
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
                        OnLog(new LogEventArgs(Properties.Resources.DynamoUpToDate, LogLevel.Console));
                    }
                }
            }

            versionCheckInProgress = false;
        }

        public void QuitAndInstallUpdate()
        {
            OnLog(new LogEventArgs("UpdateManager.QuitAndInstallUpdate-Invoked", LogLevel.File));

            if (ShutdownRequested != null)
                ShutdownRequested(this);
        }

        public void HostApplicationBeginQuit()
        {
            // Double check that the updater path is not null and that there
            // exists a file at that location on disk.
            // Although this updater is stored in a temp directory,
            // and the user wouldn't have come across it, there's the
            // outside chance that it was deleted. Update cannot
            // continue without this file.

            if (string.IsNullOrEmpty(UpdateFileLocation) || !File.Exists(UpdateFileLocation))
                return;

            var currDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var updater = Path.Combine(currDir, INSTALLUPDATE_EXE);
            
            // Double check that that the updater program exists.
            // This program lives in the users's base Dynamo directory. If 
            // it doesn't exist, we can't run the update.

            if (!File.Exists(updater)) 
                return;

            var p = new Process
            {
                StartInfo =
                {
                    FileName = updater,
                    Arguments = UpdateFileLocation,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            if (hostApplicationProcessId != -1)
            {
                p.StartInfo.Arguments += " " + hostApplicationProcessId;
            }
            p.Start();
        }

        public void RegisterExternalApplicationProcessId(int id)
        {
            hostApplicationProcessId = id;
        }

        #endregion

        #region Private Event Handlers

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            versionCheckInProgress = false;

            if (e == null)
                return;

            string errorMessage = ((null == e.Error) ? "Successful" : e.Error.Message);
            OnLog(new LogEventArgs(Properties.Resources.UpdateDownloadComplete, LogLevel.Console));
            OnLog(new LogEventArgs(errorMessage, LogLevel.File));

            UpdateFileLocation = string.Empty;

            if (e.Error != null)
                return;

            // Dirty patch: this ensures that we have a property that reflects the update status 
            // only after the update has been downloaded.
            DownloadedUpdateInfo = UpdateInfo;

            UpdateFileLocation = (string)e.UserState;
            OnLog(new LogEventArgs("Update download complete.", LogLevel.Console));

            if (null != UpdateDownloaded)
                UpdateDownloaded(this, new UpdateDownloadedEventArgs(e.Error, UpdateFileLocation));
        }

        public void OnLog(LogEventArgs args)
        {
            if (Log != null)
            {
                Log(args);
            }
        }

        #endregion

        #region Private Class Helper Methods

        /// <summary>
        /// Get the file name of the latest build on S3
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private string GetLatestBuildFromS3(IAsynchronousRequest request, bool checkDailyBuilds)
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
                    OnLog(new LogEventArgs(e, LogLevel.Console));
                    return null;
                }
            }

            // Reads filenames from S3, and pulls out those which include 
            // DynamoInstall, and optionally, those that include DynamoDailyInstall.
            // Order the results according to their LastUpdated field.

            var bucketresult = doc.Element(ns + "ListBucketResult");

            if (bucketresult == null)
            {
                return null;
            }

            var builds = bucketresult.Descendants(ns + "LastModified").
                OrderByDescending(x => DateTime.Parse(x.Value)).
                Where(x => x.Parent.Value.Contains(Configuration.InstallerNameBase) || x.Parent.Value.Contains(OLD_DAILY_INSTALL_NAME_BASE)).
                Select(x => x.Parent);


            var xElements = builds as XElement[] ?? builds.ToArray();
            if (!xElements.Any())
            {
                return null;
            }

            var fileNames = xElements.Select(x => x.Element(ns + "Key").Value);

            string latestBuild = string.Empty;
            latestBuild = checkDailyBuilds ?
                fileNames.FirstOrDefault(x => IsDailyBuild(Configuration.InstallerNameBase, x) || IsDailyBuild(OLD_DAILY_INSTALL_NAME_BASE, x)) :
                fileNames.FirstOrDefault(x => IsStableBuild(Configuration.InstallerNameBase, x));

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
            return BinaryVersion.FromString(string.Format("{0}.{1}.{2}", v.Major, v.Minor, v.Build));
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
            var index = filePath.IndexOf(installNameBase, StringComparison.Ordinal);
            if (index < 0)
                return null;

            // Skip past the 'installNameBase' since we are only interested 
            // in getting the version numbers that come after the base name.
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var version = fileName.Substring(index + installNameBase.Length);

            var splits = version.Split(new [] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (splits.Count() < 3) // This can be 4 if it includes revision number.
                return null;

            ushort major, minor, build;
            if (!ushort.TryParse(splits[0], out major))
                return null;
            if (!ushort.TryParse(splits[1], out minor))
                return null;
            if (!ushort.TryParse(splits[2], out build))
                return null;

            return BinaryVersion.FromString(string.Format("{0}.{1}.{2}.0", major, minor, build));
        }

        private void SetUpdateInfo(BinaryVersion latestBuildVersion, string latestBuildDownloadUrl, string signatureUrl)
        {
            UpdateInfo = new AppVersionInfo()
            {
                Version = latestBuildVersion,
                VersionInfoURL = Configuration.DownloadSourcePath,
                InstallerURL = latestBuildDownloadUrl,
                SignatureURL = signatureUrl
            };
        }

        /// <summary>
        /// Check if a file name is a daily build.
        /// </summary>
        /// <param name="installNameBase"></param>
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
        /// <param name="installNameBase"></param>
        /// <param name="fileName"></param>
        /// <returns>True if this is a stable build, otherwise false.</returns>
        internal static bool IsStableBuild(string installNameBase, string fileName)
        {
            if (!fileName.Contains(installNameBase))
            {
                return false;
            }
            return !IsDailyBuild(installNameBase, fileName);
        }

        /// <summary>
        /// Async call to request downloading a file from web.
        /// This call raises UpdateDownloaded event notification.
        /// </summary>
        /// <param name="url">Web URL for file to download.</param>
        /// <param name="version">The version of package that is to be downloaded.</param>
        /// <param name="tempPath">Temp folder path where the update package
        /// to be downloaded.</param>
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
        /// <param name="url">Signature file url for download.</param>
        /// <param name="tempPath">Temp folder path where the signature file 
        /// to be downloaded.</param>
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
                OnLog(new LogEventArgs(string.Format(Properties.Resources.UpdateDownloadProgress, e.ProgressPercentage), LogLevel.Console));
                currentDownloadProgress = e.ProgressPercentage;
            }
        }

        #endregion

        /// <summary>
        /// Checks for the product update by requesting for update version info 
        /// from configured download source path. This method will skip the 
        /// update check if a newer version of the product is already installed.
        /// </summary>
        /// <param name="manager">Update manager instance using which product
        /// update check nees to be done.</param>
        internal static void CheckForProductUpdate(IUpdateManager manager)
        {
            //If we already have higher version installed, don't look for product update.
            if(manager.Configuration.DynamoLookUp != null && manager.Configuration.DynamoLookUp.LatestProduct > manager.ProductVersion)
                return;

            var downloadUri = new Uri(manager.Configuration.DownloadSourcePath);
            manager.CheckForProductUpdate(new UpdateRequest(downloadUri, manager));
        }
    }

    /// <summary>
    /// Lookup for installed products
    /// </summary>
    public abstract class DynamoLookUp : IDynamoLookUp
    {
        /// <summary>
        /// Gets the version of latest product
        /// </summary>
        public BinaryVersion LatestProduct { get { return GetLatestInstallVersion(); } }

        /// <summary>
        /// Locates DynamoCore.dll at given install path and gets file version
        /// </summary>
        /// <param name="installPath">Dynamo install path</param>
        /// <returns>Dynamo version if valid Dynamo exists else null</returns>
        public virtual Version GetDynamoVersion(string installPath)
        {
            if(!Directory.Exists(installPath))//null or empty installPath will return false
                return null;

            var filePath = Directory.GetFiles(installPath, "*DynamoCore.dll").FirstOrDefault();
            return String.IsNullOrEmpty(filePath) ? null : Version.Parse(FileVersionInfo.GetVersionInfo(filePath).FileVersion);
        }

        /// <summary>
        /// Gets all dynamo install path on the system by looking into the Windows registry. 
        /// </summary>
        /// <returns>List of Dynamo install path</returns>
        public abstract IEnumerable<string> GetDynamoInstallLocations();
        
        private BinaryVersion GetLatestInstallVersion()
        {
            var dynamoInstallations = GetDynamoInstallLocations();
            if(null == dynamoInstallations)
                return null;

            var latestVersion =
                dynamoInstallations.Select(GetDynamoVersion).OrderBy(s => s).LastOrDefault();
            return latestVersion == null ? null : BinaryVersion.FromString(latestVersion.ToString());
        }
    }
}
