using Microsoft.Extensions.Caching.Memory;

namespace WEBDOAN.Models;

public class OnlineUserService : IOnlineUserService
{
    private readonly IMemoryCache _cache;
    private const string Key = "OnlineUsers";

    public OnlineUserService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void UserActive(string userId)
    {
        var users = _cache.Get<Dictionary<string, DateTime>>(Key) ?? new();
        users[userId] = DateTime.UtcNow;
        _cache.Set(Key, users, TimeSpan.FromMinutes(10));
    }

    public void UserInactive(string userId)
    {
        var users = _cache.Get<Dictionary<string, DateTime>>(Key);
        if (users != null && users.ContainsKey(userId))
        {
            users.Remove(userId);
            _cache.Set(Key, users, TimeSpan.FromMinutes(10));
        }
    }

    public int GetOnlineCount()
    {
        var users = _cache.Get<Dictionary<string, DateTime>>(Key);
        if (users == null) return 0;

        var threshold = DateTime.UtcNow.AddMinutes(-5);
        return users.Values.Count(t => t >= threshold);
    }
}
