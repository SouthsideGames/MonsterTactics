namespace ChessMonsterTactics
{
    public class Tile
    {
        public string Position { get; set; }   // Example: "A1", "B2"
        public Piece OccupyingPiece { get; set; }  // Null if empty
        public bool NeedsRedraw { get; set; } = true;  // Optional,

        public Tile() {}
        
        public Tile(string position)
        {
            Position = position;
            NeedsRedraw = true;  // Always draw once at start
        }

        public void PlacePiece(Piece piece)
        {
            OccupyingPiece = piece;
            NeedsRedraw = true;
        }

        public void ClearPiece()
        {
            OccupyingPiece = null;
            NeedsRedraw = true;
        }
    }
}