module App.Rop

type Result<'TSuccess, 'TMessage> = 
    | Success of 'TSuccess * 'TMessage list
    | Failure of 'TMessage list

let succeed x = Success(x, [])

let either fSuccess fFailure = 
    function 
    | Success(x, msgs) -> fSuccess (x, msgs)
    | Failure errors -> fFailure errors

let bind f = 
    let mergeMessages msgs = 
        let fSuccess (x, msgs2) = Success(x, msgs @ msgs2)
        let fFailure errs = Failure(errs @ msgs)
        either fSuccess fFailure
    
    let fSuccess (x, msgs) = f x |> mergeMessages msgs
    let fFailure errs = Failure errs
    either fSuccess fFailure

let apply f result = 
    match f, result with
    | Success(f, msgs1), Success(x, msgs2) -> (f x, msgs1 @ msgs2) |> Success
    | Failure errs, Success(_, msgs) | Success(_, msgs), Failure errs -> errs @ msgs |> Failure
    | Failure errs1, Failure errs2 -> errs1 @ errs2 |> Failure

let lift f result = 
    let f' = f |> succeed
    apply f' result

let map = lift

let valueOrDefault f = 
    function 
    | Success(x, _) -> x
    | Failure errors -> f errors
