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

        var allVertices = adj.Keys.ToList();
        List<string> bestClique = new();

        void BronKerbosch(HashSet<string> R, HashSet<string> P, HashSet<string> X)
        {
            if (P.Count == 0 && X.Count == 0)
            {
                if (R.Count > bestClique.Count)
                {
                    bestClique = R.OrderBy(s => s, StringComparer.Ordinal).ToList();
                }
                return;
            }
            string pivot = null;
            var unionPX = new HashSet<string>(P, StringComparer.Ordinal);
            unionPX.UnionWith(X);
            int maxNeighbors = -1;
            foreach (var u in unionPX)
            {
                if (!adj.TryGetValue(u, out var nu)) continue;
                int cnt = nu.Count(v => P.Contains(v));
                if (cnt > maxNeighbors) { maxNeighbors = cnt; pivot = u; }
            }

            // candidates = P \ N(pivot)
            IEnumerable<string> candidates;
            if (pivot == null) candidates = P.ToList();
            else
            {
                var neigh = adj.ContainsKey(pivot) ? adj[pivot] : new HashSet<string>(StringComparer.Ordinal);
                candidates = P.Where(v => !neigh.Contains(v)).ToList();
            }

            foreach (var v in candidates.ToList())
            {
                var Rnew = new HashSet<string>(R, StringComparer.Ordinal) { v };
                var Pnew = new HashSet<string>(adj[v], StringComparer.Ordinal);
                Pnew.IntersectWith(P);
                var Xnew = new HashSet<string>(adj[v], StringComparer.Ordinal);
                Xnew.IntersectWith(X);
                BronKerbosch(Rnew, Pnew, Xnew);
                P.Remove(v);
                X.Add(v);
                if (R.Count + P.Count <= bestClique.Count) return;
            }
        }

        var R0 = new HashSet<string>(StringComparer.Ordinal);
        var P0 = new HashSet<string>(allVertices, StringComparer.Ordinal);
        var X0 = new HashSet<string>(StringComparer.Ordinal);
        BronKerbosch(R0, P0, X0);
        var password = string.Join(',', bestClique);
        Console.WriteLine(password);
    }
}
