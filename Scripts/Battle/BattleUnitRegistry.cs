using System.Collections.Generic;
using System.Diagnostics;
using Goblinos.Scripts.Units;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

/// <summary>
/// Authoritative registry of all units participating in a single battle.
/// Responsible for unit participation, spatial occupancy, and battle-scoped
/// unit lifecycle events. Commits final results of movement events.
/// </summary>
public partial class BattleUnitRegistry: Node
{
    /** Signals */
    [Signal]
    public delegate void UnitRegisteredEventHandler(BattleUnit unit, Vector2I cell);
    [Signal]
    public delegate void UnitUnregisteredEventHandler(BattleUnit unit, Vector2I cell);
    [Signal]
    public delegate void UnitMoveResolvedEventHandler(BattleUnit unit, Vector2I fromCell, Vector2I toCell);
    [Signal]
    public delegate void UnitDiedEventHandler(BattleUnit unit, Vector2I cell);
    
    /** Fields */

    private readonly List<BattleUnit> _units = new();
    private readonly Dictionary<Vector2I, BattleUnit> _unitsByCell = new();
    private readonly Dictionary<BattleUnit, Vector2I> _cellsByUnit = new();

    /* Properties */

    /// <summary>
    /// Read-only list of all units currently participating in the battle.
    /// </summary>
    public IReadOnlyList<BattleUnit> Units => _units;

    public override void _Ready()
    {
        _SetupSubscriptions();
    }

    public override void _ExitTree()
    {
        _RemoveSubscriptions();
    }

    private void _SetupSubscriptions()
    {
        // Listen to unit death, update registry
    }

    private void _RemoveSubscriptions()
    {
    }

    /** Registration / Lifecycle */

    /// <summary>
    /// Registers a unit as participating in the battle.
    /// <param name="unit"></param>
    /// <param name="initialCell"></param>
    /// </summary>
    public void RegisterUnit(BattleUnit unit, Vector2I initialCell)
    {
        DebugUtil.Log("[BattleUnitRegistry] RegisterUnit " + unit, DebugLogSeverity.Trace, DebugLogCategory.UnitLifecycle);
        
        // Check cell for existing unit & that unit isn't already registered
        var cellEmpty = !IsCellOccupied(initialCell);
        var isUnregistered = !Contains(unit);
        Debug.Assert(cellEmpty, "Cannot register unit to non-empty cell.");
        Debug.Assert(isUnregistered, "Unable to register unit, already registered.");
        if (!cellEmpty || !isUnregistered)
            return;

        // Add to both dicts
        _units.Add(unit);
        _unitsByCell[initialCell] = unit;
        _cellsByUnit[unit] = initialCell;
    }

    /// <summary>
    /// Unregisters a unit from the battle.
    /// </summary>
    public void UnregisterUnit(BattleUnit unit)
    {
        DebugUtil.Log("[BattleUnitRegistry] UnregisterUnit " + unit, DebugLogSeverity.Trace, DebugLogCategory.UnitLifecycle);
        var isRegistered = Contains(unit);
        Debug.Assert(isRegistered, "Unable to unregister unit, not registered.");
        if (!isRegistered)
            return;

        var cell = _cellsByUnit[unit];
        _unitsByCell.Remove(cell);
        _cellsByUnit.Remove(unit);
    }

    /// <summary>
    /// Marks a unit as dead for the purposes of this battle.
    /// </summary>
    public void NotifyUnitDied(BattleUnit unit)
    {
        var containsUnit = Contains(unit);
        Debug.Assert(containsUnit, "Unit died, already unregistered");
        if (!containsUnit)
            return;
        
        var cell = _cellsByUnit[unit];
        
        DebugUtil.Log($"[BattleUnitRegistry] [Signal] UnitDied unit={unit} cell={cell}" + unit, DebugLogSeverity.Trace, DebugLogCategory.Signal);
        DebugUtil.Log("[BattleUnitRegistry] NotifyUnitDied " + unit, DebugLogSeverity.Trace, DebugLogCategory.UnitLifecycle);
        EmitSignal(SignalName.UnitDied, unit, cell);
        
        
    }

