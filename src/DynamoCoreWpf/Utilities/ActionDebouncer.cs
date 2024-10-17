using System;
using System.Threading;
using System.Windows.Threading;

namespace Dynamo.Wpf.Utilities
{
    internal class ActionDebouncer
    {
        Dispatcher _dispatcher;
        CancellationTokenSource _cts;
        public ActionDebouncer(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void Debounce(int timeout, Action action)
        {
            if (_cts != null)
                _cts.Cancel();
            _cts = new CancellationTokenSource();
            System.Threading.Tasks.Task.Delay(timeout, _cts.Token).ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                    _dispatcher.Invoke(action);
            });
        }
    }
}
