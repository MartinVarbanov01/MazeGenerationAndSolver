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
            int x = 29, y = 120;
            //int x = 20, y = 80;
            Maze maze = new Maze(x, y);
            maze.FillMaze(1, y / 2, "w", 0);
            maze.PrintMaze();
            Console.ReadLine();
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
        private Cell[,] maze;
        Random rnd = new Random();
        HashSet<(int, int)> visited = new HashSet<(int, int)>();
        Stack<(int, int)> path = new Stack<(int, int)>();
        Queue<(int, int)> render = new Queue<(int, int)>();

        public Maze(int heigth, int width)
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
                    visited.Contains((x + 1, y)) || x + 1 > heigth - 2 ? -1 : 2
                };
                maze[x, y].SetWall(pos, 1);
                if (x == heigth - 2 && y == width / 2)
                {
                    maze[x, y].SetWall("s", 1);
                }
                if (directions.All(d => d == -1))
                {
                    //Console.SetCursorPosition(y, x);
                    //Console.Write(maze[x, y].GetCellValue());
                    //Thread.Sleep(1);
                    return;
                }
                string newPos = GetDirection(x, y, directions);
                maze[x, y].SetWall(newPos, 1);
                //Console.SetCursorPosition(x, y);
                //Console.Write(maze[y, x].GetCellValue());
                //Thread.Sleep(1);
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
                int direction = pos[rnd.Next(4)];
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
        public void PrintMaze()
        {
            path.Push((heigth - 1, width / 2));
            SetPathMap();
            path.Push((0, width / 2));
            long targetFrameTime = 2;
            var sw = new Stopwatch();
            while (render.Count != 0)
            {
                (int y, int x) = render.Dequeue();
                Console.SetCursorPosition(x, y);
                Console.Write(maze[y, x].GetCellValue());
                sw.Start();
                while(sw.ElapsedMilliseconds < targetFrameTime)
                {

                }
                sw.Restart();
            }
            Console.ReadKey();
            while (path.Count != 0)
            {
                (int y, int x) = path.Pop();
                Console.SetCursorPosition(x, y);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(maze[y, x].GetCellValue());
                Thread.Sleep(10);
            }
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
    }
}
