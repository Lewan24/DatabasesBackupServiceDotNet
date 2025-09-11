using Modules.Backup.Shared.Dtos;
using OneOf;
using OneOf.Types;

namespace Client.UI.Data.Services;

public class BackupsHttpClientService(TokenHttpClientService api)
{
    public async Task<OneOf<List<ServerConnectionDto>, string>> GetServersAsync()
        => await api.GetAsync<List<ServerConnectionDto>>("/api/servers/GetMyServers");
    
    public async Task<OneOf<List<ServerNameIdDto>, string>> GetServersNamesForSchedulesAsync()
        => await api.GetAsync<List<ServerNameIdDto>>("/api/servers/GetMyServersForSchedule");
    
    public async Task<OneOf<Success, string>> CreateServerAsync(ServerConnectionDto newServer)
        => await api.PostAsync("/api/servers/CreateServer", newServer);
    
    public async Task<OneOf<Success, string>> EditServerAsync(ServerConnectionDto server)
        => await api.PostAsync("/api/servers/EditServer", server);
    
    public async Task<OneOf<Success, string>> ToggleServerDisabledStatus(Guid serverId)
        => await api.PostAsync("/api/servers/ToggleServerDisabledStatus", serverId);
    
    public async Task<OneOf<Success, string>> CascadeDeleteServer(Guid serverId)
        => await api.PostAsync("/api/servers/CascadeDeleteServer", serverId);

    public async Task<OneOf<Success, string>> TestServer(Guid serverId)
        => await api.PostAsync("/api/servers/TestServerConnection", serverId);
    
    public async Task<OneOf<List<BackupsScheduleDto>, string>> GetSchedulesAsync()
        => await api.GetAsync<List<BackupsScheduleDto>>("/api/schedules/GetMySchedules");

    public async Task<OneOf<Success, string>> CreateScheduleAsync(BackupsScheduleDto schedule)
        => await api.PostAsync("/api/schedules/CreateSchedule", schedule);
    
    public async Task<OneOf<Success, string>> EditScheduleAsync(BackupsScheduleDto schedule)
        => await api.PostAsync("/api/schedules/EditSchedule", schedule);
    
    public async Task<OneOf<Success, string>> DeleteScheduleAsync(Guid scheduleId)
        => await api.PostAsync("/api/schedules/DeleteSchedule", scheduleId);

    public async Task<OneOf<Success, string>> BackupServerAsync(Guid serverId)
        => await api.PostAsync("/api/backups/PerformBackup", serverId);
}