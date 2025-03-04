using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ChessMonsterTactics
{
    public static class LearningManager
    {
        private const string FilePath = "ai_memory.json";

        // Data structure to track player and AI performance.
        private class LearningData
        {
            public int PlayerWins { get; set; } = 0;
            public int AIWins { get; set; } = 0;

            // Future placeholder: Track most used pieces or stats across games.
            public Dictionary<string, int> PieceUsage { get; set; } = new Dictionary<string, int>();
        }

        private static LearningData data;

        // Load data at the start of the game.
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
        }

        // Display current win/loss stats at the start of the game.
        public static void ShowCurrentStats()
        {
            Console.WriteLine("=== Current Performance Stats ===");
            Console.WriteLine($"Player Wins: {data.PlayerWins}");
            Console.WriteLine($"AI Wins: {data.AIWins}");
            Console.WriteLine("=================================");
        }

        // Record match result after a game ends.
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

            TrackPieceUsage(finalPieces);
            SaveData();
        }

        // Track how often each piece was used in completed matches (optional future system).
        private static void TrackPieceUsage(List<Piece> pieces)
        {
            foreach (var piece in pieces)
            {
                if (!data.PieceUsage.ContainsKey(piece.Id))
                {
                    data.PieceUsage[piece.Id] = 0;
                }
                data.PieceUsage[piece.Id]++;
            }
        }

        // Save data to JSON file.
        private static void SaveData()
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
    }
}
