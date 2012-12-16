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
type private Token =
   | Open | Close
   | Quote | Unquote
   | Number of string
   | String of string
   | Symbol of string

let private tokenize source =
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
   ///Expression representing a symbol.
   | Symbol of string
   ///Expression representing a list of sub expressions.
   | List of Expression list
   ///Expression representing a function.
   | Function of (Continuation -> Expression list -> Expression)
   ///Expression representing a continuation (used by call/cc in Scheme).
   | Current of Continuation
   ///Expression representing an invalid value (used for mutation, where expressions shouldn't return anything).
   ///Should NOT be used except internally by this interpreter.
   | Dummy of string
///Function that takes an Expression and returns an Expression.
and Continuation = Expression -> Expression

type Frame = Expression ref [] ref
type Environment = Frame list ref

///FScheme Function delegate. Takes a list of Expressions as arguments, and returns an Expression.
type ExternFunc = delegate of Expression list -> Expression

///Makes an Expression.Function out of an ExternFunc
let makeExternFunc (externFunc : ExternFunc) =
   Function(fun c args -> externFunc.Invoke(args) |> c)

type Parser =
   | Number_P of double
   | String_P of string
   | Symbol_P of string
   | List_P of Parser list
   | PrimFun_P of (Continuation -> Expression list -> Expression)

type Macro = Parser list -> Parser
type MacroEnvironment = Map<string, Macro>

///Let* construct
let LetStar : Macro = function
   | List_P(bindings) :: body ->
     let folder b a = 
        match b with
        | List_P([Symbol_P(name); expr]) as bind ->
            List_P([Symbol_P("let"); List_P([bind]); a])
        | m -> failwith "bad let*"
     List_P(Symbol_P("begin") :: body) |> List.foldBack folder bindings 
   | m -> failwith "bad let*"

///And Macro
let rec And : Macro = function
   | [] -> Number_P(1.0)
   | [expr] -> expr
   | h :: t -> List_P([Symbol_P("if"); h; And t; Number_P(0.0)])

///Or Macro
let rec Or : Macro = function
   | [] -> Number_P(0.0)
   | [expr] -> expr
   | h :: t -> List_P([Symbol_P("if"); h; Number_P(1.0); Or t])

let macroEnv = 
    Map.ofList [
       "let*", LetStar
       "and", And
       "or", Or
    ]

///FScheme Macro delegate. Takes a list of unevaluated Expressions and an Environment as arguments, and returns an Expression.
type ExternMacro = delegate of Parser list -> Parser

///Makes a Macro out of an ExternMacro
let makeExternMacro (ex : ExternMacro) : Macro = ex.Invoke

///AST for FScheme expressions
type Syntax =
   | Number_S of double
   | String_S of string
   | Id of string
   | SetId of string * Syntax
   | Bind of string list * Syntax list * Syntax
   | BindRec of string list * Syntax list * Syntax
   | Fun of string list * Syntax
   | List_S of Syntax list
   | If of Syntax * Syntax * Syntax
   | Define of string * Syntax
   | Begin of Syntax list
   | PrimFun_S of (Continuation -> Expression list -> Expression)
   | Quote_S of Parser

let rec private printParser = function
   | Number_P(n) -> n.ToString()
   | String_P(s) -> "\"" + s + "\""
   | Symbol_P(s) -> s
   | List_P(ps) -> "(" + String.Join(" ", List.map printParser ps) + ")"
   | PrimFun_P(_) -> "<primitive-fun>"

