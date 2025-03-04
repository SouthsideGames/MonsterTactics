using System;

static class Utils
{
    private static Random random = new Random();

    public static int RandomStat(int min, int max) => random.Next(min, max + 1);

    public static string RandomAbility() =>
        new[] { "Fire Leap", "Healing Mist", "Stonewall", "Root Strike", "Psychic Blast", "Dark Strike", "Arcane Pulse" }[random.Next(7)];

    public static string RandomPassive() =>
        new[] { "Blazing Stride", "Water Ward", "Rooted Resilience", "Immovable Object", "System Reboot", "Adaptive Shielding", "Energy Surge" }[random.Next(7)];

    public static string RandomUltimate() =>
        new[] { "Sky Strike", "Blessing of Stars", "Mind Collapse", "Swarm Assault" }[random.Next(4)];

    public static string RandomPosition()
    {
        char file = (char)('A' + random.Next(8));
        int rank = random.Next(1, 9);
        return $"{file}{rank}";
    }

    public static (int, int) PositionToCoordinates(string position)
    {
        int file = position[0] - 'A';
        int rank = int.Parse(position[1].ToString()) - 1;
        return (rank, file);
    }

    public static string CoordinatesToPosition(int row, int col)
    {
        char file = (char)('A' + col);
        int rank = row + 1;
        return $"{file}{rank}";
    }
}
