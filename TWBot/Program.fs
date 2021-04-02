//dotnet publish -c Release  -r linux-arm -p:PublishSingleFile=true --self-contained true

open TWBot
open TWBot.TypesDefinition

[<EntryPoint>]
let main argv =
    let b =
        Bot(new System.Net.Sockets.TcpClient())
    
    let rw = Bot.connection b
    
    let spamMessageReflyq (rw: ReaderWriter) channel minutes = async {
        let mutable tempCounter = Bot.Cache.tempReflyqMessageCounter
        while true do
            System.Threading.Thread.Sleep(Bot.Utils.minute*minutes)
            if APITwitch.checkOnline channel && (Bot.Cache.tempReflyqMessageCounter - tempCounter) >= 20 then
                Bot.sendRaw rw (sprintf "PRIVMSG #%s :https://discord.gg/BvRarMSpm3 анонсы стримов тут, в будущем возможно появится, что то еще Agakakskagesh\n\r" channel.String)
    }
    
    let spamMessageXandr (rw: ReaderWriter) channel minutes = async {
        while true do
            System.Threading.Thread.Sleep(Bot.Utils.minute*minutes)
            if APITwitch.checkOnline channel then
                Bot.sendRaw rw (sprintf "PRIVMSG #%s :Команды бота: !help / VK: https://vk.com/xandr_tv / YouTube: https://www.youtube.com/channel/UC0oObsGZKntyAP_OoMnFIPA / GoodGame: https://goodgame.ru/channel/Xandr_Sh/ / Discord: https://discord.gg/5bDJWKK\n\r" channel.String)
    }
    
    let spamMessageNewrite (rw: ReaderWriter) channel minutes = async {
        while true do
            System.Threading.Thread.Sleep(Bot.Utils.minute*minutes)
            if APITwitch.checkOnline channel then
                Bot.sendRaw rw (sprintf "PRIVMSG #%s :Стримы спонтанны, неизвестно когда будут Kappa Анонсы и всякое по моддингу скайрима здесь: https://discord.gg/RbJPhEU2eR\n\r" channel.String)
    }
    
    Async.StartAsTask (spamMessageReflyq rw Reflyq 20) |> ignore
    Async.StartAsTask (spamMessageXandr rw XandrSH 15) |> ignore
    Async.StartAsTask (spamMessageNewrite rw Newrite 30) |> ignore
    
    while true do
        Bot.handleChat rw
        System.Threading.Thread.Sleep(10)
    0
