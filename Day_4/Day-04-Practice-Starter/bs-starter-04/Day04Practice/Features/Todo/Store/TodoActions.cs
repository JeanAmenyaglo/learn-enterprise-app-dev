namespace Day04Practice.Features.Todo.Store;

public record AddTodoAction(string Title);
public record RemoveTodoAction(int Id);

// PRE-BUILT -- the third action ships written, and so does its reducer. Toggle is the
// worked example for immutable list replacement: read it, then apply the same shape to
// Remove. See TodoReducers.ReduceToggleTodoAction.
public record ToggleTodoAction(int Id);