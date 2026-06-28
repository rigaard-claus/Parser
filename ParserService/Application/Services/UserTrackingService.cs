using Microsoft.EntityFrameworkCore;
using ParserService.Data.Contexts;
using ParserService.Data.Entities;

namespace ParserService.Application.Services
{
    public class UserTrackingService(IDbContextFactory<DbTourParser> contextFactory)
    {
        public async Task<UserEntity> GetOrCreateUserAsync(string deviceId)
        {
            using var context = await contextFactory.CreateDbContextAsync();

            var user = await context.Users.FirstOrDefaultAsync(u => u.DeviceId == deviceId);

            if (user == null)
            {
                user = new UserEntity
                {
                    Guid = Guid.NewGuid(),
                    DeviceId = deviceId,
                    FirstSeenAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow
                };
                context.Users.Add(user);
            }
            else
            {
                user.LastSeenAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
            return user;
        }
    }
}
