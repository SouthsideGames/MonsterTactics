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
            { "Arcane Pulse", 4 }
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
            { "System Reboot", "Heals 5 HP after every kill." }
        };

        public static Dictionary<string, Piece> PieceTemplates = new Dictionary<string, Piece>
        {
            { "FireKnight1", new Piece { Id = "FireKnight1", Type = "Knight", Pack = "Fire Pack", Health = 35, Attack = 9, Defense = 4, Speed = 6, Ability = "Fire Leap", Passive = "Blazing Stride", Ultimate = "Sky Strike" } },
            { "AquaBishop1", new Piece { Id = "AquaBishop1", Type = "Bishop", Pack = "Water Pack", Health = 30, Attack = 7, Defense = 3, Speed = 5, Ability = "Healing Mist", Passive = "Water Ward", Ultimate = "Blessing of the Stars" } },
            { "EarthRook1", new Piece { Id = "EarthRook1", Type = "Rook", Pack = "Earth Pack", Health = 40, Attack = 8, Defense = 5, Speed = 4, Ability = "Stonewall", Passive = "Immovable Object", Ultimate = "Fortress Mode" } },
            { "ShadowPawn1", new Piece { Id = "ShadowPawn1", Type = "Pawn", Pack = "Shadow Pack", Health = 25, Attack = 6, Defense = 2, Speed = 5, Ability = "Dark Strike", Passive = "Shadow Step", Ultimate = "Swarm Assault" } },
            { "OverclockKnight1", new Piece { Id = "OverclockKnight1", Type = "Knight", Pack = "Cyber Pack", Health = 40, Attack = 17, Defense = 4, Speed = 5, Ability = "Blitz Protocol", Passive = "System Reboot" } },
            { "PlasmaQueen1", new Piece { Id = "PlasmaQueen1", Type = "Queen", Pack = "Cyber Pack", Health = 45, Attack = 21, Defense = 5, Speed = 4, Ability = "Plasma Barrage", Passive = "Energy Surge" } }
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
            { "Shadow Pack", "All Shadow Pack members gain +1 Speed when 3 or more are on the team." }
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
            }
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
    }
}
