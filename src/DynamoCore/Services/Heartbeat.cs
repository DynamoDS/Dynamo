using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

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
                InstrumentationLogger.LogInfo("Heartbeat-Uptime-s", DateTime.Now.Subtract(startTime).TotalSeconds.ToString(CultureInfo.InvariantCulture));
                Thread.Sleep(HEARTBEAT_INTERVAL_MS);
            }



        }



    }

}
