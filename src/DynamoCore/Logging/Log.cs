/**
 * 
 * Luke Church, 2011
 * luke@church.name
 * 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Dynamo.Logging
{
    internal class UsageLog : ILogger, IDisposable
    {
        private const string URL = "https://dynamoinstr.appspot.com/rpc";
        //private const string URL = "http://192.168.1.68:8080/rpc";
        
        private const int MAX_DATA_LENGTH = 500000;
        private const int MAX_NAME_LENGTH = 256;

        private const int MAX_COUNT = 50000;
        private Queue<Dictionary<string, string>> items;

        /// <summary>
        /// This mutex guards transactions on the memory store
        /// </summary>
        private object dbMutex = new object();

        #region State holders

        private string appName;
        /// <summary>
        /// The name of the application associated with the log entries
        /// </summary>
        public string AppName
        {
            get
            {
                return appName;
            }
            set
            {
                if (!ValidateTextContent(value))
                    throw new ArgumentException(
                        "App name must only be letters, numbers or -");

                if (!ValidateLength(value))
                    throw new ArgumentException("App Name must be 256 chars or less");


                appName = value;
            }
        }

        private string userID;
        /// <summary>
        /// A Guid string for the user associated with the log entries
        /// </summary>
        public string UserID
        {
            get
            {
                return userID;
            }
            set
            {


                if (!ValidateTextContent(value))
                    throw new ArgumentException(
                        "User ID name must only be letters, numbers or -");

                if (!ValidateLength(value))
                    throw new ArgumentException("UserID must be 256 chars or less");


                userID = value;
            }
        }
        private string sessionID;
        /// <summary>
        /// A Guid string for the session that the user is engaging with
        /// </summary>
        public string SessionID
        {
            get
            {
                return sessionID;
            }
            set
            {
                if (!ValidateTextContent(value))
                    throw new ArgumentException(
                        "Session ID name must only be letters, numbers or -");

                if (!ValidateLength(value))
                    throw new ArgumentException("Session ID must be 256 chars or less");


                sessionID = value;
            }
        }


        #endregion




        /// <summary>
        /// An object used to guard the uploadedItemsCount
        /// </summary>
        private Object uploadedItemsMutex = new object();
        private long uploadedItemsCount = 0;

        /// <summary>
        /// The number of items that this log entity has successfully uploaded
        /// This may be greater than the number of calls due to splitting of large entries
        /// </summary>
        public long UploadedItems
        {
            get
            {
                lock (uploadedItemsMutex)
                {
                    return uploadedItemsCount;
                }
            }

            private set
            {
                lock (uploadedItemsMutex)
                {
                    uploadedItemsCount = value;
                }
            }
        }

        private bool EnableDiagnosticsOutput
        {
            get;
            set;
        }

        System.Diagnostics.Stopwatch sw;

        private Thread uploaderThread;
        private const int EMPTY_DELAY_MS = 500;
        private const int ERROR_DELAY_MS = 10000;
        private const int DELAY_MS = 10;

        #region Public API


        /// <summary>
        /// Create a new log instance
        /// </summary>
        /// <param name="appName">The name of the application associated with this log</param>
        /// <param name="userID">A statistically unique string associated with the user, e.g. a GUID</param>
        /// <param name="sessionID">A statistically unique string associated with the session, e.g. a GUID</param>
        public UsageLog(string appName, string userID, string sessionID)
        {
            try
            {

                this.EnableDiagnosticsOutput = false;

                AppName = appName;
                UserID = userID;
                SessionID = sessionID;

                this.sw = new System.Diagnostics.Stopwatch();
                items = new Queue<Dictionary<string, string>>();
                sw.Start();


                uploaderThread = new Thread(UploaderExec);
                uploaderThread.IsBackground = true;
                uploaderThread.Start();
            }
            catch (Exception e)
            {
                LastResortFailure(e);
            }
        }


        /// <summary>
        /// Log an item at debug priority
        /// </summary>
        /// <param name="tag">Tag associated with the log item</param>
        /// <param name="text">Text to log</param>
        public void Debug(string tag, string text)
        {
            ValidateInput(tag, text);
            PrepAndPushItem(tag, "Debug", text);
        }

        /// <summary>
        /// Log an item at error priority
        /// </summary>
        /// <param name="tag">Tag associated with the log item</param>
        /// <param name="text">Text to log</param>
        public void Error(string tag, string text)
        {
            ValidateInput(tag, text);
            PrepAndPushItem(tag, "Error", text);
        }

        /// <summary>
        /// Log an item at info priority
        /// </summary>
        /// <param name="tag">Tag associated with the log item</param>
        /// <param name="text">Text to log</param>
        public void Log(string tag, string text)
        {
            ValidateInput(tag, text);
            PrepAndPushItem(tag, "Info", text);
        }

        /// <summary>
        /// Log an item at verbose priority
        /// </summary>
        /// <param name="tag">Tag associated with the log item</param>
        /// <param name="text">Text to log</param>
        public void Verbose(string tag, string text)
        {
            ValidateInput(tag, text);
            PrepAndPushItem(tag, "Verbose", text);
        }

        #endregion

        /// <summary>
        /// Methods that preps a write request
        /// This method calls recursively up to once if the text is too large and needs
        /// splitting
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="priority"></param>
        /// <param name="text"></param>
        private void PrepAndPushItem(string tag, string priority, string text)
        {
            //We don't need to validate the content of text as it's going to get base64
            //encoded
            if (this.EnableDiagnosticsOutput)
            {
                System.Diagnostics.Debug.Assert(ValidateTextContent(tag));
                System.Diagnostics.Debug.Assert(ValidateTextContent(priority));
            }

            List<String> splitText = SplitText(text);

            if (splitText.Count > 1)
            {
                //Add a GUID and a serial to the Tag (We've reserved enough space for this)
                //Recursive call the write with the split text
                Guid g = Guid.NewGuid();

                for (int i = 0; i < splitText.Count; i++)
                {
                    PrepAndPushItem(
                        tag + "-" + g.ToString() + "-" + i.ToString(),
                        priority,
                        splitText[i]);
                }

                return;
            }

            text = splitText[0];

            byte[] byteRepresentation = System.Text.Encoding.UTF8.GetBytes(text);
            string safeStr = System.Convert.ToBase64String(byteRepresentation);

            //Destroy the original representations to ensure runtime errors if used later in this method
            text = null;

            string dateTime = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
            string microTime = sw.ElapsedMilliseconds.ToString();

            var item = new Dictionary<string, string>
            {
                { "Tag", tag },
                { "Priority", priority },
                { "AppIdent", AppName },
                { "UserID", UserID },
                { "SessionID", SessionID },
                { "DateTime", dateTime },
                { "MicroTime", microTime },
                { "Data", safeStr }
            };

            PushItem(item);
        }

        /// <summary>
        /// ASync add the log item onto the queue to be pushed to the db
        /// </summary>
        /// <param name="item">
        /// A <see cref="Dictionary{String, String}"/>
        /// </param>
        private void PushItem(Dictionary<String, String> item)
        {

            lock (dbMutex)
            {
                if (items.Count > UsageLog.MAX_COUNT)
                    return;

                items.Enqueue(item);

                // Write item to file
                string mydocpath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                using (System.IO.StreamWriter outputFile = new System.IO.StreamWriter(Path.Combine(mydocpath, "AnalyticsLog.txt"), true))
                {
                    byte[] b = Convert.FromBase64String(item["Data"]);
                    var dataStr = System.Text.Encoding.UTF8.GetString(b);
                    outputFile.WriteLine("Tag: " + item["Tag"] + "\nData: " + dataStr);
                }
            }

        }

        #region Uploader Thread methods

        /// <summary>
        /// Core thread method for handling the upload
        /// </summary>
        private void UploaderExec()
        {
            try
            {

                while (true)
                {
                    if (uploaderThread == null) break;

                    Thread.Sleep(DELAY_MS);

                    Dictionary<String, String> itemToUpload = null;

                    lock (dbMutex)
                    {
                        if (items.Count > 0)
                            itemToUpload = items.Dequeue();
                    }

                    //If there are no items 
                    if (itemToUpload == null)
                    {
                        Thread.Sleep(EMPTY_DELAY_MS);
                        continue;
                    }

                    if (!UploadItem(itemToUpload))
                    {
                        lock (dbMutex)
                        {
                            items.Enqueue(itemToUpload);
                        }
                    }
                    
                }
            }
            catch (Exception e)
            {
                LastResortFailure(e);
            }

        }

        /// <summary>
        /// Code to transfer an item iver the network
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true if success, false otherwise</returns>
        private bool UploadItem(Dictionary<String, String> item)
        {
            try
            {

                StringBuilder sb = new StringBuilder();
                sb.Append("[\"BasicStore\", {");

                bool first = true;

                foreach (string key in item.Keys)
                {
                    if (!first)
                        sb.Append(",");
                    else
                        first = false;

                    sb.Append("\"");
                    sb.Append(key);
                    sb.Append("\" : \"");
                    sb.Append(item[key]);
                    sb.Append("\"");

                }

                sb.Append("}]");

                if (this.EnableDiagnosticsOutput)
                    System.Diagnostics.Debug.WriteLine(sb.ToString());

                WebRequest request = WebRequest.Create(URL);

                request.Method = "POST";
                byte[] byteArray = Encoding.UTF8.GetBytes(sb.ToString());

                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
                WebResponse response = request.GetResponse();

                if (((HttpWebResponse)response).StatusCode != HttpStatusCode.OK)
                    throw new Exception(((HttpWebResponse)response).StatusDescription);

                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                if (this.EnableDiagnosticsOutput)
                    System.Diagnostics.Debug.WriteLine(responseFromServer);

                reader.Close();
                dataStream.Close();
                response.Close();

                UploadedItems += long.Parse(responseFromServer);
                return true;
            }
            catch (Exception e)
            {
                Thread.Sleep(ERROR_DELAY_MS);


                if (this.EnableDiagnosticsOutput)
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                return false;
            }
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Split a string into shorter strings as defined by the const
        /// params
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private List<String> SplitText(String text)
        {
            int len = text.Length;

            List<String> ret = new List<string>();
            int added = 0;
            for (int i = 0; i < len / (MAX_DATA_LENGTH + 1); i++)
            {
                ret.Add(text.Substring(i * MAX_DATA_LENGTH, MAX_DATA_LENGTH));
                added += MAX_DATA_LENGTH;
            }

            ret.Add(text.Substring(added));

#if DEBUG
            int totalTextCount = 0;
            foreach (String str in ret)
                totalTextCount += str.Length;

            if (this.EnableDiagnosticsOutput)
                System.Diagnostics.Debug.Assert(totalTextCount == len);
#endif

            return ret;
        }

        /// <summary>
        /// Do input validation tests
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="text"></param>
        private void ValidateInput(string tag, string text)
        {
            if (tag == null)
                throw new ArgumentNullException("Tag must not be null");

            if (text == null)
                throw new ArgumentNullException("Text must not be null");

            if (!ValidateLength(tag))
                throw new ArgumentException("Tag must be 256 chars or less");


            if (!ValidateTextContent(tag))
                throw new ArgumentException(
                    "Tag must only be letters, numbers or '-', '.'");

        }

        /// <summary>
        /// Ensure that the text that is being sent is only alphanumeric
        /// and hypen
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool ValidateTextContent(string str)
        {
            char[] chars = str.ToCharArray();

            foreach (char ch in chars)
            {
                if (!(char.IsLetterOrDigit(ch) || ch == '-' || ch == '.'))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Ensure that the names and tags fit within the safe window after
        /// we've put a GUID and a serial on the end of them
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool ValidateLength(string str)
        {
            if (str == null)
                return false;

            if (str.Length > MAX_NAME_LENGTH)
                return false;
            else
                return true;

        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                uploaderThread.Abort();
            }
            catch (Exception)
            {

            }
        }

        #endregion

        private void LastResortFailure(Exception e)
        {

            //Do nothing, just absorb the error, the implicitly causes logging to shut down

        }

        public void Log(string message)
        {
            throw new NotImplementedException();
        }

        public void LogError(string error)
        {
            throw new NotImplementedException();
        }

        public void LogWarning(string warning, WarningLevel level)
        {
            throw new NotImplementedException();
        }

        public void Log(Exception e)
        {
            throw new NotImplementedException();
        }
    }
}

