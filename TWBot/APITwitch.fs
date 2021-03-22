module TWBot.APITwitch

open TWBot
open FSharp.Data
open FSharp.Json
open DataBase
open TokensData
open TypesDefinition

let deserializeRespons<'a> respons =
    Logger.Log.StartTrace(sprintf "Start json deserialize respons to record", Logger.LogLevel.Information)

    match respons with
    | Ok (ok) ->
        try
            Logger.Log.StartTrace(sprintf "json deserializeRespons %s" ok, Logger.LogLevel.Debug)
            Json.deserialize<'a> ok |> Ok
        with :? JsonDeserializationError as eX ->
            Logger.Log.StartTrace(
                sprintf "deserializeRespons<'a> JsonDeserializationError Err:%s P: %A" eX.Message respons,
                Logger.LogLevel.Error
            )

            Logger.Log.StartTrace(
                sprintf "deserializeRespons<'a> JsonDeserializationError EX: %A" eX.StackTrace,
                Logger.LogLevel.Exception
            )

            Error(sprintf "deserializeRespons<'a> JsonDeserializationError Err:%s P: %A" eX.Message respons)
    | Error (err) ->
        Logger.Log.StartTrace(sprintf "Failed json deserialize respons to record, respons err", Logger.LogLevel.Warning)
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
    Logger.Log.StartTrace(sprintf "Start getUsers api request for %s" userName, Logger.LogLevel.Information)

    try
        let url = sprintf @"%susers" _APIurl
        Logger.Log.StartTrace(sprintf "getUsers %s" url, Logger.LogLevel.Debug)

        Http.RequestString(url, httpMethod = HttpMethod.Get, query = [ "login", userName ], headers = _headers ())
        |> Ok
    with :? System.Net.WebException as eX ->
        Logger.Log.StartTrace(sprintf "getUsers WebException Err:%s P: %s" eX.Message userName, Logger.LogLevel.Error)
        Logger.Log.StartTrace(sprintf "getUsers WebException EX: %A" eX.StackTrace, Logger.LogLevel.Exception)
        Error(sprintf "getUsers WebException Err:%s P: %s" eX.Message userName)

let searchChannels (channel: Channels) =
    Logger.Log.StartTrace(sprintf "Start searchChannels api request for %s" channel.String, Logger.LogLevel.Information)

    try
        let url = sprintf @"%ssearch/channels" _APIurl
        Logger.Log.StartTrace(sprintf "searchChannels %s" url, Logger.LogLevel.Debug)

        Http.RequestString(url, httpMethod = HttpMethod.Get, query = [ "query", channel.String ], headers = _headers ())
        |> Ok
    with :? System.Net.WebException as eX ->
        Logger.Log.StartTrace(
            sprintf "searchChannels WebException Err:%s P: %s" eX.Message channel.String,
            Logger.LogLevel.Error
        )

        Logger.Log.StartTrace(sprintf "searchChannels WebException EX: %A" eX.StackTrace, Logger.LogLevel.Exception)
        Error(sprintf "searchChannels WebException Err:%s P: %s" eX.Message channel.String)

let getStreams (channel: Channels) =
    Logger.Log.StartTrace(sprintf "Start getStreams api request for %s" channel.String, Logger.LogLevel.Information)

    try
        let url = sprintf @"%sstreams" _APIurl
        Logger.Log.StartTrace(sprintf "getStreams %s" url, Logger.LogLevel.Debug)

        Http.RequestString(url, httpMethod = "GET", query = [ "user_login", channel.String ], headers = _headers ())
        |> Ok
    with :? System.Net.WebException as eX ->
        Logger.Log.StartTrace(
            sprintf "getStreams WebException Err:%s P: %s" eX.Message channel.String,
            Logger.LogLevel.Error
        )

        Logger.Log.StartTrace(sprintf "getStreams WebException EX: %A" eX.StackTrace, Logger.LogLevel.Exception)
        Error(sprintf "getStreams WebException Err:%s P: %s" eX.Message channel.String)

