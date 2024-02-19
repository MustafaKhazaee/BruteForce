
namespace BruteForce.RealTime.Interfaces;

/// <summary>
/// This singleton service uses in-memory storage.
/// </summary>
public interface IRealTimeUserStorage
{
    public Dictionary<string, string> GetUsersMapping();

    public string? GetGroupNameByConnectionId(string connectionId);

    public string? GetGroupNameByUserId(string userId);

    public string? GetConnectionIdByUserId(string userId);

    public string? GetUserIdByConnectionId(string connectionId);

    public List<string> GetConnectionIdsByGroupName(string groupName);

    public List<string> GetUserIdsByGroupName(string groupName);

    public void AddUserToGroup(string groupName, string userId);

    public void RemoveUserFromGroup(string groupName, string userId);

    public void AddUsersToGroup(string groupName, List<string> userIds);

    public void RemoveUsersFromGroup(string groupName, List<string> userIds);

    public void AddUser(string userId, string connectionId);

    public void RemoveUser(string userId);

    public void AddUsers(Dictionary<string, string> users);

    public void RemoveUsers(List<string> userIds);
}
