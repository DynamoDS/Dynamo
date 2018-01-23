using System;
using System.IO;
using System.Security.AccessControl;
using System.Xml;
using Newtonsoft.Json;

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
        /// This is a utility method for checking if given path contains valid XML document.
        /// </summary>
        /// <param name="path">path to the target xml file</param>
        /// <param name="xmlDoc">System.Xml.XmlDocument repensentation of target xml file</param>
        /// <returns></returns>
        public static bool isValidXML(string path, out XmlDocument xmlDoc)
        {
            // Based on https://msdn.microsoft.com/en-us/library/875kz807(v=vs.110).aspx
            // Exception thrown indicate invalid XML document path 
            try
            {
                xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                return true;
            }
            catch(Exception ex)
            {
                xmlDoc = null;
                throw ex;
            }
        }

        /// <summary>
        /// This is a utility method for checking if given path contains valid Json document.
        /// </summary>
        /// <param name="path">path to the target json file</param>
        /// <param name="fileContents"> string contents of target json file</param>
        /// <returns></returns>
        public static bool isValidJson(string path, out string fileContents)
        {
            fileContents = "";
            try
            {
                fileContents = File.ReadAllText(path);
                fileContents = fileContents.Trim();
                if ((fileContents.StartsWith("{") && fileContents.EndsWith("}")) || //For object
                    (fileContents.StartsWith("[") && fileContents.EndsWith("]"))) //For array
                {
                    var obj = Newtonsoft.Json.Linq.JToken.Parse(fileContents);
                    return true;
                }
                return false;
            }
            catch (JsonReaderException jex)
            {
                //Exception in parsing Json
                Console.WriteLine(jex.Message);
                return false;
            }
            catch (Exception ex) //some other exception
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }
    }
}
