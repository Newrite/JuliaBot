module TWBot.Logger

open TWBot.TypesDefinition
open System
open System.IO
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

type LogLevel =
    | Error
    | Warning
    | Information
    | Debug
    | Exception

    member self.String =
        match self with
        | Error -> "E"
        | Warning -> "W"
        | Information -> "I"
        | Debug -> "D"
        | Exception -> "EX"

module LogChat =
    let private messageUser (msgr: MessageRead) =
        sprintf "[%s][%A][%s] %s" msgr.Channel.String DateTime.Now msgr.User.Name msgr.Message
        
    let private toWrite (messageToLog: string) (channel: Channels) =
        use logFile =
            let path =
                sprintf "logschat/%s.txt" channel.String
            File.Open(path, FileMode.Append)
        use logFileWriter = new StreamWriter(logFile)
        logFileWriter.WriteLine(messageToLog)
        logFileWriter.Flush()
        
    let private toWriteRaw (messageToLog: string) =
        use logFile =
            let path =
                sprintf "logschat/%s.txt" "TWITCH_RAW_MESSAGE"
            File.Open(path, FileMode.Append)
        use logFileWriter = new StreamWriter(logFile)
        logFileWriter.WriteLine(messageToLog)
        logFileWriter.Flush()
            
    let writePrintBot messageToLog (channel: Channels) =
        printfn "%s" messageToLog
        toWrite messageToLog channel
        
    let writePrintRaw messageToLog =
        printfn "%s" messageToLog
        toWriteRaw messageToLog
        
    let writePrint msgr =
        let msg = messageUser msgr
        printfn "%s" msg
        toWrite msg msgr.Channel

module private Log =

    let toWrite (logLevel: LogLevel) (messageToLog: string) =
        if logLevel = LogLevel.Exception then
            use logFile =
                File.Open("logexception.txt", FileMode.Append)

            use logFileWriter = new StreamWriter(logFile)
            logFileWriter.WriteLine(messageToLog)
            logFileWriter.Flush()
        else
            use logFile =
                File.Open("logmain.txt", FileMode.Append)

            use logFileWriter = new StreamWriter(logFile)
            logFileWriter.WriteLine(messageToLog)
            logFileWriter.Flush()

    let WritePrint (logLevel: LogLevel) messageToLog =
        printfn "%s" messageToLog
        toWrite logLevel messageToLog

type Log() =
    static member TraceWarn
        (
            message: string,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] line: int
        ) =

        let logLevel = LogLevel.Warning

        let finalMessage =
            sprintf "[%s][%A]%s %s... %s" logLevel.String DateTime.Now path (string line) message

        Log.WritePrint logLevel finalMessage

    static member TraceErr
        (
            message: string,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] line: int
        ) =

        let logLevel = LogLevel.Error

        let finalMessage =
            sprintf "[%s][%A]%s %s... %s" logLevel.String DateTime.Now path (string line) message

        Log.WritePrint logLevel finalMessage

    static member TraceExc
        (
            message: string,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] line: int
        ) =

        let logLevel = LogLevel.Exception

        let finalMessage =
            sprintf "[%s][%A]%s %s... %s" logLevel.String DateTime.Now path (string line) message

        Log.WritePrint logLevel finalMessage

    static member TraceInf
        (
            message: string,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] line: int
        ) =

        let logLevel = LogLevel.Information

        let finalMessage =
            sprintf "[%s][%A]%s %s... %s" logLevel.String DateTime.Now path (string line) message

        Log.WritePrint logLevel finalMessage

    static member TraceDeb
        (
            message: string,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] line: int
        ) =

        let logLevel = LogLevel.Debug

        let finalMessage =
            sprintf "[%s][%A]%s %s... %s" logLevel.String DateTime.Now path (string line) message

        Log.WritePrint logLevel finalMessage
