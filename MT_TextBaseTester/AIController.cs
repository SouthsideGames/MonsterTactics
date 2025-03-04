using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class AIController
    {
        private static Random random = new Random();

        

        public void TakeTurn(Board board, string team, int difficulty)
        {
            board.ProcessStartOfTurnEffects(team);

            var pieces = board.Pieces.Where(p => p.Team == team && p.Health > 0).ToList();
            if (pieces.Count == 0) return;

            foreach (var piece in pieces)
            {
                string move;

                if (difficulty < 3) // Easy/Medium → Alpha-Beta Pruning
                {
                    move = FindBestMoveAlphaBeta(board, piece, 3, int.MinValue, int.MaxValue, true);
                }
                else // Hard → Monte Carlo Tree Search (MCTS)
                {
                    move = FindBestMoveMCTS(board, piece, 100);
                }

                if (!string.IsNullOrEmpty(move))
                {
                    board.LogTurn($"{piece.Team} {piece.Id} moved from {piece.Position} to {move}");
                    piece.Position = move;
                }
                else
                {
                    board.LogTurn($"{piece.Team} {piece.Id} has no valid moves.");
                }
            }
        }

        // 🔹 Alpha-Beta Pruning for Easy & Medium AI
        private string FindBestMoveAlphaBeta(Board board, Piece piece, int depth, int alpha, int beta, bool maximizingPlayer)
        {
            var legalMoves = MovementValidator.GetLegalMoves(piece, board.Pieces);
            string bestMove = null;

            if (depth == 0 || board.CheckWinCondition(out _))
                return EvaluateBoard(board, piece.Team).ToString();

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;
                foreach (var move in legalMoves)
                {
                    var simulatedBoard = board.Clone();
                    simulatedBoard.MovePiece(piece, move);

                    int eval = int.Parse(FindBestMoveAlphaBeta(simulatedBoard, piece, depth - 1, alpha, beta, false));
                    if (eval > maxEval)
                    {
                        maxEval = eval;
                        bestMove = move;
                    }

                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha) break;
                }
                return depth == 3 ? bestMove : maxEval.ToString();
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (var move in legalMoves)
                {
                    var simulatedBoard = board.Clone();
                    simulatedBoard.MovePiece(piece, move);

                    int eval = int.Parse(FindBestMoveAlphaBeta(simulatedBoard, piece, depth - 1, alpha, beta, true));
                    if (eval < minEval)
                    {
                        minEval = eval;
                        bestMove = move;
                    }

                    beta = Math.Min(beta, eval);
                    if (beta <= alpha) break;
                }
                return depth == 3 ? bestMove : minEval.ToString();
            }
        }

        // 🔹 Monte Carlo Tree Search (MCTS) for Hard AI
        private string FindBestMoveMCTS(Board board, Piece piece, int simulations)
        {
            var legalMoves = MovementValidator.GetLegalMoves(piece, board.Pieces);
            if (legalMoves.Count == 0) return null;

            Dictionary<string, int> moveWins = new();
            Dictionary<string, int> movePlays = new();

            foreach (var move in legalMoves)
            {
                moveWins[move] = 0;
                movePlays[move] = 0;

                for (int i = 0; i < simulations; i++)
                {
                    var simulatedBoard = board.Clone();
                    simulatedBoard.MovePiece(piece, move);

                    string winner = SimulateRandomGame(simulatedBoard);
                    if (winner == piece.Team)
                        moveWins[move]++;
                    movePlays[move]++;
                }
            }

            return moveWins.OrderByDescending(kvp => (double)kvp.Value / movePlays[kvp.Key]).First().Key;
        }

        // 🔹 Simulate Random Game for MCTS
        private string SimulateRandomGame(Board board)
        {
            string winner = null;  
            while (!board.CheckWinCondition(out winner))
            {
                var pieces = board.Pieces.Where(p => p.Health > 0).ToList();
                var piece = pieces[random.Next(pieces.Count)];
                var moves = MovementValidator.GetLegalMoves(piece, board.Pieces);

                if (moves.Count > 0)
                {
                    string move = moves[random.Next(moves.Count)];
                    board.MovePiece(piece, move);
                }
            }

            return winner;
        }

        // 🔹 Basic Board Evaluation for Alpha-Beta
        private int EvaluateBoard(Board board, string team)
        {
            int score = 0;
            foreach (var piece in board.Pieces)
            {
                if (piece.Team == team)
                {
                    score += piece.Health + piece.Attack * 2;
                }
                else
                {
                    score -= piece.Health + piece.Attack * 2;
                }
            }
            return score;
        }

        private void UseAbility(Piece piece, Board board)
        {
            string abilityName = piece.Ability.Split('–')[0].Trim();

            if (!MonsterDatabase.AbilityCosts.TryGetValue(abilityName, out int abilityCost))
                return;

            int adjustedCost = Math.Max(1, abilityCost + piece.EnergyCostModifier);  // <-- Place here
            if (piece.Energy < adjustedCost) return;

            piece.Energy -= adjustedCost;

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
