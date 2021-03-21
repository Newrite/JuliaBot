module TWBot.TypesDefinition

//Типы в основном под базу данных
type ChannelSettings =
    | Prefix
    | EmotionCoolDown
    | Toggle
    | EmotionToggle

type BotSettings = | APIBearerToken

type BotSetting =
    { bSetting: BotSettings
      bValue: string }

type ChannelSetting =
    { chSetting: ChannelSettings
      chValue: string }

type ChannelCommand = { chCommand: string; chAnswer: string }


//Типы под твич
type Channels =
    | Reflyq
    | Newrite
    | Kotik
    | XandrSH
    | Markus
    | Felicia
    | Desmond
    | Lebelius
    | Enemy
    | Lievrefr
    | Cryo
    member self.String =
        match self with
        | Reflyq -> "reflyq"
        | Newrite -> "newr1te"
        | Kotik -> "xartasovplay"
        | XandrSH -> "xandr_sh"
        | Markus -> "markus242"
        | Felicia -> "felicia_nought"
        | Desmond -> "desmond_hh"
        | Lebelius -> "lebelius"
        | Enemy -> "enemycbc"
        | Lievrefr -> "lievrefru"
        | Cryo -> "cryo_0"

type StatusUser =
    | Broadcaster
    | Moderator
    | VIPSubscriber
    | Subscriber
    | VIP
    | Unsubscriber
    | NotFound

type CutDownAnswer =
    { AnswFunc: Lazy<unit>
      Answer: string }

type ChannelOption =
    | Channel of Channels
    | ChannelList of Channels list
    | All

type CommandList =
    { cmdName: string list
      Command: Lazy<ChannelCommand>
      Channel: ChannelOption
      Ban: Channels list }

type RewardList =
    { RewardCode: string
      Command: Lazy<string>
      Channel: Channels }

type User =
    { Name: string
      DisplayName: string
      isModerator: bool
      isSubscriber: bool
      isVIP: bool
      isTurbo: bool
      UserID: string }

type MessageRead =
    { Channel: Channels
      RoomID: string
      User: User
      Message: string
      //Command: Option<Lazy<ChannelCommand>>
      RewardCode: Option<string> }

type ReaderWriter =
    { Reader: System.IO.StreamReader
      Writer: System.IO.StreamWriter }

type MessageWrite =
    { Channel: Channels
      Message: string
      Writer: System.IO.StreamWriter }

type Bot = Bot of System.Net.Sockets.TcpClient

//Кэш
type CacheCatDown =
    { TimeWhen: int64
      TimeHowLongKilled: int64
      TimeHowLongCantKill: int64
      Channel: Channels
      WhoKill: string
      WhoKilled: string }

type CacheCatDownReward =
    { TimeWhen: int64
      TimeHowLong: int64
      Channel: Channels
      WhoKilled: string }

type CacheUser = { Channel: Channels; User: User }

type CacheLove =
    { LoverName: string
      LovedName: string
      TimeWhen: int64
      PercentLove: int }

type CacheChannelSettings =
    { Prefix: string
      EmotionCoolDown: int64
      Toggle: bool
      EmotionToggle: bool
      LastReactTime: int64 }

type CacheChannelCommands =
    { ListCMD: Channels * ChannelCommand list }

//Типы под апи твича
//http://tmi.twitch.tv/group/user/<channel>/chatters
type _Links = { link: Option<string> }

type Chatters =
    { broadcaster: Option<string list>
      vips: Option<string list>
      moderators: Option<string list>
      staff: Option<string list>
      admins: Option<string list>
      global_mods: Option<string list>
      viewers: Option<string list> }

type Chat =
    { _links: _Links
      chatter_count: int
      chatters: Chatters }

type ``OAuth client credentials flow`` =
    { access_token: string
      expires_in: uint
      scope: Option<string list>
      token_type: string }

type Pagination = { cursor: Option<string> }

//https://dev.twitch.tv/docs/api/reference#get-users
type GetUsersData =
    { id: string
      login: string
      display_name: string
      ``type``: string
      broadcaster_type: string
      description: string
      profile_image_url: string
      offline_image_url: string
      view_count: uint
      email: Option<string>
      created_at: string }

type GetUsers = { data: GetUsersData list }

//https://dev.twitch.tv/docs/api/reference#search-channels
type SearchChannelsData =
    { broadcaster_language: string
      broadcaster_login: string
      display_name: string
      game_id: string
      id: string
      is_live: bool
      tags_ids: Option<string list>
      thumbnail_url: string
      title: string
      started_at: string }

type SearchChannels =
    { data: SearchChannelsData list
      pagination: Pagination }

//https://dev.twitch.tv/docs/api/reference#get-streams
type GetStreamsData =
    { id: string
      user_id: string
      user_login: string
      user_name: string
      game_id: string
      ``type``: string
      title: string
      viewer_count: uint
      started_at: string
      language: string
      thumbnail_url: string
      tag_ids: Option<string list> }

type GetStreams =
    { data: GetStreamsData list
      pagination: Pagination }

type GetUserSubscriptionData =
    { broadcaster_id: string
      broadcaster_name: string
      broadcaster_login: string
      is_gift: bool
      tier: string }

type GetUserSubscribtion = { data: GetUserSubscriptionData list }
