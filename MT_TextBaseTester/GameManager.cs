using System;
using System.Linq;
using System.Collections.Generic;

namespace ChessMonsterTactics
{
    public class GameManager
    {
        private int difficulty;  // Easy, Medium, Hard
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

            if (choice?.ToLower() == "quit")
                Environment.Exit(0);

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

            // Ask for difficulty
            Console.WriteLine("\nSelect Difficulty:");
            Console.WriteLine("1 - Easy");
            Console.WriteLine("2 - Medium");
            Console.WriteLine("3 - Hard");

            string difficultyInput = Console.ReadLine()?.Trim();
            if (difficultyInput?.ToLower() == "quit") Environment.Exit(0);

            if (!int.TryParse(difficultyInput, out difficulty) || difficulty < 1 || difficulty > 3)
            {
                difficulty = 1;  // Default to Easy
            }

            // Ask player to select a pack
            Console.WriteLine("\nSelect Your Pack:");
            List<string> allPacks = new() { "Starter Pack", "Fire Pack", "Cyber Pack", "Shadow Pack" };

            for (int i = 0; i < allPacks.Count; i++)
            {
                Console.WriteLine($"{i + 1} - {allPacks[i]}");
            }

            string packChoice = Console.ReadLine()?.Trim();
            if (packChoice?.ToLower() == "quit") Environment.Exit(0);

            int packIndex = int.TryParse(packChoice, out int result) ? result - 1 : 0;
            string playerPack = allPacks[Math.Clamp(packIndex, 0, allPacks.Count - 1)];

            // Player team from selected pack
            var playerTeam = MonsterDatabase.GenerateBalancedStarterTeam("Player", playerPack);

            // AI team uses a **random pack**
            string aiPack = allPacks[random.Next(allPacks.Count)];
            var aiTeam = MonsterDatabase.GenerateBalancedStarterTeam("AI", aiPack);

            Board board = new Board();
            board.Pieces.AddRange(playerTeam);
            board.Pieces.AddRange(aiTeam);

            board.ShowTeamPreview();

            if (aiVsAi)
            {
                RunAiVsAi(board);
            }
            else
            {
                RunPlayerVsAi(board);
            }
        }

        void ShowHowToPlay()
        {
            Console.WriteLine("\n==== How to Play Chess Monster Tactics ====");
            Console.WriteLine("- The game follows chess-like movement, but with health and special abilities.");
            Console.WriteLine("- Type 'quit' anytime to exit.");
            Console.WriteLine("- To move a piece, type its name and location like this: 'Pawn at A2'");
            Console.WriteLine("- After selecting a piece, enter where you want to move it: 'A3'");
            Console.WriteLine("- Abilities can be used instead of moving.");
            Console.WriteLine("- Synergies trigger bonuses when you have 3 or more pieces from the same pack.");
            Console.WriteLine("============================================\n");
        }

        void SearchForPiece()
        {
            Console.WriteLine("\nEnter piece name (e.g., SparkPawn1) or type (e.g., Knight) to search:");
            string searchQuery = Console.ReadLine()?.Trim();

            if (searchQuery?.ToLower() == "quit") Environment.Exit(0);

            var matchingPieces = MonsterDatabase.PieceTemplates
                .Where(kvp => kvp.Key.Equals(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                              kvp.Value.Type.Equals(searchQuery, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Value)
                .ToList();

            if (matchingPieces.Count == 0)
            {
                Console.WriteLine("No matching pieces found.");
                return;
            }

            Console.WriteLine($"\nFound {matchingPieces.Count} matching piece(s):");
            foreach (var piece in matchingPieces)
            {
                ShowPieceDetails(piece);
            }
        }

        void ShowPieceDetails(Piece piece)
        {
            Console.WriteLine($"\n=== Details for {piece.Id} ===");
            Console.WriteLine($"Type: {piece.Type}");
            Console.WriteLine($"Health: {piece.Health}");
            Console.WriteLine($"Attack: {piece.Attack}");
            Console.WriteLine($"Defense: {piece.Defense}");
            Console.WriteLine($"Speed: {piece.Speed}");
            Console.WriteLine($"Special Ability: {piece.Ability}");
            Console.WriteLine($"Passive Ability: {piece.Passive}");
            Console.WriteLine($"Pack: {piece.Pack}");

            if (MonsterDatabase.EvolutionChains.TryGetValue(piece.Id, out var evolutions))
            {
                Console.WriteLine("Evolution Path:");
                foreach (var evolution in evolutions)
                {
                    Console.WriteLine($"- Level {evolution.Level}: {evolution.EvolvedForm}");
                }
            }

            Console.WriteLine("==================================");
        }

        void RunAiVsAi(Board board)
        {
            string winner = "";
            AIController ai1 = new AIController();
            AIController ai2 = new AIController();

            while (true)
            {
                ai1.TakeTurn(board, "Player", difficulty);
                if (CheckEndGame(board, out winner)) break;

                ai2.TakeTurn(board, "AI", difficulty);
                if (CheckEndGame(board, out winner)) break;
            }

            ShowMatchSummary(board, winner);
        }

        void RunPlayerVsAi(Board board)
        {
            string winner = "";
            PlayerController player = new PlayerController();
            AIController ai = new AIController();

            while (true)
            {
                player.TakeTurn(board);
                if (CheckEndGame(board, out winner)) break;

                ai.TakeTurn(board, "AI", difficulty);
                if (CheckEndGame(board, out winner)) break;
            }

            ShowMatchSummary(board, winner);
        }

        bool CheckEndGame(Board board, out string winner)
        {
            if (board.CheckWinCondition(out winner))
            {
                LearningManager.RecordMatchResult(winner, board.Pieces);
                return true;
            }
            return false;
        }

        private void ShowMatchSummary(Board board, string winner)
        {
            Console.WriteLine("\n=== Match Summary ===");
            Console.WriteLine($"Winner: {winner}");

            int playerPieces = board.Pieces.Count(p => p.Team == "Player" && p.Health > 0);
            int aiPieces = board.Pieces.Count(p => p.Team == "AI" && p.Health > 0);
            int totalPlayerDamage = board.Pieces.Where(p => p.Team == "Player").Sum(p => p.TotalDamageDealt);
            int totalAIDamage = board.Pieces.Where(p => p.Team == "AI").Sum(p => p.TotalDamageDealt);

            Console.WriteLine($"Total Player Damage Dealt: {totalPlayerDamage}");
            Console.WriteLine($"Total AI Damage Dealt: {totalAIDamage}");

            var mvp = board.Pieces.OrderByDescending(p => p.TotalDamageDealt).FirstOrDefault();
            if (mvp != null)
            {
                Console.WriteLine($"Most Valuable Piece: {mvp.Team} {mvp.Id} - {mvp.TotalDamageDealt} Damage - {mvp.TotalKills} Kills");
            }

            board.ShowDetailedEndOfGameReport(winner);
        }
    }
}
