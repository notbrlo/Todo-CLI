module Todo_CLI.TodoModels
open System

type TodoEntry = {EntryId: Guid; Completed: bool; Body: string}
type TodoUser = {UserId: Guid; Email: string; Entries: TodoEntry list}