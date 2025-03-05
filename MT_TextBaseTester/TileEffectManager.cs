using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class TileEffectManager
    {
        private readonly Board _board;
        public Dictionary<string, string> TileEffects = new();

        public TileEffectManager(Board board)
        {
            _board = board;
        }

        public void ApplyStartOfTurnEffects(Piece piece)
        {
            if (TileEffects.TryGetValue(piece.Position, out string effect))
            {
                ApplyTileEffect(piece, effect);
            }
        }

        public void ApplyOnEntryEffects(Piece piece, string newPosition)
        {
            if (TileEffects.TryGetValue(newPosition, out string effect))
            {
                ApplyTileEntryEffect(piece, newPosition, effect);
            }
        }

        private void ApplyTileEffect(Piece piece, string effect)
        {
            switch (effect)
            {
                case "Burning":
                    piece.Health -= 5;
                    _board.LogTurn($"{piece.Team} {piece.Id} took 5 damage from Burning Tile at {piece.Position}!");
                    break;

                case "Healing":
                    piece.Health += 3;
                    _board.LogTurn($"{piece.Team} {piece.Id} healed 3 HP from Healing Tile at {piece.Position}!");
                    break;

                case "Poisoned":
                    piece.Health -= 3;
                    _board.LogTurn($"{piece.Team} {piece.Id} took 3 poison damage at {piece.Position}!");
                    break;

                case "Energized":
                    piece.Energy = Math.Min(piece.Energy + 1, 10);
                    _board.LogTurn($"{piece.Team} {piece.Id} restored 1 energy from Energized Tile at {piece.Position}!");
                    break;

                case "Shielded":
                    piece.Defense += 2;
                    _board.LogTurn($"{piece.Team} {piece.Id} gained +2 defense from Shielded Tile at {piece.Position}!");
                    break;

                case "Cursed":
                    piece.EnergyCostModifier = 1;
                    _board.LogTurn($"{piece.Team} {piece.Id} suffers +1 energy cost from Cursed Tile at {piece.Position}!");
                    break;

                case "Blessed":
                    piece.EnergyCostModifier = -1;
                    _board.LogTurn($"{piece.Team} {piece.Id} gains -1 energy cost from Blessed Tile at {piece.Position}!");
                    break;
            }
        }

        private void ApplyTileEntryEffect(Piece piece, string position, string effect)
        {
            switch (effect)
            {
                case "Spiked":
                    piece.Health -= 5;
                    _board.LogTurn($"{piece.Team} {piece.Id} took 5 damage from Spiked Tile at {position}!");
                    break;

                case "Warp":
                    string randomTile = GetRandomSafeTile();
                    _board.LogTurn($"{piece.Team} {piece.Id} was warped from {position} to {randomTile}!");
                    _board.MovePiece(piece, randomTile);
                    break;

                case "Frozen":
                    piece.Speed = Math.Max(1, piece.Speed - 1);
                    _board.LogTurn($"{piece.Team} {piece.Id} has reduced speed due to Frozen Tile at {position}!");
                    break;
            }
        }

        public string GetRandomSafeTile()
        {
            var emptyTiles = GenerateAllBoardTiles()
                .Where(tile => !_board.Pieces.Any(p => p.Position == tile))
                .ToList();

            if (emptyTiles.Count > 0)
            {
                Random rand = new Random();
                return emptyTiles[rand.Next(emptyTiles.Count)];
            }

            return "A1";  // Fallback if all tiles are full
        }

        public void SetTileEffect(string position, string effect)
        {
            TileEffects[position] = effect;
            _board.LogTurn($"Tile {position} is now {effect}!");
        }

        public void ClearTileEffect(string position)
        {
            if (TileEffects.ContainsKey(position))
            {
                TileEffects.Remove(position);
                _board.LogTurn($"Tile {position} is no longer affected.");
            }
        }

        private List<string> GenerateAllBoardTiles()
        {
            List<string> allTiles = new();
            string files = "ABCDEFGH";
            for (int rank = 1; rank <= 8; rank++)
            {
                foreach (char file in files)
                {
                    allTiles.Add($"{file}{rank}");
                }
            }
            return allTiles;
        }

        public void ApplyEffectsToAllPieces(Board board)
        {
            foreach (var piece in board.Pieces.Where(p => p.Health > 0))
            {
                if (TileEffects.TryGetValue(piece.Position, out string effect))
                {
                    ApplyTileEffect(piece, effect);
                }
            }
        }
    }
}
