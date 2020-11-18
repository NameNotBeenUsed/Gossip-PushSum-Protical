open System
open Akka.FSharp
open Akka.Actor
open ProjectTwo


let getAlgoGossipCode(topology : String, algo: String): String =
    
    
    if ((algo.Equals("gossip")) && (topology.Equals("full"))) then  "GOSSIP-FULL"
    elif ((algo.Equals("gossip")) && (topology.Equals("2D"))) then  "GOSSIP-2D" 
    elif ((algo.Equals("gossip")) && (topology.Equals("imp2D"))) then  "GOSSIP-IMP2D"
    elif ((algo.Equals("gossip")) && (topology.Equals("line"))) then "GOSSIP-LINE"
    elif ((algo.Equals("pushsum")) && (topology.Equals("full"))) then  "PUSH-SUM-FULL"
    elif ((algo.Equals("pushsum")) && (topology.Equals("2D"))) then "PUSH-SUM-2D"
    elif ((algo.Equals("pushsum")) && (topology.Equals("imp2D"))) then "PUSH-SUM-IMP2D"
    elif ((algo.Equals("pushsum")) && (topology.Equals("line"))) then "PUSH-SUM-LINE"
    else "unknown message"


    //for our ease, we are rounding off the number of actors to a perfect square for matric topologies
let getRevisedActorsNum(topology: String, actualNumActor: int) :int = 
    let mutable revisedNumActors = actualNumActor
    
    if (topology.Equals("2D")||(topology.Equals("imp2D"))) then
        let mutable sqRootValue = int(Math.Sqrt(float(revisedNumActors)))
        while (sqRootValue * sqRootValue <> revisedNumActors) do
            revisedNumActors <- revisedNumActors+1
            sqRootValue <- int(Math.Sqrt(float(revisedNumActors)))
    revisedNumActors

[<EntryPoint>]
let main argv =
    let system = ActorSystem.Create("Gossip")
    let line = Console.ReadLine()
    let para = line.Split ' '
    let numActor = para.[0] |> int
    //full 2D imp2D line
    let topology = para.[1]
    //gossip pushsum
    let algo = para.[2]
    let numStartNodes = para.[3] |> int
    let msgcount = 10
    //let algoTopoCode = getAlgoGossipCode(topology,algo)
    let reviseNumActors = getRevisedActorsNum(topology,numActor)
    let algoTopoCode = getAlgoGossipCode(topology,algo)
    //BossActor(numOfWorkers:int, algoTopoCode:string, system:ActorSystem)
    let name = "boss"
    let args = [| box reviseNumActors; box algoTopoCode; box system |]
    let boss = system.ActorOf(Props.Create(typeof<Boss.BossActor>, args), name)
    boss <! "start" 
    //Topology(numOfNodes:int, topology:string, algorithm:string, numStartNodes:int, msgCount:int, system:ActorSystem, boss:IActorRef)
    let topo = new Topology.Topology(reviseNumActors, topology, algo, numStartNodes, msgcount, system, boss)
    

    system.WhenTerminated.Wait()
    0 
