using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace ProtoCore.Utils
{
    public static class FileUtils
    {
        private static string mInstallPath;
        /// <summary>
        /// Locates the given file from the search path options and gets the 
        /// full file path.
        /// </summary>
        /// <param name="fileName">Name of the file to locate</param>
        /// <param name="options">Options structure for search path, if options 
        /// is null it will search only in the executing assembly path or the
        /// current directory.</param>
        /// <returns>Full path for the file if located successfully else the 
        /// file name when failed to loate the given file</returns>
        public static string GetDSFullPathName(string fileName, Options options = null)
        {
            // trim white space chars
            var trimChars = new[] {'\n','\t','\r',' '};
            fileName = fileName.Trim(trimChars);

            //1.  First search at .exe module directory, in case files of the same name exists in the following directories.
            //    The .exe module directory is of highest priority.
            //    CodeBase is used here because Assembly.Location does not work quite well when the module is shallow-copied in nunit test.
            if (Path.IsPathRooted(fileName))
            {
                if (File.Exists(fileName))
                    return Path.GetFullPath(fileName);
                fileName = Path.GetFileName(fileName);
            }

            string fullPathName;

            if (GetFullPath(fileName, GetInstallLocation(), out fullPathName))
                return fullPathName;

            //2. Search relative to the .ds file directory
            string rootModulePath = ".";
            if (null!=options && !string.IsNullOrEmpty(options.RootModulePathName))
                rootModulePath = options.RootModulePathName;

            if (GetFullPath(fileName, rootModulePath, out fullPathName))
                return fullPathName;

            if (null != options)
            {
                //3. Search at include directories.
                //   This will include the import path.
                foreach (string directory in options.IncludeDirectories)
                {
                    fullPathName = Path.Combine(directory, fileName);
                    if (null != fullPathName && File.Exists(fullPathName))
                        return fullPathName;
                }
            }

            //4. Search the absolute path or relative to the current directory
            if (File.Exists(fileName))
                return Path.GetFullPath(fileName);

            return fileName;
        }

        /// <summary>
        /// Loads a given file from the search path defined via options include directories.
        /// </summary>
        /// <param name="fileName">Name of the assembly file</param>
        /// <param name="options">Options structure for search path, if options 
        /// is null it will search only in the executing assembly path or the
        /// current directory.</param>
        /// <returns>Loaded Assembly or null if load failed</returns>
        public static Assembly LoadAssembly(string fileName, Options options = null)
        {
            string filePath = GetDSFullPathName(fileName, options);
            try
            {
                return ProtoFFI.FFIExecutionManager.Instance.LoadAssembly(filePath);
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
            catch (System.IO.FileLoadException)
            {
                return null;
            }
            catch (System.IO.FileFormatException)
            {
                return null;
            }
        }

        private static bool GetFullPath(string fileName, string hintPath, out string fullPath)
        {
            string directoryPath = Directory.Exists(hintPath) ? hintPath : Path.GetDirectoryName(hintPath);
            string fullPathName = Path.Combine(directoryPath, fileName);
            if (File.Exists(fullPathName))
            {
                fullPath = Path.GetFullPath(fullPathName);
                return true;
            }

            fullPath = string.Empty;
            return false;
        }

        public static string GetFullPathName(string fileName, string currentDirectory = null)
        {
            if (string.IsNullOrEmpty(currentDirectory))
                currentDirectory = Directory.GetCurrentDirectory();

            if (File.Exists(fileName))
            {
                return Path.GetFullPath(fileName);
            }
            else
            {
                string fullPathName = Path.Combine(currentDirectory, fileName);
                if (File.Exists(fullPathName))
                {
                    return Path.GetFullPath(fullPathName);
                }
            }

            return null;
        }

        public static string GetInstallLocation()
        {
            if (string.IsNullOrEmpty(mInstallPath))
            {
                var protoInterface = typeof(Autodesk.DesignScript.Interfaces.IExtensionApplication).Assembly;
                var uri = new UriBuilder(protoInterface.CodeBase);
                mInstallPath = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
            }

            return mInstallPath;
        }
    }
}
