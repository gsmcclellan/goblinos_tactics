using System;
using Goblinos.Scripts.Battle.Units;
using Godot;

namespace Goblinos.Scripts.Battle.Preview;

public partial class CombatPreview: Resource
{
    public BattleUnit Attacker;
    public BattleUnit Defender;

    public int AttackerHitChance;
    public int DefenderHitChance;

    public int AttackerDamage;
    public int DefenderDamage;
    public int AttackerExpectedHitPoints => Math.Max(0, Attacker.CurrentHitPoints - DefenderDamage);
    public int DefenderExpectedHitPoints => Math.Max(0, Defender.CurrentHitPoints - AttackerDamage);
    
}