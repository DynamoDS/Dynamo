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

module Dynamo.FScheme
#light

//An F# Scheme Interpreter

open System
open System.Numerics
open System.IO

//Simple Tokenizer for quickly defining expressions in Scheme syntax.
type Token =
   | Open | Close
   | Quote | Unquote
   | Number of string
   | String of string
   | Symbol of string

let tokenize source =
   let rec string acc = function
      | '\\' :: '"' :: t -> string (acc + "\"") t // escaped quote becomes quote
      | '\\' :: 'b' :: t -> string (acc + "\b") t // escaped backspace
      | '\\' :: 'f' :: t -> string (acc + "\f") t // escaped formfeed
      | '\\' :: 'n' :: t -> string (acc + "\n") t // escaped newline
      | '\\' :: 'r' :: t -> string (acc + "\r") t // escaped return
      | '\\' :: 't' :: t -> string (acc + "\t") t // escaped tab
      | '\\' :: '\\' :: t -> string (acc + "\\") t // escaped backslash
      | '"' :: t -> acc, t // closing quote terminates
      | c :: t -> string (acc + (c.ToString())) t // otherwise accumulate chars
      | _ -> failwith "Malformed string."
   let rec comment = function
      | '\r' :: t | '\n' :: t -> t // terminated by line end
      | [] -> [] // or by EOF
      | _ :: t -> comment t
   let rec token acc = function
      | (')' :: _) as t -> acc, t // closing paren terminates
      | w :: t when Char.IsWhiteSpace(w) -> acc, t // whitespace terminates
      | [] -> acc, [] // end of list terminates
      | c :: t -> token (acc + (c.ToString())) t // otherwise accumulate chars
   let rec tokenize' acc = function
      | w :: t when Char.IsWhiteSpace(w) -> tokenize' acc t // skip whitespace
      | '(' :: t -> tokenize' (Open :: acc) t
      | ')' :: t -> tokenize' (Close :: acc) t
      | '\'' :: t -> tokenize' (Quote :: acc) t
      | ',' :: t -> tokenize' (Unquote :: acc) t
      | ';' :: t -> comment t |> tokenize' acc // skip over comments
      | '"' :: t -> // start of string
         let s, t' = string "" t
         tokenize' (Token.String(s) :: acc) t'
      | '-' :: d :: t when Char.IsDigit(d) -> // start of negative number
         let n, t' = token ("-" + d.ToString()) t
         tokenize' (Token.Number(n) :: acc) t'
      | '+' :: d :: t | d :: t when Char.IsDigit(d) -> // start of positive number
         let n, t' = token (d.ToString()) t
         tokenize' (Token.Number(n) :: acc) t'
      | s :: t -> // otherwise start of symbol
         let s, t' = token (s.ToString()) t
         tokenize' (Token.Symbol(s) :: acc) t'
      | [] -> List.rev acc // end of list terminates
   tokenize' [] source

///Types of FScheme Expressions
type Expression =
   ///Expression representing any .NET object.
   | Container of obj
   ///Expression representing a number (double).
   | Number of double
   ///Expression representing a string.
   | String of string
   ///Expression representing a variable.
   | Symbol of string
   ///Expression representing a list of sub expressions.
   | List of Expression list
   ///Expression representing a function.
   | Function of (Continuation -> Expression list -> Expression)
   ///Expression representing a "macro".
   | Special of (Continuation -> Environment -> Expression list -> Expression)
   ///Expression representing a continuation (used by call/cc in Scheme).
   | Current of Continuation
   ///Expression representing an invalid value (used for mutation, where expressions shouldn't return anything).
   ///Should NOT be used except internally by this interpreter.
   | Dummy of string
///Function that takes an Expression and returns an Expression.
and Continuation = Expression -> Expression
///Reference to a Map that maps strings to references to Expressions.
and Environment = Map<string, Expression ref> ref

///FScheme Function delegate. Takes a list of Expressions as arguments, and returns an Expression.
type ExternFunc = delegate of Expression list -> Expression

///FScheme Macro delegate. Takes a list of unevaluated Expressions and an Environment as arguments, and returns an Expression.
type ExternMacro = delegate of Expression list * Environment -> Expression

