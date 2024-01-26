namespace BruteForce.Domain.Interfaces;

public interface ICurrentUser
{
    string GetUserName();

    /// <summary>
    /// Returns the Id of user as string (It may be int or guid and you have to convert it yourself)
    /// </summary>
    /// <returns></returns>
    string GetUserId();
}
