namespace Goblinos.Scripts.Units.Stats;

public static class DerivedStatsCalculator
{
    
    
    private const int AccuracyBase = 60;
    private const int AccuracyAgilityMult = 2;
    private const int AccuracyPresenceMult = 2;
    private const int CritDamageMultiplier = 150; // percent
    private const int EvasionBase = 10;
    private const int EvasionAgilityMult = 2;
    private const int EvasionPresenceMult = 1;
    private const int HpBase = 20;
    private const int HpVitalityMult = 3;
    private const int MightDamageMult = 1;
    private const int MindDamageMult = 1;
    private const int ProtectionVitalityMult = 0;
    private const int ProtectionDefenseMult = 2;
    private const int ProtectionResistanceMult = 2;
    public static DerivedStats Build(UnitStats unitStats, int unitLevel)
    {
        // TODO - add modifiers
        // Core Attributes
        var might = unitStats.BaseStats.Might;
        var agility = unitStats.BaseStats.Agility;
        var vitality = unitStats.BaseStats.Vitality;
        var mind = unitStats.BaseStats.Mind;
        var presence = unitStats.BaseStats.Presence;
        var luck = unitStats.BaseStats.Luck;
        // Base Stats
        var movement = unitStats.BaseStats.Movement;
        var baseHitPoints = unitStats.BaseStats.BaseHitPoints;
        var defense = unitStats.BaseStats.Defense;
        var resistance = unitStats.BaseStats.Resistance;

        var weaponDamage = 10 + unitLevel; // TODO weapons
        var weaponAccuracy = 10 + 2 * unitLevel; // TODO weapons
        
        // MaxHP = BaseHP + vitality * mult
        var maxHitPoints = baseHitPoints + vitality * HpVitalityMult;
        
        // Damage = Weapon + primary stat * bonus
        // Accuracy = 60 + 2*Agility + Weapon accuracy bonus
        var physicalDamage = weaponDamage + might * MightDamageMult;
        var physicalAccuracy = AccuracyBase + agility * AccuracyAgilityMult + weaponAccuracy;

        var magicDamage = weaponDamage + mind * MindDamageMult;
        var magicAccuracy = AccuracyBase + presence * AccuracyPresenceMult + weaponAccuracy;
        
        // Evasion = base + 2*Agility + Presence + terrain evasion bonus
        var evasion = EvasionBase + agility * EvasionAgilityMult + presence * EvasionPresenceMult;
        
        // Protection = stat + vitality * modifier
        var physicalProtection = defense * ProtectionDefenseMult;
        var magicResistance = resistance * ProtectionResistanceMult;
        
        // CritChance = Luck + Presence
        // CritDefense = Luck + Presence
                
        // HitChance = Clamp(Accuracy - Evasion + RNG(-5,5), 5, 95)
        // FinalDamage    = RawDamage * (1 - PhysProtection/100
        
                
        // AttackSpeed = agility
        
        // StatusChance = presence + Mind + luck
        // StatusResist = Resistance + presence + vitality + luck
        
        // ArmorPierce = base + modifiers only
        // MagicPenetration = base + modifiers only

        
        // Stage A: apply precompute modifiers to core/base TODO
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
            physicalAccuracy: physicalAccuracy,
            magicAccuracy: magicAccuracy,
            evasion: evasion,
            physicalProtection: physicalProtection,
            magicResistance: magicResistance
        );
    }
}