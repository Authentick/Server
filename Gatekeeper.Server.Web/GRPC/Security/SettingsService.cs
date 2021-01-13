using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthServer.Server.Models;
using AuthServer.Server.Services.Authentication.TwoFactorAuthenticators.Implementation;
using AuthServer.Server.Services.User;
using AuthServer.Shared.Security;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace AuthServer.Server.GRPC.Security
{
    [Authorize]
    public class SettingsService : AuthServer.Shared.Security.Settings.SettingsBase
    {
        private readonly UserManager _userManager;
        private readonly AuthDbContext _authDbContext;

        public SettingsService(
            UserManager userManager,
            AuthDbContext authDbContext)
        {
            _userManager = userManager;
            _authDbContext = authDbContext;
        }

        public override async Task<AddNewAuthenticatorAppReply> AddNewAuthenticatorApp(AddNewAuthenticatorAppRequest request, ServerCallContext context)
        {
            AppUser? user = await _userManager.GetUserAsync(context.GetHttpContext().User);

            UserTotpDevice device = new UserTotpDevice
            {
                CreationTime = SystemClock.Instance.GetCurrentInstant(),
                Name = request.Name,
                SharedSecret = request.SharedSecret,
                User = user,
            };
            _authDbContext.Add(device);
            await _authDbContext.SaveChangesAsync();

            // FIXME: This should really not be in here
            await _userManager.SetTwoFactorEnabledAsync(user, true);

            return new AddNewAuthenticatorAppReply
            {
                Success = true,
            };
        }

        public override async Task<ChangePasswordReply> ChangePassword(ChangePasswordRequest request, ServerCallContext context)
        {
            AppUser? user = await _userManager.GetUserAsync(context.GetHttpContext().User);

            if (user == null)
            {
                return new ChangePasswordReply { Success = false };
            }

            IdentityResult result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            if (result.Succeeded)
            {
                return new ChangePasswordReply { Success = true };
            }

            return new ChangePasswordReply { Success = false };
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override Task<NewAuthenticatorSecret> GetNewAuthenticatorSecret(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new NewAuthenticatorSecret
            {
                Secret = Base32.ToBase32(Rfc6238AuthenticationService.GenerateRandomKey()),
            });
        }

        public override async Task<TwoFactorListReply> ListAuthenticatorApps(Empty request, ServerCallContext context)
        {
            AppUser? user = await _userManager.GetUserAsync(context.GetHttpContext().User);

            TwoFactorListReply reply = new TwoFactorListReply();
            List<UserTotpDevice> devices = await _authDbContext.UserTotpDevices
                .AsNoTracking()
                .Where(d => d.User == user)
                .ToListAsync();

            foreach (UserTotpDevice device in devices)
            {
                TwoFactorDevice replyDevice = new TwoFactorDevice
                {
                    Added = NodaTime.Serialization.Protobuf.NodaExtensions.ToTimestamp(device.CreationTime),
                    Name = device.Name,
                    Id = device.Id.ToString(),
                    LastUsed = (device.LastUsedTime != null) ? NodaTime.Serialization.Protobuf.NodaExtensions.ToTimestamp(device.LastUsedTime) : null,
                };

                reply.TwoFactorDevices.Add(replyDevice);
            }

            return reply;
        }

        public override Task<TwoFactorListReply> ListAuthenticatorKeys(Empty request, ServerCallContext context)
        {
            return base.ListAuthenticatorKeys(request, context);
        }

        public override async Task<RemoveAuthenticatorReply> RemoveAuthenticator(RemoveAuthenticatorRequest request, ServerCallContext context)
        {
            Guid id = new Guid(request.Id);
            AppUser user = await _userManager.GetUserAsync(context.GetHttpContext().User);

            UserTotpDevice device = await _authDbContext.UserTotpDevices
                .Where(d => d.User == user)
                .Where(d => d.Id == id)
                .SingleAsync();

            int deviceCount = await _authDbContext.UserTotpDevices
                .Where(d => d.User == user)
                .CountAsync();

            _authDbContext.Remove(device);
            await _authDbContext.SaveChangesAsync();

            // FIXME: Should really not be in here
            if(deviceCount == 1)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, false);
            }

            return new RemoveAuthenticatorReply
            {
                Success = true,
            };
        }
    }
}