namespace ProjectTwo

open System
open Akka.Actor
open Akka.FSharp

module Worker =
    
    //type GossipRumorMsg = string
    //type PushSumMsg = {Sum:double; Weight:double}

    //For each actor, sum(i)=i, weight(i)=1
    //Gossip Algorithm: stop when it has heard the rumor 10 times.
    //Push-Sum Algorithm: s/w didn't change more than 10^-10 3 consecutive rounds.
    type Worker(numOfNodes:int, msgCount:int, adjacentNodes:list<int>, workers:array<IActorRef>, initSum:int, system:ActorSystem, bossActor:IActorRef) =
        inherit Actor()
        
        //let bossActor = system.ActorSelection(bossPath) 
        //let boss = select "" system
        //the number of rumors received from other workers
        let containsNumber number list = List.exists (fun elem -> elem = number) list

        let rand = Random()
        let mutable count:int = 0
        let mutable sentMaxReport = false
        let mutable actorSum:double = double(initSum)
        let mutable actorWeight:double = 1.0
        let DIVERGENCE:double = 2.0
        let PUSH_SUM_TERMINATION_VALUE:double = Math.Pow(10.0, -10.0)
        let mutable pushSumCounter = 0
        let PUSH_SUM_MAX_RUMOURS = 3

        let sendMsg(sum:double, weight:double) = 
            let numOfAdjacentNodes = adjacentNodes.Length
            let mutable send:bool = false
            let mutable i:int = 0
            while(not send && i<numOfAdjacentNodes) do
                //let rand = Random()
                let randNode = rand.Next(numOfAdjacentNodes)
                //if the worker is not terminated, use boss to check whether it is true or not
                let tempList = Boss.BossActor.TerminatedWorkers
                if not(containsNumber (adjacentNodes.Item(randNode)) tempList) then
                    let myMessage = {PushSumMsg.Sum = sum; PushSumMsg.Weight = weight}
                    workers.[adjacentNodes.Item(randNode)] <! myMessage
                    send <- true
                i <- i + 1

        override x.OnReceive message =
            match box message with
            | :? PushSumMsg as msg ->
                //TODO
                count <- count + 1
                Boss.BossActor.TotalExchangedMsgs <- Boss.BossActor.TotalExchangedMsgs + 1.0
                let oldSByW = actorSum / actorWeight
                
                actorSum <- actorSum + msg.Sum
                actorWeight <- actorWeight + msg.Weight

                actorSum <- actorSum / DIVERGENCE
                actorWeight <- actorWeight / DIVERGENCE

                let newSByW = actorSum / actorWeight

                let adjecentWorkersLength = adjacentNodes.Length
        
                if sentMaxReport = false then
                    if (count = 1 || Math.Abs(newSByW - oldSByW) > PUSH_SUM_TERMINATION_VALUE) then
                        pushSumCounter <- 0
                        sendMsg(actorSum, actorWeight)
                    elif pushSumCounter < PUSH_SUM_MAX_RUMOURS then
                        pushSumCounter <- pushSumCounter + 1
                        for i = 0 to 5 do
                            sendMsg(actorSum, actorWeight)
                    elif (pushSumCounter = PUSH_SUM_MAX_RUMOURS && sentMaxReport = false) then
                        
                        bossActor <! initSum
                        sentMaxReport <- true
                        for i = 0 to 5 do
                            sendMsg(actorSum, actorWeight)
                    
            | :? string as msg ->  // "rumour"
                //TODO
                //bossActor.totalExchangesMssges += 1
                if count < msgCount then
                    count <- count + 1
                    let adjacentWorkersLength = adjacentNodes.Length
                    //let rand = Random()
                    for i = 0 to 5 do //send to 5 other adjecent nodes
                        let randIndex = rand.Next(adjacentWorkersLength)
                        workers.[adjacentNodes.Item(randIndex)] <! "rumour"
                elif sentMaxReport = false then
                   
                    bossActor <! initSum
                    sentMaxReport <- true
            | _ -> failwith "unknown message"