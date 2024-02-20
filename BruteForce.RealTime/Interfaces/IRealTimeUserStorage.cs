
namespace BruteForce.RealTime.Interfaces;

/// <summary>
/// This singleton service uses in-memory storage.
/// </summary>
public interface IRealTimeUserStorage
{
    /// <summary>
    /// Mapping of UserId to a set of connectionIds
    /// </summary>
    public Dictionary<string, HashSet<string>> GetUsersMapping();

    public string? GetGroupNameByConnectionId(string connectionId);

    public string? GetGroupNameByUserId(string userId);

    public List<string>? GetConnectionIdsByUserId(string userId);

    public string? GetUserIdByConnectionId(string connectionId);

    public List<string> GetConnectionIdsByGroupName(string groupName);

    public List<string> GetUserIdsByGroupName(string groupName);

    public void AddUserToGroup(string groupName, string userId);

    public void RemoveUserFromGroup(string groupName, string userId);

    public void AddUsersToGroup(string groupName, List<string> userIds);

    public void RemoveUsersFromGroup(string groupName, List<string> userIds);

    public void AddUser(string userId, string connectionId);

    public void RemoveUser(string userId);

    public void AddUsers(Dictionary<string, HashSet<string>> users);

    public void RemoveUsers(List<string> userIds);
}
