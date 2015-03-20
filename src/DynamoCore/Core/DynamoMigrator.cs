using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Serialization;
using Dynamo.Interfaces;
using Dynamo.Models;
using ProtoCore.AST.AssociativeAST;
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
    /// Constructors of classes derived from DynamoMigratorBase need to be public
    /// as their instances are created through reflection.
    /// </summary>
    internal class DynamoMigratorBase
    {
        protected readonly IPathManager pathManager;

        #region virtual properties

        protected virtual string PackagesDirectory
        {
            get
            {
                return this.pathManager.PackagesDirectory;
            }
        }

        protected virtual string DefinitionsDirectory
        {
            get
            {
                return this.pathManager.UserDefinitions;
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

        protected DynamoMigratorBase(IPathResolver pathResolver, FileVersion version)
        {
            this.pathManager = new PathManager(new PathManagerParams()
            {
                MajorFileVersion = version.MajorPart,
                MinorFileVersion = version.MinorPart,
                PathResolver = pathResolver
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
            Copy(sourceMigrator.PackagesDirectory, this.PackagesDirectory);
            Copy(sourceMigrator.DefinitionsDirectory, this.DefinitionsDirectory);

            this.PreferenceSettings = sourceMigrator.PreferenceSettings;
            return this;
        }

        #endregion

        /// <summary>
        /// Migrates preference settings and copies packages and custom node 
        /// definitions from the last but one version to the currently installed Dynamo version
        /// </summary>
        /// <param name="pathManager"></param>
        /// <param name="pathResolver"></param>
        /// <returns>new migrator instance after migration</returns>
        public static DynamoMigratorBase MigrateBetweenDynamoVersions(IPathManager pathManager, IPathResolver pathResolver)
        {
            // No migration required if the current version is <= version 0.7
            if (pathManager.MajorFileVersion == 0 &&
                pathManager.MinorFileVersion <= 7)
                return null;

            var userDataDir = Path.GetDirectoryName(pathManager.UserDataDirectory);
            var versions = GetInstalledVersions(userDataDir).ToList();
            if (versions.Count() < 2)
                return null; // No need for migration

            var previousVersion = versions[1];
            var currentVersion = versions[0];
            Debug.Assert(currentVersion.MajorPart == pathManager.MajorFileVersion
                && currentVersion.MinorPart == pathManager.MinorFileVersion);

            return Migrate(pathResolver, previousVersion, currentVersion);
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
        public static IEnumerable<FileVersion> GetInstalledVersions(string rootFolder)
        {
            if(string.IsNullOrEmpty(rootFolder))
                throw new ArgumentNullException("rootFolder");

            var fileVersions = new List<FileVersion>();
            if(!Directory.Exists(rootFolder))
                throw new DirectoryNotFoundException("rootFolder");

            var subDirs = Directory.EnumerateDirectories(rootFolder);
            foreach (var subDir in subDirs)
            {
                var dirName = new DirectoryInfo(subDir).Name;

                var versions = dirName.Split('.');
                if(versions.Length < 2)
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
        /// <param name="pathResolver"></param>
        /// <param name="fromVersion"> source Dynamo version from which to migrate </param>
        /// <param name="toVersion"> target Dynamo version into which to migrate </param>
        /// <returns> new migrator instance after migration </returns>
        internal static DynamoMigratorBase Migrate(IPathResolver pathResolver, FileVersion fromVersion, FileVersion toVersion)
        {
            // Create concrete DynamoMigratorBase object using previousVersion and reflection
            var sourceMigrator = CreateMigrator(pathResolver, fromVersion);
            Debug.Assert(sourceMigrator != null);

            // get migrator object for current version
            var targetMigrator = CreateMigrator(pathResolver, toVersion);
            Debug.Assert(targetMigrator != null);

            return targetMigrator.MigrateFrom(sourceMigrator); 
        }

        /// <summary>
        /// Given a root user directory, this creates an instance of the migrator
        /// depending on the input version using reflection. Returns the default
        /// migrator (DynamoMigratorBase) if version specific migrator does not exist
        /// </summary>
        /// <param name="pathResolver"></param>
        /// <param name="version"> input version for which migrator instance is created </param>
        /// <returns> instance of migrator specific to input version. </returns>
        internal static DynamoMigratorBase CreateMigrator(IPathResolver pathResolver, FileVersion version)
        {
            var className = "Dynamo.Core.DynamoMigrator" + version.MajorPart + version.MinorPart;

            var args = new object[] { pathResolver, version };
            var type = Assembly.GetExecutingAssembly().GetType(className);

            if(type != null)
                return (DynamoMigratorBase)Activator.CreateInstance(type, args);

            return new DynamoMigratorBase(pathResolver, version);
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
        public DynamoMigrator07(IPathResolver pathResolver, FileVersion version)
            : base(pathResolver, version)
        {
        }

        // These overridden functions are strictly not required as they simply duplicate base class functionality
        // They are implemented here simply to serve as templates for further class extensions from DynamoMigratorBase
        protected override PreferenceSettings ReadPreferences()
        {
            return base.ReadPreferences();
        }

        protected override DynamoMigratorBase MigrateFrom(DynamoMigratorBase sourceMigrator)
        {
            // We should not be migrating from a previous version into Dynamo 0.7
            throw new NotImplementedException();
        }
    }

    internal class DynamoMigrator08 : DynamoMigratorBase
    {
        public DynamoMigrator08(IPathResolver pathResolver, FileVersion version)
            : base(pathResolver, version)
        {
        }

        // These overridden functions are strictly not required as they simply duplicate base class functionality
        // They are implemented here simply to serve as templates for further class extensions from DynamoMigratorBase
        protected override PreferenceSettings ReadPreferences()
        {
            return base.ReadPreferences();
        }

        protected override DynamoMigratorBase MigrateFrom(DynamoMigratorBase sourceMigrator)
        {
            return base.MigrateFrom(sourceMigrator);
        }
    }
}
