using Microsoft.AspNetCore.Http;

namespace Modules.Administration.Shared.Interfaces;

public interface IAdminModuleApi
{
    Task<IResult> AmIAdmin(string? username);
}