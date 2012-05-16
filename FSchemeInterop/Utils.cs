using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.FSharp.Collections;

namespace Dynamo.FSchemeInterop
{
   //Miscellaneous helper and convenience methods.
   public static class Utils
   {
      //Makes an FScheme Expression representing an anonymous function.
      public static FScheme.Expression MakeAnon(IEnumerable<string> inputSyms, FScheme.Expression body)
      {
         return mkExprList(
            FScheme.Expression.NewSymbol("lambda"),
            FScheme.Expression.NewList(convertSequence(
               inputSyms.Select(x => FScheme.Expression.NewSymbol(x))
            )),
            body
         );
      }

      //Makes an FScheme List Expression out of all given arguments.
      public static FScheme.Expression mkExprList(params FScheme.Expression[] ar)
      {
         return FScheme.Expression.NewList(mkList(ar));
      }

      //Makes an FSharp list from all given arguments.
      public static FSharpList<T> mkList<T>(params T[] ar)
      {
         FSharpList<T> foo = FSharpList<T>.Empty;
         for (int n = ar.Length - 1; n >= 0; n--)
            foo = FSharpList<T>.Cons(ar[n], foo);
         return foo;
      }

      //Converts the gicven IEnumerable into an FSharp list.
      public static FSharpList<T> convertSequence<T>(IEnumerable<T> seq)
      {
         FSharpList<T> result = FSharpList<T>.Empty;
         foreach (T element in seq.Reverse<T>())
         {
            result = FSharpList<T>.Cons(element, result);
         }
         return result;
      }
   }
}
