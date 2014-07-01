using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamo.Models;
using Dynamo.Properties;
using Dynamo.UpdateManager;
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

        // We need to play nicely with the background thread and ask politely 
        // when the application is shutting down. Whenever this event is raised,
        // the thread loop exits, causing the thread to terminate. Since garbage
        // collector does not collect an unreferenced thread, setting "instance"
        // to "null" will not help release the thread.
        // 
        private AutoResetEvent shutdownEvent = new AutoResetEvent(false);

        private Heartbeat()
        {
            startTime = DateTime.Now;
            heartbeatThread = new Thread(this.ExecThread);
            heartbeatThread.IsBackground = true;
            heartbeatThread.Start();
        }

        private void DestroyInternal()
        {
            shutdownEvent.Set(); // Signal the shutdown event... 
            heartbeatThread.Join(); // ... wait for thread to end.
            heartbeatThread = null;
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

        public static void DestroyInstance()
        {
            lock (typeof(Heartbeat))
            {
                if (instance != null)
                {
                    instance.DestroyInternal();
                    instance = null;
                }
            }
        }

        private void ExecThread()
        {
            Thread.Sleep(WARMUP_DELAY_MS);

            while (true)
            {
                try
                {
                    InstrumentationLogger.LogAnonymousEvent("Heartbeat", "ApplicationLifeCycle", GetVersionString() );

                    String usage = PackFrequencyDict(ComputeNodeFrequencies());
                    String errors = PackFrequencyDict(ComputeErrorFrequencies());

                    InstrumentationLogger.LogPiiInfo("Node-usage", usage);
                    InstrumentationLogger.LogPiiInfo("Nodes-with-errors", errors);

                    string workspace =
                        dynSettings.Controller.DynamoModel.CurrentWorkspace
                                   .GetStringRepOfWorkspaceSync();

                    InstrumentationLogger.LogPiiInfo("Workspace", workspace);


                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception in Heartbeat " + e);
                }

                // The following call will return "true" if the event is 
                // signaled, which can only happen when "DestroyInternal" 
                // is called as the application is shutting down. Otherwise,
                // when the wait time ellapsed, the loop continues to log 
                // the next set of information.
                // 
                if (shutdownEvent.WaitOne(HEARTBEAT_INTERVAL_MS))
                    break;
            }
        }

        private string GetVersionString()
        {
            try
            {
                string executingAssemblyPathName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(executingAssemblyPathName);
                return myFileVersionInfo.FileVersion;
            }
            catch (Exception)
            {
                return "";
            }
        }



        /// <summary>
        /// Turn a frequency dictionary into a string that can be sent
        /// </summary>
        /// <param name="frequencies"></param>
        /// <returns></returns>
        private string PackFrequencyDict(Dictionary<String, int> frequencies)
        {
            //@TODO(Luke): Merge with ComputeNodeFrequencies http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3842
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
            //@TODO(Luke): Merge with ComputeNodeFrequencies http://adsk-oss.myjetbrains.com/youtrack/issue/MAGN-3842

            Dictionary<String, int> ret = new Dictionary<string, int>();

            if (dynSettings.Controller == null || dynSettings.Controller.DynamoModel == null ||
                dynSettings.Controller.DynamoModel.AllNodes == null)
                return ret;

            foreach (var node in dynSettings.Controller.DynamoModel.AllNodes)
            {
                string fullName = node.NickName;
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
                if (node.State != ElementState.Error)
                    continue;

                string fullName = node.NickName;
                if (!ret.ContainsKey(fullName))
                    ret[fullName] = 0;
                
                int count = ret[fullName];
                ret[fullName] = count + 1;
            }

            return ret;
        }

    }

}
