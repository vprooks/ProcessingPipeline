open System
open System.Threading
open ProcessingPipeline.ProcessingNode

[<EntryPoint>]
let main argv = 
    // Create nodes
    let outputNode = Node("02", 100)    
    let internalNode = Node("01", 500, outputNode)
    let inputNode = Node("00", 200, internalNode)    
    // Initialize all the nodes
    [inputNode; internalNode; outputNode] |> List.map (fun x -> x.Init()) |> ignore
    let rnd = System.Random()
    Logger.Log("Program starts")
    for i in 0..5 do
        let task: Task = if rnd.NextDouble() > 0.1 then Some("Task id:<" + i.ToString() + ">") else None
        match task with
        | Some t -> Logger.Log("Sending message = " + t)
        | None -> Logger.Log("Input message is corrupted")
        inputNode.Post (HereIsTaskForYou task)
        Thread.Sleep 300

    // Wait for all tasks to finish
    Thread.Sleep 2000
    Logger.Log("Program finished. Press ENTER to quit ... ")
    Console.ReadLine() |> ignore
    0 // return an integer exit code
