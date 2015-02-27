using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamo.Utilities;

namespace Dynamo.Core.Threading
{
    internal static class AsyncTaskExtensions
    {
        internal static IDisposable Then(this AsyncTask task, AsyncTaskCompletedHandler action)
        {
            task.Completed += action;
            return Disposable.Create(() => task.Completed -= action);
        }

        internal static IDisposable ThenPost(this AsyncTask task, AsyncTaskCompletedHandler action, SynchronizationContext context = null)
        {
            if (context == null) context = new SynchronizationContext(); // uses the default
            return task.Then((t) => context.Post((_) => action(task), null));
        }

        internal static IDisposable ThenSend(this AsyncTask task, AsyncTaskCompletedHandler action, SynchronizationContext context = null)
        {
            if (context == null) context = new SynchronizationContext(); // uses the default
            return task.Then((t) => context.Send((_) => action(task), null));
        }
    }
}
