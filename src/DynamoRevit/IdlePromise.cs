using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autodesk.Revit.UI;

namespace Dynamo.Utilities
{
   public delegate T IdlePromiseDelegate<T>();

   class _IdlePromise
   {
      internal static Queue<Action> promises = new Queue<Action>();
      internal static Queue<Action> shutdown_promises = new Queue<Action>();
      
      internal static void AddShutdownPromise(Action d)
      {
          shutdown_promises.Enqueue(d);
      }

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

      internal static bool HasPendingShutdownPromises()
      {
          return shutdown_promises.Any();
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

       public static void ClearShutdownPromises()
       {
           _IdlePromise.shutdown_promises.Clear();
       }

       public static bool HasPendingShutdownPromises()
       {
           return _IdlePromise.HasPendingShutdownPromises();
       }

      public static void RegisterIdle(UIControlledApplication uIApplication)
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

      public static void ExecuteOnShutdown(Action p)
      {
          _IdlePromise.AddShutdownPromise(p);
      }

       //shuffle all the shutdown promises onto the 
       //normal queue to be processed
        public static void Shutdown()
        {
            foreach (var p in _IdlePromise.shutdown_promises)
            {
                ExecuteOnIdle(p);
            }

            ClearShutdownPromises();
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
