using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class PieceEvolutionManager
    {
        private readonly Board _board;

        public PieceEvolutionManager(Board board)
        {
            _board = board;
        }

        public void AwardXP(Piece piece, int xp)
        {
            piece.Experience += xp;
            CheckLevelUp(piece);
        }

        private void CheckLevelUp(Piece piece)
        {
            int requiredXP = GetXPRequiredForLevel(piece.Level);
            if (piece.Experience >= requiredXP && piece.Level < 10)
            {
                piece.Level++;
                piece.Experience -= requiredXP;
                ApplyLevelUpStatBoost(piece);
                _board.LogTurn($"{piece.Team} {piece.Id} leveled up to Level {piece.Level}!");
                CheckUltimateUnlock(piece);
                CheckForPieceEvolution(piece);
            }
        }

        private int GetXPRequiredForLevel(int level)
        {
            return level switch
            {
                1 => 10,
                2 => 20,
                3 => 30,
                4 => 50,
                5 => 70,
                6 => 100,
                7 => 130,
                8 => 170,
                9 => 220,
                _ => int.MaxValue
            };
        }

        private void ApplyLevelUpStatBoost(Piece piece)
        {
            piece.Health = (int)(piece.Health * 1.05);
            piece.Attack = (int)(piece.Attack * 1.03);
            piece.Defense = (int)(piece.Defense * 1.02);
        }

        private void CheckUltimateUnlock(Piece piece)
        {
            if (piece.Level >= 5 && !piece.UltimateUnlocked)
            {
                piece.UltimateUnlocked = true;
                _board.LogTurn($"{piece.Team} {piece.Id} unlocked their Ultimate Ability!");
            }
        }

        private void CheckForPieceEvolution(Piece piece)
        {
            if (MonsterDatabase.EvolutionChains.TryGetValue(piece.Id, out var evolutionStages))
            {
                var nextEvolution = evolutionStages.FirstOrDefault(e => e.Level == piece.Level);
                if (nextEvolution != default)
                {
                    EvolvePiece(piece, nextEvolution.EvolvedForm);
                }
            }
        }

        private void EvolvePiece(Piece piece, string evolvedId)
        {
            if (!MonsterDatabase.PieceTemplates.TryGetValue(evolvedId, out var evolvedPiece))
            {
                _board.LogTurn($"❌ Evolution error: No data found for {evolvedId}");
                return;
            }

            piece.Id = evolvedPiece.Id;
            piece.Type = evolvedPiece.Type;
            piece.Health = evolvedPiece.Health;
            piece.Attack = evolvedPiece.Attack;
            piece.Defense = evolvedPiece.Defense;
            piece.Speed = evolvedPiece.Speed;
            piece.Ability = evolvedPiece.Ability;
            piece.Passive = evolvedPiece.Passive;
            piece.Ultimate = evolvedPiece.Ultimate;
            piece.Pack = evolvedPiece.Pack;

            _board.LogTurn($"{piece.Team} {piece.Id} evolved into {evolvedId}!");
        }
    }
}
