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
    internal abstract class DynamoMigratorBase
    {
        protected readonly string rootFolder;

        public string CurrentVersionPath
        {
            get
            {
                return Path.Combine(rootFolder, String.Format("{0}.{1}", MajorVersion, MinorVersion));
            }
        }

        public abstract int MinorVersion { get; }
        public abstract int MajorVersion { get; }

        protected DynamoMigratorBase()
        {
        }

        protected DynamoMigratorBase(string rootFolder)
        {
            this.rootFolder = rootFolder;
        }

        public abstract string ReadPreferences();

        public abstract PreferenceSettings WritePreferences(string preferenceSettingsFilePath);
    }

    internal class DynamoMigrator07 : DynamoMigratorBase
    {
        public override int MajorVersion { get { return 0; } }
        public override int MinorVersion { get { return 7; } }

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
        public override int MajorVersion { get { return 0; } }
        public override int MinorVersion { get { return 8; } }

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
