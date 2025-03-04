using System;

namespace ChessMonsterTactics
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Type 'test' to start the Chess Monster Tactics Tester.");
            if (Console.ReadLine()?.Trim().ToLower() == "test")
            {
                GameManager gameManager = new GameManager();
                gameManager.SetupGame();
            }
        }
    }
}
