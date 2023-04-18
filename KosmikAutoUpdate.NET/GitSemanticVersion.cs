using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KosmikAutoUpdate.NET;

[JsonConverter(typeof(GitSemanticVersionJsonConverter))]
public record GitSemanticVersion(int Major, int Minor, int Patch, int Commits) : IComparable {
    
    public GitSemanticVersion(string str) : this(0, 0, 0, 0) {
        try {
            var a = str.Split("+");
            if (a.Length > 1) Commits = int.Parse(a[1]);

            var b = a[0].Split(".");
            Major = int.Parse(b[0]);
            Minor = int.Parse(b[1]);
            Patch = int.Parse(b[2]);
        }
        catch (Exception ex) {
            throw new ArgumentException("Invalid Version string!", nameof(str), ex);
        }
    }

    public override string ToString() =>
        Commits == 0 ? $"{Major}.{Minor}.{Patch}" : $"{Major}.{Minor}.{Patch}+{Commits}";

    #region Comparing
    
    public int CompareTo(object? obj) {
        if (obj is GitSemanticVersion other)
            return GitSemanticVersionComparer.Compare(this, other);
        return 1;
    }

    private sealed class GitSemanticVersionRelationalComparer : IComparer<GitSemanticVersion> {
        public int Compare(GitSemanticVersion x, GitSemanticVersion y) {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            var majorComparison = x.Major.CompareTo(y.Major);
            if (majorComparison != 0) return majorComparison;
            var minorComparison = x.Minor.CompareTo(y.Minor);
            if (minorComparison != 0) return minorComparison;
            var patchComparison = x.Patch.CompareTo(y.Patch);
            if (patchComparison != 0) return patchComparison;
            return x.Commits.CompareTo(y.Commits);
        }
    }

    public static IComparer<GitSemanticVersion> GitSemanticVersionComparer { get; } =
        new GitSemanticVersionRelationalComparer();
    
    #endregion

    #region JSON Converter between GitSemanticVersion and JSON string
    
    private class GitSemanticVersionJsonConverter : JsonConverter<GitSemanticVersion> {
        public override GitSemanticVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            return new GitSemanticVersion(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, GitSemanticVersion value, JsonSerializerOptions options) {
            writer.WriteStringValue(value.ToString());
        }
    }

    #endregion
}