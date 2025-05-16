using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MazeCApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            //Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.ForegroundColor = ConsoleColor.Black;
            while (true)
            {
                int x = 29, y = 120;
                //int x = 50, y = 50;
                Maze maze = new Maze(x, y, 1, 1);
                maze.FillMaze(1, y / 2, "w", 0);
                //maze.PrintMazeInstant();
                maze.PrintMazeSlow();
                maze.SolveMaze();
            }
        }
    }
    public class Cell
    {
        public int path = int.MaxValue;
        private Dictionary<string, string> cells = new Dictionary<string, string>
        {
            { "0000"," "},
            { "1000","╨"},
            { "0100","╞"},
            { "0010","╥"},
            { "0001","╡"},
            { "0011","╗"},
            { "1001","╝"},
            { "1100","╚"},
            { "0110","╔"},
            { "1010","║"},
            { "0101","═"},
            { "0111","╦"},
            { "1011","╣"},
            { "1101","╩"},
            { "1110","╠"},
            { "1111","╬"}
        };
        private int[] walls = { 0, 0, 0, 0 };
        public void SetWall(string direction, int value)
        {
            if (direction == "w")
            {
                walls[0] = value;
            }
            else if (direction == "a")
            {
                walls[3] = value;
            }
            else if (direction == "s")
            {
                walls[2] = value;
            }
            else if (direction == "d")
            {
                walls[1] = value;
            }
        }
        public string GetCellValue()
        {
            string result = walls[0].ToString() + walls[1].ToString() + walls[2].ToString() + walls[3].ToString();
            return cells[result];
        }

    }
    public class Maze
    {
        int heigth;
        int width;
        int drawSpeed;
        int solveSpeed;
        private Cell[,] maze;
        Random rnd = new Random();
        HashSet<(int, int)> visited = new HashSet<(int, int)>();
        Stack<(int, int)> path = new Stack<(int, int)>();
        Queue<(int, int)> render = new Queue<(int, int)>();

        public Maze(int heigth, int width, int drawSpeed, int solveSpeed)
        {
            this.heigth = heigth;
            this.width = width;
            maze = new Cell[heigth, width];
            for (int i = 0; i < heigth; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    maze[i, j] = new Cell(); // Initialize each cell
                }
            }
            maze[0, width / 2].SetWall("w", 1);
            maze[0, width / 2].SetWall("s", 1);
            maze[heigth - 1, width / 2].SetWall("w", 1);
            maze[heigth - 1, width / 2].SetWall("s", 1);
            this.drawSpeed = drawSpeed;
            this.solveSpeed = solveSpeed;
        }
        public void FillMaze(int x, int y, string pos, int path)
        {
            visited.Add((x, y));
            maze[x, y].path = path;
            render.Enqueue((x, y));
            while (true)
            {
                var directions = new List<int>() {
                    visited.Contains((x, y - 1)) || y - 1 < 1 ? - 1 : 3,
                    visited.Contains((x, y + 1)) || (y + 1 > width - 2) ? -1 : 1,
                    visited.Contains((x - 1, y)) || x - 1 < 1 ? - 1 : 0,
                    visited.Contains((x + 1, y)) || x + 1 > heigth - 2 ? -1 : 2,

                };
                maze[x, y].SetWall(pos, 1);
                if (x == heigth - 2 && y == width / 2)
                {
                    maze[x, y].SetWall("s", 1);
                }
                if (directions.All(d => d == -1))
                {
                    return;
                }
                string newPos = GetDirection(x, y, directions);
                maze[x, y].SetWall(newPos, 1);
                if (newPos == "w")
                {
                    FillMaze(x - 1, y, "s", path + 1);
                }
                if (newPos == "a")
                {
                    FillMaze(x, y - 1, "d", path + 1);
                }
                if (newPos == "s")
                {
                    FillMaze(x + 1, y, "w", path + 1);
                }
                if (newPos == "d")
                {
                    FillMaze(x, y + 1, "a", path + 1);
                }
            }
        }
        private string GetDirection(int x, int y, List<int> pos)
        {
            while (true)
            {
                int direction = pos[rnd.Next(pos.Count)];
                if (direction == 0)
                {
                    return "w";
                }
                if (direction == 1)
                {
                    return "d";
                }
                if (direction == 2)
                {
                    return "s";
                }
                if (direction == 3)
                {
                    return "a";
                }
            }
        }
        public void PrintMazeSlow()
        {
            Console.BackgroundColor = ConsoleColor.DarkGray;
            //path.Push((heigth - 1, width / 2));
            SetPathMap();
            //path.Push((0, width / 2));
            var sw = new Stopwatch();
            while (render.Count != 0)
            {
                (int y, int x) = render.Dequeue();
                Console.SetCursorPosition(x, y);
                Console.Write(maze[y, x].GetCellValue());
                sw.Start();
                while (sw.ElapsedMilliseconds < drawSpeed)
                {

                }
                sw.Restart();
            }
            Console.BackgroundColor = ConsoleColor.Black;
        }
        public void PrintMazeInstant()
        {
            //path.Push((heigth - 1, width / 2));
            SetPathMap();
            //path.Push((0, width / 2));
            while (render.Count != 0)
            {
                (int y, int x) = render.Dequeue();
                Console.SetCursorPosition(x, y);
                Console.Write(maze[y, x].GetCellValue());
            }
        }
        public void SolveMaze()
        {
            var max = path.Count;
            while (path.Count != 0)
            {
                var rgb = Rainbow(((float)path.Count / (float)max) * 2);
                Console.Write($"\x1b[38;2;{rgb.R};{rgb.G};{rgb.B}m");
                (int y, int x) = path.Pop();
                Console.SetCursorPosition(x, y);
                Console.Write(maze[y, x].GetCellValue());
                Thread.Sleep(solveSpeed);
            }
            Console.ForegroundColor = ConsoleColor.Black;
        }
        private void SetPathMap()
        {
            int x = width / 2;
            int y = heigth - 2;
            while (true)
            {
                path.Push((y, x));
                try
                {
                    if (maze[y - 1, x].path == maze[y, x].path - 1)
                    {
                        y--;
                        continue;
                    }
                }
                catch { }
                try
                {
                    if (maze[y + 1, x].path == maze[y, x].path - 1)
                    {
                        y++;
                        continue;
                    }
                }
                catch { }
                try
                {
                    if (maze[y, x - 1].path == maze[y, x].path - 1)
                    {
                        x--;
                        continue;
                    }
                }
                catch { }
                try
                {
                    if (maze[y, x + 1].path == maze[y, x].path - 1)
                    {
                        x++;
                        continue;
                    }
                }
                catch { }
                if (maze[y, x].path == 0)
                {
                    break;
                }
            }
        }
        private static Color Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 6);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;

            switch ((int)div)
            {
                case 0:
                    return new Color(255, ascending, 0);
                case 1:
                    return new Color(descending, 255, 0);
                case 2:
                    return new Color(0, 255, ascending);
                case 3:
                    return new Color(0, descending, 255);
                case 4:
                    return new Color(ascending, 0, 255);
                default: // case 5:
                    return new Color(255, 0, descending);
            }
        }
        public class Color
        {
            public int R;
            public int G;
            public int B;
            public Color(int r, int g, int b)
            {
                this.R = r;
                this.G = g;
                this.B = b;
            }
        }
    }
}
