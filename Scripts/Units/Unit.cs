using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Units.Stats;
using Godot;

namespace Goblinos.Scripts.Units;

public class Unit
{
    // base/core stats
    public string Id;
    public string UnitName;
    public UnitStats Stats;
    // level / XP / growths
    public int Level;
    public int Experience;

    public bool IsFriendly;

    // Future stuff:
    // class/job
    // inventory, equipment slots
    // learned abilities
    // long-term flags (injuries, traits, bonds)
}