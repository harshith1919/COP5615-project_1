open System
open System.Net
open System.Net.Sockets
open System.IO

let port = 12345

let handleClient (clientSocket: TcpClient) =
    try
        let stream = clientSocket.GetStream()
        let reader = new StreamReader(stream)
        let writer = new StreamWriter(stream)
        writer.AutoFlush <- true

        printfn "Client connected: %s" (clientSocket.Client.RemoteEndPoint.ToString())
        writer.WriteLine("Hello")
        let mutable continueReading = true

        while continueReading do
            try
                let request = reader.ReadLine()
                match request with
                | null -> continueReading <- false // Connection closed by the client
                | "bye" ->
                    writer.WriteLine("-5")
                    printfn "Client requested termination."
                    continueReading <- false
                | "terminate" ->
                    writer.WriteLine("-5")
                    printfn "Client requested termination. Server is shutting down..."
                    continueReading <- false
                | request ->
                    let parts = request.Split(' ')
                    if parts.Length >= 3 && parts.Length<=5 then
                        let operation = parts.[0]
                        let operands = Array.map Int32.Parse (Array.skip 1 parts)
                        
                        let result =
                            match operation with
                            | "add" -> Array.sum operands
                            | "subtract" -> Array.reduce (-) operands
                            | "multiply" -> Array.reduce (*) operands
                            | _ -> Int32.MinValue // Invalid operation

                        writer.WriteLine(sprintf "%d" result)
                        printfn "Received: %s. Responded: %d" request result
                    else
                        writer.WriteLine("Invalid request format. Please use: operation operand1 operand2")
            with
            | ex -> 
                printfn "Error: %s" ex.Message
                continueReading <- false
    with
    | ex -> printfn "Error: %s" ex.Message

    clientSocket.Close()

let startServer () =
    let ipAddress = IPAddress.Parse("127.0.0.1")
    let listener = TcpListener(ipAddress, port)
    listener.Start()

    printfn "Server is listening on port %d..." port

    while true do
        let client = listener.AcceptTcpClient()
        System.Threading.Tasks.Task.Run(fun () -> handleClient client) |> ignore

[<EntryPoint>]
let main argv =
    startServer()
    Console.ReadLine() |> ignore // Keep the server running
    0 // Return an integer exit code
