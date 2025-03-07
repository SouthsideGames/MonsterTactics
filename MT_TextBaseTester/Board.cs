using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class Board
    {
        public List<Piece> Pieces = new();
        public List<string> TurnHistory { get; private set; } = new();
        public Stack<List<Piece>> BoardHistory = new();
        public List<BoardSnapshot> Snapshots = new();
        private static readonly Random Random = new();

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

        public void CheckPawnPromotion(Piece piece)
        {
            if (piece.Type != "Pawn") return;

            string promotionRank = piece.Team == "Player" ? "8" : "1";
            if (piece.Position.EndsWith(promotionRank))
            {
                string promotedType = piece.Team == "Player" ? AskPlayerForPromotionChoice() : "Queen";
                PromotePawn(piece, promotedType);
            }
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
            var pack = pawn.Pack;
            var eligiblePieces = MonsterDatabase.PieceTemplates.Values
                .Where(p => p.Type == newType && p.Pack == pack)
                .ToList();

            if (eligiblePieces.Count == 0)
            {
                Console.WriteLine($"❌ No {newType} found in {pack}. Defaulting to Queen.");
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
            Snapshots.Add(new BoardSnapshot
            {
                TurnNumber = turnNumber,
                Pieces = Pieces.Select(p => p.Clone()).ToList(),
                TileEffects = TileEffects.CloneTileEffects()
            });
        }

        public void RestoreSnapshot(BoardSnapshot snapshot)
        {
            Pieces = snapshot.Pieces.Select(p => p.Clone()).ToList();
            TileEffects.RestoreTileEffects(snapshot.TileEffects);
            LogTurn($"Rewound to Turn {snapshot.TurnNumber}");
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

        public void ShowCompleteTeamPreview()
        {
            Console.WriteLine("\n=== Team Preview ===");
            foreach (var teamGroup in Pieces.GroupBy(p => p.Team))
            {
                Console.WriteLine($"\n--- {teamGroup.Key} Team ---");

                var packCounts = teamGroup.GroupBy(p => p.Pack).ToDictionary(g => g.Key, g => g.Count());

                foreach (var piece in teamGroup)
                {
                    string synergyStatus = packCounts.TryGetValue(piece.Pack, out int count) && count >= 3
                        ? "(Synergy Active)"
                        : "";
                    Console.WriteLine($"{piece.Id} ({piece.Type}) - {piece.Pack} {synergyStatus}");
                }
            }
            Console.WriteLine("=================================");
        }

        public void ShowDetailedEndOfGameReport(string winner)
        {
            Console.WriteLine($"\n=== End-of-Game Report: {winner} ===");
            Console.WriteLine($"Total Turns: {TurnHistory.Count}");

            foreach (var piece in Pieces)
            {
                string status = piece.Health > 0 ? "Survived" : "Eliminated";
                Console.WriteLine($"{piece.Team} {piece.Id}: {piece.TotalKills} Kills, {piece.TotalDamageDealt} Damage ({status})");
            }

            Console.WriteLine("\nComplete Turn Log:");
            for (int i = 0; i < TurnHistory.Count; i++)
                Console.WriteLine($"{i + 1}. {TurnHistory[i]}");

            Console.WriteLine("=================================");
        }

        // ---------------- Passives ----------------
        private void TriggerPiecePassive(Piece piece)
        {
            switch (piece.Passive)
            {
                case "Ethereal Link":
                    ApplyEtherealLink(piece);
                    break;
                case "Reality Anchor":
                    ApplyRealityAnchor(piece);
                    break;
                case "Fire Trail":
                    TileEffects.SetTileEffect(piece.Position, "Burning");
                    break;
                case "Molten Core":
                    ApplyMoltenCore(piece);
                    break;
                case "Rebel Leader":
                    ApplyRebelLeader(piece);
                    break;
            }
        }

        private void ApplyEtherealLink(Piece piece)
        {
            foreach (var ally in Pieces.Where(p => p.Team == piece.Team && IsAdjacent(piece, p)))
            {
                ally.Defense += 1;
                LogTurn($"{piece.Team} {piece.Id} grants +1 Defense to {ally.Team} {ally.Id} with Ethereal Link.");
            }
        }

        private void ApplyRealityAnchor(Piece piece)
        {
            if (HasActivePackBonus(piece.Team, "Arcane Echoes", 3))
            {
                piece.Defense += 5;
                LogTurn($"{piece.Team} {piece.Id} gains +5 Defense from Reality Anchor!");
            }
        }

        private void ApplyMoltenCore(Piece piece)
        {
            if (IsAdjacentToTileEffect(piece.Position, "Burning"))
            {
                piece.Defense += (int)(piece.Defense * 0.10);
                LogTurn($"{piece.Team} {piece.Id}'s Molten Core activates near burning tiles! Defense boosted.");
            }
        }

        private void ApplyRebelLeader(Piece piece)
        {
            if (HasActivePackBonus(piece.Team, "Blazing Rebellion", 3))
            {
                BoostAllAlliesAttack(piece.Team, 5);
                LogTurn($"{piece.Team} {piece.Id} boosts all allies with Rebel Leader (+5 Attack).");
            }
        }

        private void BoostAllAlliesAttack(string team, int boost)
        {
            foreach (var ally in Pieces.Where(p => p.Team == team))
                ally.Attack += boost;
        }

        private bool HasActivePackBonus(string team, string pack, int requiredCount) =>
            Pieces.Count(p => p.Team == team && p.Pack == pack && p.Health > 0) >= requiredCount;

        private bool IsAdjacentToTileEffect(string position, string effect) =>
            TileEffects.GetAdjacentTiles(position).Any(t => TileEffects.TileEffects.TryGetValue(t, out string e) && e == effect);

        public bool IsAdjacent(Piece a, Piece b)
        {
            int fileDiff = Math.Abs(a.Position[0] - b.Position[0]);
            int rankDiff = Math.Abs(a.Position[1] - b.Position[1]);
            return fileDiff <= 1 && rankDiff <= 1;
        }

        public Board Clone()
        {
            var clonedBoard = new Board();

            // Deep copy all pieces
            clonedBoard.Pieces = Pieces.Select(p => p.Clone()).ToList();

            // Copy turn history
            clonedBoard.TurnHistory = new List<string>(TurnHistory);

            // Copy tile effects
            clonedBoard.TileEffects.TileEffects = new Dictionary<string, string>(TileEffects.TileEffects);

            // Other managers may or may not need copying depending on design
            // Add any other data to clone here if needed

            return clonedBoard;
        }

        public string GetAIPack()
        {
            var firstAIPiece = Pieces.FirstOrDefault(p => p.Team == "AI");
            return firstAIPiece?.Pack ?? "Starter Pack";
        }

        public bool IsTileDefensive(string position)
        {
            return TileEffects.TileEffects.TryGetValue(position, out string effect) && effect == "Shielded";
        }

        public bool IsTileSafe(string position)
        {
            // Safe = No enemies threaten it and no harmful effects
            if (IsTileUnderThreat(position, "AI"))
                return false;

            if (TileEffects.TileEffects.TryGetValue(position, out string effect))
            {
                string[] harmfulEffects = { "Burning", "Poisoned", "Cursed", "Spiked" };
                if (harmfulEffects.Contains(effect))
                    return false;
            }

            return true;
        }

        public bool IsAdjacentToPosition(string pos1, string pos2)
        {
            (int row1, int col1) = MovementValidator.PositionToCoordinates(pos1);
            (int row2, int col2) = MovementValidator.PositionToCoordinates(pos2);

            int rowDiff = Math.Abs(row1 - row2);
            int colDiff = Math.Abs(col1 - col2);

            return rowDiff <= 1 && colDiff <= 1;
        }


        public bool IsTileUnderThreat(string position, string friendlyTeam)
        {
            return Pieces.Any(p => p.Team != friendlyTeam && p.Health > 0 &&
                                    MovementValidator.IsMoveLegal(p, position, Pieces));
        }

        public void DisplayBoard()
        {
            Console.WriteLine("\nCurrent Board:");
            foreach (var piece in Pieces)
            {
                Console.WriteLine($"{piece.Team} {piece.Id} ({piece.Type}) at {piece.Position} - HP: {piece.Health} Energy: {piece.Energy} Level: {piece.Level}");
            }

            foreach (var (position, effect) in TileEffects.TileEffects)
            {
                Console.WriteLine($"Tile {position} - {effect}");
            }
            Console.WriteLine();
        }





    }
}
