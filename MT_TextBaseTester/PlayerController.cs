using System;

namespace ChessMonsterTactics
{
    public class PlayerController
    {
        public void TakeTurn(Board board)
        {
            while (true)
            {
                Console.WriteLine("Your turn. Enter piece type and position (e.g., Pawn at A1) or type 'boardstate' to view the full board:");
                string input = Console.ReadLine()?.Trim();

                if (input?.ToLower() == "quit") Environment.Exit(0);

                if (input?.ToLower() == "boardstate")
                {
                    board.DisplayBoard();
                    continue;  // Show boardstate and ask for input again.
                }

                string[] parts = input.Split(" at ");
                if (parts.Length != 2)
                {
                    Console.WriteLine("Invalid format. Please enter in the format: PieceType at Position (e.g., Pawn at A1).");
                    continue;
                }

                string pieceType = parts[0];
                string position = parts[1];

                var piece = board.Pieces.Find(p => p.Team == "Player" && p.Type.Equals(pieceType, StringComparison.OrdinalIgnoreCase) && p.Position == position);
                if (piece == null)
                {
                    Console.WriteLine("Invalid piece or position. Please try again.");
                    continue;
                }

                Console.WriteLine($"Selected: {piece.Id} ({piece.Type}) at {piece.Position} (Energy: {piece.Energy})");
                Console.WriteLine("1 - Move");
                Console.WriteLine("2 - Use Ability");

                string action = Console.ReadLine()?.Trim();
                if (action?.ToLower() == "quit") Environment.Exit(0);

                if (action == "1")
                {
                    Console.WriteLine($"Where do you want to move {piece.Id}?");
                    string newPosition = Console.ReadLine()?.Trim();
                    if (newPosition?.ToLower() == "quit") Environment.Exit(0);

                    if (!MovementValidator.IsMoveLegal(piece, newPosition, board.Pieces))
                    {
                        Console.WriteLine("❌ Illegal move! Please try again.");
                        continue;
                    }

                    board.LogTurn($"{piece.Team} {piece.Id} moved from {piece.Position} to {newPosition}");
                    board.MovePiece(piece, newPosition);
                    break;
                }
                else if (action == "2")
                {
                    if (string.IsNullOrEmpty(piece.Ability) || piece.Ability == "None")
                    {
                        Console.WriteLine($"{piece.Id} has no usable ability.");
                        continue;
                    }

                    string abilityName = piece.Ability.Split('–')[0].Trim();

                    if (!MonsterDatabase.AbilityCosts.TryGetValue(abilityName, out int cost))
                    {
                        Console.WriteLine($"Unknown ability: {abilityName}");
                        continue;
                    }

                    if (piece.Energy < cost)
                    {
                        Console.WriteLine($"{piece.Id} does not have enough energy to use {abilityName}. (Requires {cost}, has {piece.Energy})");
                        continue;
                    }

                    board.AbilityManager.UseAbility(piece, abilityName);
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid action. Please enter 1 (Move) or 2 (Use Ability).");
                }
            }
        }
    }
}
