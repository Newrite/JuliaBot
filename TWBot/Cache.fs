module TWBot.Cache

open TWBot

open System
open TypesDefinition
open DataBase

let mutable tempReflyqMessageCounter = 0
let mutable tempKaeliaMessageCounter = 0
let mutable cacheLovers : CacheLove array = Array.empty
let mutable cacheCutDown : CacheCatDown array = Array.empty
let mutable cacheCutDownReward : CacheCatDownReward array = Array.empty
let mutable cacheUsers : CacheUser array = Array.empty
let mutable cacheChannelSettings : (Channels * CacheChannelSettings) array = Array.empty
let mutable cacheChannelCommands : CacheChannelCommands array = Array.empty
let updateCacheChannelSettings (channel: Channels) =
    Logger.Log.TraceInf
    <| sprintf "Update cache channel settings for %s" channel.String
    Logger.Log.TraceDeb
    <| sprintf "Old settings %A" (Array.tryFind (fun elem -> fst elem = channel) cacheChannelSettings)
    let prefix =
        SingleData.DB.getChannelSetting channel ChannelSettings.Prefix
        |> function
        | Ok (pr) -> pr.chValue
        | Error (_) -> "!"
    let emotionCoolDawn =
        SingleData.DB.getChannelSetting channel ChannelSettings.EmotionCoolDown
        |> function
        | Ok (cd) -> Int64.Parse(cd.chValue)
        | Error (_) -> 60L
    let toggle =
        SingleData.DB.getChannelSetting channel ChannelSettings.Toggle
        |> function
        | Ok (tog) -> Boolean.Parse(tog.chValue)
        | Error (_) -> true
    let etoggle =
        SingleData.DB.getChannelSetting channel ChannelSettings.EmotionToggle
        |> function
        | Ok (etog) -> Boolean.Parse(etog.chValue)
        | Error (_) -> true
    cacheChannelSettings <- Array.filter (fun elem -> (fst elem) <> channel) cacheChannelSettings
    cacheChannelSettings <-
        Array.append
            cacheChannelSettings
            [| (channel,
                { Prefix = prefix
                  EmotionCoolDown = emotionCoolDawn
                  Toggle = toggle
                  EmotionToggle = etoggle
                  LastReactTime = (DateTime.Now.Ticks / 10000000L) }) |]
            
    Logger.Log.TraceInf "Update cache channel settings complete"
    Logger.Log.TraceDeb
    <| sprintf "New settings %A" (Array.find (fun elem -> fst elem = channel) cacheChannelSettings)
let addCacheChannelCommands (channel: Channels) =
    Logger.Log.TraceInf
    <| sprintf "Add cache channel commands for %s" channel.String
    SingleData.DB.getCommands channel
    |> function
    | Ok (commands) ->
        Logger.Log.TraceDeb
        <| sprintf "List of adds commands for %s - %A" channel.String commands
        cacheChannelCommands <- Array.append cacheChannelCommands [| { ListCMD = (channel, commands) } |]
    | Error (err) ->
        Logger.Log.TraceWarn
        <| sprintf "Add cache channel getcommands for %s return err: %s" channel.String err
let updateCacheChannelCommands (channel: Channels) =
    Logger.Log.TraceInf
    <| sprintf "Update cache channel commands for %s" channel.String
    SingleData.DB.getCommands channel
    |> function
    | Ok (commands) ->
        Logger.Log.TraceDeb
        <| sprintf "List of updates commands for %s - %A" channel.String commands
        cacheChannelCommands <-
            Array.filter (fun (elem: CacheChannelCommands) -> (fst elem.ListCMD) <> channel) cacheChannelCommands
        cacheChannelCommands <- Array.append cacheChannelCommands [| { ListCMD = (channel, commands) } |]
    | Error (err) ->
        Logger.Log.TraceWarn
        <| sprintf "Update cache channel getcommands for %s return err: %s" channel.String err
let checkCacheLove loverName lovedName =
    Logger.Log.TraceInf "Start check cache love"
    Logger.Log.TraceDeb <| sprintf "checkCacheLove lover: %s loved: %s" loverName lovedName
    Array.tryFind
        (fun (elem: CacheLove) ->
            elem.LovedName = lovedName
            && elem.LoverName = loverName)
        cacheLovers
