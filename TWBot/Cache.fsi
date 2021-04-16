module TWBot.Cache

open TWBot

open TypesDefinition

val mutable tempReflyqMessageCounter : int
val mutable tempKaeliaMessageCounter : int
val mutable cacheLovers : CacheLove array
val mutable cacheCutDown : CacheCatDown array
val mutable cacheCutDownReward : CacheCatDownReward array
val mutable cacheUsers : CacheUser array
val mutable cacheChannelSettings : (Channels * CacheChannelSettings) array
val mutable cacheChannelCommands : CacheChannelCommands array

val updateCacheChannelSettings : Channels -> unit
val addCacheChannelCommands : Channels -> unit
val updateCacheChannelCommands : Channels -> unit
val checkCacheLove : string -> string -> CacheLove option
val addCacheLove : string -> string -> int -> unit
val handleCacheLove : unit -> unit
val checkToggleChannel : Channels -> bool
val checkEmotionToggleChannel : Channels -> bool
val checkPrefixChannel : Channels -> Result<string, string>
val checkLastReactTimeChannel : Channels -> bool
val updateLastReactTimeChannel : Channels -> unit
val handleCacheUsers : MessageRead -> unit
val handleCacheCatDown : unit -> unit
val handleCacheCatDownReward : ReaderWriter -> unit
val checkCacheCatDownKill : string -> Channels -> bool
val checkCacheCatDownKilled : string -> Channels -> bool
val checkCacheCatDownRewardKilled : string -> Channels -> bool
val addCacheCatDown : CacheCatDown -> unit
val addCacheCatDownReward : CacheCatDownReward -> unit
val resolveCommandCache : MessageRead -> string -> ChannelCommand option
val userFromCache : string option -> MessageRead -> CacheUser option
val initCache : Channels list -> int -> unit
