namespace KosmikAutoUpdate.NET;

public static class Helper {
    /**
     * Computes the SHA256 Hash of a file's content.
     */
    public static string ComputeFileSha256(string path) {
        using var stream = File.OpenRead(path);
        using var md5 = System.Security.Cryptography.SHA256.Create();

        var hashBytes = md5.ComputeHash(stream);

        return Convert.ToHexString(hashBytes);
    }
}