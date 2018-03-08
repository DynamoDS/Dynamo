using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Dynamo.Configuration;
using Dynamo.Interfaces;
using Dynamo.Models;
using Dynamo.Updates;

namespace Dynamo.Core
{
    struct FileVersion : IComparable<FileVersion>
    {
        public readonly int MajorPart;
        public readonly int MinorPart;
        public readonly string UserDataRoot;

        public FileVersion(int majorPart, int minorPart, string userdataRoot)
        {
            MajorPart = majorPart;
            MinorPart = minorPart;
            UserDataRoot = userdataRoot;
        }

        public static bool operator <(FileVersion thisVersion, FileVersion otherVersion)
        {
            return thisVersion.MajorPart < otherVersion.MajorPart ||
                   thisVersion.MajorPart == otherVersion.MajorPart && thisVersion.MinorPart < otherVersion.MinorPart;
        }

        public static bool operator >(FileVersion thisVersion, FileVersion otherVersion)
        {
            return thisVersion.MajorPart > otherVersion.MajorPart ||
                   thisVersion.MajorPart == otherVersion.MajorPart && thisVersion.MinorPart > otherVersion.MinorPart;
        }

        public static bool operator >=(FileVersion thisVersion, FileVersion otherVersion)
        {
            return thisVersion > otherVersion ||
                   thisVersion.MajorPart == otherVersion.MajorPart && thisVersion.MinorPart == otherVersion.MinorPart;
        }

        public static bool operator <=(FileVersion thisVersion, FileVersion otherVersion)
        {
            return thisVersion < otherVersion ||
                   thisVersion.MajorPart == otherVersion.MajorPart && thisVersion.MinorPart == otherVersion.MinorPart;
        }

        public int CompareTo(FileVersion other)
        {
            if (this > other)
                return -1;
            if (this < other)
                return 1;
            return 0;
        }
    }

    /// <summary>
    /// Base class for all versions of Dynamo classes relating to
    /// migrating preferences, packages and definitions from older versions
    /// For version specific migration, derived classes will override base class methods
    /// The naming convention for derived classes is "DynamoMigrator{MajorVersion}{MinorVersion}"
    /// For e.g. derived migrator class for verion 0.7 is "DynamoMigrator07"
    /// Derived migrator class for version 0.8 is "DynamoMigrator08" and so on.
    /// Constructors of classes derived from DynamoMigratorBase need to be public
    /// as their instances are created through reflection.
    /// </summary>
    internal class DynamoMigratorBase
    {
        protected readonly IPathManager pathManager;

        #region virtual properties

        protected virtual string UserDataDirectory
        {
            get
            {
                return pathManager.UserDataDirectory;
            }
        }

        protected virtual string PackagesDirectory
        {
            get
            {
                // Only return the default package directory.
                return pathManager.DefaultPackagesDirectory;
            }
        }

        protected virtual string DefinitionsDirectory
        {
            get
            {
                // Only return the default custom node directory.
                return pathManager.DefaultUserDefinitions;
            }
        }

        protected virtual string PreferenceSettingsFilePath
        {
            get
            {
                return this.pathManager.PreferenceFilePath;
            }
        }

        private PreferenceSettings preferenceSettings;
        public PreferenceSettings PreferenceSettings
        {
            get
            {
                return preferenceSettings ?? ReadPreferences();
            }
            set { preferenceSettings = value; }
        }

        #endregion

        protected DynamoMigratorBase(FileVersion version)
        {
            this.pathManager = new PathManager(new PathManagerParams()
            {
                MajorFileVersion = version.MajorPart,
                MinorFileVersion = version.MinorPart,
                PathResolver = new MigrationPathResolver(version.UserDataRoot)
            });
        }

        #region virtual methods

        /// <summary>
        /// Reads preference settings from input file and deserializes it into 
        /// PreferenceSettings object 
        /// This function can be overridden by version specific migrator classes
        /// </summary>
        /// <returns> returns the deserialized preference settings </returns>
        protected virtual PreferenceSettings ReadPreferences()
        {
            PreferenceSettings settings;
            if (string.IsNullOrEmpty(this.PreferenceSettingsFilePath))
                return null;

            var serializer = new XmlSerializer(typeof(PreferenceSettings));

            using (var fs = new FileStream(this.PreferenceSettingsFilePath, FileMode.Open, FileAccess.Read))
            {
                settings = serializer.Deserialize(fs) as PreferenceSettings;

                fs.Close(); // Release file lock
            }
            
            return settings;
        }

