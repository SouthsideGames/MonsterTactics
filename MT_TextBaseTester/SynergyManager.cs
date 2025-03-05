using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class SynergyManager
    {
        private readonly Board _board;

        public SynergyManager(Board board)
        {
            _board = board;
        }

        public void ApplySynergyBonuses()
        {
            var packCounts = _board.Pieces
                .Where(p => !string.IsNullOrEmpty(p.Pack))
                .GroupBy(p => p.Pack)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var piece in _board.Pieces)
            {
                if (!string.IsNullOrEmpty(piece.Pack) && 
                    packCounts.TryGetValue(piece.Pack, out int count) && count >= 3)
                {
                    ApplyPackBonus(piece, piece.Pack);
                }
            }
        }

        private void ApplyPackBonus(Piece piece, string pack)
        {
            switch (pack)
            {
                case "Fire Pack":
                    piece.Attack = (int)(piece.Attack * 1.05);
                    break;
                case "Cyber Pack":
                    piece.Defense += 2;
                    break;
                case "Shadow Pack":
                    piece.Speed += 1;
                    break;
            }
            _board.LogTurn($"{piece.Team} {piece.Id} benefits from {pack} Synergy!");
        }
    }
}
