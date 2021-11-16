module Todo_CLI.Program
open System
open System.IO
open TodoModels
open TodoOperations

let UserPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".users")

let CreateIndexedEntries user =
    let entryArray = user.Entries |> Array.ofList
    entryArray
        |> Array.map (fun x -> (Array.findIndex (fun y -> y.EntryId = x.EntryId) entryArray, x))
        |> List.ofArray
        
let FormatIndexedEntries indexedEntries =
    let header = "Index\tCompleted\tEntry\n===============================================\n"
    indexedEntries
               |> List.map (fun (index, entry) -> $"{index}\t{entry.Completed}\t\t{entry.Body}\n")
               |> List.fold (fun x y -> x + y) header
    
let rec MarkEntry user formattedEntries =
    printf "Enter entry index (leave blank to go back to main menu): "
    let markEntryResult = 
        try
            let input = Console.ReadLine()
            if input = "" 
            then true
            else
                let input2 = input |> int
                let entry = formattedEntries |> Seq.tryFind (fun x -> input2 = fst x)
                match entry with
                | None ->
                    printfn "Invalid entry index. Try again.\n"
                    MarkEntry user formattedEntries
                | Some x ->
                    let fullEntry = snd x
                    let editedEntry = EditEntry user fullEntry.EntryId (not fullEntry.Completed) fullEntry.Body
                    editedEntry |> SaveUser UserPath
                    true
        with
            | _ ->
                printfn "Not a number. Try again.\n"
                MarkEntry user formattedEntries
    markEntryResult
    
let rec DeleteEntryLoop user formattedEntries =
    printf "Enter entry ID (leave blank to go back to main menu): "
    let deleteEntryResult = 
        try
            let input = Console.ReadLine() 
            if input = "" 
            then true 
            else
                let input2 = input |> int
                let entry = formattedEntries |> Seq.tryFind (fun x -> input2 = fst x)
                match entry with
                | None ->
                    printfn "Invalid entry index. Try again.\n"
                    DeleteEntryLoop user formattedEntries
                | Some x ->
                    let fullEntry = snd x
                    let editedEntry = DeleteEntry fullEntry.EntryId user
                    editedEntry |> SaveUser UserPath
                    true
        with
            | _ ->
                printfn "Not a number. Try again.\n"
                DeleteEntryLoop user formattedEntries
    deleteEntryResult

let rec EditEntryLoop user formattedEntries =
    printf "Enter entry ID (leave blank to go back to main menu): "
    let editEntryResult = 
        try
            let input = Console.ReadLine()
            if input = "" 
            then true
            else
                let input2 = input |> int
                let entry = formattedEntries |> Seq.tryFind (fun x -> input2 = fst x)
                match entry with
                | None ->
                    printfn "Invalid entry index. Try again.\n"
                    EditEntryLoop user formattedEntries
                | Some x ->
                    let fullEntry = snd x
                    printf "Add the text to change (leave blank to keep original text): "
                    let newBody = Console.ReadLine()
                    let bodyResult = if newBody = "" then fullEntry.Body else newBody
                    let editedEntry = EditEntry user fullEntry.EntryId fullEntry.Completed bodyResult
                    editedEntry |> SaveUser UserPath
                    true
        with
            | _ ->
                printfn "Not a number. Try again.\n"
                EditEntryLoop user formattedEntries
    editEntryResult
    
let rec CreateNewEntry user =
    printf "Enter the entry's text: "
    let body = Console.ReadLine()
    if body = "" 
    then 
        printf "Entry cannot be blank. Return to main menu? [Y/N]: "
        let choice = Console.ReadLine().ToUpper()
        match choice with
        | "Y" -> true
        | _ -> 
            printfn "Try again."
            CreateNewEntry user
    else
        let updatedUser = AddEntry user body
        updatedUser |> SaveUser UserPath
        true

let rec MainMenuLoop user users =
    let formattedEntries = CreateIndexedEntries user
    printfn $"{FormatIndexedEntries formattedEntries}"
    printfn "[N]ew Entry [E]dit Entry [D]elete Entry [M]ark Entry [Q]uit"
    printf "Make your selection: "
    let userInput = Console.ReadLine().ToUpper()
    let inputResult =
        match userInput with
        | "N" -> CreateNewEntry user
        | "E" -> EditEntryLoop user formattedEntries
        | "M" -> MarkEntry user formattedEntries
        | "D" -> DeleteEntryLoop user formattedEntries
        | "Q" -> false
        | _ ->
            printfn "Invalid input. Try again."
            MainMenuLoop user users
    inputResult

let MainLoop () =
    // try to login
    printf "Enter user email: " 
    let email = Console.ReadLine()
    let users = LoadUsers UserPath
    let mutable loginUser = GetUser email users
    while loginUser = None do
        printf "\nUser does not exist. Create a new user? [Y/N]: "
        let newUserAttempt = Console.ReadLine().ToUpper()
        match newUserAttempt with
        | "Y" ->
            let newUsers = users |> AddUser email
            newUsers |> SaveUsers UserPath
            loginUser <- GetUser email newUsers
        | "N" ->
            printfn "Try again."
            loginUser <- None
        | _ ->
            printfn "Invalid input. Must answer [Y]es or [N]o."
            loginUser <- None
    // user is logged in. Proceed.
    let mutable mainMenuResult = MainMenuLoop loginUser.Value users
    while mainMenuResult = true do
        // reload users every time we iterate
        let currentUsers = LoadUsers UserPath
        let currentUser = GetUser email currentUsers
        mainMenuResult <- MainMenuLoop currentUser.Value currentUsers
    false

[<EntryPoint>]
let main _ =
    // create directory if if doesn't already exist
    UserPath |> Directory.CreateDirectory |> ignore
    printfn "Welcome to Todo! Exit at any time with CTRL-C"
    let mutable loopResult = MainLoop ()
    while loopResult do
        loopResult <- MainLoop()
    0