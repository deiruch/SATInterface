﻿using SATInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maximize
{
    class Program
    {
        static void Main(string[] args)
        {
            var m = new Model();

            var x = new UIntVar(m, 1000);
            var y = new UIntVar(m, 200);
            var c = new UIntVar(m, 1000 * 200);

            m.AddConstr((x < 512) | (y < 100));
            m.AddConstr(c == x * y);

            m.LogOutput = false;
            m.Maximize(x + 7 * y, () => Console.WriteLine($"Intermediate result: {x.X} + 7*{y.X} = {x.X + 7 * y.X}, x*y = {c.X}"), Model.OptimizationStrategy.BinarySearch);

            Console.WriteLine($"Final result: {x.X} + 7*{y.X} = {x.X + 7 * y.X}, x*y = {c.X}");

            var best = (Val: 0, X: 0, Y: 0);
            for (var xt = 0; xt <= 1000; xt++)
                for (var yt = 0; yt <= 200; yt++)
                    if ((xt < 512) || (yt < 100))
                    {
                        //var ct = xt * yt;
                        var val = xt + 7 * yt;
                        if (val > best.Val)
                            best = (Val: val, X: xt, Y: yt);
                    }

            Console.WriteLine($"Exhaustive search found: {best.X} + 7*{best.Y} = {best.X + 7 * best.Y}, x*y = {best.X*best.Y}");
            Console.ReadLine();
        }
    }
}
