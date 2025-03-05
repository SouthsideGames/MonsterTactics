using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class AIController
    {
        private static readonly Random Random = new();
        private AIPersonalityManager personalityManager;

        public void TakeTurn(Board board, string team, int difficulty)
        {
            board.ProcessStartOfTurnEffects(team);

            if (personalityManager == null)
            {
                personalityManager = new AIPersonalityManager(board, $"{team}_AI");
            }

            var pieces = board.Pieces.Where(p => p.Team == team && p.Health > 0).ToList();

            if (pieces.Count == 0) return;

            if (difficulty == 3) // Hard - Use Monte Carlo Tree Search
            {
                var bestMove = MonteCarloTreeSearch(board, team, 50);
                ExecuteMove(board, bestMove);
            }
            else // Easy & Medium - Rule-based with Alpha-Beta for Medium
            {
                var bestMove = EvaluateBestAction(board, pieces, team, difficulty);
                ExecuteMove(board, bestMove);
            }
        }

        private (Piece piece, string move, string ability) EvaluateBestAction(Board board, List<Piece> pieces, string team, int difficulty)
        {
            (Piece piece, string move, string ability) bestAction = (null, null, null);
            int bestScore = int.MinValue;

            foreach (var piece in pieces)
            {
                var legalMoves = MovementValidator.GetLegalMoves(piece, board.Pieces);

                foreach (var move in legalMoves)
                {
                    int moveScore = EvaluateMove(board, piece, move, team, difficulty);
                    if (moveScore > bestScore)
                    {
                        bestScore = moveScore;
                        bestAction = (piece, move, null);
                    }
                }

                if (piece.Energy > 0 && !string.IsNullOrEmpty(piece.Ability))
                {
                    int abilityScore = EvaluateAbility(board, piece, team);
                    if (abilityScore > bestScore)
                    {
                        bestScore = abilityScore;
                        bestAction = (piece, null, piece.Ability);
                    }
                }
            }

            return bestAction;
        }

        private int EvaluateMove(Board board, Piece piece, string move, string team, int difficulty)
        {
            int score = 0;

            var simulatedBoard = board.Clone();
            var simulatedPiece = simulatedBoard.Pieces.First(p => p.Id == piece.Id);
            simulatedBoard.MovePiece(simulatedPiece, move);

            if (difficulty == 2) // Alpha-Beta for Medium
            {
                score += AlphaBeta(simulatedBoard, 2, int.MinValue, int.MaxValue, team == "Player");
            }

            score += personalityManager.EvaluateMoveBonus(piece, move);

            return score;
        }

        private int AlphaBeta(Board board, int depth, int alpha, int beta, bool maximizing)
        {
            if (depth == 0 || board.CheckWinCondition(out _))
            {
                return EvaluateBoardState(board);
            }

            var pieces = board.Pieces.Where(p => p.Health > 0).ToList();
            int value;

            if (maximizing)
            {
                value = int.MinValue;
                foreach (var piece in pieces.Where(p => p.Team == "Player"))
                {
                    foreach (var move in MovementValidator.GetLegalMoves(piece, board.Pieces))
                    {
                        var clone = board.Clone();
                        clone.MovePiece(clone.Pieces.First(p => p.Id == piece.Id), move);
                        value = Math.Max(value, AlphaBeta(clone, depth - 1, alpha, beta, false));
                        alpha = Math.Max(alpha, value);
                        if (alpha >= beta) break;
                    }
                }
            }
            else
            {
                value = int.MaxValue;
                foreach (var piece in pieces.Where(p => p.Team == "AI"))
                {
                    foreach (var move in MovementValidator.GetLegalMoves(piece, board.Pieces))
                    {
                        var clone = board.Clone();
                        clone.MovePiece(clone.Pieces.First(p => p.Id == piece.Id), move);
                        value = Math.Min(value, AlphaBeta(clone, depth - 1, alpha, beta, true));
                        beta = Math.Min(beta, value);
                        if (alpha >= beta) break;
                    }
                }
            }

            return value;
        }

        private int EvaluateBoardState(Board board)
        {
            int score = 0;
            foreach (var piece in board.Pieces)
            {
                if (piece.Team == "Player") score += piece.Health + piece.Attack;
                if (piece.Team == "AI") score -= piece.Health + piece.Attack;
            }
            return score;
        }

        private (Piece piece, string move, string ability) MonteCarloTreeSearch(Board board, string team, int simulations)
        {
            var legalMoves = board.Pieces
                .Where(p => p.Team == team && p.Health > 0)
                .SelectMany(p => MovementValidator.GetLegalMoves(p, board.Pieces)
                    .Select(move => (piece: p, move)))
                .ToList();

            var winRates = new Dictionary<(Piece piece, string move), double>();

            foreach (var (piece, move) in legalMoves)
            {
                int wins = 0;

                for (int i = 0; i < simulations; i++)
                {
                    var simulatedBoard = board.Clone();
                    var simulatedPiece = simulatedBoard.Pieces.First(p => p.Id == piece.Id);
                    simulatedBoard.MovePiece(simulatedPiece, move);

                    string winner = SimulateRandomGame(simulatedBoard);
                    if (winner.Contains(team)) wins++;
                }

                winRates[(piece, move)] = (double)wins / simulations;
            }

            var best = winRates.OrderByDescending(w => w.Value).Select(w => w.Key).FirstOrDefault();

            // Return as (piece, move, ability) with null ability since MCTS doesn't handle abilities
            return (best.piece, best.move, null);
        }

        private string SimulateRandomGame(Board board)
        {
            string winner = "";

            while (!board.CheckWinCondition(out winner))
            {
                var pieces = board.Pieces.Where(p => p.Health > 0).ToList();
                var piece = pieces[Random.Next(pieces.Count)];
                var moves = MovementValidator.GetLegalMoves(piece, board.Pieces);

                if (moves.Count > 0)
                {
                    var move = moves[Random.Next(moves.Count)];
                    board.MovePiece(piece, move);
                }
            }

            return winner;
        }

        private int EvaluateAbility(Board board, Piece piece, string team)
        {
            int score = 0;

            if (piece.Ability.Contains("Damage"))
            {
                var nearbyEnemies = board.Pieces.Count(p => p.Team != team && board.IsAdjacent(piece, p));
                score += nearbyEnemies * 10;
            }

            return score;
        }

        private void ExecuteMove(Board board, (Piece piece, string move, string ability) action)
        {
            if (action.ability != null)
            {
                board.AbilityManager.UseAbility(action.piece, action.ability);
            }
            else if (action.move != null)
            {
                board.LogTurn($"{action.piece.Team} {action.piece.Id} moved from {action.piece.Position} to {action.move}");
                board.MovePiece(action.piece, action.move);
            }
        }
    }
}
