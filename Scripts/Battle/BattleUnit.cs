using System;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Units;
using Godot;
using Range = Godot.Range;

namespace Goblinos.Scripts.Battle;

public partial class BattleUnit : Area2D
{
    /** Components */
    private Sprite2D _selectionNode;
    
    /** Fields */
    
    
    // UnitData class - TODO
    /** Properties */
    [Export] public string UnitName { get; private set; } = "Goblino";
    [Export] public int MaxHealth { get; private set; } = 20;
    [Export] public int Movement { get; private set; } = 4;
    [Export] public int Power { get; private set; } = 10;
    [Export] private int MinAttackRange { get; set; } = 1;
    [Export] private int MaxAttackRange { get; set; } = 1;
    [Export] public bool IsFriendly { get; private set; } = true;
    public RangeBand AttackRange { get; private set; }

    public String Id { get; private set; }

    // Realtime Properties
    private int _currentHealth;
    private bool _isSelected = false;

    public override void _Ready()
    {
        _selectionNode = GetNode<Sprite2D>("SelectionNode");
        Id = Name; // Temporary, change to Guid / constructed string when persisting.
        AttackRange = new RangeBand(MinAttackRange, MaxAttackRange);
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

