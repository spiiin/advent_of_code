using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.IO;

class Program
{
    static void Main()
    {
        var lines = File.ReadAllLines("aoc19_1.txt").ToList();

        var patterns = lines[0]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct()
            .ToArray();

        int blankIdx = lines.FindIndex(1, s => string.IsNullOrWhiteSpace(s));
        var designs = (blankIdx >= 0 ? lines.Skip(blankIdx + 1) : lines.Skip(1))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        var byFirst = new Dictionary<char, List<string>>();
        int maxLen = 0;
        foreach (var p in patterns)
        {
            maxLen = Math.Max(maxLen, p.Length);
            byFirst.TryGetValue(p[0], out var list);
            if (list == null) byFirst[p[0]] = list = new List<string>();
            list.Add(p);
        }

        foreach (var kv in byFirst) kv.Value.Sort((a, b) => b.Length.CompareTo(a.Length));

        BigInteger total = BigInteger.Zero;
        foreach (var s in designs)
        {
            total += CountWaysDP(s, byFirst, maxLen);
        }

        Console.WriteLine(total);
    }

    static BigInteger CountWaysDP(string design, Dictionary<char, List<string>> byFirst, int maxLen)
    {
        int n = design.Length;
        // ways[i] = count of suffixes s[i:]
        var ways = new BigInteger[n + 1];
        ways[n] = BigInteger.One;

        for (int i = n - 1; i >= 0; --i)
        {
            if (!byFirst.TryGetValue(design[i], out var list))
            {
                ways[i] = BigInteger.Zero;
                continue;
            }

            int upto = Math.Min(n - i, maxLen);

            foreach (var p in list)
            {
                int L = p.Length;
                if (L > upto) continue;
                if (design.AsSpan(i, L).SequenceEqual(p))
                {
                    ways[i] += ways[i + L];
                }
            }
        }

        return ways[0];
    }
}
