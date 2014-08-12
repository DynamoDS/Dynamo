using System;
using Dynamo.Controls;

namespace DynamoWebServer
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var webSocketServer = new WebServer(new WebSocket(), new SessionManager());
            webSocketServer.Start();
            
            DynamoView.MakeSandboxAndRun(null);
        }
    }
}
