



namespace TWBot
    
    module TokensData =
        
        val ClientID: string
        
        val SecretID: string
        
        val OAuth: string
        
        val Scope: string
        
        val Nickname: string
        
        val TGToken: string
        
        val reward: string

namespace TWBot
    
    module TypesDefinition =
        
        val inline (^) : f: ('a -> 'b) -> x: 'a -> 'b
        
        val inline (</>) : a: 'a -> al: seq<'a> -> bool when 'a: equality
        type Object with
            
            member inline In: al: seq<'a> -> bool when 'a: equality
        
        type ChannelSettings =
            | Prefix
            | EmotionCoolDown
            | Toggle
            | EmotionToggle
        
        type BotSettings = | APIBearerToken
        
        type BotSetting =
            {
              bSetting: BotSettings
              bValue: string
            }
        
        type ChannelSetting =
            {
              chSetting: ChannelSettings
              chValue: string
            }
        
        type ChannelCommand =
            {
              chCommand: string
              chAnswer: string
            }
        
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
            | Madelinqa
            | Kaelia
            | Chu8
            | Zein
            
            member String: string
            
            static member ToList: Channels list
            
            static member ToStringList: string list
        
        type StatusUser =
            | Broadcaster
            | Moderator
            | VIPSubscriber
            | Subscriber
            | VIP
            | Unsubscriber
            | NotFound
        
        type ChannelOption =
            | Channel of Channels
            | ChannelList of Channels list
            | All
        
        type User =
            {
              Name: string
              DisplayName: string
              isModerator: bool
              isSubscriber: bool
              isVIP: bool
              isTurbo: bool
              UserID: string
            }
        
        type MessageRead =
            {
              Channel: Channels
              RoomID: string
              User: User
              Message: string
              RewardCode: Option<string>
            }
        
        [<NoComparison>]
        type ReaderWriter =
            {
              Reader: System.IO.StreamReader
              Writer: System.IO.StreamWriter
            }
        
        [<NoComparison>]
        type MessageWrite =
            {
              Channel: Channels
              Message: string
              Writer: System.IO.StreamWriter
            }
        
        [<NoComparison>]
        type MessageContext =
            {
              MessageRead: MessageRead
              ReaderWriter: ReaderWriter
            }
        
        [<NoEquality; NoComparison>]
        type CommandList =
            {
              cmdName: string list
              Command: MessageContext -> string
              Channel: ChannelOption
              Ban: Channels list
            }
        
        [<NoEquality; NoComparison>]
        type RewardList =
            {
              RewardCode: string
              Command: MessageContext -> string
              Channel: Channels
            }
        
        [<NoComparison>]
        type CutDownAnswer =
            {
              AnswFunc: Lazy<unit>
              Answer: string
            }
        
        [<NoComparison>]
        type Bot = | Bot of System.Net.Sockets.TcpClient
        
        type CacheCatDown =
            {
              TimeWhen: int64
              TimeHowLongKilled: int64
              TimeHowLongCantKill: int64
              Channel: Channels
              WhoKill: string
              WhoKilled: string
            }
        
        type CacheCatDownReward =
            {
              TimeWhen: int64
              TimeHowLong: int64
              Channel: Channels
              WhoKilled: string
            }
        
        type CacheUser =
            {
              Channel: Channels
              User: User
            }
        
        type CacheLove =
            {
              LoverName: string
              LovedName: string
              TimeWhen: int64
              PercentLove: int
            }
        
        type CacheChannelSettings =
            {
              Prefix: string
              EmotionCoolDown: int64
              Toggle: bool
              EmotionToggle: bool
              LastReactTime: int64
            }
        
        type CacheChannelCommands =
            { ListCMD: Channels * ChannelCommand list }
        
        type _Links =
            { link: Option<string> }
        
        type Chatters =
            {
              broadcaster: Option<string list>
              vips: Option<string list>
              moderators: Option<string list>
              staff: Option<string list>
              admins: Option<string list>
              global_mods: Option<string list>
              viewers: Option<string list>
            }
        
        type Chat =
            {
              _links: _Links
              chatter_count: int
              chatters: Chatters
            }
        
        type ``OAuth client credentials flow`` =
            {
              access_token: string
              expires_in: uint
              scope: Option<string list>
              token_type: string
            }
        
        type Pagination =
            { cursor: Option<string> }
        
        type GetUsersData =
            {
              id: string
              login: string
              display_name: string
              ``type`` : string
              broadcaster_type: string
              description: string
              profile_image_url: string
              offline_image_url: string
              view_count: uint
              email: Option<string>
              created_at: string
            }
        
        type GetUsers =
            { data: GetUsersData list }
        
        type SearchChannelsData =
            {
              broadcaster_language: string
              broadcaster_login: string
              display_name: string
              game_id: string
              id: string
              is_live: bool
              tags_ids: Option<string list>
              thumbnail_url: string
              title: string
              started_at: string
            }
        
        type SearchChannels =
            {
              data: SearchChannelsData list
              pagination: Pagination
            }
        
        type GetStreamsData =
            {
              id: string
              user_id: string
              user_login: string
              user_name: string
              game_id: string
              game_name: string
              ``type`` : string
              title: string
              viewer_count: uint
              started_at: string
              language: string
              thumbnail_url: string
              tag_ids: Option<string list>
            }
        
        type GetStreams =
            {
              data: GetStreamsData list
              pagination: Pagination
            }
        
        type GetUserSubscriptionData =
            {
              broadcaster_id: string
              broadcaster_name: string
              broadcaster_login: string
              is_gift: bool
              tier: string
            }
        
        type GetUserSubscribtion =
            { data: GetUserSubscriptionData list }

