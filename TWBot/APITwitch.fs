module TWBot.APITwitch

open TWBot
open FSharp.Data
open FSharp.Json
open DataBase
open TokensData
open TypesDefinition

let deserializeRespons<'a> respons =
    Logger.Log.TraceInf <| sprintf "Start json deserialize respons to record"

    match respons with
    | Ok (ok) ->
        try
            Logger.Log.TraceDeb <| sprintf "json deserializeRespons %s" ok
            Json.deserialize<'a> ok |> Ok
        with :? JsonDeserializationError as eX ->
            Logger.Log.TraceErr <| sprintf "deserializeRespons<'a> JsonDeserializationError Err:%s P: %A" eX.Message respons

            Logger.Log.TraceExc <| sprintf "deserializeRespons<'a> JsonDeserializationError EX: %A" eX.StackTrace

            Error <| sprintf "deserializeRespons<'a> JsonDeserializationError Err:%s P: %A" eX.Message respons
    | Error (err) ->
        Logger.Log.TraceWarn <| sprintf "Failed json deserialize respons to record, respons err"
        Error err

[<Literal>]
let private _APIurl = @"https://api.twitch.tv/helix/"

let private bearerToken (prefix: bool) =
    match SingleData.DB.getBotSetting BotSettings.APIBearerToken with
    | Ok (token) ->
        if prefix then
            "Bearer " + token.bValue
        else
            token.bValue
    | Error (err) -> err

let private _headers () =
    [ "Accept", "application/vnd.twitchtv.v5+json"
      "Authorization", bearerToken true
      "Client-ID", ClientID ]

let getUsers userName =
    Logger.Log.TraceInf <| sprintf "Start getUsers api request for %s" userName

    try
        let url = sprintf @"%susers" _APIurl
        Logger.Log.TraceDeb <| sprintf "getUsers %s" url

        Http.RequestString(url, httpMethod = HttpMethod.Get, query = [ "login", userName ], headers = _headers ())
        |> Ok
    with :? System.Net.WebException as eX ->
        Logger.Log.TraceErr <| sprintf "getUsers WebException Err:%s P: %s" eX.Message userName
        Logger.Log.TraceExc <| sprintf "getUsers WebException EX: %A" eX.StackTrace
        Error <| sprintf "getUsers WebException Err:%s P: %s" eX.Message userName

let searchChannels (channel: Channels) =
    Logger.Log.TraceInf <| sprintf "Start searchChannels api request for %s" channel.String

    try
        let url = sprintf @"%ssearch/channels" _APIurl
        Logger.Log.TraceDeb <| sprintf "searchChannels %s" url

        Http.RequestString(url, httpMethod = HttpMethod.Get, query = [ "query", channel.String ], headers = _headers ())
        |> Ok
    with :? System.Net.WebException as eX ->
        Logger.Log.TraceErr <| sprintf "searchChannels WebException Err:%s P: %s" eX.Message channel.String

        Logger.Log.TraceExc <| sprintf "searchChannels WebException EX: %A" eX.StackTrace
        Error <| sprintf "searchChannels WebException Err:%s P: %s" eX.Message channel.String

let getStreams (channel: Channels) =
    Logger.Log.TraceInf <| sprintf "Start getStreams api request for %s" channel.String

    try
        let url = sprintf @"%sstreams" _APIurl
        Logger.Log.TraceDeb <| sprintf "getStreams %s" url

        Http.RequestString(url, httpMethod = "GET", query = [ "user_login", channel.String ], headers = _headers ())
        |> Ok
    with :? System.Net.WebException as eX ->
        Logger.Log.TraceErr <| sprintf "getStreams WebException Err:%s P: %s" eX.Message channel.String

        Logger.Log.TraceExc <| sprintf "getStreams WebException EX: %A" eX.StackTrace
        Error <| sprintf "getStreams WebException Err:%s P: %s" eX.Message channel.String

