using Goblinos.Scripts.Units.Stats.Types;

namespace Goblinos.Scripts.Units.Stats;

public class StatBlock
{
    /** Core Attributes */
    // Physical power, damage
    public int Might;
    // hit, dodge, crit
    public int Agility;
    // health, defense
    public int Vitality;
    // magic damage, magic def
    public int Mind;
    // status apply, status defense, magic defense
    public int Presence;
    // crit, crit def
    public int Luck;

    /** Base Stats */
    public int Movement;
    public int MaxHitPoints;
    public int Defense;
    public int Resistance;

    // Weapon proficiency
    public StatBlock(
        int might,
        int agility,
        int vitality,
        int mind,
        int presence,
        int luck,
        int movement,
        int maxHitPoints,
        int defense,
        int resistance)
    {
        Might = might;
        Agility = agility;
        Vitality = vitality;
        Mind = mind;
        Presence = presence;
        Luck = luck;
        Movement = movement;
        MaxHitPoints = maxHitPoints;
        Defense = defense;
        Resistance = resistance;
    }
}