using System;
using System.Collections.Generic;

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

    static void Main()
    {
        var lines = File.ReadAllLines("input.txt");

        long total = 0;
        foreach (var ln in lines)
        {
            long s = long.Parse(ln);
            for (int i = 0; i < 2000; i++) s = Next(s);
            total += s;
        }

        Console.WriteLine(total);
    }
}