namespace FsSlackLib

module SlackClient =

    open System
    open System.Net
    open System.Text
    open System.IO
    open Domain
    open FSharp.Data
    open FSharp.Data.JsonExtensions
    open Newtonsoft.Json
    open Newtonsoft.Json.Linq
    open Ninja.WebSockets
    open System.Drawing
    open System.Net.WebSockets
    open System.Threading
    open System.Threading.Tasks
    open EventArgs

    let jsonToObject<'a> msg =
        msg |> JObject.Parse 
        |> fun obj -> obj.SelectToken "$" 
        |> fun json -> json.ToObject<'a>()

    type SlackClient (apiKey) =

        let urlRtmStart = "https://slack.com/api/rtm.start?token=" + apiKey;

        let hello = new Event<_>()
        let goodbye = new Event<_>()
        let onUserTyping = new Event<_>()
        let onMessage = new Event<_>()
        let onMessageEdit = new Event<_>()
        let processCommand = new Event<_>()

        let token = new CancellationTokenSource()

        let websocket = 
            let httpWebRequest = WebRequest.Create(new Uri(urlRtmStart)) :?> HttpWebRequest
            httpWebRequest.Method <- "GET"
            let httpResponse = httpWebRequest.GetResponse() :?> HttpWebResponse

            let streamReader = new StreamReader(httpResponse.GetResponseStream())
            let res = streamReader.ReadToEnd()
            let metaData = JsonValue.Parse(res)
            let wsUrl = metaData?url.AsString()

            let factory : WebSocketClientFactory = new Ninja.WebSockets.WebSocketClientFactory()
            let wsOptions = new WebSocketClientOptions()
            wsOptions.KeepAliveInterval <- TimeSpan.Zero 
            wsOptions.NoDelay <- true
            factory.ConnectAsync(new Uri(wsUrl), wsOptions).Result

        let triggerEvent this msgType msg =
            match msgType with
            | "hello" -> hello.Trigger(this, null)
            | "goodbye" -> goodbye.Trigger(this, null)
            | "message" -> let a = jsonToObject<MessageEventArgs> msg
                           match a.Subtype with
                           | null              -> onMessage.Trigger(this, a)
                                                  if a.Text.StartsWith("!") then 
                                                    let c = EventArgs.populateCommandEventArgs a.Text a.Channel a.User
                                                    processCommand.Trigger(this, c)
                           | "message_changed" -> onMessageEdit.Trigger(this, a)
                                                  if a.Message.Text.StartsWith("!") then
                                                    let c = EventArgs.populateCommandEventArgs a.Message.Text a.Channel a.User
                                                    processCommand.Trigger(this, c)
                           | _                 -> printfn "%A" msg

            | "user_typing" -> let a = jsonToObject<UserTypingEventArgs> msg
                               onUserTyping.Trigger(this, a)
                               printfn "%A" a
            | _ -> printfn "Received %A" msgType

        member private this.getData<'T> endpoint path =
            let getJsonFrom (endpoint:string) =
                let separator = if endpoint.Contains "?" then '&' else '?'
                let url = sprintf "%s%ctoken=%s" endpoint separator apiKey
                let webClient = new WebClient()
                webClient.DownloadString url

            getJsonFrom endpoint
            |> JObject.Parse
            |> fun obj -> obj.SelectToken path
            |> fun json -> json.ToObject<'T>()

        member private this.processMessage msg =
            let data = JsonValue.Parse(msg)
            let msgType = data.["type"].AsString()
            triggerEvent this msgType msg

        member this.Connect = 
            let ws = websocket
            if ws.State = WebSocketState.Open then
                let workflow = Listener.startListening ws (fun msg -> (this.processMessage msg))
                Async.Start (workflow, token.Token)
            else
                printfn "Websocket connection failed!"

        member this.Disconnect =
            token.Cancel
            
        member this.GetChannels () =
            this.getData<ChannelDescription array> "https://slack.com/api/channels.list" "$.channels"

        member this.GetUsers () =
            this.getData<User array> "https://slack.com/api/users.list" "$.members"

        member this.GetUser id =
            this.getData<User> (sprintf "https://slack.com/api/users.info?user=%s" id) "$.user"

        member this.GetImChannelForUser id =
            this.getData<string> (sprintf "https://slack.com/api/im.open?user=%s" id) "$.channel.id"

        member this.PostMessage channel message =
            let postMsgUrl = StringBuilder "https://slack.com/api/chat.postMessage?token="
            postMsgUrl.Append apiKey |> ignore
            postMsgUrl.Append "&channel=" |> ignore
            channel |> Uri.EscapeDataString |> postMsgUrl.Append |> ignore
            postMsgUrl.Append "&text=" |> ignore
            message |> Uri.EscapeDataString |> postMsgUrl.Append |> ignore
            let endpoint = postMsgUrl.ToString()
            this.getData<bool> endpoint "$.ok"

        member this.CheckAuth () =
            this.getData<AuthResult> "https://slack.com/api/auth.test" "$"

        // Events
        [<CLIEvent>]
        member this.Hello = hello.Publish

        [<CLIEvent>]
        member this.Goodbye = goodbye.Publish

        [<CLIEvent>]
        member this.OnUserTyping = onUserTyping.Publish

        [<CLIEvent>]
        member this.OnMessage = onMessage.Publish

        [<CLIEvent>]
        member this.OnMessageEdit = onMessageEdit.Publish

        [<CLIEvent>]
        member this.ProcessCommand = processCommand.Publish