namespace TWBot
    
    module Logger =
        
        val mutable fileNameMain: string
        
        val mutable fileNameException: string
        
        type LogLevel =
            | ErrorL
            | WarningL
            | InformationL
            | DebugL
            | ExceptionL
            
            member String: string
        
        module LogChat =
            
            val private messageUser: msgr: TypesDefinition.MessageRead -> string
            
            val private toWrite:
              messageToLog: string -> channel: TypesDefinition.Channels -> unit
            
            val private toWriteRaw: messageToLog: string -> unit
            
            val writePrintBot:
              messageToLog: string -> channel: TypesDefinition.Channels -> unit
            
            val writePrintRaw: messageToLog: string -> unit
            
            val writePrint: msgr: TypesDefinition.MessageRead -> unit
        
        module private Log =
            
            val toWrite:
              logLevel: LogLevel -> messageToLog: string -> nameMain: string
              -> nameEx: string -> unit
            
            val WritePrint:
              logLevel: LogLevel -> messageToLog: string -> nameMain: string
              -> nameEx: string -> unit
        
        type Log =
            
            new: unit -> Log
            
            static member
              TraceDeb: message: string * path: string * line: int -> unit
            
            static member
              TraceErr: message: string * path: string * line: int -> unit
            
            static member
              TraceExc: message: string * path: string * line: int -> unit
            
            static member
              TraceInf: message: string * path: string * line: int -> unit
            
            static member
              TraceWarn: message: string * path: string * line: int -> unit
            
            static member logFileEx: string with set
            
            static member logFileName: string with set

namespace TWBot
    
    module ActivePatterns =
        
        val (|Prefix|EmotionCoolDown|Toggle|EmotionToggle|) :
          value: TypesDefinition.ChannelSettings
            -> Choice<string,string,string,string>
        
        val (|APIBearerToken|) : value: TypesDefinition.BotSettings -> string
        
        val (|ResolveChannelString|) :
          channel: string -> TypesDefinition.Channels option
        
        val resolveUser:
          roomID: string -> user: TypesDefinition.User
            -> TypesDefinition.StatusUser
        
        val resolveChannelString:
          channel: string -> TypesDefinition.Channels option
        
        val (|SubVin|SubLoose|Streamer|Moder|Nill|) :
          user: TypesDefinition.StatusUser -> Choice<unit,unit,unit,unit,unit>
        
        val (|VipVin|VipLoose|Streamer|Moder|Nill|) :
          user: TypesDefinition.StatusUser -> Choice<unit,unit,unit,unit,unit>
        
        val (|UnsubVin|UnsubLoose|Streamer|Moder|Nill|) :
          user: TypesDefinition.StatusUser -> Choice<unit,unit,unit,unit,unit>
        
        val (|SubVipVin|SubVipLoose|Streamer|Moder|Nill|) :
          user: TypesDefinition.StatusUser -> Choice<unit,unit,unit,unit,unit>
        
        val (|TryKillSub|TryKillVIP|TryKillStreamer|TryKillModer|TryKillNill|TryKillUnsub|) :
          user: TypesDefinition.StatusUser
            -> Choice<unit,unit,unit,unit,unit,unit>
        
        val (|SMOrc|PogChamp|Greetings|NoReact|) :
          msgr: TypesDefinition.MessageRead -> Choice<string,string,string,unit>

