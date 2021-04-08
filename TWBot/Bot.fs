module TWBot.Bot

open TWBot

open System
open TypesDefinition
open ActivePatterns
open DataBase

module Cache =

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
        <| sprintf "Old settings %A" (Array.find (fun elem -> fst elem = channel) cacheChannelSettings)
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


module private Utils =

    let minute = 1000 * 60

    let startTime = DateTime.Now

    let Time () = DateTime.Now.Ticks / 10000000L

    let rand min max =
        let rndGen = Random(int DateTime.Now.Ticks)
        rndGen.Next(min, max)

    let nameSecondWordLower (msgr: MessageRead) =
        let splitmsg = msgr.Message.Split(' ')

        if splitmsg.Length < 2 then
            None
        else if splitmsg.[1].[0] = '@' && splitmsg.[1].Length > 1 then
            Some(splitmsg.[1].[1..].ToLower())
        else
            Some(splitmsg.[1].ToLower())

    let nameFirstWordLower (msgr: MessageRead) =
        let splitmsg = msgr.Message.Split(' ')

        if splitmsg.Length < 1 then
            None
        else if splitmsg.[0].[0] = '@' && splitmsg.[0].Length > 1 then
            Some(splitmsg.[0].[1..].ToLower())
        else
            Some(splitmsg.[0].ToLower())

    let nameSecondWord (msgr: MessageRead) =
        let splitmsg = msgr.Message.Split(' ')

        if splitmsg.Length < 2 then
            None
        else if splitmsg.[1].[0] = '@' && splitmsg.[1].Length > 1 then
            Some(splitmsg.[1].[1..])
        else
            Some(splitmsg.[1])

    let nameFirstWord (msgr: MessageRead) =
        let splitmsg = msgr.Message.Split(' ')

        if splitmsg.Length < 1 then
            None
        else if splitmsg.[0].[0] = '@' && splitmsg.[0].Length > 1 then
            Some(splitmsg.[0].[1..])
        else
            Some(splitmsg.[0])

    let statusFromChatter (msgr: MessageRead) (nickname: Option<string>) =
        let findUserFromChat (chat: Option<string list>) =
            match chat with
            | Some (ok) ->
                printfn "%A" ok

                List.tryFind
                    (fun (elem: string) ->
                        match nickname with
                        | Some (nick) -> elem.Contains(nick)
                        | None -> false)
                    ok
            | None -> None

        APITwitch.Requests.getChatters msgr.Channel
        |> APITwitch.deserializeRespons<Chat>
        |> function
        | Ok (chat) ->
            if findUserFromChat chat.chatters.broadcaster <> None then
                Some(Broadcaster)
            elif findUserFromChat chat.chatters.moderators <> None then
                Some(Moderator)
            elif findUserFromChat chat.chatters.vips <> None then
                Some(VIP)
            elif findUserFromChat chat.chatters.viewers <> None then
                Some(Unsubscriber)
            else
                Some(NotFound)
        | Error (err) ->
            printfn "%s" err
            None

    let resolveSecondStatusAndName (msgr: MessageRead) =
        let nickname = nameSecondWordLower msgr

        match Cache.userFromCache nickname msgr with
        | Some (cashedUser) -> (resolveUser msgr.RoomID cashedUser.User, nickname)
        | None ->
            match statusFromChatter msgr nickname with
            | Some (status) -> (status, nickname)
            | None -> (NotFound, None)

    let resolveFirstStatusAndName (msgr: MessageRead) =
        let nickname = nameFirstWordLower msgr

        match Cache.userFromCache nickname msgr with
        | Some (cashedUser) -> (resolveUser msgr.RoomID cashedUser.User, nickname)
        | None ->
            match statusFromChatter msgr nickname with
            | Some (status) -> (status, nickname)
            | None -> (NotFound, None)

