namespace ProjectTwo

open System
open Akka.Actor
open Akka.FSharp

module Boss = 
    
    type BossActor(numOfWorkers:int, algoTopoCode:string, system:ActorSystem) = 
        inherit Actor()

        //let mutable StartTime = System.DateTime.Now.TimeOfDay.Milliseconds
        //let mutable terminatedWorkers = []
        //let mutable jobCompletionRation:double = 0.0
        //let mutable totalExchangedMsgs = 0
        //let mutable checkPointMsgs = 0

        let mutable completedWorkersCount: int = 0
        let mutable minPropagationLimit: double = 0.0

        let changeValue() = 
            match algoTopoCode with
            | "GOSSIP-FULL" -> minPropagationLimit <- 0.9
            | "GOSSIP-LINE" -> minPropagationLimit <- 0.9
            | "GOSSIP-2D" -> minPropagationLimit <- 0.9
            | "GOSSIP-IMP2D" -> minPropagationLimit <- 0.9    
            | "PUSH-SUM-FULL" -> minPropagationLimit <- 0.7    
            | "PUSH-SUM-2D" -> minPropagationLimit <- 0.7    
            | "PUSH-SUM-LINE" -> minPropagationLimit <- 0.7   
            | "PUSH-SUM-IMP2D" -> minPropagationLimit <- 0.7
            | _ -> failwith "unknown message"
        
        do changeValue()
        do printfn " ========== STARTING THE MASTER NODE =============== "

        static let mutable terminatedWorkers:list<int> = []
        static let mutable startTime = System.Diagnostics.Stopwatch.StartNew()
        static let mutable jobCompletionRation:double = 0.0
        static let mutable totalExchangedMsgs:double = 0.0
        static let mutable checkPointMsgs:double = 0.0
        
        static member TerminatedWorkers
            with get() = terminatedWorkers
            and set(list) = terminatedWorkers <- list
   
        static member StartTime
            with get() = startTime
            and set(num) = startTime <- num

        static member JobCompletionRation
            with get() = jobCompletionRation
            and set(num) = jobCompletionRation <- num

        static member TotalExchangedMsgs
            with get() = totalExchangedMsgs
            and set(num) = totalExchangedMsgs <- num

        static member CheckPointMsgs
            with get() = checkPointMsgs
            and set(num) = checkPointMsgs <- num

        //Duration

        override x.OnReceive message = 
            match box message with
            | :? int as msg ->
                completedWorkersCount <- completedWorkersCount + 1
                let newJobCompletionratio = double(completedWorkersCount) / double(numOfWorkers)
                let jobCompletionRatioDiff = newJobCompletionratio - BossActor.JobCompletionRation
                let tempList = List.append BossActor.TerminatedWorkers [msg]
                BossActor.TerminatedWorkers <- tempList

                if newJobCompletionratio >= minPropagationLimit then
                    startTime.Stop()
                    //printfn "Taken time: %d" (System.DateTime.Now.TimeOfDay.Milliseconds - BossActor.StartTime)
                    printfn "Taken time: %f" (startTime.Elapsed.TotalMilliseconds)
                    system.Terminate() |> ignore

                if jobCompletionRatioDiff >= 0.1 then
                    BossActor.JobCompletionRation <- newJobCompletionratio
                    printfn " ========== GOSSIP PROPAGATION %A ==========" (newJobCompletionratio * 100.0)
            | :? string as msg ->
                printfn "%s" msg
            | _ -> failwith "unknown message"
