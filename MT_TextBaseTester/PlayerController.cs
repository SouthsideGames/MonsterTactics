using System;

namespace ChessMonsterTactics
{
    public class PlayerController
    {
        public void TakeTurn(Board board)
        {
            while (true)
            {
                Console.WriteLine("Your turn. Enter piece type and position (e.g., Pawn at A1), or type 'boardstate' to view the board:");
                string input = Console.ReadLine()?.Trim();

                if (input?.ToLower() == "quit") 
                    Environment.Exit(0);

                if (input?.ToLower() == "boardstate")
                {
                    board.Renderer.DrawBoard();
                    continue;  // Show the board, then loop back to asking for a piece.
                }

                string[] parts = input.Split(" at ");
                if (parts.Length != 2)
                {
                    Console.WriteLine("Invalid format. Please enter in the format: PieceType at Position (e.g., Pawn at A1).");
                    continue;
                }

                string pieceType = parts[0].Trim();
                string position = parts[1].Trim();

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
                if (action?.ToLower() == "quit")
                    Environment.Exit(0);

                if (action == "1")
                {
                    Console.WriteLine($"Where do you want to move {piece.Id}?");

                    string newPosition = Console.ReadLine()?.Trim();
                    if (newPosition?.ToLower() == "quit")
                        Environment.Exit(0);

                    if (!MovementValidator.IsMoveLegal(piece, newPosition, board.Pieces))
                    {
                        Console.WriteLine("❌ Illegal move! Please try again.");
                        continue;
                    }

                    board.MovePiece(piece, newPosition);
                    board.LogTurn($"{piece.Team} {piece.Id} moved to {newPosition}");
                    break;  // Move ends the turn
                }
                else if (action == "2")
                {
                    UseAbility(piece, board);
                    break;  // Ability use ends the turn
                }
                else
                {
                    Console.WriteLine("Invalid action. Please enter 1 (Move) or 2 (Use Ability).");
                }
            }
        }

        private void UseAbility(Piece piece, Board board)
        {
            if (string.IsNullOrEmpty(piece.Ability) || piece.Ability == "None")
            {
                Console.WriteLine($"{piece.Id} has no active ability.");
                return;
            }

            string abilityName = piece.Ability.Split('–')[0].Trim();

            if (!MonsterDatabase.AbilityCosts.TryGetValue(abilityName, out int abilityCost))
            {
                Console.WriteLine($"Unknown ability: {abilityName}");
                return;
            }

            int adjustedCost = Math.Max(1, abilityCost + piece.EnergyCostModifier);  // <-- Place here
            if (piece.Energy < adjustedCost)
            {
                Console.WriteLine($"{piece.Id} does not have enough energy to use {abilityName} (cost: {adjustedCost}).");
                return;
            }

            piece.Energy -= adjustedCost;

            if (MonsterDatabase.AbilityDescriptions.TryGetValue(abilityName, out string description))
            {
                Console.WriteLine($"{piece.Id} uses {abilityName}: {description}");
            }
            else
            {
                Console.WriteLine($"{piece.Id} uses {abilityName}!");
            }

            board.LogTurn($"{piece.Team} {piece.Id} used {abilityName}");
            board.ApplyAbilityEffect(piece, abilityName);
        }
    }
}
