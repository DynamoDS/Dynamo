using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Dynamo.Utilities
{
   public delegate T IdlePromiseDelegate<T>();

   class _IdlePromise
   {
      static Queue<Action> promises = new Queue<Action>();

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

      internal static void register(Autodesk.Revit.UI.UIApplication uIApplication)
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

      internal static void RegisterIdle(Autodesk.Revit.UI.UIApplication uIApplication)
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

         return this.value;
      }

      public static T ExecuteOnIdle(IdlePromiseDelegate<T> p)
      {
         return new IdlePromise<T>(p).RedeemPromise();
      }
   }
}
