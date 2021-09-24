module TWBot.BotTwitch

open TWBot

open System
open TypesDefinition
open ActivePatterns
open DataBase

module private Utils =

    let minute = 1000 * 60

    let startTime = DateTime.Now

    let Time () = let x = DateTime.Now in x.Ticks / 10000000L

    let rand min max =
        let rndGen = Random(let x = DateTime.Now in x.Ticks |> int)
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

                ok
                |> List.tryFind
                   ^ fun (elem: string) ->
                       match nickname with
                       | Some (nick) -> elem.Contains(nick)
                       | None -> false
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

            let private catDownRewardFunction defendNickname howLongMuted (ctx: MessageContext) =
                APITwitch.IRC.sendRaw
                    ctx.ReaderWriter
                    (sprintf
                        "PRIVMSG #%s :/timeout %s %s\n\r"
                        ctx.MessageRead.Channel.String
                        defendNickname
                        howLongMuted)

                Cache.addCacheCatDownReward
                    { TimeWhen = Utils.Time()
                      TimeHowLong = Int64.Parse(howLongMuted)
                      Channel = ctx.MessageRead.Channel
                      WhoKilled = defendNickname }

            let private catDownFunction defendNickname killer howLongMuted howLongCantUse (ctx: MessageContext) =
                APITwitch.IRC.sendRaw
                    ctx.ReaderWriter
                    (sprintf
                        "PRIVMSG #%s :/timeout %s %s\n\r"
                        ctx.MessageRead.Channel.String
                        defendNickname
                        howLongMuted)

                Cache.addCacheCatDown
                    { TimeWhen = Utils.Time()
                      TimeHowLongKilled = Int64.Parse(howLongMuted)
                      TimeHowLongCantKill = Int64.Parse(howLongCantUse)
                      Channel = ctx.MessageRead.Channel
                      WhoKill = killer
                      WhoKilled = defendNickname }

            let answerSubVin (ctx: MessageContext) defendNickname =
                [| { AnswFunc =
                         lazy (catDownFunction defendNickname ctx.MessageRead.User.Name inMuteTime userMuteTime ctx)
                     Answer =
                         sprintf
                             "/me %s произносит FUS RO Dah и несчастного %s сдувает нахуй из чатика roflanEbalo"
                             ctx.MessageRead.User.DisplayName
                             defendNickname }
                   { AnswFunc =
                         lazy (catDownFunction defendNickname ctx.MessageRead.User.Name inMuteTime userMuteTime ctx)
                     Answer =
                         sprintf
                             "/me %s закидывает грозовыми порошками бедного %s, ужасное зрелище monkaS"
                             ctx.MessageRead.User.DisplayName
                             defendNickname }
                   { AnswFunc =
                         lazy (catDownFunction defendNickname ctx.MessageRead.User.Name inMuteTime userMuteTime ctx)
                     Answer =
                         sprintf
                             "/me %s выпускает шквал фаерболов из посоха, от %s, осталась лишь горстка пепла REEeee"
                             ctx.MessageRead.User.DisplayName
                             defendNickname }
                   { AnswFunc =
                         lazy (catDownFunction defendNickname ctx.MessageRead.User.Name inMuteTime userMuteTime ctx)
                     Answer =
                         sprintf
                             "/me %s с помощью погребения изолирует %s от чатика Minik"
                             ctx.MessageRead.User.DisplayName
                             defendNickname }
                   { AnswFunc =
                         lazy (catDownFunction defendNickname ctx.MessageRead.User.Name inMuteTime userMuteTime ctx)
                     Answer =
                         sprintf
                             "/me %s знакомит %s со своим дреморой, вам лучше не знать подробностей Kappa"
                             ctx.MessageRead.User.DisplayName
                             defendNickname }
                   { AnswFunc =
                         lazy (catDownFunction defendNickname ctx.MessageRead.User.Name inMuteTime userMuteTime ctx)
                     Answer =
                         sprintf
                             "/me %s обернувшись вервольфом раздирает на куски бедного %s"
                             ctx.MessageRead.User.DisplayName
                             defendNickname } |]

            let answerVin (ctx: MessageContext) defendNickname =
                [| { AnswFunc =
                         lazy (catDownFunction defendNickname ctx.MessageRead.User.Name inMuteTime userMuteTime ctx)
                     Answer =
                         sprintf
                             "/me %s запускает фаербол в ничего не подозревающего %s и он сгорает дотла..."
                             ctx.MessageRead.User.DisplayName
                             defendNickname }
                   { AnswFunc =
                         lazy (catDownFunction defendNickname ctx.MessageRead.User.Name inMuteTime userMuteTime ctx)
                     Answer =
                         sprintf
                             "/me %s подчиняет волю %s с помощью иллюзии, теперь он может делать с ним, что хочет gachiBASS"
                             ctx.MessageRead.User.DisplayName
                             defendNickname }
                   { AnswFunc =
                         lazy
                             (catDownFunction defendNickname ctx.MessageRead.User.Name inMuteTime userMuteTime ctx

                              catDownFunction
                                  ctx.MessageRead.User.Name
                                  ctx.MessageRead.User.Name
                                  inMuteTime
                                  userMuteTime
                                  ctx)
                     Answer =
                         sprintf
                             "/me %s с разбега совершает сокрушительный удар по черепушке %s, кто же знал, что %s решит надеть колечко малого отражения roflanEbalo"
                             ctx.MessageRead.User.DisplayName
                             defendNickname
                             defendNickname }
                   { AnswFunc =
                         lazy (catDownFunction defendNickname ctx.MessageRead.User.Name inMuteTime userMuteTime ctx)
                     Answer =
                         sprintf
                             "/me %s подкравшись к %s перерезает его горло, всё было тихо, ни шума ни крика..."
                             ctx.MessageRead.User.DisplayName
                             defendNickname }
                   { AnswFunc =
                         lazy (catDownFunction defendNickname ctx.MessageRead.User.Name inMuteTime userMuteTime ctx)
                     Answer =
                         sprintf
                             "/me %s подкидывает яд в карманы %s, страшная смерть..."
                             ctx.MessageRead.User.DisplayName
                             defendNickname }
                   { AnswFunc =
                         lazy (catDownFunction defendNickname ctx.MessageRead.User.Name inMuteTime userMuteTime ctx)
                     Answer =
                         sprintf
                             "/me %s взламывает жопу %s, теперь он в его полном распоряжении gachiHYPER"
                             ctx.MessageRead.User.DisplayName
                             defendNickname } |]

            let answerSubLoose (ctx: MessageContext) defendNickname =
                [| { AnswFunc = lazy ()
                     Answer =
                         sprintf
                             "/me %s произносит FUS RO Dah, но у %s маг. 50%s резиста и он стоит как ни в чем не бывало peepoClown"
                             ctx.MessageRead.User.DisplayName
                             defendNickname
                             "%" }
                   { AnswFunc =
                         lazy
                             (catDownFunction
                                 ctx.MessageRead.User.Name
                                 ctx.MessageRead.User.Name
                                 inMuteTime
                                 userMuteTime
                                 ctx)
                     Answer =
                         sprintf
                             "/me %s забыл зарядить свой посох, результат предсказуем Kapp"
                             ctx.MessageRead.User.DisplayName } |]

            let answerLoose (ctx: MessageContext) defendNickname =
                [| { AnswFunc = lazy ()
                     Answer =
                         sprintf
                             "/me %s мастерским выстрелом поражает голову %s, стрела проходит на вылет, жизненноважные органы не задеты roflanEbalo"
                             ctx.MessageRead.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy ()
                     Answer =
                         sprintf
                             "/me %s пытается поразить %s молнией, но кап абсорба говорит - НЕТ! EZ"
                             ctx.MessageRead.User.DisplayName
                             defendNickname }
                   { AnswFunc =
                         lazy
                             (catDownFunction defendNickname ctx.MessageRead.User.Name inMuteTime userMuteTime ctx

                              catDownFunction
                                  ctx.MessageRead.User.Name
                                  ctx.MessageRead.User.Name
                                  inMuteTime
                                  userMuteTime
                                  ctx)
                     Answer =
                         sprintf
                             "/me %s подкрадывается к %s, но вдруг из ниоткуда появившийся медведь убивает их обоих roflanEbalo"
                             ctx.MessageRead.User.DisplayName
                             defendNickname }
                   { AnswFunc =
                         lazy
                             (catDownFunction
                                 ctx.MessageRead.User.Name
                                 ctx.MessageRead.User.Name
                                 inMuteTime
                                 userMuteTime
                                 ctx)
                     Answer =
                         sprintf
                             "/me %s запускает фаербол в  %s, но он успевает защититься зеркалом Шалидора и вы погибаете..."
                             ctx.MessageRead.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy ()
                     Answer =
                         sprintf
                             "/me %s стреляет из лука в %s, 1ое попадание, 2ое, 3ье, 10ое.. но %s всё еще жив, а хули ты хотел от луков? roflanEbalo"
                             ctx.MessageRead.User.DisplayName
                             defendNickname
                             defendNickname }
                   { AnswFunc =
                         lazy
                             (catDownFunction
                                 ctx.MessageRead.User.Name
                                 ctx.MessageRead.User.Name
                                 inMuteTime
                                 userMuteTime
                                 ctx)
                     Answer =
                         sprintf
                             "/me %s завидев %s хорошенько разбегается, чтобы нанести удар и вдруг.. падает без сил так и не добежав до %s, а вот нехуй альтмером в тяже играть roflanEbalo"
                             ctx.MessageRead.User.DisplayName
                             defendNickname
                             defendNickname }
                   { AnswFunc =
                         lazy
                             (catDownFunction
                                 ctx.MessageRead.User.Name
                                 ctx.MessageRead.User.Name
                                 inMuteTime
                                 userMuteTime
                                 ctx)
                     Answer =
                         sprintf
                             "/me %s пытается подкрасться к %s, но вдруг - вас заметили roflanEbalo"
                             ctx.MessageRead.User.DisplayName
                             defendNickname } |]

            let answerReward (ctx: MessageContext) defendNickname (mutedTime: string) =


                [| { AnswFunc = lazy (catDownRewardFunction defendNickname mutedTime ctx)
                     Answer =
                         sprintf
                             "/me %s, вставляет кляп в рот ничего не подозревающего %s"
                             ctx.MessageRead.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy (catDownRewardFunction defendNickname mutedTime ctx)
                     Answer = sprintf "/me %s, помолчи немного дружок, ты всех утомил" defendNickname }
                   { AnswFunc = lazy (catDownRewardFunction defendNickname mutedTime ctx)
                     Answer =
                         sprintf
                             "/me %s, отправляет %s отдыхать, чат может спать спокойно"
                             ctx.MessageRead.User.DisplayName
                             defendNickname }
                   { AnswFunc = lazy (catDownRewardFunction defendNickname mutedTime ctx)
                     Answer = sprintf "/me душный %s больше не потревожит вас" defendNickname }
                   { AnswFunc = lazy (catDownRewardFunction defendNickname mutedTime ctx)
                     Answer =
                         sprintf
                             "/me %s, проветривает чатик от присутствия %s"
                             ctx.MessageRead.User.DisplayName
                             defendNickname } |]

        let catDown (ctx: MessageContext) =
            if ctx.MessageRead.User.Name = "ifozar" then
                APITwitch.IRC.sendRaw
                    ctx.ReaderWriter
                    (sprintf "PRIVMSG #%s :/timeout %s 300\n\r" ctx.MessageRead.Channel.String ctx.MessageRead.User.Name)

                sprintf @"%s заебал уже эту хуйню писать" ctx.MessageRead.User.DisplayName
            else
                match resolveUser ctx.MessageRead.RoomID ctx.MessageRead.User with
                | Broadcaster -> sprintf "Стримлер ты что пишешь..."
                | Moderator -> sprintf "Моё уважение модераторскому корпусу, но нет roflanZdarova"
                | VIPSubscriber -> sprintf "Можно пожалуйста постримить? PepeHands"
                | Subscriber -> sprintf "Зачем ты это делаешь? roflanZachto"
                | VIP -> sprintf "Ты ходишь по тонкому льду, випчик.. Ладно живи roflanEbalo"
                | Unsubscriber ->
                    APITwitch.IRC.sendRaw
                        ctx.ReaderWriter
                        (sprintf
                            "PRIVMSG #%s :/timeout %s 120\n\r"
                            ctx.MessageRead.Channel.String
                            ctx.MessageRead.User.Name)

                    sprintf "Я тебя щас нахуй вырублю, ансаб блять НЫА roflanEbalo"
                | NotFound -> sprintf "/me какая-то непонятная хрень...%s" ctx.MessageRead.User.Name

        let rewardMute (ctx: MessageContext) =
            let activation mutedTime nick =
                let array = Answers.answerReward ctx nick mutedTime

                let elem =
                    Array.item (Utils.rand 0 array.Length) array

                elem.AnswFunc.Force()
                elem.Answer

            let defenderStatus, defenderName =
                Utils.resolveFirstStatusAndName ctx.MessageRead

            match defenderName with
            | Some (name) ->
                if name = ctx.MessageRead.User.Name then
                    match ctx.MessageRead.User.isSubscriber with
                    | true ->
                        APITwitch.IRC.sendRaw
                            ctx.ReaderWriter
                            (sprintf
                                "PRIVMSG #%s :/timeout %s 150\n\r"
                                ctx.MessageRead.Channel.String
                                ctx.MessageRead.User.Name)
                    | false ->
                        APITwitch.IRC.sendRaw
                            ctx.ReaderWriter
                            (sprintf
                                "PRIVMSG #%s :/timeout %s 300\n\r"
                                ctx.MessageRead.Channel.String
                                ctx.MessageRead.User.Name)

                    sprintf "%s, я давно хотела это сделать peepoGun" ctx.MessageRead.User.DisplayName
                else
                    match defenderStatus with
                    | TryKillSub -> sprintf "%s, минус две тыщи roflanEbalo" ctx.MessageRead.User.DisplayName
                    | TryKillModer -> sprintf "%s, ты чё дурачокус? peepoClown" ctx.MessageRead.User.DisplayName
                    | TryKillStreamer -> sprintf "%s, ага как скажешь roflanEbalo" ctx.MessageRead.User.DisplayName
                    | TryKillNill -> sprintf "/me не может найти кому это она должна затолкать свой банхаммер"
                    | TryKillVIP -> activation mutedTimeVIP name
                    | TryKillUnsub -> activation mutedTimeOther name
            | None -> "Ну ты и дурачокус, пустое сообщение отправил..."

        let catDownUserAnswer (ctx: MessageContext) (victory: bool) defenderNickName =
            let random = Utils.rand 0 99

            let activation (array: CutDownAnswer array) =
                let elem =
                    Array.item (Utils.rand 0 array.Length) array

                elem.AnswFunc.Force()
                elem.Answer

            match victory with
            | true ->
                if ctx.MessageRead.User.isSubscriber && random > 50 then
                    activation
                    <| Answers.answerSubVin ctx defenderNickName
                else
                    activation
                    <| Answers.answerVin ctx defenderNickName
            | false ->
                if ctx.MessageRead.User.isSubscriber && random > 50 then
                    activation
                    <| Answers.answerSubLoose ctx defenderNickName
                else
                    activation
                    <| Answers.answerLoose ctx defenderNickName

        let catDownUserStart (ctx: MessageContext) attackUserStatus defenderUserStatus defenderNickName =
            if Cache.checkCacheCatDownKill ctx.MessageRead.User.Name ctx.MessageRead.Channel then
                APITwitch.IRC.sendRaw ctx.ReaderWriter
                <| sprintf "PRIVMSG #%s :/timeout @%s 30" ctx.MessageRead.Channel.String ctx.MessageRead.User.Name

                sprintf "Камень бьет ножницы, а я бью твое ебало спамер, НЫА roflanEbalo"
            elif ctx.MessageRead.User.Name = defenderNickName then
                sprintf "Осуждаю roflanEbalo"
            elif Cache.checkCacheCatDownKilled defenderNickName ctx.MessageRead.Channel then
                sprintf "%s уже вырублен" defenderNickName
            else
                match attackUserStatus with
                | Broadcaster ->
                    match Utils.nameSecondWord ctx.MessageRead with
                    | Some (name) ->
                        if defenderUserStatus = NotFound then
                            sprintf "/me достает БФГ9000, но не может найти цель %s..." defenderNickName
                        else
                            APITwitch.IRC.sendRaw ctx.ReaderWriter
                            <| sprintf
                                "PRIVMSG #%s :/timeout @%s 30"
                                ctx.MessageRead.Channel.String
                                ctx.MessageRead.User.Name

                            sprintf
                                "/me %s произносит YOL TooR Shul и испепеляет %s monkaX"
                                ctx.MessageRead.User.DisplayName
                                name
                    | None -> sprintf "/me ты куда пыхаешь..."
                | Moderator -> sprintf "%s, ты что забыл свой банхаммер дома? monkaHmm" ctx.MessageRead.User.DisplayName
                | NotFound -> sprintf "/me something wrong... %s" ctx.MessageRead.User.Name
                | Subscriber ->
                    match defenderUserStatus with
                    | SubVin -> catDownUserAnswer ctx true defenderNickName
                    | SubLoose -> catDownUserAnswer ctx false defenderNickName
                    | Moder -> sprintf "%s, Agakakskagesh Agakakskagesh Agakakskagesh" ctx.MessageRead.User.DisplayName
                    | Streamer -> "У стримера бесплотность с капом отката на крики roflanEbalo"
                    | Nill -> sprintf "/me достает БФГ9000, но не может найти цель %s..." defenderNickName
                    | _ -> "/me something wrong..."
                | VIPSubscriber ->
                    match defenderUserStatus with
                    | SubVipVin -> catDownUserAnswer ctx true defenderNickName
                    | SubVipLoose -> catDownUserAnswer ctx false defenderNickName
                    | Moder -> sprintf "%s, Agakakskagesh Agakakskagesh Agakakskagesh" ctx.MessageRead.User.DisplayName
                    | Streamer -> "У стримера бесплотность с капом отката на крики roflanEbalo"
                    | Nill -> sprintf "/me достает БФГ9000, но не может найти цель %s..." defenderNickName
                | VIP ->
                    match defenderUserStatus with
                    | VipVin -> catDownUserAnswer ctx true defenderNickName
                    | VipLoose -> catDownUserAnswer ctx false defenderNickName
                    | Moder -> sprintf "%s, Agakakskagesh Agakakskagesh Agakakskagesh" ctx.MessageRead.User.DisplayName
                    | Streamer -> "У стримера бесплотность с капом отката на крики roflanEbalo"
                    | Nill -> sprintf "/me достает БФГ9000, но не может найти цель %s..." defenderNickName
                    | _ -> "/me something wrong..."
                | Unsubscriber ->
                    match defenderUserStatus with
                    | UnsubVin -> catDownUserAnswer ctx true defenderNickName
                    | UnsubLoose -> catDownUserAnswer ctx false defenderNickName
                    | Moder -> sprintf "%s, Agakakskagesh Agakakskagesh Agakakskagesh" ctx.MessageRead.User.DisplayName
                    | Streamer -> "У стримера бесплотность с капом отката на крики roflanEbalo"
                    | Nill -> sprintf "/me достает БФГ9000, но не может найти цель %s..." defenderNickName
                    | _ -> "/me something wrong..."

        let catDownUser (ctx: MessageContext) =
            let defenderUserStatus, defenderNickName =
                Utils.resolveSecondStatusAndName ctx.MessageRead

            printfn "%A %A" defenderUserStatus defenderNickName

            if defenderNickName = None then
                sprintf @"Ты дурачокус? кого вырубать roflanEbalo"
            else
                let attackUserStatus =
                    resolveUser ctx.MessageRead.RoomID ctx.MessageRead.User

                catDownUserStart ctx attackUserStatus defenderUserStatus defenderNickName.Value

    let love (ctx: MessageContext) =
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

        Utils.nameSecondWord ctx.MessageRead
        |> function
        | Some (lovedName) ->
            Cache.checkCacheLove ctx.MessageRead.User.DisplayName lovedName
            |> function
            | Some (lovers) -> loveMessage lovers.LoverName lovers.LovedName lovers.PercentLove
            | None ->
                let lovePercent = Utils.rand 0 100
                Cache.addCacheLove ctx.MessageRead.User.DisplayName lovedName lovePercent
                loveMessage ctx.MessageRead.User.DisplayName lovedName lovePercent
        | None -> sprintf "%s ****void тоже любит тебя!" ctx.MessageRead.User.DisplayName

    let uptime (ctx: MessageContext) =
        if APITwitch.Requests.checkOnline ctx.MessageRead.Channel then
            APITwitch.Requests.getStream ctx.MessageRead.Channel
            |> APITwitch.deserializeRespons<GetStreams>
            |> function
            | Ok data ->
                let sinceTime =
                    DateTime.Now
                    - DateTime.Parse(data.data.[0].started_at)

                let answer =
                    let days =
                        match sinceTime.Days with
                        | day when day > 0 -> sprintf "%dd " day
                        | _ -> ""

                    let time since suffix whenValue =
                        match since with
                        | sv when whenValue <> "" -> sprintf "%d%s" sv suffix
                        | sv when sv > 0 -> sprintf "%d%s" sv suffix
                        | _ -> ""

                    let hours = time sinceTime.Hours "h " days
                    let minutes = time sinceTime.Minutes "m " hours
                    let seconds = time sinceTime.Seconds "s" minutes
                    days + hours + minutes + seconds

                sprintf "Стрим длится уже %s." answer
            | Error (_) -> "Стрим офлайн."
        else
            "Стрим офлайн"

    let roulette (ctx: MessageContext) =
        if ctx.MessageRead.User.isModerator then
            "Хорошая попытка, модератор..."
        elif ctx.MessageRead.User.UserID = ctx.MessageRead.RoomID then
            "Вызывайте дурку BloodTrail"
        elif Utils.rand 0 99 < 50 then
            APITwitch.IRC.sendRaw
                ctx.ReaderWriter
                (sprintf
                    "PRIVMSG #%s :/me подносит револьвер к виску %s\n\r"
                    ctx.MessageRead.Channel.String
                    ctx.MessageRead.User.DisplayName)

            System.Threading.Thread.Sleep(2000)

            APITwitch.IRC.sendRaw
                ctx.ReaderWriter
                (sprintf
                    "PRIVMSG #%s :/timeout @%s 120 рулетка\n\r"
                    ctx.MessageRead.Channel.String
                    ctx.MessageRead.User.Name)

            sprintf
                "Револьвер выстреливает! %s погибает у чатлан на руках BibleThump 7"
                ctx.MessageRead.User.DisplayName
        else
            APITwitch.IRC.sendRaw
                ctx.ReaderWriter
                (sprintf
                    "PRIVMSG #%s :/me подносит револьвер к виску %s\n\r"
                    ctx.MessageRead.Channel.String
                    ctx.MessageRead.User.DisplayName)

            System.Threading.Thread.Sleep(2000)
            sprintf "Револьвер издает щелчок, %s выживает! PogChamp" ctx.MessageRead.User.DisplayName

    let harakiri (ctx: MessageContext) =
        if ctx.MessageRead.User.isModerator then
            "Хорошая попытка, модератор..."
        elif ctx.MessageRead.User.UserID = ctx.MessageRead.RoomID then
            "Вызывайте дурку BloodTrail"
        else
            sprintf "/timeout @%s 60 харакири" ctx.MessageRead.User.Name

    let addCommandDataBase (ctx: MessageContext) =
        Logger.Log.TraceDeb
        <| sprintf "addCommandDataBase %A" ctx.MessageRead

        let splited = ctx.MessageRead.Message.Split(' ')

        if (ctx.MessageRead.User.UserID
            <> ctx.MessageRead.RoomID)
           && (ctx.MessageRead.User.UserID <> "70592477") then
            "Access denied"
        elif splited.Length < 3 then
            "Ну и что добавлять? Команда или ответ не указаны."
        else
            let command = splited.[1].ToLower()

            let commandAnswer =
                splited.[2..]
                |> Array.reduce ^ fun acc elem -> acc + " " + elem

            SingleData.DB.addChannelCommand
                ctx.MessageRead.Channel
                { chCommand = command
                  chAnswer = commandAnswer }
            |> function
            | Ok (answer) ->
                Cache.updateCacheChannelCommands ctx.MessageRead.Channel
                answer
            | Error (err) ->
                Cache.updateCacheChannelCommands ctx.MessageRead.Channel
                err

    let deleteCommandDataBase (ctx: MessageContext) =
        Logger.Log.TraceDeb
        <| sprintf "deleteCommandDataBase %A" ctx.MessageRead

        let splited = ctx.MessageRead.Message.Split(' ')

        if (ctx.MessageRead.User.UserID
            <> ctx.MessageRead.RoomID)
           && (ctx.MessageRead.User.UserID <> "70592477") then
            "Access denied"
        elif splited.Length < 2 then
            "Ну и что удалять? Команда для удаления не указана."
        else

            Cache.resolveCommandCache ctx.MessageRead (splited.[1].ToLower())
            |> function
            | Some (command) ->
                SingleData.DB.deleteChannelCommand ctx.MessageRead.Channel command
                |> function
                | Ok (answer) ->
                    Cache.updateCacheChannelCommands ctx.MessageRead.Channel
                    answer
                | Error (err) -> err
            | None -> sprintf "Команда %s не найдена" (splited.[1].ToLower())

    let listCommandDataBase (ctx: MessageContext) =
        Logger.Log.TraceDeb
        <| sprintf "listCommandDataBase %A" ctx.MessageRead

        SingleData.DB.getCommands ctx.MessageRead.Channel
        |> function
        | Ok (list) ->
            if not list.IsEmpty then
                " Кастомные команды: "
                + (list
                   |> List.collect ^ fun elem -> [ elem.chCommand ]
                   |> List.reduce ^ fun acc elem -> acc + ", " + elem)
            else
                " Список кастомных команд пуст."
        | Error (err) -> err

    let updateCommandDataBase (ctx: MessageContext) =
        Logger.Log.TraceDeb
        <| sprintf "updateCommandDataBase %A" ctx.MessageRead

        let splited = ctx.MessageRead.Message.Split(' ')

        if (ctx.MessageRead.User.UserID
            <> ctx.MessageRead.RoomID)
           && (ctx.MessageRead.User.UserID <> "70592477") then
            "Access denied"
        elif splited.Length < 3 then
            "Ну и что обновлять? Команда для обновления либо новое значение не указаны."
        else

            let command = splited.[1].ToLower()

            let commandAnswer =
                splited.[2..]
                |> Array.reduce ^ fun acc elem -> acc + " " + elem

            Cache.resolveCommandCache ctx.MessageRead command
            |> function
            | Some (cmd) ->
                let newCommand = { cmd with chAnswer = commandAnswer }

                SingleData.DB.updateChannelCommand ctx.MessageRead.Channel newCommand
                |> function
                | Ok (answer) ->
                    Cache.updateCacheChannelCommands ctx.MessageRead.Channel
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

    let commandList () =

        [ { cmdName = [ "ping"; "pong"; "пинг" ]
            Command = (fun _ -> "pong")
            Channel = All
            Ban = [] }
          { cmdName = [ "roll"; "ролл" ]
            Command = (fun _ -> string (Utils.rand 1 20))
            Channel = All
            Ban = [] }
          { cmdName = [ "ball"; "8ball"; "шар" ]
            Command = (fun _ -> Array.item (Utils.rand 0 ball.Length) ball)
            Channel = All
            Ban = [] }
          { cmdName = [ "build"; "билд" ]
            Command = (fun _ -> Array.item (Utils.rand 0 buildAnswers.Length) buildAnswers)
            Channel = All
            Ban = [] }
          { cmdName = [ "julia"; "джулия"; "юля" ]
            Command = (fun _ -> Array.item (Utils.rand 0 genericAnswers.Length) genericAnswers)
            Channel = All
            Ban = [] }
          { cmdName = [ "вырубай" ]
            Command = Reflyq.catDown
            Channel = Channel(Reflyq)
            Ban = [] }
          { cmdName = [ "вырубить" ]
            Command = Reflyq.catDownUser
            Channel = Channel(Reflyq)
            Ban = [] }
          { cmdName = [ "love"; "люблю" ]
            Command = love
            Channel = All
            Ban = [] }
          { cmdName = [ "addcmd" ]
            Command = addCommandDataBase
            Channel = All
            Ban = [] }
          { cmdName = [ "deletecmd" ]
            Command = deleteCommandDataBase
            Channel = All
            Ban = [] }
          { cmdName = [ "updatecmd" ]
            Command = updateCommandDataBase
            Channel = All
            Ban = [] }
          { cmdName = [ "listcmd" ]
            Command = listCommandDataBase
            Channel = All
            Ban = [] }
          { cmdName = [ "live"; "жива" ]
            Command = (fun _ -> sprintf "%A" (DateTime.Now - Utils.startTime))
            Channel = All
            Ban = [] }
          { cmdName = [ "uptime"; "аптайм" ]
            Command = uptime
            Channel = All
            Ban = [] }
          { cmdName = [ "харакири"; "сеппуку" ]
            Command = harakiri
            Channel = All
            Ban = [ Reflyq; Kaelia ] }
          { cmdName = [ "рулетка" ]
            Command = roulette
            Channel = All
            Ban = [ Kotik; Reflyq; Kaelia ] } ]


    let rewardList () =
        [ { RewardCode = "fa297b45-75cc-4ef2-ba49-841b0fa86ec1"
            Command = Reflyq.rewardMute
            Channel = Reflyq } ]

