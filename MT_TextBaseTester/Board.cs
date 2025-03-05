using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class Board
    {
        public List<Piece> Pieces = new List<Piece>();
        public List<string> TurnHistory { get; private set; } = new();

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
            // Manual team creation logic can be placed here if needed.
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
    }
}
