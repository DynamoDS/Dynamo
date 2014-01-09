module Dynamo.FScheme
#light

//An F# Scheme Interpreter
open System
open System.IO
open System.Diagnostics

//Simple Tokenizer for quickly defining expressions in Scheme syntax.
type private Token =
    | Open | Close
    | QuoteT | QuasiT | Unquote
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
        | '\'' :: t -> tokenize' (QuoteT :: acc) t
        | '`' :: t -> tokenize' (QuasiT :: acc) t
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


///Types of FScheme Values
type Value =
    ///Expression representing any .NET object.
    | Container of obj
    ///Expression representing a number (double).
    | Number of double
    ///Expression representing a string.
    | String of string
    ///Expression representing a symbol.
    | Symbol of string
    ///Expression representing a list of sub expressions.
    | List of Value list
    ///Expression representing a function.
    | Function of (Value list -> Value)
    ///Invalid value (used for mutation, where expressions shouldn't return anything).
    | Dummy of string


///Converts an expression to a boolean value
let ValueToBool = function
    //Empty list or empty string is false.
    | List([]) | String("") -> false
    //Zero is false.
    | Number(n) when n = 0. -> false
    //Everything else is true.
    | _ -> true

type Frame = Value ref [] ref
type Environment = Frame list ref

///FScheme Function delegate. Takes a list of Expressions as arguments, le returns an Expression.
type ExternFunc = delegate of Value list -> Value

///Makes an Expression.Function out of an ExternFunc
let makeExternFunc (externFunc : ExternFunc) =
    Function(externFunc.Invoke)

type Syntax =
    | Number_S of double
    | String_S of string
    | Symbol_S of string
    | Dot_S
    | Func_S of (Value list -> Value)
    | List_S of Syntax list
    | Container_S of obj

type Macro = Syntax list -> Syntax
type MacroEnvironment = Map<string, Macro>

