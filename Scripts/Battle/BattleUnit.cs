using System;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleUnit : Area2D
{
    // UI elements
    private Sprite2D _selectionNode;
    
    // Unit properties
    // UnitData class - TODO
    [Export] public string UnitName { get; private set; } = "Goblino";
    [Export] public int MaxHealth { get; private set; } = 20;
    [Export] public int Movement { get; private set; } = 4;
    [Export] public int Power { get; private set; } = 10;

    public String Id { get; private set; }

    public bool IsFriendly => true;
    
    // Realtime Properties
    private int _currentHealth;
    private bool _isSelected = false;

    public override void _Ready()
    {
        _selectionNode = GetNode<Sprite2D>("SelectionNode");
        Id = Name; // Temporary, change to Guid / constructed string when persisting.
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