let getUserSubscribtion channelID userID =
    Logger.Log.StartTrace(sprintf "Start getUserSubscribtion api request for %s" userID, Logger.LogLevel.Information)

    try
        let url = sprintf @"%ssubscriptions/user" _APIurl
        Logger.Log.StartTrace(sprintf "getUserSubscribtion %s" url, Logger.LogLevel.Debug)

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
        Logger.Log.StartTrace(
            sprintf "getUserSubscribtion WebException Err:%s P: %s %s" eX.Message channelID userID,
            Logger.LogLevel.Error
        )

        Logger.Log.StartTrace(
            sprintf "getUserSubscribtion WebException EX: %A" eX.StackTrace,
            Logger.LogLevel.Exception
        )

        Error(sprintf "getUserSubscribtion WebException Err:%s P: %s %s" eX.Message channelID userID)

let getChatters (channel: Channels) =
    Logger.Log.StartTrace(sprintf "Start getChatters api request for %s" channel.String, Logger.LogLevel.Information)

    try
        let url =
            sprintf @"http://tmi.twitch.tv/group/user/%s/chatters" channel.String

        Logger.Log.StartTrace(sprintf "getChatters %s" url, Logger.LogLevel.Debug)

        Http.RequestString(url, httpMethod = HttpMethod.Get)
        |> Ok
    with :? System.Net.WebException as eX ->
        Logger.Log.StartTrace(
            sprintf "getChatters WebException Err:%s P: %s" eX.Message channel.String,
            Logger.LogLevel.Error
        )

        Logger.Log.StartTrace(sprintf "getChatters WebException EX: %A" eX.StackTrace, Logger.LogLevel.Exception)
        Error(sprintf "getChatters WebException Err:%s P: %s" eX.Message channel.String)

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
    Logger.Log.StartTrace(sprintf "Start getAccessToken api request", Logger.LogLevel.Information)

    try
        let url = "https://id.twitch.tv/oauth2/token"
        Logger.Log.StartTrace(sprintf "getAccessToken %s" url, Logger.LogLevel.Debug)

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
            Logger.Log.StartTrace(sprintf "Token access get successful", Logger.LogLevel.Information)
            Ok(ok)
    with :? System.Net.WebException as eX ->
        Logger.Log.StartTrace(sprintf "getAccessToken WebException Err:%s" eX.Message, Logger.LogLevel.Error)
        Logger.Log.StartTrace(sprintf "getAccessToken WebException EX: %A" eX.StackTrace, Logger.LogLevel.Exception)
        Error(sprintf "getAccessToken WebException Err:%s" eX.Message)

let private revokeAccessToken () =
    Logger.Log.StartTrace(sprintf "Start revokeAccessToken api request", Logger.LogLevel.Information)

    try
        let url = "https://id.twitch.tv/oauth2/revoke"
        Logger.Log.StartTrace(sprintf "revokeAccessToken %s" url, Logger.LogLevel.Debug)

        Http.RequestString(
            url,
            httpMethod = HttpMethod.Post,
            query =
                [ "client_id", ClientID
                  "token", bearerToken false ]
        )
        |> ignore

        Logger.Log.StartTrace(sprintf "Token revoke successful", Logger.LogLevel.Information)
        Ok("Token revoke successful")
    with :? System.Net.WebException as eX ->
        Logger.Log.StartTrace(sprintf "revokeAccessToken WebException Err:%s" eX.Message, Logger.LogLevel.Error)
        Logger.Log.StartTrace(sprintf "revokeAccessToken WebException EX: %A" eX.StackTrace, Logger.LogLevel.Exception)
        Error(sprintf "revokeAccessToken WebException Err:%s" eX.Message)

let updateAccessToken () =
    Logger.Log.StartTrace(sprintf "Start updateAccessToken operation", Logger.LogLevel.Information)
    revokeAccessToken () |> ignore

    getAccessToken ()
    |> deserializeRespons<``OAuth client credentials flow``>
    |> function
    | Ok (ok) ->
        Logger.Log.StartTrace(sprintf "UpdateAccessToken successful", Logger.LogLevel.Information)

        SingleData.DB.setBotSetting
            { bSetting = BotSettings.APIBearerToken
              bValue = ok.access_token }
        |> ignore
    | Error (_) -> Logger.Log.StartTrace(sprintf "UpdateAccessToken failed, respons from json", Logger.LogLevel.Warning)
