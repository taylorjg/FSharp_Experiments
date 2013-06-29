open System.Text.RegularExpressions

type Token =
    | ID of string
    | INT of int
    | HAT
    | PLUS
    | MINUS

let tokenR = new Regex(@"((?<token>(\d+|\w+|\^|\+|-))\s*)*")

let tokenize (s: string) =
    [for x in tokenR.Match(s).Groups.["token"].Captures do
        let token =
            match x.Value with
            | "^" -> HAT
            | "-" -> MINUS
            | "+" -> PLUS
            | s when System.Char.IsDigit s.[0] -> INT (int s)
            | s -> ID s
        yield token]

type Term =
    | Term of int * string * int
    | Const of int

type Polynomial = Term list
type TokenStream = Token list

let tryToken (src: TokenStream) =
    match src with
    | tok :: rest -> Some(tok, rest)
    | _ -> None

let parseExponent src =
    match tryToken src with
    | Some (HAT, src) ->
        match tryToken src with
        | Some (INT num, src) ->
            num, src
        | _ -> failwith "expected an integer after '^'"
    | _ -> 1, src

let parseTerm src =
    match tryToken src with
    | Some (INT num, src) ->
        match tryToken src with
        | Some (ID id, src) ->
            let exp, src = parseExponent src
            Term (num, id, exp), src
        | _ -> Const num, src
    | Some (ID id, src) ->
        let exp, src = parseExponent src
        Term (1, id, exp), src
    | _ -> failwith "end of token stream in term"

let rec parsePolynomial src =
    let t1, src = parseTerm src
    match tryToken src with
    | Some (PLUS, src) ->
        let p2, src = parsePolynomial src
        (t1 :: p2), src
    | Some (MINUS, src) ->
        let p2, src = parsePolynomial src
        (t1 :: p2), src
    | _ -> [t1], src

let parse input =
    let src = tokenize input
    let result, src = parsePolynomial src
    match tryToken src with
    | Some _ -> failwith "unexpected input at the end of the stream"
    | None -> result

printfn "tokenize x^5 + 2x^3 + 20: %A" (tokenize "x^5 + 2x^3 + 20")
printfn "tokenize x^5 - 2x^3 + 20: %A" (tokenize "x^5 - 2x^3 + 20")
printfn "parse x^5 + 2x^3 + 20: %A" (parse "x^5 + 2x^3 + 20")
printfn "parse x^5 - 2x^3 + 20: %A" (parse "x^5 - 2x^3 + 20")
