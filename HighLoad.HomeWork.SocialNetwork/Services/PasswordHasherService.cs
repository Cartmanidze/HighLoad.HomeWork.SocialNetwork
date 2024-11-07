using System.Security.Cryptography;
using HighLoad.HomeWork.SocialNetwork.Interfaces;

namespace HighLoad.HomeWork.SocialNetwork.Services;

public class PasswordHasherService : IPasswordHasher
{
    public string HashPassword(string password)
    {
        using var deriveBytes = new Rfc2898DeriveBytes(password, 16, 10000, HashAlgorithmName.SHA1);
        var salt = deriveBytes.Salt;
        var key = deriveBytes.GetBytes(32);

        var hashBytes = new byte[48];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(key, 0, hashBytes, 16, 32);

        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        var hashBytes = Convert.FromBase64String(storedHash);
        var salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);

        using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA1);
        var key = deriveBytes.GetBytes(32);

        for (var i = 0; i < 32; i++)
        {
            if (hashBytes[i + 16] != key[i])
            {
                return false;
            }
        }

        return true;
    }
}