namespace Goblinos.Scripts.Units.Stats;

public static class DerivedStatsCalculator
{
    public static DerivedStats Build(UnitStats unitStats)
    {
        // Base stats / core attributes
        var might = unitStats.BaseStats.Might;
        var agility = unitStats.BaseStats.Agility;
        var vitality = unitStats.BaseStats.Vitality;
        var mind = unitStats.BaseStats.Mind;
        var presence = unitStats.BaseStats.Presence;
        var luck = unitStats.BaseStats.Luck;
        var movement = unitStats.BaseStats.Movement;
        var maxHitPoints = unitStats.BaseStats.MaxHitPoints;
        var defense = unitStats.BaseStats.Defense;
        var resistance = unitStats.BaseStats.Resistance;
        
        // PhysicalDamage = Might + weapon
        // MagicDamage = Mind + weapon
        // AttackSpeed = agility
        // Accuracy = 60 + 2*Agility + Weapon accuracy bonus
        // Evasion = 10 + 2*Agility + Presence + terrain evasion bonus
        // CritChance = Luck + Presence
        // CritDefense = Luck + Presence
        // PhysicalProtection = Defense + 2*Vitality
        // MagicProtection = Resistance + Mind + Presence
        // StatusChance = presence + Mind + luck
        // StatusResist = Resistance + presence + vitality + luck
        // ArmorPierce = base + modifiers only
        // MagicPenetration = base + modifiers only

        var physicalDamage = might;
        var magicDamage = mind;
        // var attackSpeed = 0;
        // var accuracy = 0;
        // var evasion = 0;
        // var critChance = 0;
        // var critDefense = 0;
        var physicalProtection = defense;
        var magicResistance = resistance;
        // var statusChance = 0;
        // var statusResist = 0;
        // var armorPierce = 0;
        // var magicPenetration = 0;
        
        // Stage A: apply precompute modifiers to core/base
        // Stage B: compute derived
        // Stage C: apply postcompute modifiers to derived
        // return snapshot
        return new DerivedStats(
            might: might,
            agility: agility,
            vitality: vitality,
            mind: mind,
            presence: presence,
            luck: luck,
            movement: movement,
            maxHitPoints: maxHitPoints,
            defense: defense,
            resistance: resistance,
            physicalDamage: physicalDamage,
            magicDamage: magicDamage,
            physicalProtection: physicalProtection,
            magicResistance: magicResistance
        );
    }
}