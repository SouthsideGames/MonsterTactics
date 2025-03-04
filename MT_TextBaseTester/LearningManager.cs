using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ChessMonsterTactics
{
    public static class LearningManager
    {
        private const string FilePath = "ai_memory.json";
        private const string HallOfFameFile = "hall_of_fame.json";  // New file for piece stats

        private class LearningData
        {
            public int PlayerWins { get; set; } = 0;
            public int AIWins { get; set; } = 0;
        }

        private static LearningData data;

        private static Dictionary<string, PieceStats> hallOfFame;

        public static void Initialize()
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                data = JsonSerializer.Deserialize<LearningData>(json);
            }
            else
            {
                data = new LearningData();
            }

            hallOfFame = LoadHallOfFame();  // Load piece stats
        }

        public static void ShowCurrentStats()
        {
            Console.WriteLine("=== Current Performance Stats ===");
            Console.WriteLine($"Player Wins: {data.PlayerWins}");
            Console.WriteLine($"AI Wins: {data.AIWins}");
            Console.WriteLine("=================================");
        }

        public static void RecordMatchResult(string winner, List<Piece> finalPieces)
        {
            if (winner.Contains("Player"))
            {
                data.PlayerWins++;
            }
            else if (winner.Contains("AI"))
            {
                data.AIWins++;
            }

            SaveData();
            SaveHallOfFame(finalPieces);  // Track individual piece stats after each match
        }

        private static void SaveData()
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        // --- New Section for Hall of Fame ---

        public static void SaveHallOfFame(List<Piece> finalPieces)
        {
            foreach (var piece in finalPieces)
            {
                if (!hallOfFame.ContainsKey(piece.Id))
                {
                    hallOfFame[piece.Id] = new PieceStats();
                }

                hallOfFame[piece.Id].TotalKills += piece.TotalKills;
                hallOfFame[piece.Id].TotalDamageDealt += piece.TotalDamageDealt;
                if (piece.Health > 0) hallOfFame[piece.Id].TotalWins++;
            }

            File.WriteAllText(HallOfFameFile, JsonSerializer.Serialize(hallOfFame, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static Dictionary<string, PieceStats> LoadHallOfFame()
        {
            if (File.Exists(HallOfFameFile))
            {
                string json = File.ReadAllText(HallOfFameFile);
                return JsonSerializer.Deserialize<Dictionary<string, PieceStats>>(json);
            }
            return new Dictionary<string, PieceStats>();
        }
    }

    public class PieceStats
    {
        public int TotalWins { get; set; }
        public int TotalKills { get; set; }
        public int TotalDamageDealt { get; set; }
    }
}
