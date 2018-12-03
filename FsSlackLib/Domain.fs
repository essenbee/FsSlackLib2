namespace FsSlackLib

module Domain =

    open Newtonsoft.Json
    open System
  
    [<CLIMutable>]
    type ChannelDescription =
        { [<JsonProperty("id")>] Id:string
          [<JsonProperty("name")>] Name:string
          [<JsonProperty("is_channel")>] IsChannel:bool
          [<JsonProperty("creator")>] Creator:string
          [<JsonProperty("is_archived")>] IsArchived:bool
          [<JsonProperty("is_general")>] IsGeneral:bool
          [<JsonProperty("is_member")>] IsMember:bool
          [<JsonProperty("members")>] MemberIds:string array }

    [<CLIMutable>]
    type User =
        { [<JsonProperty("id")>] Id:string
          [<JsonProperty("team_id")>] TeamId:string
          [<JsonProperty("name")>] Name:string
          [<JsonProperty("deleted")>] Deleted:bool
          [<JsonProperty("status")>] Status:string
          [<JsonProperty("color")>] Color:string
          [<JsonProperty("real_name")>] RealName:string
          [<JsonProperty("tz")>] Timezone:string
          [<JsonProperty("image_512")>] Avatar:string
          [<JsonProperty("is_admin")>] IsAdmin:bool
          [<JsonProperty("is_restricted")>] IsRestricted:bool
          [<JsonProperty("is_bot")>] IsBot:bool }

    [<CLIMutable>]
    type AuthResult =
        { [<JsonProperty("ok")>] Success:bool
          [<JsonProperty("url")>] Url:string
          [<JsonProperty("team")>] Team:string
          [<JsonProperty("user")>] User:string
          [<JsonProperty("team_id")>] TeamId:string
          [<JsonProperty("user_id")>] UserId:string }

    [<CLIMutable>]
    type InstantConversation =
        { [<JsonProperty("id")>] Id:string
          [<JsonProperty("is_im")>] IsIm:bool
          [<JsonProperty("user")>] User:string
          [<JsonProperty("is_user_deleted")>] IsUserDeleted:bool }

    type SentMessage =
        { ChannelId:string
          Text:string
          Botname:string option
          AsUser:bool
          IconUrl:string option }

    type SentFile =
        { ChannelId:string
          Text:string
          Filepath:string
          Title:string option }

        static member Empty =
          { ChannelId=""; Text=""; Filepath=""; Title=None }

