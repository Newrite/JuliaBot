module TWBot.ActivePatterns

open TWBot
open TypesDefinition
open System

let (|Prefix|EmotionCoolDown|Toggle|EmotionToggle|) value =
    match value with
    | ChannelSettings.Prefix -> Prefix("Prefix")
    | ChannelSettings.EmotionCoolDown -> EmotionCoolDown("EmotionCoolDown")
    | ChannelSettings.Toggle -> Toggle("Toggle")
    | ChannelSettings.EmotionToggle -> EmotionToggle("EmotionToggle")

let (|APIBearerToken|) value =
    match value with
    | BotSettings.APIBearerToken -> APIBearerToken("APIBearerToken")

let (|ResolveChannelString|) (channel: string) =
    match channel with
    | "reflyq" -> Some(Reflyq)
    | "newr1te" -> Some(Newrite)
    | "xartasovplay" -> Some(Kotik)
    | "xandr_sh" -> Some(XandrSH)
    | "markus242" -> Some(Markus)
    | "cryo_0" -> Some(Cryo)
    | "felicia_nought" -> Some(Felicia)
    | "lebelius" -> Some(Lebelius)
    | "enemycbc" -> Some(Enemy)
    | "lievrefru" -> Some(Lievrefr)
    | "madelineqt" -> Some(Madelinqa)
    | "kaelia_kael" -> Some(Kaelia)
    | "crtzein" -> Some(Zein)
    | "desmond_hh" -> Some(Desmond)
    | _ -> None

let resolveUser (roomID: string) (user: User) =
    if roomID = user.UserID then
        Broadcaster
    elif user.isModerator then
        Moderator
    elif user.isSubscriber && user.isVIP then
        VIPSubscriber
    elif user.isSubscriber then
        Subscriber
    elif user.isVIP then
        VIP
    else
        Unsubscriber

let resolveChannelString (channel: string) =
    match channel with
    | ResolveChannelString name -> name

//#nowarn "0052"
let (|SubVin|SubLoose|Streamer|Moder|Nill|) user =
    let random =
        Random(int DateTime.Now.Ticks).Next(0, 99)

    match user with
    | Broadcaster -> Streamer
    | Subscriber ->
        if random >= 50 then
            SubVin
        else
            SubLoose
    | VIPSubscriber ->
        if random >= 75 then
            SubVin
        else
            SubLoose
    | VIP ->
        if random >= 25 then
            SubVin
        else
            SubLoose
    | Unsubscriber ->
        if random >= 15 then
            SubVin
        else
            SubLoose
    | Moderator -> Moder
    | NotFound -> Nill

let (|VipVin|VipLoose|Streamer|Moder|Nill|) user =
    let random =
        Random(int DateTime.Now.Ticks).Next(0, 99)

    match user with
    | Broadcaster -> Streamer
    | Subscriber ->
        if random >= 75 then
            VipVin
        else
            VipLoose
    | VIPSubscriber ->
        if random >= 85 then
            VipVin
        else
            VipLoose
    | VIP ->
        if random >= 50 then
            VipVin
        else
            VipLoose
    | Unsubscriber ->
        if random >= 25 then
            VipVin
        else
            VipLoose
    | Moderator -> Moder
    | NotFound -> Nill

let (|UnsubVin|UnsubLoose|Streamer|Moder|Nill|) user =
    let random =
        Random(int DateTime.Now.Ticks).Next(0, 99)

    match user with
    | Broadcaster -> Streamer
    | Subscriber ->
        if random >= 85 then
            UnsubVin
        else
            UnsubLoose
    | VIPSubscriber ->
        if random >= 95 then
            UnsubVin
        else
            UnsubLoose
    | VIP ->
        if random >= 75 then
            UnsubVin
        else
            UnsubLoose
    | Unsubscriber ->
        if random >= 50 then
            UnsubVin
        else
            UnsubLoose
    | Moderator -> Moder
    | NotFound -> Nill

let (|SubVipVin|SubVipLoose|Streamer|Moder|Nill|) user =
    let random =
        Random(int DateTime.Now.Ticks).Next(0, 99)

    match user with
    | Broadcaster -> Streamer
    | Subscriber ->
        if random >= 25 then
            SubVipVin
        else
            SubVipLoose
    | VIPSubscriber ->
        if random >= 50 then
            SubVipVin
        else
            SubVipLoose
    | VIP ->
        if random >= 15 then
            SubVipVin
        else
            SubVipLoose
    | Unsubscriber ->
        if random >= 5 then
            SubVipVin
        else
            SubVipLoose
    | Moderator -> Moder
    | NotFound -> Nill

let (|TryKillSub|TryKillVIP|TryKillStreamer|TryKillModer|TryKillNill|TryKillUnsub|) user =
    match user with
    | Broadcaster -> TryKillStreamer
    | Subscriber -> TryKillSub
    | VIPSubscriber -> TryKillSub
    | VIP -> TryKillVIP
    | Unsubscriber -> TryKillUnsub
    | Moderator -> TryKillModer
    | NotFound -> TryKillNill

let (|SMOrc|PogChamp|Greetings|NoReact|) (msgr: MessageRead) =
    let message = msgr.Message

    if
        message.ToLower().Contains("привет")
        || message.ToLower().Contains("дарова")
        || message.ToLower().Contains("hello")
    then
        Greetings "/me MrDestructoid 04 1F 04 40 04 38 04 32 04 35 04 42 MrDestructoid"
    elif message.Contains("PogChamp") then
        PogChamp "PogChamp"
    elif message.Contains("SMOrc") then
        SMOrc "SMOrc"
    else
        NoReact
