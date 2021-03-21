module TWBot.Logger

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
    static member StartTrace
        (
            message: string,
            logLevel: LogLevel,
            [<CallerFilePath; Optional; DefaultParameterValue("")>] path: string,
            [<CallerLineNumber; Optional; DefaultParameterValue(0)>] line: int
        ) =
        let finalMessage =
            sprintf "[%s][%A]%s %s... %s" logLevel.String DateTime.Now path (string line) message

        Log.WritePrint logLevel finalMessage
