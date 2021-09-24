module TWBot.APITwitch

open TWBot
open FSharp.Data
open FSharp.Json
open DataBase
open TokensData
open TypesDefinition

let deserializeRespons<'a> respons =
    Logger.Log.TraceInf
    <| sprintf "Start json deserialize respons to record"

    match respons with
    | Ok (ok) ->
        try
            Logger.Log.TraceDeb
            <| sprintf "json deserializeRespons %s" ok

            Json.deserialize<'a> ok |> Ok
        with :? JsonDeserializationError as eX ->
            Logger.Log.TraceErr
            <| sprintf "deserializeRespons<'a> JsonDeserializationError Err:%s P: %A" eX.Message respons

            Logger.Log.TraceExc
            <| sprintf "deserializeRespons<'a> JsonDeserializationError EX: %A" eX.StackTrace

            Error
            <| sprintf "deserializeRespons<'a> JsonDeserializationError Err:%s P: %A" eX.Message respons
    | Error (err) ->
        Logger.Log.TraceWarn
        <| sprintf "Failed json deserialize respons to record, respons err"

        Error err

module Requests =

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
        Logger.Log.TraceInf
        <| sprintf "Start getUsers api request for %s" userName

        try
            let url = sprintf @"%susers" _APIurl
            Logger.Log.TraceDeb <| sprintf "getUsers %s" url

            Http.RequestString(url, httpMethod = HttpMethod.Get, query = [ "login", userName ], headers = _headers ())
            |> Ok
        with :? System.Net.WebException as eX ->
            Logger.Log.TraceErr
            <| sprintf "getUsers WebException Err:%s P: %s" eX.Message userName

            Logger.Log.TraceExc
            <| sprintf "getUsers WebException EX: %A" eX.StackTrace

            Error
            <| sprintf "getUsers WebException Err:%s P: %s" eX.Message userName

    let searchChannels (channel: Channels) =
        Logger.Log.TraceInf
        <| sprintf "Start searchChannels api request for %s" channel.String

        try
            let url = sprintf @"%ssearch/channels" _APIurl

            Logger.Log.TraceDeb
            <| sprintf "searchChannels %s" url

            Http.RequestString(
                url,
                httpMethod = HttpMethod.Get,
                query = [ "query", channel.String ],
                headers = _headers ()
            )
            |> Ok
        with :? System.Net.WebException as eX ->
            Logger.Log.TraceErr
            <| sprintf "searchChannels WebException Err:%s P: %s" eX.Message channel.String

            Logger.Log.TraceExc
            <| sprintf "searchChannels WebException EX: %A" eX.StackTrace

            Error
            <| sprintf "searchChannels WebException Err:%s P: %s" eX.Message channel.String

    let getStream (channel: Channels) =
        Logger.Log.TraceInf
        <| sprintf "Start getStreams api request for %s" channel.String

        try
            let url = sprintf @"%sstreams" _APIurl
            Logger.Log.TraceDeb <| sprintf "getStreams %s" url

            Http.RequestString(url, httpMethod = "GET", query = [ "user_login", channel.String ], headers = _headers ())
            |> Ok
        with :? System.Net.WebException as eX ->
            Logger.Log.TraceErr
            <| sprintf "getStreams WebException Err:%s P: %s" eX.Message channel.String

            Logger.Log.TraceExc
            <| sprintf "getStreams WebException EX: %A" eX.StackTrace

            Error
            <| sprintf "getStreams WebException Err:%s P: %s" eX.Message channel.String
            
    let getStreams (channels: Channels list) =
        Logger.Log.TraceInf
        <| sprintf "Start getStreams api request for %A" channels

        try
            let url = sprintf @"%sstreams" _APIurl
            Logger.Log.TraceDeb <| sprintf "getStreams %s" url

            Http.RequestString(url, httpMethod = "GET",
                               query =  [ for channel in channels -> "user_login", channel.String ],
                               headers = _headers ())
            |> Ok
        with :? System.Net.WebException as eX ->
            Logger.Log.TraceErr
            <| sprintf "getStreams WebException Err:%s P: %A" eX.Message channels

            Logger.Log.TraceExc
            <| sprintf "getStreams WebException EX: %A" eX.StackTrace

            Error
            <| sprintf "getStreams WebException Err:%s P: %A" eX.Message channels      

    let getUserSubscribtion channelID userID =
        Logger.Log.TraceInf
        <| sprintf "Start getUserSubscribtion api request for %s" userID

        try
            let url = sprintf @"%ssubscriptions/user" _APIurl

            Logger.Log.TraceDeb
            <| sprintf "getUserSubscribtion %s" url

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
            Logger.Log.TraceErr
            <| sprintf "getUserSubscribtion WebException Err:%s P: %s %s" eX.Message channelID userID

            Logger.Log.TraceExc
            <| sprintf "getUserSubscribtion WebException EX: %A" eX.StackTrace

            Error
            <| sprintf "getUserSubscribtion WebException Err:%s P: %s %s" eX.Message channelID userID

    let getChatters (channel: Channels) =
        Logger.Log.TraceInf
        <| sprintf "Start getChatters api request for %s" channel.String

        try
            let url =
                sprintf @"http://tmi.twitch.tv/group/user/%s/chatters" channel.String

            Logger.Log.TraceDeb
            <| sprintf "getChatters %s" url

            Http.RequestString(url, httpMethod = HttpMethod.Get)
            |> Ok
        with :? System.Net.WebException as eX ->
            Logger.Log.TraceErr
            <| sprintf "getChatters WebException Err:%s P: %s" eX.Message channel.String

            Logger.Log.TraceExc
            <| sprintf "getChatters WebException EX: %A" eX.StackTrace

            Error
            <| sprintf "getChatters WebException Err:%s P: %s" eX.Message channel.String

    let checkOnline channel =
        getStream channel
        |> deserializeRespons<GetStreams>
        |> function
        | Ok (ok) ->
            if ok.data.Length > 0 then
                true
            else
                false
        | Error (_) -> false

    let private getAccessToken () =
        Logger.Log.TraceInf
        <| sprintf "Start getAccessToken api request"

        try
            let url = "https://id.twitch.tv/oauth2/token"

            Logger.Log.TraceDeb
            <| sprintf "getAccessToken %s" url

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
                Logger.Log.TraceInf
                <| sprintf "Token access get successful"

                Ok ok
        with :? System.Net.WebException as eX ->
            Logger.Log.TraceErr
            <| sprintf "getAccessToken WebException Err:%s" eX.Message

            Logger.Log.TraceExc
            <| sprintf "getAccessToken WebException EX: %A" eX.StackTrace

            Error
            <| sprintf "getAccessToken WebException Err:%s" eX.Message

    let private revokeAccessToken () =
        Logger.Log.TraceInf
        <| sprintf "Start revokeAccessToken api request"

        try
            let url = "https://id.twitch.tv/oauth2/revoke"

            Logger.Log.TraceDeb
            <| sprintf "revokeAccessToken %s" url

            Http.RequestString(
                url,
                httpMethod = HttpMethod.Post,
                query =
                    [ "client_id", ClientID
                      "token", bearerToken false ]
            )
            |> ignore

            Logger.Log.TraceInf
            <| sprintf "Token revoke successful"

            Ok "Token revoke successful"
        with :? System.Net.WebException as eX ->
            Logger.Log.TraceErr
            <| sprintf "revokeAccessToken WebException Err:%s" eX.Message

            Logger.Log.TraceExc
            <| sprintf "revokeAccessToken WebException EX: %A" eX.StackTrace

            Error
            <| sprintf "revokeAccessToken WebException Err:%s" eX.Message

    let updateAccessToken () =
        Logger.Log.TraceInf
        <| sprintf "Start updateAccessToken operation"

        revokeAccessToken () |> ignore

        getAccessToken ()
        |> deserializeRespons<``OAuth client credentials flow``>
        |> function
        | Ok (ok) ->
            Logger.Log.TraceInf
            <| sprintf "UpdateAccessToken successful"

            SingleData.DB.setBotSetting
                { bSetting = BotSettings.APIBearerToken
                  bValue = ok.access_token }
            |> ignore
        | Error (_) ->
            Logger.Log.TraceWarn
            <| sprintf "UpdateAccessToken failed, respons from json"


