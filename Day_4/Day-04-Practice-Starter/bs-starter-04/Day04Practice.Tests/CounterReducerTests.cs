using Day04Practice.Features.Counter.Store;

namespace Day04Practice.Tests;

public class CounterReducerTests
{
    [Fact]
    public void ReduceIncrementCounterAction_WithCountOf5_ReturnsCountOf6()
    {
        // Arrange
        var state = new CounterState { Count = 5 };
        var action = new IncrementCounterAction();

        // Act
        var result = CounterReducers.ReduceIncrementCounterAction(state, action);

        // Assert
        Assert.Equal(6, result.Count);
    }
[Fact]
public void ReduceDecrementCounterAction_WithCountOf3_ReturnsCountOf2()
{
    // Arrange
    var initialState = new CounterState { Count = 3 };
    var action = new DecrementCounterAction();

    // Act
    var newState = CounterReducers.ReduceDecrementCounterAction(
        initialState, action);

    // Assert
    Assert.Equal(2, newState.Count);
}

[Fact]
public void ReduceResetCounterAction_WithCountOf10_ReturnsCountOf0()
{
    // Arrange
    var initialState = new CounterState { Count = 10 };
    var action = new ResetCounterAction();

    // Act
    var newState = CounterReducers.ReduceResetCounterAction(
        initialState, action);

    // Assert
    Assert.Equal(0, newState.Count);
}
}