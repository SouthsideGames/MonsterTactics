using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public static class PieceInfoManager
    {
        public static void ShowPieceInfo()
        {
            while (true)
            {
                Console.WriteLine("\n=== Piece Information ===");
                Console.WriteLine("1 - Pawns");
                Console.WriteLine("2 - Knights");
                Console.WriteLine("3 - Bishops");
                Console.WriteLine("4 - Rooks");
                Console.WriteLine("5 - Queens");
                Console.WriteLine("6 - Kings");
                Console.WriteLine("7 - Back to Main Menu");

                string choice = Console.ReadLine()?.Trim().ToLower();
                if (choice == "quit") Environment.Exit(0);
                if (choice == "7") return;

                string typeFilter = choice switch
                {
                    "1" => "Pawn",
                    "2" => "Knight",
                    "3" => "Bishop",
                    "4" => "Rook",
                    "5" => "Queen",
                    "6" => "King",
                    _ => null
                };

                if (typeFilter == null)
                {
                    Console.WriteLine("Invalid selection. Please try again.");
                    continue;
                }

                List<string> matchingPieces = MonsterDatabase.PieceTemplates
                    .Where(kvp => kvp.Value.Type == typeFilter)
                    .Select(kvp => kvp.Key)
                    .ToList();

                if (matchingPieces.Count == 0)
                {
                    Console.WriteLine($"No {typeFilter}s found.");
                    continue;
                }

                Console.WriteLine($"\n=== {typeFilter}s ===");
                for (int i = 0; i < matchingPieces.Count; i++)
                {
                    Console.WriteLine($"{i + 1} - {matchingPieces[i]}");
                }

                Console.WriteLine("\nType the number of the piece to view details, or type 'back' to return.");

                string pieceChoice = Console.ReadLine()?.Trim();
                if (pieceChoice?.ToLower() == "quit") Environment.Exit(0);
                if (pieceChoice?.ToLower() == "back") continue;

                if (int.TryParse(pieceChoice, out int pieceIndex) &&
                    pieceIndex >= 1 && pieceIndex <= matchingPieces.Count)
                {
                    string selectedPieceId = matchingPieces[pieceIndex - 1];
                    if (MonsterDatabase.PieceTemplates.TryGetValue(selectedPieceId, out Piece piece))
                    {
                        ShowPieceDetails(piece);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid selection. Please try again.");
                }
            }
        }

        private static void ShowPieceDetails(Piece piece)
        {
            Console.WriteLine($"\n=== Details for {piece.Id} ===");
            Console.WriteLine($"Type: {piece.Type}");
            Console.WriteLine($"Health: {piece.Health}");
            Console.WriteLine($"Attack: {piece.Attack}");
            Console.WriteLine($"Defense: {piece.Defense}");
            Console.WriteLine($"Speed: {piece.Speed}");
            Console.WriteLine($"Special Ability: {piece.Ability}");
            Console.WriteLine($"Passive Ability: {piece.Passive}");
            Console.WriteLine($"Pack: {piece.Pack}");

            if (MonsterDatabase.EvolutionChains.TryGetValue(piece.Id, out var evolutions))
            {
                Console.WriteLine("Evolution Path:");
                foreach (var evolution in evolutions)
                {
                    Console.WriteLine($"- Level {evolution.Level}: {evolution.EvolvedForm}");
                }
            }

            Console.WriteLine("==================================");
        }

        private static void ShowAbility(string label, string abilityName)
        {
            if (string.IsNullOrEmpty(abilityName) || abilityName == "None")
            {
                Console.WriteLine($"{label}: None");
                return;
            }

            string description = MonsterDatabase.AbilityDescriptions.ContainsKey(abilityName)
                ? MonsterDatabase.AbilityDescriptions[abilityName]
                : "No description available.";

            Console.WriteLine($"{label}: {abilityName} - {description}");
        }   
    }
}
