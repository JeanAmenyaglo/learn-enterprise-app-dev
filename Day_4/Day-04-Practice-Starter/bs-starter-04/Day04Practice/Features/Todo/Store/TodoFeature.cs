using Fluxor;

namespace Day04Practice.Features.Todo.Store;

public class TodoFeature : Feature<TodoState>
{
    public override string GetName() => "Todo";
    protected override TodoState GetInitialState() => new TodoState();
}