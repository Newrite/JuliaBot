//dotnet publish -c Release  -r linux-arm -p:PublishSingleFile=true --self-contained true

open TWBot
open TWBot.TypesDefinition

[<EntryPoint>]
let main argv =
    let b = Bot(new System.Net.Sockets.TcpClient())

    let readerWriter = APITwitch.IRC.connection b

    let minute = 1000 * 60
    
    let spamMessageReflyq (rw: ReaderWriter) channel minutes =
        async {
            let mutable tempCounter = Bot.Cache.tempReflyqMessageCounter

            while true do
                System.Threading.Thread.Sleep(minute * minutes)

                if APITwitch.Requests.checkOnline channel
                   && (Bot.Cache.tempReflyqMessageCounter - tempCounter)
                      >= 20 then
                    APITwitch.IRC.sendRaw
                        rw
                        (sprintf
                            "PRIVMSG #%s :https://discord.gg/BvRarMSpm3 анонсы стримов тут, в будущем возможно появится, что то еще Agakakskagesh\n\r"
                            channel.String)
        }

    let spamMessageXandr (rw: ReaderWriter) channel minutes =
        async {
            while true do
                System.Threading.Thread.Sleep(minute * minutes)

                if APITwitch.Requests.checkOnline channel then
                    APITwitch.IRC.sendRaw
                        rw
                        (sprintf
                            "PRIVMSG #%s :Команды бота: !help / VK: https://vk.com/xandr_tv / YouTube: https://www.youtube.com/channel/UC0oObsGZKntyAP_OoMnFIPA / GoodGame: https://goodgame.ru/channel/Xandr_Sh/ / Discord: https://discord.gg/5bDJWKK\n\r"
                            channel.String)
        }

    let spamMessageNewrite (rw: ReaderWriter) channel minutes =
        async {
            while true do
                System.Threading.Thread.Sleep(minute * minutes)

                if APITwitch.Requests.checkOnline channel then
                    APITwitch.IRC.sendRaw
                        rw
                        (sprintf
                            "PRIVMSG #%s :Стримы спонтанны, неизвестно когда будут Kappa Анонсы и всякое по моддингу скайрима здесь: https://discord.gg/RbJPhEU2eR\n\r"
                            channel.String)
        }

    Bot.Cache.initCache APITwitch.IRC.channels APITwitch.IRC.channels.Length

    Async.StartAsTask(spamMessageReflyq readerWriter Reflyq 20)
    |> ignore

    Async.StartAsTask(spamMessageXandr readerWriter XandrSH 15)
    |> ignore

    Async.StartAsTask(spamMessageNewrite readerWriter Newrite 30)
    |> ignore

    while true do
        APITwitch.IRC.readChat readerWriter
        |> Bot.Handlers.handleLine
        |> function
        | Ok (msgr) ->
            if msgr.Channel = Reflyq then
                Bot.Cache.tempReflyqMessageCounter <- Bot.Cache.tempReflyqMessageCounter + 1

            Bot.Handlers.handleCache msgr readerWriter
            Bot.Handlers.handleMasterCommands msgr readerWriter

            match Bot.Cache.checkToggleChannel msgr.Channel with
            | true ->
                Bot.Handlers.handleHelper msgr readerWriter
                Bot.Handlers.handleReacts msgr readerWriter
                Bot.Handlers.handleCommands msgr readerWriter
                Bot.Handlers.handleRewards msgr readerWriter
            | false -> ()
        | Error (err) -> printfn "%s" err

        System.Threading.Thread.Sleep(10)

    0
