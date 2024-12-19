using System;
using System.Threading;
using System.Windows.Threading;

namespace Dynamo.Wpf.Utilities
{
    internal class ActionDebouncer : IDisposable
    {
        private readonly Dispatcher dispatcher;
        private CancellationTokenSource cts;

        public ActionDebouncer(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public void Cancel()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }

        public void Debounce(int timeout, Action action)
        {
            Cancel();
            cts = new CancellationTokenSource();
            System.Threading.Tasks.Task.Delay(timeout, cts.Token).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    dispatcher.BeginInvoke(action);
                }
            });
        }

        public void Dispose()
        {
            Cancel();
        }
    }
}
