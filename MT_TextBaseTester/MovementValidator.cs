using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public static class MovementValidator
    {
        private static readonly Dictionary<string, (int, int)> PositionToCoordinates = GeneratePositionMap();

        public static bool IsMoveLegal(Piece piece, string targetPosition, List<Piece> allPieces)
        {
            var (startRow, startCol) = PositionToCoordinates[piece.Position];
            var (targetRow, targetCol) = PositionToCoordinates[targetPosition];

            switch (piece.Type)
            {
                case "King": return IsKingMoveLegal(startRow, startCol, targetRow, targetCol);
                case "Queen": return IsStraightOrDiagonalMoveLegal(startRow, startCol, targetRow, targetCol, allPieces);
                case "Rook": return IsStraightMoveLegal(startRow, startCol, targetRow, targetCol, allPieces);
                case "Bishop": return IsDiagonalMoveLegal(startRow, startCol, targetRow, targetCol, allPieces);
                case "Knight": return IsKnightMoveLegal(startRow, startCol, targetRow, targetCol);
                case "Pawn": return IsPawnMoveLegal(piece, startRow, startCol, targetRow, targetCol, allPieces);
                default: return false;
            }
        }

        public static List<string> GetLegalMoves(Piece piece, List<Piece> allPieces)
        {
            var legalMoves = new List<string>();

            foreach (var position in PositionToCoordinates.Keys)
            {
                if (IsMoveLegal(piece, position, allPieces))
                {
                    legalMoves.Add(position);
                }
            }

            return legalMoves;
        }

        private static bool IsKingMoveLegal(int startRow, int startCol, int targetRow, int targetCol)
        {
            return Math.Abs(startRow - targetRow) <= 1 && Math.Abs(startCol - targetCol) <= 1;
        }

        private static bool IsKnightMoveLegal(int startRow, int startCol, int targetRow, int targetCol)
        {
            int rowDiff = Math.Abs(startRow - targetRow);
            int colDiff = Math.Abs(startCol - targetCol);
            return (rowDiff == 2 && colDiff == 1) || (rowDiff == 1 && colDiff == 2);
        }

        private static bool IsStraightOrDiagonalMoveLegal(int startRow, int startCol, int targetRow, int targetCol, List<Piece> allPieces)
        {
            return IsStraightMoveLegal(startRow, startCol, targetRow, targetCol, allPieces) ||
                   IsDiagonalMoveLegal(startRow, startCol, targetRow, targetCol, allPieces);
        }

        private static bool IsStraightMoveLegal(int startRow, int startCol, int targetRow, int targetCol, List<Piece> allPieces)
        {
            if (startRow == targetRow)
            {
                int step = startCol < targetCol ? 1 : -1;
                for (int col = startCol + step; col != targetCol; col += step)
                {
                    if (PieceAtPosition(startRow, col, allPieces) != null) return false;
                }
                return true;
            }

            if (startCol == targetCol)
            {
                int step = startRow < targetRow ? 1 : -1;
                for (int row = startRow + step; row != targetRow; row += step)
                {
                    if (PieceAtPosition(row, startCol, allPieces) != null) return false;
                }
                return true;
            }

            return false;
        }

        private static bool IsDiagonalMoveLegal(int startRow, int startCol, int targetRow, int targetCol, List<Piece> allPieces)
        {
            int rowDiff = Math.Abs(startRow - targetRow);
            int colDiff = Math.Abs(startCol - targetCol);
            if (rowDiff != colDiff) return false;

            int rowStep = (targetRow > startRow) ? 1 : -1;
            int colStep = (targetCol > startCol) ? 1 : -1;

            for (int i = 1; i < rowDiff; i++)
            {
                if (PieceAtPosition(startRow + i * rowStep, startCol + i * colStep, allPieces) != null) return false;
            }

            return true;
        }

        private static bool IsPawnMoveLegal(Piece pawn, int startRow, int startCol, int targetRow, int targetCol, List<Piece> allPieces)
        {
            int direction = (pawn.Team == "Player") ? 1 : -1;
            int rowDiff = targetRow - startRow;
            int colDiff = Math.Abs(startCol - targetCol);

            if (colDiff == 0)  // Forward movement
            {
                if (rowDiff == direction && PieceAtPosition(targetRow, targetCol, allPieces) == null)
                    return true;

                if ((startRow == 2 && pawn.Team == "Player") || (startRow == 7 && pawn.Team == "AI"))
                {
                    if (rowDiff == 2 * direction &&
                        PieceAtPosition(startRow + direction, startCol, allPieces) == null &&
                        PieceAtPosition(targetRow, targetCol, allPieces) == null)
                    {
                        return true;
                    }
                }
            }
            else if (colDiff == 1 && rowDiff == direction)
            {
                var targetPiece = PieceAtPosition(targetRow, targetCol, allPieces);
                if (targetPiece != null && targetPiece.Team != pawn.Team)
                    return true;
            }

            return false;
        }

        private static Piece PieceAtPosition(int row, int col, List<Piece> allPieces)
        {
            string position = CoordinatesToPosition(row, col);
            return allPieces.FirstOrDefault(p => p.Position == position);
        }

        private static Dictionary<string, (int, int)> GeneratePositionMap()
        {
            var map = new Dictionary<string, (int, int)>();
            string files = "ABCDEFGH";

            for (int row = 1; row <= 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    string pos = $"{files[col]}{row}";
                    map[pos] = (row, col + 1);
                }
            }
            return map;
        }

        private static string CoordinatesToPosition(int row, int col)
        {
            string files = "ABCDEFGH";
            return $"{files[col - 1]}{row}";
        }

        
    }
}
