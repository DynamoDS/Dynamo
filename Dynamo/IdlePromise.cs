using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Dynamo.Utilities
{
   public delegate T IdlePromiseDelegate<T>();

   delegate void _IdlePromiseDelegate();
   class _IdlePromise
   {
      static Queue<_IdlePromiseDelegate> promises = new Queue<_IdlePromiseDelegate>();

      static bool idleRegged = false;

      internal static void AddPromise(_IdlePromiseDelegate d)
      {
         if (!idleRegged)
         {
            dynElementSettings.SharedInstance.Doc.Application.Idling +=
               new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(Application_Idling);
            idleRegged = true;
         }

         promises.Enqueue(d);
      }

      static void Application_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
      {
         Thread.Sleep(1);
         try
         {
            while (HasPendingPromises())
            {
               promises.Dequeue()();
            }
         }
         finally
         {
            dynElementSettings.SharedInstance.Doc.Application.Idling -=
               new EventHandler<Autodesk.Revit.UI.Events.IdlingEventArgs>(Application_Idling);
            idleRegged = false;
         }
      }

      internal static bool HasPendingPromises()
      {
         return promises.Any();
      }
   }

   public static class IdlePromises
   {
      public static bool HasPendingPromises()
      {
         return _IdlePromise.HasPendingPromises();
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
