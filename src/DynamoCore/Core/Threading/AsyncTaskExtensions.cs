using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamo.Utilities;

namespace Dynamo.Core.Threading
{
    /// <summary>
    ///     Tools for working productively with AsyncTask's
    /// </summary>
    internal static class AsyncTaskExtensions
    {
        /// <summary>
        ///     Upon completion of the task, invoke the specified action
        /// </summary>
        /// <returns>An IDisposable representing the event subscription</returns>
        internal static IDisposable Then(this AsyncTask task, AsyncTaskCompletedHandler action)
        {
            task.Completed += action;
            return Disposable.Create(() => task.Completed -= action);
        }

        /// <summary>
        ///     Upon completion of the task, invoke the action asynchronously in the specified
        ///     SynchronizationContext
        /// </summary>
        /// <returns>An IDisposable representing the event subscription</returns>
        internal static IDisposable ThenPost(this AsyncTask task, AsyncTaskCompletedHandler action, SynchronizationContext context = null)
        {
            if (context == null) context = new SynchronizationContext(); // uses the default
            return task.Then((t) => context.Post((_) => action(task), null));
        }

        /// <summary>
        ///     Upon completion of the task, invoke the action synchronously in the specified
        ///     SynchronizationContext
        /// </summary>
        /// <returns>An IDisposable representing the event subscription</returns>
        internal static IDisposable ThenSend(this AsyncTask task, AsyncTaskCompletedHandler action, SynchronizationContext context = null)
        {
            if (context == null) context = new SynchronizationContext(); // uses the default
            return task.Then((t) => context.Send((_) => action(task), null));
        }

        /// <summary>
        ///     Await the completion of a collection of scheduled tasks.  The given action
        ///     will only be executed after all events are complete or discarded.  The tasks
        ///     must already be scheduled and not yet completed or this action will never be 
        ///     executed.
        /// </summary>
        /// <returns>An IDisposable representing all of the event subscription</returns>
        internal static IDisposable AllComplete(this IEnumerable<AsyncTask> tasks, Action<IEnumerable<AsyncTask>> action)
        {
            // If the task list is empty, we immediately invoke the action
            if (!tasks.Any())
            {
                action(tasks);
            }

            // We'll have to keep track of the event subscriptions for disposal
            var callbacks = new List<Tuple<AsyncTask, AsyncTaskCompletedHandler>>();

            // We'll perform an asynchronous count down
            var count = tasks.Count();

            // Because of task parallelism, we need to synchronize access to the count
            var mutex = new object();

            // Subscribe to all of the tasks' Completed event
            foreach (var task in tasks)
            {
                AsyncTaskCompletedHandler innerAction = (_) =>
                {
                    lock (mutex)
                    {
                        // Decrement the count
                        count--;
                    }

                    // If count is 0, we're done running tasks
                    if (count == 0)
                    {
                        action(tasks);
                    }
                };

                // Store the subscription
                callbacks.Add(new Tuple<AsyncTask, AsyncTaskCompletedHandler>(task, innerAction));

                // Subscribe!
                task.Completed += innerAction;
                task.Discarded += innerAction;
            }

            // This disposable will unsubscribe from all of the Completed, Discarded events
            return Disposable.Create(() =>
            {
                foreach (var t in callbacks)
                {
                    t.Item1.Completed -= t.Item2;
                    t.Item1.Discarded -= t.Item2;
                }
            });
        }
    }
}
