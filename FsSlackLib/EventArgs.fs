namespace FsSlackLib

open Domain
open FSharp.Data
open FSharp.Data

module EventArgs =
    open Newtonsoft.Json

    type Icons =
        { [<JsonProperty("image_48")>] Image_48: string }

    type Bot =
        { [<JsonProperty("id")>] Id: string
          [<JsonProperty("app_id")>] AppId: string
          [<JsonProperty("name")>] Name : string 
          [<JsonProperty("icons")>] Icons : Icons }

    type Channel =
        {  [<JsonProperty("id")>] Id: string
           [<JsonProperty("name")>] Name : string 
           [<JsonProperty("name")>] Creator : string}

    type Message =
        { [<JsonProperty("text")>] Text: string }

    type PreviousMessage =
        { [<JsonProperty("type")>] Type: string
          [<JsonProperty("user")>] User: string
          [<JsonProperty("text")>] Text: string
          [<JsonProperty("ts")>] Timestamp: string
          [<JsonProperty("event_ts")>] EventTimestamp: string
          [<JsonProperty("client_msg_id")>] ClientMsgId: string }

    type UserTypingEventArgs =
        { [<JsonProperty("type")>] Type: string
          [<JsonProperty("channel")>] Channel: string
          [<JsonProperty("user")>] User: string }

    type BotAddedEventArgs =
        { [<JsonProperty("type")>] Type: string
          [<JsonProperty("bot")>] Bot: Bot }

    type BotChangedEventArgs =
        { [<JsonProperty("type")>] Type: string
          [<JsonProperty("bot")>] Bot: Bot }

    type ChannelArchiveEventArgs =
        { [<JsonProperty("type")>] Type: string
          [<JsonProperty("channel")>] Channel: string
          [<JsonProperty("user")>] User: string }

    type ChannelUnarchiveEventArgs =
        { [<JsonProperty("type")>] Type: string
          [<JsonProperty("channel")>] Channel: string
          [<JsonProperty("user")>] User: string }
   
    type ChannelCreatededEventArgs =
        { [<JsonProperty("type")>] Type: string
          [<JsonProperty("channel")>] Channel: Channel }

    type ChannelDeletedEventArgs =
        { [<JsonProperty("type")>] Type: string
          [<JsonProperty("channel")>] Channel: string }

    type MessageEventArgs =
        { [<JsonProperty("type")>] Type: string
          [<JsonProperty("subtype")>] Subtype: string
          [<JsonProperty("channel")>] Channel: string
          [<JsonProperty("user")>] User: string
          [<JsonProperty("text")>] Text: string
          [<JsonProperty("ts")>] Timestamp: string
          [<JsonProperty("event_ts")>] EventTimestamp: string
          [<JsonProperty("team")>] Team: string 
          [<JsonProperty("client_msg_id")>] ClientMsgId: string
          [<JsonProperty("message")>] Message: Message
          [<JsonProperty("previous_message")>] PreviousMessage: PreviousMessage }


    // This type is not a native of the Slack RTM API; it is used with bots to process "!" commands
    type CommandEventArgs =
        { FullCommandText: string
          Command: string
          ArgsAsList: string list
          ArgsAsString: string
          CommandIdentifier: string
          Channel: string
          User: string } 

    let populateCommandEventArgs (text: string) (channel: string) (user: string) =
        let words = text.Split ' '
        let command = words.[0].Replace("!", "")
        { FullCommandText = text
          CommandIdentifier = "!"
          User = user
          Channel = channel
          Command = command 
          ArgsAsList = words.[1..words.Length-1 ] |> Array.toList 
          ArgsAsString = words.[1..words.Length-1 ] |> String.concat " " }

    
        