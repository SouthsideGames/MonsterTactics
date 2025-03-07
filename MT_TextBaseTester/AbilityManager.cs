using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class AbilityManager
    {
        private readonly Board _board;
        private static readonly Random _random = new();

        public AbilityManager(Board board)
        {
            _board = board;
        }

        public static bool AbilityInvolvesMovement(string ability)
        {
            var movementAbilities = new HashSet<string>
            {
                "Rune Dash", 
                "Ethereal Shift", 
                "Hyper Leap" 
                // Add others if more are added in future
            };
            return movementAbilities.Contains(ability);
        }

        public void UseAbility(Piece piece, string abilityName)
        {
            if (string.IsNullOrEmpty(abilityName))
            {
                _board.LogTurn($"{piece.Team} {piece.Id} tried to use an undefined ability.");
                return;
            }

            if (!MonsterDatabase.AbilityCosts.TryGetValue(abilityName, out int cost))
            {
                _board.LogTurn($"{piece.Team} {piece.Id} tried to use unknown ability: {abilityName}.");
                return;
            }

            int modifiedCost = Math.Max(1, cost + piece.EnergyCostModifier);

            if (piece.Energy < modifiedCost)
            {
                _board.LogTurn($"{piece.Team} {piece.Id} does not have enough energy to use {abilityName}. Needed: {modifiedCost}, Available: {piece.Energy}");
                return;
            }

            piece.Energy -= modifiedCost;

            _board.LogTurn($"{piece.Team} {piece.Id} uses {abilityName}!");

            ApplyAbilityEffect(piece, abilityName);
        }

        public void ApplyAbilityEffect(Piece piece, string abilityName)
        {
            switch (abilityName)
            {
                // Starter Pack
                case "Shock Pulse":
                    DamageNearbyEnemy(piece, 5);
                    break;
                case "Blitz Protocol":
                    piece.Attack += 5;
                    _board.LogTurn($"{piece.Team} {piece.Id} gains +5 Attack from Blitz Protocol!");
                    break;
                case "Overdrive Command":
                    BoostNearbyAllies(piece, 5, 2);
                    break;

                // Fire Pack
                case "Flame Jab":
                    DamageNearbyEnemy(piece, 6);
                    break;
                case "Lava Surge":
                    DamageNearbyEnemies(piece, 8, 2);
                    break;
                case "Eruption Shield":
                    piece.Defense += 4;
                    _board.LogTurn($"{piece.Team} {piece.Id} gains +4 Defense from Eruption Shield!");
                    break;
                case "Flame Storm":
                    DamageAllEnemies(piece, 10);
                    break;
                case "Ember Command":
                    BoostAllAllies(piece, 3, 1);
                    break;

                // Cyber Pack
                case "EMP Blast":
                    DamageNearbyEnemy(piece, 5);
                    break;
                case "Scan Pulse":
                    ScanNearbyEnemies(piece);
                    break;
                case "Electrowave":
                    DamageNearbyEnemies(piece, 7, 3);
                    break;
                case "System Override":
                    BoostAllAllies(piece, 4, 0);
                    break;

                // Shadow Pack
                case "Dark Strike":
                    DamageNearbyEnemy(piece, 7);
                    break;
                case "Void Pulse":
                    DamageNearbyEnemies(piece, 6, 2);
                    break;
                case "Queen’s Curse":
                    WeakenAllEnemies(piece, 2);
                    break;
                case "King’s Veil":
                    ApplyStealthToAllAllies(piece);
                    break;

                default:
                    _board.LogTurn($"{piece.Team} {piece.Id} used {abilityName}, but no effect is defined.");
                    break;
            }
        }

        // ========= Helper Methods =========

        private void DamageNearbyEnemy(Piece piece, int damage)
        {
            var enemy = _board.Pieces
                .FirstOrDefault(p => p.Team != piece.Team && _board.IsAdjacent(piece, p));

            if (enemy != null)
            {
                _board.CombatManager.DamagePiece(piece, enemy, damage);
                _board.LogTurn($"{piece.Team} {piece.Id} hits {enemy.Team} {enemy.Id} for {damage} damage!");
            }
        }

        private void DamageNearbyEnemies(Piece piece, int damage, int maxTargets)
        {
            var enemies = _board.Pieces
                .Where(p => p.Team != piece.Team && _board.IsAdjacent(piece, p))
                .Take(maxTargets);

            foreach (var enemy in enemies)
            {
                _board.CombatManager.DamagePiece(piece, enemy, damage);
                _board.LogTurn($"{piece.Team} {piece.Id} hits {enemy.Team} {enemy.Id} for {damage} damage!");
            }
        }

        private void DamageAllEnemies(Piece piece, int damage)
        {
            foreach (var enemy in _board.Pieces.Where(p => p.Team != piece.Team))
            {
                _board.CombatManager.DamagePiece(piece, enemy, damage);
                _board.LogTurn($"{piece.Team} {piece.Id} hits {enemy.Team} {enemy.Id} for {damage} damage!");
            }
        }

        private void BoostNearbyAllies(Piece piece, int attackBoost, int speedBoost)
        {
            var allies = _board.Pieces
                .Where(p => p.Team == piece.Team && _board.IsAdjacent(piece, p));

            foreach (var ally in allies)
            {
                ally.Attack += attackBoost;
                ally.Speed += speedBoost;
                _board.LogTurn($"{piece.Team} {piece.Id} boosts {ally.Team} {ally.Id} by +{attackBoost} Attack and +{speedBoost} Speed!");
            }
        }

        private void BoostAllAllies(Piece piece, int attackBoost, int speedBoost)
        {
            foreach (var ally in _board.Pieces.Where(p => p.Team == piece.Team))
            {
                ally.Attack += attackBoost;
                ally.Speed += speedBoost;
                _board.LogTurn($"{piece.Team} {piece.Id} boosts all allies by +{attackBoost} Attack and +{speedBoost} Speed!");
            }
        }

        private void ScanNearbyEnemies(Piece piece)
        {
            var enemies = _board.Pieces
                .Where(p => p.Team != piece.Team && _board.IsAdjacent(piece, p));

            foreach (var enemy in enemies)
            {
                _board.LogTurn($"Scan Pulse detected: {enemy.Team} {enemy.Id} at {enemy.Position} - HP: {enemy.Health}");
            }
        }

        private void WeakenAllEnemies(Piece piece, int attackReduction)
        {
            foreach (var enemy in _board.Pieces.Where(p => p.Team != piece.Team))
            {
                enemy.Attack = Math.Max(1, enemy.Attack - attackReduction);
                _board.LogTurn($"{piece.Team} {piece.Id} weakens {enemy.Team} {enemy.Id} by -{attackReduction} Attack!");
            }
        }

        private void ApplyStealthToAllAllies(Piece piece)
        {
            foreach (var ally in _board.Pieces.Where(p => p.Team == piece.Team))
            {
                ally.Speed += 1;  // Stealth represented as a Speed buff (optional customization)
                _board.LogTurn($"{piece.Team} {piece.Id} grants stealth (Speed +1) to {ally.Team} {ally.Id}!");
            }
        }
    }
}
