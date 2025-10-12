using System;
using System.IO;

record Pair(string First, string Second);

class Program
{
    static void Main()
    {
        List<Pair> pairs = [];

        foreach (var line in File.ReadAllLines("input.txt"))
        {
            var parts = line.Split('-');
            if (parts.Length == 2)
            {
                var sorted = parts.OrderBy(x => x).ToArray();
                pairs.Add(new Pair(sorted[0], sorted[1]));
            }
        }
        var adj = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        void AddEdge(string a, string b)
        {
            if (!adj.TryGetValue(a, out var setA)) adj[a] = setA = new HashSet<string>(StringComparer.Ordinal);
            if (!adj.TryGetValue(b, out var setB)) adj[b] = new HashSet<string>(StringComparer.Ordinal);
            setA.Add(b);
            adj[b].Add(a);
        }
        foreach (var (a, b) in pairs) AddEdge(a, b);
        var verts = adj.Keys.OrderBy(x => x, StringComparer.Ordinal).ToList();
        var triangles = new List<(string A, string B, string C)>();

        for (int i = 0; i < verts.Count; i++)
        {
            var a = verts[i];
            if (!adj.TryGetValue(a, out var na)) continue;

            for (int j = i + 1; j < verts.Count; j++)
            {
                var b = verts[j];
                if (!na.Contains(b)) continue;

                var nb = adj[b];
                for (int k = j + 1; k < verts.Count; k++)
                {
                    var c = verts[k];
                    if (na.Contains(c) && nb.Contains(c))
                    {
                        if (a.StartsWith("t") || b.StartsWith("t") || c.StartsWith("t"))
                        {
                            triangles.Add((a, b, c));
                        }
                    }
                }
            }
        }
        foreach (var (a, b, c) in triangles.OrderBy(t => t.A).ThenBy(t => t.B).ThenBy(t => t.C))
            Console.WriteLine($"{a}-{b}-{c}");
        Console.WriteLine(triangles.Count);
    }
}
