﻿using System;
using System.IO;
using System.Linq;
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
                return Finfo.IsReadOnly || !HasWritePermissionOnDir(Finfo.Directory.ToString());
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

        /// <summary>
        /// searchs for a file with an extension in a list of paths, returns the first match
        /// where the extension is case insensitive. If no file is found, returns an empty string.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="extension"></param>
        /// <param name="searchPaths"></param>
        /// <returns></returns>
        public static string FindFileInPaths(string filename, string extension, string[] searchPaths)
        {
            foreach (var path in searchPaths)
            {
                if (Directory.Exists(path))
                {
                    var files = Directory.GetFiles(path);
                    //matches occur where filename and extension are the same when both are lowercased
                    var matches = files.ToList().Where(x => String.CompareOrdinal(Path.GetFileName(x).ToLower(), (filename + extension).ToLower()) == 0) ;
                    if (matches.Count() > 1)
                    {
                        Console.WriteLine(string.Format("While searching for {0}{1} in {2}, {3} matches were found, the first will be loaded",
                            filename, extension, path, matches.Count().ToString()));
                    }
                    if (matches.Count() > 0)
                    {
                        //found a match, return the first one
                        return matches.First();
                    }

                }
            }
            return string.Empty;
        }
    }
}
