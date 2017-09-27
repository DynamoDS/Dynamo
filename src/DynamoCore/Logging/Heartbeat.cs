using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamo.Graph.Nodes;
using Dynamo.Models;

namespace Dynamo.Logging
{
    /// <summary>
    /// Class to automatically report various metrics of the application usage
    /// </summary>
    internal class Heartbeat
    {
        private static Heartbeat instance;
        private const int WARMUP_DELAY_MS = 5000;
        private const int HEARTBEAT_INTERVAL_MS = 60 * 1000;
        private readonly DateTime startTime;
        private Thread heartbeatThread;
        private readonly DynamoModel dynamoModel;

        private readonly AutoResetEvent shutdownEvent = new AutoResetEvent(false);

        private Heartbeat(DynamoModel dynamoModel)
        {
            // KILLDYNSETTINGS - this is provisional - but we need to enforce that Hearbeat is 
            // not referencing multiple DynamoModels
            this.dynamoModel = dynamoModel;

            startTime = DateTime.Now;
            heartbeatThread = new Thread(this.ExecThread);
            heartbeatThread.IsBackground = true;
            heartbeatThread.Start();
        }

        private void DestroyInternal()
        {
            System.Diagnostics.Debug.WriteLine("Heartbeat Destory Internal called");

            shutdownEvent.Set(); // Signal the shutdown event... 

            // TODO: Temporary comment out this Join statement. It currently 
            // causes Dynamo to go into a deadlock when it is shutdown for the 
            // second time on Revit (that's when the HeartbeatThread is trying 
            // to call 'GetStringRepOfWorkspaceSync' below (the method has no 
            // chance of executing, and therefore, will never return due to the
            // main thread being held up here waiting for the heartbeat thread 
            // to end).
            // 
            // heartbeatThread.Join(); // ... wait for thread to end.

            heartbeatThread = null;
        }

        public static Heartbeat GetInstance(DynamoModel dynModel)
        {
            lock (typeof(Heartbeat))
            {
                if (instance == null)
                    instance = new Heartbeat(dynModel);
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
                    StabilityCookie.WriteUptimeBeat(DateTime.Now.Subtract(startTime));

                    //Disable heartbeat to avoid 150 event/session limit
                    //InstrumentationLogger.LogAnonymousEvent("Heartbeat", "ApplicationLifeCycle", GetVersionString());

                    String usage = PackFrequencyDict(ComputeNodeFrequencies());
                    String errors = PackFrequencyDict(ComputeErrorFrequencies());

                    Analytics.LogPiiInfo("Node-usage", usage);
                    Analytics.LogPiiInfo("Nodes-with-errors", errors);

                    DynamoModel.OnRequestDispatcherInvoke(
                        () =>
                        {
                            string workspace = dynamoModel.CurrentWorkspace == null ? string.Empty :
                                dynamoModel.CurrentWorkspace
                                    .GetStringRepOfWorkspace();
                            Analytics.LogPiiInfo("Workspace", workspace);
                        });

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

            if (dynamoModel == null)
                return ret;

            foreach (var node in dynamoModel.Workspaces.SelectMany(ws => ws.Nodes))
            {
                string fullName = node.Name;
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

            if (dynamoModel == null)
                return ret;

            foreach (var node in dynamoModel.Workspaces.SelectMany(ws => ws.Nodes))
            {
                if (node.State != ElementState.Error)
                    continue;

                string fullName = node.Name;
                if (!ret.ContainsKey(fullName))
                    ret[fullName] = 0;
                
                int count = ret[fullName];
                ret[fullName] = count + 1;
            }

            return ret;
        }

    }

}
