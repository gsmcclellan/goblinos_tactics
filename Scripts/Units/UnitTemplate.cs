using Goblinos.Scripts.Units.Stats;
using Goblinos.Scripts.Units.Stats.Types;

namespace Goblinos.Scripts.Units;

public class UnitTemplate
{
    public string Id;
    public string DisplayName;
    public string ImageFileName;
    public StatBlock BaseStats;
    public StatGrowthProfile StatGrowthProfile;
    public int MinRange = 1;
    public int MaxRange = 1;
}