///Makes an Expression.Function out of an ExternFunc
let makeExternFunc (externFunc : ExternFunc) =
   Function(fun c args -> externFunc.Invoke(args) |> c)

///Makes an Expression.Special out of an ExternMacro
let makeExternMacro (externMacro : ExternMacro) =
   Special(fun c e args -> externMacro.Invoke(args, e) |> c)

//A simple parser
let parse source =
   let map = function
      | Token.Number(n) -> Expression.Number(Double.Parse(n))
      | Token.String(s) -> Expression.String(s)
      | Token.Symbol(s) -> Expression.Symbol(s)
      | _ -> failwith "Syntax error."
   let rec list f t acc =
      let e, t' = parse' [] t
      parse' (List(f e) :: acc) t'
   and parse' acc = function
      | Open :: t -> list id t acc
      | Close :: t -> (List.rev acc), t
      | Quote :: Open :: t -> list (fun e -> [Symbol("quote"); List(e)]) t acc
      | Quote :: h :: t -> parse' (List([Symbol("quote"); map h]) :: acc) t
      | Unquote :: Open :: t -> list (fun e -> [Symbol("unquote"); List(e)]) t acc
      | Unquote :: h :: t -> parse' (List([Symbol("unquote"); map h]) :: acc) t
      | h :: t -> parse' ((map h) :: acc) t
      | [] -> (List.rev acc), []
   let result, _ = parse' [] (tokenize source)
   result

///Converts the given Expression to a string.
let rec print = function
   | List(Dummy(_) :: _) -> "" // don't print accumulated statement dummy values
   | List(list) -> "(" + String.Join(" ", (List.map print list)) + ")"
   | String(s) -> "\"" + s + "\""
   | Symbol(s) -> s
   | Number(n) -> n.ToString()
   | Container(o) -> o.ToString()
   | Function(_) | Special(_) | Current(_) -> "Function"
   | Dummy(_) -> "" // sometimes useful to emit value for debugging, but normally we ignore

///Prints a malformed statement error.
let malformed n e = sprintf "Malformed '%s': %s" n (print (List([e]))) |> failwith

