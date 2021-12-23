namespace Todo_CLI
module TodoModels = 
    open System

    type TodoEntry = {EntryId: Guid; Completed: bool; Body: string; LastUpdated: DateTime}
    type TodoUser = {UserId: Guid; Email: string; Entries: TodoEntry list}