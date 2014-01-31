
using System;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;
using ProtoCore.Utils;
using System.Xml.Serialization;
using System.Text;

namespace ProtoCore
{
    public static class TLSUtils
    {
        public static string GetTLSData()
        {
             return "hello world data";
        }
    }

    /// <summary>
    /// This data structure contains the exection state of a single callsite
    /// </summary>
    public class CallsiteExecutionStateNode
    {
        public string ID { get; set; }
        public string Data { get; set; }
        public int RunID { get; set; }

        public CallsiteExecutionStateNode()
        {
            ID = string.Empty;
            Data = string.Empty;
            RunID = 0;
        }

        public CallsiteExecutionStateNode(string csID, string csData)
        {
            ID = csID;
            Data = csData;
            RunID = 0;
        }
    }

    public class CallsiteExecutionState
    {

#region Static_Utils

        private static string ext = "csstate";
        public static string filename = "vmstate_test";

        private static FileStream fileStream = null;
        private static string filePath = GetThisSessionFileName();


        /// <summary>
        /// Generate a callsite guid, given a functiongroup ID and the expression ID
        /// </summary>
        /// <param name="functionUID"></param>
        /// <param name="ExprUID"></param>
        /// <returns></returns>
        public static string GetCallsiteGUID(string functionUID, int ExprUID)
        {
            // This is a naive implementation, explore a better one
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            string inString = functionUID + ExprUID.ToString();
            
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(inString);
            byte[] hash = md5.ComputeHash(inputBytes);


            // Convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }


        public static string GetThisSessionFileName()
        {
            return filename + "." + ext;
        }

#endregion

#region Static_LoadStore_Methods

        public static bool SaveState(CallsiteExecutionState data)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(CallsiteExecutionState));
                fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                serializer.Serialize(fileStream, data);
                fileStream.Close();
            }
            catch (Exception e)
            {
                return false;
            }

            if (null != fileStream)
            {
                fileStream.Close();
            }
            return true;
        }

        public static CallsiteExecutionState LoadState()
        {
            CallsiteExecutionState csState = null;
            if (!string.IsNullOrEmpty(filePath) && (File.Exists(filePath)))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(CallsiteExecutionState));
                    fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    csState = serializer.Deserialize(fileStream) as CallsiteExecutionState;
                }
                catch (Exception)
                {
                    csState = new CallsiteExecutionState();
                }
            }
            else
            {
                csState = new CallsiteExecutionState();
            }

            return csState;
        }

#endregion


        public CallsiteExecutionState()
        {
            //CallsiteDataMap = new Dictionary<string, string>();
            //key = new List<string>();
            //value = new List<string>();
            CallsiteDataMap = new List<CallsiteExecutionStateNode>();
        }

        /// <summary>
        /// CallsiteDataMap is the mapping between a unique callsite ID and the associated data
        /// This data is updated for every call to a callsite
        /// </summary>
        //public Dictionary<string, string> CallsiteDataMap { get; set; }
        public List<CallsiteExecutionStateNode> CallsiteDataMap { get; set; }


        //public List<string> key { get; set; }
        //public List<string> value { get; set; }

        /// <summary>
        /// Stores the data given a callsite ID
        /// This does not increment the runID
        /// Adds a new entry if the callsite ID does not exist
        /// </summary>
        /// <param name="callsiteID"></param>
        /// <param name="dataObj"></param>
        public void Store(string callsiteID, Object dataObj)
        {
            Validity.Assert(null != CallsiteDataMap);
            string data = dataObj as string;

            CallsiteExecutionStateNode csNode = CallsiteDataMap.Find(x => x.ID == callsiteID);
            if (null != csNode)
            {
                // Modify node data if it exists
                csNode.Data = data;
            }
            else
            {
                // Add new entry otherwise
                CallsiteDataMap.Add(new CallsiteExecutionStateNode(callsiteID, data));
            }
        }

        /// <summary>
        /// Stores the data given a callsite ID
        /// This increments the runID
        /// Adds a new entry if the callsite ID does not exist
        /// Returns the runid associated with the callsiteID
        /// </summary>
        /// <param name="callsiteID"></param>
        /// <param name="dataObj"></param>
        public int StoreAndUpdateRunId(string callsiteID, Object dataObj)
        {
            Validity.Assert(null != CallsiteDataMap);
            string data = dataObj as string;
            CallsiteExecutionStateNode csNode = CallsiteDataMap.Find(x => x.ID == callsiteID);
            int runid = ProtoCore.DSASM.Constants.kInvalidIndex;
            if (null != csNode)
            {
                // Modify node data if it exists
                csNode.Data = data;
                runid = ++csNode.RunID;
            }
            else
            {
                // Add new entry otherwise
                csNode = new CallsiteExecutionStateNode(callsiteID, data);
                CallsiteDataMap.Add(csNode);

                // A new entry means the run id is always 0
                runid = csNode.RunID;
            }
            return runid;
        }

        public string LoadData(string callsiteID)
        {
            Validity.Assert(null != CallsiteDataMap);
            CallsiteExecutionStateNode csNode = CallsiteDataMap.Find(x => x.ID == callsiteID);
            if (null != csNode)
            {
                return csNode.Data;
            }
            return null;
        }

        public int LoadRunID(string callsiteID)
        {
            Validity.Assert(null != CallsiteDataMap);
            CallsiteExecutionStateNode csNode = CallsiteDataMap.Find(x => x.ID == callsiteID);
            if (null != csNode)
            {
                return csNode.RunID;
            }
            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        //public void Store(string callsiteID, Object dataObj)
        //{
        //    // TODO Jun: implement serializable object
        //    string data = dataObj as string;

        //    int valueIndex = -1;
        //    if (key.Contains(callsiteID))
        //    {
        //        valueIndex = key.IndexOf(callsiteID);
        //        value[valueIndex] = data;
        //    }
        //    else
        //    {
        //        key.Add(callsiteID);
        //        value.Add(data);
        //    }
        //}

        //public string Load(string callsiteID)
        //{
        //    int valueIndex = -1;
        //    if (key.Contains(callsiteID))
        //    {
        //        valueIndex = key.IndexOf(callsiteID);
        //        return value[valueIndex];
        //    }
        //    return null;
        //}

        public int GetCSStateCount()
        {
            Validity.Assert(null != CallsiteDataMap);
            return CallsiteDataMap.Count;
        }
    }
}