let addCacheLove loverName lovedName percentLove =
    Logger.Log.TraceInf "Start add cache love"
    Logger.Log.TraceDeb <| sprintf "addCacheLove lover: %s loved: %s percent: %d" loverName lovedName percentLove
    checkCacheLove loverName lovedName
    |> function
    | None ->
        cacheLovers <-
            Array.append
                cacheLovers
                [| { LoverName = loverName
                     LovedName = lovedName
                     TimeWhen = (DateTime.Now.Ticks / 10000000L)
                     PercentLove = percentLove } |]
        Logger.Log.TraceInf "Lovers added to cache"
    | _ -> Logger.Log.TraceInf "Lovers already in cache"
let handleCacheLove () =
    cacheLovers <-
        Array.filter
            (fun (elem: CacheLove) ->
                if ((DateTime.Now.Ticks / 10000000L) - elem.TimeWhen) < 86400L then
                    Logger.Log.TraceInf "Proc filter hanlder cache love"
                    Logger.Log.TraceInf <| sprintf "handleCacheLove elem: %A" elem
                    true
                else false) cacheLovers
let checkToggleChannel channel =
    Array.tryFind (fun (elem: (Channels * CacheChannelSettings)) -> channel = fst elem) cacheChannelSettings
    |> function
    | Some (tup) ->
        //Лог закомментирован, потому что функция вызывается каждую итерацию основного цикла
        //Logger.Log.TraceInf <| sprintf "Check channel toggle for %s: %b" channel.String (snd tup).Toggle
        (snd tup).Toggle
    | None ->
        Logger.Log.TraceWarn <| sprintf "Check channel toggle for %s: No found" channel.String
        false
let checkEmotionToggleChannel channel =
    Array.tryFind (fun (elem: (Channels * CacheChannelSettings)) -> channel = fst elem) cacheChannelSettings
    |> function
    | Some (tup) ->
        //Лог закомментирован, потому что функция вызывается каждую итерацию основного цикла
        //Logger.Log.TraceInf <| sprintf "Check channel emotiontoggle for %s: %b" channel.String (snd tup).EmotionToggle
        (snd tup).EmotionToggle
    | None ->
        Logger.Log.TraceWarn <| sprintf "Check channel emotiontoggle for %s: No found" channel.String
        false
let checkPrefixChannel channel =
    Array.tryFind (fun (elem: (Channels * CacheChannelSettings)) -> channel = fst elem) cacheChannelSettings
    |> function
    | Some (tup) ->
        //Лог закомментирован, потому что функция вызывается каждое сообщение в чате
        //Logger.Log.TraceInf <| sprintf "Check channel prefix for %s: %s" channel.String (snd tup).Prefix
        Ok((snd tup).Prefix)
    | None ->
        Logger.Log.TraceWarn <| sprintf "Check channel prefix for %s: No found" channel.String
        Error("Not found prefix")
let checkLastReactTimeChannel channel =
    Array.tryFind
        (fun (elem: (Channels * CacheChannelSettings)) ->
            let settings = snd elem
            ((DateTime.Now.Ticks / 10000000L)
             - settings.LastReactTime) > settings.EmotionCoolDown
            && channel = fst elem)
        cacheChannelSettings
    |> function
    | Some (_) -> true
    | None -> false
let updateLastReactTimeChannel channel =
    Array.tryFind (fun (elem: (Channels * CacheChannelSettings)) -> channel = fst elem) cacheChannelSettings
    |> function
    | Some (ok) ->
        let settings =
            { (snd ok) with
                  LastReactTime = DateTime.Now.Ticks / 10000000L }
        cacheChannelSettings <- Array.filter (fun elem -> (fst elem) <> channel) cacheChannelSettings
        cacheChannelSettings <- Array.append cacheChannelSettings [| (channel, settings) |]
    | None -> ()