namespace TWBot
    
    module DataBase =
        
        type SingleData =
            
            private new: unit -> SingleData
            
            member
              addChannelCommand: channel: TypesDefinition.Channels
                                 -> command: TypesDefinition.ChannelCommand
                                   -> Result<string,string>
            
            member
              private checkChannelCommand: channel: TypesDefinition.Channels
                                           -> command: TypesDefinition.ChannelCommand
                                             -> Result<bool,string>
            
            member createBotSettingsTable: unit -> Result<string,string>
            
            member
              createChannelCommandsTable: channel: TypesDefinition.Channels
                                            -> Result<string,string>
            
            member
              createChannelSettingsTable: channel: TypesDefinition.Channels
                                            -> Result<string,string>
            
            member
              deleteChannelCommand: channel: TypesDefinition.Channels
                                    -> command: TypesDefinition.ChannelCommand
                                      -> Result<string,string>
            
            member
              getBotSetting: setting: TypesDefinition.BotSettings
                               -> Result<TypesDefinition.BotSetting,string>
            
            member
              getChannelSetting: channel: TypesDefinition.Channels
                                 -> setting: TypesDefinition.ChannelSettings
                                   -> Result<TypesDefinition.ChannelSetting,
                                             string>
            
            member
              getCommand: channel: TypesDefinition.Channels -> command: string
                            -> Result<TypesDefinition.ChannelCommand,string>
            
            member
              getCommands: channel: TypesDefinition.Channels
                             -> Result<TypesDefinition.ChannelCommand list,
                                       string>
            
            member private initBotSettingsTable: unit -> Result<string,string>
            
            member
              private initChannelSettingsTable: channel: TypesDefinition.Channels
                                                  -> Result<string,string>
            
            member
              setBotSetting: setting: TypesDefinition.BotSetting
                               -> Result<string,string>
            
            member
              setChannelSetting: channel: TypesDefinition.Channels
                                 -> setting: TypesDefinition.ChannelSetting
                                   -> Result<string,string>
            
            member
              updateChannelCommand: channel: TypesDefinition.Channels
                                    -> command: TypesDefinition.ChannelCommand
                                      -> Result<string,string>
            
            static member DB: SingleData

namespace TWBot
    
    module APITwitch =
        
        val deserializeRespons:
          respons: Result<string,string> -> Result<'a,string>
        
        module Requests =
            
            [<Literal>]
            val private _APIurl: string = "https://api.twitch.tv/helix/"
            
            val private bearerToken: prefix: bool -> string
            
            val private _headers: unit -> (string * string) list
            
            val getUsers: userName: string -> Result<string,string>
            
            val searchChannels:
              channel: TypesDefinition.Channels -> Result<string,string>
            
            val getStream:
              channel: TypesDefinition.Channels -> Result<string,string>
            
            val getStreams:
              channels: TypesDefinition.Channels list -> Result<string,string>
            
            val getUserSubscribtion:
              channelID: string -> userID: string -> Result<string,string>
            
            val getChatters:
              channel: TypesDefinition.Channels -> Result<string,string>
            
            val checkOnline: channel: TypesDefinition.Channels -> bool
            
            val private getAccessToken: unit -> Result<string,string>
            
            val private revokeAccessToken: unit -> Result<string,string>
            
            val updateAccessToken: unit -> unit
        
        module IRC =
            
            [<Literal>]
            val private caps: string
              =
              "CAP REQ :twitch.tv/membership twitch.tv/tags twitch.tv/commands"
            
            val channels: TypesDefinition.Channels list
            
            val private readerWriter:
              bot: TypesDefinition.Bot -> TypesDefinition.ReaderWriter
            
            val sendMessage: msg: TypesDefinition.MessageWrite -> unit
            
            val sendRaw: rw: TypesDefinition.ReaderWriter -> raw: string -> unit
            
            val readChat: rw: TypesDefinition.ReaderWriter -> string
            
            val private initDBChannel: channel: TypesDefinition.Channels -> unit
            
            val private initDBBot: unit -> unit
            
            val private joinChannels:
              rw: TypesDefinition.ReaderWriter
              -> channelsList: TypesDefinition.Channels list -> unit
            
            val connection:
              bot: TypesDefinition.Bot -> TypesDefinition.ReaderWriter

