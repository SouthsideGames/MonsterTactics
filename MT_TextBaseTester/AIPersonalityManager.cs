using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class AIPersonalityManager
    {
        private readonly string _aiName;
        private readonly Board _board;
        private string _personalityType;
        private readonly Random _random = new();
        private AIPersonalityType personalityType;

        public AIPersonalityManager(Board board, string aiName, AIPersonalityType personality)
        {
            this._board = board;
            this._aiName = aiName;
            this.personalityType = personality;
        }

        private void LoadPersonality()
        {
            var personalities = PersistentDataManager.LoadAIPersonalities();

            if (personalities.TryGetValue(_aiName, out var personalityData))
            {
                _personalityType = personalityData.PersonalityType;
            }
            else
            {
                _personalityType = AssignRandomPersonality();
                personalities[_aiName] = new PersistentDataManager.AIPersonalityStats
                {
                    PersonalityType = _personalityType
                };
                PersistentDataManager.SaveAIPersonalities(personalities);
            }

            _board.LogTurn($"{_aiName} personality set to {_personalityType}.");
        }

        private string AssignRandomPersonality()
        {
            var personalities = new[] { "Aggressive", "Defensive", "SynergyHunter" };
            return personalities[_random.Next(personalities.Length)];
        }

        public string GetPersonality()
        {
            return _personalityType;
        }

        public int EvaluateMoveBonus(Piece piece, string move)
        {
            int score = 0;

            switch (personalityType)
            {
                case AIPersonalityType.Aggressive:
                    if (IsNearEnemy(piece, move)) score += 5;  // Aggressive AI likes engaging enemies
                    break;

                case AIPersonalityType.Defensive:
                    if (IsNearAlly(piece, move)) score += 3;  // Defensive AI prefers clustering with allies
                    if (_board.IsTileDefensive(move)) score += 5;  // Defensive AI likes defensive tiles
                    break;

                case AIPersonalityType.Sneaky:
                    if (_board.IsTileSafe(move)) score += 4;  // Sneaky AI prefers hiding in safe tiles
                    break;

                case AIPersonalityType.Balanced:
                default:
                    // No special bias
                    break;
            }

            return score;
        }

        private int EvaluateAggressiveMoveBonus(Piece piece, string move)
        {
            var targetPiece = _board.Pieces.FirstOrDefault(p => p.Position == move && p.Team != piece.Team);
            if (targetPiece != null)
            {
                return 15 + (targetPiece.Health / 2);  // Aggressive personalities love captures
            }
            return 0;
        }

        private int EvaluateDefensiveMoveBonus(Piece piece, string move)
        {
            var king = _board.Pieces.FirstOrDefault(p => p.Team == piece.Team && p.Type == "King");
            if (king != null && _board.IsAdjacentToPosition(move, king.Position))
            {
                return 10;  // Defensive personalities like staying near the king
            }
            if (_board.IsTileUnderThreat(move, piece.Team))
            {
                return -10;  // Avoid danger
            }
            return 0;
        }

        private int EvaluateSynergyHunterMoveBonus(Piece piece, string move)
        {
            var nearbySamePack = _board.Pieces.Any(p => p.Team == piece.Team && p.Pack == piece.Pack && _board.IsAdjacentToPosition(p.Position, move));
            return nearbySamePack ? 10 : 0;  // Bonus for staying near same-pack pieces
        }

        private bool IsNearEnemy(Piece piece, string position)
        {
            return _board.Pieces.Any(p => p.Team != piece.Team && _board.IsAdjacentToPosition(p.Position, position));
        }

        private bool IsNearAlly(Piece piece, string position)
        {
            return _board.Pieces.Any(p => p.Team == piece.Team && p.Id != piece.Id && _board.IsAdjacentToPosition(p.Position, position));
        }
    }
}
