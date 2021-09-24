module TWBot.BotTG

open TWBot

open System
open TypesDefinition
open ActivePatterns
open DataBase
open Logger

open Funogram.Api
open Funogram.Types
open Funogram.Telegram.Api
open Funogram.Telegram.Types
open Funogram.Telegram.Bot


let private con =
    { defaultConfig with
          Token = TokensData.TGToken }

let onHelp (ctx: UpdateContext) =
    if ctx.Update.Message.IsSome then
        let chatID = ctx.Update.Message.Value.Chat.Id
        "Бот для оповещения о начале стримов и всякое такое."
        |>sendMessage chatID
        |>api con
        |>Async.RunSynchronously
        |>ignore
    ()
    
let onStart (ctx: UpdateContext) =
    if ctx.Update.Message.IsSome then
        let chatID = ctx.Update.Message.Value.Chat.Id
        "Ага я тут."
        |>sendMessage chatID
        |>api con
        |>Async.RunSynchronously
        |>ignore
    ()
    
let onUpdate ctx =
    processCommands ctx [
        cmd "/help" onHelp
        cmd "/start" onStart
    ] |> ignore
    ()

let startTG () =
    startBot con onUpdate None
    |> Async.RunSynchronously