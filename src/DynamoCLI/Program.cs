using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamo.Applications;
using System.Diagnostics;
using Dynamo.Models;
using Dynamo.Services;
using System.Text.RegularExpressions;
using Dynamo.Logging;

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
                var locale = Dynamo.Applications.StartupUtils.SetLocale(cmdLineArgs);
                var model = Dynamo.Applications.StartupUtils.MakeModel(true);
                var runner = new CommandLineRunner(model);
                runner.Run(cmdLineArgs);
                
            }
            catch (Exception e)
            {
                try
                {
                    DynamoModel.IsCrashing = true;
                    InstrumentationLogger.LogException(e);
                    StabilityTracking.GetInstance().NotifyCrash();

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
