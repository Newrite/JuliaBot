//dotnet publish -c Release  -r linux-arm -p:PublishSingleFile=true --self-contained true
//builds for arm orange pi pc+
//for build on other platform edit TWBot.fsproj
open TWBot
open TWBot.TypesDefinition
open TWBot.Logger

[<EntryPoint>]
let main argv =
    Log.TraceInf "Start bot..."
    let b = Bot(new System.Net.Sockets.TcpClient())
    Log.TraceInf "Do connection..."
    let readerWriter = APITwitch.IRC.connection b
    Log.TraceInf "Connection complete."
    let minute = 1000 * 60

    let spamMessageReflyq (rw: ReaderWriter) (channel: Channels) minutes =
        async {
            Log.TraceInf "Start async spamMessageReflyq"

            Log.TraceDeb
            <| sprintf "spamMessageReflyq %s sleepMin %d" channel.String minutes

            let mutable tempCounter = Cache.tempReflyqMessageCounter

            while true do
                Log.TraceInf "sleep spamMessageReflyq..."
                System.Threading.Thread.Sleep(minute * minutes)
                Log.TraceInf "wakeup spamMessageReflyq, start checkOnline"

                Log.TraceDeb
                <| sprintf "spamMessageReflyq counter message tc %d cache %d" tempCounter Cache.tempReflyqMessageCounter

                if APITwitch.Requests.checkOnline channel
                   && (Cache.tempReflyqMessageCounter - tempCounter)
                      >= 20 then
                    Log.TraceInf "spamMessageReflyq, start message"

                    APITwitch.IRC.sendRaw
                        rw
                        (sprintf
                            "PRIVMSG #%s :https://discord.gg/BvRarMSpm3 анонсы стримов тут, в будущем возможно появится, что то еще Agakakskagesh\n\r"
                            channel.String)

                    tempCounter <- Cache.tempReflyqMessageCounter

                    Log.TraceDeb
                    <| sprintf "spamMessageReflyq, new tempCounter %d" tempCounter
        }

    let spamMessageKaelia (rw: ReaderWriter) (channel: Channels) minutes =
        async {
            Log.TraceInf "Start async spamMessageKaelia"

            Log.TraceDeb
            <| sprintf "spamMessageKaelia %s sleepMin %d" channel.String minutes

            let mutable tempCounter = Cache.tempKaeliaMessageCounter

            while true do
                Log.TraceInf "sleep spamMessageKaelia..."
                System.Threading.Thread.Sleep(minute * minutes)
                Log.TraceInf "wakeup spamMessageKaelia, start checkOnline"

                Log.TraceDeb
                <| sprintf "spamMessageKaelia counter message tc %d cache %d" tempCounter Cache.tempKaeliaMessageCounter

                if APITwitch.Requests.checkOnline channel
                   && (Cache.tempKaeliaMessageCounter - tempCounter)
                      >= 20 then
                    Log.TraceInf "spamMessageKaelia, start message"

                    APITwitch.IRC.sendRaw
                        rw
                        (sprintf
                            "PRIVMSG #%s :Чтобы быть в курсе свежих новостей и знать, когда новая подрубка: https://discord.gg/K9KRxvRkxq\n\r"
                            channel.String)

                    tempCounter <- Cache.tempKaeliaMessageCounter

                    Log.TraceDeb
                    <| sprintf "spamMessageKaelia, new tempCounter %d" tempCounter
        }

    let spamMessageXandr (rw: ReaderWriter) (channel: Channels) minutes =

        async {
            Log.TraceInf "Start async spamMessageXandr"

            Log.TraceDeb
            <| sprintf "spamMessageXandr %s sleepMin %d" channel.String minutes

            while true do
                Log.TraceInf "sleep spamMessageXandr..."
                System.Threading.Thread.Sleep(minute * minutes)
                Log.TraceInf "wakeup spamMessageXandr, start checkOnline"

                if APITwitch.Requests.checkOnline channel then
                    Log.TraceInf "spamMessageXandr, start message"

                    APITwitch.IRC.sendRaw
                        rw
                        (sprintf
                            "PRIVMSG #%s :Команды бота: !help / VK: https://vk.com/xandr_tv / YouTube: https://www.youtube.com/channel/UC0oObsGZKntyAP_OoMnFIPA / GoodGame: https://goodgame.ru/channel/Xandr_Sh/ / Discord: https://discord.gg/5bDJWKK\n\r"
                            channel.String)
        }

    let spamMessageNewrite (rw: ReaderWriter) (channel: Channels) minutes =
        async {
            Log.TraceInf "Start async spamMessageNewrite"

            Log.TraceDeb
            <| sprintf "spamMessageNewrite %s sleepMin %d" channel.String minutes

            while true do
                Log.TraceInf "sleep spamMessageNewrite..."
                System.Threading.Thread.Sleep(minute * minutes)
                Log.TraceInf "wakeup spamMessageNewrite, start checkOnline"

                if APITwitch.Requests.checkOnline channel then
                    Log.TraceInf "spamMessageNewrite, start message"

                    APITwitch.IRC.sendRaw
                        rw
                        (sprintf
                            "PRIVMSG #%s :Стримы спонтанны, неизвестно когда будут Kappa Анонсы и всякое по моддингу скайрима здесь: https://discord.gg/RbJPhEU2eR\n\r"
                            channel.String)
        }

    Log.TraceInf "Start initCache for channels"

    Log.TraceDeb
    <| sprintf "initCache channels %A" APITwitch.IRC.channels

    Cache.initCache APITwitch.IRC.channels APITwitch.IRC.channels.Length
    Log.TraceInf "Start Async spammers"

    Async.StartAsTask(spamMessageReflyq readerWriter Reflyq 20)
    |> ignore

    System.Threading.Thread.Sleep(5000)

    Async.StartAsTask(spamMessageXandr readerWriter XandrSH 15)
    |> ignore

    System.Threading.Thread.Sleep(5000)

    Async.StartAsTask(spamMessageKaelia readerWriter Kaelia 20)
    |> ignore

    System.Threading.Thread.Sleep(5000)

    Async.StartAsTask(spamMessageNewrite readerWriter Newrite 30)
    |> ignore

    System.Threading.Thread.Sleep(5000)
    Log.TraceInf "Start app loop"

    while true do
        APITwitch.IRC.readChat readerWriter
        |> Bot.Handlers.handleLine
        |> function
        | Ok (msgr) ->
            match msgr.Channel with
            | Reflyq -> Cache.tempReflyqMessageCounter <- Cache.tempReflyqMessageCounter + 1
            | Kaelia -> Cache.tempKaeliaMessageCounter <- Cache.tempKaeliaMessageCounter + 1
            | _ -> ()

            let ctx =
                { MessageRead = msgr
                  ReaderWriter = readerWriter }

            Bot.Handlers.handleCache ctx
            Bot.Handlers.handleMasterCommands ctx

            LogChat.writePrint msgr

            match Cache.checkToggleChannel msgr.Channel with
            | true ->
                Bot.Handlers.handleHelper ctx
                Bot.Handlers.handleReacts ctx
                Bot.Handlers.handleCommands ctx
                Bot.Handlers.handleRewards ctx
            | false -> ()
        | Error (err) ->
            Log.TraceWarn
            <| sprintf "Something wrong with parse line err: %s" err

        System.Threading.Thread.Sleep(10)

    0
