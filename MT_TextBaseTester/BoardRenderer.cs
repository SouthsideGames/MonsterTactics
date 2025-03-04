using System;
using System.Collections.Generic;

namespace ChessMonsterTactics
{
    public class BoardRenderer
    {
        private readonly Dictionary<string, Tile> tiles;

        public BoardRenderer()
        {
            tiles = new Dictionary<string, Tile>();

            string files = "ABCDEFGH";
            for (int rank = 1; rank <= 8; rank++)
            {
                foreach (char file in files)
                {
                    string position = $"{file}{rank}";
                    tiles[position] = new Tile(position);
                }
            }
        }

        public void UpdateTile(string position, Piece piece)
        {
            if (tiles.ContainsKey(position))
            {
                if (piece != null)
                {
                    tiles[position].PlacePiece(piece);
                }
                else
                {
                    tiles[position].ClearPiece();
                }
            }
        }

        public void DrawBoard()
        {
            Console.WriteLine("\nCurrent Board:");

            foreach (var tile in tiles.Values)
            {
                if (tile.NeedsRedraw)
                {
                    DrawTile(tile);
                    tile.NeedsRedraw = false;  // Reset the flag after drawing
                }
            }

            Console.WriteLine();
        }

        private void DrawTile(Tile tile)
        {
            if (tile.OccupyingPiece != null)
            {
                var piece = tile.OccupyingPiece;
                Console.WriteLine($"{piece.Team} {piece.Id} ({piece.Type}) at {tile.Position} - HP: {piece.Health} Energy: {piece.Energy}");
            }
            else
            {
                Console.WriteLine($"Empty Tile at {tile.Position}");
            }
        }
    }
}
