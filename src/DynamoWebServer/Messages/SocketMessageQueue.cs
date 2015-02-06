using System;
using System.Collections.Generic;
using System.Threading;

namespace DynamoWebServer.Messages
{
    class SocketMessageQueue
    {
        // One to signal message availability, another for shutdown.
        private readonly AutoResetEvent[] waitHandles = 
        {
            new AutoResetEvent(false), // For message availability
            new AutoResetEvent(false)  // For shutdown event
        };

        private readonly object locker = new object();

        private enum HandleIndex
        {
            MessageAvailable, Shutdown
        }

        private readonly Queue<Action> messageQueue = new Queue<Action>();
        private readonly Thread worker;

        public SocketMessageQueue()
        {
            worker = new Thread(Consume);
            worker.Start();
        }

        public void Shutdown()
        {
            lock (locker)
            {
                messageQueue.Clear();
            }
            waitHandles[(int)HandleIndex.Shutdown].Set(); // Shutdown requested
            worker.Join(); // Wait for thread to end.
        }

        public void EnqueueMessage(Action message)
        {
            lock (locker)
            {
                messageQueue.Enqueue(message);
                waitHandles[(int)HandleIndex.MessageAvailable].Set(); // A message is available.
            }
        }

        private void Consume()
        {
            while (true)
            {
                Action message = null;
                lock (locker)
                {
                    if (messageQueue.Count > 0)
                        message = messageQueue.Dequeue();
                }
                if (message != null)
                {
                    message();
                    continue;
                }
                // No more messages, go into wait.
                int eventIndex = WaitHandle.WaitAny(waitHandles);
                if (eventIndex == 1) // Shutdown event.
                    break;

                // Otherwise, pick up the next message.
            }
        }
    }
}
