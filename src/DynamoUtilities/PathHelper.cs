using System;
using System.IO;
using System.Linq;
using System.Text;

namespace DynamoUtilities
{
    public class PathHelper
    {
        // This return an exception if any operation failed and the folder
        // wasn't created. It's the resposibility of the caller of this function
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
    }
}
