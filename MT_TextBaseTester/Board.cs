using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class Board
    {
        public List<Piece> Pieces = new List<Piece>();
        public List<string> TurnHistory { get; private set; } = new List<string>();
        public BoardRenderer Renderer { get; private set; } = new BoardRenderer();
        public TileEffectManager TileEffects { get; private set; }

        public Board()
        {
            TileEffects = new TileEffectManager(this);
        }

        public void RandomlyGeneratePieces()
        {
            Pieces = MonsterDatabase.GenerateRandomTeam("Player");
            Pieces.AddRange(MonsterDatabase.GenerateRandomTeam("AI"));
            foreach (var piece in Pieces)
            {
                Renderer.UpdateTile(piece.Position, piece);
            }
        }

        public void PlayerCreateTeam()
        {
            // Manual team creation logic placeholder.
        }

        public bool CheckWinCondition(out string winner)
        {
            bool playerKing = Pieces.Any(p => p.Team == "Player" && p.Type == "King" && p.Health > 0);
            bool aiKing = Pieces.Any(p => p.Team == "AI" && p.Type == "King" && p.Health > 0);

            if (!playerKing)
            {
                winner = "AI Wins!";
                return true;
            }
            if (!aiKing)
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

        public void ApplyAbilityEffect(Piece piece, string abilityName)
        {
            switch (abilityName)
            {
                case "Shock Pulse":
                    DamageNearbyEnemy(piece, 5);
                    break;
                case "Blitz Protocol":
                    piece.Attack += 5;
                    LogTurn($"{piece.Team} {piece.Id} activated Blitz Protocol, gaining +5 Attack!");
                    break;
                case "Overdrive Command":
                    BoostNearbyAllies(piece, 5, 2);
                    break;
                default:
                    LogTurn($"{piece.Team} {piece.Id} used {abilityName}, but no effect implemented.");
                    break;
            }
        }

        public void PerformHitAndRetreat(Piece attacker, Piece target)
        {
            DamagePiece(attacker, target, attacker.Attack);
            if (target.Health > 0)
            {
                string retreatTile = FindClosestSafeTile(attacker);
                LogTurn($"{attacker.Team} {attacker.Id} strikes {target.Team} {target.Id} and retreats to {retreatTile}");
                MovePiece(attacker, retreatTile);
            }
            else
            {
                LogTurn($"{attacker.Team} {attacker.Id} captures {target.Team} {target.Id} and takes the tile.");
                MovePiece(attacker, target.Position);
            }
        }

        private string FindClosestSafeTile(Piece piece)
        {
            var legalMoves = MovementValidator.GetLegalMoves(piece, Pieces);
            var safeMoves = legalMoves.Where(pos => !IsTileUnderThreat(pos, piece.Team)).ToList();
            return safeMoves.FirstOrDefault() ?? piece.Position;
        }

        private void DamageNearbyEnemy(Piece attacker, int damage)
        {
            var enemy = Pieces.FirstOrDefault(p => p.Team != attacker.Team && IsAdjacent(attacker, p));
            if (enemy != null) DamagePiece(attacker, enemy, damage);
        }

        private void DamagePiece(Piece attacker, Piece target, int damage)
        {
            target.Health -= damage;
            attacker.TotalDamageDealt += damage;

            LogTurn($"{attacker.Team} {attacker.Id} dealt {damage} damage to {target.Team} {target.Id}");

            if (target.Health <= 0)
            {
                target.Health = 0;
                attacker.TotalKills++;
                LogTurn($"{target.Team} {target.Id} was eliminated by {attacker.Team} {attacker.Id}");
                Renderer.UpdateTile(target.Position, null);
            }
        }

        public void ApplySynergyBonuses()
        {
            var packCounts = Pieces
                .Where(p => !string.IsNullOrEmpty(p.Pack))
                .GroupBy(p => p.Pack)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var piece in Pieces)
            {
                if (packCounts.TryGetValue(piece.Pack, out int count) && count >= 3)
                {
                    ApplyPackBonus(piece, piece.Pack);
                }
            }
        }

        private void ApplyPackBonus(Piece piece, string pack)
        {
            switch (pack)
            {
                case "Fire Pack": piece.Attack = (int)(piece.Attack * 1.05); break;
                case "Cyber Pack": piece.Defense += 2; break;
                case "Shadow Pack": piece.Speed += 1; break;
            }
            LogTurn($"{piece.Team} {piece.Id} benefits from {pack} Synergy!");
        }

        public void MovePiece(Piece piece, string newPosition)
        {
            Renderer.UpdateTile(piece.Position, null);
            piece.Position = newPosition;
            Renderer.UpdateTile(newPosition, piece);
            TileEffects.ApplyOnEntryEffects(piece, newPosition);
        }

        public void ProcessStartOfTurnEffects(string team)
        {
            foreach (var piece in Pieces.Where(p => p.Team == team && p.Health > 0))
            {
                TileEffects.ApplyStartOfTurnEffects(piece);
            }
        }

        public Board Clone()
        {
            var clone = new Board
            {
                Pieces = Pieces.Select(p => p.Clone()).ToList(),
                TurnHistory = new List<string>(TurnHistory)
            };

            foreach (var piece in clone.Pieces)
                clone.Renderer.UpdateTile(piece.Position, piece);

            return clone;
        }

        public bool IsTileUnderThreat(string position, string team)
        {
            return Pieces.Any(p => p.Team != team && MovementValidator.IsMoveLegal(p, position, Pieces));
        }

        public bool IsAdjacent(Piece a, Piece b)
        {
            int fileDiff = Math.Abs(a.Position[0] - b.Position[0]);
            int rankDiff = Math.Abs(a.Position[1] - b.Position[1]);
            return fileDiff <= 1 && rankDiff <= 1;
        }

        public bool IsAdjacentToPosition(string pos1, string pos2)
        {
            int fileDiff = Math.Abs(pos1[0] - pos2[0]);
            int rankDiff = Math.Abs(pos1[1] - pos2[1]);
            return fileDiff <= 1 && rankDiff <= 1;
        }

        private void BoostNearbyAllies(Piece piece, int attackBoost, int speedBoost)
        {
            var nearbyAllies = Pieces.Where(p => p.Team == piece.Team && IsAdjacent(piece, p)).ToList();

            foreach (var ally in nearbyAllies)
            {
                ally.Attack += attackBoost;
                ally.Speed += speedBoost;
                LogTurn($"{piece.Team} {piece.Id} boosted {ally.Team} {ally.Id}, giving +{attackBoost} Attack and +{speedBoost} Speed!");
            }
        }

    }
}
