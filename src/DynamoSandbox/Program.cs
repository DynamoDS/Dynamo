using System;
using System.Diagnostics;
using Dynamo;
using Dynamo.Utilities;

namespace DynamoSandbox
{
    class Program
    {
        static DynamoController dynamoController;

        [STAThread]
        static void Main(string[] args)
        {
            DynamoLogger.Instance.StartLogging();

            try
            {
                bool startWithUI = true;

                dynamoController = new DynamoController(new Dynamo.FSchemeInterop.ExecutionEnvironment(), startWithUI);

                if (startWithUI)
                {
                    dynSettings.Bench.ShowDialog();
                }
                
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }
    }
}
