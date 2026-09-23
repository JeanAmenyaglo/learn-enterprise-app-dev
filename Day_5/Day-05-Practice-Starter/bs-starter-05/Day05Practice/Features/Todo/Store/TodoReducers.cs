using Day05Practice.Services;
using Fluxor;

namespace Day05Practice.Features.Todo.Store;

public static class TodoReducers
{
    // --- Day 4 reducers (working) ---

    [ReducerMethod]
    public static TodoState ReduceAddTodoAction(TodoState state, AddTodoAction action)
        => state with
        {
            Items = [.. state.Items, new TodoItem(state.NextId, action.Title, false)],
            NextId = state.NextId + 1
        };

    [ReducerMethod]
    public static TodoState ReduceRemoveTodoAction(TodoState state, RemoveTodoAction action)
        => state with { Items = state.Items.Where(t => t.Id != action.Id).ToList() };

    [ReducerMethod]
    public static TodoState ReduceToggleTodoAction(TodoState state, ToggleTodoAction action)
        => state with
        {
            Items = state.Items.Select(t =>
                t.Id == action.Id ? t with { IsComplete = !t.IsComplete } : t
            ).ToList()
        };

    [ReducerMethod]
public static TodoState ReduceLoadTodosAction(
    TodoState state,
    LoadTodosAction action)
{
    return state with { IsLoading = true, ErrorMessage = null };
}

[ReducerMethod]
public static TodoState ReduceLoadTodosSuccessAction(
    TodoState state,
    LoadTodosSuccessAction action)
{
    return state with { Items = action.Items, IsLoading = false };
}

[ReducerMethod]
public static TodoState ReduceLoadTodosFailureAction(
    TodoState state,
    LoadTodosFailureAction action)
{
    return state with { ErrorMessage = action.Error, IsLoading = false };
}
}
