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
open System.Diagnostics

//Simple Tokenizer for quickly defining expressions in Scheme syntax.
type private Token =
    | Open | Close
    | Quote | Quasi | Unquote
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
        | '`' :: t -> tokenize' (Quasi :: acc) t
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
    | Function of (Expression list -> Expression)
    ///Expression representing an invalid value (used for mutation, where expressions shouldn't return anything).
    ///Should NOT be used except internally by this interpreter.
    | Dummy of string


///Converts an expression to a boolean value
let private exprToBool = function
    //Empty list or empty string is false, evaluate else branch.
    | List([]) | String("") -> false
    //Zero is false, evaluate else branch.
    | Number(n) when n = 0. -> false
    //Everything else is true, evaluate then branch.
    | _ -> true

type Frame = Expression ref [] ref
type Environment = Frame list ref

type Parameter =
    | Normal of string
    | Tail of string

///FScheme Function delegate. Takes a list of Expressions as arguments, and returns an Expression.
type ExternFunc = delegate of Expression list -> Expression

///Makes an Expression.Function out of an ExternFunc
let makeExternFunc (externFunc : ExternFunc) =
    Function(externFunc.Invoke)

type Parser =
    | Number_P of double
    | String_P of string
    | Symbol_P of string
    | Dot_P
    | Func_P of (Expression list -> Expression)
    | List_P of Parser list
    | Container_P of obj

type Macro = Parser list -> Parser
type MacroEnvironment = Map<string, Macro>

