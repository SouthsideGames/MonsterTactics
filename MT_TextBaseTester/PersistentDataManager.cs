using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ChessMonsterTactics
{
    public static class PersistentDataManager
    {
        private const string HallOfFameFile = "hall_of_fame.json";
        private const string AIPersonalitiesFile = "ai_personalities.json";

        // Hall of Fame data structure
        public class PieceStats
        {
            public int TotalKills { get; set; }
            public int TotalDamageDealt { get; set; }
            public int TotalWins { get; set; }
        }

        // AI Personality data structure
        public class AIPersonalityStats
        {
            public int Wins { get; set; }
            public int Losses { get; set; }
            public string PersonalityType { get; set; }  // Aggressive, Defensive, SynergyHunter
        }

        public static Dictionary<string, PieceStats> LoadHallOfFame()
        {
            if (File.Exists(HallOfFameFile))
            {
                string json = File.ReadAllText(HallOfFameFile);
                return JsonSerializer.Deserialize<Dictionary<string, PieceStats>>(json) ?? new();
            }
            return new();
        }

        public static void SaveHallOfFame(Dictionary<string, PieceStats> hallOfFame)
        {
            string json = JsonSerializer.Serialize(hallOfFame, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(HallOfFameFile, json);
        }

        public static void UpdateHallOfFame(List<Piece> finalPieces)
        {
            var hallOfFame = LoadHallOfFame();

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

            SaveHallOfFame(hallOfFame);
        }

        public static Dictionary<string, AIPersonalityStats> LoadAIPersonalities()
        {
            if (File.Exists(AIPersonalitiesFile))
            {
                string json = File.ReadAllText(AIPersonalitiesFile);
                return JsonSerializer.Deserialize<Dictionary<string, AIPersonalityStats>>(json) ?? new();
            }
            return new();
        }

        public static void SaveAIPersonalities(Dictionary<string, AIPersonalityStats> aiPersonalities)
        {
            string json = JsonSerializer.Serialize(aiPersonalities, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(AIPersonalitiesFile, json);
        }

        public static void UpdateAIPersonality(string aiName, bool isWin)
        {
            var personalities = LoadAIPersonalities();

            if (!personalities.ContainsKey(aiName))
            {
                personalities[aiName] = new AIPersonalityStats
                {
                    PersonalityType = "Unknown"
                };
            }

            if (isWin)
            {
                personalities[aiName].Wins++;
            }
            else
            {
                personalities[aiName].Losses++;
            }

            SaveAIPersonalities(personalities);
        }
    }
}
