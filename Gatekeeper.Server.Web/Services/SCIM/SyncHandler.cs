using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using Gatekeeper.SCIM.Client.Action;
using Gatekeeper.SCIM.Client.Result;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Server.Services.SCIM
{
    public class SyncHandler : ISyncHandler
    {
        private readonly AuthDbContext _authDbContext;

        public SyncHandler(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        private async Task<Gatekeeper.SCIM.Client.Client> GetScimClient(Guid scimAppSettingsId)
        {
            SCIMAppSettings scimAppSettings = await _authDbContext
                .SCIMAppSettings
                .AsNoTracking()
                .SingleAsync(s => s.Id == scimAppSettingsId);

            Gatekeeper.SCIM.Client.Client client = new Gatekeeper.SCIM.Client.Client(
                new Uri(scimAppSettings.Endpoint)
            );
            client.SetAuthToken(scimAppSettings.Credentials);

            return client;
        }

        public async Task SyncUserAsync(Guid userId, Guid scimAppSettingsId)
        {
            ScimUserSyncState? syncState = await _authDbContext
                .ScimUserSyncStates
                .SingleOrDefaultAsync(s => s.SCIMAppSettings.Id == scimAppSettingsId && s.User.Id == userId);

            AppUser user = await _authDbContext
                .Users
                .SingleAsync(u => u.Id == userId);

            Gatekeeper.SCIM.Client.Schema.Core20.User scimUser = new Gatekeeper.SCIM.Client.Schema.Core20.User
            {
                ExternalId = user.Id.ToString(),
                UserName = user.UserName,
                Emails = new List<Gatekeeper.SCIM.Client.Schema.Core20.User.EmailAttribute>() {
                    new  Gatekeeper.SCIM.Client.Schema.Core20.User.EmailAttribute
                    {
                        Value = user.Email,
                        Primary = true
                    },
                },
                DisplayName = user.UserName,
                Active = true,
            };

            Gatekeeper.SCIM.Client.Client scimClient = await GetScimClient(scimAppSettingsId);
            if (syncState == null)
            {
                CreateAction<Gatekeeper.SCIM.Client.Schema.Core20.User> createUserAction = new CreateAction<Gatekeeper.SCIM.Client.Schema.Core20.User>(scimUser);
                CreateResult<Gatekeeper.SCIM.Client.Schema.Core20.User> createUserResult = await scimClient.PerformAction<CreateResult<Gatekeeper.SCIM.Client.Schema.Core20.User>>(createUserAction);

                if (createUserResult.ResultStatus == StateEnum.Success &&
                    createUserResult.Resource != null &&
                    createUserResult.Resource.Id != null
                )
                {
                    syncState = new ScimUserSyncState
                    {
                        User = user,
                        SCIMAppSettingsId = scimAppSettingsId,
                        ServiceId = createUserResult.Resource.Id,
                    };
                    _authDbContext.Add(syncState);
                    await _authDbContext.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("SCIM initial sync failed");
                }
            }
            else
            {
                scimUser.Id = syncState.ServiceId;
                UpdateUserAction updateUserAction = new UpdateUserAction(scimUser);
                UpdateUserResult updateUserResult = await scimClient.PerformAction<UpdateUserResult>(updateUserAction);

                if (updateUserResult.ResultStatus != StateEnum.Success)
                {
                    throw new Exception("SCIM update failed");
                }
            }
        }

        public async Task UnsyncUserAsync(Guid userId, Guid scimAppSettingsId)
        {
            ScimUserSyncState syncState = await _authDbContext
               .ScimUserSyncStates
               .SingleAsync(s => s.SCIMAppSettings.Id == scimAppSettingsId && s.User.Id == userId);

            Gatekeeper.SCIM.Client.Client scimClient = await GetScimClient(scimAppSettingsId);

            DeleteUserAction deleteUser = new DeleteUserAction(syncState.ServiceId);
            DeleteUserResult deleteUserResult = await scimClient.PerformAction<DeleteUserResult>(deleteUser);
            _authDbContext.Remove(syncState);
            await _authDbContext.SaveChangesAsync();
        }

        public async Task SyncGroupAsync(Guid groupId, Guid scimAppSettingsId)
        {
            ScimGroupSyncState? syncState = await _authDbContext
                .ScimGroupSyncStates
                .SingleOrDefaultAsync(s => s.SCIMAppSettings.Id == scimAppSettingsId && s.UserGroup.Id == groupId);

            List<ScimUserSyncState> userSyncStates = await _authDbContext
                .ScimUserSyncStates
                .Where(s => s.SCIMAppSettings.Id == scimAppSettingsId && s.User.Groups.Any(g => g.Id == groupId))
                .ToListAsync();
            List<Gatekeeper.SCIM.Client.Schema.Core20.Group.GroupMembership> groupMemberships = new List<Gatekeeper.SCIM.Client.Schema.Core20.Group.GroupMembership>();
            foreach (ScimUserSyncState userSyncState in userSyncStates)
            {
                groupMemberships.Add(new Gatekeeper.SCIM.Client.Schema.Core20.Group.GroupMembership
                {
                    Value = userSyncState.ServiceId,
                });
            }

            UserGroup group = await _authDbContext
                .UserGroup
                .SingleAsync(u => u.Id == groupId);

            Gatekeeper.SCIM.Client.Schema.Core20.Group scimGroup = new Gatekeeper.SCIM.Client.Schema.Core20.Group
            {
                ExternalId = group.Id.ToString(),
                DisplayName = group.Name,
                Members = groupMemberships,
            };

            Gatekeeper.SCIM.Client.Client scimClient = await GetScimClient(scimAppSettingsId);
            if (syncState == null)
            {
                CreateAction<Gatekeeper.SCIM.Client.Schema.Core20.Group> createGroupAction = new CreateAction<Gatekeeper.SCIM.Client.Schema.Core20.Group>(scimGroup);
                CreateResult<Gatekeeper.SCIM.Client.Schema.Core20.Group> createUserResult = await scimClient.PerformAction<CreateResult<Gatekeeper.SCIM.Client.Schema.Core20.Group>>(createGroupAction);

                if (createUserResult.ResultStatus == StateEnum.Success &&
                    createUserResult.Resource != null &&
                    createUserResult.Resource.Id != null
                )
                {
                    syncState = new ScimGroupSyncState
                    {
                        UserGroup = group,
                        SCIMAppSettingsId = scimAppSettingsId,
                        ServiceId = createUserResult.Resource.Id,
                    };
                    _authDbContext.Add(syncState);
                    await _authDbContext.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("SCIM initial sync failed");
                }
            }
            else
            {
                scimGroup.Id = syncState.ServiceId;
                UpdateGroupAction updateGroup = new UpdateGroupAction(scimGroup);
                UpdateGroupResult updateGroupResult = await scimClient.PerformAction<UpdateGroupResult>(updateGroup);

                if (updateGroupResult.ResultStatus != StateEnum.Success)
                {
                    throw new Exception("SCIM update failed");
                }
            }
        }

        public async Task UnsyncGroupAsync(Guid groupId, Guid scimAppSettingsId)
        {
            ScimGroupSyncState syncState = await _authDbContext
               .ScimGroupSyncStates
               .SingleAsync(s => s.SCIMAppSettings.Id == scimAppSettingsId && s.UserGroup.Id == groupId);

            Gatekeeper.SCIM.Client.Client scimClient = await GetScimClient(scimAppSettingsId);

            DeleteGroupAction deleteGroup = new DeleteGroupAction(syncState.ServiceId);
            DeleteGroupResult deleteGroupResult = await scimClient.PerformAction<DeleteGroupResult>(deleteGroup);

            _authDbContext.Remove(syncState);
            await _authDbContext.SaveChangesAsync();
        }

        public async Task FullSyncAsync(Guid scimAppSettingsId)
        {
            SCIMAppSettings scimAppSettings = await _authDbContext
                .SCIMAppSettings
                .Include(u => u.AuthApp)
                    .ThenInclude(a => a.UserGroups)
                        .ThenInclude(g => g.Members)
                .SingleAsync(s => s.Id == scimAppSettingsId);

            Gatekeeper.SCIM.Client.Client client = new Gatekeeper.SCIM.Client.Client(new Uri(scimAppSettings.Endpoint));
            client.SetAuthToken(scimAppSettings.Credentials);

            // Create a dictionary of all permitted users
            HashSet<Guid> allAppUsers = new HashSet<Guid>();
            foreach (UserGroup group in scimAppSettings.AuthApp.UserGroups)
            {
                foreach (AppUser member in group.Members)
                {
                    allAppUsers.Add(member.Id);
                }
            }

            // Read SCIM states
            List<ScimUserSyncState> userSyncStates = await _authDbContext
                .ScimUserSyncStates
                .Include(s => s.User)
                .Where(s => s.SCIMAppSettings == scimAppSettings)
                .ToListAsync();

            // Update users
            foreach (Guid userId in allAppUsers)
            {
                BackgroundJob.Enqueue((ISyncHandler syncHandler) => syncHandler.SyncUserAsync(userId, scimAppSettingsId));
            }

            // Remove users
            List<ScimUserSyncState> usersToRemove = userSyncStates
                .Where(s => !allAppUsers.Contains(s.User.Id))
                .ToList();
            foreach (ScimUserSyncState syncState in usersToRemove)
            {
                BackgroundJob.Enqueue((ISyncHandler syncHandler) => syncHandler.UnsyncUserAsync(syncState.User.Id, scimAppSettingsId));
            }

            // Read scim states
            List<ScimGroupSyncState> groupSyncStates = await _authDbContext
                .ScimGroupSyncStates
                .Include(s => s.UserGroup)
                .Where(s => s.SCIMAppSettings == scimAppSettings)
                .ToListAsync();

            // Update groups
            foreach (UserGroup group in scimAppSettings.AuthApp.UserGroups)
            {
                BackgroundJob.Enqueue((ISyncHandler syncHandler) => syncHandler.SyncGroupAsync(group.Id, scimAppSettingsId));
            }

            // Remove groups
            List<ScimGroupSyncState> groupsToRemove = groupSyncStates
                .Where(s => !scimAppSettings.AuthApp.UserGroups.Contains(s.UserGroup))
                .ToList();
            foreach (ScimGroupSyncState syncState in groupsToRemove)
            {
                BackgroundJob.Enqueue((ISyncHandler syncHandler) => syncHandler.UnsyncGroupAsync(syncState.UserGroup.Id, scimAppSettingsId));
            }
        }
    }
}
