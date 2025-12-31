using Godot;
using System;

public partial class Unit : Area2D
{
    // Unit properties
    [Export] private string _unitName;
    [Export] private int _maxHealth;
    [Export] private int _strength;
    
    // UI elements
    private Sprite2D _selectionElement;
    
    private int _currentHealth;

    private bool _isSelected = false;

    public override void _Ready()
    {
        this._selectionElement = GetNode<Sprite2D>("SelectionNode");
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
        _selectionElement.Visible = _isSelected;
    }

    private void OnInputEvent(Viewport viewport, InputEvent @event, int shape_idx)
    {
        if (@event is InputEventMouseButton mb &&
            mb.ButtonIndex == MouseButton.Left &&
            mb.Pressed)
        {
            GD.Print("Event clicked");
            ToggleSelected();
            viewport.SetInputAsHandled();
        }
    }
}
