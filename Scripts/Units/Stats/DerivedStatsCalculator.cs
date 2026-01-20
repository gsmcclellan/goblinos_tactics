namespace Goblinos.Scripts.Units.Stats;

public sealed class DerivedStatsCalculator
{
    public DerivedStats Build(UnitStats unitStats)
    {
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

        var physicalDamage = unitStats.BaseStats.Might;
        var magicDamage = unitStats.BaseStats.Mind;
        var attackSpeed = 0;
        var accuracy = 0;
        var evasion = 0;
        var critChance = 0;
        var critDefense = 0;
        var physicalProtection = unitStats.BaseStats.Defense;
        var magicProteciton = unitStats.BaseStats.Resistance;
        var statusChance = 0;
        var statusResist = 0;
        var armorPierce = 0;
        var magicPenetratio = 0;
        
        // Stage A: apply precompute modifiers to core/base
        // Stage B: compute derived
        // Stage C: apply postcompute modifiers to derived
        // return snapshot
        return new DerivedStats();
    }
}