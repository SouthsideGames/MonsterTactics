using System;
using System.Linq;
using System.Collections.Generic;

namespace ChessMonsterTactics
{
    public class GameManager
    {
        private int difficulty;  // Easy, Medium, Hard
        private bool debugMode = false;
        private int currentTurnNumber = 1;
        private static readonly Random random = new();

        public void SetupGame()
        {
            LearningManager.Initialize();
            LearningManager.ShowCurrentStats();

            Console.WriteLine("\nWelcome to Chess Monster Tactics!");
            Console.WriteLine("1 - Player vs AI");
            Console.WriteLine("2 - AI vs AI");
            Console.WriteLine("3 - How to Play");
            Console.WriteLine("4 - Piece Info");
            Console.WriteLine("5 - Search for Piece");
            Console.WriteLine("Type 'quit' at any time to exit.");

            string choice = Console.ReadLine()?.Trim();
            if (choice?.ToLower() == "quit") Environment.Exit(0);

            if (choice == "3")
            {
                ShowHowToPlay();
                return;
            }

            if (choice == "4")
            {
                PieceInfoManager.ShowPieceInfo();
                return;
            }

            if (choice == "5")
            {
                SearchForPiece();
                return;
            }

            bool aiVsAi = choice == "2";

            Console.WriteLine("\nSelect Difficulty:");
            Console.WriteLine("1 - Easy");
            Console.WriteLine("2 - Medium");
            Console.WriteLine("3 - Hard");

            string difficultyInput = Console.ReadLine()?.Trim();
            if (difficultyInput?.ToLower() == "quit") Environment.Exit(0);

            difficulty = aiVsAi ? 2 : int.TryParse(difficultyInput, out int result) ? result : 1;

            List<string> allPacks = new() { "Fire Pack", "Cyber Pack", "Shadow Pack", "Arcane Echoes", "Blazing Rebellion" };

            string playerPack = SelectPack("Player", allPacks);
            var playerTeam = MonsterDatabase.GenerateBalancedStarterTeam("Player", playerPack);

            string aiPack = SelectPack(aiVsAi ? "AI 1" : "AI", allPacks);
            var aiTeam = MonsterDatabase.GenerateBalancedStarterTeam("AI", aiPack);

            Board board = new();
            board.Pieces.AddRange(playerTeam);
            board.Pieces.AddRange(aiTeam);

            board.ShowCompleteTeamPreview();

            if (aiVsAi)
            {
                RunAiVsAi(board, playerPack, aiPack);
            }
            else
            {
                RunPlayerVsAi(board);
            }

            if (!aiVsAi)
            {
                Console.WriteLine("Enable Debug Mode? (y/n)");
                debugMode = Console.ReadLine()?.Trim().ToLower() == "y";
            }
        }

        private string SelectPack(string playerType, List<string> packs)
        {
            Console.WriteLine($"\n{playerType}, select your pack:");
            for (int i = 0; i < packs.Count; i++)
            {
                Console.WriteLine($"{i + 1} - {packs[i]}");
            }
            int choice = int.Parse(Console.ReadLine() ?? "1") - 1;
            return packs[Math.Clamp(choice, 0, packs.Count - 1)];
        }

        void ShowHowToPlay()
        {
            Console.WriteLine("\n==== How to Play Chess Monster Tactics ====");
            Console.WriteLine("- Chess-like movement, but with health and abilities.");
            Console.WriteLine("- Use 'quit' anytime to exit.");
            Console.WriteLine("- Type 'scan' to view both teams.");
            Console.WriteLine("- Abilities & synergies give unique bonuses.");
        }

        void SearchForPiece()
        {
            Console.WriteLine("\nEnter piece name or type:");
            string query = Console.ReadLine()?.Trim();
            if (query?.ToLower() == "quit") Environment.Exit(0);

            var results = MonsterDatabase.PieceTemplates
                .Where(kvp => kvp.Key.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                              kvp.Value.Type.Equals(query, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Value)
                .ToList();

            if (!results.Any()) Console.WriteLine("No matching pieces found.");
            else results.ForEach(p => ShowPieceDetails(p));
        }

        void ShowPieceDetails(Piece piece)
        {
            Console.WriteLine($"{piece.Id} ({piece.Type}) - Pack: {piece.Pack}, HP: {piece.Health}, ATK: {piece.Attack}");
        }

        void RunAiVsAi(Board board, string playerPack, string aiPack)
        {
            string winner = "";
            AIController ai1 = new();
            AIController ai2 = new();

            var ai1Personality = MonsterDatabase.PackPersonalities[playerPack];
            var ai2Personality = MonsterDatabase.PackPersonalities[aiPack];

            while (true)
            {
                ai1.TakeTurn(board, "Player", 2, ai1Personality);
                if (CheckEndGame(board, out winner)) break;

                ai2.TakeTurn(board, "AI", 2, ai2Personality);
                if (CheckEndGame(board, out winner)) break;
            }
            ShowMatchSummary(board, winner);
        }

        void RunPlayerVsAi(Board board)
        {
            string winner = "";
            PlayerController player = new();
            AIController ai = new();

            while (true)
            {
                player.TakeTurn(board);
                if (debugMode) board.SaveSnapshot(currentTurnNumber++);

                if (CheckEndGame(board, out winner)) break;

                ai.TakeTurn(board, "AI", difficulty);
                if (debugMode) board.SaveSnapshot(currentTurnNumber++);

                if (CheckEndGame(board, out winner)) break;
            }
            ShowMatchSummary(board, winner);
        }

        bool CheckEndGame(Board board, out string winner)
        {
            return board.CheckWinCondition(out winner);
        }

        void ShowMatchSummary(Board board, string winner)
        {
            Console.WriteLine($"Winner: {winner}\n");
            board.ShowDetailedEndOfGameReport(winner);
        }
    }
}