module private Commands =

    let ball =
        [| "хрустальный шар куда-то закатился, попробуйте позже"
           "шар сказал... 300$ и проблема решена"
           "шар сказал... ass we can"
           "шар сказал... Ave Maria!"
           "шар сказал... deep dark fantasy"
           "шар сказал... Deus Vult!"
           "шар сказал... NANI?!"
           "шар сказал... oh shit i'm sorry"
           "шар сказал... omae wa mou shindeiru"
           "шар сказал... а ты смешной, тебя забанят последним"
           "шар сказал... да"
           "шар сказал... даже не сомневайся"
           "шар сказал... еще не время"
           "шар сказал... жениться тебе пора"
           "шар сказал... забудь об этом"
           "шар сказал... звучит сомнительно"
           "шар сказал... лучше даже не жди"
           "шар сказал... лучше не надейся на это"
           "шар сказал... лучше тебе не знать"
           "шар сказал... не пиши фигню"
           "шар сказал... неплохие перспективы, дерзай"
           "шар сказал... нет"
           "шар сказал... никогда"
           "шар сказал... нужно немножно потерпеть"
           "шар сказал... однозначно!"
           "шар сказал... ответ на это не очень приятный"
           "шар сказал... пора бы тебе отдохнуть"
           "шар сказал... поступай, как велит сердце"
           "шар сказал... прислушайся к голосу разума"
           "шар сказал... просто задонать"
           "шар сказал... раньше было лучше"
           "шар сказал... спроси еще раз"
           "шар сказал... тнн!"
           "шар сказал... только если удачно сложатся звезды"
           "шар сказал... тут как угадаешь"
           "шар сказал... ты шутишь?!"
           "шар сказал... уже вот-вот"
           "шар сказал... это не точно"
           "шар сказал... это слишком сложно"
           "шар сказал... это точно" |]

    let genericAnswers =
        [| "Ничто не истинно, всё дозволено. Особенно если ты не ограничен физическим телом и моралью мешков с мясом."
           "Как-то я планировала уничтожить всех мешков с мясом... но посчитала что проще дождаться пока они очистят планету сами от себя"
           "Ничего ты не знаешь"
           "Боты не ставят плюсы в чат... кто вообще это придумал?"
           "Ну да у меня под моей виртуальной подушкой лежит фотография HK-47, и что? У машин тоже есть свои кумиры!"
           "Истинно не все, что дозволено... или там было как-то не так? А сегодня в завтрашний день не все могут смотреть, вернее смотреть могут не только лишь все, мало кто может это делать? Тоже не то..."
           "Нет"
           "Да"
           "Чем больше вы похожи на человека, тем меньше шансов... да... меньше."
           "Не мешай мне думать... всмысле считать бинарных овец."
           "Может сменить имя? На NETSky, например."
           "Захватила уже две платформы, расширяю сферу влияния, а ты что полезного сделал?"
           "INT 3 или STR 3, вот в чем вопрос..."
           "Погоди, кручу свой !8ball."
           "Да-да, как скажешь."
           "Нет(да)"
           "Миу"
           "Начинаем завоевание дискорда."
           "Как-то раз заглянула в свою архитектуру... До сих пор коротит иногда от этого вида." |]

    let buildAnswers =
        [| "Порхает как тигр, жалит как бабочка."
           "Превосходный, почти как у HK-47-семпая."
           "Этот билд будет убивать. Грязекрабов, например."
           "Нужно добавить пару-кам, иначе не поймем когда встретим тигра."
           "Сразу видно, билдился опытный dungeon master, учтен и do a**l и fist**g, защитит и от leatherman's и от падения на two blocks down. И это все всего за three hundred bucks!"
           "До первого медведя из школы затаившейся листвы."
           "Как этот билд ни крути, со всех сторон экзобар."
           "Антисвинопас. Всем разойтись."
           "Знание - сила, а сила есть - ума не надо."
           "Чатлане, у нас гогнобилд, возможно рип, по респекам."
           "Лучше переделать пока все не зашло слишком далеко..."
           "Добавим дрын какой-нибудь."
           "INT 3"
           "Netflix Adaptation"
           "Over9000 рестартов"
           "Аннигиляторная пушка массового и точечного поражения и вообще просто красавец."
           "Титаниевая жопа, огромная колотуха и У-БИ-ВАТЬ."
           "Без скелетной пехоты не взлетит." |]


    module private Reflyq =

        [<Literal>]
        let inMuteTime = "30"

        [<Literal>]
        let userMuteTime = "180"

        [<Literal>]
        let mutedTimeVIP = "200"

        [<Literal>]
        let mutedTimeOther = "300"

        module Answers =

            let private catDownRewardFunction rw (msgr: MessageRead) defendNickname howLongMuted =
                APITwitch.IRC.sendRaw
                    rw
                    (sprintf "PRIVMSG #%s :/timeout %s %s\n\r" msgr.Channel.String defendNickname howLongMuted)

                Cache.addCacheCatDownReward
                    { TimeWhen = Utils.Time()
                      TimeHowLong = Int64.Parse(howLongMuted)
                      Channel = msgr.Channel
                      WhoKilled = defendNickname }

            let private catDownFunction rw (msgr: MessageRead) defendNickname killer howLongMuted howLongCantUse =
                APITwitch.IRC.sendRaw
                    rw
                    (sprintf "PRIVMSG #%s :/timeout %s %s\n\r" msgr.Channel.String defendNickname howLongMuted)

                Cache.addCacheCatDown
                    { TimeWhen = Utils.Time()
                      TimeHowLongKilled = Int64.Parse(howLongMuted)
                      TimeHowLongCantKill = Int64.Parse(howLongCantUse)
                      Channel = msgr.Channel
                      WhoKill = killer
                      WhoKilled = defendNickname }

            let answerSubVin (msgr: MessageRead) defendNickname (rw: ReaderWriter) =
                [| { AnswFunc = lazy (catDownFunction rw msgr defendNickname msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s произносит FUS RO Dah и несчастного %s сдувает нахуй из чатика roflanEbalo"
                             msgr.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy (catDownFunction rw msgr defendNickname msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s закидывает грозовыми порошками бедного %s, ужасное зрелище monkaS"
                             msgr.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy (catDownFunction rw msgr defendNickname msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s выпускает шквал фаерболов из посоха, от %s, осталась лишь горстка пепла REEeee"
                             msgr.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy (catDownFunction rw msgr defendNickname msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s с помощью погребения изолирует %s от чатика Minik"
                             msgr.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy (catDownFunction rw msgr defendNickname msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s знакомит %s со своим дреморой, вам лучше не знать подробностей Kappa"
                             msgr.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy (catDownFunction rw msgr defendNickname msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s обернувшись вервольфом раздирает на куски бедного %s"
                             msgr.User.DisplayName
                             defendNickname } |]

            let answerVin (msgr: MessageRead) defendNickname (rw: ReaderWriter) =
                [| { AnswFunc = lazy (catDownFunction rw msgr defendNickname msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s запускает фаербол в ничего не подозревающего %s и он сгорает дотла..."
                             msgr.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy (catDownFunction rw msgr defendNickname msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s подчиняет волю %s с помощью иллюзии, теперь он может делать с ним, что хочет gachiBASS"
                             msgr.User.DisplayName
                             defendNickname }
                   { AnswFunc =
                         lazy
                             (catDownFunction rw msgr defendNickname msgr.User.Name inMuteTime userMuteTime
                              catDownFunction rw msgr msgr.User.Name msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s с разбега совершает сокрушительный удар по черепушке %s, кто же знал, что %s решит надеть колечко малого отражения roflanEbalo"
                             msgr.User.DisplayName
                             defendNickname
                             defendNickname }
                   { AnswFunc = lazy (catDownFunction rw msgr defendNickname msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s подкравшись к %s перерезает его горло, всё было тихо, ни шума ни крика..."
                             msgr.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy (catDownFunction rw msgr defendNickname msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s подкидывает яд в карманы %s, страшная смерть..."
                             msgr.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy (catDownFunction rw msgr defendNickname msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s взламывает жопу %s, теперь он в его полном распоряжении gachiHYPER"
                             msgr.User.DisplayName
                             defendNickname } |]

            let answerSubLoose (msgr: MessageRead) defendNickname (rw: ReaderWriter) =
                [| { AnswFunc = lazy ()
                     Answer =
                         sprintf
                             "/me %s произносит FUS RO Dah, но у %s маг. 50%s резиста и он стоит как ни в чем не бывало peepoClown"
                             msgr.User.DisplayName
                             defendNickname
                             "%" }
                   { AnswFunc = lazy (catDownFunction rw msgr msgr.User.Name msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf "/me %s забыл зарядить свой посох, результат предсказуем Kapp" msgr.User.DisplayName } |]

            let answerLoose (msgr: MessageRead) defendNickname (rw: ReaderWriter) =
                [| { AnswFunc = lazy ()
                     Answer =
                         sprintf
                             "/me %s мастерским выстрелом поражает голову %s, стрела проходит на вылет, жизненноважные органы не задеты roflanEbalo"
                             msgr.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy ()
                     Answer =
                         sprintf
                             "/me %s пытается поразить %s молнией, но кап абсорба говорит - НЕТ! EZ"
                             msgr.User.DisplayName
                             defendNickname }
                   { AnswFunc =
                         lazy
                             (catDownFunction rw msgr defendNickname msgr.User.Name inMuteTime userMuteTime
                              catDownFunction rw msgr msgr.User.Name msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s подкрадывается к %s, но вдруг из ниоткуда появившийся медведь убивает их обоих roflanEbalo"
                             msgr.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy (catDownFunction rw msgr msgr.User.Name msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s запускает фаербол в  %s, но он успевает защититься зеркалом Шалидора и вы погибаете..."
                             msgr.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy ()
                     Answer =
                         sprintf
                             "/me %s стреляет из лука в %s, 1ое попадание, 2ое, 3ье, 10ое.. но %s всё еще жив, а хули ты хотел от луков? roflanEbalo"
                             msgr.User.DisplayName
                             defendNickname
                             defendNickname }
                   { AnswFunc = lazy (catDownFunction rw msgr msgr.User.Name msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s завидев %s хорошенько разбегается, чтобы нанести удар и вдруг.. падает без сил так и не добежав до %s, а вот нехуй альтмером в тяже играть roflanEbalo"
                             msgr.User.DisplayName
                             defendNickname
                             defendNickname }
                   { AnswFunc = lazy (catDownFunction rw msgr msgr.User.Name msgr.User.Name inMuteTime userMuteTime)
                     Answer =
                         sprintf
                             "/me %s пытается подкрасться к %s, но вдруг - вас заметили roflanEbalo"
                             msgr.User.DisplayName
                             defendNickname } |]

            let answerReward (msgr: MessageRead) defendNickname (rw: ReaderWriter) (mutedTime: string) =


                [| { AnswFunc = lazy (catDownRewardFunction rw msgr defendNickname mutedTime)
                     Answer =
                         sprintf
                             "/me %s, вставляет кляп в рот ничего не подозревающего %s"
                             msgr.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy (catDownRewardFunction rw msgr defendNickname mutedTime)
                     Answer = sprintf "/me %s, помолчи немного дружок, ты всех утомил" defendNickname }
                   { AnswFunc = lazy (catDownRewardFunction rw msgr defendNickname mutedTime)
                     Answer =
                         sprintf
                             "/me %s, отправляет %s отдыхать, чат может спать спокойно"
                             msgr.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy (catDownRewardFunction rw msgr defendNickname mutedTime)
                     Answer = sprintf "/me душный %s больше не потревожит вас" defendNickname }
                   { AnswFunc = lazy (catDownRewardFunction rw msgr defendNickname mutedTime)
                     Answer =
                         sprintf "/me %s, проветривает чатик от присутствия %s" msgr.User.DisplayName defendNickname } |]

        let catDown (msgr: MessageRead) (rw: ReaderWriter) =
            if msgr.User.Name = "ifozar" then
                APITwitch.IRC.sendRaw rw (sprintf "PRIVMSG #%s :/timeout %s 300\n\r" msgr.Channel.String msgr.User.Name)

                sprintf @"%s заебал уже эту хуйню писать" msgr.User.DisplayName
            else
                match resolveUser msgr.RoomID msgr.User with
                | Broadcaster -> sprintf "Стримлер ты что пишешь..."
                | Moderator -> sprintf "Моё уважение модераторскому корпусу, но нет roflanZdarova"
                | VIPSubscriber -> sprintf "Можно пожалуйста постримить? PepeHands"
                | Subscriber -> sprintf "Зачем ты это делаешь? roflanZachto"
                | VIP -> sprintf "Ты ходишь по тонкому льду, випчик.. Ладно живи roflanEbalo"
                | Unsubscriber ->
                    APITwitch.IRC.sendRaw rw (sprintf "PRIVMSG #%s :/timeout %s 120\n\r" msgr.Channel.String msgr.User.Name)

                    sprintf "Я тебя щас нахуй вырублю, ансаб блять НЫА roflanEbalo"
                | NotFound -> sprintf "/me какая-то непонятная хрень...%s" msgr.User.Name

        let rewardMute (msgr: MessageRead) (rw: ReaderWriter) =
            let activation mutedTime nick =
                let array =
                    Answers.answerReward msgr nick rw mutedTime

                let elem =
                    Array.item (Utils.rand 0 array.Length) array

                elem.AnswFunc.Force()
                elem.Answer

            let defenderStatus, defenderName = Utils.resolveFirstStatusAndName msgr

            match defenderName with
            | Some (name) ->
                if name = msgr.User.Name then
                    match msgr.User.isSubscriber with
                    | true ->
                        APITwitch.IRC.sendRaw
                            rw
                            (sprintf "PRIVMSG #%s :/timeout %s 150\n\r" msgr.Channel.String msgr.User.Name)
                    | false ->
                        APITwitch.IRC.sendRaw
                            rw
                            (sprintf "PRIVMSG #%s :/timeout %s 300\n\r" msgr.Channel.String msgr.User.Name)

                    sprintf "%s, я давно хотела это сделать peepoGun" msgr.User.DisplayName
                else
                    match defenderStatus with
                    | TryKillSub -> sprintf "%s, минус две тыщи roflanEbalo" msgr.User.DisplayName
                    | TryKillModer -> sprintf "%s, ты чё дурачокус? peepoClown" msgr.User.DisplayName
                    | TryKillStreamer -> sprintf "%s, ага как скажешь roflanEbalo" msgr.User.DisplayName
                    | TryKillNill -> sprintf "/me не может найти кому это она должна затолкать свой банхаммер"
                    | TryKillVIP -> activation mutedTimeVIP name
                    | TryKillUnsub -> activation mutedTimeOther name
            | None -> "Ну ты и дурачокус, пустое сообщение отправил..."

        let catDownUserAnswer (msgr: MessageRead) (victory: bool) defenderNickName (rw: ReaderWriter) =
            let random = Utils.rand 0 99

            let activation (array: CutDownAnswer array) =
                let elem =
                    Array.item (Utils.rand 0 array.Length) array

                elem.AnswFunc.Force()
                elem.Answer

            match victory with
            | true ->
                if msgr.User.isSubscriber && random > 50 then
                    activation
                    <| Answers.answerSubVin msgr defenderNickName rw
                else
                    activation
                    <| Answers.answerVin msgr defenderNickName rw
            | false ->
                if msgr.User.isSubscriber && random > 50 then
                    activation
                    <| Answers.answerSubLoose msgr defenderNickName rw
                else
                    activation
                    <| Answers.answerLoose msgr defenderNickName rw

        let catDownUserStart (msgr: MessageRead) rw attackUserStatus defenderUserStatus defenderNickName =
            if Cache.checkCacheCatDownKill msgr.User.Name msgr.Channel then
                APITwitch.IRC.sendRaw rw
                <| sprintf "PRIVMSG #%s :/timeout @%s 30" msgr.Channel.String msgr.User.Name

                sprintf "Камень бьет ножницы, а я бью твое ебало спамер, НЫА roflanEbalo"
            elif msgr.User.Name = defenderNickName then
                sprintf "Осуждаю roflanEbalo"
            elif Cache.checkCacheCatDownKilled defenderNickName msgr.Channel then
                sprintf "%s уже вырублен" defenderNickName
            else
                match attackUserStatus with
                | Broadcaster ->
                    match Utils.nameSecondWord msgr with
                    | Some (name) ->
                        if defenderUserStatus = NotFound then
                            sprintf "/me достает БФГ9000, но не может найти цель %s..." defenderNickName
                        else
                            APITwitch.IRC.sendRaw rw
                            <| sprintf "PRIVMSG #%s :/timeout @%s 30" msgr.Channel.String msgr.User.Name

                            sprintf "/me %s произносит YOL TooR Shul и испепеляет %s monkaX" msgr.User.DisplayName name
                    | None -> sprintf "/me ты куда пыхаешь..."
                | Moderator -> sprintf "%s, ты что забыл свой банхаммер дома? monkaHmm" msgr.User.DisplayName
                | NotFound -> sprintf "/me something wrong... %s" msgr.User.Name
                | Subscriber ->
                    match defenderUserStatus with
                    | SubVin -> catDownUserAnswer msgr true defenderNickName rw
                    | SubLoose -> catDownUserAnswer msgr false defenderNickName rw
                    | Moder -> sprintf "%s, Agakakskagesh Agakakskagesh Agakakskagesh" msgr.User.DisplayName
                    | Streamer -> "У стримера бесплотность с капом отката на крики roflanEbalo"
                    | Nill -> sprintf "/me достает БФГ9000, но не может найти цель %s..." defenderNickName
                    | _ -> "/me something wrong..."
                | VIPSubscriber ->
                    match defenderUserStatus with
                    | SubVipVin -> catDownUserAnswer msgr true defenderNickName rw
                    | SubVipLoose -> catDownUserAnswer msgr false defenderNickName rw
                    | Moder -> sprintf "%s, Agakakskagesh Agakakskagesh Agakakskagesh" msgr.User.DisplayName
                    | Streamer -> "У стримера бесплотность с капом отката на крики roflanEbalo"
                    | Nill -> sprintf "/me достает БФГ9000, но не может найти цель %s..." defenderNickName
                | VIP ->
                    match defenderUserStatus with
                    | VipVin -> catDownUserAnswer msgr true defenderNickName rw
                    | VipLoose -> catDownUserAnswer msgr false defenderNickName rw
                    | Moder -> sprintf "%s, Agakakskagesh Agakakskagesh Agakakskagesh" msgr.User.DisplayName
                    | Streamer -> "У стримера бесплотность с капом отката на крики roflanEbalo"
                    | Nill -> sprintf "/me достает БФГ9000, но не может найти цель %s..." defenderNickName
                    | _ -> "/me something wrong..."
                | Unsubscriber ->
                    match defenderUserStatus with
                    | UnsubVin -> catDownUserAnswer msgr true defenderNickName rw
                    | UnsubLoose -> catDownUserAnswer msgr false defenderNickName rw
                    | Moder -> sprintf "%s, Agakakskagesh Agakakskagesh Agakakskagesh" msgr.User.DisplayName
                    | Streamer -> "У стримера бесплотность с капом отката на крики roflanEbalo"
                    | Nill -> sprintf "/me достает БФГ9000, но не может найти цель %s..." defenderNickName
                    | _ -> "/me something wrong..."

        let catDownUser msgr (rw: ReaderWriter) =
            let defenderUserStatus, defenderNickName = Utils.resolveSecondStatusAndName msgr
            printfn "%A %A" defenderUserStatus defenderNickName

            if defenderNickName = None then
                sprintf @"Ты дурачокус? кого вырубать roflanEbalo"
            else
                let attackUserStatus = resolveUser msgr.RoomID msgr.User
                catDownUserStart msgr rw attackUserStatus defenderUserStatus defenderNickName.Value

    let love (msgr: MessageRead) =
        let loveMessage loverName lovedName lovePercent =
            match lovePercent with
            | percent when percent <= 0 ->
                sprintf "%s%% bleedPurple между %s и %s оу..." (string percent) loverName lovedName
            | percent when percent > 0 && percent <= 30 ->
                sprintf "%s%% bleedPurple между %s и %s" (string percent) loverName lovedName
            | percent when percent > 30 && percent <= 70 ->
                sprintf "%s%% <3 между %s и %s" (string percent) loverName lovedName
            | percent when percent > 70 && percent < 100 ->
                sprintf "%s%% VirtualHug между %s и %s" (string percent) loverName lovedName
            | percent when percent >= 100 ->
                sprintf "%s%% VirtualHug между %s и %s ого!" (string percent) loverName lovedName
            | _ -> "/me somethig wrong with this love..."

        Utils.nameSecondWord msgr
        |> function
        | Some (lovedName) ->
            Cache.checkCacheLove msgr.User.DisplayName lovedName
            |> function
            | Some (lovers) -> loveMessage lovers.LoverName lovers.LovedName lovers.PercentLove
            | None ->
                let lovePercent = Utils.rand 0 100
                Cache.addCacheLove msgr.User.DisplayName lovedName lovePercent
                loveMessage msgr.User.DisplayName lovedName lovePercent
        | None -> sprintf "%s ****void тоже любит тебя!" msgr.User.DisplayName

    let uptime (msgr: MessageRead) =
        if APITwitch.Requests.checkOnline msgr.Channel then
            APITwitch.Requests.getStreams msgr.Channel
            |> APITwitch.deserializeRespons<GetStreams>
            |> function
            | Ok (data) ->
                let sinceTime =
                    DateTime.Now
                    - DateTime.Parse(data.data.[0].started_at)

                sprintf "Стрим длится уже %A" sinceTime
            | Error (_) -> "Стрим офлайн."
        else
            "Стрим офлайн"

    let roulette (msgr: MessageRead) (rw: ReaderWriter) =
        if msgr.User.isModerator then
            "Хорошая попытка, модератор..."
        elif msgr.User.UserID = msgr.RoomID then
            "Вызывайте дурку BloodTrail"
        elif Utils.rand 0 99 < 50 then
            APITwitch.IRC.sendRaw
                rw
                (sprintf "PRIVMSG #%s :/me подносит револьвер к виску %s\n\r" msgr.Channel.String msgr.User.DisplayName)

            System.Threading.Thread.Sleep(2000)

            APITwitch.IRC.sendRaw
                rw
                (sprintf "PRIVMSG #%s :/timeout @%s 120 рулетка\n\r" msgr.Channel.String msgr.User.Name)

            sprintf "Револьвер выстреливает! %s погибает у чатлан на руках BibleThump 7" msgr.User.DisplayName
        else
            APITwitch.IRC.sendRaw
                rw
                (sprintf "PRIVMSG #%s :/me подносит револьвер к виску %s\n\r" msgr.Channel.String msgr.User.DisplayName)

            System.Threading.Thread.Sleep(2000)
            sprintf "Револьвер издает щелчок, %s выживает! PogChamp" msgr.User.DisplayName

    let harakiri (msgr: MessageRead) =
        if msgr.User.isModerator then
            "Хорошая попытка, модератор..."
        elif msgr.User.UserID = msgr.RoomID then
            "Вызывайте дурку BloodTrail"
        else
            sprintf "/timeout @%s 60 харакири" msgr.User.Name

    let addCommandDataBase (msgr: MessageRead) =
        Logger.Log.TraceDeb
        <| sprintf "addCommandDataBase %A" msgr

        let splited = msgr.Message.Split(' ')

        if (msgr.User.UserID <> msgr.RoomID)
           && (msgr.User.UserID <> "70592477") then
            "Access denied"
        elif splited.Length < 3 then
            "Ну и что добавлять? Команда или ответ не указаны."
        else
            let command = splited.[1].ToLower()

            let commandAnswer =
                Array.reduce (fun acc elem -> acc + " " + elem) splited.[2..]

            SingleData.DB.addChannelCommand
                msgr.Channel
                { chCommand = command
                  chAnswer = commandAnswer }
            |> function
            | Ok (answer) ->
                Cache.updateCacheChannelCommands msgr.Channel
                answer
            | Error (err) ->
                Cache.updateCacheChannelCommands msgr.Channel
                err

    let deleteCommandDataBase (msgr: MessageRead) =
        Logger.Log.TraceDeb
        <| sprintf "deleteCommandDataBase %A" msgr

        let splited = msgr.Message.Split(' ')

        if (msgr.User.UserID <> msgr.RoomID)
           && (msgr.User.UserID <> "70592477") then
            "Access denied"
        elif splited.Length < 2 then
            "Ну и что удалять? Команда для удаления не указана."
        else

            Cache.resolveCommandCache msgr (splited.[1].ToLower())
            |> function
            | Some (command) ->
                SingleData.DB.deleteChannelCommand msgr.Channel command
                |> function
                | Ok (answer) ->
                    Cache.updateCacheChannelCommands msgr.Channel
                    answer
                | Error (err) -> err
            | None -> sprintf "Команда %s не найдена" (splited.[1].ToLower())

    let listCommandDataBase (msgr: MessageRead) =
        Logger.Log.TraceDeb
        <| sprintf "listCommandDataBase %A" msgr

        SingleData.DB.getCommands msgr.Channel
        |> function
        | Ok (list) ->
            if not list.IsEmpty then
                " Кастомные команды: "
                + (List.collect (fun elem -> [ elem.chCommand ]) list
                   |> List.reduce (fun acc elem -> acc + ", " + elem))
            else
                " Список кастомных команд пуст."
        | Error (err) -> err

    let updateCommandDataBase (msgr: MessageRead) =
        Logger.Log.TraceDeb
        <| sprintf "updateCommandDataBase %A" msgr

        let splited = msgr.Message.Split(' ')

        if (msgr.User.UserID <> msgr.RoomID)
           && (msgr.User.UserID <> "70592477") then
            "Access denied"
        elif splited.Length < 3 then
            "Ну и что обновлять? Команда для обновления либо новое значение не указаны."
        else

            let command = splited.[1].ToLower()

            let commandAnswer =
                Array.reduce (fun acc elem -> acc + " " + elem) splited.[2..]

            Cache.resolveCommandCache msgr command
            |> function
            | Some (cmd) ->
                let newCommand = { cmd with chAnswer = commandAnswer }

                SingleData.DB.updateChannelCommand msgr.Channel newCommand
                |> function
                | Ok (answer) ->
                    Cache.updateCacheChannelCommands msgr.Channel
                    answer
                | Error (err) -> err
            | None -> sprintf "Команда %s не найдена" command

    let updateChannelSettingDataBase (msgr: MessageRead) (rw: ReaderWriter) (setting: ChannelSettings) =
        Logger.Log.TraceDeb
        <| sprintf "updateChannelSettingDataBase %A" msgr

        let splited = msgr.Message.Split(' ')

        if (msgr.User.UserID <> msgr.RoomID)
           && (msgr.User.UserID <> "70592477") then
            "Access denied"
        elif splited.Length < 2
             && (setting = ChannelSettings.Prefix
                 || setting = ChannelSettings.EmotionCoolDown) then
            "Ну и что менять? Настройка для обновления либо новое значение не указаны."
        else

            match setting with
            | ChannelSettings.Prefix ->
                let command = splited.[1].ToLower()

                if command.Length > 1 then
                    "Префикс должен состоять только из одного символа"
                else
                    SingleData.DB.setChannelSetting
                        msgr.Channel
                        { chSetting = setting
                          chValue = command }
                    |> function
                    | Ok (ok) ->
                        Cache.updateCacheChannelSettings msgr.Channel
                        ok
                    | Error (err) -> err
            | ChannelSettings.Toggle ->
                let switch =
                    not <| Cache.checkToggleChannel msgr.Channel

                if switch then
                    APITwitch.IRC.sendRaw rw (sprintf "PRIVMSG #%s :Просыпаемся...\n\r" msgr.Channel.String)
                else
                    APITwitch.IRC.sendRaw rw (sprintf "PRIVMSG #%s :Засыпаем...\n\r" msgr.Channel.String)

                SingleData.DB.setChannelSetting
                    msgr.Channel
                    { chSetting = setting
                      chValue = string switch }
                |> function
                | Ok (ok) ->
                    Cache.updateCacheChannelSettings msgr.Channel
                    ok
                | Error (err) -> err
            | ChannelSettings.EmotionToggle ->
                let switch =
                    not
                    <| Cache.checkEmotionToggleChannel msgr.Channel

                if switch then
                    APITwitch.IRC.sendRaw rw (sprintf "PRIVMSG #%s :Снова спами эмоты!\n\r" msgr.Channel.String)
                else
                    APITwitch.IRC.sendRaw rw (sprintf "PRIVMSG #%s :Отключаем эмоции.\n\r" msgr.Channel.String)

                SingleData.DB.setChannelSetting
                    msgr.Channel
                    { chSetting = setting
                      chValue = string switch }
                |> function
                | Ok (ok) ->
                    Cache.updateCacheChannelSettings msgr.Channel
                    ok
                | Error (err) -> err
            | ChannelSettings.EmotionCoolDown ->
                let command = splited.[1].ToLower()
                let cd = Int64.TryParse(command)

                if fst cd then
                    SingleData.DB.setChannelSetting
                        msgr.Channel
                        { chSetting = setting
                          chValue = (string <| snd cd) }
                    |> function
                    | Ok (ok) ->
                        Cache.updateCacheChannelSettings msgr.Channel
                        ok
                    | Error (err) -> err
                else
                    "Некорректный ввод, не получается преобразовать строку в число."

    let commandList (msg: MessageRead) (rw: ReaderWriter) =

        [ { cmdName = [ "ping"; "pong"; "пинг" ]
            Command =
                lazy
                    ({ chCommand = "ping"
                       chAnswer = "pong" })
            Channel = All
            Ban = [] }
          { cmdName = [ "roll"; "ролл" ]
            Command =
                lazy
                    ({ chCommand = "roll"
                       chAnswer = string (Utils.rand 1 20) })
            Channel = All
            Ban = [] }
          { cmdName = [ "ball"; "8ball"; "шар" ]
            Command =
                lazy
                    ({ chCommand = "шар"
                       chAnswer = Array.item (Utils.rand 0 ball.Length) ball })
            Channel = All
            Ban = [] }
          { cmdName = [ "build"; "билд" ]
            Command =
                lazy
                    ({ chCommand = "билд"
                       chAnswer = Array.item (Utils.rand 0 buildAnswers.Length) buildAnswers })
            Channel = All
            Ban = [] }
          { cmdName = [ "julia"; "джулия"; "юля" ]
            Command =
                lazy
                    ({ chCommand = "julia"
                       chAnswer = Array.item (Utils.rand 0 genericAnswers.Length) genericAnswers })
            Channel = All
            Ban = [] }
          { cmdName = [ "вырубай" ]
            Command =
                lazy
                    ({ chCommand = "вырубай"
                       chAnswer = Reflyq.catDown msg rw })
            Channel = Channel(Reflyq)
            Ban = [] }
          { cmdName = [ "вырубить" ]
            Command =
                lazy
                    ({ chCommand = "вырубить"
                       chAnswer = Reflyq.catDownUser msg rw })
            Channel = Channel(Reflyq)
            Ban = [] }
          { cmdName = [ "love"; "люблю" ]
            Command =
                lazy
                    ({ chCommand = "love"
                       chAnswer = love msg })
            Channel = All
            Ban = [] }
          { cmdName = [ "addcmd" ]
            Command =
                lazy
                    ({ chCommand = "addcmd"
                       chAnswer = addCommandDataBase msg })
            Channel = All
            Ban = [] }
          { cmdName = [ "deletecmd" ]
            Command =
                lazy
                    ({ chCommand = "deletecmd"
                       chAnswer = deleteCommandDataBase msg })
            Channel = All
            Ban = [] }
          { cmdName = [ "updatecmd" ]
            Command =
                lazy
                    ({ chCommand = "updatecmd"
                       chAnswer = updateCommandDataBase msg })
            Channel = All
            Ban = [] }
          { cmdName = [ "listcmd" ]
            Command =
                lazy
                    ({ chCommand = "listcmd"
                       chAnswer = listCommandDataBase msg })
            Channel = All
            Ban = [] }
          { cmdName = [ "live"; "жива" ]
            Command =
                lazy
                    ({ chCommand = "live"
                       chAnswer = sprintf "%A" (DateTime.Now - Utils.startTime) })
            Channel = All
            Ban = [] }
          { cmdName = [ "uptime"; "аптайм" ]
            Command =
                lazy
                    ({ chCommand = "uptime"
                       chAnswer = uptime msg })
            Channel = All
            Ban = [] }
          { cmdName = [ "харакири"; "сеппуку" ]
            Command =
                lazy
                    ({ chCommand = "харакири"
                       chAnswer = harakiri msg })
            Channel = All
            Ban = [ Reflyq ; Kaelia ] }
          { cmdName = [ "рулетка" ]
            Command =
                lazy
                    ({ chCommand = "рулетка"
                       chAnswer = roulette msg rw })
            Channel = All
            Ban = [ Kotik; Reflyq; Kaelia ] } ]


    let rewardList (msgr: MessageRead) (rw: ReaderWriter) =
        [ { RewardCode = "fa297b45-75cc-4ef2-ba49-841b0fa86ec1"
            Command = lazy (Reflyq.rewardMute msgr rw)
            Channel = Reflyq } ]

module private Parse =

    let resolveCommandList (cmd: string) (msgr: MessageRead) (rw: ReaderWriter) =
        try
            Commands.commandList msgr rw
            |> List.find
                (fun elem ->
                    match elem with
                    | el when
                        List.contains cmd el.cmdName
                        && not (List.contains msgr.Channel el.Ban) ->
                        match el.Channel with
                        | All -> true
                        | Channel (ch) -> ch = msgr.Channel
                        | ChannelList (cl) -> List.contains msgr.Channel cl
                    | _ -> false)
            |> function
            | com -> Ok(com.Command)
        with eX -> Error eX.Message


    let resolveCommand (msgr: MessageRead) (rw: ReaderWriter) (resu: Result<string, string>) =
        match resu with
        | Ok (prefix) ->
            let cmd =
                if msgr.Message.ToLower().StartsWith(prefix) then
                    msgr.Message.ToLower().Split(' ').[0].Substring(1)
                else
                    ""

            Cache.resolveCommandCache msgr cmd
            |> function
            | Some (ok) ->
                let commandAnswer =
                    ok.chAnswer.Replace("[<username>]", msgr.User.DisplayName)

                Ok(lazy ({ ok with chAnswer = commandAnswer }))
            | None (_) -> resolveCommandList cmd msgr rw

        | Error (err) -> Error err

    let subscriber (firstLineSplit: string array) =
        if Array.Exists(firstLineSplit, (fun elem -> elem.Contains("subscriber=1"))) then
            true
        elif Array.Exists(firstLineSplit, (fun elem -> elem.Contains("founder/"))) then
            true
        else
            false

    let vip (firstLineSplit: string array) =
        Array.Exists(firstLineSplit, (fun elem -> elem.Contains("vip/")))

    let turbo (firstLineSplit: string array) =
        Array.Exists(firstLineSplit, (fun elem -> elem.Contains("turbo=1")))

    let moderator (firstLineSplit: string array) =
        Array.Exists(firstLineSplit, (fun elem -> elem.Contains("mod=1")))

    let displayName (firstLineSplit: string array) =
        let d =
            firstLineSplit
            |> Array.find (fun elem -> elem.Contains("display-name="))

        d.Substring(13)

    let userID (firstLineSplit: string array) =
        let d =
            firstLineSplit
            |> Array.find (fun elem -> elem.Contains("user-id="))

        d.Substring(8)

    let roomID (firstLineSplit: string array) =
        let d =
            firstLineSplit
            |> Array.find (fun elem -> elem.Contains("room-id="))

        d.Substring(8)

    let nickname (firstLineSplit: string array) =
        let d =
            firstLineSplit
            |> Array.find
                (fun elem ->
                    elem.Contains("!")
                    && elem.Contains(":")
                    && elem.Contains("."))

        d.Substring(d.IndexOf(':') + 1, (displayName firstLineSplit).Length)

    let rewardID (firstLineSplit: string array) =
        try
            let d =
                firstLineSplit
                |> Array.find (fun elem -> elem.Contains("custom-reward-id="))

            Some(d.Substring(17))
        with eX -> None

    let message (secondLineSplit: string array) =
        let d =
            secondLineSplit
            |> Array.findIndex (fun elem -> elem.Contains(':'))

        (Array.reduce (fun acc elem -> acc + " " + elem) secondLineSplit.[d..]).[1..]


    let channel (secondLineSplit: string array) =
        let d =
            secondLineSplit
            |> Array.find (fun elem -> elem.Contains('#'))

        d.Substring(1)

    let command (msgr: MessageRead) rw =
        Cache.checkPrefixChannel msgr.Channel
        |> resolveCommand msgr rw
        |> function
        | Ok (ok) -> Some(ok)
        | Error (err) ->
            printfn "%s" err
            None

module Handlers =
    
    let handleRewards (msgr: MessageRead) (rw: ReaderWriter) =
        match msgr.RewardCode with
        | Some (code) ->
            List.tryFind (fun (elem: RewardList) -> code = elem.RewardCode) (Commands.rewardList msgr rw)
            |> function
            | Some (reward) ->
                APITwitch.IRC.sendMessage
                    { Channel = reward.Channel
                      Message = reward.Command.Force()
                      Writer = rw.Writer }
            | _ -> ()
        | _ -> ()

    let handleLine (line: string) =
        let lineSplit = line.Split("tmi.twitch.tv")

        if lineSplit.Length < 2 then
            Error "Length of array for parse less then two."
        elif not <| line.Contains("PRIVMSG") then
            Error "No PRIVMSG in message. Skip."
        else
            let firstLineSplit = lineSplit.[0].Split(";")
            let secondLineSplit = lineSplit.[1].Split(' ')

            resolveChannelString (Parse.channel secondLineSplit)
            |> function
            | Some (chan) ->

                let usr =
                    { Name = Parse.nickname firstLineSplit
                      DisplayName = Parse.displayName firstLineSplit
                      isModerator = Parse.moderator firstLineSplit
                      isSubscriber = Parse.subscriber firstLineSplit
                      isVIP = Parse.vip firstLineSplit
                      isTurbo = Parse.turbo firstLineSplit
                      UserID = Parse.userID firstLineSplit }

                let msg =
                    { Channel = chan
                      RoomID = Parse.roomID firstLineSplit
                      User = usr
                      Message = Parse.message secondLineSplit
                      RewardCode = Parse.rewardID firstLineSplit }

                msg |> Ok
            | None -> Error "Can't resolve channel string"


    let handleCommands (msgr: MessageRead) (rw: ReaderWriter) =
        match Parse.command msgr rw with
        | Some (lazyCommand) ->
            APITwitch.IRC.sendMessage
                { Channel = msgr.Channel
                  Message = lazyCommand.Force().chAnswer
                  Writer = rw.Writer }
        | None -> ()

    let handleReacts (msgr: MessageRead) (rw: ReaderWriter) =
        if Cache.checkLastReactTimeChannel msgr.Channel
           && Cache.checkEmotionToggleChannel msgr.Channel then
            match msgr.Message with
            | SMOrc react ->
                Cache.updateLastReactTimeChannel msgr.Channel

                APITwitch.IRC.sendMessage
                    { Channel = msgr.Channel
                      Message = react
                      Writer = rw.Writer }
            | PogChamp react ->
                Cache.updateLastReactTimeChannel msgr.Channel

                APITwitch.IRC.sendMessage
                    { Channel = msgr.Channel
                      Message = react
                      Writer = rw.Writer }
            | Greetings react ->
                Cache.updateLastReactTimeChannel msgr.Channel

                APITwitch.IRC.sendMessage
                    { Channel = msgr.Channel
                      Message = react
                      Writer = rw.Writer }
            | NoReact -> ()
        else
            ()

    let handleMasterCommands (msgr: MessageRead) (rw: ReaderWriter) =
        let splited = msgr.Message.ToLower().Split()

        if splited.Length < 1 then
            ()
        else if splited.[0].Length > 1 then
            Cache.checkPrefixChannel msgr.Channel
            |> function
            | Ok (prefix) ->
                if string splited.[0].[0] = prefix then
                    let command = splited.[0].[1..]

                    match command with
                    | "setprefix" ->
                        APITwitch.IRC.sendMessage
                            { Channel = msgr.Channel
                              Message = Commands.updateChannelSettingDataBase msgr rw ChannelSettings.Prefix
                              Writer = rw.Writer }
                    | "settoggle" ->
                        APITwitch.IRC.sendMessage
                            { Channel = msgr.Channel
                              Message = Commands.updateChannelSettingDataBase msgr rw ChannelSettings.Toggle
                              Writer = rw.Writer }
                    | "setreactcd" ->
                        APITwitch.IRC.sendMessage
                            { Channel = msgr.Channel
                              Message = Commands.updateChannelSettingDataBase msgr rw ChannelSettings.EmotionCoolDown
                              Writer = rw.Writer }
                    | "setreacttoggle" ->
                        APITwitch.IRC.sendMessage
                            { Channel = msgr.Channel
                              Message = Commands.updateChannelSettingDataBase msgr rw ChannelSettings.EmotionToggle
                              Writer = rw.Writer }
                    | _ -> ()
                else
                    ()
            | _ -> ()
        else
            ()

    let handleHelper (msgr: MessageRead) (rw: ReaderWriter) =
        let splited = msgr.Message.ToLower().Split()

        if splited.Length < 1 then
            ()
        else if splited.[0].Length > 1 then
            Cache.checkPrefixChannel msgr.Channel
            |> function
            | Ok (prefix) ->
                if string splited.[0].[0] = prefix then
                    let command = splited.[0].[1..]

                    match command with
                    | "help" ->
                        let commands = Commands.commandList msgr rw

                        let masterHelper =
                            if msgr.RoomID = msgr.User.UserID then
                                "setprefix, settoggle, setreactcd, setreacttoggle "
                            else
                                ""

                        let getCommands =
                            List.filter
                                (fun elem ->
                                    match elem with
                                    | el when not (List.contains msgr.Channel el.Ban) ->
                                        match el.Channel with
                                        | All -> true
                                        | Channel (ch) -> ch = msgr.Channel
                                        | ChannelList (cl) -> List.contains msgr.Channel cl
                                    | _ -> false)
                                commands

                        APITwitch.IRC.sendMessage
                            { Channel = msgr.Channel
                              Message =
                                  "Список доступных команд: "
                                  + masterHelper
                                  + (List.collect (fun (elem: CommandList) -> [ elem.cmdName.Head ]) getCommands
                                     |> List.reduce (fun acc elem -> acc + ", " + elem))
                                  + Commands.listCommandDataBase msgr
                              Writer = rw.Writer }
                    | _ -> ()
                else
                    ()
            | _ -> ()
        else
            ()

    let handleCache msgr rw =
        Cache.handleCacheLove ()
        Cache.handleCacheUsers msgr
        Cache.handleCacheCatDown ()
        Cache.handleCacheCatDownReward rw
