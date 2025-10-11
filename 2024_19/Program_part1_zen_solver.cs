using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.IO;
using Microsoft.Z3;
using ZenLib.Solver;
using static ZenLib.Zen;

class Program
{
    static void Main()
    {
        var lines = File.ReadAllLines("aoc19_1.txt").ToList();
        var patterns = lines[0]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
        int blankIdx = lines.FindIndex(1, s => string.IsNullOrWhiteSpace(s));
        var designs = (blankIdx >= 0 ? lines.Skip(blankIdx + 1) : lines.Skip(1))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        string alt = string.Join("|", patterns.Select(EscapeForRegex));
        string regexText = $"^({alt})+$";

        ZenLib.Regex<char> zenRegex;
        zenRegex = ZenLib.Regex.Parse(regexText);

        var cfg = new SolverConfig
        {
            SolverType = SolverType.Z3,
            SolverTimeout = TimeSpan.FromMilliseconds(500)
        };

        int possible = 0;

        foreach (var design in designs)
        {
            var s = Symbolic<string>();
            var constraint = And(s == design, s.MatchesRegex(zenRegex));
            if (constraint.Solve(config: cfg).VariableAssignment != null)
            {
                possible++;
            }
        }

        Console.WriteLine(possible);
    }

    static string EscapeForRegex(string p)
        => System.Text.RegularExpressions.Regex.Escape(p);
}

