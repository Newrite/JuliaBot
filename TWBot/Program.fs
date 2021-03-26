open TWBot
open TWBot.TypesDefinition

//dotnet publish -c Release  -r linux-arm -p:PublishSingleFile=true --self-contained true

[<EntryPoint>]
let main argv =
    let b =
        Bot(new System.Net.Sockets.TcpClient())
    
    let rw = Bot.connection b
    
    let spamMessageReflyq (rw: ReaderWriter) = async {
        let mutable tempCounter = Bot.Cache.tempReflyqMessageCounter
        let minute = 1000*60
        while true do
            System.Threading.Thread.Sleep(minute*20)
            if APITwitch.checkOnline Reflyq && (Bot.Cache.tempReflyqMessageCounter - tempCounter) >= 20 then
                Bot.sendRaw rw (sprintf "PRIVMSG #%s :https://discord.gg/BvRarMSpm3 анонсы стримов тут, в будущем возможно появится, что то еще Agakakskagesh\n\r" Reflyq.String)
    }
    
    let spamMessageXandr (rw: ReaderWriter) = async {
        let minute = 1000*60
        while true do
            System.Threading.Thread.Sleep(minute*15)
            if APITwitch.checkOnline XandrSH then
                Bot.sendRaw rw (sprintf "PRIVMSG #%s :Команды бота: !help / VK: https://vk.com/xandr_tv / YouTube: https://www.youtube.com/channel/UC0oObsGZKntyAP_OoMnFIPA / GoodGame: https://goodgame.ru/channel/Xandr_Sh/ / Discord: https://discord.gg/5bDJWKK\n\r" XandrSH.String)
    }
    
    Async.StartAsTask (spamMessageReflyq rw) |> ignore
    Async.StartAsTask (spamMessageXandr rw) |> ignore
    
    while true do
        Bot.handleChat rw
        System.Threading.Thread.Sleep(100)
    0
