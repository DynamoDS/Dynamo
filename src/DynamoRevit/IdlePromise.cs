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
using System.Diagnostics;

namespace Dynamo.Utilities
{
   public delegate T IdlePromiseDelegate<T>();

   class _IdlePromise
   {
      internal static Queue<Action> promises = new Queue<Action>();

      internal static void AddPromise(Action d)
      {
         promises.Enqueue(d);
      }

      static void Application_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
      {
         Thread.Sleep(1);
         while (HasPendingPromises())
         {
            promises.Dequeue()();
         }
      }

      internal static bool HasPendingPromises()
      {
         return promises.Any();
      }

      internal static void register(Autodesk.Revit.UI.UIControlledApplication uIApplication)
      {
         uIApplication.Idling += new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(Application_Idling);
      }
   }

   public static class IdlePromise
   {
      public static bool HasPendingPromises()
      {
         return _IdlePromise.HasPendingPromises();
      }

      public static void ClearPromises()
      {
          _IdlePromise.promises.Clear();
      }

      public static void RegisterIdle(Autodesk.Revit.UI.UIControlledApplication uIApplication)
      {
         _IdlePromise.register(uIApplication);
      }

      public static void ExecuteOnIdle(Action p, bool async = true)
      {
         bool redeemed = false;

         _IdlePromise.AddPromise(
            delegate
            {
               p();
               redeemed = true;
            }
         );

         if (!async)
         {
            while (!redeemed)
            {
               Thread.Sleep(1);
            }
         }
      }
   }

   public class IdlePromise<T>
   {
      private T value;
      private bool redeemed;

      public IdlePromise(IdlePromiseDelegate<T> d)
      {
         this.redeemed = false;
         this.value = default(T);

         _IdlePromise.AddPromise(
            delegate
            {
               this.value = d();
               this.redeemed = true;
            }
         );
      }

      public static bool HasPendingPromises()
      {
         return _IdlePromise.HasPendingPromises();
      }

      public T RedeemPromise()
      {
         while (!this.redeemed)
         {
            Thread.Sleep(1);
         }

         DynamoLogger.Instance.Log(string.Format("Promise redeemed for {0}", this.value.ToString()));
         return this.value;
      }

      public static T ExecuteOnIdle(IdlePromiseDelegate<T> p)
      {
          DynamoLogger.Instance.Log(string.Format("Redeeming promise for {0}", p.GetType().ToString()));
         return new IdlePromise<T>(p).RedeemPromise();
      }
   }
}
