using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class MatchSummaryManager
    {
        private readonly Board _board;

        public MatchSummaryManager(Board board)
        {
            _board = board;
        }

        public void ShowMatchSummary(string winner)
        {
            Console.WriteLine("\n=== Match Summary ===");
            Console.WriteLine($"Winner: {winner}");
            Console.WriteLine($"Total Turns: {_board.TurnHistory.Count}");

            int playerPieces = _board.Pieces.Count(p => p.Team == "Player" && p.Health > 0);
            int aiPieces = _board.Pieces.Count(p => p.Team == "AI" && p.Health > 0);

            int playerDamage = _board.Pieces.Where(p => p.Team == "Player").Sum(p => p.TotalDamageDealt);
            int aiDamage = _board.Pieces.Where(p => p.Team == "AI").Sum(p => p.TotalDamageDealt);

            Console.WriteLine($"Player Pieces Remaining: {playerPieces}");
            Console.WriteLine($"AI Pieces Remaining: {aiPieces}");
            Console.WriteLine($"Total Player Damage Dealt: {playerDamage}");
            Console.WriteLine($"Total AI Damage Dealt: {aiDamage}");

            var mvp = _board.Pieces.OrderByDescending(p => p.TotalDamageDealt).FirstOrDefault();
            if (mvp != null)
            {
                Console.WriteLine($"Most Valuable Piece: {mvp.Team} {mvp.Id} - {mvp.TotalDamageDealt} Damage - {mvp.TotalKills} Kills");
            }

            Console.WriteLine("\n=== Activated Synergies ===");
            var packCounts = _board.Pieces
                .Where(p => !string.IsNullOrEmpty(p.Pack))
                .GroupBy(p => p.Pack)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var pack in packCounts)
            {
                if (pack.Value >= 3 && MonsterDatabase.PackBonuses.ContainsKey(pack.Key))
                {
                    Console.WriteLine($"- {pack.Key}: {MonsterDatabase.PackBonuses[pack.Key]}");
                }
            }

            ShowDetailedEndOfGameReport(winner);
        }

        public void ShowDetailedEndOfGameReport(string winner)
        {
            Console.WriteLine("\n=== Detailed End-of-Game Report ===");
            Console.WriteLine($"Winner: {winner}");
            Console.WriteLine($"Total Turns: {_board.TurnHistory.Count}");

            Console.WriteLine("\nPiece Performance:");
            foreach (var piece in _board.Pieces)
            {
                string status = piece.Health > 0 ? "Survived" : "Eliminated";
                Console.WriteLine($"- {piece.Team} {piece.Id}: {piece.TotalKills} Kills, {piece.TotalDamageDealt} Damage Dealt ({status})");
            }

            Console.WriteLine("\nAbilities Used:");
            var abilityUsage = _board.TurnHistory
                .Where(entry => entry.Contains("used"))
                .GroupBy(entry => entry.Split(' ')[1] + " " + entry.Split(' ')[2])
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var ability in abilityUsage)
            {
                Console.WriteLine($"- {ability.Key} used abilities {ability.Value} times");
            }

            Console.WriteLine("\nEvolution Log:");
            var evolutionLog = _board.TurnHistory.Where(entry => entry.Contains("evolved into")).ToList();
            if (evolutionLog.Any())
            {
                foreach (var log in evolutionLog)
                {
                    Console.WriteLine($"- {log}");
                }
            }
            else
            {
                Console.WriteLine("No evolutions occurred.");
            }

            Console.WriteLine("\nComplete Turn History:");
            for (int i = 0; i < _board.TurnHistory.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {_board.TurnHistory[i]}");
            }

            Console.WriteLine("==================================");
        }
    }
}
