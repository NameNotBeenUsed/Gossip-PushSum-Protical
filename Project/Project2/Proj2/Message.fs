namespace ProjectTwo

open System
open Akka.Actor
open Akka.FSharp

[<AutoOpen>]
module Message = 
    
    type PushSumMsg = {Sum:double; Weight:double}

    type MyMessage = {NumActors:int; algoTopoCode:string; numStartNodes:int; msgcount:int}

    
