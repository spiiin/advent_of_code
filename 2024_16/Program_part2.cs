using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    // Порядок направлений: East(0), North(1), West(2), South(3)
    static readonly int[] DR = { 0, -1, 0, 1 };
    static readonly int[] DC = { 1, 0, -1, 0 };
    const long TurnCost = 1000;
    const long StepCost = 1;

    static void Main(string[] args)
    {
        var lines = ReadAllInput(["aoc16_1.txt"]);
        Console.WriteLine(SolvePart2(lines));
    }

    static List<string> ReadAllInput(string[] args)
    {
        if (args.Length > 0 && File.Exists(args[0]))
            return File.ReadAllLines(args[0]).Where(s => s != null && s.Length > 0).ToList();

        var list = new List<string>();
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            if (line.Length == 0) continue; // пропустим пустые строки
            list.Add(line);
        }
        return list;
    }

    static long SolvePart1(List<string> lines, out (int r, int c) S, out (int r, int c) E,
                           out char[,] g, out int H, out int W, out long[,,] distStart)
    {
        Parse(lines, out g, out H, out W, out S, out E);

        distStart = DijkstraFromStart(g, H, W, S);
        long best = long.MaxValue;
        for (int d = 0; d < 4; d++)
            best = Math.Min(best, distStart[E.r, E.c, d]);
        if (best == long.MaxValue) throw new Exception("До цели добраться нельзя.");
        return best;
    }

    static int SolvePart2(List<string> lines)
    {
        long[,,] distS = default!;
        Parse(lines, out var g, out int H, out int W, out var S, out var E);

        // Dijkstra от старта (ориентация фиксирована: East=0)
        distS = DijkstraFromStart(g, H, W, S);

        // Лучшая стоимость как минимум по всем ориентациям в E
        long best = long.MaxValue;
        for (int d = 0; d < 4; d++)
            best = Math.Min(best, distS[E.r, E.c, d]);
        if (best == long.MaxValue) throw new Exception("До цели добраться нельзя.");

        // Обратный Dijkstra от цели: стартуем из четырех состояний (E, d) с нулевой стоимостью
        var distT = DijkstraReverseFromEnd(g, H, W, E);

        // Клетка (r,c) лежит на каком-то оптимальном пути,
        // если существует направление d: distS[r,c,d] + distT[r,c,d] == best
        int countCells = 0;
        for (int r = 0; r < H; r++)
        {
            for (int c = 0; c < W; c++)
            {
                if (g[r, c] == '#') continue;
                bool onAnyBest = false;
                for (int d = 0; d < 4; d++)
                {
                    long a = distS[r, c, d];
                    long b = distT[r, c, d];
                    if (a != long.MaxValue && b != long.MaxValue && a + b == best)
                    {
                        onAnyBest = true;
                        break;
                    }
                }
                if (onAnyBest) countCells++;
            }
        }
        return countCells;
    }

    static void Parse(List<string> lines, out char[,] g, out int H, out int W,
                      out (int r, int c) S, out (int r, int c) E)
    {
        H = lines.Count;
        W = lines[0].Length;
        g = new char[H, W];
        S = (-1, -1);
        E = (-1, -1);
        for (int r = 0; r < H; r++)
        {
            if (lines[r].Length != W) throw new Exception("Неровные строки карты.");
            for (int c = 0; c < W; c++)
            {
                char ch = lines[r][c];
                g[r, c] = ch;
                if (ch == 'S') S = (r, c);
                if (ch == 'E') E = (r, c);
            }
        }
        if (S.r < 0 || E.r < 0) throw new Exception("Нужны S и E на карте.");
    }

    static long[,,] DijkstraFromStart(char[,] g, int H, int W, (int r, int c) S)
    {
        var dist = new long[H, W, 4];
        for (int r = 0; r < H; r++)
            for (int c = 0; c < W; c++)
                for (int d = 0; d < 4; d++)
                    dist[r, c, d] = long.MaxValue;

        bool In(int r, int c) => r >= 0 && r < H && c >= 0 && c < W && g[r, c] != '#';

        // старт: (S, East=0)
        dist[S.r, S.c, 0] = 0;
        var pq = new PriorityQueue<(int r, int c, int d), long>();
        pq.Enqueue((S.r, S.c, 0), 0);

        while (pq.Count > 0)
        {
            pq.TryDequeue(out var st, out var cur);
            if (cur != dist[st.r, st.c, st.d]) continue;

            // поворот влево
            int dl = (st.d + 1) & 3;
            long nl = cur + TurnCost;
            if (nl < dist[st.r, st.c, dl])
            {
                dist[st.r, st.c, dl] = nl;
                pq.Enqueue((st.r, st.c, dl), nl);
            }

            // поворот вправо
            int drt = (st.d + 3) & 3;
            long nr = cur + TurnCost;
            if (nr < dist[st.r, st.c, drt])
            {
                dist[st.r, st.c, drt] = nr;
                pq.Enqueue((st.r, st.c, drt), nr);
            }

            // шаг вперёд
            int r2 = st.r + DR[st.d];
            int c2 = st.c + DC[st.d];
            if (In(r2, c2))
            {
                long nf = cur + StepCost;
                if (nf < dist[r2, c2, st.d])
                {
                    dist[r2, c2, st.d] = nf;
                    pq.Enqueue((r2, c2, st.d), nf);
                }
            }
        }

        return dist;
    }

    // Обратный граф: из (r,c,d) можно прийти
    // - из (r,c,(d+1)&3) и (r,c,(d+3)&3) (кто-то повернулся, чтобы стать d)
    // - из (r-DR[d], c-DC[d], d) (кто-то шагнул вперёд в d и попал в (r,c,d))
    static long[,,] DijkstraReverseFromEnd(char[,] g, int H, int W, (int r, int c) E)
    {
        var dist = new long[H, W, 4];
        for (int r = 0; r < H; r++)
            for (int c = 0; c < W; c++)
                for (int d = 0; d < 4; d++)
                    dist[r, c, d] = long.MaxValue;

        bool In(int r, int c) => r >= 0 && r < H && c >= 0 && c < W && g[r, c] != '#';

        var pq = new PriorityQueue<(int r, int c, int d), long>();

        // стартовые состояния обратного поиска — все 4 ориентации в клетке E
        for (int d = 0; d < 4; d++)
        {
            dist[E.r, E.c, d] = 0;
            pq.Enqueue((E.r, E.c, d), 0);
        }

        while (pq.Count > 0)
        {
            pq.TryDequeue(out var st, out var cur);
            if (cur != dist[st.r, st.c, st.d]) continue;

            // предок через "поворот" (симметрично)
            int dl = (st.d + 1) & 3;
            long nl = cur + TurnCost;
            if (nl < dist[st.r, st.c, dl])
            {
                dist[st.r, st.c, dl] = nl;
                pq.Enqueue((st.r, st.c, dl), nl);
            }

            int drt = (st.d + 3) & 3;
            long nr = cur + TurnCost;
            if (nr < dist[st.r, st.c, drt])
            {
                dist[st.r, st.c, drt] = nr;
                pq.Enqueue((st.r, st.c, drt), nr);
            }

            // предок через "шаг назад" (то есть предыдущая клетка, из которой шагнули вперед)
            int rp = st.r - DR[st.d];
            int cp = st.c - DC[st.d];
            if (In(rp, cp))
            {
                long nf = cur + StepCost;
                if (nf < dist[rp, cp, st.d])
                {
                    dist[rp, cp, st.d] = nf;
                    pq.Enqueue((rp, cp, st.d), nf);
                }
            }
        }

        return dist;
    }
}