module private Parse =

    let resolveCommandList (cmd: string) (ctx: MessageContext) =
        try
            Commands.commandList ()
            |> List.find
               ^ fun elem ->
                   match elem with
                   | el when
                       cmd </> el.cmdName
                       && not (ctx.MessageRead.Channel </> el.Ban) ->
                       match el.Channel with
                       | All -> true
                       | Channel (ch) -> ch = ctx.MessageRead.Channel
                       | ChannelList (cl) -> ctx.MessageRead.Channel </> cl
                   | _ -> false
            |> function
            | com -> Ok com.Command
        with eX -> Error eX.Message


    let resolveCommand (ctx: MessageContext) (resu: Result<string, string>) =
        match resu with
        | Ok (prefix) ->
            let cmd =
                if ctx
                    .MessageRead
                    .Message
                    .ToLower()
                       .StartsWith(prefix) then
                    ctx.MessageRead.Message.ToLower().Split(' ').[0]
                        .Substring(1)
                else
                    ""

            Cache.resolveCommandCache ctx.MessageRead cmd
            |> function
            | Some (ok) ->
                let commandAnswer =
                    ok.chAnswer.Replace("[<username>]", ctx.MessageRead.User.DisplayName)

                Ok(fun _ -> commandAnswer)
            | None (_) -> resolveCommandList cmd ctx

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
            |> Array.find
               ^ fun elem -> elem.Contains("display-name=")

        d.Substring(13)

    let userID (firstLineSplit: string array) =
        let d =
            firstLineSplit
            |> Array.find ^ fun elem -> elem.Contains("user-id=")

        d.Substring(8)

    let roomID (firstLineSplit: string array) =
        let d =
            firstLineSplit
            |> Array.find ^ fun elem -> elem.Contains("room-id=")

        d.Substring(8)

    let nickname (firstLineSplit: string array) =
        let d =
            firstLineSplit
            |> Array.find
               ^ fun elem ->
                   elem.Contains("!")
                   && elem.Contains(":")
                   && elem.Contains(".")

        d.Substring(d.IndexOf(':') + 1, (displayName firstLineSplit).Length)

    let rewardID (firstLineSplit: string array) =
        try
            let d =
                firstLineSplit
                |> Array.find
                   ^ fun elem -> elem.Contains("custom-reward-id=")

            Some(d.Substring(17))
        with eX -> None

    let message (secondLineSplit: string array) =
        let d =
            secondLineSplit
            |> Array.findIndex ^ fun elem -> elem.Contains(':')

        (((((secondLineSplit.[d..]
             |> Array.reduce ^ fun acc elem -> acc + " " + elem))))).[1..]


    let channel (secondLineSplit: string array) =
        let d =
            secondLineSplit
            |> Array.find ^ fun elem -> elem.Contains('#')

        d.Substring(1)

    let command (ctx: MessageContext) =
        Cache.checkPrefixChannel ctx.MessageRead.Channel
        |> resolveCommand ctx
        |> function
        | Ok (ok) -> Some(ok)
        | Error (err) ->
            printfn "%s" err
            None

