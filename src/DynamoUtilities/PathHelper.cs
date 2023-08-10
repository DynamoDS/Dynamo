using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Xml;
using Newtonsoft.Json;

namespace DynamoUtilities
{
    internal static class OSHelper
    {
#if NET6_0_OR_GREATER
        [SupportedOSPlatformGuard("windows")]
#endif
        public static bool IsWindows()
        {
#if NET6_0_OR_GREATER
            return OperatingSystem.IsWindows();
#else
            return true;// net48, assuming we will no deliver net48 on anything else but windows (also no more mono builds)
#endif

        }
    }
    public class PathHelper
    {
        private static readonly string sizeUnits = " KB";
        private const long KbConversionConstant = 1024;

        // This return an exception if any operation failed and the folder
        // wasn't created. It's the responsibility of the caller of this function
        // to check whether the folder creation is successful or not.
        public static Exception CreateFolderIfNotExist(string folderPath)
        {
            try
            {
                // When network path is access denied, the Directory.Exits however still 
                // return true.
                // EnumerateDirectories operation is additional check
                // to catch exception for network path.
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                Directory.EnumerateDirectories(folderPath);
            }
            catch (IOException ex) { return ex; }
            catch (ArgumentException ex) { return ex; }
            catch (UnauthorizedAccessException ex) { return ex; }

            return null;
        }

        /// <summary>
        /// Checks if the file path string is valid and file exist.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns></returns>
        public static bool IsValidPath(string filePath)
        {
            return (!string.IsNullOrEmpty(filePath) && (File.Exists(filePath)));
        }

        /// <summary>
        /// Check if user has readonly privilege to the folder path.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns></returns>
        public static bool IsReadOnlyPath(string filePath)
        {
            if (IsValidPath(filePath))
            {
                FileInfo Finfo = new FileInfo(filePath);
                // We mark the path read only when
                // 1. file read-only
                // 2. user does not have write access to the folder

                // We have no cross platform Directory access writes APIs.
                bool hasWritePermissionOnDir = OSHelper.IsWindows() ? HasWritePermissionOnDir(Finfo.Directory.ToString()) : true;
                return Finfo.IsReadOnly || !hasWritePermissionOnDir;
            }
            else
                return false;
        }

