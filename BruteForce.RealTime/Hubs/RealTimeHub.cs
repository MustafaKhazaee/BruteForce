
using Microsoft.AspNetCore.SignalR;
using BruteForce.Domain.Interfaces;
using BruteForce.RealTime.Interfaces;

namespace BruteForce.RealTime.Hubs;

/// <summary>
/// Using this Hub will automatically add mapping from userId to connection when user is connected.
/// </summary>
public class RealTimeHub(IRealTimeUserStorage userStorage, ICurrentUser currentUser) : Hub
{
    private readonly IRealTimeUserStorage _userStorage = userStorage;
    private readonly string _userId = currentUser.GetUserId();

    public override Task OnConnectedAsync()
    {
        _userStorage.AddUser(_userId, Context.ConnectionId);
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _userStorage.RemoveUser(_userId);
        return Task.CompletedTask;
    }

    public Dictionary<string, HashSet<string>> GetUserMapping() => _userStorage.GetUsersMapping();

    public async Task SendMessageAsync(string userId, string message, string methodName, CancellationToken cancellationToken = default)
        => await Clients.User(userId).SendAsync(methodName, message, cancellationToken);

    public async Task SendMessageToGroupAsync(string groupName, string methodName, string message, CancellationToken cancellationToken = default)
        => await Clients.Group(groupName).SendAsync(methodName, message, cancellationToken);
    
    public async Task AddUserToGroupAsync(string groupName, string userId, CancellationToken cancellationToken = default)
    {
        List<string>? connectionIds = _userStorage.GetConnectionIdsByUserId(userId);
        if (connectionIds is null)
            return;

        connectionIds.ForEach(async id => await Groups.AddToGroupAsync(id, groupName, cancellationToken));

        _userStorage.AddUserToGroup(groupName, _userId);
    }

    public async Task RemoveUserFromGroupAsync(string groupName, string userId, CancellationToken cancellationToken = default)
    {
        List<string>? connectionIds = _userStorage.GetConnectionIdsByUserId(userId);

        if (connectionIds is null)
            return;

        connectionIds.ForEach(async id => await Groups.RemoveFromGroupAsync(id, groupName, cancellationToken));

        _userStorage.RemoveUserFromGroup(groupName, _userId);
    }

    public async Task AddCurrentUserToGroupAsync(string groupName, CancellationToken cancellationToken = default)
    {
        string connectionId = Context.ConnectionId;

        await Groups.AddToGroupAsync(connectionId, groupName, cancellationToken);

        _userStorage.AddUserToGroup(groupName, _userId);
    }

    public async Task RemoveCurrentUserFromGroupAsync(string groupName, CancellationToken cancellationToken = default)
    {
        string connectionId = Context.ConnectionId;

        await Groups.RemoveFromGroupAsync(connectionId, groupName, cancellationToken);

        _userStorage.RemoveUserFromGroup(groupName, _userId);
    }

    public async Task SendToOthersAsync(string methodName, string message, CancellationToken cancellationToken = default)
        => await Clients.Others.SendAsync(methodName, message, cancellationToken);

    public async Task SendToAllAsync(string methodName, string message, CancellationToken cancellationToken = default)
        => await Clients.All.SendAsync(methodName, message, cancellationToken);

    public async Task SendToCallerAsync(string methodName, string message, CancellationToken cancellationToken = default)
        => await Clients.Caller.SendAsync(methodName, message, cancellationToken);

    public async Task SendToOthersInGroupAsync(string groupName, string methodName, string message, CancellationToken cancellationToken = default)
        => await Clients.OthersInGroup(groupName).SendAsync(methodName, message, cancellationToken);

    public async Task SendToUsersAsync(IReadOnlyList<string> userIds, string methodName, string message, CancellationToken cancellationToken = default)
        => await Clients.Users(userIds).SendAsync(methodName, message, cancellationToken);
}
