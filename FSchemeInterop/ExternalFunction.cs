using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Expression = Dynamo.FScheme.Expression;

using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;

namespace Dynamo.FSchemeInterop
{
   //Delegate respresenting an External Function which can be called by FScheme.
   //public delegate Expression ExternFunc(FSharpList<Expression> args);

   public delegate Expression ExternMacro(
      FSharpList<Expression> args,
      ExecutionEnvironment environment
   );

   //Delegate representing a Continuation which can be called by FScheme.
   public delegate Expression Continuation(Expression arg);

   //Class containing helper methods for creating usable External Functions.
   public static class FuncContainer
   {
      //Returns an FScheme Expression which wraps the given ExternFunc, so that it
      //can be called by FScheme.
      public static Expression MakeFunction(FScheme.ExternFunc func)
      {
         //return Expression.NewFunction(
         //   FuncConvert.ToFSharpFunc(
         //      delegate(FSharpFunc<Expression, Expression> c)
         //      {
         //         return FuncConvert.ToFSharpFunc(
         //            delegate(FSharpList<Expression> args)
         //            {
         //               return c.Invoke(func(args));
         //            }
         //         );
         //      }
         //   )
         //);
         return FScheme.makeExternFunc(func);
      }

      public static Expression MakeMacro(FScheme.ExternMacro macro)
      {
         //return Expression.NewSpecial(
         //   FuncConvert.ToFSharpFunc(
         //     delegate(FSharpFunc<Expression, Expression> c)
         //     {
         //        return FuncConvert.ToFSharpFunc(
         //           delegate(FSharpList<FSharpRef<FSharpMap<string, FSharpRef<Expression>>>> env)
         //           {
         //              return FuncConvert.ToFSharpFunc(
         //                 delegate(FSharpList<Expression> args)
         //                 {
         //                    return c.Invoke(macro(args, env));
         //                 }
         //              );
         //           }
         //        );
         //     }
         //   )
         //);
         return FScheme.makeExternMacro(macro);
      }

      public static Expression MakeMacro(ExternMacro macro)
      {
         //return Expression.NewSpecial(
         //   FuncConvert.ToFSharpFunc(
         //     delegate(FSharpFunc<Expression, Expression> c)
         //     {
         //        return FuncConvert.ToFSharpFunc(
         //           delegate(FSharpList<FSharpRef<FSharpMap<string, FSharpRef<Expression>>>> env)
         //           {
         //              return FuncConvert.ToFSharpFunc(
         //                 delegate(FSharpList<Expression> args)
         //                 {
         //                    return c.Invoke(macro(args, env));
         //                 }
         //              );
         //           }
         //        );
         //     }
         //   )
         //);
         return FScheme.makeExternMacro(new FScheme.ExternMacro(
            delegate(FSharpList<Expression> args, FSharpRef<FSharpMap<string, FSharpRef<Expression>>> env)
            {
               return macro(args, new ExecutionEnvironment(env));
            }
         ));
      }

      public static FSharpFunc<Expression, Expression> MakeContinuation(Continuation c)
      {
         return FuncConvert.ToFSharpFunc(
            (Converter<Expression, Expression>)(x => c(x))
         );
      }
   }
}