        /// <summary>
        /// Returns whether current user has write access to the folder path.
        /// </summary>
        /// <param name="folderPath">Folder path</param>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        public static bool HasWritePermissionOnDir(string folderPath)
        {
            try
            {
                var writeAllow = false;
                var writeDeny = false;
                DirectoryInfo dInfo = new DirectoryInfo(folderPath);
                if (dInfo == null)
                    return false;
                var accessControlList = dInfo.GetAccessControl();
                if (accessControlList == null)
                    return false;
                var accessRules = accessControlList.GetAccessRules(true, true,
                                            typeof(System.Security.Principal.SecurityIdentifier));
                if (accessRules == null)
                    return false;

                foreach (FileSystemAccessRule rule in accessRules)
                {
                    // When current rule does not contain setting related to WRITE, skip.
                    if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write)
                        continue;

                    if (rule.AccessControlType == AccessControlType.Allow)
                        writeAllow = true;
                    else if (rule.AccessControlType == AccessControlType.Deny)
                        writeDeny = true;
                }

                return writeAllow && !writeDeny;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns whether current user has read access to the folder path.
        /// </summary>
        /// <param name="folderPath">Folder path</param>
        /// <returns></returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("windows")]
#endif
        internal static bool HasReadPermissionOnDir(string folderPath)
        {
            try
            {
                var readAllow = false;
                var readDeny = false;

                DirectoryInfo dInfo = new DirectoryInfo(folderPath);
                if (dInfo == null)
                    return false;
                var accessControlList = dInfo.GetAccessControl();
                if (accessControlList == null)
                    return false;

                var accessRules = accessControlList.GetAccessRules(true, true,
                                            typeof(System.Security.Principal.SecurityIdentifier));
                if (accessRules == null)
                    return false;

                var curentUser = WindowsIdentity.GetCurrent();
                foreach (FileSystemAccessRule rule in accessRules)
                {
                    // When current rule does not contain setting related to Read, skip.
                    if ((FileSystemRights.Read & rule.FileSystemRights) != FileSystemRights.Read)
                        continue;

                    if (!curentUser.User.Equals(rule.IdentityReference) &&
                        !curentUser.Groups.Contains(rule.IdentityReference))
                        continue;

                    if (rule.AccessControlType == AccessControlType.Allow)
                        readAllow = true;
                    else if (rule.AccessControlType == AccessControlType.Deny)
                        readDeny = true;
                }

                return readAllow && !readDeny;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// This is a utility method for checking if given path contains valid XML document.
        /// </summary>
        /// <param name="path">path to the target xml file</param>
        /// <param name="xmlDoc">System.Xml.XmlDocument representation of target xml file</param>
        /// <param name="ex"></param>
        /// <returns>Return true if file is Json, false if file is not Json, exception as out param</returns>
        public static bool isValidXML(string path, out XmlDocument xmlDoc, out Exception ex)
        {
            // Based on https://msdn.microsoft.com/en-us/library/875kz807(v=vs.110).aspx
            // Exception thrown indicate invalid XML document path or due to other failure
            try
            {
                xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                ex = null;
                return true;
            }
            catch (Exception e)
            {
                xmlDoc = null;
                ex = e;
                return false;
            }
        }

        /// <summary>
        /// This is a utility method for checking if a given string represents a valid Json document.
        /// </summary>
        /// <param name="fileContents"> string contents of target json file</param>
        /// <param name="ex"></param>
        /// <returns>Return true if fileContents is Json, false if file is not Json, exception as out param</returns>
        public static bool isFileContentsValidJson(string fileContents, out Exception ex)
        {
            ex = null;
            if (string.IsNullOrEmpty(fileContents))
            {
                ex = new JsonReaderException();
                return false;
            }

            try
            {
                fileContents = fileContents.Trim();
                if ((fileContents.StartsWith("{") && fileContents.EndsWith("}")) || //For object
                    (fileContents.StartsWith("[") && fileContents.EndsWith("]"))) //For array
                {
                    var obj = Newtonsoft.Json.Linq.JToken.Parse(fileContents);
                    return true;
                }
                else
                {
                    ex = new JsonReaderException();
                }
            }
            catch (Exception e)
            {
                ex = e;
            }

            return false;
        }

        /// <summary>
        /// This is a utility method for checking if given path contains valid Json document.
        /// </summary>
        /// <param name="path">path to the target json file</param>
        /// <param name="fileContents"> string contents of target json file</param>
        /// <param name="ex"></param>
        /// <returns>Return true if file is Json, false if file is not Json, exception as out param</returns>
        public static bool isValidJson(string path, out string fileContents, out Exception ex)
        {
            fileContents = "";
            try
            {
                fileContents = File.ReadAllText(path);
                return isFileContentsValidJson(fileContents, out ex);
            }
            catch (Exception e) //some other exception
            {
                Console.WriteLine(e.ToString());
                ex = e;
                return false;
            }
        }

        // Check if the file name contains any special non-printable chatacters.
        public static bool IsFileNameInValid(string fileName)
        {
            // Some other extra characters that are to be checked. 
            char[] invalidCharactersFileName = { '#', '%', '&', '.', ' ' };

            if (fileName.Any(f => Path.GetInvalidFileNameChars().Contains(f)) || fileName.IndexOfAny(invalidCharactersFileName) > -1)
                return true;

            return false;
        }

        /// <summary>
        /// This is a utility method for generating a default name to the snapshot image. 
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>Returns a default name(along with the timestamp) for the workspace image</returns>
        [Obsolete("This function will be removed in future version of Dynamo - please use the version with more parameters")]
        public static String GetScreenCaptureNameFromPath(String filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            String timeStamp = string.Format("{0:yyyy-MM-dd_hh-mm-ss}", DateTime.Now);
            String snapshotName = fileInfo.Name.Replace(fileInfo.Extension, "_") + timeStamp;
            return snapshotName;
        }

        /// <summary>
        /// This is a utility method for generating a default name to the snapshot image. 
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="isTimeStampIncluded">Is timestamp included in file path</param>
        /// <returns>Returns a default name(along with the timestamp) for the workspace image</returns>
        public static String GetScreenCaptureNameFromPath(String filePath, bool isTimeStampIncluded)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (isTimeStampIncluded)
            {
                String timeStamp = string.Format("{0:yyyy-MM-dd_hh-mm-ss}", DateTime.Now);
                String snapshotName = fileInfo.Name.Replace(fileInfo.Extension, "_") + timeStamp;
                return snapshotName;
            }
            else
            {
                return fileInfo.Name.Replace(fileInfo.Extension, string.Empty);
            }

        }

        /// <summary>
        /// Computes the file size from the path.
        /// </summary>
        /// <param name="path">File path</param>
        public static string GetFileSize(string path)
        {
            if (path != null)
            {
                var fileInfo = new FileInfo(path);
                long size = fileInfo.Length / KbConversionConstant;
                return size.ToString() + sizeUnits;
            }

            return null;
        }

        /// <summary>
        /// Checks if the file exists at the specified path and computes size.
        /// </summary>
        /// <param name="path">File path</param>
        /// <param name="fileExists"></param>
        /// <param name="size"></param>
        /// <returns>Returns a boolean if the file exists at the path and also returns its size</returns>
        public static void FileInfoAtPath(string path, out bool fileExists, out string size)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(path);
                fileExists = true;
                var fileLength = fileInfo.Length / KbConversionConstant;
                size = fileLength.ToString() + sizeUnits;
            }
            catch
            {
                fileExists = false;
                size = string.Empty;
            }
        }

