using System;
using System.Collections.Generic;
using System.Threading;

namespace SnakeGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Game hra = new Game();
            hra.Run();
        }
    }

    class Game
    {
        private const int WindowHeight = 30;
        private const int WindowWidth = 60;
        private int cellWidth;
        private int cellHeight;

        private Snake snake;
        private Berry berry;
        private bool isGameOver;
        private int score;
        private string currentDirection;
        private Random random;
        private Renderer renderer;
        private InputHandler inputHandler;

        public Game()
        {
            // Nastavenie konzolového okna
            Console.WindowHeight = WindowHeight;
            Console.WindowWidth = WindowWidth;
            cellWidth = Console.WindowWidth / 2;   // počet buniek horizontálne
            cellHeight = Console.WindowHeight;       // počet buniek vertikálne

            random = new Random();
            int startX = cellWidth / 2;
            int startY = cellHeight / 2;
            snake = new Snake(startX, startY, ConsoleColor.Red);
            currentDirection = "RIGHT";
            berry = new Berry(random.Next(1, cellWidth - 1), random.Next(1, cellHeight - 1));
            score = 5;
            isGameOver = false;
            renderer = new Renderer(cellWidth, cellHeight);
            inputHandler = new InputHandler();
        }

        public void Run()
        {
            while (!isGameOver)
            {
                Console.Clear();

                // Kontrola kolízie so stenami
                if (snake.Head.X == cellWidth - 1 || snake.Head.X == 0 ||
                    snake.Head.Y == cellHeight - 1 || snake.Head.Y == 0)
                {
                    isGameOver = true;
                }

                renderer.Render(snake, berry, score, cellWidth, cellHeight);

                // Kontrola, či had zjedol bobuľu
                if (snake.Head.X == berry.X && snake.Head.Y == berry.Y)
                {
                    score++;
                    snake.Grow();
                    berry.Respawn(random, cellWidth, cellHeight);
                }

                if (snake.IsSelfCollision())
                {
                    isGameOver = true;
                }

                if (isGameOver)
                    break;

                // Časovanie a spracovanie vstupu (100 ms)
                DateTime startTime = DateTime.Now;
                bool buttonPressed = false;
                while (true)
                {
                    DateTime currentTime = DateTime.Now;
                    if (currentTime.Subtract(startTime).TotalMilliseconds > 100)
                        break;
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

                // Aktualizácia hada
                snake.AddBodySegment(); // uloženie aktuálnej pozície hlavy do tela

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

                // Odstránenie chvosta, ak had neporastie
                snake.Trim(score);
            }

            Console.Clear();
            Console.SetCursorPosition(cellWidth, cellHeight / 2);
            Console.WriteLine("Game over, Score: " + score);
            Console.SetCursorPosition(cellWidth, cellHeight / 2 + 1);
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
            // Pri zjedení bobule sa chvost neodstraňuje
        }

        public void Trim(int desiredLength)
        {
            if (Body.Count > desiredLength)
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
        private const int offsetX = 2; // Pridaný horizontálny posun (offset) o 2 znaky
        private const int offsetY = 0; // Vertikálny offset, ak je potrebný

        public Renderer(int cellWidth, int cellHeight)
        {
            this.cellWidth = cellWidth;
            this.cellHeight = cellHeight;
        }

        public void Render(Snake snake, Berry berry, int score, int boardWidth, int boardHeight)
        {
            DrawBorders(boardWidth, boardHeight);
            DrawSnake(snake);
            DrawBerry(berry);
            DrawScore(score, boardWidth);
        }

        void DrawBorders(int boardWidth, int boardHeight)
        {
            // Horný okraj
            for (int i = 0; i < boardWidth; i++)
            {
                Console.SetCursorPosition(offsetX + i * 2, offsetY);
                Console.Write("██");
            }
            // Spodný okraj
            for (int i = 0; i < boardWidth; i++)
            {
                Console.SetCursorPosition(offsetX + i * 2, boardHeight - 1 + offsetY);
                Console.Write("██");
            }
            // Ľavý okraj
            for (int i = 0; i < boardHeight; i++)
            {
                Console.SetCursorPosition(offsetX, i + offsetY);
                Console.Write("██");
            }
            // Pravý okraj
            for (int i = 0; i < boardHeight; i++)
            {
                Console.SetCursorPosition(offsetX + (boardWidth - 1) * 2, i + offsetY);
                Console.Write("██");
            }
        }

        void DrawSnake(Snake snake)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (var segment in snake.Body)
            {
                Console.SetCursorPosition(offsetX + segment.X * 2, segment.Y + offsetY);
                Console.Write("██");
            }
            Console.ForegroundColor = snake.Head.Color;
            Console.SetCursorPosition(offsetX + snake.Head.X * 2, snake.Head.Y + offsetY);
            Console.Write("██");
        }

        void DrawBerry(Berry berry)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition(offsetX + berry.X * 2, berry.Y + offsetY);
            Console.Write("██");
        }

        void DrawScore(int score, int boardWidth)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(offsetX + boardWidth * 2 + 2, 1 + offsetY);
            Console.Write("Score: " + score);
        }
    }

    class InputHandler
    {
        public string GetDirection(ConsoleKey key, string currentDirection)
        {
            switch (key)
            {
                case ConsoleKey.UpArrow:
                    if (currentDirection != "DOWN")
                        return "UP";
                    break;
                case ConsoleKey.DownArrow:
                    if (currentDirection != "UP")
                        return "DOWN";
                    break;
                case ConsoleKey.LeftArrow:
                    if (currentDirection != "RIGHT")
                        return "LEFT";
                    break;
                case ConsoleKey.RightArrow:
                    if (currentDirection != "LEFT")
                        return "RIGHT";
                    break;
            }
            return currentDirection;
        }
    }
}
