using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    //   dotnet run            // size=71, drops=1024
    //   dotnet run 71 1024
    //   type input.txt | dotnet run
    static void Main(string[] args)
    {
        int size = args.Length >= 1 && int.TryParse(args[0], out var s) ? s : 71;
        int drops = args.Length >= 2 && int.TryParse(args[1], out var d) ? d : 1024;

        var lines = ReadAllLinesFromStdIn()
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        var coords = new List<(int x, int y)>(lines.Count);
        foreach (var line in lines)
        {
            var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 2) continue;
            int x = int.Parse(parts[0]);
            int y = int.Parse(parts[1]);
            coords.Add((x, y));
        }

        var blocked = new bool[size, size];
        int n = Math.Min(drops, coords.Count);
        for (int i = 0; i < n; i++)
        {
            var (x, y) = coords[i];
            if (x >= 0 && x < size && y >= 0 && y < size)
                blocked[x, y] = true;
        }

        int answer = BfsShortestPath(blocked, size);
        Console.WriteLine(answer);
    }

    static int BfsShortestPath(bool[,] blocked, int size)
    {
        if (blocked[0, 0] || blocked[size - 1, size - 1])
            return -1;

        var dist = new int[size, size];
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                dist[x, y] = -1;

        var q = new Queue<(int x, int y)>();
        q.Enqueue((0, 0));
        dist[0, 0] = 0;

        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue();
            int d = dist[x, y];

            if (x == size - 1 && y == size - 1)
                return d;

            for (int dir = 0; dir < 4; dir++)
            {
                int nx = x + dx[dir];
                int ny = y + dy[dir];
                if (nx < 0 || ny < 0 || nx >= size || ny >= size) continue;
                if (blocked[nx, ny]) continue;
                if (dist[nx, ny] != -1) continue;
                dist[nx, ny] = d + 1;
                q.Enqueue((nx, ny));
            }
        }

        return -1; //no way
    }

    static IEnumerable<string> ReadAllLinesFromStdIn()
    {
        string? line;
        while ((line = Console.ReadLine()) != null)
            yield return line;
    }
}
