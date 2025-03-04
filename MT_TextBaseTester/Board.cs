using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessMonsterTactics
{
    public class Board
    {
        public List<Piece> Pieces = new List<Piece>();
        public List<string> TurnHistory { get; private set; } = new List<string>();

        public void RandomlyGeneratePieces()
        {
            Pieces = MonsterDatabase.GenerateRandomTeam("Player");
            Pieces.AddRange(MonsterDatabase.GenerateRandomTeam("AI"));
        }

        public void PlayerCreateTeam()
        {
            // Manual team creation logic goes here if needed.
        }

        public void DisplayBoard()
        {
            Console.WriteLine("\nCurrent Board:");
            foreach (var piece in Pieces)
            {
                Console.WriteLine($"{piece.Team} {piece.Id} ({piece.Type}) at {piece.Position} - HP: {piece.Health} Energy: {piece.Energy} Level: {piece.Level}");
            }
            Console.WriteLine();
        }

        public bool CheckWinCondition(out string winner)
        {
            bool playerKing = Pieces.Any(p => p.Team == "Player" && p.Type == "King" && p.Health > 0);
            bool aiKing = Pieces.Any(p => p.Team == "AI" && p.Type == "King" && p.Health > 0);

            if (!playerKing)
            {
                winner = "AI Wins!";
                return true;
            }
            if (!aiKing)
            {
                winner = "Player Wins!";
                return true;
            }

            winner = "";
            return false;
        }

        public void LogTurn(string message)
        {
            TurnHistory.Add(message);
            Console.WriteLine(message);
        }

        public void ApplyAbilityEffect(Piece piece, string abilityName)
        {
            switch (abilityName)
            {
                case "Shock Pulse":
                    DamageNearbyEnemy(piece, 5);
                    break;
                case "Blitz Protocol":
                    piece.Attack += 5;
                    LogTurn($"{piece.Team} {piece.Id} activated Blitz Protocol, gaining +5 Attack!");
                    break;
                case "Overdrive Command":
                    BoostNearbyAllies(piece, 5, 2);
                    break;
                default:
                    LogTurn($"{piece.Team} {piece.Id} used {abilityName}, but no effect implemented.");
                    break;
            }
        }

        public void PerformHitAndRetreat(Piece attacker, Piece target)
        {
            DamagePiece(attacker, target, attacker.Attack);

            if (target.Health > 0)
            {
                string retreatTile = FindClosestSafeTile(attacker);
                LogTurn($"{attacker.Team} {attacker.Id} strikes {target.Team} {target.Id} and retreats to {retreatTile}");
                attacker.Position = retreatTile;
            }
            else
            {
                LogTurn($"{attacker.Team} {attacker.Id} captures {target.Team} {target.Id} and takes the tile.");
                attacker.Position = target.Position;
            }
        }

        private string FindClosestSafeTile(Piece piece)
        {
            var legalMoves = MovementValidator.GetLegalMoves(piece, Pieces);
            var safeMoves = legalMoves.Where(pos => !IsTileUnderThreat(pos, piece.Team)).ToList();

            if (safeMoves.Count > 0)
            {
                return safeMoves.First();  // Simplified; could be more advanced
            }

            return piece.Position;  // No safe move found
        }

        private void DamageNearbyEnemy(Piece attacker, int damage)
        {
            var enemy = Pieces.FirstOrDefault(p => p.Team != attacker.Team && IsAdjacent(attacker, p));
            if (enemy != null)
            {
                DamagePiece(attacker, enemy, damage);
            }
        }

        private void DamageNearbyEnemies(Piece attacker, int damage, int maxTargets)
        {
            var enemies = Pieces.Where(p => p.Team != attacker.Team && IsAdjacent(attacker, p))
                                .Take(maxTargets)
                                .ToList();

            foreach (var enemy in enemies)
            {
                DamagePiece(attacker, enemy, damage);
            }
        }

        private void DamagePiece(Piece attacker, Piece target, int damage)
        {
            target.Health -= damage;
            attacker.TotalDamageDealt += damage;

            LogTurn($"{attacker.Team} {attacker.Id} dealt {damage} damage to {target.Team} {target.Id}");

            if (target.Health <= 0)
            {
                target.Health = 0;
                attacker.TotalKills++;
                LogTurn($"{target.Team} {target.Id} was eliminated by {attacker.Team} {attacker.Id}");
            }
        }

        private void BoostNearbyAllies(Piece piece, int attackBoost, int speedBoost)
        {
            var allies = Pieces.Where(p => p.Team == piece.Team && IsAdjacent(piece, p)).ToList();

            foreach (var ally in allies)
            {
                ally.Attack += attackBoost;
                ally.Speed += speedBoost;
                LogTurn($"{ally.Team} {ally.Id} gains +{attackBoost} Attack and +{speedBoost} Speed!");
            }
        }

        public bool IsAdjacent(Piece a, Piece b)
        {
            int fileDiff = Math.Abs(a.Position[0] - b.Position[0]);
            int rankDiff = Math.Abs(a.Position[1] - b.Position[1]);
            return fileDiff <= 1 && rankDiff <= 1;
        }

        public void ApplySynergyBonuses()
        {
            var packCounts = Pieces.Where(p => !string.IsNullOrEmpty(p.Pack))
                                   .GroupBy(p => p.Pack)
                                   .ToDictionary(g => g.Key, g => g.Count());

            foreach (var piece in Pieces)
            {
                if (!string.IsNullOrEmpty(piece.Pack) && packCounts.TryGetValue(piece.Pack, out int count) && count >= 3)
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
            LogTurn($"{piece.Team} {piece.Id} benefits from {pack} Synergy!");
        }

        public bool IsTileUnderThreat(string position, string team)
        {
            return Pieces.Where(p => p.Team != team && p.Health > 0)
                         .Any(p => MovementValidator.IsMoveLegal(p, position, Pieces));
        }

        public void ShowTeamPreview()
        {
            Console.WriteLine("\n=== Team Preview ===");

            var packCounts = Pieces
                .Where(p => !string.IsNullOrEmpty(p.Pack))
                .GroupBy(p => p.Pack)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var piece in Pieces.Where(p => p.Team == "Player"))
            {
                string synergyStatus = (packCounts.ContainsKey(piece.Pack) && packCounts[piece.Pack] >= 3)
                    ? "(Synergy Active)"
                    : "";
                Console.WriteLine($"{piece.Id} ({piece.Type}) - {piece.Pack} {synergyStatus}");
            }

            Console.WriteLine("=================================");
        }

        private void AwardXP(Piece piece, int xp)
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
                LogTurn($"{piece.Team} {piece.Id} leveled up to Level {piece.Level}!");
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

        // Ultimate Ability System
        private void CheckUltimateUnlock(Piece piece)
        {
            if (piece.Level >= 5 && !piece.UltimateUnlocked)
            {
                piece.UltimateUnlocked = true;
                LogTurn($"{piece.Team} {piece.Id} unlocked their Ultimate Ability!");
            }
        }

        public void ChargeUltimate(Piece piece, int chargeAmount)
        {
            if (piece.UltimateUnlocked)
            {
                piece.UltimateCharge += chargeAmount;
                if (piece.UltimateCharge >= 100)
                {
                    UseUltimateAbility(piece);
                    piece.UltimateCharge = 0; // Reset after use
                }
            }
        }

        private void UseUltimateAbility(Piece piece)
        {
            LogTurn($"{piece.Team} {piece.Id} unleashes their Ultimate Ability: {piece.Ultimate}!");
            ApplyAbilityEffect(piece, piece.Ultimate);
        }

        public void ProcessStartOfTurnEffects(string team)
        {
            foreach (var piece in Pieces.Where(p => p.Team == team && p.Health > 0))
            {
                TriggerPiecePassive(piece);
            }
        }

        private void TriggerPiecePassive(Piece piece)
        {
            switch (piece.Passive)
            {
                case "Water Ward":
                    HealAdjacentAllies(piece, 2);
                    break;

                case "Rooted Resilience":
                    if (IsAdjacentToFriendlyPiece(piece, "Pawn"))
                    {
                        piece.Defense += 2;
                        LogTurn($"{piece.Team} {piece.Id} gains +2 Defense from Rooted Resilience!");
                    }
                    break;

                case "Blazing Stride":
                    LeaveBurningTile(piece.Position);
                    break;

                case "Commanding Presence":
                    BoostAdjacentAllies(piece, 5);
                    break;

                default:
                    // No passive or unhandled passive
                    break;
            }
        }

        private void HealAdjacentAllies(Piece piece, int healAmount)
        {
            var allies = Pieces.Where(p => p.Team == piece.Team && IsAdjacent(piece, p)).ToList();
            foreach (var ally in allies)
            {
                ally.Health += healAmount;
                LogTurn($"{piece.Team} {piece.Id} heals {ally.Team} {ally.Id} for {healAmount} HP with Water Ward!");
            }
        }

        private bool IsAdjacentToFriendlyPiece(Piece piece, string type)
        {
            return Pieces.Any(p => p.Team == piece.Team && p.Type == type && IsAdjacent(piece, p));
        }

        private void LeaveBurningTile(string position)
        {
            LogTurn($"The tile at {position} is now burning! Any piece moving there takes 5 damage.");
            // You could later add logic to check if a piece enters a burning tile.
        }

        private void BoostAdjacentAllies(Piece piece, int attackBoost)
        {
            var allies = Pieces.Where(p => p.Team == piece.Team && IsAdjacent(piece, p)).ToList();
            foreach (var ally in allies)
            {
                ally.Attack += attackBoost;
                LogTurn($"{piece.Team} {piece.Id} boosts {ally.Team} {ally.Id}'s Attack by {attackBoost} with Commanding Presence!");
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
                LogTurn($"❌ Evolution error: No data found for {evolvedId}");
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

            LogTurn($"{piece.Team} {piece.Id} evolved into {evolvedId}!");
        }

        public void ShowDetailedEndOfGameReport(string winner)
        {
            Console.WriteLine("\n=== Detailed End-of-Game Report ===");

            Console.WriteLine($"\nWinner: {winner}");
            Console.WriteLine($"Total Turns: {TurnHistory.Count}");

            Console.WriteLine("\nPiece Performance:");
            foreach (var piece in Pieces)
            {
                string status = piece.Health > 0 ? "Survived" : "Eliminated";
                Console.WriteLine($"- {piece.Team} {piece.Id}: {piece.TotalKills} Kills, {piece.TotalDamageDealt} Damage Dealt ({status})");
            }

            Console.WriteLine("\nAbilities Used:");
            var abilityUsage = TurnHistory
                .Where(entry => entry.Contains("used"))
                .GroupBy(entry => entry.Split(' ')[1] + " " + entry.Split(' ')[2])
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var ability in abilityUsage)
            {
                Console.WriteLine($"- {ability.Key} used abilities {ability.Value} times");
            }

            Console.WriteLine("\nEvolution Log:");
            var evolutionLog = TurnHistory.Where(entry => entry.Contains("evolved into")).ToList();
            if (evolutionLog.Count > 0)
            {
                foreach (var log in evolutionLog)
                {
                    Console.WriteLine($"- {log}");
                }
            }
            else
            {
                Console.WriteLine("No evolutions occurred.");
            }

            Console.WriteLine("\nComplete Turn History:");
            for (int i = 0; i < TurnHistory.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {TurnHistory[i]}");
            }

            Console.WriteLine("==================================");
        }

        public bool IsAdjacentToPosition(string position1, string position2)
        {
            if (string.IsNullOrEmpty(position1) || string.IsNullOrEmpty(position2))
                return false;

            int fileDiff = Math.Abs(position1[0] - position2[0]);
            int rankDiff = Math.Abs(position1[1] - position2[1]);

            return fileDiff <= 1 && rankDiff <= 1;
        }


        

    }
}