namespace TWBot
    
    module Cache =
        
        val private memoize:
          f: ('a -> 'b)
          -> filter: (System.Collections.Generic.Dictionary<'a,'b> -> unit)
            -> ('a -> 'b) when 'a: equality
        
        val mutable tempReflyqMessageCounter: int
        
        val mutable tempKaeliaMessageCounter: int
        
        val mutable cacheLovers: TypesDefinition.CacheLove array
        
        val mutable cacheCutDown: TypesDefinition.CacheCatDown array
        
        val mutable cacheCutDownReward: TypesDefinition.CacheCatDownReward array
        
        val mutable cacheUsers: TypesDefinition.CacheUser array
        
        val mutable cacheChannelSettings:
          (TypesDefinition.Channels * TypesDefinition.CacheChannelSettings) array
        
        val mutable cacheChannelCommands:
          TypesDefinition.CacheChannelCommands array
        
        val updateCacheChannelSettings:
          channel: TypesDefinition.Channels -> unit
        
        val addCacheChannelCommands: channel: TypesDefinition.Channels -> unit
        
        val updateCacheChannelCommands:
          channel: TypesDefinition.Channels -> unit
        
        val checkCacheLove:
          loverName: string -> lovedName: string
            -> TypesDefinition.CacheLove option
        
        val addCacheLove:
          loverName: string -> lovedName: string -> percentLove: int -> unit
        
        val handleCacheLove: unit -> unit
        
        val checkToggleChannel: channel: TypesDefinition.Channels -> bool
        
        val checkEmotionToggleChannel: channel: TypesDefinition.Channels -> bool
        
        val checkPrefixChannel:
          channel: TypesDefinition.Channels -> Result<string,string>
        
        val checkLastReactTimeChannel: channel: TypesDefinition.Channels -> bool
        
        val updateLastReactTimeChannel:
          channel: TypesDefinition.Channels -> unit
        
        val handleCacheUsers: msgr: TypesDefinition.MessageRead -> unit
        
        val handleCacheCatDown: unit -> unit
        
        val handleCacheCatDownReward: rw: TypesDefinition.ReaderWriter -> unit
        
        val checkCacheCatDownKill:
          whoKill: string -> channel: TypesDefinition.Channels -> bool
        
        val checkCacheCatDownKilled:
          whoKilled: string -> channel: TypesDefinition.Channels -> bool
        
        val checkCacheCatDownRewardKilled:
          whoKilled: string -> channel: TypesDefinition.Channels -> bool
        
        val addCacheCatDown: cutDown: TypesDefinition.CacheCatDown -> unit
        
        val addCacheCatDownReward:
          cutDownReward: TypesDefinition.CacheCatDownReward -> unit
        
        val resolveCommandCache:
          msgr: TypesDefinition.MessageRead -> cmd: string
            -> TypesDefinition.ChannelCommand option
        
        val userFromCache:
          nickname: string option -> msgr: TypesDefinition.MessageRead
            -> TypesDefinition.CacheUser option
        
        val initCache:
          channelsList: TypesDefinition.Channels list -> listSize: int -> unit

