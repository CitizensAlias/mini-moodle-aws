namespace minimoodle.Services;

public interface IAuthService
{
    Task<Result> Login(string username, string password, CancellationToken cancellationToken = default);
    Task<Result> Logout(CancellationToken cancellationToken = default);
    Task<Result<UserDto>> GetCurrentUser(CancellationToken cancellationToken = default);
}
