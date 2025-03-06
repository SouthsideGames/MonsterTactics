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

            Board board = new Board();

            if (aiVsAi)
            {
                // AI vs AI - Both sides get randomly generated teams
                board.Pieces.AddRange(MonsterDatabase.GenerateBalancedStarterTeam("Player", GetRandomPack()));
                board.Pieces.AddRange(MonsterDatabase.GenerateBalancedStarterTeam("AI", GetRandomPack()));
            }
            else
            {
                // Player creates team manually
                board.Pieces.AddRange(TeamManager.PlayerCreateTeam("Player"));
                board.Pieces.AddRange(MonsterDatabase.GenerateBalancedStarterTeam("AI", GetRandomPack()));

                Console.WriteLine("Enable Debug Mode? (y/n)");
                debugMode = Console.ReadLine()?.Trim().ToLower() == "y";
            }

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
            Console.WriteLine("- Chess movement, but with health and abilities.");
            Console.WriteLine("- Type 'quit' anytime to exit.");
            Console.WriteLine("- Move example: 'Pawn at A2 to A3'.");
            Console.WriteLine("- Use abilities or attack nearby enemies.");
            Console.WriteLine("- Debug commands (if enabled): 'scan', 'rewind'.");
            Console.WriteLine("============================================\n");
        }

        void SearchForPiece()
        {
            Console.WriteLine("\nEnter piece name or type (e.g., Knight):");
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

            foreach (var piece in matchingPieces)
            {
                ShowPieceDetails(piece);
            }
        }

        void ShowPieceDetails(Piece piece)
        {
            Console.WriteLine($"\n=== {piece.Id} ({piece.Type}) ===");
            Console.WriteLine($"Pack: {piece.Pack}");
            Console.WriteLine($"HP: {piece.Health}, ATK: {piece.Attack}, DEF: {piece.Defense}, SPD: {piece.Speed}");
            Console.WriteLine($"Ability: {piece.Ability}, Passive: {piece.Passive}");

            if (MonsterDatabase.EvolutionChains.TryGetValue(piece.Id, out var evolutions))
            {
                Console.WriteLine("Evolves into:");
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
                if (debugMode)
                {
                    Console.WriteLine("Enter 'scan', 'rewind', or press Enter to continue:");
                    string input = Console.ReadLine()?.Trim();

                    if (input == "rewind")
                    {
                        HandleRewindCommand(board);
                        continue;
                    }
                    else if (input == "scan")
                    {
                        PerformBattlefieldScan(board);
                        continue;
                    }
                }

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

            board.ShowDetailedEndOfGameReport(winner);
        }

        public void PerformBattlefieldScan(Board board)
        {
            Console.WriteLine("\n=== Battlefield Scan ===");
            foreach (var piece in board.Pieces.OrderBy(p => p.Team).ThenBy(p => p.Type))
            {
                string status = piece.Health > 0 ? "Active" : "Eliminated";
                Console.WriteLine($"{piece.Team} {piece.Id} ({piece.Type}) - {piece.Position} - HP: {piece.Health}, Energy: {piece.Energy}, Status: {status}");
            }
            Console.WriteLine("==================================");
        }

        public void HandleRewindCommand(Board board)
        {
            if (!board.Snapshots.Any())
            {
                Console.WriteLine("No turns to rewind to.");
                return;
            }

            Console.WriteLine("Available turns:");
            foreach (var snapshot in board.Snapshots)
            {
                Console.WriteLine($"- Turn {snapshot.TurnNumber}");
            }

            Console.Write("Rewind to turn: ");
            if (int.TryParse(Console.ReadLine(), out int turnNumber))
            {
                var snapshot = board.Snapshots.FirstOrDefault(s => s.TurnNumber == turnNumber);
                if (snapshot != null)
                {
                    board.RestoreSnapshot(snapshot);
                    Console.WriteLine($"Rewound to Turn {turnNumber}");
                }
                else
                {
                    Console.WriteLine("Invalid turn.");
                }
            }
        }

        private string GetRandomPack()
        {
            string[] packs = { "Fire Pack", "Cyber Pack", "Shadow Pack" };
            return packs[random.Next(packs.Length)];
        }
    }
}
