using System;
using System.Diagnostics;
using Dynamo;
using Dynamo.Controls;
using Dynamo.Utilities;

namespace DynamoSandbox
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            DynamoLogger.Instance.StartLogging();

            try
            {
                new DynamoController(new Dynamo.FSchemeInterop.ExecutionEnvironment(), true, typeof(DynamoViewModel), Context.NONE);
                dynSettings.Bench.ShowDialog();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }
    }
}
