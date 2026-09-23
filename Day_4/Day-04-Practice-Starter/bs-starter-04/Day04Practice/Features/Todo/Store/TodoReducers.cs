using Day04Practice.Features.Todo.Models;
using Fluxor;

namespace Day04Practice.Features.Todo.Store;

public static class TodoReducers
{
    [ReducerMethod]
    public static TodoState ReduceAddTodoAction(
    TodoState state,
    AddTodoAction action)
    {
        var newId = state.Items.Count > 0 ? state.Items.Max(t => t.Id) + 1 : 1;
        var newItem = new TodoItem(newId, action.Title, false);
        return state with { Items = [.. state.Items, newItem] };
    }

    [ReducerMethod]
    public static TodoState ReduceRemoveTodoAction(
        TodoState state,
        RemoveTodoAction action)
    {
        var updatedItems = state.Items.Where(t => t.Id != action.Id).ToList();
        return state with { Items = updatedItems };
    }

    // PRE-BUILT -- your worked example. Collection state is still IMMUTABLE: a reducer never
    // edits the existing list, it builds a NEW one. .Select(... ? item with {…} : item).ToList()
    // is a copy that replaces only the changed item. Same `with` rule as Counter, now over a
    // list of records. The two reducers above use the same idea with different LINQ shapes.
    [ReducerMethod]
    public static TodoState ReduceToggleTodoAction(
        TodoState state,
        ToggleTodoAction action)
    {
        var updatedItems = state.Items
            .Select(t => t.Id == action.Id
                ? t with { IsComplete = !t.IsComplete }
                : t)
            .ToList();
        return state with { Items = updatedItems };
    }
}