///Simple wrapper for arithmatic operations.
let math op name cont = function
   //If the arguments coming in consist of at least two numbers...
   | Number(n) :: Number(n2) :: ns ->
      //op': function that takes two Expression.Numbers and applies the given op.
      //     if the second argument is not an Expression.Number, then throw exception
      let op' a = function Number(b) -> op a b | m -> malformed (sprintf "%s arg" name) m
      //Reduce list of Expressions (ns) using op'. Pass result to continuation.
      Number(List.fold op' (op n n2) ns) |> cont
   //Otherwise, fail.
   | m -> malformed name (List(m))

//Arithmatic functions
let Add = math (+) "addition"
let Subtract = math (-) "subtraction"
let Multiply = math (*) "multiplication"
let Divide = math (/) "division"
let Modulus = math (%) "modulus"
let Exponent = math ( ** ) "exponent"

///Simple wrapper for comparison operations.
let boolMath (op : (IComparable -> IComparable -> bool)) name cont args =
   let comp a' b' = 
      match (op a' b') with
      | true -> Number(1.0) |> cont
      | _ -> Number(0.0) |> cont
   match args with
   | [Number(a); Number(b)] -> comp a b
   | [String(a); String(b)] -> comp a b
   | m -> malformed name (List(m))

//Comparison operations.
let LTE = boolMath (<=) "less-than-or-equals"
let GTE = boolMath (>=) "greater-than-or-equals"
let LT = boolMath (<) "less-than"
let GT = boolMath (>) "greater-than"
let EQ = boolMath (=) "equals"

//Random Number
let _r = new Random()
let RandomDbl cont = function _ -> Number(_r.NextDouble()) |> cont

//List Functions
let IsEmpty cont = function [List([])] -> Number(1.0) |> cont | _ -> Number (0.0) |> cont
let Cons cont = function [h; List(t)] -> (List(h :: t)) |> cont | m -> malformed "cons" (List(m))
let Car cont = function [List(h :: _)] -> h |> cont | m -> malformed "car" (List(m))
let Cdr cont = function [List(_ :: t)] -> List(t) |> cont | m -> malformed "cdr" (List(m))
let Rev cont = function [List(l)] -> List(List.rev l) |> cont | m -> malformed "reverse" (List(m))
let MakeList cont = function (elements : Expression list) -> List(elements) |> cont
let Len cont = function [List(l)] -> Number(double (List.length l)) |> cont | m -> malformed "len" (List(m))
let Append cont = function [List(l1); List(l2)] -> List(List.append l1 l2) |> cont | m -> malformed "append" (List(m))
let Take cont = function [Number(n); List(l)] -> List(Seq.take (int n) l |> List.ofSeq) |> cont | m -> malformed "take" (List(m))
let Get cont = function [Number(n); List(l)] -> l.Item (int n) |> cont | m -> malformed "get" (List(m))
let Drop cont = function [Number(n); List(l)] -> List(Seq.skip (int n) l |> List.ofSeq) |> cont | m -> malformed "drop" (List(m))

///Sorts using natural ordering. Only works for primitive types (numbers, strings, etc.)
let Sort cont = function
   //We expect a list of expressions as the only argument.
   | [List(l)] ->
      //Peek and see what kind of data we're sorting
      match l with
      //If the first element is an Expression.Number...
      | Number(n) :: _ ->
         //converter: Makes sure given Expression is an Expression.Number.
         //           If it is an Expression.Number, pull the Number from it.
         //           Otherwise, fail.
         let converter = function
            | Number(n) -> n
            | m -> malformed "sort" m
         //Convert Expression.Numbers to doubles, sort them, then convert them back to Expression.Numbers.
         List(List.map converter l |> List.sort |> List.map (fun n -> Number(n))) |> cont
      //If the first element is an Expression.String...
      | String(s) :: _ ->
         //converter: Makes sure given Expression is an Expression.String.
         //           If it is an Expression.String, pull the string from it.
         //           Otherwise, fail.
         let converter = function
            | String(s) -> s
            | m -> malformed "sort" m
         //Convert Expression.Strings to strings, sort them, then convert them back to Expression.Strings.
         List(List.map converter l |> List.sort |> List.map (fun n -> String(n))) |> cont
      //Otherwise, fail.
      | _ -> malformed "sort" (List(l))
   //Otherwise, fail.
   | m -> malformed "sort" (List(m))

///Build Sequence
let BuildSeq cont = function
   | [Number(start); Number(stop); Number(step)] -> [start .. step .. stop] |> List.map Number |> List |> cont
   | m -> malformed "build-seq" (List(m))

let String2Num cont = function
    | [String(s)] -> Number(Convert.ToDouble(s)) |> cont
    | m -> malformed "string" (List(m))

let Num2String cont = function
    | [Number(n)] -> String(n.ToString()) |> cont
    | m -> malformed "number" (List(m))

let Concat cont = function
    | [List(l)] -> 
        let rec concat a = function
            | String(s) :: l -> concat (a + s) l
            | [] -> String(a) |> cont
            | m :: _ -> malformed "string" m
        concat "" l
    | m -> malformed "concat" (List(m))

///Extends the given environment with the given bindings.
let rec extend (env : Environment) = function
   | [] -> env
   | (a, b) :: t -> extend (Map.add a b env.Value |> ref) t

///Looks up a symbol in the given environment.
let lookup (env : Environment) symbol = 
   match Map.tryFind symbol env.Value with
   | Some(e) -> e
   | None -> sprintf "No binding for '%s'." symbol |> failwith

///Zips given lists into a list of bindings.
let zip args parameters =
   let args' = // passing more args than params results in last param treated as list
      let plen = List.length parameters
      if List.length args = plen then
         args
      else
         let split ts = ts (plen - 1) args |> List.ofSeq
         split Seq.take @ [List(Symbol("list") :: split Seq.skip)]
   List.zip parameters args'

///Error construct
let Throw cont = function
   | [String(s)] -> failwith s
   | m -> malformed "throw" (List(m))

///If construct
let rec If cont env = function
   | [condition; t; f] -> //(if [condition] [t] [f])
      eval (function
         | List([]) | String("") -> 
            eval cont env f // empty list or empty string is false
         | Number(n) when n = 0.0 ->
            eval cont env f // zero is false
         | _ -> eval cont env t) env condition // everything else is true
   | m -> malformed "if" (List(m))

///Let construct
and Let cont env = function
   | [List(bindings); body] ->
      let rec mapbind acc = function
         | List([Symbol(s); e]) :: t -> eval (fun x -> mapbind ((s, ref x) :: acc) t) env e
         | [] ->
            let frame = List.rev acc
            let env' = extend env frame
            eval cont env' body
         | _ -> failwith "Malformed 'let' binding."
      mapbind [] bindings
   | m -> malformed "let" (List(m))

///LetRec (Recursive Let) construct
and LetRec cont env = function
   //Input is two arguments, a list of bindings and the letrec body
   | [List(bindings); body] -> 
      //bind is a function that takes a Expression.List of two arguments, the second of which is ignored
      let bind = function
         | List([Symbol(s); _]) -> s, ref (Dummy("Dummy 'letrec'"))
         | m -> malformed "letrec binding" m
      //Map all of the bindings to dummys
      let env' = List.map bind bindings |> extend env
      // now update dummy env - assumes dummy env will be captured but not actually accessed (e.g. lambda)
      let rec mapupdate = function
         //If the input is a List (of symbol/expression pairs), then we evaluate the expression in the dummy environment
         //and then update the environment with the result, and recurse
         | List([Symbol(s); e]) :: t -> eval (fun x -> (env'.Value.Item s) := x; mapupdate t) env' e
         //If the input is empty (base case), then we simply evaluate the body with the dummy environment
         | [] -> eval cont env' body
         //Error case
         | _ -> failwith "Malformed 'let' binding."
      mapupdate bindings
   | m -> malformed "letrec" (List(m))

///Let* construct
and LetStar cont env = function
   | [List(bindings); body] ->
      let rec foldbind env' = function
         | List([Symbol(s); e]) :: t -> eval (fun x -> foldbind ([s, ref x] |> extend env') t) env' e
         | [] -> eval cont env' body
         | _ -> failwith "Malformed 'let*' binding."
      foldbind env bindings
   | m -> malformed "let*" (List(m))

///Anonymous function (lambda) construct
and Lambda cont env = function
   | [List(parameters); body] ->
      let closure cont' env' args =
         // bind parameters to actual arguments (evaluated in the caller's environment)
         let rec mapbind acc = function
            // If the list of bindings isn't empty, evaluate the parameter (a), place it on the accumulator,
            // and continue mapbind with the rest of the list.
            | (Symbol(p), a) :: t -> eval (fun x -> mapbind ((p, ref x) :: acc) t) env' a
            //If the list is empty...
            | [] ->
               // env :: definition-time environment
               // env' :: run-time environment (for evaluating arguments)
               
               //let env'' = Map.fold (fun state key value -> Map.add key value state) env'.Value env.Value |> ref
               
               //extend the captured definition-time environment
               let env''' = if not acc.IsEmpty
                            then List.rev acc |> extend env//''
                            else env//''
               //and evaluate the body with the new environment
               eval cont' env''' body
            | _ -> failwith "Malformed lambda param."
         // Use mapbind to create our closure
         mapbind [] (zip args parameters)
      // Return the closure function as a special form
      Special(closure) |> cont
   | m -> malformed "lambda" (List(m))

///Code quotation construct
and Quote cont env =
   let rec unquote cont' = function
      | List([Symbol("unquote"); e]) -> eval cont' env e
      | List(Symbol("unquote") :: _) as m -> malformed "unquote (too many args)" m
      | List(lst) ->
         let rec mapunquote acc = function
            | h' :: t' ->
               unquote (fun x -> mapunquote (x :: acc) t') h'
            | [] -> List(List.rev acc)
         mapunquote [] lst |> cont'
      | e -> cont' e
   function [e] -> unquote cont e | m -> malformed "quote" (List(m))

///Eval construct -- evaluates code quotations
and Eval cont env = function 
   | [args] -> eval (eval cont env) env args
   | m -> malformed "eval" (List(m))

///Macro construct -- similar to functions, but arguments are passed unevaluated. Useful for short-circuiting.
and Macro cont env = function
   | [List(parameters); body] ->
      let closure cont' env' args =
         // bind parameters to actual arguments (but unevaluated, unlike lambda)
         let bind = function 
            | Symbol(p), a -> p, ref a 
            | _, m -> malformed "macro parameter" m // bound unevaluated
         let env'' = zip args parameters |> List.map bind |> extend env // extend the captured definition-time environment
         //Evaluate the body in the extended environment (params mapped to args), then evaluate the result in the passed
         //environment.
         eval (eval cont' env') env'' body
      Special(closure) |> cont
   | m -> malformed "macro" (List(m))

///Set! construct -- mutation
and Set cont env = function
   | [Symbol(s); e] -> eval (fun x -> (lookup env s) := x; Dummy(sprintf "Set %s" s) |> cont) env e
   | m -> malformed "set!" (List(m))

///Begin construct
and Begin cont env =
   let rec foldeval last = function
      | h :: t -> eval (fun x -> foldeval x t) env h
      | [] -> last |> cont
   foldeval (Dummy("Empty 'begin'"))
  
///Define construct -- Mutates base environment to contain the given definition
and Define cont env = function
   | [Symbol(s); e] ->
      let def = ref (Dummy("Dummy 'define'"))
      env := Map.add s def env.Value
      eval (fun x -> def := x; Dummy(sprintf "Defined %s" s) |> cont) env e
   | m -> malformed "define" (List(m))

///Load construct -- loads library files, reads them using the simple tokenizer and parser.
and load file = Load (fun _ -> Dummy("")) [String(file)] |> ignore
and Load cont = function
   | [String(file)] ->
      (File.OpenText(file)).ReadToEnd() |> List.ofSeq |> parse |> List.iter (eval (fun _ -> Dummy("Dummy 'load'")) environment >> ignore)
      Symbol(sprintf "Loaded '%s'." file) |> cont
   | m -> malformed "load" (List(m))

///Display construct -- used to print to stdout
and Display cont = function
   | [e] -> print e |> printf "DISPLAY: %s \n"; Dummy("Dummy 'display'") |> cont
   | m -> malformed "display" (List(m))

///Call/cc -- gives access to the current interpreter continuation.
and CallCC cont env = function
   | [callee] -> eval (function Special(fn) -> fn cont env [Current(cont)] | m -> malformed "call/cc" m) env callee
   | m -> malformed "call/cc" (List(m))

///Our base environment
and environment =
   Map.ofList
      ["*", ref (Function(Multiply))
       "/", ref (Function(Divide))
       "%", ref (Function(Modulus))
       "+", ref (Function(Add))
       "-", ref (Function(Subtract))
       "pow", ref (Function(Exponent))
       "if", ref (Special(If))
       "let", ref (Special(Let))
       "letrec", ref (Special(LetRec))
       "let*", ref (Special(LetStar))
       "lambda", ref (Special(Lambda))
       "cons", ref (Function(Cons))
       "car", ref (Function(Car))
       "first", ref (Function(Car))
       "cdr", ref (Function(Cdr))
       "rest", ref (Function(Cdr))
       "len", ref (Function(Len))
       "length", ref (Function(Len))
       "append", ref (Function(Append))
       "take", ref (Function(Take))
       "get", ref (Function(Get))
       "drop", ref (Function(Drop))
       "build-seq", ref (Function(BuildSeq))
       "quote", ref (Special(Quote))
       "eval", ref (Special(Eval))
       "macro", ref (Special(Macro))
       "set!", ref (Special(Set))
       "begin", ref (Special(Begin))
       "define", ref (Special(Define))
       "load", ref (Function(Load))
       "display", ref (Function(Display))
       "call/cc", ref (Special(CallCC))
       "true", ref (Number(1.0))
       "false", ref (Number(0.0))
       "<=", ref (Function(LTE))
       ">=", ref (Function(GTE))
       "<", ref (Function(LT))
       ">", ref (Function(GT))
       "=", ref (Function(EQ))
       "empty", ref (List([]))
       "empty?", ref (Function(IsEmpty))
       "reverse", ref (Function(Rev))
       "rev", ref (Function(Rev))
       "list", ref (Function(MakeList))
       "sort", ref (Function(Sort))
       "throw", ref (Function(Throw))
       "rand", ref (Function(RandomDbl))
       "string->num", ref (Function(String2Num))
       "num->string", ref (Function(Num2String))
       "concat-strings", ref (Function(Concat))
      ] |> ref

///Our eval loop
and eval cont env expression =
   match expression with
   //Basic values get passed to the continuation as themselves
   | Number(_) | String(_) | Current(_) | Container(_) 
   //As do function-like objects
   | Function(_) | Special(_) as lit -> lit |> cont 
   //Symbols are looked up in the environment, dereferenced, and then passed to the continuation.
   | Symbol(s) -> (lookup env s).Value |> cont 
   //Lists are function calls. First the head is evaluated into a function object, then depending
   //on the object...
   | List(h :: t) ->
      eval (function
         //Functions have their arguments evaluated and then applied to the function
         | Function(f) -> apply cont env f t 
         //Special forms have their unevaluated arguments passed to them, along with the environment
         | Special(f) -> f cont env t 
         //Continuations take one argument, and have that argument passed
         | Current(f) -> match t with [rtn] -> f rtn | m -> malformed "call/cc args" (List(m))
         | m -> h :: t |> apply cont env MakeList) (*malformed "expression" m*) env h
   //Anything else causes an error.
   | Dummy(s) -> sprintf "Cannot evaluate dummy value: %s" s |> failwith
   | _ -> failwith "Malformed expression."

///Our apply loop
and apply cont env fn args =
   //Tail recursive function used to evaluate a list of expressions.
   let rec mapeval acc = function
      //If the list is not empty, evaluate the head, then evaluate the rest of the list
      | h :: t -> eval (fun a -> mapeval (a :: acc) t) env h
      //Otherwise, we're done here. Pass the reversed accumulated list to the function.
      | [] -> fn cont (List.rev acc)
   mapeval [] args

///REP -- Read/Eval/Print
let rep env = List.ofSeq >> parse >> List.head >> eval id env >> print

///REPL -- Read/Eval/Print Loop
let rec repl output : unit =
   printf "%s\n> " output
   try Console.ReadLine() |> rep environment |> repl
   with ex -> repl ex.Message

type ErrorLog = delegate of string -> unit

//Tests
let test (log : ErrorLog) =
   let case source expected =
      try
         //printfn "TEST: %s" source
         let output = rep environment source
         //Console.WriteLine(sprintf "TESTING: %s" source)
         if output <> expected then
            sprintf "TEST FAILED: %s [Expected: %s, Actual: %s]" source expected output |> log.Invoke
      with ex -> sprintf "TEST CRASHED: %s [%s]" ex.Message source |> log.Invoke
   
   //Not
   case "(define not 
            (lambda (x) (if x 0 1)))" ""
   case "(not true)" "0"
   case "(not false)" "1"
   case "(not 0)" "1" // or (true)
   case "(not 1)" "0" // or (false)
   
   //And
   case "(define and 
            (macro (a b) 
               '(if ,a (if ,b 1 0) 0)))" ""
   case "(and 0 0)" "0" // or (false)
   case "(and 1 0)" "0" // or (false)
   case "(and 0 1)" "0" // or (false)
   case "(and 1 1)" "1" // or (true)
   
   //Or
   case "(define or 
            (macro (a b) 
               '(if ,a 1 (if ,b 1 0))))" ""
   case "(or 0 0)" "0" // or (false)
   case "(or 1 0)" "1" // or (true)
   case "(or 0 1)" "1" // or (true)
   case "(or 1 1)" "1" // or (true)

   //Xor
   case "(define xor 
            (lambda (a b) 
               (and (or a b) 
                    (not (and a b)))))" ""
   case "(xor 0 0)" "0" // xor (false)
   case "(xor 1 0)" "1" // xor (true)
   case "(xor 0 1)" "1" // xor (true)
   case "(xor 1 1)" "0" // xor (false)
   
   //Apply
   case "(define apply 
            ;; apply :: (X Y ... Z -> A) [list X Y ... Z] -> A
            (macro (f args) 
               '(eval (cons ,f ,args))))" ""
   case "(apply + '(1 2))" "3"
   case "(apply append '((1) (2)))" "(1 2)"
   
   //Fold
   case "(begin 
            (define fold 
               ;; fold :: (X Y -> Y) Y [listof X] -> Y
               (lambda (f a xs) 
                  (if (empty? xs) 
                      a 
                      (fold f (f (first xs) a) (rest xs))))) 
            (fold + 0 '(1 2 3)))" 
        "6"
   case "(fold * 1 '(2 3 4 5))" "120" // fold
   
   //Map
   case "(begin 
            (define map
               ;; map :: (X -> Y) [listof X] -> [listof Y] 
               (lambda (f lst) 
                  (reverse (fold (lambda (fold-first fold-acc) (cons (f fold-first) fold-acc)) 
                                 empty
                                 lst)))) 
            (map (lambda (x) x) '(1 2 3)))" 
        "(1 2 3)"
   
   //Filter
   case "(begin 
            (define filter 
               ;; filter :: (X -> bool) [listof X] -> [listof X]
               (lambda (p lst) 
                  (reverse (fold (lambda (f a) (if (p f) (cons f a) a)) 
                                 empty 
                                 lst)))) 
            (filter (lambda (x) (< x 2)) '(0 2 3 4 1 6 5)))"
        "(0 1)"
   
   //Cartesian Product
   case "(begin 
            (define cp-atom-list 
               (lambda (at lst) 
                  (letrec ((cal* (lambda (x l a) 
                                    (if (empty? l) 
                                        (reverse a) 
                                        (cal* x (rest l) (cons (cons x (first l)) a)))))) 
                     (cal* at lst empty)))) 
            
            (define cp-list-list 
               (lambda (l1 l2)
                  (letrec ((cll* (lambda (m n a) 
                                    (if (or (empty? m) (empty? n))
                                        a 
                                        (cll* (rest m) n (append a (cp-atom-list (first m) n)))))))
                     (cll* l1 l2 empty)))) 
                     
            (define cartesian-product 
               ;; cartesian-product :: (X Y ... Z -> A) [listof X] [listof Y] ... [listof Z] -> [listof A]
               (lambda (comb lsts) 
                  (let* ((lofls (reverse lsts)) 
                         (rst (rest lofls))
                         (cp (lambda (lsts) 
                                 (fold cp-list-list 
                                       (map list (first lofls))
                                       rst))))
                     (map (lambda (args) (apply comb args)) (cp lofls))))))" ""
   case "(cartesian-product list (list 1 2) (list 3 4) (list 5 6))" "((1 3 5) (1 3 6) (1 4 5) (1 4 6) (2 3 5) (2 3 6) (2 4 5) (2 4 6))"
   
   //Sorting
   case "(begin 
            (define qs 
               ;; qs :: [listof X] (X -> Y) (Y Y -> bool) -> [listof X]
               (lambda (lst f c) 
                  (if (empty? lst) 
                      empty 
                      (let* ((pivot (f (first lst))) 
                            (lt (filter (lambda (x) (c (f x) pivot)) (rest lst)))
                            (gt (filter (lambda (x) (not (c (f x) pivot))) (rest lst))))
                        (append (qs lt f c) (cons (first lst) (qs gt f c))))))) 
            
            (define sort-with 
               ;; sort-with :: [listof X] (X X -> int) -> [listof X]
               (lambda (lst comp) 
                  (qs lst 
                      (lambda (x) x)
                      (lambda (a b) (< (comp a b) 0))))) 
                      
            (define sort-by
               ;; sort-by :: [listof X] (X -> IComparable) -> [listof X]
               (lambda (lst proj) 
                  (map (lambda (x) (first x))                       ;; Convert back to original list
                       (qs (map (lambda (x) (list x (proj x))) lst) ;; Sort list of original element/projection pairs
                           (lambda (y) (first (rest y)))            ;; Sort based on the second element in the sub-lists
                           <)))))                                   ;; Compare using less-than" ""
   case "(sort-by '((2 2) (2 1) (1 1)) (lambda (x) (fold + 0 x)))" "((1 1) (2 1) (2 2))"
   case "(sort-with '((2 2) (2 1) (1 1)) (lambda (x y) (let ((size (lambda (l) (fold + 0 l)))) (- (size x) (size y)))))" "((1 1) (2 1) (2 2))"
   
   //Combine
   case "(define zip
            ;; zip :: [listof X] [listof Y] ... [listof Z] -> [listof [listof X Y ... Z]]
            (lambda (lofls) 
               (letrec ((zip'' (lambda (lofls a al) 
                                 (if (empty? lofls) 
                                     (list (reverse a) (reverse al)) 
                                     (if (empty? (first lofls)) 
                                         (list empty al) 
                                         (zip'' (rest lofls) (cons (first (first lofls)) a) (cons (rest (first lofls)) al)))))) 
                        (zip' (lambda (lofls acc) 
                                 (let ((result (zip'' lofls empty empty))) 
                                    (if (empty? (first result)) 
                                        (reverse acc) 
                                        (let ((p (first result)) 
                                              (t (first (rest result)))) 
                                          (zip' t (cons p acc)))))))) 
                  (zip' lofls empty))))" ""
   case "(define combine 
            ;; combine :: (X Y ... Z -> A) [listof X] [listof Y] ... [listof Z] -> [listof A]
            (lambda (f lofls) 
               (map (lambda (x) (apply f x)) 
                    (zip lofls))))" ""
   case "(zip '((1) (2) (3)) '((4) (5) (6)))" "(((1) (4)) ((2) (5)) ((3) (6)))"
   case "(combine (lambda (x y) (begin (display x) (display y) (append x y))) '((1) (2) (3)) '((4) (5) (6)))" "((1 4) (2 5) (3 6))"
   
   //Engine Tests
   case "(quote (* 2 3))" "(* 2 3)" // quote primitive
   case "(eval '(* 2 3))" "6" // eval quoted expression
   case "(quote (* 2 (- 5 2)))" "(* 2 (- 5 2))" // quote nested
   case "(quote (* 2 (unquote (- 5 2))))" "(* 2 3)" // quote nested unquote
   case "(let ((a 1)) (begin (set! a 2) a))" "2" // begin and assign
   case "(let* ((a 5) (dummy (set! a 10))) a)" "10" // re-assign after let
   case "(begin (define too-many (lambda (a x) x)) (too-many 1 2 3))" "(2 3)"
   case "(too-many '(1 1) '(2 2) '(3 3))" "((2 2) (3 3))"
   case "(reverse '(1 2 3))" "(3 2 1)" // reverse
   case "(list 1 2 3)" "(1 2 3)"
   case "(let ((square (lambda (x) (* x x)))) (map square '(1 2 3 4 5 6 7 8 9)))" "(1 4 9 16 25 36 49 64 81)" // mapping
   case "(let ((square (lambda (x) (* x x)))) (map square '(9)))" "(81)" // mapping single
   case "(let ((square (lambda (x) (* x x)))) (map square '()))" "()" // mapping empty
   case "(call/cc (lambda (c) (c 10)))" "10" // super-simple call/cc
   case "(call/cc (lambda (c) (if (c 10) 20 30)))" "10" // call/cc bailing out of 'if'
   case "(+ 8 (call/cc (lambda (k^) (* (k^ 5) 100))))" "13" // call/cc bailing out of multiplication
   case "(* (+ (call/cc (lambda (k^) (/ (k^ 5) 4))) 8) 3)" "39" // call/cc nesting
   case "(let ((divide (lambda (x y) (call/cc (lambda (k^) (if (= y 0) (k^ \"error\") (/ x y))))))) (divide 1 0))" "\"error\"" // call/cc as an error handler
   
   //Built-in Tests
   case "(cons 1 (cons 5 (cons \"hello\" empty)))" "(1 5 \"hello\")" // cons and empty
   case "(car (list 5 6 2))" "5" // car / first
   case "(cdr (list 5 6 2))" "(6 2)" // cdr / rest
   case "(len (list 5 6 2))" "3" // len
   case "(append '(1 2 3) '(4 5 6))" "(1 2 3 4 5 6)" // append
   case "(take 2 '(1 2 3))" "(1 2)" // take
   case "(get 2 '(1 2 3))" "3" // get
   case "(drop 2 '(1 2 3))" "(3)" // drop
   case "(build-seq 0 10 1)" "(0 1 2 3 4 5 6 7 8 9 10)" // build-seq
   case "(reverse '(1 2 3))" "(3 2 1)" // reverse
   case "(empty? '())" "1" // empty?
   case "(empty? '(1))" "0" // empty?
   case "(sort '(8 4 7 6 1 0 2 9))" "(0 1 2 4 6 7 8 9)" // sort
   case "(sort (list \"b\" \"c\" \"a\"))" "(\"a\" \"b\" \"c\")" // sort

   //Scope
   case "(let ((y 6)) (let ((f (lambda (x) (+ x y)))) (let ((y 5)) (f 4))))" "10"
