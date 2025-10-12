using System;
using System.Collections.Generic;
using System.IO;

class Program
{
    static long Next(long s)
    {
        long v = s * 64;
        s ^= v;
        s &= 0xFFFFFF;
        v = s / 32;
        s ^= v;
        s &= 0xFFFFFF;
        v = s * 2048;
        s ^= v;
        s &= 0xFFFFFF;
        return s;
    }

    static int Pack(int a, int b, int c, int d)
    {
        a += 9; b += 9; c += 9; d += 9;
        return (((a * 19 + b) * 19 + c) * 19 + d);
    }

    static void Main()
    {
        var lines = File.ReadAllLines("input.txt");
        var totals = new Dictionary<int, long>(1 << 17);
        foreach (var ln in lines)
        {
            if (string.IsNullOrWhiteSpace(ln)) continue;
            long s = long.Parse(ln.Trim());
            int prevPrice = (int)(s % 10);
            var firstHit = new Dictionary<int, int>(1 << 12);
            bool have1 = false, have2 = false, have3 = false;
            int d1 = 0, d2 = 0, d3 = 0;
            for (int i = 1; i <= 2000; i++)
            {
                s = Next(s);
                int price = (int)(s % 10);
                int d = price - prevPrice;
                if (!have1)
                {
                    d1 = d; have1 = true;
                }
                else if (!have2)
                {
                    d2 = d; have2 = true;
                }
                else if (!have3)
                {
                    d3 = d; have3 = true;
                }
                else
                {
                    int key = Pack(d1, d2, d3, d);
                    if (!firstHit.ContainsKey(key)) firstHit[key] = price;
                    d1 = d2; d2 = d3; d3 = d;
                }
                prevPrice = price;
            }
            foreach (var kv in firstHit)
            {
                if (totals.TryGetValue(kv.Key, out var sum)) totals[kv.Key] = sum + kv.Value;
                else totals[kv.Key] = kv.Value;
            }
        }
        long best = 0;
        foreach (var kv in totals) if (kv.Value > best) best = kv.Value;
        Console.WriteLine(best);
    }
}