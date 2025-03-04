using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class AIController
    {
        private static Random random = new Random();

        public void TakeTurn(Board board, string team)
        {
            board.ProcessStartOfTurnEffects(team);

            var pieces = board.Pieces.Where(p => p.Team == team && p.Health > 0).ToList();
            if (pieces.Count == 0) return;

            var bestAction = EvaluateBestAction(board, pieces, team);
            ExecuteAction(board, bestAction);
        }

        private (Piece piece, string move, string ability) EvaluateBestAction(Board board, List<Piece> pieces, string team)
        {
            var bestAction = (piece: (Piece)null, move: (string)null, ability: (string)null);
            int highestScore = int.MinValue;

            foreach (var piece in pieces)
            {
                var legalMoves = MovementValidator.GetLegalMoves(piece, board.Pieces);
                foreach (var move in legalMoves)
                {
                    int score = EvaluateMove(board, piece, move, team);
                    if (score > highestScore)
                    {
                        highestScore = score;
                        bestAction = (piece, move, null);
                    }
                }

                if (CanUseAbility(piece, board))
                {
                    int abilityScore = EvaluateAbility(board, piece, team);
                    if (abilityScore > highestScore)
                    {
                        highestScore = abilityScore;
                        bestAction = (piece, null, piece.Ability);
                    }
                }
            }

            return bestAction;
        }

        private int EvaluateMove(Board board, Piece piece, string targetPosition, string team)
        {
            int score = 0;

            // Check if capturing an enemy piece
            var targetPiece = board.Pieces.FirstOrDefault(p => p.Position == targetPosition && p.Team != team);
            if (targetPiece != null)
            {
                score += 10 + (targetPiece.Health / 2); // Higher score for capturing higher HP pieces
            }

            // Check if moving into threatened tile
            if (board.IsTileUnderThreat(targetPosition, team))
            {
                score -= 5;  // Penalize moving into danger
            }
            else
            {
                score += 2;  // Safe move bonus
            }

            // Synergy Bonus - Staying near pack allies
            if (board.Pieces.Any(p => p.Team == team && p.Pack == piece.Pack && p.Position != piece.Position && board.IsAdjacentToPosition(p.Position, targetPosition)))
            {
                score += 3;
            }

            // King Pressure - Moving adjacent to enemy king
            var enemyKing = board.Pieces.FirstOrDefault(p => p.Team != team && p.Type == "King");
            if (enemyKing != null && board.IsAdjacentToPosition(targetPosition, enemyKing.Position))
            {
                score += 5; // Extra pressure bonus
            }

            return score;
        }

        private int EvaluateAbility(Board board, Piece piece, string team)
        {
            if (string.IsNullOrEmpty(piece.Ability)) return 0;

            int score = 0;
            if (piece.Ability.Contains("Damage") || piece.Ability.Contains("Pulse"))
            {
                var nearbyEnemies = board.Pieces
                    .Where(p => p.Team != team && board.IsAdjacent(piece, p)).ToList();

                score += nearbyEnemies.Count * 10; // Bonus for hitting multiple enemies
            }

            return score;
        }

        private bool CanUseAbility(Piece piece, Board board)
        {
            if (string.IsNullOrEmpty(piece.Ability)) return false;

            string abilityName = piece.Ability.Split('–')[0].Trim();
            if (MonsterDatabase.AbilityCosts.TryGetValue(abilityName, out int cost))
            {
                return piece.Energy >= cost;
            }

            return false;
        }

        private void ExecuteAction(Board board, (Piece piece, string move, string ability) action)
        {
            if (action.ability != null)
            {
                UseAbility(action.piece, board);
            }
            else if (action.move != null)
            {
                board.LogTurn($"{action.piece.Team} {action.piece.Id} moved from {action.piece.Position} to {action.move}");
                action.piece.Position = action.move;
            }
        }

        private void UseAbility(Piece piece, Board board)
        {
            string abilityName = piece.Ability.Split('–')[0].Trim();

            if (!MonsterDatabase.AbilityCosts.TryGetValue(abilityName, out int cost))
                return;

            if (piece.Energy < cost) return;

            piece.Energy -= cost;

            if (MonsterDatabase.AbilityDescriptions.TryGetValue(abilityName, out string description))
            {
                Console.WriteLine($"{piece.Team} {piece.Id} uses {abilityName}: {description}");
            }
            else
            {
                Console.WriteLine($"{piece.Team} {piece.Id} uses {abilityName}!");
            }

            board.LogTurn($"{piece.Team} {piece.Id} used {abilityName}");
            board.ApplyAbilityEffect(piece, abilityName);
        }
    }
}