        /// <summary>
        /// Migrates preference settings, packages, custom node definitions, etc. 
        /// from source migrator version to current version.
        /// This function can be overridden by version specific migrator classes
        /// </summary>
        /// <param name="sourceMigrator"> source migrator version from which to migrate from </param>
        /// /// <returns>new migrator instance after migration</returns>
        protected virtual DynamoMigratorBase MigrateFrom(DynamoMigratorBase sourceMigrator)
        {
            PreferenceSettings = sourceMigrator.PreferenceSettings;
            if (PreferenceSettings == null) return this;

            // All preference settings are copied over including custom package folders
            // However if one of the custom folder locations points to the user data directory
            // of the previous version, it needs to be replaced with that of the current version
            PreferenceSettings.CustomPackageFolders.Clear();
            PreferenceSettings.CustomPackageFolders.Insert(0, UserDataDirectory);
            
            return this;
        }

        #endregion

        #region static APIs
        /// <summary>
        /// Migrates preference settings and copies packages and custom node 
        /// definitions from the last but one version to the currently installed Dynamo version
        /// </summary>
        /// <param name="pathManager"></param>
        /// <param name="dynamoLookup"></param>
        /// <returns>new migrator instance after migration</returns>
        public static DynamoMigratorBase MigrateBetweenDynamoVersions(IPathManager pathManager, IDynamoLookUp dynamoLookup = null)
        {
            //Get the current version from the current path manager user data directory.
            var currentVersion = GetInstallVersionFromUserDataFolder(pathManager.UserDataDirectory);
            var previousVersion = GetLatestVersionToMigrate(pathManager, dynamoLookup, currentVersion);
            
            if (!previousVersion.HasValue || previousVersion.Value.UserDataRoot == null)
                return null; //Don't have previous version for migration

            return Migrate(previousVersion.Value, currentVersion);
        }

        /// <summary>
        /// Returns the most recent version to migrate to the given current version.
        /// </summary>
        /// <param name="pathManager"></param>
        /// <param name="dynamoLookup"></param>
        /// <param name="currentVersion"></param>
        /// <returns>FileVersion?</returns>
        public static FileVersion? GetLatestVersionToMigrate(IPathManager pathManager, IDynamoLookUp dynamoLookup, FileVersion currentVersion)
        {
            var versions = GetInstalledVersions(pathManager, dynamoLookup);

            if (versions.Count() < 2)
                return null; // No need for migration


            foreach (var version in versions)
            {
                if (version < currentVersion) return version;
                
                if(version <= currentVersion 
                    && version.UserDataRoot != currentVersion.UserDataRoot)
                {
                    return version;
                }
            }
            return null;
        }

        
        /// <summary>
        /// Returns a list of file version objects given a root folder. Assuming the 
        /// following folders exist:
        /// 
        ///     e:\some\path\0.4
        ///     e:\some\path\0.75
        ///     e:\some\path\1.82
        /// 
        /// Calling this method with "e:\\some\\path" would return an ordered 
        /// list in the following way (from largest number to smallest number):
        /// 
        ///     { FileVersion(1, 82), FileVersion(0, 75), FileVersion(0, 4) }
        /// 
        /// </summary>
        /// <param name="rootFolder">The root folder under which versioned sub-
        /// folders are expected to be found. This argument must represent a valid
        /// local directory.</param>
        /// <returns>Return a list of FileVersion objects, ordered by newest 
        /// version to the oldest version. If no versioned sub-folder exists, then 
        /// the returned list is empty.</returns>
        /// <exception cref="System.ArgumentNullException">This exception is 
        /// thrown if rootFolder is null or an empty string.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">This exception
        /// is thrown if rootFolder points to an invalid directory.</exception>
        public static IEnumerable<FileVersion> GetInstalledVersions(string rootFolder)
        {
            return GetInstalledVersionsCore(() =>
                {
                    if (string.IsNullOrEmpty(rootFolder))
                        throw new ArgumentNullException("rootFolder");

                    if (!Directory.Exists(rootFolder))
                        throw new DirectoryNotFoundException("rootFolder");

                    return Directory.EnumerateDirectories(rootFolder);
                });
        }

