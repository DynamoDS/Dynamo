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
            if(rootFolder == null)
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
    }
}
