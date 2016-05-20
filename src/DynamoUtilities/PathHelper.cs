using System;
using System.IO;
using System.Security.AccessControl;

namespace DynamoUtilities
{
    public class PathHelper
    {
        // This return an exception if any operation failed and the folder
        // wasn't created. It's the responsibility of the caller of this function
        // to check whether the folder creation is successful or not.
        public static Exception CreateFolderIfNotExist(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
            }
            catch (IOException ex) { return ex; }
            catch (ArgumentException ex) { return ex; }
            catch (UnauthorizedAccessException ex) { return ex; }

            return null;
        }

        /// <summary>
        /// Checks if the file path string is valid and file exist.
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static bool IsValidPath(string folderPath)
        {
            return (!string.IsNullOrEmpty(folderPath) && (File.Exists(folderPath)));
        }

        /// <summary>
        /// Check if user has readonly privilege to the folder path.
        /// </summary>
        /// <param name="folderPath">Folder path</param>
        /// <returns></returns>
        public static bool IsReadOnlyPath(string folderPath)
        {
            if (IsValidPath(folderPath))
            {
                FileInfo Finfo = new FileInfo(folderPath);
                // We mark the path read only when
                // 1. file read-only
                // 2. user does not have write access to the folder
                return Finfo.IsReadOnly || !HasWritePermissionOnDir(folderPath);
            }
            else
                return false;
        }

        /// <summary>
        /// Returns whether current user has write access to the folder path.
        /// </summary>
        /// <param name="folderPath">Folder path</param>
        /// <returns></returns>
        public static bool HasWritePermissionOnDir(string folderPath)
        {
            var writeAllow = false;
            var writeDeny = false;
            var accessControlList = Directory.GetAccessControl(folderPath);
            if (accessControlList == null)
                return false;
            var accessRules = accessControlList.GetAccessRules(true, true,
                                        typeof(System.Security.Principal.SecurityIdentifier));
            if (accessRules == null)
                return false;

            foreach (FileSystemAccessRule rule in accessRules)
            {
                if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write)
                    continue;

                if (rule.AccessControlType == AccessControlType.Allow)
                    writeAllow = true;
                else if (rule.AccessControlType == AccessControlType.Deny)
                    writeDeny = true;
            }

            return writeAllow && !writeDeny;
        }
    }
}
