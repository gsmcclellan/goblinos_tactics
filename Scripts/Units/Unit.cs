using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Test;
using Goblinos.Scripts.Units.Stats;
using Godot;

namespace Goblinos.Scripts.Units;

public class Unit
{
    
    public string Id;
    public string TemplateId;
    public string UnitName;
    public UnitStats Stats;
    
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
}