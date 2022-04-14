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
        static FeatureFlagsManager FeatureFlags { get; set; }
        private const string checkFeatureFlagCommandToken = @"<<<<<CheckFeatureFlag>>>>>";
        private const string endOfDataToken = @"<<<<<Eod>>>>>";
        private const string startOfDataToken = @"<<<<<Sod>>>>>";
        private static int hostProccessId = -1;
        static void Main(string[] args)
        {
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

                    FeatureFlagsManager.MessageLogged += FeatureFlagsManager_MessageLogged;
                   
                    FeatureFlags = new FeatureFlagsManager(ops.UserKey, ops.MobileKey);
                    Console.WriteLine("");
                    while (true)
                    {
                       
                        var line = Console.ReadLine();
                        if (line == checkFeatureFlagCommandToken)
                        {
                            CheckFeatureFlag();
                        }
                        Console.WriteLine(endOfDataToken);
                    }

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

        static void CheckFeatureFlag()
        {
            var data = GetData();
            var array = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var ffkey = array[0];
            var defaultValString = array[1];
            var typeName = array[2];
            var type = Type.GetType(typeName);
            //convert default val to correct type.
            var defaultValTyped = Convert.ChangeType(defaultValString, type);

            // invoke it, we'll get our flag or the default back.
            var output = FeatureFlagsManager.CheckFeatureFlag(ffkey, type, defaultValTyped);
            Console.WriteLine(startOfDataToken);
            Console.WriteLine(output);
        }

        static string GetData()
        {
            using (StringWriter data = new StringWriter())
            {
                while (true)
                {
                    var line = Console.ReadLine();
                    if (line == endOfDataToken)
                    {
                        break;
                    }
                    data.WriteLine(line);
                }

                return data.ToString();
            }
        }

        private static void FeatureFlagsManager_MessageLogged(string message)
        {
            Console.WriteLine(message);
        }
    }
}
