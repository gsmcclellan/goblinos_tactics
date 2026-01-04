#nullable enable
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class UnitSelectionController: Node
{
    /** Signals */
    [Signal] public delegate void SelectedUnitChanged(Node? selectedUnit);

    [Signal]
    public delegate void HoveredUnitChanged(Node? hoveredUnit);
    // SelectedTerrain
    // HoveredTerrain
    // SelectedCell
    // HoveredCell

    /** Components */
    [Export]
    private NodePath _cursorPath;
    [Export]
    private NodePath _unitRegistryPath;

    private GridCursor _cursor;
    private UnitRegistry _unitRegistry;

    /** Fields */
    private Vector2I _hoveredCell;
    private Vector2I _selectedCell;

    private BattleUnit? _hoveredUnit;
    private BattleUnit? _selectedUnit;

    /** Properties */
    public BattleUnit? HoveredUnit => _hoveredUnit;
    public BattleUnit? SelectedUnit => _selectedUnit;

    public bool IsUnitHovered => _hoveredUnit != null;
    public bool IsUnitSelected => _selectedUnit != null;
    
    // SelectHovered()
    // ClearSelection()
}