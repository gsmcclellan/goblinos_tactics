using System;
using System.Collections.Generic;
using Goblinos.Logging;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Units;
using Goblinos.Scripts.Units.Stats;
using Godot;
using Range = Godot.Range;

namespace Goblinos.Scripts.Battle;

public partial class BattleUnit : Area2D
{
    /** Components */
    private readonly Logger _logger = LogManager.For<BattleUnit>();
    
    private Sprite2D _selectionNode;
    
    /** Fields */
    
    
    // UnitData class - TODO
    /** Properties */
    public Unit Unit { get; private set; }
    public int CurrentHealth { get; private set; }
    public List<StatModifier> BattleModifiers { get; } = [];

    public bool IsDefeated => CurrentHealth <= 0;
    
    /** Facade Properties */
    public RangeBand AttackRange => new RangeBand(1, 1); // TODO - base on weapon.
    public bool IsFriendly => Unit.IsFriendly;
    public int MaxHealth => Unit.Stats.BaseStats.MaxHealth;
    public int Movement => Unit.Stats.BaseStats.Movement;
    public string UnitName => Unit.UnitName;
    

    public String Id { get; private set; }

    // Realtime Properties
    private bool _isSelected = false;

    public BattleUnit(Unit unit)
    {
        Unit = unit;
        CurrentHealth = MaxHealth;
    }

    public BattleUnit()
    {
    }

    public override void _Ready()
    {
        _selectionNode = GetNode<Sprite2D>("SelectionNode");
        Id = Name; // Temporary, change to Guid / constructed string when persisting.
    }
    
    /// <summary>
    /// Binds a persistent Unit to this battle instance and initializes battle-only state.
    /// </summary>
    public void Bind(Unit unit)
    {
        _logger.Log($"Bind " + unit.UnitName, LogSeverity.Info, LogCategory.UnitLifecycle);

        Unit = unit;
        CurrentHealth = unit.Stats.BaseStats.MaxHealth;
    }
    
    /// <summary>
    /// Applies damage to CurrentHealth.
    /// </summary>
    public void ApplyDamage(int damage)
    {
        _logger.Log("[BattleUnit] ApplyDamage " + damage, LogSeverity.Info, LogCategory.UnitLifecycle);

        CurrentHealth -= damage;
        if (CurrentHealth < 0)
            CurrentHealth = 0;
    }
    public void Select()
    {
        ToggleSelected(true);
    }

    public void Deselect()
    {
        ToggleSelected(false);
    }

    public void ToggleSelected(bool? force = null)
    {
        _isSelected = force ?? !_isSelected;
        UpdateSelectionUi();
    }

    private void UpdateSelectionUi()
    {
        _selectionNode.Visible = _isSelected;
    }
}