        /// <summary>
        /// Returns list of FileVersion objects, given the IPathManager and 
        /// IDynamoLookUp objects. If a valid IDynamoLookUp interface object
        /// is passed, this method uses the lookup to get Dynamo user data locations.
        /// </summary>
        /// <param name="pathManager"></param>
        /// <param name="dynamoLookup"></param>
        /// <returns></returns>
        public static IEnumerable<FileVersion> GetInstalledVersions(IPathManager pathManager, IDynamoLookUp dynamoLookup)
        {
            return dynamoLookup != null
                ? GetInstalledVersionsCore(() => dynamoLookup.GetDynamoUserDataLocations())
                : GetInstalledVersions(Path.GetDirectoryName(pathManager.UserDataDirectory));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userDataFolders"></param>
        /// <returns></returns>
        private static IEnumerable<FileVersion> GetInstalledVersionsCore(Func<IEnumerable<string>> userDataFolders)
        {
            var folders = userDataFolders();
            return folders.Select(x => GetInstallVersionFromUserDataFolder(x)).OrderBy(x => x);
        }

        /// <summary>
        /// Returns FileVersion object of the Dynamo installation from the given
        /// user data folder path. Returns default(FileVersion) if file version
        /// can't be determined from the given folder path.
        /// </summary>
        /// <param name="folder">User data folder path</param>
        /// <returns>A valid or default FileVersion object</returns>
        public static FileVersion GetInstallVersionFromUserDataFolder(string folder)
        {
            var dirInfo = new DirectoryInfo(folder);
            var dirName = dirInfo.Name;

            var versions = dirName.Split('.');
            if (versions.Length < 2)
                return default(FileVersion);

            int majorVersion;
            if (!Int32.TryParse(versions[0], out majorVersion))
                return default(FileVersion);

            int minorVersion;
            if (Int32.TryParse(versions[1], out minorVersion))
            {
                return new FileVersion(majorVersion, minorVersion, dirInfo.Parent.FullName);
            }

            return default(FileVersion);
        }

        /// <summary>
        /// Given a root user data directory, this migrates user preferences,
        /// packages and custom node definition files from one installed version
        /// into another
        /// </summary>
        /// <param name="fromVersion"> source Dynamo version from which to migrate </param>
        /// <param name="toVersion"> target Dynamo version into which to migrate </param>
        /// <returns> new migrator instance after migration, null if there's no migration </returns>
        internal static DynamoMigratorBase Migrate(FileVersion fromVersion, FileVersion toVersion)
        {
            // Create concrete DynamoMigratorBase object using previousVersion and reflection
            var sourceMigrator = CreateMigrator(fromVersion);
            Debug.Assert(sourceMigrator != null);

            // get migrator object for current version
            var targetMigrator = CreateMigrator(toVersion);
            Debug.Assert(targetMigrator != null);

            bool isPackagesDirectoryEmpty = !Directory.EnumerateFileSystemEntries(targetMigrator.PackagesDirectory).Any();
            bool isDefinitionsDirectoryEmpty = !Directory.EnumerateFileSystemEntries(targetMigrator.DefinitionsDirectory).Any();

            // Migrate only if both packages and definitions directories are empty
            if (isPackagesDirectoryEmpty && isDefinitionsDirectoryEmpty)
            {
                DynamoModel.OnRequestMigrationStatusDialog(new SettingsMigrationEventArgs(
                    SettingsMigrationEventArgs.EventStatusType.Begin));

                return targetMigrator.MigrateFrom(sourceMigrator);
            }
            return null;
        }

        /// <summary>
        /// Given a FileVersion, this creates an instance of the migrator
        /// depending on the input version using reflection. Returns the default
        /// migrator (DynamoMigratorBase) if version specific migrator does not exist
        /// </summary>
        /// <param name="version"> input version for which migrator instance is created </param>
        /// <returns> instance of migrator specific to input version. </returns>
        internal static DynamoMigratorBase CreateMigrator(FileVersion version)
        {
            var className = "Dynamo.Core.DynamoMigrator" + version.MajorPart + version.MinorPart;

            var type = Assembly.GetExecutingAssembly().GetType(className);

            if (type != null)
            {
                var args = new object[] { version };
                return (DynamoMigratorBase)Activator.CreateInstance(type, args);
            }

            return new DynamoMigratorBase(version);
        }
        #endregion

    }


    class MigrationPathResolver : IPathResolver
    {
        private readonly IEnumerable<string> emptyList = Enumerable.Empty<string>();
        public MigrationPathResolver(string userDataRoot)
        {
            UserDataRootFolder = userDataRoot;
        }

        public IEnumerable<string> AdditionalResolutionPaths
        {
            get { return emptyList; }
        }

        public IEnumerable<string> AdditionalNodeDirectories
        {
            get { return emptyList; }
        }

        public IEnumerable<string> PreloadedLibraryPaths
        {
            get { return emptyList; }
        }

        public string UserDataRootFolder { get; private set; }

        public string CommonDataRootFolder
        {
            get { return string.Empty; }
        }
    }

}
