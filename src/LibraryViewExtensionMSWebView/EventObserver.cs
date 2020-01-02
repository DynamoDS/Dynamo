using System;
using System.Threading;

namespace Dynamo.LibraryViewExtensionMSWebBrowser
{
    /// <summary>
    /// A class that observes an event and notifies the clients with a transformed 
    /// data as configured using reduce function.
    /// </summary>
    /// <typeparam name="TValue">Type of value passed when event is raised</typeparam>
    /// <typeparam name="TResult">Type of transformed data to notify to the client</typeparam>
    class EventObserver<TValue, TResult> : IDisposable
    {
        private Timer throttler = null;
        private TimeSpan duetime;
        private bool accumulate = false;
        private bool disposed = false;

        private TResult result;
        private Func<TResult, TValue, TResult> reducer;
        private Action<TResult> observer;

        private object root = new object();

        /// <summary>
        /// This event is fired when this object is disposed, clients can 
        /// register a callback for Dispose.
        /// </summary>
        public event Action Disposed;

        /// <summary>
        /// Creates the event observer with a registered observe action and 
        /// a reduce function for notification.
        /// </summary>
        /// <param name="observe">This action will be invoked to notify the client</param>
        /// <param name="reduce">This function will be invoked to reduce the 
        /// actual event parameter together with last reduced value returned by this reducer.</param>
        public EventObserver(Action<TResult> observe, Func<TResult, TValue, TResult> reduce)
        {
            observer = observe;
            reducer = reduce;
            accumulate = true;
            result = default(TResult);
        }

        /// <summary>
        /// Creates the event observer with a registered observe action and 
        /// a reduce function for notification.
        /// </summary>
        /// <param name="observe">This action will be invoked to notify the client</param>
        /// <param name="transform">This function will be invoked to transform the 
        /// actual event parameter.</param>
        public EventObserver(Action<TResult> observe, Func<TValue, TResult> transform)
        {
            observer = observe;
            reducer = (x, y) => transform(y);
            result = default(TResult);
        }

        public static TValue Identity(TValue last, TValue current)
        {
            return current;
        }

        /// <summary>
        /// The client need to register this method as an event callback for 
        /// the event that needs to be observed.
        /// </summary>
        /// <param name="value">Event parameter passed</param>
        public void OnEvent(TValue value)
        {
            if (disposed) return; 

            lock (root) { result = reducer(result, value); }

            OnEventCore(value);
        }

        /// <summary>
        /// The core implementation for OnEvent, if throttle is active it will
        /// update the throttle timer with the due time, so that if no subsequent 
        /// event is raised within the given throttle due time, the client will 
        /// be notified.
        /// </summary>
        /// <param name="value"></param>
        protected virtual void OnEventCore(TValue value)
        {
            if (throttler != null)
            {
                throttler.Change(duetime, TimeSpan.FromMilliseconds(0));
            }
            else
            {
                Notify(this);
            }
        }

        /// <summary>
        /// Creates an observer that throttle the stream of notification 
        /// for a given duetime i.e. notification will be fired only if duetime
        /// is ellapsed from the last event trigger, if the event trigger is
        /// sooner then the observer will again wait for the given duetime
        /// to notify the client.
        /// </summary>
        /// <param name="duetime">Due time for throttle</param>
        /// <returns>The modified event Observer which throttle the events</returns>
        public EventObserver<TValue, TResult> Throttle(TimeSpan duetime)
        {
            if (disposed) throw new ObjectDisposedException("EventObserver");

            this.duetime = duetime;
            this.throttler = new Timer(Notify, this, Timeout.Infinite, Timeout.Infinite);
            //when throttled, reset the result after notification is fired.
            accumulate = false;
            return this;
        }

        /// <summary>
        /// Notify the client, could be triggered from throttle timer or 
        /// called directly by passing this object as state.
        /// </summary>
        /// <param name="state"></param>
        protected void Notify(object state)
        {
            if (disposed) throw new ObjectDisposedException("EventObserver");

            TResult r;
            lock (root)
            {
                r = result;
                if (!accumulate) result = default(TResult);
            }
            observer(r);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                if (Disposed != null) { Disposed(); }
                Disposed = null;
                observer = null;
                reducer = null;
            }
        }
    }

    /// <summary>
    /// Implements IDisposable to invoke a given dispose action on Dispose().
    /// </summary>
    class AnonymousDisposable : IDisposable
    {
        private Action dispose;

        public AnonymousDisposable(Action dispose)
        {
            this.dispose = dispose;
        }

        public void Dispose()
        {
            if(dispose != null) { dispose(); }
            dispose = null;
        }
    }
}
