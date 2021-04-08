module TWBot.DataBase

open TypesDefinition
open System.Data.SQLite



type SingleData private () =
    static let dataBase = SingleData()
    let databaseFilename = "data.sqlite3"

    let connectionStringFile =
        Logger.Log.TraceInf
        <| sprintf "Connection to DB Data Source=%s;Version=3" databaseFilename

        sprintf "Data Source=%s;Version=3;" databaseFilename

    let connection =
        Logger.Log.TraceDeb "Start to connection database"
        new SQLiteConnection(connectionStringFile)

    do connection.Open()
    static member DB = dataBase

    member private self.initBotSettingsTable() =
        let cmd =
            @"insert or ignore into BotSettings (Setting, Value) values"
            + "(@APIBearerToken, @APIBearerTokenValue)"

        Logger.Log.TraceInf "Start init botSettings"

        Logger.Log.TraceDeb
        <| sprintf "botSettings %s" cmd

        use cmd = new SQLiteCommand(cmd, connection)

        cmd.Parameters.AddWithValue("@APIBearerToken", "APIBearerToken")
        |> ignore

        cmd.Parameters.AddWithValue("@APIBearerTokenValue", "None")
        |> ignore

        try
            let resp = cmd.ExecuteNonQuery()

            if resp = -1 then
                Logger.Log.TraceInf
                <| sprintf "BotSettings is already created Code:%s" (string resp)

                Ok
                <| sprintf "BotSettings is already created Code:%s" (string resp)
            else
                Logger.Log.TraceInf
                <| sprintf "BotSettings is successful created Code:%s" (string resp)

                Ok
                <| sprintf "BotSettings is succeful created Code:%s" (string resp)
        with :? SQLiteException as eX ->
            Logger.Log.TraceErr
            <| sprintf "BotSettings SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

            Logger.Log.TraceExc
            <| sprintf "BotSettings SQLiteException EX: %A" eX.StackTrace

            Error
            <| sprintf "BotSettings SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

    member self.createBotSettingsTable() =
        let cmd =
            "create table if not exists BotSettings (Setting text not null primary key, Value text)"

        Logger.Log.TraceInf "Start create BotSettingsTable"

        Logger.Log.TraceDeb
        <| sprintf "BotSettingsTable %s" cmd

        use cmd = new SQLiteCommand(cmd, connection)

        try
            let resp = cmd.ExecuteNonQuery()

            if resp = -1 then
                Logger.Log.TraceInf
                <| sprintf "BotSettingsTable is already created Code:%s" (string resp)

                Ok
                <| sprintf "BotSettingsTable is already created Code:%s" (string resp)
            else
                Logger.Log.TraceInf
                <| sprintf "BotSettingsTable is successful created Code:%s" (string resp)

                self.initBotSettingsTable () |> ignore

                Ok
                <| sprintf "BotSettingsTable is successful created Code:%s" (string resp)
        with :? SQLiteException as eX ->
            Logger.Log.TraceErr
            <| sprintf "CreateBotSettings SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

            Logger.Log.TraceExc
            <| sprintf "CreateBotSettings SQLiteException EX: %A" eX.StackTrace

            Error
            <| sprintf "CreateBotSettings SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

    member self.createChannelCommandsTable(channel: Channels) =
        let cmd =
            sprintf @"create table if not exists %sCommands (CMD text not null primary key, ANSWER text)" channel.String

        Logger.Log.TraceInf
        <| sprintf "Start create ChannelCommandsTable for %s" channel.String

        Logger.Log.TraceDeb
        <| sprintf "ChannelCommandsTable %s %s" channel.String cmd

        use cmd = new SQLiteCommand(cmd, connection)

        try
            let resp = cmd.ExecuteNonQuery()

            if resp = -1 then
                Logger.Log.TraceInf
                <| sprintf "ChannelCommandsTable for %s is already created Code:%s" channel.String (string resp)

                Ok
                <| sprintf "ChannelCommandsTable for %s is already created Code:%s" channel.String (string resp)
            else
                Logger.Log.TraceInf
                <| sprintf "ChannelCommandsTable for %s is successful created Code:%s" channel.String (string resp)

                Ok
                <| sprintf "ChannelCommandsTable for %s is successful created Code:%s" channel.String (string resp)
        with :? SQLiteException as eX ->
            Logger.Log.TraceErr
            <| sprintf "ChannelCommandsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

            Logger.Log.TraceExc
            <| sprintf "ChannelCommandsTable SQLiteException EX: %A" eX.StackTrace

            Error
            <| sprintf "ChannelCommandsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

    member private self.initChannelSettingsTable(channel: Channels) =
        let cmd =
            sprintf @"insert or ignore into %sSettings " channel.String

        let cmd =
            cmd
            + "(Setting, Value) values"
            + "(@Prefix, @PrefixValue),"
            + "(@EmotionCoolDown, @EmotionCoolDownValue),"
            + "(@Toggle, @ToggleValue),"
            + "(@EmotionToggle, @EmotionToggleValue)"


        Logger.Log.TraceInf
        <| sprintf "Start init ChannelSettingsTable for %s" channel.String

        Logger.Log.TraceDeb
        <| sprintf "ChannelSettingsTable %s P: %s" channel.String cmd

        use cmd = new SQLiteCommand(cmd, connection)

        cmd.Parameters.AddWithValue("@Prefix", "Prefix")
        |> ignore

        cmd.Parameters.AddWithValue("@PrefixValue", "!")
        |> ignore

        cmd.Parameters.AddWithValue("@EmotionCoolDown", "EmotionCoolDown")
        |> ignore

        cmd.Parameters.AddWithValue("@EmotionCoolDownValue", "60")
        |> ignore

        cmd.Parameters.AddWithValue("@Toggle", "Toggle")
        |> ignore

        cmd.Parameters.AddWithValue("@ToggleValue", "true")
        |> ignore

        cmd.Parameters.AddWithValue("@EmotionToggle", "EmotionToggle")
        |> ignore

        cmd.Parameters.AddWithValue("@EmotionToggleValue", "true")
        |> ignore

        try
            let code = cmd.ExecuteNonQuery()

            Logger.Log.TraceInf
            <| sprintf "ChannelSettingsTable for %s is successful init Code:%s" channel.String (string code)

            Ok
            <| sprintf "ChannelSettingsTable for %s is successful init Code:%s" channel.String (string code)
        with :? SQLiteException as eX ->

            Logger.Log.TraceErr
            <| sprintf "ChannelSettingsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

            Logger.Log.TraceExc
            <| sprintf "ChannelSettingsTable SQLiteException EX: %A" eX.StackTrace

            Error
            <| sprintf "ChannelSettingsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

    member private self.checkChannelCommand (channel: Channels) (command: ChannelCommand) =
        let cmd =
            sprintf @"select CMD from %sCommands where CMD = (@command)" channel.String


        Logger.Log.TraceInf
        <| sprintf "Start check %s channel command %s" channel.String command.chCommand

        Logger.Log.TraceDeb
        <| sprintf "checkChannelCommand %s P: %s" channel.String cmd

        use cmd = new SQLiteCommand(cmd, connection)

        cmd.Parameters.AddWithValue("@command", command.chCommand)
        |> ignore

        try
            let resp = cmd.ExecuteScalar()

            if resp <> null then
                Logger.Log.TraceInf
                <| sprintf "%s channel command %s is exist Code%s" channel.String command.chCommand (string resp)

                Ok true
            else
                Logger.Log.TraceInf
                <| sprintf "%s channel command %s is not exist Code%s" channel.String command.chCommand (string resp)

                Ok false
        with :? SQLiteException as eX ->

            Logger.Log.TraceErr
            <| sprintf "checkChannelCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

            Logger.Log.TraceExc
            <| sprintf "checkChannelCommand SQLiteException EX: %A" eX.StackTrace

            Error
            <| sprintf "checkChannelCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

    member self.createChannelSettingsTable(channel: Channels) =
        let cmd =
            sprintf
                @"create table if not exists %sSettings (Setting text not null primary key, Value text)"
                channel.String

        Logger.Log.TraceInf
        <| sprintf "Start create ChannelSettingsTable for %s channel" channel.String

        Logger.Log.TraceDeb
        <| sprintf "createChannelSettingsTable %s P: %s" channel.String cmd

        use cmd = new SQLiteCommand(cmd, connection)

        try
            let resp = cmd.ExecuteNonQuery()

            if resp = -1 then
                Logger.Log.TraceWarn
                <| sprintf "ChannelSettingsTable for %s is already created Code%s" channel.String (string resp)

                Ok
                <| sprintf "ChannelSettingsTable for %s is already created Code%s" channel.String (string resp)
            else
                self.initChannelSettingsTable channel |> ignore

                Logger.Log.TraceInf
                <| sprintf "ChannelSettingsTable for %s is created Code%s" channel.String (string resp)

                Ok
                <| sprintf "ChannelSettingsTable for %s is created Code%s" channel.String (string resp)
        with :? SQLiteException as eX ->

            Logger.Log.TraceErr
            <| sprintf "createChannelSettingsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

            Logger.Log.TraceExc
            <| sprintf "createChannelSettingsTable SQLiteException EX: %A" eX.StackTrace

            Error
            <| sprintf "createChannelSettingsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

    member self.addChannelCommand (channel: Channels) (command: ChannelCommand) =

        Logger.Log.TraceInf
        <| sprintf "Start check command for add ChannelCommand for %s channel" channel.String

        self.checkChannelCommand channel command
        |> function
        | Ok (ok) ->
            if not ok then
                let cmd =
                    sprintf @"insert or ignore into %sCommands (CMD, ANSWER) values (@command, @answer)" channel.String

                Logger.Log.TraceInf
                <| sprintf "Start add ChannelCommand for %s channel" channel.String

                Logger.Log.TraceDeb
                <| sprintf "addChannelCommand %s P: %s %s" channel.String command.chAnswer command.chCommand

                use cmd = new SQLiteCommand(cmd, connection)

                cmd.Parameters.AddWithValue("@command", command.chCommand)
                |> ignore

                cmd.Parameters.AddWithValue("@answer", command.chAnswer)
                |> ignore

                try

                    let code = cmd.ExecuteNonQuery()

                    Logger.Log.TraceInf
                    <| sprintf
                        "Command %s for channel %s successful created Code%s"
                        command.chCommand
                        channel.String
                        (string code)

                    Ok
                    <| sprintf
                        "Command %s for channel %s successful created Code%s"
                        command.chCommand
                        channel.String
                        (string code)
                with :? SQLiteException as eX ->
                    Logger.Log.TraceErr
                    <| sprintf "createChannelSettingsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

                    Logger.Log.TraceExc
                    <| sprintf "createChannelSettingsTable SQLiteException EX: %A" eX.StackTrace

                    Error
                    <| sprintf "createChannelSettingsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters
            else
                Logger.Log.TraceWarn
                <| sprintf "Command %s for channel %s is already exist" command.chCommand channel.String

                Error
                <| sprintf "Command %s for channel %s is already exist" command.chCommand channel.String
        | Error (err) ->
            Logger.Log.TraceWarn
            <| sprintf "Check command failed for %s channel" channel.String

            Error err

    member self.deleteChannelCommand (channel: Channels) (command: ChannelCommand) =
        Logger.Log.TraceInf
        <| sprintf "Start check command for delete ChannelCommand for %s channel" channel.String

        self.checkChannelCommand channel command
        |> function
        | Ok (ok) ->
            if ok then
                let cmd =
                    sprintf @"delete from %sCommands where CMD = (@command)" channel.String

                Logger.Log.TraceInf
                <| sprintf "Start delete ChannelCommand for %s channel" channel.String

                Logger.Log.TraceDeb
                <| sprintf "deleteChannelCommand %s P: %s %s" channel.String command.chAnswer command.chCommand

                use cmd = new SQLiteCommand(cmd, connection)

                cmd.Parameters.AddWithValue("@command", command.chCommand)
                |> ignore

                try
                    let code = cmd.ExecuteNonQuery()

                    Logger.Log.TraceInf
                    <| sprintf
                        "Command %s for channel %s successful delete Code%s"
                        command.chCommand
                        channel.String
                        (string code)

                    Ok
                    <| sprintf
                        "Command %s for channel %s successful delete Code%s"
                        command.chCommand
                        channel.String
                        (string code)
                with :? SQLiteException as eX ->

                    Logger.Log.TraceErr
                    <| sprintf "deleteChannelCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

                    Logger.Log.TraceExc
                    <| sprintf "deleteChannelCommand SQLiteException EX: %A" eX.StackTrace

                    Error
                    <| sprintf "deleteChannelCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters
            else
                Logger.Log.TraceWarn
                <| sprintf "Command %s for channel %s nor found" command.chCommand channel.String

                Ok
                <| sprintf "Command %s for channel %s nor found" command.chCommand channel.String
        | Error (err) ->
            Logger.Log.TraceWarn
            <| sprintf "Check command failed for %s channel" channel.String

            Error err


    member self.updateChannelCommand (channel: Channels) (command: ChannelCommand) =
        Logger.Log.TraceInf
        <| sprintf "Start check command for update ChannelCommand for %s channel" channel.String

        self.checkChannelCommand channel command
        |> function
        | Ok (ok) ->
            if ok then
                let cmd =
                    sprintf @"update %sCommands set ANSWER = (@value) where CMD = (@command)" channel.String

                Logger.Log.TraceInf
                <| sprintf "Start update ChannelCommand for %s channel" channel.String

                Logger.Log.TraceDeb
                <| sprintf "updateChannelCommand %s P: %s %s" channel.String command.chAnswer command.chCommand

                use cmd = new SQLiteCommand(cmd, connection)

                cmd.Parameters.AddWithValue("@value", command.chAnswer)
                |> ignore

                cmd.Parameters.AddWithValue("@command", command.chCommand)
                |> ignore

                try
                    let code = cmd.ExecuteNonQuery()

                    Logger.Log.TraceInf
                    <| sprintf
                        "Command %s for channel %s successful update Code%s"
                        command.chCommand
                        channel.String
                        (string code)

                    Ok
                    <| sprintf
                        "Command %s for channel %s successful update Code%s"
                        command.chCommand
                        channel.String
                        (string code)
                with :? SQLiteException as eX ->

                    Logger.Log.TraceErr
                    <| sprintf "updateChannelCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

                    Logger.Log.TraceExc
                    <| sprintf "updateChannelCommand SQLiteException EX: %A" eX.StackTrace

                    Error
                    <| sprintf "updateChannelCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters
            else
                Logger.Log.TraceWarn
                <| sprintf "Command %s for channel %s nor found" command.chCommand channel.String

                Ok
                <| sprintf "Command %s for channel %s nor found" command.chCommand channel.String
        | Error (err) ->
            Logger.Log.TraceWarn
            <| sprintf "Check command failed for %s channel" channel.String

            Error err

    member self.setChannelSetting (channel: Channels) (setting: ChannelSetting) =
        let execute settingName =
            let cmd =
                sprintf @"update %sSettings set Value = (@value) where Setting = (@setting)" channel.String

            Logger.Log.TraceInf
            <| sprintf "Start set ChannelSetting for %s channel" channel.String

            Logger.Log.TraceDeb
            <| sprintf "setChannelSetting %s %s to %s" channel.String settingName setting.chValue

            use cmd = new SQLiteCommand(cmd, connection)

            cmd.Parameters.AddWithValue("@value", setting.chValue)
            |> ignore

            cmd.Parameters.AddWithValue("@setting", settingName)
            |> ignore

            try
                let resp = cmd.ExecuteNonQuery()

                if resp = 0 then
                    Logger.Log.TraceWarn
                    <| sprintf
                        "ChannelSetting %s for %s channel not found Code%s"
                        settingName
                        channel.String
                        (string resp)

                    Ok
                    <| sprintf
                        "ChannelSetting %s for %s channel not found Code%s"
                        settingName
                        channel.String
                        (string resp)
                else
                    Logger.Log.TraceInf
                    <| sprintf
                        "ChannelSetting %s for %s channel successful updated Code%s"
                        settingName
                        channel.String
                        (string resp)

                    Ok
                    <| sprintf
                        "ChannelSetting %s for %s channel successful updated Code%s"
                        settingName
                        channel.String
                        (string resp)
            with :? SQLiteException as eX ->

                Logger.Log.TraceErr
                <| sprintf "setChannelSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

                Logger.Log.TraceExc
                <| sprintf "setChannelSetting SQLiteException EX: %A" eX.StackTrace

                Error
                <| sprintf "setChannelSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

        Logger.Log.TraceInf
        <| sprintf "Start match ChannelSetting for %s channel" channel.String

        match setting.chSetting with
        | ActivePatterns.Prefix settingName -> execute settingName
        | ActivePatterns.EmotionCoolDown settingName -> execute settingName
        | ActivePatterns.Toggle settingName -> execute settingName
        | ActivePatterns.EmotionToggle settingName -> execute settingName

    member self.getChannelSetting (channel: Channels) (setting: ChannelSettings) =
        let execute settingName =
            let cmd =
                sprintf @"select * from %sSettings where Setting = (@setting)" channel.String

            Logger.Log.TraceInf
            <| sprintf "Start get ChannelSetting for %s channel" channel.String

            Logger.Log.TraceDeb
            <| sprintf "getChannelSetting %s %s" channel.String settingName

            use cmd = new SQLiteCommand(cmd, connection)

            cmd.Parameters.AddWithValue("@setting", settingName)
            |> ignore

            try
                let reader = cmd.ExecuteReader()


                if reader.Read() then

                    Logger.Log.TraceInf
                    <| sprintf "Get ChannelSetting for %s channel successful" channel.String

                    Ok
                    <| match setting with
                       | Prefix _ ->
                           { chSetting = ChannelSettings.Prefix
                             chValue = reader.["value"].ToString() }
                       | EmotionCoolDown _ ->
                           { chSetting = ChannelSettings.EmotionCoolDown
                             chValue = reader.["value"].ToString() }
                       | Toggle _ ->
                           { chSetting = ChannelSettings.Toggle
                             chValue = reader.["value"].ToString() }
                       | EmotionToggle _ ->
                           { chSetting = ChannelSettings.EmotionToggle
                             chValue = reader.["value"].ToString() }
                else

                    Logger.Log.TraceWarn
                    <| sprintf "Get ChannelSetting %s for %s channel not found" settingName channel.String

                    Error
                    <| sprintf "Get ChannelSetting %s for %s channel not found" settingName channel.String
            with :? SQLiteException as eX ->

                Logger.Log.TraceErr
                <| sprintf "getChannelSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

                Logger.Log.TraceExc
                <| sprintf "getChannelSetting SQLiteException EX: %A" eX.StackTrace

                Error
                <| sprintf "getChannelSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

        Logger.Log.TraceInf
        <| sprintf "Start match ChannelSetting for %s channel" channel.String

        match setting with
        | ActivePatterns.Prefix settingName -> execute settingName
        | ActivePatterns.EmotionCoolDown settingName -> execute settingName
        | ActivePatterns.Toggle settingName -> execute settingName
        | ActivePatterns.EmotionToggle settingName -> execute settingName

    member self.getCommand (channel: Channels) (command: string) =
        let cmd =
            sprintf @"select * from %sCommands where CMD = (@command)" channel.String

        Logger.Log.TraceInf
        <| sprintf "Start getCommand %s for %s channel" command channel.String

        Logger.Log.TraceDeb
        <| sprintf "getCommand %s %s" channel.String cmd

        use cmd = new SQLiteCommand(cmd, connection)

        cmd.Parameters.AddWithValue("@command", command)
        |> ignore

        try
            let reader = cmd.ExecuteReader()

            if reader.Read() then
                Logger.Log.TraceInf
                <| sprintf "Get Command %s for %s channel successful" command channel.String

                Ok
                <| { chCommand = reader.["CMD"].ToString()
                     chAnswer = reader.["ANSWER"].ToString() }
            else
                Logger.Log.TraceWarn
                <| sprintf "Command %s for %s channel not found" command channel.String

                Error
                <| sprintf "Command %s for %s channel not found" command channel.String
        with :? SQLiteException as eX ->

            Logger.Log.TraceErr
            <| sprintf "getCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

            Logger.Log.TraceExc
            <| sprintf "getCommand SQLiteException EX: %A" eX.StackTrace

            Error
            <| sprintf "getCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

    member self.getCommands(channel: Channels) =
        let cmd =
            sprintf @"select * from %sCommands" channel.String

        Logger.Log.TraceInf
        <| sprintf "Start getCommands for %s channel" channel.String

        Logger.Log.TraceDeb
        <| sprintf "getCommands %s %s" channel.String cmd

        use cmd = new SQLiteCommand(cmd, connection)

        try
            let reader = cmd.ExecuteReader()

            if not reader.IsClosed then
                Logger.Log.TraceInf
                <| sprintf "Get Commands for %s channel successful" channel.String

                Ok
                <| [ while reader.Read() do

                         yield
                             { chCommand = reader.["CMD"].ToString()
                               chAnswer = reader.["ANSWER"].ToString() } ]
            else
                Logger.Log.TraceWarn
                <| sprintf "Commands for %s channel not found" channel.String

                Error
                <| sprintf "Commands for %s channel not found" channel.String
        with :? SQLiteException as eX ->

            Logger.Log.TraceErr
            <| sprintf "getCommands SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

            Logger.Log.TraceExc
            <| sprintf "getCommands SQLiteException EX: %A" eX.StackTrace

            Error
            <| sprintf "getCommands SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

    member self.setBotSetting(setting: BotSetting) =
        let execute settingName =
            let cmd =
                sprintf @"update BotSettings set Value = (@value) where Setting = (@setting)"

            Logger.Log.TraceInf
            <| sprintf "Start set BotSetting %s" settingName

            Logger.Log.TraceDeb
            <| sprintf "setBotSetting %s" cmd

            use cmd = new SQLiteCommand(cmd, connection)

            cmd.Parameters.AddWithValue("@value", setting.bValue)
            |> ignore

            cmd.Parameters.AddWithValue("@setting", settingName)
            |> ignore

            try
                let resp = cmd.ExecuteNonQuery()

                if resp = 0 then
                    Logger.Log.TraceWarn
                    <| sprintf "BotSetting %s not found" settingName

                    Ok
                    <| sprintf "BotSetting %s not found" settingName
                else
                    Logger.Log.TraceWarn
                    <| sprintf "BotSetting %s successful updated" settingName

                    Ok
                    <| sprintf "BotSetting %s successful updated" settingName
            with :? SQLiteException as eX ->

                Logger.Log.TraceErr
                <| sprintf "setBotSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

                Logger.Log.TraceExc
                <| sprintf "setBotSetting SQLiteException EX: %A" eX.StackTrace

                Error
                <| sprintf "setBotSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

        Logger.Log.TraceInf
        <| sprintf "Start match BotSetting for set"

        match setting.bSetting with
        | ActivePatterns.APIBearerToken settingName -> execute settingName

    member self.getBotSetting(setting: BotSettings) =
        let execute settingName =
            let cmd =
                sprintf @"select * from BotSettings where Setting = (@setting)"

            Logger.Log.TraceInf
            <| sprintf "Start get BotSetting %s" settingName

            Logger.Log.TraceDeb
            <| sprintf "getBotSetting %s" cmd

            use cmd = new SQLiteCommand(cmd, connection)

            cmd.Parameters.AddWithValue("@setting", settingName) |> string |> Logger.Log.TraceDeb
            

            try
                let reader = cmd.ExecuteReader()

                if reader.Read() then

                    Logger.Log.TraceInf
                    <| sprintf "BotSetting %s successful get" settingName

                    Ok
                    <| match setting with
                       | APIBearerToken _ ->
                           { bSetting = BotSettings.APIBearerToken
                             bValue = reader.["value"].ToString() }
                else
                    Logger.Log.TraceWarn
                    <| sprintf "BotSetting %s not found" settingName

                    Error
                    <| sprintf "BotSetting %s not found" settingName
            with :? SQLiteException as eX ->
                Logger.Log.TraceWarn
                <| sprintf "getBotSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

                Logger.Log.TraceExc
                <| sprintf "getBotSetting SQLiteException EX: %A" eX.StackTrace

                Error
                <| sprintf "getBotSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters

        match setting with
        | ActivePatterns.APIBearerToken settingName -> execute settingName
