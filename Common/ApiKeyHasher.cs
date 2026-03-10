using System.Security.Cryptography;
using System.Text;

namespace Sentinel.Common;

public static class ApiKeyHasher
{
    public static string Hash(string rawKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(bytes).ToLower();
    }

}