module Handlers =

    let handleRewards (ctx: MessageContext) =
        match ctx.MessageRead.RewardCode with
        | Some (code) ->
            Commands.rewardList ()
            |> List.tryFind
               ^ fun (elem: RewardList) -> code = elem.RewardCode
            |> function
            | Some (reward) ->
                APITwitch.IRC.sendMessage
                    { Channel = reward.Channel
                      Message = reward.Command ctx
                      Writer = ctx.ReaderWriter.Writer }
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


    let handleCommands (ctx: MessageContext) =
        match Parse.command ctx with
        | Some (commandFunc) ->
            APITwitch.IRC.sendMessage
                { Channel = ctx.MessageRead.Channel
                  Message = commandFunc ctx
                  Writer = ctx.ReaderWriter.Writer }
        | None -> ()

    let handleReacts (ctx: MessageContext) =
        if Cache.checkLastReactTimeChannel ctx.MessageRead.Channel
           && Cache.checkEmotionToggleChannel ctx.MessageRead.Channel then
            match ctx.MessageRead with
            | SMOrc react ->
                Cache.updateLastReactTimeChannel ctx.MessageRead.Channel

                APITwitch.IRC.sendMessage
                    { Channel = ctx.MessageRead.Channel
                      Message = react
                      Writer = ctx.ReaderWriter.Writer }
            | PogChamp react ->
                Cache.updateLastReactTimeChannel ctx.MessageRead.Channel

                APITwitch.IRC.sendMessage
                    { Channel = ctx.MessageRead.Channel
                      Message = react
                      Writer = ctx.ReaderWriter.Writer }
            | Greetings react ->
                Cache.updateLastReactTimeChannel ctx.MessageRead.Channel

                APITwitch.IRC.sendMessage
                    { Channel = ctx.MessageRead.Channel
                      Message = react
                      Writer = ctx.ReaderWriter.Writer }
            | NoReact -> ()
        else
            ()

    let handleMasterCommands (ctx: MessageContext) =
        let splited =
            ctx.MessageRead.Message.ToLower().Split()

        if splited.Length < 1 then
            ()
        else if splited.[0].Length > 1 then
            Cache.checkPrefixChannel ctx.MessageRead.Channel
            |> function
            | Ok (prefix) ->
                if string splited.[0].[0] = prefix then
                    let command = splited.[0].[1..]

                    match command with
                    | "setprefix" ->
                        APITwitch.IRC.sendMessage
                            { Channel = ctx.MessageRead.Channel
                              Message =
                                  Commands.updateChannelSettingDataBase
                                      ctx.MessageRead
                                      ctx.ReaderWriter
                                      ChannelSettings.Prefix
                              Writer = ctx.ReaderWriter.Writer }
                    | "settoggle" ->
                        APITwitch.IRC.sendMessage
                            { Channel = ctx.MessageRead.Channel
                              Message =
                                  Commands.updateChannelSettingDataBase
                                      ctx.MessageRead
                                      ctx.ReaderWriter
                                      ChannelSettings.Toggle
                              Writer = ctx.ReaderWriter.Writer }
                    | "setreactcd" ->
                        APITwitch.IRC.sendMessage
                            { Channel = ctx.MessageRead.Channel
                              Message =
                                  Commands.updateChannelSettingDataBase
                                      ctx.MessageRead
                                      ctx.ReaderWriter
                                      ChannelSettings.EmotionCoolDown
                              Writer = ctx.ReaderWriter.Writer }
                    | "setreacttoggle" ->
                        APITwitch.IRC.sendMessage
                            { Channel = ctx.MessageRead.Channel
                              Message =
                                  Commands.updateChannelSettingDataBase
                                      ctx.MessageRead
                                      ctx.ReaderWriter
                                      ChannelSettings.EmotionToggle
                              Writer = ctx.ReaderWriter.Writer }
                    | _ -> ()
                else
                    ()
            | _ -> ()
        else
            ()

    let handleHelper (ctx: MessageContext) =
        let splited =
            ctx.MessageRead.Message.ToLower().Split()

        if splited.Length < 1 then
            ()
        else if splited.[0].Length > 1 then
            Cache.checkPrefixChannel ctx.MessageRead.Channel
            |> function
            | Ok (prefix) ->
                if string splited.[0].[0] = prefix then
                    let command = splited.[0].[1..]

                    match command with
                    | "help" ->
                        let commands = Commands.commandList ()

                        let masterHelper =
                            if ctx.MessageRead.RoomID = ctx.MessageRead.User.UserID then
                                "setprefix, settoggle, setreactcd, setreacttoggle "
                            else
                                ""

                        let getCommands =
                            commands
                            |> List.filter
                               ^ fun elem ->
                                   match elem with
                                   | el when not (List.contains ctx.MessageRead.Channel el.Ban) ->
                                       match el.Channel with
                                       | All -> true
                                       | Channel (ch) -> ch = ctx.MessageRead.Channel
                                       | ChannelList (cl) -> List.contains ctx.MessageRead.Channel cl
                                   | _ -> false

                        APITwitch.IRC.sendMessage
                            { Channel = ctx.MessageRead.Channel
                              Message =
                                  "Список доступных команд: "
                                  + masterHelper
                                  + (getCommands
                                     |> List.collect
                                        ^ fun (elem: CommandList) -> [ elem.cmdName.Head ]
                                     |> List.reduce ^ fun acc elem -> acc + ", " + elem)
                                  + Commands.listCommandDataBase ctx
                              Writer = ctx.ReaderWriter.Writer }
                    | _ -> ()
                else
                    ()
            | _ -> ()
        else
            ()

    let handleCache (ctx: MessageContext) =
        Cache.handleCacheLove ()
        Cache.handleCacheUsers ctx.MessageRead
        Cache.handleCacheCatDown ()
        Cache.handleCacheCatDownReward ctx.ReaderWriter
