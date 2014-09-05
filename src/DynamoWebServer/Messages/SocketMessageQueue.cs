using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace DynamoWebServer.Messages
{
    class SocketMessageQueue
    {
        private readonly object locker = new object();

        private Queue<Action> messageQueue;
        private Thread worker;

        public SocketMessageQueue()
        {
            messageQueue = new Queue<Action>();
            worker = new Thread(Consume);
            worker.Start();
        }

        public void Shutdown()
        {
            lock (locker)
            {
                messageQueue.Clear();
                messageQueue.Enqueue(null);
                Monitor.Pulse(locker);
            }
        }

        public void EnqueueItem(Action message)
        {
            lock (locker)
            {
                messageQueue.Enqueue(message);
                Monitor.Pulse(locker);
            }
        }

        private void Consume()
        {
            while (true)
            {
                Action message;                               
                lock (locker)
                {
                    while (messageQueue.Count == 0)
                    {
                        Monitor.Wait(locker);
                    }
                    message = messageQueue.Dequeue();
                }
                if (message == null)
                {
                    return;
                }
                (Application.Current != null ? Application.Current.Dispatcher : Dispatcher.CurrentDispatcher).Invoke(message);
            }
        }
    }
}