namespace TWBot
    
    module BotTwitch =
        
        module private Utils =
            
            val minute: int
            
            val startTime: System.DateTime
            
            val Time: unit -> int64
            
            val rand: min: int -> max: int -> int
            
            val nameSecondWordLower:
              msgr: TypesDefinition.MessageRead -> string option
            
            val nameFirstWordLower:
              msgr: TypesDefinition.MessageRead -> string option
            
            val nameSecondWord:
              msgr: TypesDefinition.MessageRead -> string option
            
            val nameFirstWord:
              msgr: TypesDefinition.MessageRead -> string option
            
            val statusFromChatter:
              msgr: TypesDefinition.MessageRead -> nickname: Option<string>
                -> TypesDefinition.StatusUser option
            
            val resolveSecondStatusAndName:
              msgr: TypesDefinition.MessageRead
                -> TypesDefinition.StatusUser * string option
            
            val resolveFirstStatusAndName:
              msgr: TypesDefinition.MessageRead
                -> TypesDefinition.StatusUser * string option
        
        module private Commands =
            
            val ball: string[]
            
            val genericAnswers: string[]
            
            val buildAnswers: string[]
            
            module Reflyq =
                
                [<Literal>]
                val inMuteTime: string = "30"
                
                [<Literal>]
                val userMuteTime: string = "180"
                
                [<Literal>]
                val mutedTimeVIP: string = "200"
                
                [<Literal>]
                val mutedTimeOther: string = "300"
                
                module Answers =
                    
                    val catDownRewardFunction:
                      defendNickname: string -> howLongMuted: string
                      -> ctx: TypesDefinition.MessageContext -> unit
                    
                    val catDownFunction:
                      defendNickname: string -> killer: string
                      -> howLongMuted: string -> howLongCantUse: string
                      -> ctx: TypesDefinition.MessageContext -> unit
                    
                    val answerSubVin:
                      ctx: TypesDefinition.MessageContext
                      -> defendNickname: string
                        -> TypesDefinition.CutDownAnswer[]
                    
                    val answerVin:
                      ctx: TypesDefinition.MessageContext
                      -> defendNickname: string
                        -> TypesDefinition.CutDownAnswer[]
                    
                    val answerSubLoose:
                      ctx: TypesDefinition.MessageContext
                      -> defendNickname: string
                        -> TypesDefinition.CutDownAnswer[]
                    
                    val answerLoose:
                      ctx: TypesDefinition.MessageContext
                      -> defendNickname: string
                        -> TypesDefinition.CutDownAnswer[]
                    
                    val answerReward:
                      ctx: TypesDefinition.MessageContext
                      -> defendNickname: string -> mutedTime: string
                        -> TypesDefinition.CutDownAnswer[]
                
                val catDown: ctx: TypesDefinition.MessageContext -> string
                
                val rewardMute: ctx: TypesDefinition.MessageContext -> string
                
                val catDownUserAnswer:
                  ctx: TypesDefinition.MessageContext -> victory: bool
                  -> defenderNickName: string -> string
                
                val catDownUserStart:
                  ctx: TypesDefinition.MessageContext
                  -> attackUserStatus: TypesDefinition.StatusUser
                  -> defenderUserStatus: TypesDefinition.StatusUser
                  -> defenderNickName: string -> string
                
                val catDownUser: ctx: TypesDefinition.MessageContext -> string
            
            val love: ctx: TypesDefinition.MessageContext -> string
            
            val uptime: ctx: TypesDefinition.MessageContext -> string
            
            val roulette: ctx: TypesDefinition.MessageContext -> string
            
            val harakiri: ctx: TypesDefinition.MessageContext -> string
            
            val addCommandDataBase:
              ctx: TypesDefinition.MessageContext -> string
            
            val deleteCommandDataBase:
              ctx: TypesDefinition.MessageContext -> string
            
            val listCommandDataBase:
              ctx: TypesDefinition.MessageContext -> string
            
            val updateCommandDataBase:
              ctx: TypesDefinition.MessageContext -> string
            
            val updateChannelSettingDataBase:
              msgr: TypesDefinition.MessageRead
              -> rw: TypesDefinition.ReaderWriter
              -> setting: TypesDefinition.ChannelSettings -> string
            
            val commandList: unit -> TypesDefinition.CommandList list
            
            val rewardList: unit -> TypesDefinition.RewardList list
        
        module private Parse =
            
            val resolveCommandList:
              cmd: string -> ctx: TypesDefinition.MessageContext
                -> Result<(TypesDefinition.MessageContext -> string),string>
            
            val resolveCommand:
              ctx: TypesDefinition.MessageContext -> resu: Result<string,string>
                -> Result<(TypesDefinition.MessageContext -> string),string>
            
            val subscriber: firstLineSplit: string array -> bool
            
            val vip: firstLineSplit: string array -> bool
            
            val turbo: firstLineSplit: string array -> bool
            
            val moderator: firstLineSplit: string array -> bool
            
            val displayName: firstLineSplit: string array -> string
            
            val userID: firstLineSplit: string array -> string
            
            val roomID: firstLineSplit: string array -> string
            
            val nickname: firstLineSplit: string array -> string
            
            val rewardID: firstLineSplit: string array -> string option
            
            val message: secondLineSplit: string array -> string
            
            val channel: secondLineSplit: string array -> string
            
            val command:
              ctx: TypesDefinition.MessageContext
                -> (TypesDefinition.MessageContext -> string) option
        
        module Handlers =
            
            val handleRewards: ctx: TypesDefinition.MessageContext -> unit
            
            val handleLine:
              line: string -> Result<TypesDefinition.MessageRead,string>
            
            val handleCommands: ctx: TypesDefinition.MessageContext -> unit
            
            val handleReacts: ctx: TypesDefinition.MessageContext -> unit
            
            val handleMasterCommands:
              ctx: TypesDefinition.MessageContext -> unit
            
            val handleHelper: ctx: TypesDefinition.MessageContext -> unit
            
            val handleCache: ctx: TypesDefinition.MessageContext -> unit

namespace TWBot
    
    module BotTG =
        
        val private con: Funogram.Types.BotConfig
        
        val onHelp: ctx: Funogram.Telegram.Bot.UpdateContext -> unit
        
        val onStart: ctx: Funogram.Telegram.Bot.UpdateContext -> unit
        
        val onUpdate: ctx: Funogram.Telegram.Bot.UpdateContext -> unit
        
        val startTG: unit -> unit

namespace TWBot
    
    module Main =
        
        val main: argv: string[] -> int

