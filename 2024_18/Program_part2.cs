using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    //   dotnet run -- 71               < input.txt
    //   dotnet run -- 71 0             < input.txt   // startCheck=0
    //   dotnet run -- 71 1000          < input.txt   // start from 1000
    //
    static void Main(string[] args)
    {
        int size = args.Length >= 1 && int.TryParse(args[0], out var s) ? s : 71;
        int startCheck = args.Length >= 2 && int.TryParse(args[1], out var sc) ? sc : 0;

        var coords = ReadAllCoordsFromStdIn().ToList();
        var blocked = new bool[size, size];

        for (int i = 0; i < coords.Count; i++)
        {
            var (x, y) = coords[i];
            if (InBounds(x, y, size))
                blocked[x, y] = true;

            if (i + 1 < startCheck) continue;

            if (!InBounds(0, 0, size) || !InBounds(size - 1, size - 1, size) ||
                blocked[0, 0] || blocked[size - 1, size - 1] || !Reachable(blocked, size))
            {
                Console.WriteLine($"{x},{y}");
                return;
            }
        }

        Console.WriteLine("No cutoff");
    }

    static IEnumerable<(int x, int y)> ReadAllCoordsFromStdIn()
    {
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 2) continue;
            yield return (int.Parse(parts[0]), int.Parse(parts[1]));
        }
    }

    static bool InBounds(int x, int y, int size) => x >= 0 && y >= 0 && x < size && y < size;

    static bool Reachable(bool[,] blocked, int size)
    {
        if (blocked[0, 0] || blocked[size - 1, size - 1]) return false;

        var dist = new sbyte[size, size];
        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                dist[i, j] = -1;

        var q = new Queue<(int x, int y)>();
        q.Enqueue((0, 0));
        dist[0, 0] = 0;

        // 4-neighborhood
        int[] dx = { 1, -1, 0, 0 };
        int[] dy = { 0, 0, 1, -1 };

        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue();
            if (x == size - 1 && y == size - 1) return true;

            for (int dir = 0; dir < 4; dir++)
            {
                int nx = x + dx[dir], ny = y + dy[dir];
                if (nx < 0 || ny < 0 || nx >= size || ny >= size) continue;
                if (blocked[nx, ny]) continue;
                if (dist[nx, ny] != -1) continue;
                dist[nx, ny] = 0;
                q.Enqueue((nx, ny));
            }
        }
        return false;
    }
}
