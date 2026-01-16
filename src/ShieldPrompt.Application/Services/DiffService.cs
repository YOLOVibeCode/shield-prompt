using System.Text;
using ShieldPrompt.Application.Interfaces;

namespace ShieldPrompt.Application.Services;

/// <summary>
/// Service for computing file diffs using a simple line-by-line comparison.
/// </summary>
public class DiffService : IDiffService
{
    /// <inheritdoc />
    public IReadOnlyList<DiffLine> ComputeDiff(string original, string modified)
    {
        var originalLines = SplitLines(original);
        var modifiedLines = SplitLines(modified);

        var result = new List<DiffLine>();

        // Use simple LCS (Longest Common Subsequence) based diff
        var lcs = ComputeLcs(originalLines, modifiedLines);
        var diff = BuildDiff(originalLines, modifiedLines, lcs);

        return diff;
    }

    /// <inheritdoc />
    public string GenerateUnifiedDiff(string original, string modified, string filePath)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine($"--- a/{filePath}");
        sb.AppendLine($"+++ b/{filePath}");

        var diffLines = ComputeDiff(original, modified);

        if (diffLines.Count == 0)
            return sb.ToString();

        // Simple unified diff output
        foreach (var line in diffLines)
        {
            var prefix = line.Type switch
            {
                DiffLineType.Added => "+",
                DiffLineType.Removed => "-",
                _ => " "
            };
            sb.AppendLine($"{prefix}{line.Content}");
        }

        return sb.ToString();
    }

    private static string[] SplitLines(string content)
    {
        if (string.IsNullOrEmpty(content))
            return Array.Empty<string>();

        return content.Split('\n').Select(l => l.TrimEnd('\r')).ToArray();
    }

    private static int[,] ComputeLcs(string[] a, string[] b)
    {
        var m = a.Length;
        var n = b.Length;
        var dp = new int[m + 1, n + 1];

        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                if (a[i - 1] == b[j - 1])
                    dp[i, j] = dp[i - 1, j - 1] + 1;
                else
                    dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
            }
        }

        return dp;
    }

    private static List<DiffLine> BuildDiff(string[] original, string[] modified, int[,] lcs)
    {
        var result = new List<DiffLine>();
        var i = original.Length;
        var j = modified.Length;

        var tempResult = new List<DiffLine>();

        while (i > 0 || j > 0)
        {
            if (i > 0 && j > 0 && original[i - 1] == modified[j - 1])
            {
                tempResult.Add(new DiffLine(DiffLineType.Context, i, j, original[i - 1]));
                i--;
                j--;
            }
            else if (j > 0 && (i == 0 || lcs[i, j - 1] >= lcs[i - 1, j]))
            {
                tempResult.Add(new DiffLine(DiffLineType.Added, null, j, modified[j - 1]));
                j--;
            }
            else if (i > 0)
            {
                tempResult.Add(new DiffLine(DiffLineType.Removed, i, null, original[i - 1]));
                i--;
            }
        }

        tempResult.Reverse();
        return tempResult;
    }
}
