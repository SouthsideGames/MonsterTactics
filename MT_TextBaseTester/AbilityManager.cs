using System;
using System.Collections.Generic;

namespace ChessMonsterTactics
{
    public class AbilityManager
    {
        private readonly Board _board;

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

            if (piece.Energy < cost)
            {
                _board.LogTurn($"{piece.Team} {piece.Id} does not have enough energy to use {abilityName}.");
                return;
            }

            piece.Energy -= cost;

            _board.LogTurn($"{piece.Team} {piece.Id} uses {abilityName}!");

            ApplyAbilityEffect(piece, abilityName);
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
                    _board.LogTurn($"{piece.Team} {piece.Id} activated Blitz Protocol, gaining +5 Attack!");
                    break;

                case "Overdrive Command":
                    BoostNearbyAllies(piece, 5, 2);
                    break;

                default:
                    _board.LogTurn($"{piece.Team} {piece.Id} used {abilityName}, but no effect is defined.");
                    break;
            }
        }

        private void DamageNearbyEnemy(Piece attacker, int damage)
        {
            var enemy = _board.Pieces.Find(p => p.Team != attacker.Team && _board.IsAdjacent(attacker, p));
            if (enemy != null)
            {
                _board.CombatManager.DamagePiece(attacker, enemy, damage);
                _board.LogTurn($"{attacker.Team} {attacker.Id} used an ability to deal {damage} damage to {enemy.Team} {enemy.Id}.");
            }
        }

        private void BoostNearbyAllies(Piece piece, int attackBoost, int speedBoost)
        {
            var allies = _board.Pieces.FindAll(p => p.Team == piece.Team && _board.IsAdjacent(piece, p));

            foreach (var ally in allies)
            {
                ally.Attack += attackBoost;
                ally.Speed += speedBoost;
                _board.LogTurn($"{piece.Team} {piece.Id} boosts {ally.Team} {ally.Id} with +{attackBoost} Attack and +{speedBoost} Speed.");
            }
        }
    }
}
