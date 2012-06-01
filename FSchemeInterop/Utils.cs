using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.FSharp.Collections;
using Expression = Dynamo.FScheme.Expression;

namespace Dynamo.FSchemeInterop
{
   //Miscellaneous helper and convenience methods.
   public static class Utils
   {
      //Makes an FScheme Expression representing an anonymous function.
      public static Expression MakeAnon(IEnumerable<string> inputSyms, Expression body)
      {
         return mkExprList(
            Expression.NewSymbol("lambda"),
            Expression.NewList(convertSequence(
               inputSyms.Select(x => Expression.NewSymbol(x))
            )),
            body
         );
      }

      //Makes an FScheme List Expression out of all given arguments.
      public static Expression mkExprList(params Expression[] ar)
      {
         return Expression.NewList(mkList(ar));
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
