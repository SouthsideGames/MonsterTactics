using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class TileEffectManager
    {
        public Dictionary<string, string> TileEffects = new();

        private Board board;

        public TileEffectManager(Board board)
        {
            this.board = board;
        }

        // Apply effects that happen at the start of a piece's turn
        public void ApplyStartOfTurnEffects(Piece piece)
        {
            if (TileEffects.TryGetValue(piece.Position, out string effect))
            {
                ApplyTileEffect(piece, effect);
            }
        }

        // Apply effects that happen when a piece enters a tile
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
                    board.LogTurn($"{piece.Team} {piece.Id} took 5 damage from Burning Tile at {piece.Position}!");
                    break;

                case "Healing":
                    piece.Health += 3;
                    board.LogTurn($"{piece.Team} {piece.Id} healed 3 HP from Healing Tile at {piece.Position}!");
                    break;

                case "Poisoned":
                    piece.Health -= 3;
                    board.LogTurn($"{piece.Team} {piece.Id} took 3 poison damage at {piece.Position}!");
                    break;

                case "Energized":
                    piece.Energy = Math.Min(piece.Energy + 1, 10);
                    board.LogTurn($"{piece.Team} {piece.Id} restored 1 energy from Energized Tile at {piece.Position}!");
                    break;

                case "Shielded":
                    piece.Defense += 2;
                    board.LogTurn($"{piece.Team} {piece.Id} gained +2 defense from Shielded Tile at {piece.Position}!");
                    break;

                case "Cursed":
                    piece.EnergyCostModifier = 1;
                    board.LogTurn($"{piece.Team} {piece.Id} suffers +1 energy cost from Cursed Tile at {piece.Position}!");
                    break;

                case "Blessed":
                    piece.EnergyCostModifier = -1;
                    board.LogTurn($"{piece.Team} {piece.Id} gains -1 energy cost from Blessed Tile at {piece.Position}!");
                    break;
            }
        }

        private void ApplyTileEntryEffect(Piece piece, string position, string effect)
        {
            switch (effect)
            {
                case "Spiked":
                    piece.Health -= 5;
                    board.LogTurn($"{piece.Team} {piece.Id} took 5 damage from Spiked Tile at {position}!");
                    break;

                case "Warp":
                    string randomTile = GetRandomSafeTile();
                    board.LogTurn($"{piece.Team} {piece.Id} was warped from {position} to {randomTile}!");
                    board.MovePiece(piece, randomTile);
                    break;

                case "Frozen":
                    piece.Speed = Math.Max(1, piece.Speed - 1);
                    board.LogTurn($"{piece.Team} {piece.Id} has reduced speed due to Frozen Tile at {position}!");
                    break;
            }
        }

        public string GetRandomSafeTile()
        {
            var emptyTiles = board.Renderer.GetAllEmptyTiles();
            if (emptyTiles.Count > 0)
            {
                Random rand = new Random();
                return emptyTiles[rand.Next(emptyTiles.Count)];
            }
            return "A1";
        }

        public void SetTileEffect(string position, string effect)
        {
            TileEffects[position] = effect;
            board.LogTurn($"Tile {position} is now {effect}!");
        }

        public void ClearTileEffect(string position)
        {
            if (TileEffects.ContainsKey(position))
            {
                TileEffects.Remove(position);
                board.LogTurn($"Tile {position} is no longer affected.");
            }
        }

        
    }
}