    /// <summary>
    /// Clears all registered units. Intended for battle teardown.
    /// </summary>
    public void Clear()
    {
        DebugUtil.Log("[BattleUnitRegistry] Clear", DebugLogSeverity.Info, DebugLogCategory.Exit);
        _units.Clear();
        _unitsByCell.Clear();
        _cellsByUnit.Clear();
    }
    
    /* Filtering / Queries */

    /// <summary>
    /// Determines if the registry has a given unit
    /// </summary>
    /// <param name="unit"></param>
    /// <returns>true if unit is registered</returns>
    public bool Contains(BattleUnit unit)
    {
        var hasUnit = _units.Contains(unit);

        DebugUtil.Log($"[BattleUnitRegistry] Contains [unit]={unit} :: {hasUnit}", DebugLogSeverity.Extra, DebugLogCategory.DebugOnly);
        return hasUnit;
    }

    /// <summary>
    /// Enumerates units belonging to the given team/faction.
    /// </summary>
    public IEnumerable<BattleUnit> GetUnitsByTeam(UnitTeam team)
    {
        DebugUtil.Log("[BattleUnitRegistry] GetUnitsByTeam " + team, DebugLogSeverity.Extra, DebugLogCategory.DebugOnly);

        yield break;
    }

    /// <summary>
    /// Enumerates all units that are still alive.
    /// </summary>
    public IEnumerable<BattleUnit> GetLivingUnits()
    {
        DebugUtil.Log("[BattleUnitRegistry] GetLivingUnits", DebugLogSeverity.Extra, DebugLogCategory.DebugOnly);

        yield break;
    }

    /// <summary>
    /// Determines if a cell is occupied by a unit
    /// </summary>
    /// <param name="cell"></param>
    /// <returns>true if cell is occupied</returns>
    public bool IsCellOccupied(Vector2I cell)
    {
        var isOccupied = _unitsByCell.ContainsKey(cell);
        DebugUtil.Log($"[BattleUnitRegistry] IsCellOccupied [cell]={cell} :: {isOccupied}", DebugLogSeverity.Info, DebugLogCategory.UnitLifecycle);

        return isOccupied;
    }

    /// <summary>
    /// Determines if a unit has a cell associated with it, out property is the cell
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="cell">Vector2I of the unit</param>
    /// <returns>true if unit has a cell associated with it</returns>
    public bool TryGetCell(BattleUnit unit, out Vector2I cell)
    {
        var hasCell = _cellsByUnit.TryGetValue(unit, out cell);
        DebugUtil.Log($"[BattleUnitRegistry] TryGetCell [unit]={unit} [hasCell]={hasCell} :: [cell]={cell}", DebugLogSeverity.Info, DebugLogCategory.UnitLifecycle);

        return hasCell;
    }

    /// <summary>
    /// Determines if cell contains a unit, out property is the unit
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="unit"></param>
    /// <returns>true if cell is occupied</returns>
    public bool TryGetUnitAtCell(Vector2I cell, out BattleUnit unit)
    {
        var hasUnit = _unitsByCell.TryGetValue(cell, out unit);
        
        DebugUtil.Log($"[BattleUnitRegistry] TryGetUnitAtCell [cell]={cell} [hasUnit]={hasUnit} :: [unit]={unit}", DebugLogSeverity.Info, DebugLogCategory.UnitLifecycle);
        return hasUnit;
    }
    
    /** Movement */

    /// <summary>
    /// Updates the registry when a unit moves between cells.
    /// </summary>
    public void ApplyUnitMove(BattleUnit unit, Vector2I fromCell, Vector2I toCell)
    {
        DebugUtil.Log($"[BattleUnitRegistry] ApplyUnitMove [unit]={unit} [from]={fromCell} [to]={toCell}", DebugLogSeverity.Info, DebugLogCategory.UnitLifecycle);
    }
}