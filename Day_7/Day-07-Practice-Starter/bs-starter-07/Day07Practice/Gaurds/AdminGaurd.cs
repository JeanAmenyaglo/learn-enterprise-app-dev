using Day07Practice.Features.User.Store;

namespace Day07Practice.Guards;

public static class AdminGuard
{
    public static bool ShouldRedirect(UserState state) =>
        state.Role != "Admin";
}