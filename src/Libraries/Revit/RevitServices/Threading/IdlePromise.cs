//Copyright © Autodesk, Inc. 2012. All rights reserved.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace RevitServices.Threading
{
    public delegate T IdlePromiseDelegate<out T>();
    
    /// <summary>
    /// Dispatches delegates in the Revit Idle thread.
    /// </summary>
    public static class IdlePromise
    {
        internal static Queue<Action> Promises = new Queue<Action>();
        internal static Queue<Action> ShutdownPromises = new Queue<Action>();

        public static bool InIdleThread { get; internal set; }

        private static void Application_Idling(object sender, IdlingEventArgs e)
        {
            InIdleThread = true;
            Thread.Sleep(1);
            while (HasPendingPromises())
            {
                Promises.Dequeue()();
            }
            InIdleThread = false;
        }

        internal static void Register(UIControlledApplication uIApplication)
        {
            uIApplication.Idling += Application_Idling;
        }

        /// <summary>
        /// Are there currently promises queued for Idle thread invocation?
        /// </summary>
        public static bool HasPendingPromises()
        {
            return Promises.Any();
        }

        /// <summary>
        /// Clears the Idle Promise queue of all pending invocations.
        /// </summary>
        public static void ClearPromises()
        {
            Promises.Clear();
        }

        public static void ClearShutdownPromises()
        {
            ShutdownPromises.Clear();
        }

        public static bool HasPendingShutdownPromises()
        {
            return ShutdownPromises.Any();
        }

        /// <summary>
        /// Sets the Idle thread invocater to use the Idle event of the given UIControlledApplication.
        /// </summary>
        /// <param name="uIApplication">UIControlledApplication to use the Idle event of.</param>
        public static void RegisterIdle(UIControlledApplication uIApplication)
        {
            Register(uIApplication);
        }

        /// <summary>
        /// Executes the given Action delegate on the Idle thread asynchronously.
        /// </summary>
        /// <param name="p">Delegate to be invoked on the Idle thread.</param>
        public static void ExecuteOnIdleAsync(Action p)
        {
            Promises.Enqueue(p);
        }

        /// <summary>
        /// Dispatches the given IdlePromiseDelegate on the Revit Idle thread asynchronously.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="p"></param>
        /// <returns></returns>
        public static IdlePromise<T> ExecuteOnIdleAsync<T>(IdlePromiseDelegate<T> p)
        {
            return new IdlePromise<T>(p);
        }

        /// <summary>
        /// Executes the given Action delegate on the Idle thread and blocks the calling
        /// thread until its invocation is complete.
        /// </summary>
        /// <param name="p">Delefate to be invoked on the Idle thread.</param>
        public static void ExecuteOnIdleSync(Action p)
        {
            bool redeemed = false;

            Promises.Enqueue(
                delegate
                {
                    p();
                    redeemed = true;
                });

            while (!redeemed)
            {
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Dispatches the given IdlePromiseDelegate on the Revit Idle thread synchronously.
        /// </summary>
        /// <param name="p">Delegate to be invoked on the Idle thread.</param>
        /// <returns>Result of the delegate.</returns>
        public static T ExecuteOnIdleSync<T>(IdlePromiseDelegate<T> p)
        {
            return new IdlePromise<T>(p).RedeemPromise();
        }

        public static void ExecuteOnShutdown(Action p)
        {
            ShutdownPromises.Enqueue(p);
        }

        //shuffle all the shutdown promises onto the 
        //normal queue to be processed
        public static void Shutdown()
        {
            foreach (var p in ShutdownPromises)
            {
                ExecuteOnIdleAsync(p);
            }

            ClearShutdownPromises();
        }
    }

    /// <summary>
    /// Dispatches delegates in the Revit Idle thread.
    /// </summary>
    /// <typeparam name="T">Return type of the delegate to be dispatched.</typeparam>
    public class IdlePromise<T>
    {
        private T value;
        private bool redeemed;

        /*
        /// <summary>
        ///     Automatically attempt to redeem the promise when it is used.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static implicit operator T(IdlePromise<T> p)
        {
            return p.RedeemPromise();
        }
        // This is really cool but probably not the most usable. --SJE
        */

        /// <summary>
        /// Creates a new IdlePromise out of the given delegate, immediately queuing its dispatch
        /// on the Idle thread asynchronously.
        /// </summary>
        /// <param name="d">Delegate to be invoked on the Idle thread.</param>
        internal IdlePromise(IdlePromiseDelegate<T> d)
        {
            redeemed = false;
            value = default(T);

            IdlePromise.ExecuteOnIdleAsync(
                delegate
                {
                    value = d();
                    redeemed = true;
                });
        }

        /// <summary>
        /// Blocks the calling thread until the promise has completed its invocation
        /// on the idle thread.
        /// </summary>
        /// <returns>The result of the promise delegate.</returns>
        public T RedeemPromise()
        {
            while (!redeemed)
            {
                Thread.Sleep(1);
            }

            return value;
        }
    }
}