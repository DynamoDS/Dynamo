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
    struct FileVersion
    {
        public readonly int MajorPart;
        public readonly int MinorPart;

        public FileVersion(int majorPart, int minorPart)
        {
            MajorPart = majorPart;
            MinorPart = minorPart;
        }
    }

    /// <summary>
    /// Base class for all versions of Dynamo classes relating to
    /// migrating preferences, packages and definitions from older versions
    /// </summary>
    internal class DynamoMigratorBase
    {
        protected readonly string rootFolder;

        /// <summary>
        /// Returns the path to the user AppData directory for the current version
        /// </summary>
        protected string CurrentVersionPath
        {
            get
            {
                return Path.Combine(rootFolder, String.Format("{0}.{1}", DynamoVersion.MajorPart, DynamoVersion.MinorPart));
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

        protected DynamoMigratorBase(string rootFolder)
        {
            this.rootFolder = rootFolder;
        }

        /// <summary>
        /// Reads the preference settings for the currently installed Dynamo version
        /// </summary>
        /// <returns> preference settings full file path </returns>
        protected virtual string ReadPreferences()
        {
            const string preferenceSettingsFileName = PathManager.PreferenceSettingsFileName;
            var preferenceSettingsFilePath = Path.Combine(CurrentVersionPath, preferenceSettingsFileName);

            return File.Exists(preferenceSettingsFilePath) ? preferenceSettingsFilePath : string.Empty;
        }

        /// <summary>
        /// Reads preference settings from input file and deserializes it into 
        /// PreferenceSettings object in memory
        /// </summary>
        /// <param name="preferenceSettingsFilePath"></param>
        /// <returns> returns the deserialized settings object </returns>
        protected virtual PreferenceSettings WritePreferences(string preferenceSettingsFilePath)
        {
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
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            return settings;
        }

        /// <summary>
        /// Migrates preference settings and copies packages and custom node 
        /// definitions from older versions to the currently installed Dynamo version
        /// </summary>
        /// <returns> preference settings read from preference settings file </returns>
        public static PreferenceSettings MigrateBetweenDynamoVersions(IPathManager pathManager)
        {
            var rootFolder = pathManager.UserDataDirectory;
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var currentVersionInfo = FileVersionInfo.GetVersionInfo(assemblyPath);

            // No migration required if the current version is <= version 0.7
            if (currentVersionInfo.FileMajorPart == 0 &&
                currentVersionInfo.FileMinorPart <= 7)
                return null;

            var previousVersion = FindPreviousVersion(rootFolder, currentVersionInfo);

            // No migration required if no previous version exists
            if (previousVersion.MajorPart == currentVersionInfo.FileMajorPart &&
                previousVersion.MinorPart == currentVersionInfo.FileMinorPart)
                return null;

            // Create concrete DynamoMigratorBase object using previousVersion and reflection
            var sourceMigrator = CreateMigrator(rootFolder, previousVersion.MajorPart, previousVersion.MinorPart);
            Debug.Assert(sourceMigrator != null);
            var preferenceSettingsFile = sourceMigrator.ReadPreferences();

            // get migrator object for current version
            var targetMigrator = CreateMigrator(rootFolder, currentVersionInfo.FileMajorPart, currentVersionInfo.FileMinorPart);
            Debug.Assert(targetMigrator != null);
            var preferenceSettings = targetMigrator.WritePreferences(preferenceSettingsFile);

            var sourceDir = Path.Combine(sourceMigrator.CurrentVersionPath, PathManager.PackagesDirectoryName);
            var targetDir = Path.Combine(targetMigrator.CurrentVersionPath, PathManager.PackagesDirectoryName);
            Copy(sourceDir, targetDir);

            sourceDir = Path.Combine(sourceMigrator.CurrentVersionPath, PathManager.DefinitionsDirectoryName);
            targetDir = Path.Combine(targetMigrator.CurrentVersionPath, PathManager.DefinitionsDirectoryName);
            Copy(sourceDir, targetDir);

            return preferenceSettings;
        }

        private static FileVersion FindPreviousVersion(string rootFolder, FileVersionInfo currentVersionInfo)
        {
            int fileMajorPart = currentVersionInfo.FileMajorPart;
            int fileMinorPart = currentVersionInfo.FileMinorPart;
            while (fileMajorPart >= 0 && fileMinorPart > 7)
            {
                if (fileMinorPart == 0)
                {
                    fileMinorPart = 9;
                    fileMajorPart--;
                }
                else
                {
                    fileMinorPart--;
                }
                var previousVersionPath = Path.Combine(rootFolder, String.Format("{0}.{1}", fileMajorPart, fileMinorPart));
                if (Directory.Exists(previousVersionPath))
                    break;

            }
            return new FileVersion(fileMajorPart, fileMinorPart);
        }

        private static DynamoMigratorBase CreateMigrator(string rootFolder, int majorVersion, int minorVersion)
        {
            var className = "Dynamo.Core.DynamoMigrator" + majorVersion + minorVersion;
            const string baseClass = "Dynamo.Core.DynamoMigratorBase";

            var args = new object[] { rootFolder };
            var type = Assembly.GetExecutingAssembly().GetType(className);
            var baseType = Assembly.GetExecutingAssembly().GetType(baseClass);

            if(type != null)
                return (DynamoMigratorBase)Activator.CreateInstance(type, args);
            else
            {
                return (DynamoMigratorBase) Activator.CreateInstance(baseType);
            }
        }

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

    }

    internal class DynamoMigrator07 : DynamoMigratorBase
    {
        protected override FileVersion DynamoVersion { get{return new FileVersion(0, 7);} }

        protected DynamoMigrator07(string rootFolder) : base(rootFolder)
        {
        }
        
        protected override PreferenceSettings WritePreferences(string preferenceSettingsFilePath)
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

        protected override string ReadPreferences()
        {
            throw new NotImplementedException();
        }

        protected override PreferenceSettings WritePreferences(string preferenceSettingsFilePath)
        {
            return base.WritePreferences(preferenceSettingsFilePath);
        }
    }
}
