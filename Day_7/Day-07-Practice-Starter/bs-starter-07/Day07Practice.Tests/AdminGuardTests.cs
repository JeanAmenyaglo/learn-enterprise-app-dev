using Day07Practice.Features.User.Store;
using Day07Practice.Guards;

namespace Day07Practice.Tests;

public class AdminGuardTests
{
    [Fact]
    public void ShouldRedirect_WhenRoleIsNotAdmin_ReturnsTrue()
    {
        var state = new UserState { Role = "User" };
        var result = AdminGuard.ShouldRedirect(state);
        Assert.True(result);
    }

    [Fact]
    public void ShouldRedirect_WhenRoleIsAdmin_ReturnsFalse()
    {
        var state = new UserState { Role = "Admin" };
        var result = AdminGuard.ShouldRedirect(state);
        Assert.False(result);
    }
}