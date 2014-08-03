using System;
using Dynamo.Controls;

namespace DynamoWebServer
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var WebSocketServer = new WebServer(new WebSocket());
            WebSocketServer.Start();
            
            DynamoView.MakeSandboxAndRun(null);
        }
    }
}
