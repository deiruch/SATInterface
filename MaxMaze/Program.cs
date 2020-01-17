﻿using SATInterface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxMaze
{
    class Program
    {
        const int W = 30;
        const int H = 30;
        const string input = " ....................#        " +
                            ".....................# XXXXXX " +
                            ".###....###########..#   X    " +
                            "...#..................#X X.XXX" +
                            "...#...................# X    " +
                            "...#...................  XXXX " +
                            "...#....................X   X " +
                            "......................... #   " +
                            "...........................#XX" +
                            ".........######.............##" +
                            ".........#...................." +
                            ".........#....#..............." +
                            ".........#....#..............." +
                            ".........#....#..............." +
                            "..#...........#..............." +
                            "..###....#....#........#######" +
                            ".........###.##..............." +
                            ".............................." +
                            ".............................." +
                            "....##.....................#.." +
                            "...#####.................###.." +
                            "...#####...................#.." +
                            "...#######....#..............." +
                            "....#######..#................" +
                            "......####..#X.......#........" +
                            ".......###.#XX.......#........" +
                            "........#..#####....###......." +
                            "...........#     ............." +
                            "...........# XXXX............." +
                            "...........#     ............ ";

        static void Main(string[] args)
        {
            var model = new Model();
            var free = new BoolExpr[W, H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    switch (input[30 * y + x])
                    {
                        case '.':
                            //case ' ':
                            free[x, y] = new BoolVar(model, $"f{x},{y}");
                            break;
                        case ' ':
                            free[x, y] = true;
                            //free[x, y] = new BoolVar(model, $"f{x},{y}");
                            break;
                        case '#':
                        case 'X':
                            free[x, y] = false;
                            break;
                        default:
                            throw new Exception();
                    }
                }

            free[0, 0] = true;
            free[W - 1, H - 1] = true;

            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    BoolExpr a = x > 0 ? free[x - 1, y] : false;
                    BoolExpr b = x < W - 1 ? free[x + 1, y] : false;
                    BoolExpr c = y > 0 ? free[x, y - 1] : false;
                    BoolExpr d = y < H - 1 ? free[x, y + 1] : false;

                    if (x == 0 && y == 0)
                        model.AddConstr(b != d);
                    else if (x == W - 1 && y == H - 1)
                        model.AddConstr(a != c);
                    else if (!ReferenceEquals(free[x, y], BoolExpr.False))
                        model.AddConstr(!free[x, y] | model.ExactlyKOf(new[] { a, b, c, d }, 2));
                }

            //cut: quad
            for (int y = 0; y < H - 1; y++)
                for (int x = 0; x < W - 1; x++)
                    model.AddConstr(!free[x, y] | !free[x + 1, y] | !free[x, y + 1] | !free[x + 1, y + 1]);

            model.Maximize(model.Sum(free.Cast<BoolVar>()), () =>
            {
                for (int y = 0; y < H; y++)
                {
                    for (int x = 0; x < W; x++)
                        Console.Write(free[x, y].X ? "." : ReferenceEquals(free[x, y], BoolExpr.False) ? "█" : "▒");
                    Console.WriteLine();
                }
            }, Model.OptimizationStrategy.Increasing);

            Console.ReadLine();
            Console.ReadLine();
            Console.ReadLine();
        }
    }
}