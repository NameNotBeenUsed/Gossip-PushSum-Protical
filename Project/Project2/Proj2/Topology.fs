namespace ProjectTwo

open System
open Akka.Actor
open Akka.FSharp

module Topology =

    //let rand = Random()
    //let ifContain number array = Array.exists(fun elem -> elem = number) array

    type Topology(numOfNodes:int, topology:string, algorithm:string, numStartNodes:int, msgCount:int, system:ActorSystem, boss:IActorRef) = //bossPath:ActorPath

        let workers = Array.zeroCreate numOfNodes
        //let workers:IActorRef[] = Array.create numOfNodes IActorRef
        //let mutable workers = Array.init
        //let mutable workers = []
        let rand = Random()
        let ifContainA number array = Array.exists(fun elem -> elem = number) array
        let ifContainL number array = List.exists(fun elem -> elem = number) array

        let genRandomStartNodes(numStartNodes:int) = 
            let randStartIndex = Array.create numStartNodes 0
            //let ifContain number array = Array.exists(fun elem -> elem = number ) array
            for i = 0 to numStartNodes - 1 do
               //let rand = Random()
               let randIndex = rand.Next(numOfNodes)
               if not (ifContainA randIndex randStartIndex) 
               then Array.set randStartIndex i randIndex// randIndexesToStart.append(randIndex)
            printfn "========== RANDOM NODES SELECTED ======= %A" randStartIndex
            randStartIndex
        
        let spreadRumour(algorithm:string, numStartNodes:int, workers:array<IActorRef>) = 
            
            let randStartIndex = genRandomStartNodes(numStartNodes)
            
            
            Boss.BossActor.StartTime <- System.Diagnostics.Stopwatch.StartNew()
            
            if algorithm.Equals("gossip") then 
                for i in randStartIndex do
                    workers.[i] <! "rumour"
            else  //algorithm.Equals("pushsum")
                for i in randStartIndex do 
                    //let msg = {PushSumMsg.Sum = 0.0; PushSumMsg.Weight = 1.0}
                    let msg = {PushSumMsg.Sum = double(i); PushSumMsg.Weight = 1.0}
                    workers.[i] <! msg

        let gossipLine(algorithm:string) = 
            
            printfn " ==========  GOSSIP LINE  ================ %d" numOfNodes

            for i = 0 to (numOfNodes - 1) do
                let mutable adjacentWorkers:list<int> = []
                
                if i - 1 >= 0 then
                    adjacentWorkers <- List.append adjacentWorkers [i-1]
                if i + 1 < numOfNodes then
                    adjacentWorkers <- List.append adjacentWorkers [i+1]

                
                let name = "worker" + string(i)
                //Worker(numOfNodes:int, msgCount:int, adjacentNodes:array<int>, workers:array<IActorRef>, initSum:int, system:ActorSystem, boss:IActorRef)
                let args = [| box numOfNodes; box msgCount; box adjacentWorkers; box workers; box i; box system; box boss |]
                let temp = system.ActorOf(Props.Create(typeof<Worker.Worker>, args), name)
                workers.[i] <- temp

            spreadRumour(algorithm, numStartNodes, workers)

        let gossip2D(algorithm:string) = 
           
            printfn " ==========  GOSSIP 2D  ================ %d" numOfNodes

            let workersPerRow:int = int(Math.Sqrt(float(numOfNodes)))
           
            for rowIndex = 0 to (workersPerRow - 1) do
                for columnIndex = 0 to (workersPerRow - 1) do
                    let mutable adjacentWorkers:list<int> = []
                    let currentWorkerPosition = rowIndex * workersPerRow + columnIndex
                    let leftColumnIndex = columnIndex - 1
                    let rightColumnIndex = columnIndex + 1
                    let topRowIndex = rowIndex - 1
                    let bottomRowIndex = rowIndex + 1

                    if leftColumnIndex >= 0 then
                        let leftWorkerPosition = rowIndex * workersPerRow + leftColumnIndex
                        adjacentWorkers <- List.append adjacentWorkers [leftWorkerPosition]

                    if rightColumnIndex < workersPerRow then
                        let rightWorkerPosition = rowIndex * workersPerRow + rightColumnIndex
                        //Array.set adjacentWorkers 1 rightWorkerPosition
                        adjacentWorkers <- List.append adjacentWorkers [rightWorkerPosition]

                    if topRowIndex >= 0 then
                        let topWorkerPosition = topRowIndex * workersPerRow + columnIndex
                        //Array.set adjacentWorkers 2 topWorkerPosition
                        adjacentWorkers <- List.append adjacentWorkers [topWorkerPosition]

                    if bottomRowIndex < workersPerRow then
                        let bottomWorkerPosition = bottomRowIndex * workersPerRow + columnIndex
                        //Array.set adjacentWorkers 3 bottomWorkerPosition
                        adjacentWorkers <- List.append adjacentWorkers [bottomWorkerPosition]

                    //workers.[currentWorkerPosition]
                    let name = "worker" + string(currentWorkerPosition)
                    //Worker(numOfNodes:int, msgCount:int, adjacentNodes:array<int>, workers:array<IActorRef>, initSum:int, system:ActorSystem, boss:IActorRef)
                    let args = [| box numOfNodes; box msgCount; box adjacentWorkers; box workers; box currentWorkerPosition; box system; box boss |]
                    let temp = system.ActorOf(Props.Create(typeof<Worker.Worker>, args), name)
                    workers.[currentWorkerPosition] <- temp
            
            spreadRumour(algorithm, numStartNodes, workers)

        let gossip2DImp(algorithm:string) = 
            
            printfn(" ========== GOSSIP 2D IMP ================ %d") numOfNodes
            
            let workersPerRow = int(Math.Sqrt(float(numOfNodes)))

            for rowIndex = 0 to (workersPerRow - 1) do
                for columnIndex = 0 to (workersPerRow - 1) do
                    let mutable adjacentWorkers:list<int> = []

                    let currentWorkerPosition = rowIndex * workersPerRow + columnIndex
                    let leftColumnIndex = columnIndex - 1
                    let rightColumnIndex = columnIndex + 1
                    let topRowIndex = rowIndex - 1
                    let bottomRowIndex = rowIndex + 1

                    if leftColumnIndex >= 0 then
                        let leftWorkerPosition = rowIndex * workersPerRow + leftColumnIndex
                        adjacentWorkers <- List.append adjacentWorkers [leftWorkerPosition]

                    if rightColumnIndex < workersPerRow then
                        let rightWorkerPosition = rowIndex * workersPerRow + rightColumnIndex
                        //Array.set adjacentWorkers 1 rightWorkerPosition
                        adjacentWorkers <- List.append adjacentWorkers [rightWorkerPosition]

                    if topRowIndex >= 0 then
                        let topWorkerPosition = topRowIndex * workersPerRow + columnIndex
                        //Array.set adjacentWorkers 2 topWorkerPosition
                        adjacentWorkers <- List.append adjacentWorkers [topWorkerPosition]

                    if bottomRowIndex < workersPerRow then
                        let bottomWorkerPosition = bottomRowIndex * workersPerRow + columnIndex
                        //Array.set adjacentWorkers 3 bottomWorkerPosition
                        adjacentWorkers <- List.append adjacentWorkers [bottomWorkerPosition]

                    let mutable randomIndex = rand.Next(numOfNodes)
                    
                    while (ifContainL randomIndex adjacentWorkers) do 
                        randomIndex <- rand.Next(numOfNodes)

                    //Array.set adjacentWorkers 4 randomIndex
                    adjacentWorkers <- List.append adjacentWorkers [randomIndex]

                    //创建workers.[currentWorkerPosition]
                    let name = "worker" + string(currentWorkerPosition)
                    //Worker(numOfNodes:int, msgCount:int, adjacentNodes:array<int>, workers:array<IActorRef>, initSum:int, system:ActorSystem, boss:IActorRef)
                    let args = [| box numOfNodes; box msgCount; box adjacentWorkers; box workers; box currentWorkerPosition; box system; box boss |]
                    let temp = system.ActorOf(Props.Create(typeof<Worker.Worker>, args), name)
                    workers.[currentWorkerPosition] <- temp

            spreadRumour(algorithm, numStartNodes, workers)

        let gossipFull(algorithm:string) = 
            
            printfn " ========== GOSSIP FULL ================ %d" numOfNodes

            let temp = [0 .. (numOfNodes - 1)]
            for i = 0 to (numOfNodes - 1) do
                let adjacentWorkers = List.filter (fun element -> element <> i) temp
                //创建workers.[i]
                let name = "worker" + string(i)
                //Worker(numOfNodes:int, msgCount:int, adjacentNodes:array<int>, workers:array<IActorRef>, initSum:int, system:ActorSystem, boss:IActorRef)
                let args = [| box numOfNodes; box msgCount; box adjacentWorkers; box workers; box i; box system; box boss |]
                let temp = system.ActorOf(Props.Create(typeof<Worker.Worker>, args), name)
                workers.[i] <- temp
            spreadRumour(algorithm, numStartNodes, workers)

        //algorithm:gossip pushsum
        //topology:full 2D imp2D line
        let findMethod() = 
            match (algorithm, topology) with
            | ("gossip", "full") -> gossipFull(algorithm)
            | ("gossip", "line") -> gossipLine(algorithm)
            | ("gossip", "2D") -> gossip2D(algorithm)
            | ("gossip", "imp2D") -> gossip2DImp(algorithm)
            | ("pushsum", "full") -> gossipFull(algorithm)
            | ("pushsum", "line") -> gossipLine(algorithm)
            | ("pushsum", "2D") -> gossip2D(algorithm)
            | ("pushsum", "imp2D") -> gossip2DImp(algorithm)
            | _ -> failwith "unknown message"

        do findMethod()



                





        



