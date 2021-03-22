open TWBot

//dotnet publish -c Release  -r linux-arm -p:PublishSingleFile=true --self-contained true

let b =
    TypesDefinition.Bot(new System.Net.Sockets.TcpClient())

let rw = Bot.connection b

while true do
    Bot.handleChat rw
    System.Threading.Thread.Sleep(100)
