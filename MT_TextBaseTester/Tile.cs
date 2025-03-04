namespace ChessMonsterTactics
{
    public class Tile
    {
        public string Position;
        public Piece OccupyingPiece;
        public bool NeedsRedraw;

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