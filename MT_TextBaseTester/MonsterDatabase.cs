using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public static class MonsterDatabase
    {
        private static readonly Random Random = new();

        public static Dictionary<string, int> AbilityCosts = new Dictionary<string, int>
        {
            { "Shock Pulse", 2 },
            { "Blitz Protocol", 4 },
            { "Mind Hack", 5 },
            { "Plasma Barrage", 6 },
            { "Overdrive Command", 5 },
            { "Fire Leap", 3 },
            { "Healing Mist", 3 },
            { "Stonewall", 2 },
            { "Psychic Blast", 5 },
            { "Dark Strike", 2 },
            { "Arcane Pulse", 4 },
            { "Rune Dash", 3 },
            { "Psychic Burst", 5 },
            { "Blazing Command", 5 }
        };

        public static Dictionary<string, string> AbilityDescriptions = new Dictionary<string, string>
        {
            { "Shock Pulse", "Deals 5 damage and reduces target's Speed by 1." },
            { "Blitz Protocol", "Move 2 extra tiles and gain +5 Attack for this turn." },
            { "Mind Hack", "Prevents a nearby enemy from using abilities for 2 turns." },
            { "Plasma Barrage", "Deal 10 damage to up to 3 targets in range." },
            { "Overdrive Command", "All allies gain +2 Speed and +5 Attack for 1 turn." },
            { "Fire Leap", "Leap to a target location and deal 5 damage to adjacent enemies." },
            { "Healing Mist", "Heals all allies within 1 tile for 10 HP." },
            { "Stonewall", "Creates an indestructible wall at a nearby location." },
            { "Psychic Blast", "Deals 8 damage to a target enemy and stuns them for 1 turn." },
            { "Dark Strike", "A shadow-infused strike that deals extra damage to enemies in the dark." },
            { "Arcane Pulse", "Fires a wave that deals 6 damage to all enemies in a line." },
            { "Shadow Step", "Allows the piece to reposition once per turn without using energy." },
            { "Energy Surge", "Gains +1 Energy for each enemy hit by Plasma Barrage." },
            { "Cyber Fortress", "Adjacent allies take 20% less damage." },
            { "System Reboot", "Heals 5 HP after every kill." },
            { "Rune Dash", "Move to a target tile and deal 8 damage to all adjacent enemies after moving." },
            { "Psychic Burst", "Deal 10 damage to an enemy within 2 tiles." },
            { "Blazing Command", "Ignite all tiles within a 1-tile radius, making them Burning for 3 turns." }
        };

        public static Dictionary<string, Piece> PieceTemplates = new()
        {
            // Fire Pack
            { "FirePawn1", new Piece("FirePawn1", "Pawn", "Fire Pack", 30, 8, 3, 4, "Flame Jab", "None") },
            { "FireKnight1", new Piece("FireKnight1", "Knight", "Fire Pack", 55, 14, 5, 6, "Flame Charge", "Blazing Stride") },
            { "FireBishop1", new Piece("FireBishop1", "Bishop", "Fire Pack", 50, 12, 4, 5, "Lava Surge", "Molten Ward") },
            { "FireRook1", new Piece("FireRook1", "Rook", "Fire Pack", 65, 16, 6, 4, "Eruption Shield", "Volcanic Defense") },
            { "FireQueen1", new Piece("FireQueen1", "Queen", "Fire Pack", 60, 18, 5, 5, "Flame Storm", "Inferno Command") },
            { "FireKing1", new Piece("FireKing1", "King", "Fire Pack", 70, 15, 6, 3, "Ember Command", "Fireguard") },

            // Cyber Pack
            { "CyberPawn1", new Piece("CyberPawn1", "Pawn", "Cyber Pack", 30, 7, 3, 4, "EMP Blast", "None") },
            { "CyberKnight1", new Piece("CyberKnight1", "Knight", "Cyber Pack", 55, 13, 5, 6, "Hyper Leap", "Reboot Cycle") },
            { "CyberBishop1", new Piece("CyberBishop1", "Bishop", "Cyber Pack", 50, 12, 4, 5, "Scan Pulse", "None") },
            { "CyberRook1", new Piece("CyberRook1", "Rook", "Cyber Pack", 65, 16, 6, 4, "None", "Adaptive Shielding") },
            { "CyberQueen1", new Piece("CyberQueen1", "Queen", "Cyber Pack", 60, 17, 5, 5, "Electrowave", "Data Surge") },
            { "CyberKing1", new Piece("CyberKing1", "King", "Cyber Pack", 70, 15, 6, 3, "System Override", "Fortress Protocol") },

            // Shadow Pack
            { "ShadowPawn1", new Piece("ShadowPawn1", "Pawn", "Shadow Pack", 30, 9, 3, 5, "Dark Strike", "Shadow Step") },
            { "ShadowKnight1", new Piece("ShadowKnight1", "Knight", "Shadow Pack", 55, 14, 5, 6, "Shadow Strike", "Shadow Step") },
            { "ShadowBishop1", new Piece("ShadowBishop1", "Bishop", "Shadow Pack", 50, 12, 4, 5, "Void Pulse", "Abyssal Aura") },
            { "ShadowRook1", new Piece("ShadowRook1", "Rook", "Shadow Pack", 65, 16, 6, 4, "Ethereal Shift", "Shifting Defense") },
            { "ShadowQueen1", new Piece("ShadowQueen1", "Queen", "Shadow Pack", 60, 18, 5, 5, "Queen’s Curse", "Dark Surge") },
            { "ShadowKing1", new Piece("ShadowKing1", "King", "Shadow Pack", 70, 15, 6, 3, "King’s Veil", "Abyssal Protection") },

            // Arcane Echoes Pack
            { "ArcanePawn1", new Piece("ArcanePawn1", "Pawn", "Arcane Echoes", 30, 5, 3, 5, "None", "None") },
            { "RunebladeKnight1", new Piece("RunebladeKnight1", "Knight", "Arcane Echoes", 55, 12, 5, 8, "Rune Dash", "None") },
            { "WispsoulBishop1", new Piece("WispsoulBishop1", "Bishop", "Arcane Echoes", 45, 8, 4, 6, "None", "Ethereal Link") },
            { "ArcanePillarRook1", new Piece("ArcanePillarRook1", "Rook", "Arcane Echoes", 65, 14, 6, 4, "None", "None") },
            { "ShardmindQueen1", new Piece("ShardmindQueen1", "Queen", "Arcane Echoes", 70, 18, 5, 7, "Psychic Burst", "None") },
            { "EnigmaKing1", new Piece("EnigmaKing1", "King", "Arcane Echoes", 85, 14, 6, 5, "None", "Reality Anchor") },

            // Blazing Rebellion Pack
            { "ScorchlingPawn1", new Piece("ScorchlingPawn1", "Pawn", "Blazing Rebellion", 35, 6, 3, 5, "None", "None") },
            { "InfernoKnight1", new Piece("InfernoKnight1", "Knight", "Blazing Rebellion", 60, 13, 5, 7, "None", "Fire Trail") },
            { "EmberlightBishop1", new Piece("EmberlightBishop1", "Bishop", "Blazing Rebellion", 50, 9, 4, 6, "None", "None") },
            { "MagmaRook1", new Piece("MagmaRook1", "Rook", "Blazing Rebellion", 70, 15, 6, 4, "None", "Molten Core") },
            { "FlameheartQueen1", new Piece("FlameheartQueen1", "Queen", "Blazing Rebellion", 75, 20, 5, 7, "Blazing Command", "None") },
            { "PyreKing1", new Piece("PyreKing1", "King", "Blazing Rebellion", 90, 14, 6, 5, "None", "Rebel Leader") },

        };
        private static Random random = new Random();

        public static List<Piece> GenerateRandomTeam(string team)
        {
            string[] pieceOrder = { "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn",
                                    "Knight", "Knight", "Bishop", "Bishop", "Rook", "Rook", "Queen", "King" };

            string[] positions = { "A2", "B2", "C2", "D2", "E2", "F2", "G2", "H2",
                                   "B1", "G1", "C1", "F1", "A1", "H1", "D1", "E1" };

            List<Piece> teamPieces = new List<Piece>();

            for (int i = 0; i < pieceOrder.Length; i++)
            {
                string type = pieceOrder[i];
                string position = positions[i];

                var availablePieces = PieceTemplates
                    .Where(kvp => kvp.Value.Type == type)
                    .Select(kvp => kvp.Key)
                    .ToList();

                string selectedId = availablePieces[random.Next(availablePieces.Count)];
                Piece template = PieceTemplates[selectedId];

                var piece = new Piece
                {
                    Id = template.Id,
                    Type = template.Type,
                    Health = template.Health,
                    Attack = template.Attack,
                    Defense = template.Defense,
                    Speed = template.Speed,
                    Ability = template.Ability,
                    Passive = template.Passive,
                    Ultimate = template.Ultimate,
                    Position = position,
                    Team = team,
                    Energy = 10
                };

                teamPieces.Add(piece);
            }

            return teamPieces;
        }

        public static Dictionary<string, string> PackBonuses = new Dictionary<string, string>
        {
            { "Fire Pack", "All Fire Pack members gain +5% Attack when 3 or more are on the team." },
            { "Cyber Pack", "All Cyber Pack members gain +2 Defense when 3 or more are on the team." },
            { "Shadow Pack", "All Shadow Pack members gain +1 Speed when 3 or more are on the team." },
            { "Arcane Echoes", "All Arcane Echoes pieces gain +2 Speed and +5% Ability Power when 3 or more are on the board." },
            { "Blazing Rebellion", "All Blazing Rebellion pieces gain +5 Attack Power and Burning Strike (10% chance to ignite enemies on basic attacks) when 3 or more are on the board." },
        };

        public static Dictionary<string, List<(int Level, string EvolvedForm)>> EvolutionChains = new()
        {
            { "SparkPawn1", new List<(int, string)>
                {
                    (5, "ThunderPawn1"),
                    (10, "StormPawn1")
                }
            },
            { "StoneRook1", new List<(int, string)>
                {
                    (5, "IronRook1"),
                    (10, "ObsidianRook1")
                }
            },
            
            // Arcane Echoes Evolutions
            { "ArcanePawn1", new List<(int, string)> { (5, "EchoKnight1"), (10, "EchoChampion1") } },
            { "RunebladeKnight1", new List<(int, string)> { (5, "MysticBladeKnight1"), (10, "VoidbladeKnight1") } },
            { "ShardmindQueen1", new List<(int, string)> { (5, "AstralQueen1"), (10, "CosmicQueen1") } },

            // Blazing Rebellion Evolutions
            { "ScorchlingPawn1", new List<(int, string)> { (5, "FlamebornPawn1"), (10, "InfernalPawn1") } },
            { "InfernoKnight1", new List<(int, string)> { (5, "HellfireKnight1"), (10, "CataclysmKnight1") } },
            { "PyreKing1", new List<(int, string)> { (5, "InfernalKing1"), (10, "BlazingEmperor1") } },
        };

         /// <summary>
        /// Generates a balanced starter team with exactly 8 pawns, 2 knights, 2 bishops, 2 rooks, 1 queen, and 1 king, all from the specified pack.
        /// </summary>
        public static List<Piece> GenerateBalancedStarterTeam(string team, string pack)
        {
            List<Piece> teamPieces = new();
            Dictionary<string, int> pieceCount = new()
            {
                { "Pawn", 0 },
                { "Knight", 0 },
                { "Bishop", 0 },
                { "Rook", 0 },
                { "Queen", 0 },
                { "King", 0 }
            };

            // Standard chess piece counts
            Dictionary<string, int> maxPieces = new()
            {
                { "Pawn", 8 },
                { "Knight", 2 },
                { "Bishop", 2 },
                { "Rook", 2 },
                { "Queen", 1 },
                { "King", 1 }
            };

            // Pieces in order so king/queen get assigned first
            string[] orderedTypes = { "King", "Queen", "Rook", "Bishop", "Knight", "Pawn" };

            foreach (var type in orderedTypes)
            {
                var eligiblePieces = PieceTemplates.Values
                    .Where(p => p.Type.Equals(type, StringComparison.OrdinalIgnoreCase) && p.Pack == pack)
                    .ToList();

                if (eligiblePieces.Count == 0)
                {
                    Console.WriteLine($"⚠️ Warning: No {type}s found for {pack} pack!");
                    continue;
                }

                int piecesToAdd = maxPieces[type];

                for (int i = 0; i < piecesToAdd; i++)
                {
                    var selectedPiece = eligiblePieces[Random.Next(eligiblePieces.Count)].Clone();
                    selectedPiece.Team = team;
                    selectedPiece.Position = AssignStartingPosition(type, team, i);
                    teamPieces.Add(selectedPiece);
                }
            }

            return teamPieces;
        }

        /// <summary>
        /// Assigns the proper starting position for each piece type according to standard chess starting layout.
        /// </summary>
        private static string AssignStartingPosition(string type, string team, int count)
        {
            string rank = team == "Player" ? "1" : "8";
            string pawnRank = team == "Player" ? "2" : "7";

            return type switch
            {
                "Pawn" => $"{(char)('A' + count)}{pawnRank}",
                "Rook" => count == 0 ? $"A{rank}" : $"H{rank}",
                "Knight" => count == 0 ? $"B{rank}" : $"G{rank}",
                "Bishop" => count == 0 ? $"C{rank}" : $"F{rank}",
                "Queen" => $"D{rank}",
                "King" => $"E{rank}",
                _ => "A1"
            };
        }

        public static readonly Dictionary<string, AIPersonalityType> PackPersonalities = new()
        {
            { "Starter Pack", AIPersonalityType.Balanced },
            { "Fire Pack", AIPersonalityType.Aggressive },
            { "Cyber Pack", AIPersonalityType.Defensive },
            { "Shadow Pack", AIPersonalityType.Sneaky }
        };

        public static Piece GetPieceTemplateByTypeAndPack(string type, string pack)
        {
            return PieceTemplates.Values.FirstOrDefault(p => p.Type == type && p.Pack == pack);
        }
    }
}
