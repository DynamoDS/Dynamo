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

        internal static IDisposable AllComplete(this IEnumerable<AsyncTask> tasks, Action<IEnumerable<AsyncTask>> action)
        {
            if (!tasks.Any())
            {
                action(tasks);
            }

            var cbs = new List<Tuple<AsyncTask, AsyncTaskCompletedHandler>>();

            var mutex = new object();
            var count = tasks.Count();

            foreach (var task in tasks)
            {
                AsyncTaskCompletedHandler innerAction = (_) =>
                {
                    lock (mutex)
                    {
                        count -= 1;
                        if (count == 0)
                        {
                            action(tasks);
                        }
                    }
                };

                cbs.Add(new Tuple<AsyncTask, AsyncTaskCompletedHandler>(task, innerAction));

                task.Completed += innerAction;
            }

            return Disposable.Create(() =>
            {
                foreach (var cbt in cbs)
                {
                    cbt.Item1.Completed -= cbt.Item2;
                }
            });
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