module IRC =

    [<Literal>]
    let private caps =
        @"CAP REQ :twitch.tv/membership twitch.tv/tags twitch.tv/commands"

    let channels = Channels.ToList

    let private readerWriter (bot: Bot) =
        Logger.Log.TraceInf "Get ReaderWriter from Bot connection"

        match bot with
        | Bot (tcp) ->
            { Reader = new System.IO.StreamReader(tcp.GetStream())
              Writer = new System.IO.StreamWriter(tcp.GetStream()) }


    let sendMessage (msg: MessageWrite) =
        Logger.Log.TraceInf "Send message irc"

        if msg.Message.Length >= 1 then

            let msgLog =
                sprintf "[%s][%A][%s] %s" msg.Channel.String System.DateTime.Now "juliaeternal" msg.Message

            Logger.LogChat.writePrintBot msgLog msg.Channel

            sprintf "PRIVMSG #%s :%s\n\r" msg.Channel.String msg.Message
            |> msg.Writer.WriteLine

            msg.Writer.Flush()
        else
            Logger.Log.TraceWarn "Send message irc. Message less length then two."

    let sendRaw (rw: ReaderWriter) (raw: string) =
        Logger.Log.TraceInf "Send raw irc"

        if raw.Length >= 1 then
            Logger.LogChat.writePrintRaw raw
            rw.Writer.WriteLine(raw)
            rw.Writer.Flush()
        else
            Logger.Log.TraceWarn "Send raw irc. Raw less length then two."

    let readChat (rw: ReaderWriter) =
        //Закомментировано, вызывается каждую итерацию основного цикла.
        //Logger.Log.TraceInf "Read chat irc"
        let msg = rw.Reader.ReadLine()
        //Logger.Log.TraceDeb <| sprintf "readChat getmsg: %s" msg
        if msg.Contains("PING :tmi.twitch.tv") then
            Logger.LogChat.writePrintRaw "PING :tmi.twitch.tv"
            Logger.LogChat.writePrintRaw "PONG :tmi.twitch.tv"
            sendRaw rw "PONG :tmi.twitch.tv"
            msg
        else
            msg

    let private initDBChannel (channel: Channels) =
        Logger.Log.TraceInf
        <| sprintf "Start initialization database for channel %s" channel.String

        SingleData.DB.createChannelSettingsTable channel
        |> sprintf "createChannelSettingsTable %A"
        |> Logger.Log.TraceDeb

        SingleData.DB.createChannelCommandsTable channel
        |> sprintf "createChannelCommandsTable %A"
        |> Logger.Log.TraceDeb

    let private initDBBot () =
        Logger.Log.TraceInf "Start initialization database for bot"

        SingleData.DB.createBotSettingsTable ()
        |> sprintf "%A"
        |> Logger.Log.TraceDeb

        Requests.updateAccessToken ()
        |> sprintf "%A"
        |> Logger.Log.TraceDeb

    let private joinChannels (rw: ReaderWriter) (channelsList: Channels list) =
        Logger.Log.TraceInf
        <| sprintf "Start join, channels: %A" channelsList

        let rec joinChannel listSize =
            if listSize - 1 < 0 then
                Logger.Log.TraceInf "Channels in list end."
                ()
            else
                let currentSize = listSize - 1
                let channel = List.item currentSize channelsList

                initDBChannel channel
                |> sprintf "%A"
                |> Logger.Log.TraceDeb

                Logger.Log.TraceDeb
                <| sprintf "Join to %s" channel.String

                channel.String
                |> sprintf "JOIN #%s\n\r"
                |> sendRaw rw

                joinChannel currentSize

        joinChannel channelsList.Length

    let connection (bot: Bot) =
        Logger.Log.TraceInf "Start connection irc"
        initDBBot ()

        match bot with
        | Bot (tcp) ->
            Logger.Log.TraceDeb "Connect to twitch server"
            tcp.Connect("irc.chat.twitch.tv", 6667)

            let rw = readerWriter (bot)
            Logger.Log.TraceDeb "Start all work to init and join channels"
            sendRaw rw (sprintf "PASS %s\n\r" OAuth)
            sendRaw rw (sprintf "NICK %s\n\r" Nickname)
            sendRaw rw (sprintf "%s\n\r" caps)
            joinChannels rw channels
            Logger.Log.TraceDeb "All work done, return ReaderWriter"
            rw
