using Modules.Auth.Shared.ActionsRequests;
using Modules.Auth.Shared.Entities;
using OneOf;
using OneOf.Types;

namespace Client.UI.Data.Interfaces;

public interface IAuthService : ICurrentUserInfo, IUserToken, ILoggingIn, IRegistration
{
}

public interface ICurrentUserInfo
{
    Task<CurrentUser?> GetCurrentUserInfo();
}

public interface IUserToken
{
    TokenModelDto? UserToken { get; set; }

    /// <summary>
    ///     POST request with passed login inputs. Deserialize and save httpResponse to UserToken object in service
    /// </summary>
    /// <param name="request"><see cref="LoginRequest" />Object that stores userName input and password</param>
    /// <exception cref="HttpRequestException">Thrown while doing POST action with passed 'request'</exception>
    /// <exception cref="ArgumentNullException">Thrown while deserializing object from httpResponse</exception>
    /// <exception cref="NotSupportedException">Thrown while deserializing object from httpResponse</exception>
    Task RefreshToken(LoginRequest request);

    Task<OneOf<True, False>> IsValid();
}

public interface ILoggingIn
{
    Task<(bool Success, string? Msg)> Login(LoginRequest? loginRequest);

    /// <summary>
    ///     POST request with passed login inputs. Deserialize and save httpResponse to bool result.
    /// </summary>
    /// <param name="request"><see cref="LoginRequest" />Object that stores userName input and password</param>
    /// <returns><see cref="bool" /> as result of login validation</returns>
    /// <exception cref="HttpRequestException">Thrown while doing POST action with passed 'request'</exception>
    /// <exception cref="ArgumentNullException">Thrown while deserializing object from httpResponse</exception>
    /// <exception cref="NotSupportedException">Thrown while deserializing object from httpResponse</exception>
    Task<bool> TryLogin(LoginRequest? request);

    Task Logout();
}

public interface IRegistration
{
    /// <summary>
    ///     Register new user.
    /// </summary>
    /// <param name="registerRequest">
    ///     <see cref="RegisterRequest" /> object that has user inputs like username/email and
    ///     password
    /// </param>
    /// <returns>Tuple with validation result and returned username from api response</returns>
    Task<((bool Success, string? Msg) Validation, string? UserName)> Register(RegisterRequest? registerRequest);
}