//A simple parser
let rec private parserToSyntax (macro_env : MacroEnvironment) parser =
    let parse' = parserToSyntax macro_env
    match parser with
    | Number_P(n) -> Number_S(n)
    | String_P(s) -> String_S(s)
    | PrimFun_P(f) -> PrimFun_S(f)
    | Symbol_P(s) -> Id(s)
    | List_P([]) -> List_S([])
    | List_P(h :: t) ->
        match h with
        //Set!
        | Symbol_P("set!") ->
            match t with
            | Symbol_P(name) :: body -> SetId(name, Begin(List.map parse' body))
            | m -> failwith "Syntax error in set!"
        //let and letrec
        | Symbol_P(s) when s = "let" || s = "letrec" ->
            match t with
            | List_P(bindings) :: body ->
               let rec makeBind names bndings = function
                  | List_P([Symbol_P(name); bind]) :: t -> makeBind (name :: names) ((parse' bind) :: bndings) t
                  | [] -> 
                    let f = if s = "let" then Bind else BindRec
                    f(names, bndings, Begin(List.map parse' body))
                  | m -> sprintf "Syntax error in %s bindings." s |> failwith
               makeBind [] [] bindings
            | m -> sprintf "Syntax error in %s." s |> failwith
        //lambda
        | Symbol_P("lambda") | Symbol_P("λ") ->
            match t with
            | List_P(parameters) :: body ->
               Fun(List.map (function Symbol_P(s) -> s | m -> failwith "Syntax error in function definition.") parameters, Begin(List.map parse' body))
            | m -> List_P(t) |> printParser |> sprintf "Syntax error in function definition: %s" |> failwith
        //if
        | Symbol_P("if") ->
            match t with
            | [cond; then_case; else_case] -> If(parse' cond, parse' then_case, parse' else_case)
            | m -> failwith "Syntax error in if"//: %s" expr |> failwith
        //define
        | Symbol_P("define") -> 
            match t with
            | Symbol_P(name) :: body -> Define(name, Begin(List.map parse' body))
            | m -> failwith "Syntax error in define"//: %s" expr |> failwith
        //quote
        | Symbol_P("quote") ->
            match t with
            | [expr] -> Quote_S(expr)
            | m -> failwith "Syntax error in quote"
        //unquote
        | Symbol_P("unquote") ->
            failwith "unquote outside of quote"
        //begin
        | Symbol_P("begin") ->
            Begin(List.map parse' t)
        //defined macros
        | Symbol_P(s) when macro_env.ContainsKey s ->
            macro_env.[s] t |> parse'
        //otherwise...
        | _ -> List_S(List.map parse' (h :: t))

//A simple parser
let private stringToParser source =
   let map = function
      | Token.Number(n) -> Number_P(Double.Parse(n))
      | Token.String(s) -> String_P(s)
      | Token.Symbol(s) -> Symbol_P(s)
      | _ -> failwith "Syntax error."
   let rec list f t acc =
      let e, t' = parse' [] t
      parse' (List_P(f e) :: acc) t'
   and parse' acc = function
      | Open :: t -> list id t acc
      | Close :: t -> (List.rev acc), t
      | Quote :: Open :: t -> list (fun e -> [Symbol_P("quote"); List_P(e)]) t acc
      | Quote :: h :: t -> parse' (List_P([Symbol_P("quote"); map h]) :: acc) t
      | Unquote :: Open :: t -> list (fun e -> [Symbol_P("unquote"); List_P(e)]) t acc
      | Unquote :: h :: t -> parse' (List_P([Symbol_P("unquote"); map h]) :: acc) t
      | h :: t -> parse' ((map h) :: acc) t
      | [] -> (List.rev acc), []
   let result, _ = parse' [] (tokenize source)
   result

let private parse = stringToParser >> List.map (parserToSyntax macroEnv)

let rec printSyntax indent syntax = 
   let printBind name names exprs body =
      "(" + name +  " (" 
         + String.Join(
            "\n" + indent + "      ", 
            (List.map (function (n, b) -> "[" + n + (printSyntax " " b) + "]")
                      (List.zip names exprs)))
         + ")\n"
         + (printSyntax (indent + "  ") body)
         + ")"
   indent + match syntax with
            | Number_S(n) -> n.ToString()
            | String_S(s) -> "\"" + s + "\""
            | Id(s) -> s
            | SetId(s, expr) -> "(set! " + s + " " + printSyntax "" expr
            | Bind(names, exprs, body) -> printBind "let" names exprs body
            | BindRec(names, exprs, body) -> printBind "letrec" names exprs body
            | Fun(names, body) -> "(lambda (" + String.Join(" ", names) + ") " + printSyntax "" body + ")"
            | List_S(exprs) -> "(" + String.Join(" ", (List.map (printSyntax "") exprs)) + ")"
            | If(c, t, e) -> "(if " + String.Join(" ", (List.map (printSyntax "") [c; t; e])) + ")"
            | Define(names, body) -> "(define (" + String.Join(" ", names) + ")" + printSyntax " " body + ")"
            | Begin(exprs) -> "(begin " + String.Join(" ", (List.map (printSyntax "") exprs)) + ")"
            | PrimFun_S(f) -> "<primitive-function>"
            | Quote_S(p) -> "(quote " + printParser p + ")"

///Converts the given Expression to a string.
let rec print = function
   | List(Dummy(_) :: _) -> "" // don't print accumulated statement dummy values
   | List(list) -> "(" + String.Join(" ", (List.map print list)) + ")"
   | String(s) -> "\"" + s + "\""
   | Symbol(s) -> s
   | Number(n) -> n.ToString()
   | Container(o) -> o.ToString()
   | Function(_) | Current(_) -> "Function"
   | Dummy(_) -> "" // sometimes useful to emit value for debugging, but normally we ignore

///Prints a malformed statement error.
let private malformed n e = sprintf "Malformed '%s': %s" n (print (List([e]))) |> failwith

///Simple wrapper for arithmatic operations.
let private mathbin op name cont = function
   //If the arguments coming in consist of at least two numbers...
   | Number(n) :: Number(n2) :: ns ->
      //op': function that takes two Expression.Numbers and applies the given op.
      //     if the second argument is not an Expression.Number, then throw exception
      let op' a = function Number(b) -> op a b | m -> malformed (sprintf "%s arg" name) m
      //Reduce list of Expressions (ns) using op'. Pass result to continuation.
      Number(List.fold op' (op n n2) ns) |> cont
   //Otherwise, fail.
   | m -> malformed name (List(m))

let private math0 op name start cont exprs = 
  let op' a = function
    | Number(b) -> op a b
    | m -> malformed (sprintf "%s arg" name) m
  Number(List.fold op' start exprs) |> cont

let private math1 op op2 unary name cont = function
  | [Number(n)] -> Number(unary n) |> cont
  | Number(n) :: ns ->
    let cont' = function
      | Number(x) -> Number(op n x) |> cont 
      | m -> malformed (sprintf "%s arg" name) m
    op2 cont' ns
  | m -> malformed name (List(m)) 

//Arithmatic functions
let Add = math0 (+) "addition" 0.
let Subtract = math1 (-) Add (fun x -> -x) "subtraction"
let Multiply = math0 (*) "multiplication" 1.
let Divide = math1 (/) Multiply ((/) 1.) "division"
let Modulus = mathbin (%) "modulus"
let Exponent = mathbin ( ** ) "exponent"

///Simple wrapper for comparison operations.
let private boolMath (op : (IComparable -> IComparable -> bool)) name cont args =
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
let private _r = new Random()
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

///Error construct
let Throw cont = function
   | [String(s)] -> failwith s
   | m -> malformed "throw" (List(m))

///Display construct -- used to print to stdout
let Display cont = function
   | [e] -> print e |> printf "DISPLAY: %s \n"; Dummy("Dummy 'display'") |> cont
   | m -> malformed "display" (List(m))

///Call/cc -- gives access to the current interpreter continuation.
let CallCC cont = function
   | [Function(callee)] -> callee cont [Current(cont)]
   | m -> malformed "call/cc" (List(m))

let Apply cont = function
   | [Function(f); List(args)] -> f cont args
   | m -> malformed "apply" (List(m))

let Add1 cont = function
   | [Number(n)] -> Number(n + 1.0) |> cont
   | m -> malformed "add1" (List(m))

let Sub1 cont = function
   | [Number(n)] -> Number(n - 1.0) |> cont
   | m -> malformed "sub1" (List(m))

type private CompilerFrame = string list
type private CompilerEnv = CompilerFrame list ref

let private findInEnv (name : string) compenv =
   let rec find acc = function
      | h :: t ->
         match List.tryFindIndex ((=) name) h with
         | Some(i) -> Some(acc, i)
         | None -> find (acc + 1) t
      | [] -> None
   find 0 compenv

///A basic compiler
let rec private compile (compenv : CompilerEnv) syntax : (Continuation -> Environment -> Expression) =
   let compile' = compile compenv
   match syntax with
   | Number_S(n) ->
      let x = Number(n)
      fun cont _ -> x |> cont
   | String_S(s) ->
      let x = String(s)
      fun cont _ -> x |> cont
   | PrimFun_S(f) ->
      let x = Function(f)
      fun cont _ -> x |> cont
   | Id(id) ->
      match findInEnv id compenv.Value with
      | Some(i1, i2) -> fun cont env -> (env.Value.Item i1).Value.[i2].Value |> cont
      | None -> sprintf "Unbound identifier: %s" id |> failwith
   | SetId(id, expr) ->
      let ce = compile' expr
      match findInEnv id compenv.Value with
      | Some(i1, i2) -> 
         fun cont env ->
            let box = (env.Value.Item i1).Value.[i2]
            ce (fun x -> box := x; Dummy(sprintf "set! %s" id) |> cont) env
      | None -> sprintf "Unbound identifier: %s" id |> failwith
   | Bind(names, exprs, body) -> compile' (List_S(Fun(names, body) :: exprs))
   | BindRec(names, exprs, body) ->
      let cbody = compile (ref (names :: compenv.Value)) body
      let cargs = List.map (ref (names :: compenv.Value) |> compile) exprs
      let boxes = [ for _ in 1 .. List.length cargs -> ref (Dummy("letrec")) ]
      let newFrame : Frame = List.toArray boxes |> ref
      fun cont env ->
         let env' = newFrame :: env.Value |> ref
         let rec mapbind = function
            | (expr, box) :: t -> expr (fun x -> box := x; mapbind t) env'
            | [] -> cbody cont env'
         List.zip cargs boxes |> mapbind
   | Define(name, body) ->
      match List.tryFindIndex ((=) name) compenv.Value.Head with
      | Some(idx) ->
         let cbody = compile' body
         fun cont env ->
            let box = env.Value.Head.Value.[idx]
            cbody (fun x -> box := x; Dummy(sprintf "defined '%s'" name) |> cont) env
      | None ->
         let lastindex = compenv.Value.Head.Length
         compenv := (List.append compenv.Value.Head [name]) :: compenv.Value.Tail
         let cbody = compile compenv body
         fun cont env -> 
           let def = ref (Dummy(sprintf "define '%s'" name))
           Array.Resize(env.Value.Head, env.Value.Head.Value.Length + 1)
           env.Value.Head.Value.SetValue(def, lastindex)
           cbody (fun x -> def := x; Dummy(sprintf "defined '%s'" name) |> cont) env
   | Fun(names, body) ->
      let compenv' = names :: compenv.Value |> ref
      let cbody = compile compenv' body
      let amt = List.length names
      let pack args =
         let arglen = List.length args
         if arglen = amt then
            Seq.map ref args 
            |> Seq.toArray
         else
            Seq.append (Seq.take (amt - 1) args) 
                       (Seq.singleton (List(Seq.skip (amt - 1) args |> Seq.toList)))
            |> Seq.map ref 
            |> Seq.toArray
      fun cont env ->
         Function(
            fun cont' exprs -> 
               ref (pack exprs) :: env.Value 
               |> ref 
               |> cbody cont') |> cont
   | List_S(fun_expr :: args) ->
      let cfun = compile' fun_expr
      let cargs = List.map compile' args
      fun cont env ->
         let cont' = function
            | Function(f) ->
               let rec mapeval acc = function
                  | h :: t -> h (fun v -> mapeval (v :: acc) t) env
                  | [] -> List.rev acc |> f cont
               mapeval [] cargs
            | Current(c) ->
               match cargs with
               | [arg] -> arg c env
               | m -> failwith "Malformed Continuation"
            | m -> printSyntax "" syntax |> sprintf "expected function for call: %s" |> failwith
         cfun cont' env
   | If(cond, then_expr, else_expr) ->
      let ccond = compile' cond
      let cthen = compile' then_expr
      let celse = compile' else_expr
      fun cont env ->
         let cont' = function
            | List([]) | String("") ->  celse cont env // empty list or empty string is false
            | Number(n) when n = 0.0 -> celse cont env // zero is false
            | _ -> cthen cont env // everything else is true
         ccond cont' env
   | Begin([expr]) -> compile' expr
   | Begin(exprs) ->
      let body = List.map compile' exprs
      let d = Dummy("empty begin")
      fun cont env ->
         let rec runall' acc = function
            | h :: t -> h (fun x -> runall' x t) env
            | [] -> acc |> cont
         runall' d body
   | Quote_S(parser) -> makeQuote compenv parser
   | m -> failwith "Malformed expression"

and private makeQuote compenv parser =
   let makeQuote' = makeQuote compenv
   match parser with
   | Number_P(n) -> fun cont _ -> Number(n) |> cont
   | String_P(s) -> fun cont _ -> String(s) |> cont
   | Symbol_P(s) -> fun cont _ -> Symbol(s) |> cont
   | PrimFun_P(f) -> fun cont _ -> Function(f) |> cont
   | List_P(Symbol_P("unquote") :: t) ->
      match t with
      | [expr] -> parserToSyntax macroEnv expr |> compile compenv
      | _ -> failwith "malformed 'unquote'"
   | List_P(exprs) ->
      let qargs = List.map makeQuote' exprs
      fun cont env ->
         let rec mapquote acc = function
            | h :: t -> h (fun x -> mapquote (x :: acc) t) env
            | [] -> List.rev acc |> List |> cont
         mapquote [] qargs

///Eval construct -- evaluates code quotations
and Eval cont args =
   let rec toParser = function
      | Symbol(s) -> Symbol_P(s)
      | Number(n) -> Number_P(n)
      | String(s) -> String_P(s)
      | List(l) -> List_P(List.map toParser l)
      | Function(f) -> PrimFun_P(f)
      | m -> malformed "eval" m
   match args with
   | [arg] -> toParser arg 
              |> parserToSyntax macroEnv 
              |> compile compileEnvironment
              |> fun x -> x cont environment
   | m -> malformed "eval" (List(m))


///Load construct -- loads library files, reads them using the simple tokenizer and parser.
and load file = Load (fun _ -> Dummy("")) [String(file)] |> ignore
and Load cont = function
   | [String(file)] ->
      (File.OpenText(file)).ReadToEnd() 
         |> List.ofSeq 
         |> parse
         |> List.iter (fun x -> compile compileEnvironment x (fun _ -> Dummy("Dummy 'load'")) environment |> ignore)
      Dummy(sprintf "Loaded '%s'." file) |> cont
   | m -> malformed "load" (List(m))

and compileEnvironment : CompilerEnv =
   [[]] |> ref
///Our base environment
and environment : Environment =
   [[||] |> ref ] |> ref

let mutable private tempEnv : (string * Expression ref) list = []
let AddDefaultBinding name expr =
   tempEnv <- (name, ref expr) :: tempEnv

let private makeEnvironments() =
   AddDefaultBinding "*" (Function(Multiply))
   AddDefaultBinding "/" (Function(Divide))
   AddDefaultBinding "%" (Function(Modulus))
   AddDefaultBinding "+" (Function(Add))
   AddDefaultBinding "-" (Function(Subtract))
   AddDefaultBinding "pow" (Function(Exponent))
   AddDefaultBinding "cons" (Function(Cons))
   AddDefaultBinding "car" (Function(Car))
   AddDefaultBinding "first" (Function(Car))
   AddDefaultBinding "cdr" (Function(Cdr))
   AddDefaultBinding "rest" (Function(Cdr))
   AddDefaultBinding "len" (Function(Len))
   AddDefaultBinding "length" (Function(Len))
   AddDefaultBinding "append" (Function(Append))
   AddDefaultBinding "take" (Function(Take))
   AddDefaultBinding "get" (Function(Get))
   AddDefaultBinding "drop" (Function(Drop))
   AddDefaultBinding "build-seq" (Function(BuildSeq))
   AddDefaultBinding "load" (Function(Load))
   AddDefaultBinding "display" (Function(Display))
   AddDefaultBinding "call/cc" (Function(CallCC))
   AddDefaultBinding "true" (Number(1.0))
   AddDefaultBinding "false" (Number(0.0))
   AddDefaultBinding "<=" (Function(LTE))
   AddDefaultBinding ">=" (Function(GTE))
   AddDefaultBinding "<" (Function(LT))
   AddDefaultBinding ">" (Function(GT))
   AddDefaultBinding "=" (Function(EQ))
   AddDefaultBinding "empty" (List([]))
   AddDefaultBinding "null" (List([]))
   AddDefaultBinding "empty?" (Function(IsEmpty))
   AddDefaultBinding "reverse" (Function(Rev))
   AddDefaultBinding "rev" (Function(Rev))
   AddDefaultBinding "list" (Function(MakeList))
   AddDefaultBinding "sort" (Function(Sort))
   AddDefaultBinding "throw" (Function(Throw))
   AddDefaultBinding "rand" (Function(RandomDbl))
   AddDefaultBinding "string->num" (Function(String2Num))
   AddDefaultBinding "num->string"(Function(Num2String))
   AddDefaultBinding "concat-strings" (Function(Concat))
   AddDefaultBinding "eval" (Function(Eval))
   AddDefaultBinding "apply" (Function(Apply))
   AddDefaultBinding "add1" (Function(Add1))
   AddDefaultBinding "sub1" (Function(Sub1))

let Evaluate syntax = compile compileEnvironment syntax id environment

///Parses and evaluates an expression given in text form, and returns the resulting expression
let ParseText text = 
   List.ofSeq text 
   |> parse 
   |> Begin 
   |> Evaluate

let private evaluateSchemeDefs() =
   "
   (define not (lambda (x) (if x 0 1)))

   (define xor 
      (lambda (a b) 
         (and (or a b) 
               (not (and a b)))))

   (define fold 
      ;; fold :: (X Y -> Y) Y [listof X] -> Y
      (lambda (f a xs) 
         (if (empty? xs) 
               a 
               (fold f (f (first xs) a) (rest xs))))) 

   (define map
      ;; map :: (X -> Y) [listof X] -> [listof Y] 
      (lambda (f lst) 
         (reverse (fold (lambda (fold-first fold-acc) (cons (f fold-first) fold-acc)) 
                        empty
                        lst))))

   (define filter 
      ;; filter :: (X -> bool) [listof X] -> [listof X]
      (lambda (p lst) 
         (reverse (fold (lambda (f a) (if (p f) (cons f a) a)) 
                        empty 
                        lst)))) 

   (define cartesian-product 
      ;; cartesian-product :: (X Y ... Z -> A) [listof X] [listof Y] ... [listof Z] -> [listof A]
      (lambda (comb lsts)
         (letrec ((cp-atom-list (lambda (at lst) 
                                    (letrec ((cal* (lambda (x l a) 
                                                      (if (empty? l) 
                                                            (reverse a) 
                                                            (cal* x (rest l) (cons (cons x (first l)) a)))))) 
                                       (cal* at lst empty))))

                  (cp-list-list (lambda (l1 l2)
                                    (letrec ((cll* (lambda (m n a) 
                                                      (if (or (empty? m) (empty? n))
                                                            a 
                                                            (cll* (rest m) n (append a (cp-atom-list (first m) n)))))))
                                       (cll* l1 l2 empty)))))

            (let* ((lofls (reverse lsts)) 
                     (rst (rest lofls))
                     (cp (lambda (lsts) 
                           (fold cp-list-list 
                                 (map list (first lofls))
                                 rst))))
               (map (lambda (args) (apply comb args)) (cp lofls))))))

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
                  <))))                                    ;; Compare using less-than

  (define zip
      ;; zip :: [listof X] [listof Y] ... [listof Z] -> [listof [listof X Y ... Z]]
      (lambda (lofls) 
         (letrec ((zip'' (lambda (lofls a al) 
                           (if (empty? lofls) 
                                 (list (reverse a) (reverse al)) 
                                 (if (empty? (first lofls)) 
                                    (list empty al) 
                                    (zip'' (rest lofls) 
                                           (cons (first (first lofls)) a) 
                                           (cons (rest (first lofls)) al)))))) 
                  (zip' (lambda (lofls acc) 
                           (let ((result (zip'' lofls empty empty))) 
                              (if (empty? (first result)) 
                                    (reverse acc) 
                                    (let ((p (first result)) 
                                          (t (first (rest result)))) 
                                    (zip' t (cons p acc)))))))) 
            (zip' lofls empty))))

   (define combine 
      ;; combine :: (X Y ... Z -> A) [listof X] [listof Y] ... [listof Z] -> [listof A]
      (lambda (f lofls) 
         (map (lambda (x) (apply f x)) 
               (zip lofls))))

  (define for-each 
     ;; for-each :: (X -> unit) [listof X] -> unit
     (lambda (f lst) (fold (lambda (x _) (f x)) (begin) lst)))
   " |> ParseText |> ignore

makeEnvironments()
environment := [Seq.map (fun (_, x) -> x) tempEnv |> Seq.toArray |> ref]
compileEnvironment := [List.map (fun (x, _) -> x) tempEnv]
evaluateSchemeDefs()

///REP -- Read/Eval/Prints
let rep = ParseText >> print

///REPL -- Read/Eval/Print Loop
let repl() : unit =
   let rec repl' output =
      printf "%s\n> " output
      try Console.ReadLine() |> rep |> repl'
      with ex -> repl' ex.Message
   repl' ""

type ErrorLog = delegate of string -> unit

//Tests
let test (log : ErrorLog) =
   let case source expected =
      try
         //printfn "TEST: %s" source
         let output = rep source
         //Console.WriteLine(sprintf "TESTING: %s" source)
         if output <> expected then
            sprintf "TEST FAILED: %s [Expected: %s, Actual: %s]" source expected output |> log.Invoke
      with ex -> sprintf "TEST CRASHED: %s [%s]" ex.Message source |> log.Invoke
   
   //Not
   case "(not true)" "0"
   case "(not false)" "1"
   case "(not 0)" "1" // or (true)
   case "(not 1)" "0" // or (false)
   
   //And
   case "(and 0 0)" "0" // or (false)
   case "(and 1 0)" "0" // or (false)
   case "(and 0 1)" "0" // or (false)
   case "(and 1 1)" "1" // or (true)
   
   //Or
   case "(or 0 0)" "0" // or (false)
   case "(or 1 0)" "1" // or (true)
   case "(or 0 1)" "1" // or (true)
   case "(or 1 1)" "1" // or (true)

   //Xor
   case "(xor 0 0)" "0" // xor (false)
   case "(xor 1 0)" "1" // xor (true)
   case "(xor 0 1)" "1" // xor (true)
   case "(xor 1 1)" "0" // xor (false)
   
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

   //Apply
   case "(apply + '(1 2))" "3"
   case "(apply append '((1) (2)))" "(1 2)"
   
   //Fold
   case "(fold + 0 '(1 2 3))"   "6"
   case "(fold * 1 '(2 3 4 5))" "120" // fold
   
   //Map
   case "(map (lambda (x) x) '(1 2 3))" "(1 2 3)"
   
   //Filter
   case "(filter (lambda (x) (< x 2)) '(0 2 3 4 1 6 5))" "(0 1)"
   
   //Cartesian Product
   case "(cartesian-product list (list 1 2) (list 3 4) (list 5 6))" 
        "((1 3 5) (1 3 6) (1 4 5) (1 4 6) (2 3 5) (2 3 6) (2 4 5) (2 4 6))"
   
   //Sorting
   case "(sort-by '((2 2) (2 1) (1 1)) (lambda (x) (fold + 0 x)))" "((1 1) (2 1) (2 2))"
   case "(sort-with '((2 2) (2 1) (1 1)) (lambda (x y) (let ((size (lambda (l) (fold + 0 l)))) (- (size x) (size y)))))" "((1 1) (2 1) (2 2))"
   
   //Zip/Combine
   case "(zip '((1) (2) (3)) '((4) (5) (6)))" "(((1) (4)) ((2) (5)) ((3) (6)))"
   case "(combine (lambda (x y) (display x) (display y) (append x y)) '((1) (2) (3)) '((4) (5) (6)))" "((1 4) (2 5) (3 6))"
   
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
   
let runTests() = ErrorLog(Console.WriteLine) |> test
