using Microsoft.AspNetCore.Http;
using Modules.Administration.Shared.Interfaces;

namespace Modules.Administration.Application.Services;

internal class AdminModuleApi(AdminService service) : IAdminModuleApi
{
    public async Task<IResult> AmIAdmin(string? username)
        => await service.IsUserAdmin(username);
}