let handleCacheUsers (msgr: MessageRead) =
    Array.tryFind
        (fun (elem: CacheUser) ->
            elem.User = msgr.User
            && elem.Channel = msgr.Channel)
        cacheUsers
    |> function
    | None ->
        cacheUsers <-
            Array.append
                (Array.filter
                    (fun (elem: CacheUser) ->
                        not (
                            elem.User = msgr.User
                            && elem.Channel = msgr.Channel
                        ))
                    cacheUsers)
                [| { Channel = msgr.Channel
                     User = msgr.User } |]
    | _ -> ()
let handleCacheCatDown () =
    cacheCutDown <-
        Array.filter
            (fun (elem: CacheCatDown) ->
                ((DateTime.Now.Ticks / 10000000L) - elem.TimeWhen) < elem.TimeHowLongCantKill)
            cacheCutDown
let handleCacheCatDownReward (rw: ReaderWriter) =
    cacheCutDownReward <-
        Array.filter
            (fun (elem: CacheCatDownReward) ->
                if ((DateTime.Now.Ticks / 10000000L) - elem.TimeWhen) < elem.TimeHowLong then
                    true
                else
                    rw.Writer.WriteLine(sprintf "PRIVMSG #%s :/untimeout %s" elem.Channel.String elem.WhoKilled)
                    rw.Writer.Flush()
                    false)
            cacheCutDownReward
let checkCacheCatDownKill whoKill (channel: Channels) =
    Array.tryFind (fun (elem: CacheCatDown) -> elem.WhoKill = whoKill && elem.Channel = channel) cacheCutDown
    |> function
    | Some (_) -> true
    | None -> false
let checkCacheCatDownKilled whoKilled (channel: Channels) =
    Array.tryFind
        (fun (elem: CacheCatDown) ->
            elem.WhoKilled = whoKilled
            && elem.Channel = channel
            && ((DateTime.Now.Ticks / 10000000L) - elem.TimeWhen) < elem.TimeHowLongKilled)
        cacheCutDown
    |> function
    | Some (_) -> true
    | None -> false
let checkCacheCatDownRewardKilled whoKilled (channel: Channels) =
    Array.tryFind
        (fun (elem: CacheCatDownReward) ->
            elem.WhoKilled = whoKilled
            && elem.Channel = channel)
        cacheCutDownReward
    |> function
    | Some (_) -> true
    | None -> false
let addCacheCatDown (cutDown: CacheCatDown) =
    if
        not
            (
                checkCacheCatDownKill cutDown.WhoKill cutDown.Channel
                || checkCacheCatDownKilled cutDown.WhoKilled cutDown.Channel
            )
    then
        cacheCutDown <- Array.append cacheCutDown [| cutDown |]
    elif cutDown.WhoKill = cutDown.WhoKilled then
        cacheCutDown <- Array.append cacheCutDown [| cutDown |]
let addCacheCatDownReward (cutDownReward: CacheCatDownReward) =
    if not (checkCacheCatDownRewardKilled cutDownReward.WhoKilled cutDownReward.Channel) then
        cacheCutDownReward <- Array.append cacheCutDownReward [| cutDownReward |]
let resolveCommandCache (msgr: MessageRead) cmd =
    Logger.Log.TraceInf
    <| sprintf "Start resolve CommandCache "
    Array.tryFind (fun (elem: CacheChannelCommands) -> msgr.Channel = (fst elem.ListCMD)) cacheChannelCommands
    |> function
    | Some (list) ->
        Logger.Log.TraceInf
        <| sprintf "Get list of commands "
        List.tryFind (fun (elem: ChannelCommand) -> elem.chCommand = cmd) (snd list.ListCMD)
    | None -> None
let userFromCache nickname (msgr: MessageRead) =
    Array.tryFind
        (fun (elem: CacheUser) ->
            match nickname with
            | Some (nick) ->
                elem.User.Name = nick
                && msgr.Channel = elem.Channel
            | None -> false)
        cacheUsers
let rec initCache (channelsList: Channels list) listSize =
    if listSize - 1 < 0 then
        ()
    else
        let currentSize = listSize - 1
        let channel = List.item currentSize channelsList
        updateCacheChannelSettings channel
        addCacheChannelCommands channel
        initCache channelsList currentSize