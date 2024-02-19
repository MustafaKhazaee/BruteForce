
using BruteForce.RealTime.Interfaces;

namespace BruteForce.RealTime.Implementations;

public class RealTimeUserStorage : IRealTimeUserStorage
{
    // UserId to ConnectionId mapper
    private static readonly Dictionary<string, string> _users = [];
    // GroupName to Set of UseIds mapper
    private static readonly Dictionary<string, HashSet<string>> _groupUsers = [];

    public Dictionary<string, string> GetUsersMapping()
    {
        return _users;
    }

    public string? GetGroupNameByConnectionId(string connectionId)
    {
        lock (_users)
        {
            KeyValuePair<string, string>? pair = _users.FirstOrDefault(u => u.Value.Equals(connectionId));

            if (pair is null)
                return null;

            lock (_groupUsers)
            {
                KeyValuePair<string, HashSet<string>>? group = _groupUsers.FirstOrDefault(g => g.Value.Any(u => u.Equals(pair.Value.Key)));

                if (group is null)
                    return null;

                return group.Value.Key;
            }
        }
    }

    public string? GetGroupNameByUserId(string userId)
    {
        lock (_users)
        {
            KeyValuePair<string, string>? pair = _users.FirstOrDefault(u => u.Key.Equals(userId));

            if (pair is null)
                return null;

            lock (_groupUsers)
            {
                KeyValuePair<string, HashSet<string>>? group = _groupUsers.FirstOrDefault(g => g.Value.Any(u => u.Equals(pair.Value.Key)));

                if (group is null)
                    return null;

                return group.Value.Key;
            }
        }
    }

    public string? GetConnectionIdByUserId(string userId)
    {
        lock (_users)
        {
            KeyValuePair<string, string>? pair = _users.FirstOrDefault(u => u.Key.Equals(userId));

            if (pair is null)
                return null;

            return pair.Value.Value;
        }
    }

    public string? GetUserIdByConnectionId(string connectionId)
    {
        lock (_users)
        {
            KeyValuePair<string, string>? pair = _users.FirstOrDefault(u => u.Value.Equals(connectionId));

            if (pair is null)
                return null;

            return pair.Value.Key;
        }
    }

    public List<string> GetConnectionIdsByGroupName(string groupName)
    {
        lock (_groupUsers)
        {
            if (_groupUsers.TryGetValue(groupName, out HashSet<string>? userIds))
            {
                lock (_users)
                {
                    return _users.Where(u => userIds.Contains(u.Key)).Select(pair => pair.Value).ToList();
                }
            }

            return [];
        }
    }

    public List<string> GetUserIdsByGroupName(string groupName)
    {
        lock (_groupUsers)
        {
            if (_groupUsers.TryGetValue(groupName, out HashSet<string>? userIds))
            {
                return [.. userIds];
            }

            return [];
        }
    }

    public void AddUserToGroup(string groupName, string userId)
    {
        lock (_groupUsers)
        {
            if (_groupUsers[groupName] is null)
                _groupUsers[groupName] = [];

            _groupUsers[groupName].Add(userId);
        }
    }

    public void RemoveUserFromGroup(string groupName, string userId)
    {
        lock (_groupUsers)
        {
            if (_groupUsers[groupName] is null)
                return;

            _groupUsers[groupName].Remove(userId);
        }
    }

    public void AddUsersToGroup(string groupName, List<string> userIds)
    {
        lock (_groupUsers)
        {
            if (_groupUsers[groupName] is null)
                _groupUsers[groupName] = [];

            userIds.ForEach(id => _groupUsers[groupName].Add(id));
        }
    }

    public void RemoveUsersFromGroup(string groupName, List<string> userIds)
    {
        lock (_groupUsers)
        {
            if (_groupUsers[groupName] is null)
                return;

            userIds.ForEach(id => _groupUsers[groupName].Remove(id));
        }
    }

    public void AddUser(string userId, string connectionId)
    {
        lock (_users)
        {
            _users.Add(userId, connectionId);
        }
    }

    public void RemoveUser(string userId)
    {
        lock (_users)
        {
            _users.Remove(userId);
        }
    }

    public void AddUsers(Dictionary<string, string> users)
    {
        lock (_users)
        {
            foreach (var item in users)
            {
                _users.Add(item.Key, item.Value);
            }
        }
    }
    
    public void RemoveUsers(List<string> userIds)
    {
        lock (_users)
        {
            foreach (var id in userIds)
            {
                _users.Remove(id);
            }
        }
    }
}
