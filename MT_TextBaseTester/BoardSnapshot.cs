using System.Collections.Generic;

namespace ChessMonsterTactics
{
    public class BoardSnapshot
    {
        public int TurnNumber { get; set; }
        public List<Piece> Pieces { get; set; } = new();
        public Dictionary<string, string> TileEffects { get; set; } = new();
    }
}
