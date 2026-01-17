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

    /// <summary>
    /// Makes flavor based display, possibly contextual based on type.
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public string GetDisplayName(CoreAttribute attribute)
    {
        // Might - Smack, Smash, Slash, Stab, Blast
        // Agility - Scurry, Sneak
        // Vitality - Grit, Meatiness
        // Mind - Cunning, Guile, Weird, Trickery
        // Presence - Swagger, Bluster, Moxie, Menace
        // Luck - ??
        return attribute.ToString();
    }
}

public enum CoreAttribute
{
    Might,
    Agility,
    Vitality,
    Mind,
    Presence,
    Luck
}