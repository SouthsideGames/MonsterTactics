using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class AIController
    {
        private static readonly Random Random = new();
        private AIPersonalityManager personalityManager;

        public void TakeTurn(Board board, string team, int difficulty, AIPersonalityType? overridePersonality = null)
        {
            board.ProcessStartOfTurnEffects(team);

            if (personalityManager == null)
            {
                var personality = overridePersonality ?? MonsterDatabase.PackPersonalities[board.GetAIPack()];
                personalityManager = new AIPersonalityManager(board, $"{team}_AI", personality);
            }

            var pieces = board.Pieces.Where(p => p.Team == team && p.Health > 0).ToList();
            if (pieces.Count == 0) return;

            var bestMove = EvaluateBestMove(board, pieces, team, difficulty);
            ExecuteMoveAndPostActions(board, bestMove);
        }

        private (Piece piece, string move) EvaluateBestMove(Board board, List<Piece> pieces, string team, int difficulty)
        {
            var bestMove = (piece: (Piece)null, move: (string)null);
            int bestScore = int.MinValue;

            foreach (var piece in pieces)
            {
                var legalMoves = MovementValidator.GetLegalMoves(piece, board.Pieces);

                foreach (var move in legalMoves)
                {
                    int score = EvaluateMove(board, piece, move, team, difficulty);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = (piece, move);
                    }
                }
            }

            return bestMove;
        }

        private int EvaluateMove(Board board, Piece piece, string move, string team, int difficulty)
        {
            var simulatedBoard = board.Clone();
            var simulatedPiece = simulatedBoard.Pieces.First(p => p.Id == piece.Id);
            simulatedBoard.MovePiece(simulatedPiece, move);

            int score = personalityManager.EvaluateMoveBonus(piece, move);

            if (difficulty == 2)
            {
                score += AlphaBeta(simulatedBoard, 2, int.MinValue, int.MaxValue, team == "Player");
            }

            return score;
        }

        private int AlphaBeta(Board board, int depth, int alpha, int beta, bool maximizing)
        {
            if (depth == 0 || board.CheckWinCondition(out _))
            {
                return EvaluateBoardState(board);
            }

            var pieces = board.Pieces.Where(p => p.Health > 0).ToList();
            int value = maximizing ? int.MinValue : int.MaxValue;

            foreach (var piece in pieces.Where(p => p.Team == (maximizing ? "Player" : "AI")))
            {
                foreach (var move in MovementValidator.GetLegalMoves(piece, board.Pieces))
                {
                    var clone = board.Clone();
                    clone.MovePiece(clone.Pieces.First(p => p.Id == piece.Id), move);

                    int childValue = AlphaBeta(clone, depth - 1, alpha, beta, !maximizing);

                    if (maximizing)
                    {
                        value = Math.Max(value, childValue);
                        alpha = Math.Max(alpha, value);
                    }
                    else
                    {
                        value = Math.Min(value, childValue);
                        beta = Math.Min(beta, value);
                    }

                    if (alpha >= beta) return value;
                }
            }

            return value;
        }

        private int EvaluateBoardState(Board board)
        {
            int score = 0;
            foreach (var piece in board.Pieces)
            {
                int pieceValue = piece.Health + piece.Attack;
                score += piece.Team == "Player" ? pieceValue : -pieceValue;
            }
            return score;
        }

        private void ExecuteMoveAndPostActions(Board board, (Piece piece, string move) action)
        {
            if (action.piece == null || action.move == null)
            {
                board.LogTurn("AI found no valid move.");
                return;
            }

            board.LogTurn($"{action.piece.Team} {action.piece.Id} moves from {action.piece.Position} to {action.move}");
            board.MovePiece(action.piece, action.move);

            // Post-move actions
            board.ApplyTileEffectsToAllPieces();
            board.CheckPawnPromotion(action.piece);
            TriggerPostMoveActions(board, action.piece);
        }

        private void TriggerPostMoveActions(Board board, Piece piece)
        {
            var adjacentEnemies = board.Pieces
                .Where(p => p.Team != piece.Team && p.Health > 0 && board.IsAdjacent(piece, p))
                .ToList();

            if (adjacentEnemies.Any())
            {
                var target = adjacentEnemies[Random.Next(adjacentEnemies.Count)];
                board.CombatManager.DamagePiece(piece, target, piece.Attack);
            }

            // Auto-trigger ability (no movement abilities allowed)
            if (piece.Energy > 0 && !string.IsNullOrEmpty(piece.Ability))
            {
                if (!AbilityManager.AbilityInvolvesMovement(piece.Ability))
                {
                    board.AbilityManager.UseAbility(piece, piece.Ability);
                }
            }
        }

  
    }
}
