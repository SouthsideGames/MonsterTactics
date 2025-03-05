using System;

namespace ChessMonsterTactics
{
    public class CombatManager
    {
        private readonly Board _board;

        public CombatManager(Board board)
        {
            _board = board;
        }

        public void DamagePiece(Piece attacker, Piece target, int damage)
        {
            target.Health -= damage;
            attacker.TotalDamageDealt += damage;

            _board.LogTurn($"{attacker.Team} {attacker.Id} dealt {damage} damage to {target.Team} {target.Id}.");

            if (target.Health <= 0)
            {
                target.Health = 0;
                attacker.TotalKills++;
                _board.LogTurn($"{target.Team} {target.Id} was eliminated by {attacker.Team} {attacker.Id}.");
            }
        }

        public void HealPiece(Piece healer, Piece target, int healAmount)
        {
            target.Health += healAmount;
            _board.LogTurn($"{healer.Team} {healer.Id} healed {target.Team} {target.Id} for {healAmount} HP.");
        }

        public void ApplyStatusEffect(Piece target, string statusEffect)
        {
            _board.LogTurn($"{target.Team} {target.Id} is now affected by: {statusEffect}.");
            // This is where you could expand for future effects like stunned, silenced, etc.
        }
    }
}
