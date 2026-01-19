using Goblinos.Scripts.Units.Stats.Types;

namespace Goblinos.Scripts.Units.Stats;

public class StatBlock
{
    /** Core Attributes */
    public int Might;
    public int Agility;
    public int Vitality;
    public int Mind;
    public int Presence;
    public int Luck;

    /** Base Stats */
    public int Movement;
    public int MaxHealth;
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
        int maxHealth,
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
        MaxHealth = maxHealth;
        Defense = defense;
        Resistance = resistance;
    }
}