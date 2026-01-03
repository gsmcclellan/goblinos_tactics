using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleUnit : Area2D
{
    // Unit properties
    // UnitData class - TODO
    [Export] private string _unitName;
    [Export] private int _maxHealth;
    [Export] private int _strength;
    
    
    
    // UI elements
    private Sprite2D _selectionNode;
    
    // Realtime Properties
    private int _currentHealth;
    private bool _isSelected = false;

    public override void _Ready()
    {
        this._selectionNode = GetNode<Sprite2D>("SelectionNode");
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

