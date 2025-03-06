using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public static class TeamManager
    {
        /// <summary>
        /// Prompts the player to manually select their team by choosing from available pieces for each required slot.
        /// This method enforces 8 pawns, 2 knights, 2 bishops, 2 rooks, 1 queen, and 1 king.
        /// </summary>
        public static List<Piece> PlayerCreateTeam(string team)
        {
            var selectedPieces = new List<Piece>();

            string[] pieceTypes = 
            { 
                "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn", "Pawn",
                "Knight", "Knight", 
                "Bishop", "Bishop", 
                "Rook", "Rook", 
                "Queen", 
                "King" 
            };

            Console.WriteLine($"\nCreating team for: {team}");
            foreach (var type in pieceTypes)
            {
                Console.WriteLine($"\nSelect your {type}:");

                var availablePieces = MonsterDatabase.PieceTemplates.Values
                    .Where(p => p.Type == type)
                    .ToList();

                for (int i = 0; i < availablePieces.Count; i++)
                {
                    var piece = availablePieces[i];
                    Console.WriteLine($"{i + 1}: {piece.Id} ({piece.Pack})");
                }

                int choice = GetValidChoice(1, availablePieces.Count) - 1;

                var selectedPiece = availablePieces[choice].Clone();
                selectedPiece.Team = team;
                selectedPiece.Position = AssignStartingPosition(selectedPieces, selectedPiece, team); // Assign initial positions
                selectedPieces.Add(selectedPiece);
            }

            Console.WriteLine("\nTeam successfully created!");
            return selectedPieces;
        }

        /// <summary>
        /// Ensures the player selects a valid option within a given range.
        /// </summary>
        private static int GetValidChoice(int min, int max)
        {
            while (true)
            {
                string input = Console.ReadLine()?.Trim();
                if (int.TryParse(input, out int choice) && choice >= min && choice <= max)
                {
                    return choice;
                }
                Console.WriteLine($"Invalid choice. Please enter a number between {min} and {max}.");
            }
        }

        /// <summary>
        /// Assigns appropriate starting positions based on type and previously placed pieces.
        /// </summary>
        private static string AssignStartingPosition(List<Piece> existingPieces, Piece piece, string team)
        {
            string rank = team == "Player" ? "1" : "8";
            string pawnRank = team == "Player" ? "2" : "7";

            int count = existingPieces.Count(p => p.Type == piece.Type);

            return piece.Type switch
            {
                "Pawn" => $"{(char)('A' + count)}{pawnRank}",
                "Rook" => count == 0 ? $"A{rank}" : $"H{rank}",
                "Knight" => count == 0 ? $"B{rank}" : $"G{rank}",
                "Bishop" => count == 0 ? $"C{rank}" : $"F{rank}",
                "Queen" => $"D{rank}",
                "King" => $"E{rank}",
                _ => "A1"
            };
        }
    }
}