        internal static Char[] SpecialAndInvalidCharacters()
        {
            // Excluding white spaces and uncommon characters, only keeping the displayed in the Windows alert
            return System.IO.Path.GetInvalidFileNameChars().Where(x => !char.IsWhiteSpace(x) && (int)x > 31).ToArray();
        }

        /// <summary>
        /// Checks is the path is considered valid directory path.
        /// An exception is thrown if the path is considered invalid.
        /// A path is considered valid if the following conditions are true:
        /// 1. Path is not null and not empty.
        /// 2. Path is an absolute path (not relative).
        /// 4. Path has valid characters.
        /// 5. Path exists and points to a folder.
        /// 6. Dynamo has read permissions to access the path.
        /// </summary>
        /// <param name="directoryPath">The directory path that needs to be validated</param>
        /// <param name="absolutePath"></param>
        /// <param name="mustExist"></param>
        /// <param name="read"></param>
        /// <param name="write"></param>
        /// <returns>A normalized and validated path</returns>
        /// <exception cref="ArgumentNullException">Input argument is null or empty.</exception>
        /// <exception cref="ArgumentException">Input argument is not an absolute path.</exception>
        /// <exception cref="DirectoryNotFoundException">Path directory does not exist</exception>
        /// <exception cref="System.Security.SecurityException">Dynamo does not have the required permissions.</exception>
        internal static string ValidateDirectory(string directoryPath, bool absolutePath = true, bool mustExist = true, bool read = true, bool write = false)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentNullException($"The input argument is null or empty.");
            }

            if (absolutePath && !Path.GetFullPath(directoryPath).Equals(directoryPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"The input path {directoryPath} must be an absolute path");
            }

            if (mustExist && !Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"The input path: {directoryPath} does not exist or is not a folder");
            }

            // TODO: figure out read/write permissions for Linux
            if (OSHelper.IsWindows())
            {
                if (read && !PathHelper.HasReadPermissionOnDir(directoryPath))
                {
                    throw new System.Security.SecurityException($"Dynamo does not have the required permissions for the path: {directoryPath}");
                }

                if (write && !PathHelper.HasWritePermissionOnDir(directoryPath))
                {
                    throw new System.Security.SecurityException($"Dynamo does not have the required permissions for the path: {directoryPath}");
                }
            }

            return directoryPath;
        }

        /// <summary>
        /// Appends a DirectorySeparatorChar to the end of the path if no separator exists.
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        private static string FormatDirectoryPath(string dirPath)
        {
            string formattedPath = dirPath;

            string separator = Path.DirectorySeparatorChar.ToString();
            string altSeparator = Path.AltDirectorySeparatorChar.ToString();
            if (!formattedPath.EndsWith(separator) && !formattedPath.EndsWith(altSeparator))
            {
                formattedPath += separator;
            }
            return Path.GetFullPath(formattedPath);
        }

        internal static bool AreDirectoryPathsEqual(string dir1, string dir2)
        {
            string dirPath1 = FormatDirectoryPath(dir1);
            string dirPath2 = FormatDirectoryPath(dir2);
            return dirPath1.Equals(dirPath2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns true if "subdirectory" input argument is a subdirectory of the "directory" input argument.
        /// Returns false otherwise.
        /// </summary>
        /// <param name="subdirectory"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        internal static bool IsSubDirectoryOfDirectory(string subdirectory, string directory)
        {
            string subdirPath = FormatDirectoryPath(subdirectory);
            string directoryPath = FormatDirectoryPath(directory);

            return subdirPath.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns the path configured for the requested service to retrieve URL resources
        /// as defined inside the config file
        /// </summary>
        /// <param name="o">The "this" object from where the function is being called from.</param>
        /// <param name="serviceKey">Service or feature for which the address is being requested. 
        /// It should match the key specified in the config file.</param>
        /// <returns>Path that will be used to fetch resources</returns>
        public static string getServiceBackendAddress(object o, string serviceKey)
        {
            string url = null;
            if (o != null)
            {
                var path = o.GetType().Assembly.Location;
                var config = ConfigurationManager.OpenExeConfiguration(path);
                var key = config.AppSettings.Settings[serviceKey];

                if (key != null)
                {
                    url = key.Value;
                }

                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    throw new ArgumentException("Incorrectly formatted URL provided for the service.", "url");
                }
            }
            return url;
        }

        /// <summary>
        /// Returns the path configured for the requested service to retrieve resources value
        /// as defined inside the config file
        /// </summary>
        /// <param name="o">The "this" object from where the function is being called from.</param>
        /// <param name="serviceKey">Service or feature for which the resource is being requested. 
        /// It should match the key specified in the config file.</param>
        /// <returns>Value related to the key in the config file</returns>
        public static string getServiceConfigValues(object o, string serviceKey)
        {
            string val = null;
            if (o != null)
            {
                var path = o.GetType().Assembly.Location;
                var config = ConfigurationManager.OpenExeConfiguration(path);
                var key = config.AppSettings.Settings[serviceKey];

                if (key != null)
                {
                    val = key.Value;
                }
            }
            return val;
        }
    }
}
