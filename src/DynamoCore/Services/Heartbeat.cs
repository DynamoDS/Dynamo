using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamo.Models;
using Dynamo.Properties;
using Dynamo.Utilities;

namespace Dynamo.Services
{
    /// <summary>
    /// Class to automatically report various metrics of the application usage
    /// </summary>
    public class Heartbeat
    {
        private static Heartbeat instance;
        private const int WARMUP_DELAY_MS = 5000;
        private const int HEARTBEAT_INTERVAL_MS = 60 * 1000;
        private DateTime startTime;
        private Thread heartbeatThread;


        private Heartbeat()
        {
            startTime = DateTime.Now;
            heartbeatThread = new Thread(this.ExecThread);
            heartbeatThread.IsBackground = true;
            heartbeatThread.Start();
        }

        public static Heartbeat GetInstance()
        {
            lock (typeof(Heartbeat))
            {
                if (instance == null)
                    instance = new Heartbeat();
            }

            return instance;
        }


        private void ExecThread()
        {
            Thread.Sleep(WARMUP_DELAY_MS);

            while (true)
            {
                try
                {

                    InstrumentationLogger.LogInfo("Heartbeat-Uptime-s",
                                                  DateTime.Now.Subtract(startTime)
                                                          .TotalSeconds.ToString(CultureInfo.InvariantCulture));

                    String usage = PackFrequencyDict(ComputeNodeFrequencies());
                    String errors = PackFrequencyDict(ComputeErrorFrequencies());


                    InstrumentationLogger.LogInfo("Node-usage", usage);
                    InstrumentationLogger.LogInfo("Nodes-with-errors", errors);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception in Heartbeat " + e);
                }
                Thread.Sleep(HEARTBEAT_INTERVAL_MS);


            }

        }

        /// <summary>
        /// Turn a frequency dictionary into a string that can be sent
        /// </summary>
        /// <param name="frequencies"></param>
        /// <returns></returns>
        private string PackFrequencyDict(Dictionary<String, int> frequencies)
        {
            StringBuilder sb = new StringBuilder();

            foreach (String key in frequencies.Keys)
            {
                sb.Append(key);
                sb.Append(":");
                sb.Append(frequencies[key]);
                sb.Append(",");
            }

            String ret = sb.ToString();
            return ret;
        }


        private Dictionary<String, int> ComputeNodeFrequencies()
        {

            Dictionary<String, int> ret = new Dictionary<string, int>();

            if (dynSettings.Controller == null || dynSettings.Controller.DynamoModel == null ||
                dynSettings.Controller.DynamoModel.AllNodes == null)
                return ret;

            foreach (var node in dynSettings.Controller.DynamoModel.AllNodes)
            {
                string fullName = node.GetType().FullName;
                if (!ret.ContainsKey(fullName))
                    ret[fullName] = 0;

                int count = ret[fullName];
                ret[fullName] = count + 1;
            }

            return ret;
        }

        private Dictionary<String, int> ComputeErrorFrequencies()
        {
            Dictionary<String, int> ret = new Dictionary<string, int>();

            if (dynSettings.Controller == null || dynSettings.Controller.DynamoModel == null ||
                dynSettings.Controller.DynamoModel.AllNodes == null)
                return ret;

            foreach (var node in dynSettings.Controller.DynamoModel.AllNodes)
            {
                if (node.State != ElementState.ERROR)
                    continue;

                string fullName = node.GetType().FullName;
                if (!ret.ContainsKey(fullName))
                    ret[fullName] = 0;
                
                int count = ret[fullName];
                ret[fullName] = count + 1;
            }

            return ret;
        }

    }

}
