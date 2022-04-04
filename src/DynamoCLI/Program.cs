﻿using System;
using Dynamo.Applications;
using Dynamo.Models;

namespace DynamoCLI
{
    internal class Program
    {
        [STAThread]
        static internal void Main(string[] args)
        {
            try
            {
                var cmdLineArgs = StartupUtils.CommandLineArguments.Parse(args);
                var locale = StartupUtils.SetLocale(cmdLineArgs);
                if (cmdLineArgs.DisableAnalytics)
                {
                    Dynamo.Logging.Analytics.DisableAnalytics = true;
                }

                DynamoModel model;
                if (!String.IsNullOrEmpty(cmdLineArgs.ASMPath))
                {
                    model = Dynamo.Applications.StartupUtils.MakeModel(true, cmdLineArgs.ASMPath, cmdLineArgs.AnalyticsInfo);
                }
                else
                {
                    model = Dynamo.Applications.StartupUtils.MakeModel(true, string.Empty, cmdLineArgs.AnalyticsInfo);
                }
                var runner = new CommandLineRunner(model);
                runner.Run(cmdLineArgs);
                
            }
            catch (Exception e)
            {
                try
                {
                    DynamoModel.IsCrashing = true;
                    Dynamo.Logging.Analytics.TrackException(e, true);
                }
                catch
                {
                }

                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

    }
}
