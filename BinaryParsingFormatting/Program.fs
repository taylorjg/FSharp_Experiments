// This is essentially code from Chapter 8 of "Expert F# 3.0", ISBN: 9781430246503, Authors: Don Syme, Adam Granicz, Antonio Cisternino

module Program

type OutState = System.IO.BinaryWriter
type InState = System.IO.BinaryReader

type Picker<'T> = 'T -> OutState -> unit
type Unpickler<'T> = InState -> 'T

let byteP (b: byte) (st: OutState) = st.Write(b)
let byteU (st: InState) = st.ReadByte()

let boolP b st = byteP (if b then 1uy else 0uy) st
let boolU st = let b = byteU st in (b = 1uy)

let int32P i st =
    byteP (byte (i &&& 0xFF)) st
    byteP (byte ((i >>> 8) &&& 0xFF)) st
    byteP (byte ((i >>> 16) &&& 0xFF)) st
    byteP (byte ((i >>> 24) &&& 0xFF)) st

let int32U st =
    let b0 = int (byteU st)
    let b1 = int (byteU st)
    let b2 = int (byteU st)
    let b3 = int (byteU st)
    b0 ||| (b1 <<< 8) ||| (b2 <<< 16) ||| (b3 <<< 24)

let tup2P p1 p2 (a, b) st =
    (p1 a st: unit)
    (p2 b st: unit)

let tup3P p1 p2 p3 (a, b, c) st =
    (p1 a st: unit)
    (p2 b st: unit)
    (p3 c st: unit)

let tup2U u1 u2 st =
    let a = u1 st
    let b = u2 st
    (a, b)

let tup3U u1 u2 u3 st =
    let a = u1 st
    let b = u2 st
    let c = u3 st
    (a, b, c)

let rec listP p lst st =
    match lst with
    | [] -> byteP 0uy st
    | h :: t -> byteP 1uy st; p h st; listP p t st

let listU u st =
    let rec loop acc =
        let tag = byteU st
        match tag with
        | 0uy -> List.rev acc
        | 1uy -> let a = u st in loop (a :: acc)
        | n -> failwithf "listU: found number %d" n
    loop []
