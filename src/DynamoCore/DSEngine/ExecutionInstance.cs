using Dynamo.Core.IPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Dynamo.DSEngine
{
    /// <summary>
    /// ExecutionInstance's job is to run the graph.
    /// </summary>
    class ExecutionInstance
    {
        private Queue<Message> messageQueue = new Queue<Message>();
        private Thread messageThread;

        public void Run()
        {
            messageThread = new Thread(new ThreadStart(MessageLoop));
            messageThread.IsBackground = true;
            messageThread.Start();
        }

        public void MessageLoop()
        {
            while (true)
            {
                lock (messageQueue)
                {
                    if (messageQueue.Count > 0)
                    {
                        var msg = messageQueue.Dequeue();
                        // handle message
                    }
                }
                Thread.Sleep(1);
            }
        }

        public void WaitForExit()
        {
            if (messageThread != null && messageThread.IsAlive)
                messageThread.Join();
        }
    }
}
