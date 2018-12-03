namespace FsSlackLib

module Listener =

    open System.Net.WebSockets
    open System.Threading
    open System.Text
    open System

    let listener websocket (callback: string -> unit) =
        let receiveBuffer = Array.create 8192 0uy
        let receiveSegment = new ArraySegment<byte>(receiveBuffer)
        let token = Async.CancellationToken |> Async.RunSynchronously
        let rec loop (websocket: WebSocket) (receiveSegment: ArraySegment<byte>) (token: CancellationToken) =
            async {
                let receiveString (websocket: WebSocket) (receiveSegment: ArraySegment<byte>) (token: CancellationToken) : Async<string> =
                    let rec receiveImpl (buffer : ResizeArray<byte>) : Async<ResizeArray<byte>> =
                        async {
                            let! result = websocket.ReceiveAsync(receiveSegment, token) |> Async.AwaitTask
                            let trimmed = receiveSegment.Array |> Array.take result.Count
                            buffer.AddRange trimmed
                            return! match result.EndOfMessage with
                                    | true -> buffer |> async.Return
                                    | false -> receiveImpl buffer
                        }
                    async {
                        let! message = receiveImpl (new ResizeArray<byte>())
                        let encoding = new UTF8Encoding()
                        return message.ToArray() |> encoding.GetString
                    }
                let! response = receiveString websocket receiveSegment token
                do callback response
                do! loop websocket receiveSegment token
            }
        loop websocket receiveSegment token

    let startListening (websocket: WebSocket) (callback: string -> unit) =
        async {
            while true do
                do! listener websocket callback
        }
