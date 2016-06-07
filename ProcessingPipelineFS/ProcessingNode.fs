namespace ProcessingPipeline

open System

module ProcessingNode = 
    let epoch = DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)

    let currentTime () = DateTime.UtcNow

    let unixDateTime () = 
        (currentTime () - epoch).TotalMilliseconds / 1000.0
                  
    type Logger() =
        static let agent = MailboxProcessor.Start(fun inbox ->
            let rec messageLoop () = async {
                let! msg = inbox.Receive()
                printfn "[%14.4f] %s" (unixDateTime () ) msg
                return! messageLoop() 
                }
            messageLoop()
            )

        static member Log msg = agent.Post msg
        
    type ProcessingTask = Option<string>

    type Message = 
        | Shutdown
        | HereIsTaskForYou of ProcessingTask
        | AreYouReadyToWork
        | ReadyToWork
        | StartWorking
        | FinishWorking
        
    type Node(name, delay, ?nextNode:Node) =
        let mutable currentTask: ProcessingTask = None
        let mutable busy = false        
        let mutable workerAgent: Option<MailboxProcessor<Message>> = None
        let mailboxAgent = 
            let logMessageHead = "mailbox {" + name + "}:\t"
            let log msg = Logger.Log (logMessageHead + msg)
            MailboxProcessor.Start(fun inbox ->
                let rec messageLoop() = async {
                    let! msg = inbox.Receive()
                    match msg with
                    | Shutdown -> 
                        log "shutdown"
                        return ()
                    | HereIsTaskForYou task ->
                        currentTask <- task
                        match workerAgent with
                        | Some actor -> 
                            match currentTask with
                            | Some t ->
                                match busy with
                                | true -> log ">> HereIsTaskForYou: worker is busy, dropping message"
                                | false -> 
                                    log (">> HereIsTaskForYou: send task to worker: " + t)
                                    actor.Post (HereIsTaskForYou (Some t))
                                    // the current task is processed, drop it
                                    currentTask <- None
                            | None -> 
                                log ">> HereIsTaskForYou: task is not set (yet)"
                        | None ->
                            log ">> HereIsTaskForYou: worker is not set"
                    | ReadyToWork ->
                        match workerAgent with 
                        | Some actor -> 
                            match currentTask with
                            | Some t ->
                                log (">> ReadyToWork: end task to worker: " + t)
                                actor.Post (HereIsTaskForYou (Some t))
                                // the current task is processed, drop it
                                currentTask <- None
                            | None ->
                                log ">> ReadyToWork: task is not set (yet)"
                        | None ->
                            log ">> ReadyToWork: worker is not set"
                    | _ -> failwith ">> messageLoop: unrecognized message"

                    return! messageLoop()
                    }
                messageLoop()
                )

        let workerAgent_ =
            let logMessageHead = "worker  (" + name + "):\t"
            let log msg = Logger.Log (logMessageHead + msg)
            MailboxProcessor.Start(fun inbox ->
                let rec messageLoop() = async {                    
                    let! msg = inbox.Receive()
                    match msg with
                    | Shutdown -> 
                        log "shutdown"
                        return()
                    | HereIsTaskForYou task ->
                        match task with
                        | Some t ->
                            busy <- true
                            log("=> starts working on task {" + t.ToString() + "}")
                            // Imitate work here, just sleep shortly
                            do! Async.Sleep delay
                            log("<= finished working on task {" + t.ToString() + "}")
                            match nextNode with
                            | Some actor -> actor.Post (HereIsTaskForYou (Some t))
                            | _ ->  log(">> HereIsTaskForYou: next actor is not set")
                            busy <- false
                            mailboxAgent.Post ReadyToWork
                        | None ->
                            log("<> HereIsTaskForYou: received empty task")
                            mailboxAgent.Post ReadyToWork
                    | _ -> ()

                    return! messageLoop()
                    }
                messageLoop()
            )

        member this.Init () =
            workerAgent <- Some workerAgent_
        member this.Post message = mailboxAgent.Post message
        member this.Shutdown =
            mailboxAgent.Post Shutdown
            match workerAgent with
            | Some a -> a.Post Shutdown
            | _ -> ()


                        
                    


