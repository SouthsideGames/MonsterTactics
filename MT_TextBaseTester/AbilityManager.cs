using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class AbilityManager
    {
        private readonly Board _board;
        private static readonly Random _random = new Random();

        public AbilityManager(Board board)
        {
            _board = board;
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
                    _board.LogTurn($"{piece.Team} {piece.Id} delivers a fiery jab, dealing 6 damage!");
                    break;
                case "Flame Charge":
                    piece.Speed += 2;
                    DamageNearbyEnemy(piece, 7);
                    _board.LogTurn($"{piece.Team} {piece.Id} charges forward, boosting Speed by 2 and dealing 7 damage!");
                    break;
                case "Lava Surge":
                    DamageNearbyEnemies(piece, 8, 2);
                    _board.LogTurn($"{piece.Team} {piece.Id} summons a Lava Surge, damaging nearby enemies!");
                    break;
                case "Eruption Shield":
                    piece.Defense += 4;
                    _board.LogTurn($"{piece.Team} {piece.Id} gains +4 Defense from Eruption Shield!");
                    break;
                case "Flame Storm":
                    DamageAllEnemies(piece, 10);
                    _board.LogTurn($"{piece.Team} {piece.Id} engulfs the battlefield in Flame Storm, dealing 10 damage to all enemies!");
                    break;
                case "Ember Command":
                    BoostAllAllies(piece, 3, 1);
                    _board.LogTurn($"{piece.Team} {piece.Id} empowers allies with Ember Command (+3 Attack, +1 Speed)!");
                    break;

                // Cyber Pack
                case "EMP Blast":
                    DamageNearbyEnemy(piece, 5);
                    _board.LogTurn($"{piece.Team} {piece.Id} disrupts a nearby enemy with EMP Blast!");
                    break;
                case "Hyper Leap":
                    piece.Speed += 2;
                    _board.LogTurn($"{piece.Team} {piece.Id} leaps into action, gaining +2 Speed!");
                    break;
                case "Scan Pulse":
                    ScanNearbyEnemies(piece);
                    break;
                case "Electrowave":
                    DamageNearbyEnemies(piece, 7, 3);
                    _board.LogTurn($"{piece.Team} {piece.Id} fires an Electrowave, damaging multiple enemies!");
                    break;
                case "System Override":
                    BoostAllAllies(piece, 4, 0);
                    _board.LogTurn($"{piece.Team} {piece.Id} activates System Override, boosting all allies' Attack by 4!");
                    break;

                // Shadow Pack
                case "Dark Strike":
                    DamageNearbyEnemy(piece, 7);
                    _board.LogTurn($"{piece.Team} {piece.Id} performs a Dark Strike, dealing 7 damage!");
                    break;
                case "Shadow Strike":
                    DamageNearbyEnemy(piece, 8);
                    _board.LogTurn($"{piece.Team} {piece.Id} strikes from the shadows, dealing 8 damage!");
                    break;
                case "Void Pulse":
                    DamageNearbyEnemies(piece, 6, 2);
                    _board.LogTurn($"{piece.Team} {piece.Id} releases a Void Pulse, damaging nearby enemies!");
                    break;
                case "Ethereal Shift":
                    MoveToRandomAdjacentTile(piece);
                    _board.LogTurn($"{piece.Team} {piece.Id} vanishes and reappears with Ethereal Shift!");
                    break;
                case "Queen’s Curse":
                    WeakenAllEnemies(piece, 2);
                    _board.LogTurn($"{piece.Team} {piece.Id} curses all enemies, reducing their Attack by 2!");
                    break;
                case "King’s Veil":
                    ApplyStealthToAllAllies(piece);
                    _board.LogTurn($"{piece.Team} {piece.Id} shrouds all allies in King's Veil (Stealth)!");
                    break;

                default:
                    _board.LogTurn($"{piece.Team} {piece.Id} used {abilityName}, but no effect is defined.");
                    break;
            }
        }

        private void DamageNearbyEnemy(Piece piece, int damage)
        {
            var enemy = _board.Pieces.Find(p => p.Team != piece.Team && _board.IsAdjacent(piece, p));
            if (enemy != null)
            {
                _board.CombatManager.DamagePiece(piece, enemy, damage);
            }
        }

        private void DamageNearbyEnemies(Piece piece, int damage, int maxTargets)
        {
            var enemies = _board.Pieces
                .FindAll(p => p.Team != piece.Team && _board.IsAdjacent(piece, p))
                .Take(maxTargets);

            foreach (var enemy in enemies)
            {
                _board.CombatManager.DamagePiece(piece, enemy, damage);
            }
        }

        private void DamageAllEnemies(Piece piece, int damage)
        {
            foreach (var enemy in _board.Pieces.Where(p => p.Team != piece.Team))
            {
                _board.CombatManager.DamagePiece(piece, enemy, damage);
            }
        }

        private void BoostNearbyAllies(Piece piece, int attackBoost, int speedBoost)
        {
            var allies = _board.Pieces.FindAll(p => p.Team == piece.Team && _board.IsAdjacent(piece, p));
            foreach (var ally in allies)
            {
                ally.Attack += attackBoost;
                ally.Speed += speedBoost;
            }
        }

        private void BoostAllAllies(Piece piece, int attackBoost, int speedBoost)
        {
            foreach (var ally in _board.Pieces.Where(p => p.Team == piece.Team))
            {
                ally.Attack += attackBoost;
                ally.Speed += speedBoost;
            }
        }

        private void ScanNearbyEnemies(Piece piece)
        {
            var enemies = _board.Pieces.FindAll(p => p.Team != piece.Team && _board.IsAdjacent(piece, p));
            foreach (var enemy in enemies)
            {
                _board.LogTurn($"Scan detected: {enemy.Team} {enemy.Id} at {enemy.Position} - HP: {enemy.Health}");
            }
        }

        private void MoveToRandomAdjacentTile(Piece piece)
        {
            var adjacentTiles = MovementValidator.GetLegalMoves(piece, _board.Pieces)
                .Where(pos => _board.Pieces.All(p => p.Position != pos))
                .ToList();

            if (adjacentTiles.Any())
            {
                string newPosition = adjacentTiles[_random.Next(adjacentTiles.Count)];
                _board.MovePiece(piece, newPosition);
            }
        }

        private void WeakenAllEnemies(Piece piece, int attackReduction)
        {
            foreach (var enemy in _board.Pieces.Where(p => p.Team != piece.Team))
            {
                enemy.Attack = Math.Max(1, enemy.Attack - attackReduction);
            }
        }

        private void ApplyStealthToAllAllies(Piece piece)
        {
            foreach (var ally in _board.Pieces.Where(p => p.Team == piece.Team))
            {
                ally.Speed += 1;
            }
        }
    }
}
