using System.Collections.Generic;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Units;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

/// <summary>
/// Authoritative registry of all units participating in a single battle.
/// Responsible for unit participation, spatial occupancy, and battle-scoped
/// unit lifecycle events. Commits final results of movement events.
/// </summary>
public partial class UnitRegistry: Node
{
    /** Signals */
    [Signal]
    public delegate void UnitMoveResolvedEventHandler(BattleUnit unit, Vector2I fromCell, Vector2I toCell);
    [Signal]
    public delegate void UnitRegisteredEventHandler(BattleUnit unit, Vector2I cell);
    [Signal]
    public delegate void UnitUnregisteredEventHandler(BattleUnit unit, Vector2I cell);
    
    // [Signal]
    // public delegate void UnitDiedEventHandler(BattleUnit unit, Vector2I cell);

    /** Fields */
    private Logger _logger = LogManager.For<UnitRegistry>();

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
        _SubscribeToEvents();
    }

    public override void _ExitTree()
    {
        _UnsubscribeFromEvents();
    }

    private void _SubscribeToEvents()
    {
    }

    private void _UnsubscribeFromEvents()
    {
    }

    /** Registration / Lifecycle */

    /// <summary>
    /// Registers a unit as participating in the battle.
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="initialCell"></param>
    public void RegisterUnit(BattleUnit unit, Vector2I initialCell)
    {
        _logger.Log($"RegisterUnit unit={unit}", LogSeverity.Info, LogCategory.UnitLifecycle);
        if (!DebugUtil.Require(unit != null, "Cannot register null unit."))
            return;
        
        // Check cell for existing unit & that unit isn't already registered
        var cellEmpty = !IsCellOccupied(initialCell);
        var isUnregistered = !Contains(unit);
        if (!DebugUtil.Require(cellEmpty, "Cannot register unit to non-empty cell.") ||
            !DebugUtil.Require(isUnregistered, "Unable to register unit, already registered."))
            return;

        // Add to both dicts
        _units.Add(unit);
        _unitsByCell[initialCell] = unit;
        _cellsByUnit[unit] = initialCell;

        _AssertInvariants();
        EmitSignal(SignalName.UnitRegistered, unit, initialCell);
    }

    /// <summary>
    /// Unregisters a unit from the battle.
    /// </summary>
    public void UnregisterUnit(BattleUnit unit)
    {
        _logger.Log("UnregisterUnit " + unit, LogSeverity.Trace, LogCategory.UnitLifecycle);
        if (!DebugUtil.Require(unit != null, "Cannot unregister null unit."))
            return;
        
        var isRegistered = Contains(unit);
        if (!DebugUtil.Require(isRegistered, "[UnitRegistry] Unable to unregister unit, not registered."))
            return;
        
        var cell = _cellsByUnit[unit];
        _units.Remove(unit);
        _unitsByCell.Remove(cell);
        _cellsByUnit.Remove(unit);

        _AssertInvariants();
        EmitSignal(SignalName.UnitUnregistered, unit, cell);
    }

    // public void HandleUnitDeath(BattleUnit unit) TODO - unit death, finish here or move elsewhere to other battle controller
    // {
    //     // Unit has died
    //     // kill unit
    //     
    //     // unregister unit
    //     // send signal
    // }
    //
    // /// <summary>
    // /// Marks a unit as dead for the purposes of this battle.
    // /// </summary>
    // public void NotifyUnitDied(BattleUnit unit)
    // {
    //     var containsUnit = Contains(unit);
    //     DebugUtil.Require(containsUnit, "Unit died, already unregistered");
    //     if (!containsUnit)
    //         return;
    //     
    //     var cell = _cellsByUnit[unit];
    //     
    //     _logger.Log($"[Signal] UnitDied unit={unit} cell={cell}" + unit, LogSeverity.Trace, LogCategory.Signal);
    //     _logger.Log("NotifyUnitDied " + unit, LogSeverity.Trace, LogCategory.UnitLifecycle);
    //     EmitSignal(SignalName.UnitDied, unit, cell);
    // }

    /// <summary>
    /// Clears all registered units. Intended for battle teardown.
    /// </summary>
    public void Clear()
    {
        _logger.Log("Clear", LogSeverity.Info, LogCategory.Exit);
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
        var hasUnit = unit != null && _cellsByUnit.ContainsKey(unit);

        _logger.Log($"Contains [unit]={unit} :: {hasUnit}", LogSeverity.Extra, LogCategory.DebugOnly);
        return hasUnit;
    }

    /// <summary>
    /// Enumerates units belonging to the given team/faction.
    /// </summary>
    // public IEnumerable<BattleUnit> GetUnitsByTeam(UnitTeam team)
    // {
    //     _logger.Log("GetUnitsByTeam " + team, LogSeverity.Extra, LogCategory.DebugOnly);
    //
    //     yield break;
    // }

    /// <summary>
    /// Enumerates all units that are still alive.
    /// </summary>
    public IEnumerable<BattleUnit> GetLivingUnits()
    {
        _logger.Log("GetLivingUnits", LogSeverity.Extra, LogCategory.DebugOnly);

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
        _logger.Log($"IsCellOccupied [cell]={cell} :: {isOccupied}", LogSeverity.Extra, LogCategory.DebugOnly);

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
        _logger.Log($"TryGetCell [unit]={unit} [hasCell]={hasCell} :: [cell]={cell}", LogSeverity.Extra, LogCategory.DebugOnly);

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
        
        _logger.Log($"TryGetUnitAtCell [cell]={cell} [hasUnit]={hasUnit} :: [unit]={unit}", LogSeverity.Extra, LogCategory.DebugOnly);
        return hasUnit;
    }
    
    /** Movement */

    /// <summary>
    /// Updates the registry when a unit moves between cells.
    /// </summary>
    public void ApplyUnitMove(BattleUnit unit, Vector2I fromCell, Vector2I toCell)
    {
        // TODO - currently allows only move to empty cell, add additional functionality for shove, swap, etc.
        _logger.Log($"ApplyUnitMove [unit]={unit.UnitName} [from]={fromCell} [to]={toCell}", LogSeverity.Info, LogCategory.UnitLifecycle);

        if (!DebugUtil.Require(unit != null, "[UnitRegistry] Cannot move null unit."))
            return;
        
        var isUnitRegistered = TryGetCell(unit, out var unitCell);
        if (!DebugUtil.Require(isUnitRegistered, "[UnitRegistry] Cannot move unregistered unit."))
            return;
        
        var noOpMove = fromCell == toCell;
        if (!DebugUtil.Require(!noOpMove, "[UnitRegistry] No op move, fromCell == toCell"))
            return;
        
        var fromCellMatches = unitCell == fromCell;
        var isDestinationEmpty = !_unitsByCell.ContainsKey(toCell);

        if (!DebugUtil.Require(fromCellMatches, "[UnitRegistry] fromCell does not match existing location.") ||
            !DebugUtil.Require(isDestinationEmpty, "[UnitRegistry] toCell not empty"))
            return;

        _cellsByUnit[unit] = toCell;
        _unitsByCell.Remove(fromCell);
        _unitsByCell[toCell] = unit;

        _logger.Info($"[Signal] UnitMoveResolved unit={unit.UnitName}, fromCell={fromCell}, toCell={toCell}", LogCategory.Signal);
        _AssertInvariants();
        EmitSignal(SignalName.UnitMoveResolved, unit, fromCell, toCell);
    }
    
    
    // ---------------------------------------------------------------------
    // DEBUG
    // ---------------------------------------------------------------------
    
    [Conditional("DEBUG")]
    private void _AssertInvariants()
    {
        Debug.Assert(_units.Count == _cellsByUnit.Count, "Unit count mismatch");
        Debug.Assert(_cellsByUnit.Count == _unitsByCell.Count, "Cell mapping mismatch");

        foreach (var unit in _units)
            Debug.Assert(_cellsByUnit.ContainsKey(unit), "Unit list contains unregistered unit");
        
        foreach (var (unit, cell) in _cellsByUnit)
        {
            Debug.Assert(_unitsByCell.TryGetValue(cell, out var mappedUnit),
                "Cell missing from unitsByCell");

            Debug.Assert(mappedUnit == unit,
                "Roundtrip mapping mismatch");
        }
    }
}