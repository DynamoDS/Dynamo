using System;
using System.Diagnostics;
using System.Windows;
using Dynamo;
using Dynamo.Controls;
using Dynamo.ViewModels;

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
                DynamoView.Start();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }
    }
}
