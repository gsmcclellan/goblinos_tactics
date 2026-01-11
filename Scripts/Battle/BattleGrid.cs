#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Terrain;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleGrid : Node2D
{
    /** Components */
    [ExportGroup("Tiles")]
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [Export] private TileMapLayer _terrainLayer;
    [Export] private TileMapLayer _actionPreviewLayer;
    [Export(PropertyHint.Dir)] public string TerrainDbFolder = "res://Terrain";
    #pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private readonly Logger _logger = LogManager.For<BattleGrid>();
    
    /** Fields */
    private const int BasicGroundTilesAtlasId = 0;
    private const int ActionOverlayTilesAtlasId = 1;
    
    private readonly Dictionary<string, TerrainType> _terrainById = new(StringComparer.Ordinal);
    private TerrainType? _defaultTerrain;

    // per-cell cache for querying TerrainType
    private readonly Dictionary<Vector2I, TerrainType?> _terrainAtCellCache = new();
    
    // Preview Data
    private MovementPreviewResults? _movementPreview;
    // private AttackPreviewResults? _attackPreview;
    // private HashSet<Vector2I> _interactCells = new();
    private static readonly HashSet<Vector2I> EmptyCells = new();
    
    /** Properties */
    
    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------
    
    public override void _Ready()
    {
        Debug.Assert(_terrainLayer != null, "[BattleGrid] Terrain Layer not initialized");
        Debug.Assert(_actionPreviewLayer != null, "[BattleGrid] Action Preview Layer not initialized");

        ClearOverlays();
        _loadTerrainDb(TerrainDbFolder);
        
        // Pick a default: either explicit ID (recommended) or first loaded
        _defaultTerrain = _terrainById.TryGetValue("default", out var t) ? t
            : (_terrainById.Count > 0 ? FirstTerrain() : null);
        
        _logger.Log("Ready", LogSeverity.Info, LogCategory.Initialization);
    }
    
    /// <summary>
    /// Loads all TerrainType resources in terrain folder
    /// </summary>
    /// <param name="folder"></param>
    private void _loadTerrainDb(string folder)
    {
        _terrainById.Clear();

        var dir = DirAccess.Open(folder);
        if (dir == null)
        {
            _logger.Error($"[BattleGrid] Terrain folder not found: {folder}");
            return;
        }

        dir.ListDirBegin();
        string file;
        do
        {
            file = dir.GetNext();
            // Skip directories and non .res or .tres files
            if (dir.CurrentIsDir()) continue;
            if (!file.EndsWith(".tres", StringComparison.OrdinalIgnoreCase) &&
                !file.EndsWith(".res", StringComparison.OrdinalIgnoreCase))
                continue;

            var path = $"{folder}/{file}";
            var res = ResourceLoader.Load<TerrainType>(path);
            if (res == null)
            {
                GD.PushWarning($"[BattleGrid] Failed to load TerrainType: {path}");
                continue;
            }
            if (string.IsNullOrWhiteSpace(res.Id))
            {
                GD.PushWarning($"[BattleGrid] TerrainType missing Id: {path}");
                continue;
            }
            if (_terrainById.ContainsKey(res.Id))
            {
                GD.PushWarning($"[BattleGrid] Duplicate TerrainType Id '{res.Id}' at {path}");
                continue;
            }
            
            // Add to cached terrain types
            _terrainById.Add(res.Id, res);
        } while (file != "");
        dir.ListDirEnd();

        _logger.Log("Loaded TerrainTypes: {_terrainById.Count}", 0, LogCategory.Initialization);
    }
    
    // ---------------------------------------------------------------------
    // Public Methods
    // ---------------------------------------------------------------------

    /// <summary>Gets TerrainType for a cell (uses per-cell cache).</summary>
    public bool CanFocusCell(Vector2I cell)
    {
        var terrain = GetTerrainAtCell(cell);
        var canFocus = terrain is { BlocksCursor: false };
        
        _logger.Log("CanFocusCell cell={cell} :: {canFocus}", LogSeverity.Extra, LogCategory.UiNavigation);
        return canFocus;
    }

    public bool CanFocusGlobalPosition(Vector2 globalPos, out Vector2I cell)
    {
        cell = GetCellAtGlobalPosition(globalPos);
        return CanFocusCell(cell);
    }
    
    /// <summary>Clears preview overlay. Use if you change lots of tiles at once.</summary>
    public void ClearOverlays()
    {
        _actionPreviewLayer.Clear();
    }
    
    /// <summary>Clears terrain cache. Use if you change lots of tiles at once.</summary>
    public void ClearTerrainCache()
    {
        _terrainAtCellCache.Clear();
    }
    
    public Vector2 GetGlobalCenterPositionForCell(Vector2I cell)
    {
        var localPos = _terrainLayer.MapToLocal(cell);
        var tileSize = _terrainLayer.TileSet.TileSize;
        localPos += tileSize / 2;

        return _terrainLayer.ToGlobal(localPos);
    }

    public Vector2 GetGlobalPositionForCell(Vector2I cell)
    {
        var localPos = _terrainLayer.MapToLocal(cell);
        return _terrainLayer.ToGlobal(localPos);
    }
    
    /// <summary>
    /// Returns cell coordinates for a given globalPos
    /// </summary>
    /// <param name="globalPos"></param>
    /// <returns></returns>
    public Vector2I GetCellAtGlobalPosition(Vector2 globalPos)
    {
        var localPos = _terrainLayer.ToLocal(globalPos);
        return _terrainLayer.LocalToMap(localPos);
    }
    
    /// <summary>
    /// Returns TerrainType data for a given Vector2I cell
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public TerrainType? GetTerrainAtCell(Vector2I cell)
    {
        if (_terrainAtCellCache.TryGetValue(cell, out var cached))
            return cached;

        var tileData = _terrainLayer.GetCellTileData(cell);
        if (tileData == null)
            return Cache(cell, null);

        // TileSet custom data key added to terrain in TileMapLayer
        var v = tileData.GetCustomData("terrain_id");
        if (!DebugUtil.Require(v.VariantType == Variant.Type.String, $"Tile at {cell} is missing a valid 'terrain_id' custom data."))
            return Cache(cell, _defaultTerrain);

        var id = v.AsString();
        if (!string.IsNullOrEmpty(id) && _terrainById.TryGetValue(id, out var terrain))
            return Cache(cell, terrain);

        // If the tile is painted but missing terrain_id, use default
        return Cache(cell, _defaultTerrain);
    }
    
    /// <summary>Call this if the tile at a cell changes, to refresh cached terrain.</summary>
    public void InvalidateTerrainCacheAt(Vector2I cell)
    {
        _terrainAtCellCache.Remove(cell);
    }
    
    public void SetMovementPreview(MovementPreviewResults preview)
    {
        _movementPreview = preview;
        RedrawOverlay();
    }

    /// <summary>
    /// Returns true if TerrainType terrain exists at given cell coordindates, out var terrain
    /// </summary>
    /// <param name="cell"></param>
    /// <param name="terrain"></param>
    /// <returns></returns>
    public bool TryGetTerrainAtCell(Vector2I cell, out TerrainType terrain)
    {
        var t = GetTerrainAtCell(cell);
        if (t == null)
        {
            terrain = null!;
            return false;
        }

        terrain = t;
        return true;
    }
    
    
    
    // ---------------------------------------------------------------------
    // Private Methods
    // ---------------------------------------------------------------------
    
    /// <summary>Add TerrainType associated with cell to cache for quick access when making many queries</summary>
    /// <param name="cell">Vector2I position of terrain</param>
    /// <param name="terrain">TerrainType object containing data</param>
    /// <returns></returns>
    private TerrainType? Cache(Vector2I cell, TerrainType? terrain)
    {
        _terrainAtCellCache[cell] = terrain;
        return terrain;
    }
    
    //
    //
    // public void DisplayMovementPreview(MovementPreviewResults movementPreview)
    // {
    //     _actionPreviewLayer.Visible = true;
    //     _actionPreviewLayer.Clear();
    //
    //     const int sourceId = GridNavigationUtil.ActionOverlayTilesAtlasId;
    //     var atlasCoords = OverlayTypeToVector2I(ActionOverlayType.Movement);
    //     
    //     // TODO - paint all cells blue
    //     foreach (var cell in movementPreview.CostByCell.Keys)
    //     {
    //         _actionPreviewLayer.SetCell(cell, sourceId, atlasCoords);
    //     }
    // }

    
    
    
    
    
    /// <summary>
    /// Returns first TerrainType from file in directory
    /// </summary>
    /// <returns></returns>
    private TerrainType? FirstTerrain()
    {
        foreach (var kv in _terrainById) return kv.Value;
        return null;
    }

    private Vector2I OverlayTypeToVector2I(ActionOverlayType t)
    {
        return new Vector2I((int)t, 0);
    }

    private void RedrawOverlay()
    {
        ClearOverlays();
        // Attack cells
        // Movement cells
        // Interaction cells
        if (!DebugUtil.Require(_actionPreviewLayer != null, "Unable to draw overlay, _actionPreviewLayer not initialized")) 
            return;

        _actionPreviewLayer.Visible = true;
        
        var moveCells = _movementPreview?.Cells ?? EmptyCells;
        var attackCells = /*_attackPreview?.Cells ?? */EmptyCells; // TODO
        
        // Movement takes priority - cell legal for move & attack show as movement
        var attackOnly = new HashSet<Vector2I>(attackCells);
        attackOnly.ExceptWith(moveCells);
        
        // Combined set of cells to draw
        var renderedCells = new HashSet<Vector2I>(moveCells);
        renderedCells.UnionWith(moveCells);
        // renderedCells.UnionWith(_interactCells); TODO
        
        ClearOverlays(); // TODO - clear selectively by passing renderedCells & ignoring everything else

        foreach (var cell in renderedCells)
        {
            ActionOverlayType? overlayType;
            // if (_interactCells.Contains(cell))
            //     overlayType = ActionOverlayType.Interact;
            /*else*/
            if (moveCells.Contains(cell))
                overlayType = ActionOverlayType.Movement;
            else if (attackOnly.Contains(cell))
                overlayType = ActionOverlayType.Attack;
            else
            {
                _logger.Warn($"Redraw Overlay - Unknown type for cell={cell}");
                overlayType = null;
            } 
            
            if (overlayType.HasValue)
                _actionPreviewLayer.SetCell(cell, ActionOverlayTilesAtlasId, OverlayTypeToVector2I(overlayType.Value));
        }

        if (renderedCells.Count == 0)
            _actionPreviewLayer.Visible = false; // hide empty overlay
        
        _logger.Log($"RedrawOverlay cellCount={renderedCells.Count}", LogSeverity.Trace, LogCategory.BattleState);
    }
}

public enum GridOverlayType
{
    MoveRange,
    AttackRange,
    MoveAndAttackRange,
    EnemyAttackRange,
    AbilityRange
}

public enum ActionOverlayType
{
    Movement = 0,
    Attack = 1,
    Interact = 2,
    Warning = 3
}