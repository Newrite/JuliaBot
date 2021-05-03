module TWBot.Logger

open TWBot.TypesDefinition
open System
open System.IO
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

let mutable fileNameMain = "logmain.txt"
let mutable fileNameException = "logexception.txt"

type LogLevel =
    | ErrorL
    | WarningL
    | InformationL
    | DebugL
    | ExceptionL

    member self.String =
        match self with
        | ErrorL -> "E"
        | WarningL -> "W"
        | InformationL -> "I"
        | DebugL -> "D"
        | ExceptionL -> "EX"

module LogChat =
    let private messageUser (msgr: MessageRead) =
        sprintf "[%s][%A][%s] %s" msgr.Channel.String DateTime.Now msgr.User.Name msgr.Message

    let private toWrite (messageToLog: string) (channel: Channels) =
        use logFile =
            let path = sprintf "logschat/%s.txt" channel.String
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

    let toWrite (logLevel: LogLevel) (messageToLog: string) nameMain nameEx =
        if logLevel = LogLevel.ExceptionL then
            use logFile = File.Open(nameEx, FileMode.Append)

            use logFileWriter = new StreamWriter(logFile)
            logFileWriter.WriteLine(messageToLog)
            logFileWriter.Flush()
        else
            use logFile = File.Open(nameMain, FileMode.Append)

            use logFileWriter = new StreamWriter(logFile)
            logFileWriter.WriteLine(messageToLog)
            logFileWriter.Flush()

    let WritePrint (logLevel: LogLevel) messageToLog nameMain nameEx =
        printfn "%s" messageToLog
        toWrite logLevel messageToLog nameMain nameEx


type Log() =

    static let mutable m_logFileName = "logmain.txt"

    static let mutable m_logFileEx = "logexception.txt"

    static member logFileName
        with set (name) = m_logFileName <- name

    static member logFileEx
        with set (name) = m_logFileEx <- name

    static member TraceWarn
        (
            message: string,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] line: int
        ) =

        let logLevel = LogLevel.WarningL

        let finalMessage =
            sprintf "[%s][%A]%s %s... %s" logLevel.String DateTime.Now path (string line) message

        Log.WritePrint logLevel finalMessage m_logFileName m_logFileEx

    static member TraceErr
        (
            message: string,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] line: int
        ) =

        let logLevel = LogLevel.ErrorL

        let finalMessage =
            sprintf "[%s][%A]%s %s... %s" logLevel.String DateTime.Now path (string line) message

        Log.WritePrint logLevel finalMessage m_logFileName m_logFileEx

    static member TraceExc
        (
            message: string,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] line: int
        ) =

        let logLevel = LogLevel.ExceptionL

        let finalMessage =
            sprintf "[%s][%A]%s %s... %s" logLevel.String DateTime.Now path (string line) message

        Log.WritePrint logLevel finalMessage m_logFileName m_logFileEx

    static member TraceInf
        (
            message: string,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] line: int
        ) =

        let logLevel = LogLevel.InformationL

        let finalMessage =
            sprintf "[%s][%A]%s %s... %s" logLevel.String DateTime.Now path (string line) message

        Log.WritePrint logLevel finalMessage m_logFileName m_logFileEx

    static member TraceDeb
        (
            message: string,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] line: int
        ) =

        let logLevel = LogLevel.DebugL

        let finalMessage =
            sprintf "[%s][%A]%s %s... %s" logLevel.String DateTime.Now path (string line) message

        Log.WritePrint logLevel finalMessage m_logFileName m_logFileEx
