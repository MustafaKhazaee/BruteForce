
using System.Text;
using System.Security.Cryptography;

namespace BruteForce.Application.Extensions;

public static class Hashing
{
    public static string GetHash(this string text)
    {
        byte[] hashedBytes = SHA256.HashData(Encoding.Unicode.GetBytes(text));

        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
    }

    public static string GetSalt(this string text)
    {
        byte[] bytes = new byte[16];

        using RandomNumberGenerator keyGenerator = RandomNumberGenerator.Create();

        keyGenerator.GetBytes(bytes);

        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    public static string AddSalt(this string text) => $"{text}{GetSalt(text)}";
}