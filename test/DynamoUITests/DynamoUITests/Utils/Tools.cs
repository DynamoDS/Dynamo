using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DynamoTests.Utils
{
    public static class Tools
    {
        public static void SetClipboard(string text)
        {
            Thread thread = new Thread(() => Clipboard.SetText(text));
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            thread.Start();
            thread.Join();
        }

        public static string GetClipboard()
        {
            string returnValue = string.Empty;

            Thread thread = new Thread(() => returnValue = Clipboard.GetText());
            thread.SetApartmentState(ApartmentState.STA); //Set the thread to STA
            thread.Start();
            thread.Join();

            return returnValue;
        }

        public static bool DynamoAppDataExist()
        {
            return Directory.Exists(ConfigurationHelper.DynamoDataRoute);
        }

        public static bool DeleteDynamoData()
        {
            return DeleteDir(ConfigurationHelper.DynamoDataRoute);
        }

        public static bool DeleteDir(string directoryPath)
        {
            try
            {
                var dir = new DirectoryInfo(directoryPath);
                dir.Delete(true);
                return true;
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            return false;
        }

        public static bool DeleteFile(string filePath)
        {
            try
            {

                File.Delete(filePath);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public static bool FileExists(string filePath)
        {
            try
            {
                return File.Exists(filePath);
            }
            catch (IOException)
            {
                return false;
            }
        }

        public static FileInfo GetFileInfo(string filePath)
        {
            FileInfo result = null;
            try
            {
                if (FileExists(filePath))
                    result = new FileInfo(filePath);

                return result;            
            }
            catch (IOException)
            {
                return null;
            }
        }

        public static string TextFileToString(string pathInProject)
        {
            return File.ReadAllText(pathInProject, Encoding.ASCII);
        }
    }
}
