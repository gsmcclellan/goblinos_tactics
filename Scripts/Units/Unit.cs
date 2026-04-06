using Goblinos.Scripts.Combat;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Test;
using Goblinos.Scripts.Units.Stats;
using Goblinos.Scripts.Units.Stats.Types;
using Godot;

namespace Goblinos.Scripts.Units;

public class Unit
{
    
    public string Id;
    public string TemplateId;
    public string UnitName;
    public UnitStats Stats;
    
    public RangeBand AttackRange = new RangeBand(1, 1); // TODO - base on weapon.
    public AbilityDefinition Ability;
    
    public int Level;
    public int Experience;

    public bool IsFriendly;

    public string ImageFilePath => GlobalSettings.UnitImageDirPath + Template.ImageFileName + ".png";
    public UnitTemplate Template => TestUnitTemplates.Dict[TemplateId];

    // Future stuff:
    // class/job
    // inventory, equipment slots
    // learned abilities
    // long-term flags (injuries, traits, bonds)

    public int GetStat(StatName statName) => Stats.Get(statName);
}