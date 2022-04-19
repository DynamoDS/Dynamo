using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoFeatureFlags
{

    class Options
    {
        [Option('u', "userkey", Required = false, HelpText = "stable user key, if not provided, a shared key will be used.")]
        public string UserKey { get; set; }
        [Option('m', "mobilekey", Required = false, HelpText = "mobile key for dynamo feature flag env. Do not use a full sdk key. If not provided loaded from config.")]
        public string MobileKey { get; set; }
        [Option('p',"proccessID",Required = true, HelpText= "parent proccess id, if this proccess is no longer running, this application will exit.")]
        public int ProccessID { get; set; }
    }
    static class Program
    {
        static FeatureFlagsClient FeatureFlags { get; set; }
        private const string endOfDataToken = @"<<<<<Eod>>>>>";
        private const string startOfDataToken = @"<<<<<Sod>>>>>";
        private static int hostProccessId = -1;
        static void Main(string[] args)
        {
            //System.Diagnostics.Debugger.Launch();
            Parser.Default.ParseArguments<Options>(args).WithParsed(ops =>
            {
                try
                {
                    //start watchdog
                    hostProccessId = ops.ProccessID;
                    var timer = new System.Timers.Timer(10000);
                    timer.AutoReset = true;
                    timer.Elapsed += Timer_Elapsed;
                    timer.Enabled = true;

                    FeatureFlagsClient.MessageLogged += FeatureFlagsManager_MessageLogged;
                   
                    FeatureFlags = new FeatureFlagsClient(ops.UserKey, ops.MobileKey);
                    Console.WriteLine("feature flag exe starting");
                    SendAllFlags();

                }
                catch (Exception e)
                {
                    // Exit process
                }
            });
                
        }

        private static void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var currentproc = Process.GetCurrentProcess();
            var hostAlive = Process.GetProcesses().Any(x => x.Id == Program.hostProccessId);
            if (!hostAlive)
            {
                //exit
                currentproc.Kill();
            }

        }

        private static void SendAllFlags()
        {
            Console.WriteLine(startOfDataToken);
            Console.WriteLine(FeatureFlags.GetAllFlagsAsJSON());
            Console.WriteLine(endOfDataToken);
        }

        private static void FeatureFlagsManager_MessageLogged(string message)
        {
            Console.WriteLine(message);
        }
    }
}