///Let* macro
let LetStar : Macro = function
    | List_P(bindings) :: body ->
        let folder b a = 
            match b with
            | List_P([Symbol_P(name); expr]) as let' ->
                List_P([Symbol_P("let"); List_P([let']); a])
            | m -> failwith "bad let*"
        List_P(Symbol_P("begin") :: body) |> List.foldBack folder bindings 
    | m -> failwith "bad let*"

///And macro
let rec And : Macro = function
    | [] -> Number_P(1.)
    | [expr] -> expr
    | h :: t -> List_P([Symbol_P("if"); h; And t; Number_P(0.)])

///Or macro
let rec Or : Macro = function
    | [] -> Number_P(0.)
    | [expr] -> expr
    | h :: t -> List_P([Symbol_P("if"); h; Number_P(1.); Or t])

//Cond macro
let rec Cond : Macro = function
    | [List_P([Symbol_P("else"); expr])] -> expr
    | List_P([condition; expr]) :: t -> List_P([Symbol_P("if"); condition; expr; Cond t])
    | [] -> List_P([Symbol_P("begin")])
    | m -> failwith "bad cond"

let macroEnv = 
    Map.ofList [
        "let*", LetStar
        "cond", Cond
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
    | Let of string list * Syntax list * Syntax
    | LetRec of string list * Syntax list * Syntax
    | Fun of Parameter list * Syntax
    | List_S of Syntax list
    | If of Syntax * Syntax * Syntax
    | Define of string * Syntax
    | Begin of Syntax list
    | Quote_S of Parser
    | Quasi_S of Parser
    | Func_S of (Expression list -> Expression)
    | Container_S of obj

let rec private printParser = function
    | Number_P(n) -> n.ToString()
    | String_P(s) -> "\"" + s + "\""
    | Symbol_P(s) -> s
    | Func_P(_) -> "#<procedure>"
    | Dot_P -> "."
    | List_P(ps) -> "(" + String.Join(" ", List.map printParser ps) + ")"
    | Container_P(o) -> o.ToString() |> sprintf "#<object:\"%s\">"

//A simple parser
let rec private parserToSyntax (macro_env : MacroEnvironment) parser =
    let parse' = parserToSyntax macro_env
    match parser with
    | Dot_P          -> failwith "illegal use of \".\""
    | Number_P(n)    -> Number_S(n)
    | String_P(s)    -> String_S(s)
    | Symbol_P(s)    -> Id(s)
    | Func_P(f)      -> Func_S(f)
    | Container_P(o) -> Container_S(o)
    | List_P([])     -> List_S([])
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
               let rec makeLet names bndings = function
                  | List_P([Symbol_P(name); bind]) :: t -> makeLet (name :: names) ((parse' bind) :: bndings) t
                  | [] -> 
                    let f = if s = "let" then Let else LetRec
                    f(names, bndings, Begin(List.map parse' body))
                  | m -> sprintf "Syntax error in %s bindings." s |> failwith
               makeLet [] [] bindings
            | m -> sprintf "Syntax error in %s." s |> failwith

        //lambda
        | Symbol_P("lambda") | Symbol_P("λ") ->
            match t with
            | List_P(parameters) :: body ->
                let rec paramMap acc = function
                    | [Dot_P; Symbol_P(s)] -> Tail(s) :: acc |> List.rev 
                    | Symbol_P(s) :: t -> paramMap (Normal(s) :: acc) t
                    | [] -> List.rev acc
                    | m -> failwith "Syntax error in function definition."
                Fun(paramMap [] parameters, Begin(List.map parse' body))
            | m -> List_P(t) |> printParser |> sprintf "Syntax error in function definition: %s" |> failwith

        //if
        | Symbol_P("if") ->
            match t with
            | [cond; then_case] -> If(parse' cond, parse' then_case, Begin([]))
            | [cond; then_case; else_case] -> If(parse' cond, parse' then_case, parse' else_case)
            | m -> failwith "Syntax error in if"//: %s" expr |> failwith

        //define
        | Symbol_P("define") as d -> 
            match t with
            | Symbol_P(name) :: body -> Define(name, Begin(List.map parse' body))
            | List_P(name :: ps) :: body -> 
                parse' (List_P([d; name; List_P(Symbol_P("lambda") :: List_P(ps) :: body)]))
            | m -> failwith "Syntax error in define"//: %s" expr |> failwith

        //quote
        | Symbol_P("quote") ->
            match t with
            | [expr] -> Quote_S(expr)
            | m -> failwith "Syntax error in quote"
        | Symbol_P("quasiquote") -> 
            match t with
            | [expr] -> Quasi_S(expr)
            | m -> failwith "Syntax error in quasiquote"

        //unquote
        | Symbol_P("unquote") -> failwith "unquote outside of quote"

        //begin
        | Symbol_P("begin") -> Begin(List.map parse' t)

        //defined macros
        | Symbol_P(s) when macro_env.ContainsKey s -> macro_env.[s] t |> parse'

        //otherwise...
        | _ -> List_S(List.map parse' (h :: t))

//A simple parser
let private stringToParser source =
    let map = function
        | Token.Number(n)   -> Number_P(Double.Parse(n))
        | Token.String(s)   -> String_P(s)
        | Token.Symbol(".") -> Dot_P
        | Token.Symbol(s)   -> Symbol_P(s)
        | _                 -> failwith "Syntax error."
    let rec list f t acc =
        let e, t' = parse' [] t
        parse' (List_P(f e) :: acc) t'
    and parse' acc = function
        | Open :: t            -> list id t acc
        | Close :: t           -> (List.rev acc), t
        | Quote :: Open :: t   -> list (fun e -> [Symbol_P("quote"); List_P(e)]) t acc
        | Quote :: h :: t      -> parse' (List_P([Symbol_P("quote"); map h]) :: acc) t
        | Quasi :: Open :: t   -> list (fun e -> [Symbol_P("quasiquote"); List_P(e)]) t acc
        | Quasi :: h :: t      -> parse' (List_P([Symbol_P("quasiquote"); map h]) :: acc) t
        | Unquote :: Open :: t -> list (fun e -> [Symbol_P("unquote"); List_P(e)]) t acc
        | Unquote :: h :: t    -> parse' (List_P([Symbol_P("unquote"); map h]) :: acc) t
        | h :: t               -> parse' ((map h) :: acc) t
        | []                   -> (List.rev acc), []
    let result, _ = parse' [] (tokenize source)
    result

let private parse = stringToParser >> List.map (parserToSyntax macroEnv)

let rec printSyntax indent syntax = 
    let printLet name names exprs body =
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
             | Let(names, exprs, body) -> printLet "let" names exprs body
             | LetRec(names, exprs, body) -> printLet "letrec" names exprs body
             | Fun(names, body) -> "(lambda (" + String.Join(" ", names) + ") " + printSyntax "" body + ")"
             | List_S(exprs) -> "(" + String.Join(" ", (List.map (printSyntax "") exprs)) + ")"
             | If(c, t, e) -> "(if " + String.Join(" ", (List.map (printSyntax "") [c; t; e])) + ")"
             | Define(names, body) -> "(define (" + String.Join(" ", names) + ")" + printSyntax " " body + ")"
             | Begin(exprs) -> "(begin " + String.Join(" ", (List.map (printSyntax "") exprs)) + ")"
             | Quote_S(p) -> "(quote " + printParser p + ")"
             | Quasi_S(p) -> "(quasiquote " + printParser p + ")"
             | Func_S(_) -> "#<procedure>"
             | Container_S(o) -> o.ToString() |> sprintf "#<object:\"%s\">"

///Converts the given Expression to a string.
let rec print = function
    | List(Dummy(_)::_) -> "" // don't print accumulated statement dummy values
    | List(list)        -> "(" + String.Join(" ", (List.map print list)) + ")"
    | String(s)         -> "\"" + s + "\""
    | Symbol(s)         -> s
    | Number(n)         -> n.ToString()
    | Container(o)      -> o.ToString() |> sprintf "#<object:\"%s\">"
    | Function(_)       -> "#<procedure>"
    | Dummy(_)          -> "" // sometimes useful to emit value for debugging, but normally we ignore

///Prints a malformed statement error.
let private malformed n e = sprintf "Malformed '%s': %s" n (print (List([e]))) |> failwith

///Simple wrapper for arithmatic operations.
let private mathbin op name = function
    //If the arguments coming in consist of at least two numbers...
    | Number(n) :: Number(n2) :: ns ->
        ///function that takes two Expression.Numbers and applies the given op.
        ///if the second argument is not an Expression.Number, then throw exception
        let op' a = function Number(b) -> op a b | m -> malformed (sprintf "%s arg" name) m
        //Reduce list of Expressions (ns) using op'. Pass result to continuation.
        Number(List.fold op' (op n n2) ns)
    //Otherwise, fail.
    | m -> malformed name (List(m))

let private math0 op name start exprs = 
    let op' a = function
        | Number(b) -> op a b
        | m         -> malformed (sprintf "%s arg" name) m
    Number(List.fold op' start exprs)

let private math1 op op2 unary name = function
    | [Number(n)] -> Number(unary n)
    | Number(n) :: ns ->
        match op2 ns with
        | Number(x) -> Number(op n x)
        | m         -> malformed (sprintf "%s arg" name) m
    | m -> malformed name (List(m)) 

//Arithmatic functions
let Add =      math0 (+) "addition" 0.
let Subtract = math1 (-) Add (fun x -> -x) "subtraction"
let Multiply = math0 (*) "multiplication" 1.
let Divide = 
    let div a b = if b = 0. then failwith "Divide by zero" else a / b
    math1 div Multiply (div 1.) "division"
let Modulus =  mathbin (%) "modulus"
let Exponent = mathbin ( ** ) "exponent"

///Simple wrapper for comparison operations.
let private boolMath (op : (IComparable -> IComparable -> bool)) name args =
    let comp a' b' = 
        match op a' b' with
        | true -> Number(1.)
        | _ -> Number(0.)
    match args with
    | [Number(a); Number(b)] -> comp a b
    | [String(a); String(b)] -> comp a b
    | m -> malformed name (List(m))

//Comparison operations.
let LTE = boolMath (<=) "less-than-or-equals"
let GTE = boolMath (>=) "greater-than-or-equals"
let LT =  boolMath (<) "less-than"
let GT =  boolMath (>) "greater-than"
let EQ =  boolMath (=) "equals"

let Not = function
    | [expr] -> if exprToBool expr then Number(0.) else Number(1.)
    | m -> malformed "not" (List(m))

let Xor = function
    | [a; b] ->
        if exprToBool a then 
            if exprToBool b then Number(0.) else Number(1.) 
        else 
            if exprToBool b then Number(1.) else Number(0.)
    | m -> malformed "xor" (List(m))

//Random Number
let private _r = new Random()
let RandomDbl = function 
    | [] -> Number(_r.NextDouble())
    | m  -> malformed "random" (List(m))

//List Functions
let Cons =     function [h; List(t)]         -> (List(h :: t))                         | m -> malformed "cons" (List(m))
let Car =      function [List(h :: _)]       -> h                                      | m -> malformed "car" (List(m))
let Cdr =      function [List(_ :: t)]       -> List(t)                                | m -> malformed "cdr" (List(m))
let Rev =      function [List(l)]            -> List(List.rev l)                       | m -> malformed "reverse" (List(m))
let Len =      function [List(l)]            -> Number(double l.Length)                | m -> malformed "len" (List(m))
let Append =   function [List(l1); List(l2)] -> List(List.append l1 l2)                | m -> malformed "append" (List(m))
let Take =     function [Number(n); List(l)] -> List(Seq.take (int n) l |> List.ofSeq) | m -> malformed "take" (List(m))
let Get =      function [Number(n); List(l)] -> l.Item (int n)                         | m -> malformed "get" (List(m))
let Drop =     function [Number(n); List(l)] -> List(Seq.skip (int n) l |> List.ofSeq) | m -> malformed "drop" (List(m))
let IsEmpty =  function [List(l)]            -> Number(if l.IsEmpty then 1. else 0.)   | m -> malformed "empty?" (List(m))

let rec private reduceLists = function
    | []     -> Seq.empty
    | [xs]   -> seq { for x in xs -> [x] }
    | h :: t -> reduceLists t |> Seq.zip h |> Seq.map (fun (a,b) -> a::b) 

let Map = function
    | Function(f) :: lists -> 
        List(List.map (function List(l) -> l | m -> failwith "bad map arg") lists |> reduceLists |> Seq.map f |> Seq.toList)
    | m -> malformed "map" (List(m))

let FoldL = function
    | Function(f) :: a :: lists ->
        List.map (function List(l) -> l | m -> failwith "bad fold arg") lists |> reduceLists |> Seq.fold (fun a x -> f (x @ [a])) a
    | m -> malformed "foldl" (List(m))

let FoldR = function
    | Function(f) :: a :: lists ->
        List.foldBack (fun x a -> f (x @ [a])) (List.map (function List(l) -> l | m -> failwith "bad fold arg") lists |> reduceLists |> Seq.toList) a
    | m -> malformed "foldr" (List(m))

let Filter = function
    | [Function(p); List(l)] ->
        List(List.filter (fun x -> [x] |> p |> exprToBool) l)
    | m -> malformed "filter" (List(m))

let CartProd = function
    | Function(f) :: lists ->
        let product2 xs ys = seq { for x in xs do for y in ys do yield x,y }
        let rec reduceLists = function
            | []        -> seq { yield [] }
            | xs :: xss -> reduceLists xss |> product2 xs |> Seq.map (fun (a,b) -> a::b)
        List(List.map (function List(l) -> l | m -> failwith "bad cart prod arg") lists |> reduceLists |> Seq.map f |> Seq.toList)
    | m -> malformed "cartesian-product" (List(m))

let ForEach = function
    | [Function(f); List(l)] ->
        for e in l do f [e] |> ignore
        Dummy("for-each")
    | m -> malformed "for-each" (List(m))

///Sorts using natural ordering. Only works for primitive types (numbers, strings)
let Sort = function
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
                | m         -> malformed "sort" m
            //Convert Expression.Numbers to doubles, sort them, then convert them back to Expression.Numbers.
            List(List.map converter l |> List.sort |> List.map (fun n -> Number(n)))
        //If the first element is an Expression.String...
        | String(s) :: _ ->
            //converter: Makes sure given Expression is an Expression.String.
            //           If it is an Expression.String, pull the string from it.
            //           Otherwise, fail.
            let converter = function
                | String(s) -> s
                | m         -> malformed "sort" m
            //Convert Expression.Strings to strings, sort them, then convert them back to Expression.Strings.
            List(List.map converter l |> List.sort |> List.map (fun n -> String(n)))
        //Otherwise, fail.
        | _ -> malformed "sort" (List(l))
    //Otherwise, fail.
    | m -> malformed "sort" (List(m))

let SortBy = function
    | [Function(key); List(h :: t)] ->
        let mapper x = key [x], x
        let first = mapper h
        let sortBy mapper' keyed = 
            List(List.sortBy (function key, _ -> key) keyed 
                 |> List.map (function _, value -> value))
        match first with
        | Number(n), x ->
            let mapper' x =
                match mapper x with
                | Number(n), t -> n, t
                | m            -> failwith "sort-by key function must always return the same type"
            (n, x) :: List.map mapper' t |> sortBy mapper'
        | String(s), x -> 
            let mapper' x =
                match mapper x with
                | String(s), t -> s, t
                | m            -> failwith "sort-by key function must always return the same type"
            (s, x) :: List.map mapper' t |> sortBy mapper'
        | _ -> failwith "key function for sort-by must return strings or numbers"
    | m -> malformed "sort-by" (List(m))      

let SortWith = function
    | [Function(comp); List(l)] ->
        let comp' x y =
            match comp [x; y] with
            | Number(n) -> int n
            | _         -> failwith "sort-with comparitor must return numbers"
        List(List.sortWith comp' l)
    | m -> malformed "sort-with" (List(m))

///Build List
let BuildSeq = function
    | [Number(start); Number(stop); Number(step)] -> [start .. step .. stop] |> List.map Number |> List
    | m -> malformed "build-seq" (List(m))

///Converts strings to numbers
let StringToNum = function
    | [String(s)] -> Number(Convert.ToDouble(s))
    | m -> malformed "string->num" (List(m))

let NumToString = function
    | [Number(n)] -> String(n.ToString())
    | m -> malformed "num->string" (List(m))

let Concat = function
    | [List(l)] -> 
        let rec concat a = function
            | String(s) :: l -> concat (a + s) l
            | [] -> String(a)
            | m :: _ -> malformed "string" m
        concat "" l
    | m -> malformed "concat" (List(m))

///Error construct
let Throw = function
    | [String(s)] -> failwith s
    | m -> malformed "throw" (List(m))

///Display construct -- used to print to stdout
let Display = function
    | [e] -> print e |> printf "DISPLAY: %s \n"; Dummy("Dummy 'display'")
    | m   -> malformed "display" (List(m))

let Apply = function
    | [Function(f); List(args)] -> f args
    | m -> malformed "apply" (List(m))

let Add1 = function
    | [Number(n)] -> Number(n + 1.)
    | m -> malformed "add1" (List(m))

let Sub1 = function
    | [Number(n)] -> Number(n - 1.)
    | m -> malformed "sub1" (List(m))

let Identity = function
   | [e] -> e
   | m   -> malformed "identity" (List(m))


type private CompilerFrame = string list
type private CompilerEnv = CompilerFrame list ref

let private findInEnv (name : string) compenv =
    let rec find acc = function
        | h :: t ->
            match List.tryFindIndex ((=) name) h with
            | Some(i) -> Some(acc, i)
            | None    -> find (acc + 1) t
        | [] -> None
    find 0 compenv

//Returns a function that takes an argument, ignores it, and returns the given input to wrap.
let private wrap x = fun _ -> x

///Compiles Syntax
let rec private compile (compenv : CompilerEnv) syntax : (Environment -> Expression) =
    ///Utility function that compiles the given expression in the current environment
    let compile' = compile compenv
   
    //And let's begin the match
    match syntax with
    //Objects are passed as their Expression equivalent
    | Number_S(n)    -> Number(n)|> wrap
    | String_S(s)    -> String(s) |> wrap
    | Func_S(f)      -> Function(f) |> wrap
    | Container_S(o) -> Container(o) |> wrap
   
    //Identifiers
    | Id(id) ->
        match findInEnv id compenv.Value with
        //If the identifier is in the compiler environment (name registry), then we fetch it from the environment at runtime.
        | Some(i1, i2) -> fun env -> (env.Value.Item i1).Value.[i2].Value
        //If it's not there, then it's free identifier.
        | None         -> sprintf "Unbound identifier: %s" id |> failwith
   
    //Set!
    | SetId(id, expr) ->
        match findInEnv id compenv.Value with
        //If the identifier is in the compiler environment...
        | Some(i1, i2) ->
            ///Compiled sub-expression
            let ce = compile' expr 
            //At runtime we...
            fun env ->
                //Store the result of the expression in the identifier's box
                (env.Value.Item i1).Value.[i2] := ce env
                //And return a dummy
                Dummy(sprintf "set! %s" id)
        //If it's not there, then it's a free identifier.
        | None -> sprintf "Unbound identifier: %s" id |> failwith
   
    //Lets are really just anonymous function calls.
    | Let(names, exprs, body) -> compile' (List_S(Fun(List.map Normal names, body) :: exprs))
   
    //Recursive let, all identifiers must be added to the environment before any binding expressions are evaluated.
    | LetRec(names, exprs, body) ->
        ///Environment containing recursive bindings
        let compenv' = names :: compenv.Value |> ref
        ///Compiled body
        let cbody = compile compenv' body
        ///Compiled binding expressions
        let cargs = List.map (compile compenv') exprs
        ///Runtime identifier boxes
        let boxes = [| for _ in cargs -> ref (Dummy("letrec")) |]
        //At runtime...
        fun env ->
            //We append the new frame to the environment
            let env' = ref boxes :: env.Value |> ref
            //We evaluate all the binding expressions and store them in their respective identifier's box.
            for (expr, box) in Seq.zip cargs boxes do box := expr env'
            //We evaluate the body in the new environment and return the result.
            cbody env'

    //Define mutates the current environment with new identifier bindings.
    | Define(name, body) ->
        ///Dummy value returned by define statements.
        let dummy = Dummy(sprintf "defined '%s'" name)
        match List.tryFindIndex ((=) name) compenv.Value.Head with
        //If the identifier being defined is already in the environment, replace it with the new binding.
        | Some(idx) ->
            let cbody = compile' body
            fun env ->
                let box = env.Value.Head.Value.[idx]
                box := cbody env
                dummy
        //If it's not there, then we need to add it.
        | None ->
            ///The index of the new identifier box in the mutated environment.
            let lastindex = compenv.Value.Head.Length
            //Update the compiler environment.
            compenv := (compenv.Value.Head @ [name]) :: compenv.Value.Tail
            ///Compiled binding expression.
            let cbody = compile compenv body
            ///Dummy value for undefined identifiers.
            let dummy' = Dummy(sprintf "define '%s'" name)
            //At runtime...
            fun env -> 
                ///New identifier's box
                let def = ref (dummy')
                //Resize the environment to accomodate the new identifier
                Array.Resize(env.Value.Head, env.Value.Head.Value.Length + 1)
                //Place the box in the environment
                env.Value.Head.Value.SetValue(def, lastindex)
                //Evaluate the binding expression with the mutated environment
                def := cbody env
                //Return the dummy for the define statement
                dummy
   
    //Functions
    | Fun(parameters, body) ->
        ///Traverses a syntax tree looking for new identifiers from define statements.
        let findAllDefs = function
            | Begin(exprs) ->
                let rec pred defacc = function
                    | h :: t -> 
                        match h with
                        | Define(name, _) -> pred (name :: defacc) t
                        | Begin(exprs)    -> pred (pred defacc exprs) t
                        | _               -> pred defacc t
                    | [] -> List.rev defacc
                pred [] exprs
            | Define(name, _) -> [name]
            | _               -> []
        ///All identifiers being defined in the funtion's body.
        let defs = findAllDefs body
        ///Utility to extract the name of the given parameter.
        let paramToString = function Normal(s) | Tail(s) -> s
        ///Compiler environment containing the parameter names
        let compenv' = (defs @ List.map paramToString parameters) :: compenv.Value |> ref
        ///Compiled function body
        let cbody = compile compenv' body
        ///Number of sub-definitions
        let buffer = defs.Length
        ///Size of the environment frame to be created for this function
        let framesize = parameters.Length + buffer
        ///Default value for uninitialized identifiers
        let dummy = Dummy("undefined")
        ///Creates a new runtime environment frame from the arguments to this function 
        let pack args =
            ///New frame for the function call
            let frame = Array.zeroCreate framesize
            ///Recursive helper for processing arguments
            let rec pack' idx args = function
                //If there's one parameter left and it's a Tail, then store the parameter's argument as all the arguments in a list.
                | [Tail(_)] -> frame.[idx] <- List(args) |> ref
                //If there is more than one parameter left...
                | _ :: xs -> 
                    match args with
                    //Reference the first arg in the list, then pack the remaining args with the remaining names.
                    | h :: t -> frame.[idx] <- ref h; pack' (idx + 1) t xs
                    //Not enough arguments.
                    | _      -> sprintf "Arity mismatch: Cannot apply %i-arity function on %i arguments." parameters.Length args.Length |> failwith
                //If there are no parameters left...
                | [] ->
                    match args with
                    //If there are also no arguments left, we're done.
                    | [] -> ()
                    //Too many arguments.
                    | _  -> sprintf "Arity mismatch: Cannot apply %i-arity function on %i arguments." parameters.Length args.Length |> failwith
            ///List of identifier boxes for parameters
            pack' buffer args parameters
            //Initialize inner-define identifiers with dummy value
            for i in 0 .. (buffer - 1) do frame.[i] <- ref dummy
            //If we don't, just create a frame out of the packed arguments.
            frame
        //At runtime, we need to add the arguments to the environment and evaluate the body.
        fun env -> Function(fun exprs -> (pack exprs |> ref) :: env.Value |> ref |> cbody)
   
    //Function calls
    | List_S(fun_expr :: args) ->
        ///Compiled function
        let cfun = compile' fun_expr
        ///Compiled arguments
        let cargs = List.map compile' args
        ///At runtime...
        fun env ->
            //Evaluate the function expression
            match cfun env with
            //If it's a function, then evaluate the arguments and apply the function.
            | Function(f) -> List.map (fun x -> x env) cargs |> f
            //Can't call something that's not a function
            | m           -> printSyntax "" syntax |> sprintf "expected function for call: %s" |> failwith
   
    //Conditionals
    | If(cond, then_expr, else_expr) ->
        ///Compiled test
        let ccond = compile' cond
        ///Compiled then branch
        let cthen = compile' then_expr
        ///Compiled else branch
        let celse = compile' else_expr
        //At runtime, evaluate the expression and select the correct branch
        fun env -> if ccond env |> exprToBool then cthen env else celse env
   
    //A begin statement with one sub-expression is the same as just the sub-expression.
    | Begin([expr]) -> compile' expr
   
    //Expression sequences
    | Begin(exprs) ->
        ///Compiled expressions
        let body = List.map compile' exprs
        ///Dummy value for empty begin statements
        let d = Dummy("empty begin")
        ///Tail-recursive helper for evaluating a sequence of expressions.
        let rec fold env = function
            | [x]    -> x env
            | h :: t -> h env |> ignore; fold env t
            | []     -> d
        //At runtime, evaluate all expressions in the body and return the result of the last one.
        fun env -> fold env body
   
    //Code quotations
    | Quote_S(parser) -> makeQuote false compenv parser //quote (')
    | Quasi_S(parser) -> makeQuote true compenv parser //quasiquote (`)
   
    //Anything else isn't right.
    | m -> failwith "Malformed expression"

///Creates a code quotation
and private makeQuote isQuasi compenv = function
    | Number_P(n)    -> Number(n) |> wrap
    | String_P(s)    -> String(s) |> wrap
    | Symbol_P(s)    -> Symbol(s) |> wrap
    | Dot_P          -> Symbol(".") |> wrap
    | Func_P(f)      -> Function(f) |> wrap
    | Container_P(o) -> Container(o) |> wrap
    | List_P(Symbol_P("unquote") :: t) when isQuasi ->
        match t with
        | [expr] -> parserToSyntax macroEnv expr |> compile compenv
        | _      -> failwith "malformed 'unquote'"
    | List_P(exprs) ->
        let qargs = List.map (makeQuote isQuasi compenv) exprs
        fun env -> List(List.map (fun x -> x env) qargs)

///Eval construct -- evaluates code quotations
and Eval args =
    let rec toParser = function
        | Symbol(s)   -> Symbol_P(s)
        | Number(n)   -> Number_P(n)
        | String(s)   -> String_P(s)
        | Function(f) -> Func_P(f)
        | List(l)     -> List_P(List.map toParser l)
        | m           -> malformed "eval" m
    match args with
    | [arg] -> (toParser arg |> parserToSyntax macroEnv |> compile compileEnvironment) environment
    | m     -> malformed "eval" (List(m))


///Load construct -- loads library files, reads them using the simple tokenizer and parser.
and load file = Load [String(file)] |> ignore
and Load = function
    | [String(file)] ->
         (File.OpenText(file)).ReadToEnd() 
             |> List.ofSeq |> parse
             |> List.iter (fun x -> compile compileEnvironment x environment |> ignore)
         Dummy(sprintf "Loaded '%s'." file)
    | m -> malformed "load" (List(m))

///Our base compiler environment
and compileEnvironment : CompilerEnv = [[]] |> ref

///Our base environment
and environment : Environment = [[||] |> ref ] |> ref

let mutable private tempEnv : (string * Expression ref) list = []

///Adds a new binding to the default environment
let AddDefaultBinding name expr = tempEnv <- (name, ref expr) :: tempEnv

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
    AddDefaultBinding "true" (Number(1.))
    AddDefaultBinding "false" (Number(0.))
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
    AddDefaultBinding "list" (Function(List))
    AddDefaultBinding "sort" (Function(Sort))
    AddDefaultBinding "throw" (Function(Throw))
    AddDefaultBinding "rand" (Function(RandomDbl))
    AddDefaultBinding "string->num" (Function(StringToNum))
    AddDefaultBinding "num->string"(Function(NumToString))
    AddDefaultBinding "concat-strings" (Function(Concat))
    AddDefaultBinding "eval" (Function(Eval))
    AddDefaultBinding "apply" (Function(Apply))
    AddDefaultBinding "add1" (Function(Add1))
    AddDefaultBinding "sub1" (Function(Sub1))
    AddDefaultBinding "identity" (Function(Identity))
    AddDefaultBinding "map" (Function(Map))
    AddDefaultBinding "filter" (Function(Filter))
    AddDefaultBinding "foldl" (Function(FoldL))
    AddDefaultBinding "foldr" (Function(FoldR))
    AddDefaultBinding "not" (Function(Not))
    AddDefaultBinding "xor" (Function(Xor))
    AddDefaultBinding "cartesian-product" (Function(CartProd))
    AddDefaultBinding "sort-with" (Function(SortWith))
    AddDefaultBinding "sort-by" (Function(SortBy))
    AddDefaultBinding "for-each" (Function(ForEach))

let private eval ce env syntax = compile ce syntax env

///Evaluates the given Syntax
let Evaluate = eval compileEnvironment environment

///Parses and evaluates an expression given in text form, and returns the resulting expression
let ParseText = List.ofSeq >> parse >> Begin >> Evaluate

let private evaluateSchemeDefs() = 
    "
    ;; Y Combinator
    (define (Y f) ((λ (x) (x x)) (λ (x) (f (λ (g) ((x x) g))))))
    " |> ParseText |> ignore

makeEnvironments()
environment := [Seq.map (fun (_, x) -> x) tempEnv |> Seq.toArray |> ref]
compileEnvironment := [List.map (fun (x, _) -> x) tempEnv]
evaluateSchemeDefs()

///Read/Eval/Print
let REP = ParseText >> print

///Debug version of rep
let private REPd text =
    let watch = Stopwatch.StartNew()
    let x = ParseText text
    watch.Stop()
    sprintf "%s\nTime Elapsed: %d ms" (print x) watch.ElapsedMilliseconds

///Read/Eval/Print Loop
let REPL debug : unit =
    let runner = if debug then REPd else REP
    let rec repl' output =
        printf "%s\n> " output
        try Console.ReadLine() |> runner |> repl'
        with ex -> repl' ex.Message
    repl' ""

type ErrorLog = delegate of string -> unit

///Tests
let private test (log : ErrorLog) =
    let success = ref true
    let rep ce env = List.ofSeq >> parse >> Begin >> eval ce env >> print
    let case source expected =
        try
            let testEnv = List.map (fun (x : Frame) -> Array.map (fun (y : Expression ref) -> ref y.Value) x.Value |> ref) environment.Value |> ref
            let testCEnv = compileEnvironment.Value |> ref
            let output = rep testCEnv testEnv source
            if output <> expected then
                success := false
                sprintf "TEST FAILED: %s [Expected: %s, Actual: %s]" source expected output |> log.Invoke
        with ex ->
            success := false 
            sprintf "TEST CRASHED: %s [%s]" ex.Message source |> log.Invoke

    //Engine Tests
    case "(quote (* 2 3))" "(* 2 3)" // quote primitive
    case "(eval '(* 2 3))" "6" // eval quoted expression
    case "(quote (* 2 (- 5 2)))" "(* 2 (- 5 2))" // quote nested
    case "(quote (* 2 (unquote (- 5 2))))" "(* 2 (unquote (- 5 2)))" // quote nested unquote
    case "(quasiquote (* 2 (unquote (- 5 2))))" "(* 2 3)" // quote nested unquote
    case "(let ((a 1)) (begin (set! a 2) a))" "2" // begin and assign
    case "(let* ((a 5) (dummy (set! a 10))) a)" "10" // re-assign after let
    case "((λ (_ . x) x) 1 2 3)" "(2 3)" // varargs
    case "(reverse '(1 2 3))" "(3 2 1)" // reverse
    case "(list 1 2 3)" "(1 2 3)" //list
    case "(let ((square (λ (x) (* x x)))) (map square '(1 2 3 4 5 6 7 8 9)))" "(1 4 9 16 25 36 49 64 81)" // mapping
    case "(let ((square (λ (x) (* x x)))) (map square '(9)))" "(81)" // mapping single
    case "(let ((square (λ (x) (* x x)))) (map square '()))" "()" // mapping empty

    //Scope
    case "(let ((y 6)) (let ((f (λ (x) (+ x y)))) (let ((y 5)) (f 4))))" "10" //lexical
    case "(begin (define (t x) (define y 5) (+ x y)) (t 6))" "11" //nested defines

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

    //Cond
    case "(cond (else 10))" "10"
    case "(cond ((< 0 1) 10))" "10"
    case "(cond ((> 0 1) 10))" ""
    case "(cond ((> 0 1) 10) (else 20))" "20"

    //Xor
    case "(xor 0 0)" "0" // xor (false)
    case "(xor 1 0)" "1" // xor (true)
    case "(xor 0 1)" "1" // xor (true)
    case "(xor 1 1)" "0" // xor (false)
   
    //Built-in Function Tests
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

    //Apply
    case "(apply + '(1 2))" "3"
    case "(apply append '((1) (2)))" "(1 2)"
   
    //Fold
    case "(foldl cons empty '(1 2 3))"   "(3 2 1)"
    case "(foldr cons empty '(1 2 3))"   "(1 2 3)"
    case "(foldl + 0 '(1 2 3))"          "6"
    case "(foldr + 0 '(1 2 3) '(4 5 6))" "21"
    case "(foldl * 1 '(2 3) '(4 5))"     "120"
   
    //Map
    case "(map (λ (x) x) '(1 2 3))" "(1 2 3)"
    case "(map append '((1) (2) (3)) '((4) (5) (6)))" "((1 4) (2 5) (3 6))"
   
    //Filter
    case "(filter (λ (x) (< x 2)) '(0 2 3 4 1 6 5))" "(0 1)"
   
    //Cartesian Product
    case "(cartesian-product list (list 1 2) (list 3 4) (list 5 6))" 
        "((1 3 5) (1 3 6) (1 4 5) (1 4 6) (2 3 5) (2 3 6) (2 4 5) (2 4 6))"
   
    //Sorting
    case "(sort-by (λ (x) (foldl + 0 x)) '((2 2) (2 1) (1 1)))" "((1 1) (2 1) (2 2))"
    case "(sort-with (λ (x y) (let ((size (λ (l) (foldl + 0 l)))) (- (size x) (size y)))) '((2 2) (2 1) (1 1)))" "((1 1) (2 1) (2 2))"

    //Y Combinator
    case "((Y (λ (f) (λ (n) (if (<= n 1) 1 (* n (f (sub1 n))))))) 5)" "120"

    //For-Each
    case "(let ((x empty)) (for-each (λ (y) (set! x (cons y x))) '(1 2 3 4 5)) x)" "(5 4 3 2 1)"

    success.Value

let RunTests() = 
    if ErrorLog(Console.WriteLine) |> test then 
        Console.WriteLine("All Tests Passed.")
