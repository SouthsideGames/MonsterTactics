using System;
using System.Collections.Generic;

namespace ChessMonsterTactics
{
    public class BoardRenderer
    {
        public Dictionary<string, Tile> TileMap { get; private set; } = new();

        public void Initialize()
        {
            TileMap.Clear();
            for (char file = 'A'; file <= 'H'; file++)
            {
                for (int rank = 1; rank <= 8; rank++)
                {
                    string position = $"{file}{rank}";
                    TileMap[position] = new Tile { Position = position, OccupyingPiece = null };
                }
            }
        }

        public void UpdateTile(string position, Piece piece)
        {
            if (TileMap.ContainsKey(position))
            {
                TileMap[position].OccupyingPiece = piece;
            }
            else
            {
                TileMap[position] = new Tile { Position = position, OccupyingPiece = piece };
            }
        }

        public List<string> GetAllEmptyTiles()
        {
            List<string> emptyTiles = new List<string>();

            foreach (var kvp in TileMap)
            {
                if (kvp.Value.OccupyingPiece == null)
                {
                    emptyTiles.Add(kvp.Key);
                }
            }

            return emptyTiles;
        }

        public void DrawBoard()
        {
            Console.WriteLine("\n=== Current Board State ===");
            foreach (var kvp in TileMap)
            {
                var tile = kvp.Value;
                if (tile.OccupyingPiece != null)
                {
                    var piece = tile.OccupyingPiece;
                    Console.WriteLine($"{piece.Team} {piece.Id} ({piece.Type}) at {tile.Position} - HP: {piece.Health} Energy: {piece.Energy} Level: {piece.Level}");
                }
                else
                {
                    Console.WriteLine($"Empty Tile at {tile.Position}");
                }
            }
            Console.WriteLine("=================================");
        }
    }
}
