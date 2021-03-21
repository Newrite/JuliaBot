module TWBot.DataBase

open ActivePatterns
open TypesDefinition
open System.Data.SQLite



type SingleData private () =
    static let dataBase = SingleData()
    let databaseFilename = "data.sqlite3"

    let connectionStringFile =
        Logger.Log.StartTrace(
            sprintf "Connection to DB Data Source=%s;Version=3" databaseFilename,
            Logger.LogLevel.Information
        )

        sprintf "Data Source=%s;Version=3;" databaseFilename

    let connection =
        Logger.Log.StartTrace("Start to connection database", Logger.LogLevel.Information)
        new SQLiteConnection(connectionStringFile)

    do connection.Open()
    static member DB = dataBase

    member private self.initBotSettingsTable() =
        let cmd =
            @"insert or ignore into BotSettings (Setting, Value) values"
            + "(@APIBearerToken, @APIBearerTokenValue)"

        Logger.Log.StartTrace(sprintf "Start init botSettings", Logger.LogLevel.Information)
        Logger.Log.StartTrace(sprintf "botSettings %s" cmd, Logger.LogLevel.Debug)
        use cmd = new SQLiteCommand(cmd, connection)

        cmd.Parameters.AddWithValue("@APIBearerToken", "APIBearerToken")
        |> ignore

        cmd.Parameters.AddWithValue("@APIBearerTokenValue", "None")
        |> ignore

        try
            let resp = cmd.ExecuteNonQuery()

            if resp = -1 then
                Logger.Log.StartTrace(
                    sprintf "BotSettings is already created Code:%s" (string resp),
                    Logger.LogLevel.Information
                )

                Ok(sprintf "BotSettings is already created Code:%s" (string resp))
            else
                Logger.Log.StartTrace(
                    sprintf "BotSettings is successful created Code:%s" (string resp),
                    Logger.LogLevel.Information
                )

                Ok(sprintf "BotSettings is succeful created Code:%s" (string resp))
        with :? SQLiteException as eX ->
            Logger.Log.StartTrace(
                (sprintf "BotSettings SQLiteException Err:%s P: %A" eX.Message cmd.Parameters),
                Logger.LogLevel.Error
            )

            Logger.Log.StartTrace(
                (sprintf "BotSettings SQLiteException EX: %A" eX.StackTrace),
                Logger.LogLevel.Exception
            )

            Error(sprintf "BotSettings SQLiteException Err:%s P: %A" eX.Message cmd.Parameters)

    member self.createBotSettingsTable() =
        let cmd =
            "create table if not exists BotSettings (Setting text not null primary key, Value text)"

        Logger.Log.StartTrace(sprintf "Start create BotSettingsTable", Logger.LogLevel.Information)
        Logger.Log.StartTrace((sprintf "BotSettingsTable %s" cmd), Logger.LogLevel.Debug)
        use cmd = new SQLiteCommand(cmd, connection)

        try
            let resp = cmd.ExecuteNonQuery()

            if resp = -1 then
                Logger.Log.StartTrace(
                    (sprintf "BotSettingsTable is already created Code:%s" (string resp)),
                    Logger.LogLevel.Information
                )

                Ok(sprintf "BotSettingsTable is already created Code:%s" (string resp))
            else
                Logger.Log.StartTrace(
                    (sprintf "BotSettingsTable is successful created Code:%s" (string resp)),
                    Logger.LogLevel.Information
                )

                self.initBotSettingsTable () |> ignore
                Ok(sprintf "BotSettingsTable is successful created Code:%s" (string resp))
        with :? SQLiteException as eX ->
            Logger.Log.StartTrace(
                (sprintf "CreateBotSettings SQLiteException Err:%s P: %A" eX.Message cmd.Parameters),
                Logger.LogLevel.Error
            )

            Logger.Log.StartTrace(
                (sprintf "CreateBotSettings SQLiteException EX: %A" eX.StackTrace),
                Logger.LogLevel.Exception
            )

            Error(sprintf "CreateBotSettings SQLiteException Err:%s P: %A" eX.Message cmd.Parameters)

    member self.createChannelCommandsTable(channel: Channels) =
        let cmd =
            sprintf @"create table if not exists %sCommands (CMD text not null primary key, ANSWER text)" channel.String

        Logger.Log.StartTrace(
            (sprintf "Start create ChannelCommandsTable for %s" channel.String),
            Logger.LogLevel.Information
        )

        Logger.Log.StartTrace((sprintf "ChannelCommandsTable %s %s" channel.String cmd), Logger.LogLevel.Debug)

        use cmd = new SQLiteCommand(cmd, connection)

        try
            let resp = cmd.ExecuteNonQuery()

            if resp = -1 then
                Logger.Log.StartTrace(
                    (sprintf "ChannelCommandsTable for %s is already created Code:%s" channel.String (string resp)),
                    Logger.LogLevel.Information
                )

                Ok(sprintf "ChannelCommandsTable for %s is already created Code:%s" channel.String (string resp))
            else
                Logger.Log.StartTrace(
                    (sprintf "ChannelCommandsTable for %s is successful created Code:%s" channel.String (string resp)),
                    Logger.LogLevel.Information
                )

                Ok(sprintf "ChannelCommandsTable for %s is successful created Code:%s" channel.String (string resp))
        with :? SQLiteException as eX ->
            Logger.Log.StartTrace(
                (sprintf "ChannelCommandsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters),
                Logger.LogLevel.Error
            )

            Logger.Log.StartTrace(
                (sprintf "ChannelCommandsTable SQLiteException EX: %A" eX.StackTrace),
                Logger.LogLevel.Exception
            )

            Error(sprintf "ChannelCommandsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters)

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


        Logger.Log.StartTrace(
            (sprintf "Start init ChannelSettingsTable for %s" channel.String),
            Logger.LogLevel.Information
        )

        Logger.Log.StartTrace((sprintf "ChannelSettingsTable %s P: %s" channel.String cmd), Logger.LogLevel.Debug)

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

            Logger.Log.StartTrace(
                (sprintf "ChannelSettingsTable for %s is successful init Code:%s" channel.String (string code)),
                Logger.LogLevel.Information
            )

            Ok(sprintf "ChannelSettingsTable for %s is successful init Code:%s" channel.String (string code))
        with :? SQLiteException as eX ->

            Logger.Log.StartTrace(
                (sprintf "ChannelSettingsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters),
                Logger.LogLevel.Error
            )

            Logger.Log.StartTrace(
                (sprintf "ChannelSettingsTable SQLiteException EX: %A" eX.StackTrace),
                Logger.LogLevel.Exception
            )

            Error(sprintf "ChannelSettingsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters)

    member private self.checkChannelCommand (channel: Channels) (command: ChannelCommand) =
        let cmd =
            sprintf @"select CMD from %sCommands where CMD = (@command)" channel.String


        Logger.Log.StartTrace(
            (sprintf "Start check %s channel command %s" channel.String command.chCommand),
            Logger.LogLevel.Information
        )

        Logger.Log.StartTrace((sprintf "checkChannelCommand %s P: %s" channel.String cmd), Logger.LogLevel.Debug)

        use cmd = new SQLiteCommand(cmd, connection)

        cmd.Parameters.AddWithValue("@command", command.chCommand)
        |> ignore

        try
            let resp = cmd.ExecuteScalar()

            if resp <> null then
                Logger.Log.StartTrace(
                    (sprintf "%s channel command %s is exist Code%s" channel.String command.chCommand (string resp)),
                    Logger.LogLevel.Information
                )

                Ok(true)
            else
                Logger.Log.StartTrace(
                    (sprintf "%s channel command %s is not exist Code%s" channel.String command.chCommand (string resp)),
                    Logger.LogLevel.Information
                )

                Ok(false)
        with :? SQLiteException as eX ->

            Logger.Log.StartTrace(
                (sprintf "checkChannelCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters),
                Logger.LogLevel.Error
            )

            Logger.Log.StartTrace(
                (sprintf "checkChannelCommand SQLiteException EX: %A" eX.StackTrace),
                Logger.LogLevel.Exception
            )

            Error(sprintf "checkChannelCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters)

    member self.createChannelSettingsTable(channel: Channels) =
        let cmd =
            sprintf
                @"create table if not exists %sSettings (Setting text not null primary key, Value text)"
                channel.String

        Logger.Log.StartTrace(
            (sprintf "Start create ChannelSettingsTable for %s channel" channel.String),
            Logger.LogLevel.Information
        )

        Logger.Log.StartTrace((sprintf "createChannelSettingsTable %s P: %s" channel.String cmd), Logger.LogLevel.Debug)

        use cmd = new SQLiteCommand(cmd, connection)

        try
            let resp = cmd.ExecuteNonQuery()

            if resp = -1 then
                Logger.Log.StartTrace(
                    (sprintf "ChannelSettingsTable for %s is already created Code%s" channel.String (string resp)),
                    Logger.LogLevel.Warning
                )

                Ok(sprintf "ChannelSettingsTable for %s is already created Code%s" channel.String (string resp))
            else
                self.initChannelSettingsTable channel |> ignore

                Logger.Log.StartTrace(
                    (sprintf "ChannelSettingsTable for %s is created Code%s" channel.String (string resp)),
                    Logger.LogLevel.Information
                )

                Ok(sprintf "ChannelSettingsTable for %s is created Code%s" channel.String (string resp))
        with :? SQLiteException as eX ->

            Logger.Log.StartTrace(
                (sprintf "createChannelSettingsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters),
                Logger.LogLevel.Error
            )

            Logger.Log.StartTrace(
                (sprintf "createChannelSettingsTable SQLiteException EX: %A" eX.StackTrace),
                Logger.LogLevel.Exception
            )

            Error(sprintf "createChannelSettingsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters)

    member self.addChannelCommand (channel: Channels) (command: ChannelCommand) =

        Logger.Log.StartTrace(
            (sprintf "Start check command for add ChannelCommand for %s channel" channel.String),
            Logger.LogLevel.Information
        )

        self.checkChannelCommand channel command
        |> function
        | Ok (ok) ->
            if not ok then
                let cmd =
                    sprintf @"insert or ignore into %sCommands (CMD, ANSWER) values (@command, @answer)" channel.String

                Logger.Log.StartTrace(
                    (sprintf "Start add ChannelCommand for %s channel" channel.String),
                    Logger.LogLevel.Information
                )

                Logger.Log.StartTrace(
                    (sprintf "addChannelCommand %s P: %s %s" channel.String command.chAnswer command.chCommand),
                    Logger.LogLevel.Debug
                )

                use cmd = new SQLiteCommand(cmd, connection)

                cmd.Parameters.AddWithValue("@command", command.chCommand)
                |> ignore

                cmd.Parameters.AddWithValue("@answer", command.chAnswer)
                |> ignore

                try

                    let code = cmd.ExecuteNonQuery()

                    Logger.Log.StartTrace(
                        (sprintf
                            "Command %s for channel %s successful created Code%s"
                            command.chCommand
                            channel.String
                            (string code)),
                        Logger.LogLevel.Information
                    )

                    Ok(
                        sprintf
                            "Command %s for channel %s successful created Code%s"
                            command.chCommand
                            channel.String
                            (string code)
                    )
                with :? SQLiteException as eX ->
                    Logger.Log.StartTrace(
                        (sprintf "createChannelSettingsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters),
                        Logger.LogLevel.Error
                    )

                    Logger.Log.StartTrace(
                        (sprintf "createChannelSettingsTable SQLiteException EX: %A" eX.StackTrace),
                        Logger.LogLevel.Exception
                    )

                    Error(sprintf "createChannelSettingsTable SQLiteException Err:%s P: %A" eX.Message cmd.Parameters)
            else
                Logger.Log.StartTrace(
                    (sprintf "Command %s for channel %s is already exist" command.chCommand channel.String),
                    Logger.LogLevel.Warning
                )

                Error(sprintf "Command %s for channel %s is already exist" command.chCommand channel.String)
        | Error (err) ->
            Logger.Log.StartTrace(
                (sprintf "Check command failed for %s channel" channel.String),
                Logger.LogLevel.Warning
            )

            Error err

    member self.deleteChannelCommand (channel: Channels) (command: ChannelCommand) =
        Logger.Log.StartTrace(
            (sprintf "Start check command for delete ChannelCommand for %s channel" channel.String),
            Logger.LogLevel.Information
        )

        self.checkChannelCommand channel command
        |> function
        | Ok (ok) ->
            if ok then
                let cmd =
                    sprintf @"delete from %sCommands where CMD = (@command)" channel.String

                Logger.Log.StartTrace(
                    (sprintf "Start delete ChannelCommand for %s channel" channel.String),
                    Logger.LogLevel.Information
                )

                Logger.Log.StartTrace(
                    (sprintf "deleteChannelCommand %s P: %s %s" channel.String command.chAnswer command.chCommand),
                    Logger.LogLevel.Debug
                )

                use cmd = new SQLiteCommand(cmd, connection)

                cmd.Parameters.AddWithValue("@command", command.chCommand)
                |> ignore

                try
                    let code = cmd.ExecuteNonQuery()

                    Logger.Log.StartTrace(
                        (sprintf
                            "Command %s for channel %s successful delete Code%s"
                            command.chCommand
                            channel.String
                            (string code)),
                        Logger.LogLevel.Information
                    )

                    Ok(
                        sprintf
                            "Command %s for channel %s successful delete Code%s"
                            command.chCommand
                            channel.String
                            (string code)
                    )
                with :? SQLiteException as eX ->

                    Logger.Log.StartTrace(
                        (sprintf "deleteChannelCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters),
                        Logger.LogLevel.Error
                    )

                    Logger.Log.StartTrace(
                        (sprintf "deleteChannelCommand SQLiteException EX: %A" eX.StackTrace),
                        Logger.LogLevel.Exception
                    )

                    Error(sprintf "deleteChannelCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters)
            else
                Logger.Log.StartTrace(
                    (sprintf "Command %s for channel %s nor found" command.chCommand channel.String),
                    Logger.LogLevel.Warning
                )

                Ok(sprintf "Command %s for channel %s nor found" command.chCommand channel.String)
        | Error (err) ->
            Logger.Log.StartTrace(
                (sprintf "Check command failed for %s channel" channel.String),
                Logger.LogLevel.Warning
            )

            Error err


    member self.updateChannelCommand (channel: Channels) (command: ChannelCommand) =
        Logger.Log.StartTrace(
            (sprintf "Start check command for update ChannelCommand for %s channel" channel.String),
            Logger.LogLevel.Information
        )

        self.checkChannelCommand channel command
        |> function
        | Ok (ok) ->
            if ok then
                let cmd =
                    sprintf @"update %sCommands set ANSWER = (@value) where CMD = (@command)" channel.String

                Logger.Log.StartTrace(
                    (sprintf "Start update ChannelCommand for %s channel" channel.String),
                    Logger.LogLevel.Information
                )

                Logger.Log.StartTrace(
                    (sprintf "updateChannelCommand %s P: %s %s" channel.String command.chAnswer command.chCommand),
                    Logger.LogLevel.Debug
                )

                use cmd = new SQLiteCommand(cmd, connection)

                cmd.Parameters.AddWithValue("@value", command.chAnswer)
                |> ignore

                cmd.Parameters.AddWithValue("@command", command.chCommand)
                |> ignore

                try
                    let code = cmd.ExecuteNonQuery()

                    Logger.Log.StartTrace(
                        (sprintf
                            "Command %s for channel %s successful update Code%s"
                            command.chCommand
                            channel.String
                            (string code)),
                        Logger.LogLevel.Information
                    )

                    Ok(
                        sprintf
                            "Command %s for channel %s successful update Code%s"
                            command.chCommand
                            channel.String
                            (string code)
                    )
                with :? SQLiteException as eX ->

                    Logger.Log.StartTrace(
                        (sprintf "updateChannelCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters),
                        Logger.LogLevel.Error
                    )

                    Logger.Log.StartTrace(
                        (sprintf "updateChannelCommand SQLiteException EX: %A" eX.StackTrace),
                        Logger.LogLevel.Exception
                    )

                    Error(sprintf "updateChannelCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters)
            else
                Logger.Log.StartTrace(
                    (sprintf "Command %s for channel %s nor found" command.chCommand channel.String),
                    Logger.LogLevel.Warning
                )

                Ok(sprintf "Command %s for channel %s nor found" command.chCommand channel.String)
        | Error (err) ->
            Logger.Log.StartTrace(
                (sprintf "Check command failed for %s channel" channel.String),
                Logger.LogLevel.Warning
            )

            Error err

    member self.setChannelSetting (channel: Channels) (setting: ChannelSetting) =
        let execute settingName =
            let cmd =
                sprintf @"update %sSettings set Value = (@value) where Setting = (@setting)" channel.String

            Logger.Log.StartTrace(
                (sprintf "Start set ChannelSetting for %s channel" channel.String),
                Logger.LogLevel.Information
            )

            Logger.Log.StartTrace(
                (sprintf "setChannelSetting %s %s to %s" channel.String settingName setting.chValue),
                Logger.LogLevel.Debug
            )

            use cmd = new SQLiteCommand(cmd, connection)

            cmd.Parameters.AddWithValue("@value", setting.chValue)
            |> ignore

            cmd.Parameters.AddWithValue("@setting", settingName)
            |> ignore

            try
                let resp = cmd.ExecuteNonQuery()

                if resp = 0 then
                    Logger.Log.StartTrace(
                        (sprintf
                            "ChannelSetting %s for %s channel not found Code%s"
                            settingName
                            channel.String
                            (string resp)),
                        Logger.LogLevel.Warning
                    )

                    Ok(
                        sprintf
                            "ChannelSetting %s for %s channel not found Code%s"
                            settingName
                            channel.String
                            (string resp)
                    )
                else
                    Logger.Log.StartTrace(
                        (sprintf
                            "ChannelSetting %s for %s channel successful updated Code%s"
                            settingName
                            channel.String
                            (string resp)),
                        Logger.LogLevel.Information
                    )

                    Ok(
                        sprintf
                            "ChannelSetting %s for %s channel successful updated Code%s"
                            settingName
                            channel.String
                            (string resp)
                    )
            with :? SQLiteException as eX ->

                Logger.Log.StartTrace(
                    (sprintf "setChannelSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters),
                    Logger.LogLevel.Error
                )

                Logger.Log.StartTrace(
                    (sprintf "setChannelSetting SQLiteException EX: %A" eX.StackTrace),
                    Logger.LogLevel.Exception
                )

                Error(sprintf "setChannelSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters)

        Logger.Log.StartTrace(
            (sprintf "Start match ChannelSetting for %s channel" channel.String),
            Logger.LogLevel.Information
        )

        match setting.chSetting with
        | ActivePatterns.Prefix settingName -> execute settingName
        | ActivePatterns.EmotionCoolDown settingName -> execute settingName
        | ActivePatterns.Toggle settingName -> execute settingName
        | ActivePatterns.EmotionToggle settingName -> execute settingName

    member self.getChannelSetting (channel: Channels) (setting: ChannelSettings) =
        let execute settingName =
            let cmd =
                sprintf @"select * from %sSettings where Setting = (@setting)" channel.String

            Logger.Log.StartTrace(
                (sprintf "Start get ChannelSetting for %s channel" channel.String),
                Logger.LogLevel.Information
            )

            Logger.Log.StartTrace((sprintf "getChannelSetting %s %s" channel.String settingName), Logger.LogLevel.Debug)

            use cmd = new SQLiteCommand(cmd, connection)

            cmd.Parameters.AddWithValue("@setting", settingName)
            |> ignore

            try
                let reader = cmd.ExecuteReader()


                if reader.Read() then

                    Logger.Log.StartTrace(
                        (sprintf "Get ChannelSetting for %s channel successful" channel.String),
                        Logger.LogLevel.Information
                    )

                    Ok(
                        match setting with
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
                    )
                else

                    Logger.Log.StartTrace(
                        (sprintf "Get ChannelSetting %s for %s channel not found" settingName channel.String),
                        Logger.LogLevel.Warning
                    )

                    Error(sprintf "Get ChannelSetting %s for %s channel not found" settingName channel.String)
            with :? SQLiteException as eX ->

                Logger.Log.StartTrace(
                    (sprintf "getChannelSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters),
                    Logger.LogLevel.Error
                )

                Logger.Log.StartTrace(
                    (sprintf "getChannelSetting SQLiteException EX: %A" eX.StackTrace),
                    Logger.LogLevel.Exception
                )

                Error(sprintf "getChannelSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters)

        Logger.Log.StartTrace(
            (sprintf "Start match ChannelSetting for %s channel" channel.String),
            Logger.LogLevel.Information
        )

        match setting with
        | ActivePatterns.Prefix settingName -> execute settingName
        | ActivePatterns.EmotionCoolDown settingName -> execute settingName
        | ActivePatterns.Toggle settingName -> execute settingName
        | ActivePatterns.EmotionToggle settingName -> execute settingName

    member self.getCommand (channel: Channels) (command: string) =
        let cmd =
            sprintf @"select * from %sCommands where CMD = (@command)" channel.String

        Logger.Log.StartTrace(
            (sprintf "Start getCommand %s for %s channel" command channel.String),
            Logger.LogLevel.Information
        )

        Logger.Log.StartTrace((sprintf "getCommand %s %s" channel.String cmd), Logger.LogLevel.Debug)

        use cmd = new SQLiteCommand(cmd, connection)

        cmd.Parameters.AddWithValue("@command", command)
        |> ignore

        try
            let reader = cmd.ExecuteReader()

            if reader.Read() then
                Logger.Log.StartTrace(
                    (sprintf "Get Command %s for %s channel successful" command channel.String),
                    Logger.LogLevel.Information
                )

                Ok(
                    { chCommand = reader.["CMD"].ToString()
                      chAnswer = reader.["ANSWER"].ToString() }
                )
            else
                Logger.Log.StartTrace(
                    (sprintf "Command %s for %s channel not found" command channel.String),
                    Logger.LogLevel.Warning
                )

                Error(sprintf "Command %s for %s channel not found" command channel.String)
        with :? SQLiteException as eX ->

            Logger.Log.StartTrace(
                (sprintf "getCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters),
                Logger.LogLevel.Error
            )

            Logger.Log.StartTrace(
                (sprintf "getCommand SQLiteException EX: %A" eX.StackTrace),
                Logger.LogLevel.Exception
            )

            Error(sprintf "getCommand SQLiteException Err:%s P: %A" eX.Message cmd.Parameters)

    member self.getCommands(channel: Channels) =
        let cmd =
            sprintf @"select * from %sCommands" channel.String

        Logger.Log.StartTrace((sprintf "Start getCommands for %s channel" channel.String), Logger.LogLevel.Information)

        Logger.Log.StartTrace(sprintf "getCommands %s %s" channel.String cmd, Logger.LogLevel.Debug)

        use cmd = new SQLiteCommand(cmd, connection)

        try
            let reader = cmd.ExecuteReader()

            if not reader.IsClosed then
                Logger.Log.StartTrace(
                    (sprintf "Get Commands for %s channel successful" channel.String),
                    Logger.LogLevel.Information
                )

                Ok(
                    [ while reader.Read() do

                          yield
                              { chCommand = reader.["CMD"].ToString()
                                chAnswer = reader.["ANSWER"].ToString() } ]
                )
            else
                Logger.Log.StartTrace(
                    (sprintf "Commands for %s channel not found" channel.String),
                    Logger.LogLevel.Warning
                )

                Error(sprintf "Commands for %s channel not found" channel.String)
        with :? SQLiteException as eX ->

            Logger.Log.StartTrace(
                (sprintf "getCommands SQLiteException Err:%s P: %A" eX.Message cmd.Parameters),
                Logger.LogLevel.Error
            )

            Logger.Log.StartTrace(
                (sprintf "getCommands SQLiteException EX: %A" eX.StackTrace),
                Logger.LogLevel.Exception
            )

            Error(sprintf "getCommands SQLiteException Err:%s P: %A" eX.Message cmd.Parameters)

    member self.setBotSetting(setting: BotSetting) =
        let execute settingName =
            let cmd =
                sprintf @"update BotSettings set Value = (@value) where Setting = (@setting)"

            Logger.Log.StartTrace((sprintf "Start set BotSetting %s" settingName), Logger.LogLevel.Information)

            Logger.Log.StartTrace((sprintf "setBotSetting %s" cmd), Logger.LogLevel.Debug)

            use cmd = new SQLiteCommand(cmd, connection)

            cmd.Parameters.AddWithValue("@value", setting.bValue)
            |> ignore

            cmd.Parameters.AddWithValue("@setting", settingName)
            |> ignore

            try
                let resp = cmd.ExecuteNonQuery()

                if resp = 0 then
                    Logger.Log.StartTrace((sprintf "BotSetting %s not found" settingName), Logger.LogLevel.Warning)
                    Ok(sprintf "BotSetting %s not found" settingName)
                else
                    Logger.Log.StartTrace(
                        (sprintf "BotSetting %s successful updated" settingName),
                        Logger.LogLevel.Warning
                    )

                    Ok(sprintf "BotSetting %s successful updated" settingName)
            with :? SQLiteException as eX ->

                Logger.Log.StartTrace(
                    (sprintf "setBotSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters),
                    Logger.LogLevel.Error
                )

                Logger.Log.StartTrace(
                    (sprintf "setBotSetting SQLiteException EX: %A" eX.StackTrace),
                    Logger.LogLevel.Exception
                )

                Error(sprintf "setBotSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters)

        Logger.Log.StartTrace((sprintf "Start match BotSetting for set"), Logger.LogLevel.Information)

        match setting.bSetting with
        | ActivePatterns.APIBearerToken settingName -> execute settingName

    member self.getBotSetting(setting: BotSettings) =
        let execute settingName =
            let cmd =
                sprintf @"select * from BotSettings where Setting = (@setting)"

            Logger.Log.StartTrace((sprintf "Start get BotSetting %s" settingName), Logger.LogLevel.Information)

            Logger.Log.StartTrace((sprintf "getBotSetting %s" cmd), Logger.LogLevel.Debug)

            use cmd = new SQLiteCommand(cmd, connection)

            cmd.Parameters.AddWithValue("@setting", settingName)
            |> ignore

            try
                let reader = cmd.ExecuteReader()

                if reader.Read() then

                    Logger.Log.StartTrace(
                        (sprintf "BotSetting %s successful get" settingName),
                        Logger.LogLevel.Information
                    )

                    Ok(
                        match setting with
                        | APIBearerToken _ ->
                            { bSetting = BotSettings.APIBearerToken
                              bValue = reader.["value"].ToString() }
                    )
                else
                    Logger.Log.StartTrace((sprintf "BotSetting %s not found" settingName), Logger.LogLevel.Warning)
                    Error(sprintf "BotSetting %s not found" settingName)
            with :? SQLiteException as eX ->
                Logger.Log.StartTrace(
                    (sprintf "getBotSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters),
                    Logger.LogLevel.Error
                )

                Logger.Log.StartTrace(
                    (sprintf "getBotSetting SQLiteException EX: %A" eX.StackTrace),
                    Logger.LogLevel.Exception
                )

                Error(sprintf "getBotSetting SQLiteException Err:%s P: %A" eX.Message cmd.Parameters)

        match setting with
        | ActivePatterns.APIBearerToken settingName -> execute settingName
