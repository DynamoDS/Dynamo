using System;
using System.Diagnostics;
using Dynamo;
using Dynamo.Controls;

namespace DynamoSandbox
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            DynamoLogger.Instance.StartLogging();

            try
            {
                DynamoView.MakeSandboxAndRun();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }
    }
}
