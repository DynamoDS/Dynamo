using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Dynamo.Models;
using ProtoCore.Lang;

namespace Dynamo.Core
{
    public struct FileVersion
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
    internal abstract class DynamoMigratorBase
    {
        protected readonly string rootFolder;

        /// <summary>
        /// Returns the path to the user AppData directory for the current version
        /// </summary>
        public string CurrentVersionPath
        {
            get
            {
                return Path.Combine(rootFolder, String.Format("{0}.{1}", DynamoVersion.MajorPart, DynamoVersion.MinorPart));
            }
        }

        public abstract FileVersion DynamoVersion { get; }
        
        protected DynamoMigratorBase()
        {
        }

        protected DynamoMigratorBase(string rootFolder)
        {
            this.rootFolder = rootFolder;
        }

        /// <summary>
        /// Reads the preference settings for the currently installed Dynamo version
        /// </summary>
        /// <returns> preference settings full file path </returns>
        public abstract string ReadPreferences();

        /// <summary>
        /// Reads preference settings from input file and deserializes it into 
        /// PreferenceSettings object in memory
        /// </summary>
        /// <param name="preferenceSettingsFilePath"></param>
        /// <returns> returns the deserialized settings object </returns>
        public abstract PreferenceSettings WritePreferences(string preferenceSettingsFilePath);
    }

    internal class DynamoMigrator07 : DynamoMigratorBase
    {
        public override FileVersion DynamoVersion
        {
            get { return new FileVersion(0, 7); }
        }
        
        public DynamoMigrator07()
        {
        }

        public DynamoMigrator07(string rootFolder) : base(rootFolder)
        {
        }
        
        public override string ReadPreferences()
        {
            const string preferenceSettingsFileName = "DynamoSettings.xml";
            var preferenceSettingsFilePath = Path.Combine(CurrentVersionPath, preferenceSettingsFileName);

            return File.Exists(preferenceSettingsFilePath) ? preferenceSettingsFilePath : string.Empty;
        }

        public override PreferenceSettings WritePreferences(string preferenceSettingsFilePath)
        {
            throw new NotImplementedException();
        }
    }

    internal class DynamoMigrator08 : DynamoMigratorBase
    {
        public override FileVersion DynamoVersion
        {
            get { return new FileVersion(0, 8); }
        }

        public DynamoMigrator08()
        {
        }

        public DynamoMigrator08(string rootFolder) : base(rootFolder)
        {
        }

        public override string ReadPreferences()
        {
            throw new NotImplementedException();
        }

        public override PreferenceSettings WritePreferences(string preferenceSettingsFilePath)
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
    }
}
