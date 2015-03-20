using System;
using System.IO;
using Autodesk.DesignScript.Runtime;

namespace DSCore
{
    [IsVisibleInDynamoLibrary(false)]
    public class Web
    {
        public static string WebRequestByUrl(string url)
        {
            if (url == null)
            {
                throw new ArgumentException("The url cannot be null.");
            }

            //send a webrequest to the URL
            // Initialize the WebRequest.
            var myRequest = System.Net.WebRequest.Create(url);

            string responseFromServer;

            // Return the response. 
            using (var myResponse = myRequest.GetResponse())
            {
                var dataStream = myResponse.GetResponseStream();

                // Open the stream using a StreamReader for easy access.
                using (var reader = new StreamReader(dataStream))
                {
                    // Read the content.
                    responseFromServer = reader.ReadToEnd();
                }
            }

            return responseFromServer;
        }
    }
}
