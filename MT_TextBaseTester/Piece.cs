namespace ChessMonsterTactics
{
    public class Piece
    {
        public string Pack { get; set; } 
        public string Id { get; set; }
        public string Type { get; set; }
        public int Health { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public string Ability { get; set; }
        public string Passive { get; set; }
        public string Ultimate { get; set; }
        public string Position { get; set; }
        public string Team { get; set; }
        public int Energy { get; set; } = 10;
        public int TotalDamageDealt { get; set; } = 0;
        public int TotalKills { get; set; } = 0;
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public int EnergyCostModifier { get; set; } = 0;  // Default: No cost change

        public bool UltimateUnlocked { get; set; } = false;
        public int UltimateCharge { get; set; } = 0;


        public Piece Clone()
        {
            return new Piece
            {
                Id = this.Id,
                Type = this.Type,
                Team = this.Team,
                Health = this.Health,
                Attack = this.Attack,
                Defense = this.Defense,
                Speed = this.Speed,
                Ability = this.Ability,
                Passive = this.Passive,
                Ultimate = this.Ultimate,
                Pack = this.Pack,
                Position = this.Position,
                Energy = this.Energy,
                Level = this.Level,
                Experience = this.Experience,
                UltimateUnlocked = this.UltimateUnlocked,
                UltimateCharge = this.UltimateCharge,
                TotalKills = this.TotalKills,
                TotalDamageDealt = this.TotalDamageDealt
            };
        }
    }

    
}
