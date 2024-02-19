
using Microsoft.AspNetCore.SignalR;

namespace BruteForce.RealTime.Implementations;

public class UserIdProvider : IUserIdProvider
{
    public virtual string? GetUserId(HubConnectionContext connection) =>
        connection?.User?.Claims?.FirstOrDefault(c => c.Type.Equals("SignalRId"))?.Value;
}