let getUserSubscribtion channelID userID =
    Logger.Log.TraceInf <| sprintf "Start getUserSubscribtion api request for %s" userID

    try
        let url = sprintf @"%ssubscriptions/user" _APIurl
        Logger.Log.TraceDeb <| sprintf "getUserSubscribtion %s" url

        Http.RequestString(
            url,
            httpMethod = "GET",
            query =
                [ "broadcaster_id", channelID
                  "user_id", userID ],
            headers = _headers ()
        )
        |> Ok
    with :? System.Net.WebException as eX ->
        Logger.Log.TraceErr <| sprintf "getUserSubscribtion WebException Err:%s P: %s %s" eX.Message channelID userID

        Logger.Log.TraceExc <| sprintf "getUserSubscribtion WebException EX: %A" eX.StackTrace

        Error <| sprintf "getUserSubscribtion WebException Err:%s P: %s %s" eX.Message channelID userID

let getChatters (channel: Channels) =
    Logger.Log.TraceInf <| sprintf "Start getChatters api request for %s" channel.String

    try
        let url =
            sprintf @"http://tmi.twitch.tv/group/user/%s/chatters" channel.String

        Logger.Log.TraceDeb <| sprintf "getChatters %s" url

        Http.RequestString(url, httpMethod = HttpMethod.Get)
        |> Ok
    with :? System.Net.WebException as eX ->
        Logger.Log.TraceErr <| sprintf "getChatters WebException Err:%s P: %s" eX.Message channel.String

        Logger.Log.TraceExc <| sprintf "getChatters WebException EX: %A" eX.StackTrace
        Error <| sprintf "getChatters WebException Err:%s P: %s" eX.Message channel.String

let checkOnline channel =
    getStreams channel
    |> deserializeRespons<GetStreams>
    |> function
    | Ok (ok) ->
        if ok.data.Length > 0 then
            true
        else
            false
    | Error (_) -> false

let private getAccessToken () =
    Logger.Log.TraceInf <| sprintf "Start getAccessToken api request"

    try
        let url = "https://id.twitch.tv/oauth2/token"
        Logger.Log.TraceDeb <| sprintf "getAccessToken %s" url

        Http.RequestString(
            url,
            httpMethod = HttpMethod.Post,
            query =
                [ "client_id", ClientID
                  "client_secret", SecretID
                  "grant_type", "client_credentials"
                  "scope", Scope ]
        )
        |> function
        | ok ->
            Logger.Log.TraceInf <| sprintf "Token access get successful"
            Ok ok
    with :? System.Net.WebException as eX ->
        Logger.Log.TraceErr <| sprintf "getAccessToken WebException Err:%s" eX.Message
        Logger.Log.TraceExc <| sprintf "getAccessToken WebException EX: %A" eX.StackTrace
        Error <| sprintf "getAccessToken WebException Err:%s" eX.Message

let private revokeAccessToken () =
    Logger.Log.TraceInf <| sprintf "Start revokeAccessToken api request"

    try
        let url = "https://id.twitch.tv/oauth2/revoke"
        Logger.Log.TraceDeb <|  sprintf "revokeAccessToken %s" url

        Http.RequestString(
            url,
            httpMethod = HttpMethod.Post,
            query =
                [ "client_id", ClientID
                  "token", bearerToken false ]
        )
        |> ignore

        Logger.Log.TraceInf <| sprintf "Token revoke successful"
        Ok "Token revoke successful"
    with :? System.Net.WebException as eX ->
        Logger.Log.TraceErr <| sprintf "revokeAccessToken WebException Err:%s" eX.Message
        Logger.Log.TraceExc <| sprintf "revokeAccessToken WebException EX: %A" eX.StackTrace
        Error <| sprintf "revokeAccessToken WebException Err:%s" eX.Message

let updateAccessToken () =
    Logger.Log.TraceInf <| sprintf "Start updateAccessToken operation"
    revokeAccessToken () |> ignore

    getAccessToken ()
    |> deserializeRespons<``OAuth client credentials flow``>
    |> function
    | Ok (ok) ->
        Logger.Log.TraceInf <| sprintf "UpdateAccessToken successful"

        SingleData.DB.setBotSetting
            { bSetting = BotSettings.APIBearerToken
              bValue = ok.access_token }
        |> ignore
    | Error (_) -> Logger.Log.TraceWarn <| sprintf "UpdateAccessToken failed, respons from json"
