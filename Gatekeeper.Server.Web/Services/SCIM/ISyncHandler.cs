using System;
using System.Threading.Tasks;

namespace AuthServer.Server.Services.SCIM {
    public interface ISyncHandler 
    {
        Task SyncUserAsync(Guid userId, Guid scimAppSettingsId);

        Task UnsyncUserAsync(Guid userId, Guid scimAppSettingsId);
        Task FullSyncAsync(Guid scimAppSettingsId);
        Task SyncGroupAsync(Guid groupId, Guid scimAppSettingsId);
        Task UnsyncGroupAsync(Guid groupId, Guid scimAppSettingsId);
    }
}
