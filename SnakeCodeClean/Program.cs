using System;
using System.Collections.Generic;
using System.Threading;

namespace SnakeGame
{
    class Program
    {
        static void Main(string[] args)
        {
            // Nastavenie konzolového okna, bufferu a pozície
            const int windowWidth = 60;
            const int windowHeight = 30;
            Console.WindowWidth = windowWidth;
            Console.WindowHeight = windowHeight;
            // Buffer nastavíme o niečo širší, ak je potrebné
            Console.SetBufferSize(windowWidth, windowHeight);
            Console.SetWindowPosition(0, 0);

            Game hra = new Game();
            hra.Run();
        }
    }

    class Game
    {
        private const int WindowHeight = 30;
        private const int WindowWidth = 60;
        // Každá bunka je 2 znaky široká – hracie pole má cellWidth = WindowWidth/2
        private int cellWidth;
        private int cellHeight;

        private Snake snake;
        private Berry berry;
        private bool isGameOver;
        private string currentDirection;
        private Random random;
        private Renderer renderer;
        private InputHandler inputHandler;

        public Game()
        {
            cellWidth = Console.WindowWidth / 2; // Napr. 60/2 = 30 buniek
            cellHeight = Console.WindowHeight;     // 30 riadkov

            random = new Random();
            int startX = cellWidth / 2;
            int startY = cellHeight / 2;
            snake = new Snake(startX, startY, ConsoleColor.Red);
            currentDirection = "RIGHT";
            berry = new Berry(random.Next(1, cellWidth - 1), random.Next(1, cellHeight - 1));
            isGameOver = false;
            renderer = new Renderer(cellWidth, cellHeight);
            inputHandler = new InputHandler();
        }

        public void Run()
        {
            while (!isGameOver)
            {
                Console.Clear();

                // Kontrola kolízie s okrajmi hracieho poľa
                if (snake.Head.X == cellWidth - 1 || snake.Head.X == 0 ||
                    snake.Head.Y == cellHeight - 1 || snake.Head.Y == 0)
                {
                    isGameOver = true;
                }

                renderer.Render(snake, berry, cellWidth, cellHeight);

                // Kontrola, či had zjedol bobuľu
                if (snake.Head.X == berry.X && snake.Head.Y == berry.Y)
                {
                    snake.Grow();
                    berry.Respawn(random, cellWidth, cellHeight);
                }

                // Kontrola kolízie s vlastným telom
                if (snake.IsSelfCollision())
                {
                    isGameOver = true;
                }

                if (isGameOver)
                    break;

                // Časovanie a spracovanie vstupu – oneskorenie 100 ms
                DateTime startTime = DateTime.Now;
                bool buttonPressed = false;
                while ((DateTime.Now - startTime).TotalMilliseconds < 100)
                {
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                        string newDir = inputHandler.GetDirection(keyInfo.Key, currentDirection);
                        if (!string.IsNullOrEmpty(newDir) && !buttonPressed)
                        {
                            currentDirection = newDir;
                            buttonPressed = true;
                        }
                    }
                }

                // Aktualizácia hada: uloženie aktuálnej pozície hlavy do tela
                snake.AddBodySegment();

                // Pohyb hlavy podľa aktuálneho smeru
                switch (currentDirection)
                {
                    case "UP":
                        snake.Head.Y--;
                        break;
                    case "DOWN":
                        snake.Head.Y++;
                        break;
                    case "LEFT":
                        snake.Head.X--;
                        break;
                    case "RIGHT":
                        snake.Head.X++;
                        break;
                }

                // Ak had nie je rastúc, odstráň starý chvost
                snake.Trim();
            }

            // Game over – vyčistenie obrazovky a zobrazenie hlásenia
            Console.Clear();
            Console.SetCursorPosition(cellWidth, cellHeight / 2);
            Console.WriteLine("Game over");
            Console.ReadKey();
        }
    }

    class Snake
    {
        public SnakeSegment Head { get; set; }
        public List<SnakeSegment> Body { get; set; }

        public Snake(int x, int y, ConsoleColor color)
        {
            Head = new SnakeSegment(x, y, color);
            Body = new List<SnakeSegment>();
        }

        public void AddBodySegment()
        {
            Body.Add(new SnakeSegment(Head.X, Head.Y, Head.Color));
        }

        public void Grow()
        {
            // Pri zjedení bobule sa chvost neodstraňuje – skip Trim
        }

        public void Trim()
        {
            // Nastavíme pevný počet segmentov (napr. 5) pre základnú dĺžku hada
            if (Body.Count > 5)
                Body.RemoveAt(0);
        }

        public bool IsSelfCollision()
        {
            foreach (var segment in Body)
            {
                if (segment.X == Head.X && segment.Y == Head.Y)
                    return true;
            }
            return false;
        }
    }

    class SnakeSegment
    {
        public int X { get; set; }
        public int Y { get; set; }
        public ConsoleColor Color { get; set; }

        public SnakeSegment(int x, int y, ConsoleColor color)
        {
            X = x;
            Y = y;
            Color = color;
        }
    }

    class Berry
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Berry(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void Respawn(Random random, int cellWidth, int cellHeight)
        {
            X = random.Next(1, cellWidth - 1);
            Y = random.Next(1, cellHeight - 1);
        }
    }

    class Renderer
    {
        private int cellWidth;
        private int cellHeight;

        public Renderer(int cellWidth, int cellHeight)
        {
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
        }

        // Vykreslí okraje, hada a bobuľu
        public void Render(Snake snake, Berry berry, int boardWidth, int boardHeight)
        {
            DrawBorders(boardWidth, boardHeight);
            DrawSnake(snake);
            DrawBerry(berry);
        }

        void DrawBorders(int boardWidth, int boardHeight)
        {
            // Horný okraj
            for (int i = 0; i < boardWidth; i++)
            {
                Console.SetCursorPosition(i * 2, 0);
                Console.Write("██");
            }
            // Spodný okraj
            for (int i = 0; i < boardWidth; i++)
            {
                Console.SetCursorPosition(i * 2, boardHeight - 1);
                Console.Write("██");
            }
            // Ľavý okraj
            for (int i = 0; i < boardHeight; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write("██");
            }
            // Pravý okraj
            for (int i = 0; i < boardHeight; i++)
            {
                Console.SetCursorPosition((boardWidth - 1) * 2, i);
                Console.Write("██");
            }
        }

        void DrawSnake(Snake snake)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var segment in snake.Body)
            {
                Console.SetCursorPosition(segment.X * 2, segment.Y);
                Console.Write("██");
            }
            Console.ForegroundColor = snake.Head.Color;
            Console.SetCursorPosition(snake.Head.X * 2, snake.Head.Y);
            Console.Write("██");
        }

        void DrawBerry(Berry berry)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition(berry.X * 2, berry.Y);
            Console.Write("██");
        }
    }

    class InputHandler
    {
        public string GetDirection(ConsoleKey key, string currentDirection)
        {
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    if (currentDirection != "DOWN") return "UP";
                    break;
                case ConsoleKey.DownArrow:
                    if (currentDirection != "UP") return "DOWN";
                    break;
                case ConsoleKey.LeftArrow:
                    if (currentDirection != "RIGHT") return "LEFT";
                    break;
                case ConsoleKey.RightArrow:
                    if (currentDirection != "LEFT") return "RIGHT";
                    break;
            }
            return currentDirection;
        }
    }
}

