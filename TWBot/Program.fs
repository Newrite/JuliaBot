open TWBot
open TWBot.DataBase
open TWBot.TypesDefinition


//for i in 0 .. 10 do
//    Bot.Commands.commandList2 None
//    |> List.iter (fun elem -> printfn "%s" (elem.Command.Force().chAnswer))
//
//APITwitch.updateAccessToken()
//
//APITwitch.getUserSubscribtion "54987522" "54987522"
//|>APITwitch.deserializeRespons<GetUserSubscribtion>
//|>function
//|Ok(ok) -> printfn "%A" ok
//|Error(err) -> printfn "%A" err
//
let b =
    TypesDefinition.Bot(new System.Net.Sockets.TcpClient())

let rw = Bot.connection b

while true do
    Bot.handleChat rw
    System.Threading.Thread.Sleep(100)