///Let* macro
let LetStar : Macro = function
    | List_S(bindings) :: body ->
        let folder b a =
            match b with
            | List_S([Symbol_S(name); expr]) as let' ->
                List_S([Symbol_S("let"); List_S([let']); a])
            | m -> failwith "bad let*"
        List.foldBack folder bindings <| List_S(Symbol_S("begin") :: body)
    | m -> failwith "bad let*"

///And macro
let rec And : Macro = function
    | []     -> Number_S(1.)
    | [expr] -> expr
    | h :: t -> List_S([Symbol_S("if"); h; And t; Number_S(0.)])

///Or macro
let rec Or : Macro = function
    | []     -> Number_S(0.)
    | [expr] -> expr
    | h :: t -> List_S([Symbol_S("if"); h; Number_S(1.); Or t])

//Cond macro
let rec Cond : Macro = function
    | [List_S([Symbol_S("else"); expr])] -> expr
    | List_S([condition; expr]) :: t     -> List_S([Symbol_S("if"); condition; expr; Cond t])
    | []                                 -> List_S([Symbol_S("begin")])
    | m                                  -> failwith "bad cond"

let macroEnv =
    Map.ofList [
        "let*", LetStar
        "cond", Cond
        "and", And
        "or", Or
    ]

///FScheme Macro delegate.
type ExternMacro = delegate of Syntax list -> Syntax

///Makes a Macro out of an ExternMacro
let makeExternMacro (ex : ExternMacro) : Macro = ex.Invoke

///AST for FScheme expressions
type Expression =
    | Number_E of double
    | String_E of string
    | Id of string
    | SetId of string * Expression
    | Let of string list * Expression list * Expression
    | LetRec of string list * Expression list * Expression
    | Fun of Parameter list * Expression
    | List_E of Expression list
    | If of Expression * Expression * Expression
    | Define of string * Expression
    | Begin of Expression list
    | Quote of Syntax
    | Quasi of Syntax
    | Function_E of (Value list -> Value)
    | Container_E of obj
    | Value_E of Value

and Parameter =
    | Normal of string
    | Tail of string

let rec private printSyntax = function
    | Number_S(n)    -> n.ToString()
    | String_S(s)    -> "\"" + s + "\""
    | Symbol_S(s)    -> s
    | Func_S(_)      -> "#<procedure>"
    | Dot_S          -> "."
    | List_S(ps)     -> "(" + String.Join(" ", List.map printSyntax ps) + ")"
    | Container_S(o) -> sprintf "#<object:\"%s\">" <| o.ToString()

//A simple parser
let rec private syntaxToExpression (macro_env : MacroEnvironment) parser =
    let parse' = syntaxToExpression macro_env
    match parser with
    | Dot_S          -> failwith "illegal use of \".\""
    | Number_S(n)    -> Number_E(n)
    | String_S(s)    -> String_E(s)
    | Symbol_S(s)    -> Id(s)
    | Func_S(f)      -> Function_E(f)
    | Container_S(o) -> Container_E(o)
    | List_S([])     -> List_E([])
    | List_S(h :: t) ->
        match h with
        //Set!
        | Symbol_S("set!") ->
            match t with
            | Symbol_S(name) :: body -> SetId(name, Begin(List.map parse' body))
            | m                      -> failwith "Syntax error in set!"

        //let and letrec
        | Symbol_S(s) when s = "let" || s = "letrec" ->
            match t with
            | List_S(bindings) :: body ->
                let rec makeLet names bndings = function
                    | List_S([Symbol_S(name); bind]) :: t ->
                        makeLet (name :: names) ((parse' bind) :: bndings) t
                    | [] ->
                        let f = if s = "let" then Let else LetRec
                        f(names, bndings, Begin(List.map parse' body))
                    | m -> failwith <| sprintf "Syntax error in %s bindings." s
                makeLet [] [] bindings
            | m -> failwith <| sprintf "Syntax error in %s." s

        //lambda
        | Symbol_S("lambda") | Symbol_S("λ") ->
            match t with
            | List_S(parameters) :: body ->
                let rec paramMap acc = function
                    | [Dot_S; Symbol_S(s)] -> Tail(s) :: acc |> List.rev
                    | Symbol_S(s) :: t     -> paramMap (Normal(s) :: acc) t
                    | []                   -> List.rev acc
                    | m                    -> failwith "Syntax error in function definition."
                Fun(paramMap [] parameters, Begin(List.map parse' body))
            | m -> List_S(t) |> printSyntax |> sprintf "Syntax error in function definition: %s" |> failwith

        //if
        | Symbol_S("if") ->
            match t with
            | [cond; then_case]            -> If(parse' cond, parse' then_case, Begin([]))
            | [cond; then_case; else_case] -> If(parse' cond, parse' then_case, parse' else_case)
            | m                            -> failwith "Syntax error in if"//: %s" expr |> failwith

        //define
        | Symbol_S("define") as d ->
            match t with
            | Symbol_S(name) :: body -> Define(name, Begin(List.map parse' body))
            | List_S(name :: ps) :: body ->
                parse' <| List_S([d; name; List_S(Symbol_S("lambda") :: List_S(ps) :: body)])
            | m -> failwith "Syntax error in define"//: %s" expr |> failwith

        //quote
        | Symbol_S("quote") ->
            match t with
            | [expr] -> Quote(expr)
            | m      -> failwith "Syntax error in quote"
        | Symbol_S("quasiquote") ->
            match t with
            | [expr] -> Quasi(expr)
            | m      -> failwith "Syntax error in quasiquote"

        //unquote
        | Symbol_S("unquote") -> failwith "unquote outside of quote"

        //begin
        | Symbol_S("begin") -> Begin(List.map parse' t)

        //defined macros
        | Symbol_S(s) when macro_env.ContainsKey s ->  parse' <| macro_env.[s] t

        //otherwise...
        | _ -> List_E(List.map parse' <| h :: t)

//A simple parser
let private stringToSyntax source =
    let map = function
        | Token.Number(n)   -> Number_S(Double.Parse(n))
        | Token.String(s)   -> String_S(s)
        | Token.Symbol(".") -> Dot_S
        | Token.Symbol(s)   -> Symbol_S(s)
        | _                 -> failwith "Syntax error."
    let rec list f t acc =
        let e, t' = parse' [] t
        parse' (List_S(f e) :: acc) t'
    and parse' acc = function
        | Open :: t            -> list id t acc
        | Close :: t           -> (List.rev acc), t
        | QuoteT :: Open :: t  -> list (fun e -> [Symbol_S("quote"); List_S(e)]) t acc
        | QuoteT :: h :: t     -> parse' (List_S([Symbol_S("quote"); map h]) :: acc) t
        | QuasiT :: Open :: t  -> list (fun e -> [Symbol_S("quasiquote"); List_S(e)]) t acc
        | QuasiT :: h :: t     -> parse' (List_S([Symbol_S("quasiquote"); map h]) :: acc) t
        | Unquote :: Open :: t -> list (fun e -> [Symbol_S("unquote"); List_S(e)]) t acc
        | Unquote :: h :: t    -> parse' (List_S([Symbol_S("unquote"); map h]) :: acc) t
        | h :: t               -> parse' ((map h) :: acc) t
        | []                   -> (List.rev acc), []
    let result, _ = parse' [] (tokenize source)
    result

let private parse = stringToSyntax >> List.map (syntaxToExpression macroEnv)

///Converts the given Value to a string.
let rec print = function
    | List(Dummy(_)::_) -> "" // don't print accumulated statement dummy values
    | List(list)        -> "(" + String.Join(" ", List.map print list) + ")"
    | String(s)         -> "\"" + s + "\""
    | Symbol(s)         -> s
    | Number(n)         -> n.ToString()
    | Container(o)      -> sprintf "#<object:\"%s\">" <| o.ToString()
    | Function(_)       -> "#<procedure>"
    | Dummy(_)          -> "" // sometimes useful to emit value for debugging, but normally we ignore

let rec printExpression indent syntax =
    let printLet name names exprs body =
        "(" + name +  " ("
            + String.Join(
                "\n" + indent + "      ",
                List.map (function (n, b) -> "[" + n + (printExpression " " b) + "]")
                         (List.zip names exprs))
            + ")\n"
            + printExpression (indent + "  ") body
            + ")"
    let printParam = function Normal(s) | Tail(s) -> s
    indent + match syntax with
             | Number_E(n)                -> n.ToString()
             | String_E(s)                -> "\"" + s + "\""
             | Id(s)                      -> s
             | SetId(s, expr)             -> "(set! " + s + " " + printExpression "" expr + ")"
             | Let(names, exprs, body)    -> printLet "let" names exprs body
             | LetRec(names, exprs, body) -> printLet "letrec" names exprs body
             | Fun(names, body)           -> "(lambda (" + String.Join(" ", List.map printParam names) + ") " + printExpression "" body + ")"
             | List_E(exprs)              -> "(" + String.Join(" ", List.map (printExpression "") exprs) + ")"
             | If(c, t, e)                -> "(if " + String.Join(" ", List.map (printExpression "") [c; t; e]) + ")"
             | Define(name, body)         -> "(define " + name + printExpression " " body + ")"
             | Begin(exprs)               -> "(begin " + String.Join(" ", List.map (printExpression "") exprs) + ")"
             | Quote(p)                   -> "(quote " + printSyntax p + ")"
             | Quasi(p)                   -> "(quasiquote " + printSyntax p + ")"
             | Function_E(_)              -> "#<procedure>"
             | Container_E(o)             -> sprintf "#<object:\"%s\">" <| o.ToString()
             | Value_E(v)                 -> print v

///Prints a malformed statement error.
let private malformed n e = sprintf "Malformed '%s': %s" n (print (List([e]))) |> failwith

///Simple wrapper for arithmatic operations.
let private mathbin op name = function
    //If the arguments coming in consist of at least two numbers...
    | Number(n) :: Number(n2) :: ns ->
        ///function that takes two Numbers and applies the given op.
        ///if the second argument is not an Number, then throw exception
        let op' a = function Number(b) -> op a b | m -> malformed (sprintf "%s arg" name) m
        //Reduce list of Values (ns) using op'. Pass result to continuation.
        Number(List.fold op' (op n n2) ns)
    //Otherwise, fail.
    | m -> malformed name <| List(m)

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
    | m -> malformed name <| List(m)

//Arithmatic functions
let Add =      math0 (+) "addition" 0.
let Subtract = math1 (-) Add (fun x -> -x) "subtraction"
let Multiply = math0 (*) "multiplication" 1.
let Divide =
    let div a b = if b = 0. then failwith "Divide by zero" else a / b
    math1 div Multiply (div 1.) "division"
let Modulus =  mathbin (%) "modulus"
let Exponent = mathbin ( ** ) "exponent"

let Sqrt = function
   | [Number(n)] -> Number(Math.Sqrt n)
   | m -> malformed "square root" <| List(m)

///Simple wrapper for comparison operations.
let private boolMath (op : (IComparable -> IComparable -> bool)) name args =
    let comp a' b' =
        match op a' b' with
        | true -> Number(1.)
        | _ -> Number(0.)
    match args with
    | [Number(a); Number(b)] -> comp a b
    | [String(a); String(b)] -> comp a b
    | m -> malformed name <| List(m)

//Comparison operations.
let LTE = boolMath (<=) "less-than-or-equals"
let GTE = boolMath (>=) "greater-than-or-equals"
let LT =  boolMath (<) "less-than"
let GT =  boolMath (>) "greater-than"

let rec EQ = function
    | [Number(x1); Number(x2)] when x1 = x2 -> Number(1.)
    | [String(x1); String(x2)] when x1 = x2 -> Number(1.)

    | [List(x1); List(x2)] when x1.Length = x2.Length ->
        if Seq.forall2 (fun x y -> match EQ <| [x; y] with Number(1.) -> true | _ -> false)  x1 x2 then
            Number(1.)
        else
            Number(0.)
    
    | [Container(o1); Container(o2)] when o1 = o2 || o1.Equals(o2) -> Number(1.)

    | m -> Number(0.)
    

let Not = function
    | [expr] -> if ValueToBool expr then Number(0.) else Number(1.)
    | m -> malformed "not" <| List(m)

let Xor = function
    | [a; b] ->
        if ValueToBool a then
            if ValueToBool b then Number(0.) else Number(1.)
        else
            if ValueToBool b then Number(1.) else Number(0.)
    | m -> malformed "xor" <| List(m)

//Random Number
let private _r = new Random()
let RandomDbl = function
    | [] -> Number(_r.NextDouble())
    | m  -> malformed "random" <| List(m)

let rec private last = function
    | hd :: [] -> hd
    | hd :: tl -> last tl
    | m -> malformed "last" <| List(m)

//List Functions
let Car =     function [List(h :: _)]       -> h                                      | m -> malformed "car"     <| List(m)
let Cdr =     function [List(_ :: t)]       -> List(t)                                | m -> malformed "cdr"     <| List(m)
let Cons =    function [h; List(t)]         -> (List(h :: t))                         | m -> malformed "cons"    <| List(m)
let Get =     function [Number(n); List(l)] -> l.Item (int n)                         | m -> malformed "get"     <| List(m)
let Rev =     function [List(l)]            -> List(List.rev l)                       | m -> malformed "reverse" <| List(m)
let Len =     function [List(l)]            -> Number(double l.Length)                | m -> malformed "len"     <| List(m)
let Append =  function [List(l1); List(l2)] -> List(List.append l1 l2)                | m -> malformed "append"  <| List(m)
let IsEmpty = function [List(l)]            -> Number(if l.IsEmpty then 1. else 0.)   | m -> malformed "empty?"  <| List(m)
let Last =    function [List(l)]            -> last l                                 | m -> malformed "last"    <| List(m)

let Take = function 
    | [Number(n); List(l)] ->
        let n = (int n)
        if n >= 0 then
            List(Seq.take n l |> List.ofSeq) 
        else
            List(Seq.skip ((List.length l) + n) l |> List.ofSeq)
    | m -> malformed "take" <| List(m)

let Drop = function
    | [Number(n); List(l)] -> 
        let n = (int n)
        if n >= 0 then
            List(Seq.skip n l |> List.ofSeq)
        else
            List(Seq.take ((List.length l) + n) l |> List.ofSeq)
    | m -> malformed "drop" <| List(m)

let private transpose lists =
    let rec transpose' acc lists rev =
        let rec transposeStep (acc : 'a list) (acc' : 'a list list) rev = function
            | [] :: t      -> transposeStep acc acc' rev t
            | (x::xs) :: t -> transposeStep (x::acc) (xs::acc') rev t
            | []           -> (if rev then List.rev acc else acc), acc'
        match transposeStep [] [] rev lists with
        | [], _ -> List.rev acc
        | r, a  -> transpose' (r::acc) a (not rev)
    transpose' [] lists true

let Transpose = function
    | [List(lists)] ->
        let ls = List.map (function List(l) -> l | m -> failwith "bad transpose arg") lists
        let transposed = transpose ls
        List.map List transposed |> List
    | m -> malformed "transpose" <| List(m)

let Map = function
    | Function(f) :: lists ->
        List.map (function List(l) -> l | m -> failwith "bad map arg") lists
            |> transpose |> Seq.map f |> Seq.toList |> List
    | m -> malformed "map" <| List(m)

let FoldL = function
    | Function(f) :: a :: lists ->
        List.map (function List(l) -> l | m -> failwith "bad fold arg") lists
            |> transpose
            |> Seq.fold (fun a x -> f (x @ [a])) a
    | m -> malformed "foldl" <| List(m)

let FoldR = function
    | Function(f) :: a :: lists ->
        List.foldBack (fun x a' -> f (x @ [a']))
                      (List.map (function List(l) -> l | m -> failwith "bad fold arg") lists
                        |> transpose |> Seq.toList)
                      a
    | m -> malformed "foldr" <| List(m)

let Filter = function
    | [Function(p); List(l)] ->
        List(List.filter (fun x -> [x] |> p |> ValueToBool) l)
    | m -> malformed "filter" <| List(m)

let CartProd = function
    | Function(f) :: lists ->
        let product2 xs ys = seq { for x in xs do for y in ys do yield x,y }
        let rec reduceLists = function
            | []        -> seq { yield [] }
            | xs :: xss -> reduceLists xss |> product2 xs |> Seq.map (fun (a,b) -> a::b)
        List(List.map (function List(l) -> l | m -> failwith "bad cart prod arg") lists |> reduceLists |> Seq.map f |> Seq.toList)
    | m -> malformed "cartesian-product" <| List(m)

let LaceShortest = function
    | (Function(_) as f) :: lists ->
        let lists' = List.map (function List(l) -> l | m -> malformed "lace-shortest" m) lists
        let shortestLen = Seq.min <| Seq.map List.length lists'
        Map <| f :: List.map (Seq.take shortestLen >> Seq.toList >> List) lists'
    | m -> malformed "lace-shortest" <| List(m)

let LaceLongest = function
    | (Function(_) as f) :: lists ->
        let lists' = List.map (function List(l) -> l | m -> malformed "lace-longest" m) lists
        let longestLen = Seq.max <| Seq.map List.length lists'
        Map <| f :: List.map 
                        (fun l -> 
                            let len = List.length l
                            let last = List.nth l (len-1)
                            let remainder = Seq.initInfinite (fun _ -> last) |> Seq.take (longestLen - len)
                            Seq.append l remainder |> Seq.toList |> List)
                        lists'
    | m -> malformed "lace-longest" <| List(m)

let ForEach = function
    | [Function(f); List(l)] ->
        for e in l do f [e] |> ignore
        Dummy("for-each")
    | m -> malformed "for-each" <| List(m)

let OrMap = function
    | Function(f) :: lsts ->
        let ls = List.map (function List(l) -> l | m -> failwith "bad ormap arg") lsts
        let transposed = transpose ls
        let rec ormap = function
            | h :: t -> 
                let r = f h
                if ValueToBool r then r else ormap t
            | [] -> Number(0.)
        ormap transposed
    | m -> malformed "ormap" <| List(m)

let AndMap = function
    | Function(f) :: lsts ->
        let ls = List.map (function List(l) -> l | m -> failwith "bad andmap arg") lsts
        let transposed = transpose ls
        let rec andmap = function
            | h :: t -> 
                let r = f h
                if ValueToBool r then andmap t else Number(0.)
            | [] -> Number(1.)
        andmap transposed
    | m -> malformed "andmap" <| List(m)

let private key_sort_base name sortByN sortByS = function
    | [Function(key); List(h :: t)] ->
        let mapper x = key [x], x
        let first = mapper h
        match first with
        | Number(n), x ->
            let mapper' x =
                match mapper x with
                | Number(n), t -> n, t
                | m            -> failwith (name + " key function must always return the same type")
            (n, x) :: List.map mapper' t |> sortByN
        | String(s), x ->
            let mapper' x =
                match mapper x with
                | String(s), t -> s, t
                | m            -> failwith (name + " key function must always return the same type")
            (s, x) :: List.map mapper' t |> sortByS
        | _ -> failwith ("key function for " + name + " must return strings or numbers")
    | m -> malformed name <| List(m)

///Sorts a list using a key mapping function.
let SortBy = function
    | [Function(key); List([]) as l] -> l
    | args ->
        let sortBy keyed =
            List(List.sortBy (function key, _ -> key) keyed
                 |> List.map (function _, value -> value))
        key_sort_base "sort-by" sortBy sortBy args

///Fetches the min of a list using a key mapping function.
let Min = 
    let minBy keyed = List.minBy (function key, _ -> key) keyed |> function _, value -> value
    key_sort_base "min" minBy minBy

///Fetches the max of a list using a key mapping function.
let Max =
    let maxBy keyed = List.maxBy (function key, _ -> key) keyed |> function _, value -> value
    key_sort_base "max" maxBy maxBy

///Sorts using natural ordering. Only works for primitive types (numbers, strings)
let Sort = function
    //We expect a list of expressions as the only argument.
    | [List(l)] ->
        //Peek and see what kind of data we're sorting
        match l with
        //If the first element is an Number...
        | Number(n) :: _ ->
            //converter: Makes sure given Value is an Number.
            //           If it is a Number, pull the double from it.
            //           Otherwise, fail.
            let converter = function
                | Number(n) -> n
                | m         -> malformed "sort" m
            //Convert Numbers to doubles, sort them, then convert them back to Numbers.
            List(List.map converter l |> List.sort |> List.map Number)
        //If the first element is a String...
        | String(s) :: _ ->
            //converter: Makes sure given Value is a String.
            //           If it is aa String, pull the string from it.
            //           Otherwise, fail.
            let converter = function
                | String(s) -> s
                | m         -> malformed "sort" m
            //Convert Strings to strings, sort them, then convert them back to Strings.
            List(List.map converter l |> List.sort |> List.map String)
        //Otherwise, fail.
        | _ -> malformed "sort" <| List(l)
    //Otherwise, fail.
    | m -> malformed "sort" <| List(m)

///Sorts a list using a comparison function.
let SortWith = function
    | [Function(comp); List(l)] ->
        let comp' x y =
            match comp [x; y] with
            | Number(n) -> int n
            | _         -> failwith "sort-with comparitor must return numbers"
        List(List.sortWith comp' l)
    | m -> malformed "sort-with" <| List(m)

///Flatten List
let Flatten = function
    | [List(l2d)] ->
        let rec flatten' a = function
            | List(l) :: t ->
                let rec flatten'' a' = function
                    | h :: t -> flatten'' (h :: a') t
                    | []     -> a'
                flatten' (flatten'' a l) t
            | [] -> List(List.rev a)
            | m  -> malformed "flatten" <| List(m)
        flatten' [] l2d
    | m -> malformed "flatten" <| List(m)

let Range (start : double) (step : double) (stop : double) = { start .. step .. stop }

///Build List
let BuildSeq = function
    | [Number(start); Number(stop); Number(step)] -> [start .. step .. stop] |> List.map Number |> List
    | m -> malformed "build-list" <| List(m)

///Converts strings to numbers
let StringToNum = function
    | [String(s)] -> Number(Convert.ToDouble(s))
    | m -> malformed "string->num" <| List(m)

let NumToString = function
    | [Number(n)] -> String(n.ToString())
    | m -> malformed "num->string" <| List(m)

let Concat = function
    | [List(l)] ->
        let rec concat a = function
            | String(s) :: l -> concat (a + s) l
            | []             -> String(a)
            | m :: _         -> malformed "string" m
        concat "" l
    | m -> malformed "concat" <| List(m)

///Error construct
let Throw = function
    | [String(s)] -> failwith s
    | m -> malformed "throw" <| List(m)

///Display construct -- used to print to stdout
let Display = function
    | [e] -> printf "DISPLAY: %s \n" <| print e; Dummy("display")
    | m   -> malformed "display" <| List(m)

let Apply = function
    | [Function(f); List(args)] -> f args
    | m -> malformed "apply" <| List(m)

let Add1 = function
    | [Number(n)] -> Number(n + 1.)
    | m -> malformed "add1" <| List(m)

let Sub1 = function
    | [Number(n)] -> Number(n - 1.)
    | m -> malformed "sub1" <| List(m)

let Identity = function
    | [e] -> e
    | m   -> malformed "identity" <| List(m)

let MakeFuture f = 
    let p = async { return f [] }
    let t = Async.StartAsTask p
    t
    
let Redeem (t : Threading.Tasks.Task<'a>) =
    t.Wait()
    if t.IsFaulted then
        raise t.Exception
    else if t.IsCanceled then
        failwith "Cannot redeem a cancelled future order."
    else
        t.Result


type private CompilerFrame = string list
type private CompilerEnv = CompilerFrame list ref

let FindInCompilerEnv (name : string) compenv =
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
let rec private compile (compenv : CompilerEnv) expression : (Environment -> Value) =
    ///Utility function that compiles the given expression in the current environment
    let compile' = compile compenv

    //And let's begin the match
    match expression with
    //Objects are passed as their Expression equivalent
    | Number_E(n)    -> wrap <| Number(n)
    | String_E(s)    -> wrap <| String(s)
    | Function_E(f)  -> wrap <| Function(f)
    | Container_E(o) -> wrap <| Container(o)
    | Value_E(v)     -> wrap v

    //Identifiers
    | Id(id) ->
        match FindInCompilerEnv id compenv.Value with
        //If the identifier is in the compiler environment (name registry),
        //then we fetch it from the environment at runtime.
        | Some(i1, i2) -> fun env -> (env.Value.Item i1).Value.[i2].Value
        //If it's not there, then it's a free identifier.
        | None         -> failwith <| sprintf "Unbound identifier: %s" id

    //Set!
    | SetId(id, expr) ->
        match FindInCompilerEnv id compenv.Value with
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
        | None -> failwith <| sprintf "Unbound identifier: %s" id

    //Lets are really just anonymous function calls.
    | Let(names, exprs, body) -> compile' (List_E(Fun(List.map Normal names, body) :: exprs))

    //Recursive let, all identifiers must be added to the environment before any binding expressions are evaluated.
    | LetRec(names, exprs, body) ->
        ///Environment containing recursive bindings
        let compenv' = ref <| names :: compenv.Value
        ///Compiled body
        let cbody = compile compenv' body
        ///Compiled binding expressions
        let cargs = List.map (compile compenv') exprs
        ///Runtime identifier boxes
        let boxes = [| for _ in cargs -> ref (Dummy("letrec")) |]
        //At runtime...
        fun env ->
            //We append the new frame to the environment
            let env' = ref <| ref boxes :: env.Value
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
                env.Value.Head.Value.[idx] := cbody env
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
                let def = ref dummy'
                //Resize the environment to accomodate the new identifier
                Array.Resize(env.Value.Head, lastindex + 1)
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
        ///All identifiers being defined in the function's body.
        let defs = findAllDefs body
        ///Utility to extract the name of the given parameter.
        let paramToString = function Normal(s) | Tail(s) -> s
        ///Compiler environment containing the parameter names
        let compenv' = ref <| (defs @ List.map paramToString parameters) :: compenv.Value
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
                | [Tail(_)] -> frame.[idx] <- ref <| List(args)
                //If there is more than one parameter left...
                | _ :: xs ->
                    match args with
                    //Reference the first arg in the list, then pack the remaining args with the remaining names.
                    | h :: t -> frame.[idx] <- ref h; pack' (idx + 1) t xs
                    //Not enough arguments.
                    | _      -> failwith <| sprintf "Arity mismatch: Cannot apply %i-arity function on %i arguments." parameters.Length args.Length
                //If there are no parameters left...
                | [] ->
                    match args with
                    //If there are also no arguments left, we're done.
                    | [] -> ()
                    //Too many arguments.
                    | _  -> failwith <| sprintf "Arity mismatch: Cannot apply %i-arity function on %i arguments." parameters.Length args.Length
            ///List of identifier boxes for parameters
            pack' buffer args parameters
            //Initialize inner-define identifiers with dummy value
            for i in 0 .. buffer - 1 do frame.[i] <- ref dummy
            //If we don't, just create a frame out of the packed arguments.
            frame
        //At runtime, we need to add the arguments to the environment and evaluate the body.
        fun env -> Function(fun exprs -> (ref <| pack exprs) :: env.Value |> ref |> cbody)

    //Function calls
    | List_E(fun_expr :: args) ->
        ///Compiled function
        let cfun = compile' fun_expr
        ///Compiled arguments
        let cargs = List.map compile' args
        ///At runtime...
        fun env ->
            //Evaluate the function expression
            match cfun env with
            //If it's a function, then evaluate the arguments and apply the function.
            | Function(f) -> f <| List.map (fun x -> x env) cargs
            //Can't call something that's not a function
            | _           -> printExpression "" expression |> sprintf "expected function for call: %s" |> failwith

    //Conditionals
    | If(cond, then_expr, else_expr) ->
        ///Compiled test
        let ccond = compile' cond
        ///Compiled then branch
        let cthen = compile' then_expr
        ///Compiled else branch
        let celse = compile' else_expr
        //At runtime, evaluate the expression and select the correct branch
        fun env -> if ccond env |> ValueToBool then cthen env else celse env

    //An empty begin statement is valid, but returns a dummy
    | Begin([]) -> wrap <| Dummy("empty begin")
    
    //A begin statement with one sub-expression is the same as just the sub-expression.
    | Begin([expr]) -> compile' expr

    //Expression sequences
    | Begin(exprs) ->
        ///Merges all nested begin expressions
        let merge =
            let rec merge' a = function
               | [Begin([]) as e]   -> merge' (compile' e :: a) []
               | Begin(exprs') :: t -> merge' (merge' a exprs') t
               | h :: t             -> merge' (compile' h :: a) t
               | []                 -> a
            merge' [] >> List.rev
        ///Compiled expressions
        let body = merge exprs
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
    | Quote(parser) -> makeQuote false compenv parser //quote (')
    | Quasi(parser) -> makeQuote true compenv parser //quasiquote (`)

    //Anything else isn't right.
    | m -> failwith "Malformed expression"

///Creates a code quotation
and private makeQuote isQuasi compenv = function
    | Number_S(n)    -> wrap <| Number(n)
    | String_S(s)    -> wrap <| String(s)
    | Symbol_S(s)    -> wrap <| Symbol(s)
    | Dot_S          -> wrap <| Symbol(".")
    | Func_S(f)      -> wrap <| Function(f)
    | Container_S(o) -> wrap <| Container(o)
    | List_S(Symbol_S("unquote") :: t) when isQuasi ->
        match t with
        | [expr] -> syntaxToExpression macroEnv expr |> compile compenv
        | _      -> failwith "malformed 'unquote'"
    | List_S(exprs) ->
        let qargs = List.map (makeQuote isQuasi compenv) exprs
        fun env -> List(List.map (fun x -> x env) qargs)

///Eval construct -- evaluates code quotations
(*and Eval args =
    let rec toParser = function
        | Symbol(s)   -> Symbol_S(s)
        | Number(n)   -> Number_S(n)
        | String(s)   -> String_S(s)
        | Function(f) -> Func_S(f)
        | List(l)     -> List_S(List.map toParser l)
        | m           -> malformed "eval" m
    match args with
    | [arg] -> (toParser arg |> syntaxToExpression macroEnv |> compile compileEnvironment) environment
    | m     -> malformed "eval" <| List(m)*)


type FSchemeEnvironment = { cEnv : CompilerEnv; rEnv : Environment }

let private eval ce env syntax = compile ce syntax env

///Evaluates the given Syntax
let Evaluate { cEnv=cEnv; rEnv=env } = eval cEnv env

///Evaluates the given Syntax in the given Environment
let EvaluateInEnvironment = eval

///Parses and evaluates an expression given in text form, and returns the resulting expression
let ParseText e = List.ofSeq >> parse >> Begin >> Evaluate e

let CreateEnvironments() =
    let evaluateSchemeDefs e =
        "
        ;; Y Combinator
        (define (Y f) ((λ (x) (x x)) (λ (x) (f (λ (g) ((x x) g))))))
        " |> ParseText e |> ignore

    ///Our base compiler environment
    let compileEnvironment : CompilerEnv = ref [[]]
        
    ///Our base environment
    let environment : Environment = ref [ref [||]]

    let tempEnv = ref []

    ///Adds a new binding to the default environment
    let AddDefaultBinding name expr = 
        let eRef = ref expr
        tempEnv := (name, eRef) :: !tempEnv

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
    AddDefaultBinding "build-list" (Function(BuildSeq))
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
    //AddDefaultBinding "eval" (Function(Eval))
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
    AddDefaultBinding "flatten" (Function(Flatten))
    AddDefaultBinding "sqrt" (Function(Sqrt))
    AddDefaultBinding "transpose" (Function(Transpose))
    AddDefaultBinding "andmap" (Function(AndMap))
    AddDefaultBinding "ormap" (Function(OrMap))

    environment := [Seq.map (fun (_, x) -> x) !tempEnv |> Seq.toArray |> ref]
    compileEnvironment := [List.map (fun (x, _) -> x) !tempEnv]

    let env = { rEnv=environment; cEnv=compileEnvironment }
    evaluateSchemeDefs env
    env

///Read/Eval/Print
let REP e = ParseText e >> print

///Debug version of rep
let private REPd e text =
    let watch = Stopwatch.StartNew()
    let x = ParseText e text
    watch.Stop()
    sprintf "%s\nTime Elapsed: %d ms" (print x) watch.ElapsedMilliseconds

///Read/Eval/Print Loop
let REPL debug : unit =
    let env = CreateEnvironments()
    let runner = if debug then REPd else REP
    let rec repl' output =
        printf "%s\n> " output
        try Console.ReadLine() |> runner env |> repl'
        with ex -> repl' ex.Message
    repl' ""

type ErrorLog = delegate of string -> unit


///Tests
let RunTests (log : ErrorLog) =
    let success = ref true
    let rep ce env = List.ofSeq >> parse >> Begin >> eval ce env >> print
    let env = CreateEnvironments()
    let case source expected =
        try
            let testEnv : Environment =
                List.map (fun (x : Frame) ->
                            Array.map (fun (y : Value ref)
                                            -> ref y.Value)
                                      x.Value
                            |> ref)
                         env.rEnv.Value
                |> ref
            let testCEnv = env.cEnv.Value |> ref
            let output = rep testCEnv testEnv source
            if output <> expected then
                success := false
                sprintf "TEST FAILED: %s [Expected: %s, Actual: %s]" source expected output |> log.Invoke
        with ex ->
            success := false
            sprintf "TEST CRASHED: %s [%s]" ex.Message source |> log.Invoke

    //Engine Tests
    case "(quote (* 2 3))" "(* 2 3)" // quote primitive
    //case "(eval '(* 2 3))" "6" // eval quoted expression
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

    case "(let ((a (begin)) (b (begin)) (c (begin)) (d 0)) (begin (begin (begin (begin (begin (begin (set! a d)) (set! d (+ d 1))) (set! b d)) (set! d (+ d 1))) (set! c d)) (list a b c)))" "(0 1 2)"

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
    case "(build-list 0 10 1)" "(0 1 2 3 4 5 6 7 8 9 10)" // build-list
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
    case "(map list '(1 2 3) '(4 5 6) '(7 8) '(9 10 11 12))" "((1 4 7 9) (2 5 8 10) (3 6 11) (12))"

    //Transpose
    case "(transpose '((1 2 3) (4 5 6) (7 8) (9 10 11 12)))" "((1 4 7 9) (2 5 8 10) (3 6 11) (12))"

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

    //Flatten
    case "(flatten '((1 2) (3 4) (5 6)))" "(1 2 3 4 5 6)"

    success.Value

let RunTestsInConsole() =
    if RunTests <| ErrorLog(Console.WriteLine) then
        Console.WriteLine("All Tests Passed.")
