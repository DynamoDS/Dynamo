using System;
using System.IO;
using Autodesk.DesignScript.Runtime;
using Dynamo.Configuration;
using Dynamo.Events;
using Dynamo.Session;

namespace DSCore
{
    [IsVisibleInDynamoLibrary(false)]
    public class Web
    {
        public static string WebRequestByUrl(string url)
        {
            // Prevent the node from executing in no network mode.
            ExecutionSessionHelper.ThrowIfNoNetworkMode();

            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException(Properties.Resources.WebRequestNullUrlMessage);
            }

            Uri uriResult;
            var result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                          && (uriResult.Scheme == Uri.UriSchemeHttp
                              || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!result)
            {
                throw new UriFormatException("The specified url is invalid.");
            }

            //send a webrequest to the URL
            // Initialize the WebRequest.
            var myRequest = System.Net.WebRequest.Create(uriResult);

            // Set the User-Agent header required by some APIs
            if (myRequest is System.Net.HttpWebRequest httpRequest)
                httpRequest.UserAgent = Configurations.DynamoAsString;

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
