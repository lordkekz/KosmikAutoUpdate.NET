using System.Security.Cryptography;
using System.Text;

namespace KosmikAutoUpdate.NET;

public static class Utility {
    /**
     * Computes the SHA256 Hash of a file's content.
     */
    public static string ComputeFileSha256(string path) {
        using var stream = File.OpenRead(path);
        var hashBytes = SHA256.HashData(stream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /**
     * Computes the SHA256 Hash of a string in UTF8 encoding..
     */
    public static string ComputeSha256(string str) {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(str));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}