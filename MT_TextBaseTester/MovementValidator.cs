using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public static class MovementValidator
    {
        private static readonly int BoardSize = 8;

        private static bool IsWithinBounds(int row, int col)
        {
            return row >= 0 && row < BoardSize && col >= 0 && col < BoardSize;
        }

        public static string CoordinatesToPosition(int row, int col)
        {
            if (!IsWithinBounds(row, col))
                throw new ArgumentOutOfRangeException($"Invalid coordinates ({row}, {col}).");

            char file = (char)('A' + col);
            return $"{file}{row + 1}";
        }

        public static (int row, int col) PositionToCoordinates(string position)
        {
            char file = position[0];
            int rank = position[1] - '0';
            int row = rank - 1;
            int col = file - 'A';
            return (row, col);
        }

        public static bool IsMoveLegal(Piece piece, string targetPosition, List<Piece> allPieces)
        {
            return GetLegalMoves(piece, allPieces).Contains(targetPosition);
        }

        public static List<string> GetLegalMoves(Piece piece, List<Piece> allPieces)
        {
            var moves = new List<string>();
            (int startRow, int startCol) = PositionToCoordinates(piece.Position);

            switch (piece.Type)
            {
                case "Pawn":
                    AddPawnMoves(piece, startRow, startCol, moves, allPieces);
                    break;
                case "Knight":
                    AddKnightMoves(piece, startRow, startCol, moves, allPieces);
                    break;
                case "Bishop":
                    AddSlidingMoves(startRow, startCol, moves, allPieces, diagonals: true, straight: false, piece);
                    break;
                case "Rook":
                    AddSlidingMoves(startRow, startCol, moves, allPieces, diagonals: false, straight: true, piece);
                    break;
                case "Queen":
                    AddSlidingMoves(startRow, startCol, moves, allPieces, diagonals: true, straight: true, piece);
                    break;
                case "King":
                    AddKingMoves(startRow, startCol, moves, allPieces, piece);
                    break;
            }

            return moves;
        }

        private static void AddPawnMoves(Piece piece, int row, int col, List<string> moves, List<Piece> allPieces)
        {
            int direction = piece.Team == "Player" ? 1 : -1;

            // Forward move (only if not blocked)
            if (IsTileEmpty(row + direction, col, allPieces))
            {
                moves.Add(CoordinatesToPosition(row + direction, col));
            }

            // Diagonal captures (only if enemy exists there)
            TryAddCaptureMove(row + direction, col - 1, moves, piece, allPieces);
            TryAddCaptureMove(row + direction, col + 1, moves, piece, allPieces);
        }

        private static void AddKnightMoves(Piece piece, int row, int col, List<string> moves, List<Piece> allPieces)
        {
            int[][] offsets = {
                new[] { 2, 1 }, new[] { 2, -1 },
                new[] { -2, 1 }, new[] { -2, -1 },
                new[] { 1, 2 }, new[] { 1, -2 },
                new[] { -1, 2 }, new[] { -1, -2 }
            };

            foreach (var offset in offsets)
            {
                TryAddMoveOrCapture(row + offset[0], col + offset[1], moves, piece, allPieces);
            }
        }

        private static void AddSlidingMoves(int row, int col, List<string> moves, List<Piece> allPieces, bool diagonals, bool straight, Piece piece)
        {
            int[][] directions = diagonals
                ? new[] { new[] { 1, 1 }, new[] { 1, -1 }, new[] { -1, 1 }, new[] { -1, -1 } }
                : new int[0][];
            
            if (straight)
            {
                directions = directions.Concat(new[] { new[] { 1, 0 }, new[] { -1, 0 }, new[] { 0, 1 }, new[] { 0, -1 } }).ToArray();
            }

            foreach (var dir in directions)
            {
                for (int i = 1; i < BoardSize; i++)
                {
                    int targetRow = row + dir[0] * i;
                    int targetCol = col + dir[1] * i;

                    if (!IsWithinBounds(targetRow, targetCol))
                        break;

                    string pos = CoordinatesToPosition(targetRow, targetCol);
                    var blockingPiece = allPieces.FirstOrDefault(p => p.Position == pos);

                    if (blockingPiece == null)
                    {
                        moves.Add(pos);
                    }
                    else
                    {
                        if (blockingPiece.Team != piece.Team)
                        {
                            moves.Add(pos); // Capture enemy
                        }
                        break; // Friendly or enemy piece blocks further movement
                    }
                }
            }
        }

        private static void AddKingMoves(int row, int col, List<string> moves, List<Piece> allPieces, Piece piece)
        {
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    TryAddMoveOrCapture(row + dr, col + dc, moves, piece, allPieces);
                }
            }
        }

        private static void TryAddMoveOrCapture(int row, int col, List<string> moves, Piece piece, List<Piece> allPieces)
        {
            if (!IsWithinBounds(row, col)) return;

            string pos = CoordinatesToPosition(row, col);
            var targetPiece = allPieces.FirstOrDefault(p => p.Position == pos);

            if (targetPiece == null || targetPiece.Team != piece.Team)
            {
                moves.Add(pos);
            }
        }

        private static void TryAddCaptureMove(int row, int col, List<string> moves, Piece piece, List<Piece> allPieces)
        {
            if (!IsWithinBounds(row, col)) return;

            string pos = CoordinatesToPosition(row, col);
            var targetPiece = allPieces.FirstOrDefault(p => p.Position == pos);

            if (targetPiece != null && targetPiece.Team != piece.Team)
            {
                moves.Add(pos);
            }
        }

        private static bool IsTileEmpty(int row, int col, List<Piece> allPieces)
        {
            if (!IsWithinBounds(row, col)) return false;

            string pos = CoordinatesToPosition(row, col);
            return !allPieces.Any(p => p.Position == pos);
        }
    }
}
