using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Modules.Auth.Core.Entities;
using Modules.Auth.Infrastructure.DbContexts;
using Modules.Backup.Infrastructure.DbContexts;
using Modules.Backup.Shared.Dtos;
using OneOf;
using OneOf.Types;

namespace Modules.Backup.Application.Services;

public sealed class SchedulesService(
    BackupsDbContext db,
    AppIdentityDbContext appIdentityDbContext,
    UserManager<AppUser> userManager,
    ILogger<SchedulesService> logger)
{
    //TODO: Implement these methods
    public async Task<OneOf<List<BackupsScheduleDto>, string>> GetMySchedules(string? identityName)
    {
        throw new NotImplementedException();
    }

    public async Task<OneOf<Success, string>> CreateSchedule(BackupsScheduleDto schedule, string? identityName)
    {
        throw new NotImplementedException();
    }

    public async Task<OneOf<Success, string>> EditSchedule(BackupsScheduleDto schedule, string? identityName)
    {
        throw new NotImplementedException();
    }

    public async Task<OneOf<Success, string>> DeleteSchedule(Guid scheduleId, string? identityName)
    {
        throw new NotImplementedException();
    }
}