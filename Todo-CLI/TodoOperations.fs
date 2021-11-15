module Todo_CLI.TodoOperations
open System
open System.IO
open Newtonsoft.Json
open TodoModels

let private NewUser email = {UserId = Guid.NewGuid(); Email = email; Entries = []}
let private NewEntry body = {EntryId = Guid.NewGuid(); Completed = false; Body = body}

// public-facing operations
let GetUser email users = users |> Seq.tryFind (fun x -> x.Email = email)

let AddUser email users =
    match (users |> Seq.tryFind (fun x -> x.Email = email)) with
    | None -> (NewUser email)::users
    | Some _ -> users
    
let AddEntry user body =
    let newEntry = NewEntry body
    {user with Entries = newEntry::user.Entries}
    
let DeleteUser userId users = users |> List.filter (fun x -> x.UserId <> userId)

let DeleteEntry entryId user =
    {user with Entries = user.Entries |> List.filter (fun x -> x.EntryId <> entryId)}
    
let EditEntry user entryId completed body =
    let elem = user.Entries |> Seq.tryFind (fun x -> x.EntryId = entryId)
    match elem with
    | None -> user
    | Some(x) ->
        let modEntry = {x with Completed = completed; Body = body}
        let modEntries = modEntry::(user.Entries |> List.filter (fun x -> x.EntryId <> modEntry.EntryId))
        {user with Entries = modEntries}
        
let LoadUsers userPath =
    userPath 
        |> Directory.GetFiles 
        |> Array.map (File.ReadAllText)
        |> Array.map (JsonConvert.DeserializeObject<TodoUser>) 
        |> List.ofArray
    
let SaveUser (userPath: string) user =
    File.WriteAllText(Path.Combine(userPath, user.UserId.ToString()), user |> JsonConvert.SerializeObject)
    
let SaveUsers userPath users = users |> List.iter (SaveUser userPath)    
    