using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    // Направления в таком порядке: East(0), North(1), West(2), South(3)
    // Тогда поворот влево: (d+1)%4, вправо: (d+3)%4
    static readonly int[] dr = { 0, -1, 0, 1 };
    static readonly int[] dc = { 1, 0, -1, 0 };

    const long TurnCost = 1000;
    const long StepCost = 1;

    static void Main(string[] args)
    {
        var lines = ReadAllInput(["aoc16_1.txt"]);
        Console.WriteLine(Solve(lines));
    }

    static List<string> ReadAllInput(string[] args)
    {
        if (args.Length > 0 && File.Exists(args[0]))
            return File.ReadAllLines(args[0]).Where(s => s != null).ToList();

        var list = new List<string>();
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            if (line.Length == 0) continue; // пропустим пустые строки между примерами
            list.Add(line);
        }
        return list;
    }

    static long Solve(List<string> lines)
    {
        int H = lines.Count;
        int W = lines[0].Length;

        char[,] g = new char[H, W];
        (int r, int c) start = (-1, -1);
        (int r, int c) goal = (-1, -1);

        for (int r = 0; r < H; r++)
        {
            for (int c = 0; c < W; c++)
            {
                char ch = lines[r][c];
                g[r, c] = ch;
                if (ch == 'S') start = (r, c);
                if (ch == 'E') goal = (r, c);
            }
        }

        if (start.r < 0 || goal.r < 0)
            throw new Exception("Карта должна содержать S и E.");

        // dist[r, c, d]
        var dist = new long[H, W, 4];
        for (int r = 0; r < H; r++)
            for (int c = 0; c < W; c++)
                for (int d = 0; d < 4; d++)
                    dist[r, c, d] = long.MaxValue;

        // Старт: в клетке S, смотрим на восток (East = 0)
        dist[start.r, start.c, 0] = 0;

        var pq = new PriorityQueue<(int r, int c, int d), long>();
        pq.Enqueue((start.r, start.c, 0), 0);

        bool InBounds(int r, int c) => r >= 0 && r < H && c >= 0 && c < W;
        bool IsWall(int r, int c) => g[r, c] == '#';

        while (pq.Count > 0)
        {
            pq.TryDequeue(out var state, out var cur);
            int r = state.r, c = state.c, d = state.d;

            if (cur != dist[r, c, d]) continue; // устаревшая запись

            // Как только достали любую ориентацию в клетке E — это оптимальная стоимость
            if (r == goal.r && c == goal.c)
                return cur;

            // Поворот влево
            int dl = (d + 1) & 3;
            long nl = cur + TurnCost;
            if (nl < dist[r, c, dl])
            {
                dist[r, c, dl] = nl;
                pq.Enqueue((r, c, dl), nl);
            }

            // Поворот вправо
            int drt = (d + 3) & 3;
            long nr = cur + TurnCost;
            if (nr < dist[r, c, drt])
            {
                dist[r, c, drt] = nr;
                pq.Enqueue((r, c, drt), nr);
            }

            // Шаг вперёд
            int r2 = r + dr[d], c2 = c + dc[d];
            if (InBounds(r2, c2) && !IsWall(r2, c2))
            {
                long nf = cur + StepCost;
                if (nf < dist[r2, c2, d])
                {
                    dist[r2, c2, d] = nf;
                    pq.Enqueue((r2, c2, d), nf);
                }
            }
        }

        throw new Exception("До цели добраться нельзя.");
    }
}
