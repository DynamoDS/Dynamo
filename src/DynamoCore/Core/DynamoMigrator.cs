using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Dynamo.Interfaces;
using Dynamo.Models;
using ProtoCore.Lang;

namespace Dynamo.Core
{
    struct FileVersion : IComparable<FileVersion>
    {
        public readonly int MajorPart;
        public readonly int MinorPart;

        public FileVersion(int majorPart, int minorPart)
        {
            MajorPart = majorPart;
            MinorPart = minorPart;
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
    /// </summary>
    internal class DynamoMigratorBase
    {
        protected readonly string rootFolder;

        #region virtual properties
        /// <summary>
        /// Returns the path to the user AppData directory for the current version
        /// </summary>
        protected virtual string CurrentVersionPath
        {
            get
            {
                return Path.Combine(rootFolder, String.Format("{0}.{1}", DynamoVersion.MajorPart, DynamoVersion.MinorPart));
            }
        }

        protected virtual string PackagesDirectory
        {
            get
            {
                return Path.Combine(this.CurrentVersionPath, PathManager.PackagesDirectoryName);
            }
        }

        protected virtual string DefinitionsDirectory
        {
            get
            {
                return Path.Combine(this.CurrentVersionPath, PathManager.DefinitionsDirectoryName);
            }
        }

        protected virtual string PreferenceSettingsFilePath
        {
            get
            {
                const string preferenceSettingsFileName = PathManager.PreferenceSettingsFileName;
                var preferenceSettingsFilePath = Path.Combine(CurrentVersionPath, preferenceSettingsFileName);

                return File.Exists(preferenceSettingsFilePath) ? preferenceSettingsFilePath : string.Empty;
            }
        }

        protected virtual FileVersion DynamoVersion
        {
            get
            {
                // Make the default Dynamo version 0.7
                // as we only migrate from Dynamo 0.7 onwards
                return new FileVersion(0, 7);
            }
        }

        #endregion

        protected DynamoMigratorBase(string rootFolder)
        {
            this.rootFolder = rootFolder;
        }

        #region virtual methods

        /// <summary>
        /// Reads preference settings from input file and deserializes it into 
        /// PreferenceSettings object in memory
        /// This function can be overridden by version specific migrator classes
        /// </summary>
        /// <param name="sourceMigrator"> source migrator version from which to migrate from </param>
        /// <returns> returns the deserialized settings object </returns>
        protected virtual PreferenceSettings WritePreferences(DynamoMigratorBase sourceMigrator)
        {
            string preferenceSettingsFilePath = sourceMigrator.PreferenceSettingsFilePath;
            PreferenceSettings settings = null;
            if (string.IsNullOrEmpty(preferenceSettingsFilePath))
                return null;

            try
            {
                var serializer = new XmlSerializer(typeof(PreferenceSettings));

                using (var fs = new FileStream(preferenceSettingsFilePath, FileMode.Open, FileAccess.Read))
                {
                    settings = serializer.Deserialize(fs) as PreferenceSettings;
                    fs.Close(); // Release file lock
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return settings;
        }

        /// <summary>
        /// Migrates packages from source migrator version to current version
        /// This function can be overridden by version specific migrator classes
        /// </summary>
        /// <param name="sourceMigrator"> source migrator version from which to migrate from </param>
        protected virtual void MigratePackages(DynamoMigratorBase sourceMigrator)
        {
            Copy(sourceMigrator.PackagesDirectory, this.PackagesDirectory);
        }

        /// <summary>
        /// Migrates custom node definitions from source migrator version to current version
        /// This function can be overridden by version specific migrator classes
        /// </summary>
        /// <param name="sourceMigrator"> source migrator version from which to migrate from </param>
        protected virtual void MigrateDefinitions(DynamoMigratorBase sourceMigrator)
        {
            Copy(sourceMigrator.DefinitionsDirectory, this.DefinitionsDirectory);
        }

        #endregion

        /// <summary>
        /// Migrates preference settings and copies packages and custom node 
        /// definitions from the last but one version to the currently installed Dynamo version
        /// </summary>
        /// <returns> preference settings read from preference settings file </returns>
        public static PreferenceSettings MigrateBetweenDynamoVersions(IPathManager pathManager)
        {
            var userDataDir = pathManager.UserDataDirectory;
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var currentVersionInfo = FileVersionInfo.GetVersionInfo(assemblyPath);

            // No migration required if the current version is <= version 0.7
            if (currentVersionInfo.FileMajorPart == 0 &&
                currentVersionInfo.FileMinorPart <= 7)
                return null;

            var versions = GetInstalledVersions(userDataDir).ToList();
            if (versions.Count() < 2)
                return null; // No need for migration

            var previousVersion = versions[1];
            var currentVersion = versions[0];
            Debug.Assert(currentVersion.MajorPart == currentVersionInfo.FileMajorPart
                && currentVersion.MinorPart == currentVersionInfo.FileMinorPart);

            return Migrate(userDataDir, previousVersion, currentVersion);
        }

        #region static APIs

        /// <summary>
        /// Get a list of file version objects given a root folder. Assuming the 
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
        internal static IEnumerable<FileVersion> GetInstalledVersions(string rootFolder)
        {
            if (rootFolder == null)
                throw new ArgumentNullException("rootFolder");

            var fileVersions = new List<FileVersion>();
            if (!Directory.Exists(rootFolder))
                throw new DirectoryNotFoundException("rootFolder");

            var subDirs = Directory.EnumerateDirectories(rootFolder);
            foreach (var subDir in subDirs)
            {
                var dirName = new DirectoryInfo(subDir).Name;

                var versions = dirName.Split('.');
                if (versions.Length < 2)
                    continue;

                int majorVersion;
                if (!Int32.TryParse(versions[0], out majorVersion))
                    continue;

                int minorVersion;
                if (Int32.TryParse(versions[1], out minorVersion))
                {
                    fileVersions.Add(new FileVersion(majorVersion, minorVersion));
                }
            }
            fileVersions.Sort();

            return fileVersions;
        }

        /// <summary>
        /// Given a root user data directory, this migrates user preferences,
        /// packages and custom node definition files from one installed version
        /// into another
        /// </summary>
        /// <param name="userDataDir"> root user data directory </param>
        /// <param name="fromVersion"> source Dynamo version from which to migrate </param>
        /// <param name="toVersion"> target Dynamo version into which to migrate </param>
        /// <returns> preference settings read from preference settings file </returns>
        internal static PreferenceSettings Migrate(string userDataDir, FileVersion fromVersion, FileVersion toVersion)
        {
            // Create concrete DynamoMigratorBase object using previousVersion and reflection
            var sourceMigrator = CreateMigrator(userDataDir, fromVersion);
            Debug.Assert(sourceMigrator != null);

            // get migrator object for current version
            var targetMigrator = CreateMigrator(userDataDir, toVersion);
            Debug.Assert(targetMigrator != null);
            var preferenceSettings = targetMigrator.WritePreferences(sourceMigrator);

            targetMigrator.MigratePackages(sourceMigrator);

            targetMigrator.MigrateDefinitions(sourceMigrator);

            return preferenceSettings;
        }

        /// <summary>
        /// Given a root user directory, this creates an instance of the migrator
        /// depending on the input version using reflection. Returns the default
        /// migrator (DynamoMigratorBase) if version specific migrator does not exist
        /// </summary>
        /// <param name="userDataDir"> root user data directory </param>
        /// <param name="version"> input version for which migrator instance is created </param>
        /// <returns> instance of migrator specific to input version. </returns>
        internal static DynamoMigratorBase CreateMigrator(string userDataDir, FileVersion version)
        {
            var className = "Dynamo.Core.DynamoMigrator" + version.MajorPart + version.MinorPart;
            const string baseClass = "Dynamo.Core.DynamoMigratorBase";

            var args = new object[] { userDataDir };
            var type = Assembly.GetExecutingAssembly().GetType(className);
            var baseType = Assembly.GetExecutingAssembly().GetType(baseClass);

            if(type != null)
                return (DynamoMigratorBase)Activator.CreateInstance(type, args);
            else
            {
                return (DynamoMigratorBase) Activator.CreateInstance(baseType, args);
            }
        }

        #endregion

        #region private methods

        // Reference: https://msdn.microsoft.com/en-us/library/system.io.directoryinfo.aspx
        private static void Copy(string sourceDirectory, string targetDirectory)
        {
            var diSource = new DirectoryInfo(sourceDirectory);
            var diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists; if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        #endregion

    }

    internal class DynamoMigrator07 : DynamoMigratorBase
    {
        protected override FileVersion DynamoVersion { get{return new FileVersion(0, 7);} }

        protected DynamoMigrator07(string rootFolder) : base(rootFolder)
        {
        }

        protected override PreferenceSettings WritePreferences(DynamoMigratorBase sourceMigrator)
        {
            throw new NotImplementedException();
        }

        protected override void MigratePackages(DynamoMigratorBase sourceMigrator)
        {
            throw new NotImplementedException();
        }

        protected override void MigrateDefinitions(DynamoMigratorBase sourceMigrator)
        {
            throw new NotImplementedException();
        }
    }

    internal class DynamoMigrator08 : DynamoMigratorBase
    {
        protected override FileVersion DynamoVersion { get { return new FileVersion(0, 8); } }

        protected DynamoMigrator08(string rootFolder) : base(rootFolder)
        {
        }

        // These overridden functions are strictly not required as they simply duplicate base class functionality
        // They are implemented here simply to serve as templates for further class extensions from DynamoMigratorBase
        protected override PreferenceSettings WritePreferences(DynamoMigratorBase sourceMigrator)
        {
            return base.WritePreferences(sourceMigrator);
        }

        protected override void MigratePackages(DynamoMigratorBase sourceMigrator)
        {
            base.MigratePackages(sourceMigrator);
        }

        protected override void MigrateDefinitions(DynamoMigratorBase sourceMigrator)
        {
            base.MigrateDefinitions(sourceMigrator);
        }
    }

}
