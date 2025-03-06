using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class Board
    {
        public List<Piece> Pieces = new List<Piece>();
        public List<string> TurnHistory { get; private set; } = new();
        public Stack<List<Piece>> BoardHistory = new();
        public List<BoardSnapshot> Snapshots = new List<BoardSnapshot>();
        private static readonly Random Random = new Random();

        // New managers
        public TileEffectManager TileEffects { get; }
        public PieceEvolutionManager EvolutionManager { get; }
        public SynergyManager SynergyManager { get; }
        public AbilityManager AbilityManager { get; }
        public CombatManager CombatManager { get; }
        public MatchSummaryManager SummaryManager { get; }

        public Board()
        {
            TileEffects = new TileEffectManager(this);
            EvolutionManager = new PieceEvolutionManager(this);
            SynergyManager = new SynergyManager(this);
            AbilityManager = new AbilityManager(this);
            CombatManager = new CombatManager(this);
            SummaryManager = new MatchSummaryManager(this);
        }

        public void RandomlyGeneratePieces()
        {
            Pieces = MonsterDatabase.GenerateRandomTeam("Player");
            Pieces.AddRange(MonsterDatabase.GenerateRandomTeam("AI"));
        }

        public void PlayerCreateTeam()
        {
            Pieces.Clear();
            Console.WriteLine("\n=== Team Creation ===");
            Console.WriteLine("Create your custom team by selecting pieces from Fire, Cyber, and Shadow packs.");
            Console.WriteLine("You need: 8 Pawns, 2 Knights, 2 Bishops, 2 Rooks, 1 Queen, 1 King.");

            AddPiecesToTeam("Pawn", 8);
            AddPiecesToTeam("Knight", 2);
            AddPiecesToTeam("Bishop", 2);
            AddPiecesToTeam("Rook", 2);
            AddPiecesToTeam("Queen", 1);
            AddPiecesToTeam("King", 1);

            Console.WriteLine("\nYour custom team has been created!\n");
            DisplayBoard();
        }

        public void DisplayBoard()
        {
            Console.WriteLine("\nCurrent Board:");
            foreach (var piece in Pieces)
            {
                Console.WriteLine($"{piece.Team} {piece.Id} ({piece.Type}) at {piece.Position} - HP: {piece.Health} Energy: {piece.Energy} Level: {piece.Level}");
            }
            Console.WriteLine();

            foreach (var (position, effect) in TileEffects.TileEffects)
            {
                Console.WriteLine($"Tile {position} - {effect}");
            }
        }

        public bool CheckWinCondition(out string winner)
        {
            bool playerKingAlive = Pieces.Any(p => p.Team == "Player" && p.Type == "King" && p.Health > 0);
            bool aiKingAlive = Pieces.Any(p => p.Team == "AI" && p.Type == "King" && p.Health > 0);

            if (!playerKingAlive)
            {
                winner = "AI Wins!";
                return true;
            }

            if (!aiKingAlive)
            {
                winner = "Player Wins!";
                return true;
            }

            winner = "";
            return false;
        }

        public void LogTurn(string message)
        {
            TurnHistory.Add(message);
            Console.WriteLine(message);
        }

        public void ProcessStartOfTurnEffects(string team)
        {
            foreach (var piece in Pieces.Where(p => p.Team == team && p.Health > 0))
            {
                TileEffects.ApplyStartOfTurnEffects(piece);
                TriggerPiecePassive(piece);
            }
        }

        public void MovePiece(Piece piece, string newPosition)
        {
            piece.Position = newPosition;
            TileEffects.ApplyOnEntryEffects(piece, newPosition);
        }

        private void TriggerPiecePassive(Piece piece)
        {
            switch (piece.Passive)
            {
                case "Water Ward":
                    HealAdjacentAllies(piece, 2);
                    break;

                case "Rooted Resilience":
                    if (IsAdjacentToFriendlyPiece(piece, "Pawn"))
                    {
                        piece.Defense += 2;
                        LogTurn($"{piece.Team} {piece.Id} gains +2 Defense from Rooted Resilience!");
                    }
                    break;

                case "Blazing Stride":
                    TileEffects.SetTileEffect(piece.Position, "Burning");
                    break;

                case "Commanding Presence":
                    BoostAdjacentAllies(piece, 5);
                    break;

                case "Ethereal Link":
                    ApplyEtherealLink(piece);
                    break;

                case "Reality Anchor":
                    ApplyRealityAnchor(piece);
                    break;

                case "Fire Trail":
                    ApplyFireTrail(piece);
                    break;

                case "Molten Core":
                    ApplyMoltenCore(piece);
                    break;

                case "Rebel Leader":
                    ApplyRebelLeader(piece);
                    break;
            }
        }

        private void HealAdjacentAllies(Piece piece, int healAmount)
        {
            var allies = Pieces.Where(p => p.Team == piece.Team && IsAdjacent(piece, p)).ToList();
            foreach (var ally in allies)
            {
                CombatManager.HealPiece(piece, ally, healAmount);
            }
        }

        private void BoostAdjacentAllies(Piece piece, int attackBoost)
        {
            var allies = Pieces.Where(p => p.Team == piece.Team && IsAdjacent(piece, p)).ToList();
            foreach (var ally in allies)
            {
                ally.Attack += attackBoost;
                LogTurn($"{piece.Team} {piece.Id} boosts {ally.Team} {ally.Id}'s Attack by {attackBoost}!");
            }
        }

        private bool IsAdjacentToFriendlyPiece(Piece piece, string type)
        {
            return Pieces.Any(p => p.Team == piece.Team && p.Type == type && IsAdjacent(piece, p));
        }

        public bool IsAdjacent(Piece a, Piece b)
        {
            int fileDiff = Math.Abs(a.Position[0] - b.Position[0]);
            int rankDiff = Math.Abs(a.Position[1] - b.Position[1]);
            return fileDiff <= 1 && rankDiff <= 1;
        }

        public bool IsAdjacentToPosition(string position1, string position2)
        {
            int fileDiff = Math.Abs(position1[0] - position2[0]);
            int rankDiff = Math.Abs(position1[1] - position2[1]);
            return fileDiff <= 1 && rankDiff <= 1;
        }

        public bool IsTileUnderThreat(string position, string team)
        {
            return Pieces.Any(p => p.Team != team && p.Health > 0 && MovementValidator.IsMoveLegal(p, position, Pieces));
        }

        public void ShowTeamPreview()
        {
            Console.WriteLine("\n=== Team Preview ===");

            var packCounts = Pieces
                .Where(p => !string.IsNullOrEmpty(p.Pack))
                .GroupBy(p => p.Pack)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var piece in Pieces.Where(p => p.Team == "Player"))
            {
                string synergyStatus = packCounts.TryGetValue(piece.Pack, out int count) && count >= 3
                    ? "(Synergy Active)"
                    : "";
                Console.WriteLine($"{piece.Id} ({piece.Type}) - {piece.Pack} {synergyStatus}");
            }

            Console.WriteLine("=================================");
        }

        public Board Clone()
        {
            return new Board
            {
                Pieces = Pieces.Select(p => p.Clone()).ToList(),
                TurnHistory = new List<string>(TurnHistory)
            };
        }

        public void ShowDetailedEndOfGameReport(string winner)
        {
            Console.WriteLine("\n=== Detailed End-of-Game Report ===");

            Console.WriteLine($"\nWinner: {winner}");
            Console.WriteLine($"Total Turns: {TurnHistory.Count}");

            Console.WriteLine("\nPiece Performance:");
            foreach (var piece in Pieces)
            {
                string status = piece.Health > 0 ? "Survived" : "Eliminated";
                Console.WriteLine($"- {piece.Team} {piece.Id}: {piece.TotalKills} Kills, {piece.TotalDamageDealt} Damage Dealt ({status})");
            }

            Console.WriteLine("\nAbilities Used:");
            var abilityUsage = TurnHistory
                .Where(entry => entry.Contains("used"))
                .GroupBy(entry => entry.Split(' ')[1] + " " + entry.Split(' ')[2])
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var ability in abilityUsage)
            {
                Console.WriteLine($"- {ability.Key} used abilities {ability.Value} times");
            }

            Console.WriteLine("\nEvolution Log:");
            var evolutionLog = TurnHistory.Where(entry => entry.Contains("evolved into")).ToList();
            if (evolutionLog.Count > 0)
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
            for (int i = 0; i < TurnHistory.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {TurnHistory[i]}");
            }

            Console.WriteLine("==================================");
        }

        public void CheckPawnPromotion(Piece piece)
        {
            if (piece.Type != "Pawn") return;

            string promotionRank = piece.Team == "Player" ? "8" : "1";
            if (!piece.Position.EndsWith(promotionRank)) return;

            string promotedType = piece.Team == "Player" ? AskPlayerForPromotionChoice() : "Queen";

            PromotePawn(piece, promotedType);
        }

        private string AskPlayerForPromotionChoice()
        {
            Console.WriteLine("Your pawn has reached the promotion rank! Choose a piece to promote into:");
            Console.WriteLine("1 - Queen");
            Console.WriteLine("2 - Rook");
            Console.WriteLine("3 - Bishop");
            Console.WriteLine("4 - Knight");

            string choice = Console.ReadLine()?.Trim();
            return choice switch
            {
                "1" => "Queen",
                "2" => "Rook",
                "3" => "Bishop",
                "4" => "Knight",
                _ => "Queen"
            };
        }

        private void PromotePawn(Piece pawn, string newType)
        {
            var pack = pawn.Pack;  // Retain pack for synergy
            var eligiblePieces = MonsterDatabase.PieceTemplates.Values
                .Where(p => p.Type == newType && p.Pack == pack)
                .ToList();

            if (eligiblePieces.Count == 0)
            {
                Console.WriteLine($"❌ No {newType} found in {pack}! Promoting to default Queen.");
                newType = "Queen";
                eligiblePieces = MonsterDatabase.PieceTemplates.Values
                    .Where(p => p.Type == "Queen" && p.Pack == pack)
                    .ToList();
            }

            var promotedPiece = eligiblePieces[Random.Next(eligiblePieces.Count)].Clone();
            promotedPiece.Team = pawn.Team;
            promotedPiece.Position = pawn.Position;

            Pieces.Remove(pawn);
            Pieces.Add(promotedPiece);

            LogTurn($"{pawn.Team} Pawn promoted to {promotedPiece.Type} ({promotedPiece.Id})!");
        }

        public void ApplyTileEffectsToAllPieces()
        {
            TileEffects.ApplyEffectsToAllPieces(this);
        }


        public void SaveSnapshot(int turnNumber)
        {
            var snapshot = new BoardSnapshot
            {
                TurnNumber = turnNumber,
                Pieces = Pieces.Select(p => p.Clone()).ToList(),
                TileEffects = TileEffects?.CloneTileEffects() ?? new Dictionary<string, string>()
            };

            Snapshots.Add(snapshot);
        }

        public void RestoreSnapshot(BoardSnapshot snapshot)
        {
            Pieces = snapshot.Pieces.Select(p => p.Clone()).ToList();
            TileEffects?.RestoreTileEffects(snapshot.TileEffects);
            LogTurn($"Rewound to Turn {snapshot.TurnNumber}");
        }     

        public string GetAIPack()
        {
            var aiPiece = Pieces.FirstOrDefault(p => p.Team == "AI");
            return aiPiece?.Pack ?? "Starter Pack";
        }

        public bool IsTileSafe(string position)
        {
            // Safe means: No enemies threaten this tile, and the tile effect is not harmful.
            if (IsTileUnderThreat(position, "AI"))
                return false;  // AI is looking for safe tiles for its own pieces.

            if (TileEffects.TileEffects.TryGetValue(position, out string effect))
            {
                // If there’s an effect, check if it’s harmful.
                string[] harmfulEffects = { "Burning", "Poisoned", "Cursed", "Spiked" };
                if (harmfulEffects.Contains(effect))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsTileDefensive(string position)
        {
            return TileEffects.TileEffects.TryGetValue(position, out string effect) && effect == "Shielded";
        }

        private void AddPiecesToTeam(string pieceType, int requiredCount)
        {
            List<string> availablePacks = new() { "Fire Pack", "Cyber Pack", "Shadow Pack" };

            for (int i = 0; i < requiredCount; i++)
            {
                Console.WriteLine($"\nSelect {pieceType} {i + 1}/{requiredCount}");
                Console.WriteLine("Available Packs:");

                for (int j = 0; j < availablePacks.Count; j++)
                {
                    Console.WriteLine($"{j + 1} - {availablePacks[j]}");
                }

                string packChoice = Console.ReadLine()?.Trim();
                if (packChoice?.ToLower() == "quit") Environment.Exit(0);

                int packIndex = int.TryParse(packChoice, out int result) ? result - 1 : 0;
                string selectedPack = availablePacks[Math.Clamp(packIndex, 0, availablePacks.Count - 1)];

                var template = MonsterDatabase.GetPieceTemplateByTypeAndPack(pieceType, selectedPack);
                if (template == null)
                {
                    Console.WriteLine($"❌ No {pieceType} available in {selectedPack}. Please select again.");
                    i--;  // Try again for this slot
                    continue;
                }

                var newPiece = template.Clone();
                newPiece.Id = $"{newPiece.Id}{i + 1}";
                newPiece.Team = "Player";
                newPiece.Position = GetStartingPosition(pieceType, i + 1, "Player");
                Pieces.Add(newPiece);

                Console.WriteLine($"✅ Added {newPiece.Id} from {newPiece.Pack}.");
            }
        }

        private string GetStartingPosition(string pieceType, int count, string team)
        {
            // Define starting positions based on the team and piece type
            if (team == "Player")
            {
                return pieceType switch
                {
                    "Pawn" => $"A{count}",
                    "Knight" => count == 1 ? "B1" : "G1",
                    "Bishop" => count == 1 ? "C1" : "F1",
                    "Rook" => count == 1 ? "A1" : "H1",
                    "Queen" => "D1",
                    "King" => "E1",
                    _ => "A1"
                };
            }
            else
            {
                return pieceType switch
                {
                    "Pawn" => $"A{count}",
                    "Knight" => count == 1 ? "B8" : "G8",
                    "Bishop" => count == 1 ? "C8" : "F8",
                    "Rook" => count == 1 ? "A8" : "H8",
                    "Queen" => "D8",
                    "King" => "E8",
                    _ => "A8"
                };
            }
        }

        private void ApplyEtherealLink(Piece piece)
        {
            var allies = Pieces.Where(p => p.Team == piece.Team && IsAdjacent(piece, p));
            foreach (var ally in allies)
            {
                ally.Defense += 1;
                LogTurn($"{piece.Team} {piece.Id}'s Ethereal Link grants +1 Defense to {ally.Team} {ally.Id}.");
            }
        }

        private void ApplyRealityAnchor(Piece piece)
        {
            int arcaneCount = Pieces.Count(p => p.Team == piece.Team && p.Pack == "Arcane Echoes" && p.Health > 0);
            if (arcaneCount >= 3)
            {
                piece.Defense += 5;
                LogTurn($"{piece.Team} {piece.Id}'s Reality Anchor activates! +5 Defense.");
            }
        }

        private void ApplyFireTrail(Piece piece)
        {
            TileEffects.SetTileEffect(piece.Position, "Burning");
            LogTurn($"{piece.Team} {piece.Id}'s Fire Trail leaves {piece.Position} burning!");
        }

        private void ApplyMoltenCore(Piece piece)
        {
            bool nearBurningTile = GetAdjacentTiles(piece.Position).Any(pos =>
                TileEffects.TileEffects.TryGetValue(pos, out string effect) && effect == "Burning");

            if (nearBurningTile)
            {
                piece.Defense += (int)(piece.Defense * 0.10);  // 10% Defense boost
                LogTurn($"{piece.Team} {piece.Id}'s Molten Core activates near burning tiles! Defense increased.");
            }
        }

        private void ApplyRebelLeader(Piece piece)
        {
            int rebellionCount = Pieces.Count(p => p.Team == piece.Team && p.Pack == "Blazing Rebellion" && p.Health > 0);
            if (rebellionCount >= 3)
            {
                foreach (var ally in Pieces.Where(p => p.Team == piece.Team))
                {
                    ally.Attack += 5;
                }
                LogTurn($"{piece.Team} {piece.Id}'s Rebel Leader boosts all allies' Attack by +5!");
            }
        }

        private List<string> GetAdjacentTiles(string position)
        {
            char file = position[0];
            int rank = int.Parse(position[1].ToString());

            List<string> adjacent = new()
            {
                $"{(char)(file - 1)}{rank}",     // Left
                $"{(char)(file + 1)}{rank}",     // Right
                $"{file}{rank - 1}",             // Down
                $"{file}{rank + 1}",             // Up
                $"{(char)(file - 1)}{rank - 1}", // Down-left
                $"{(char)(file + 1)}{rank - 1}", // Down-right
                $"{(char)(file - 1)}{rank + 1}", // Up-left
                $"{(char)(file + 1)}{rank + 1}"  // Up-right
            };

            return adjacent.Where(pos => IsValidTile(pos)).ToList();
        }

        private bool IsValidTile(string position)
        {
            if (position.Length != 2) return false;
            char file = position[0];
            int rank = position[1] - '0';

            return file >= 'A' && file <= 'H' && rank >= 1 && rank <= 8;
        }

    }


}
