#nullable enable
using Godot;
using System;
using System.Collections.Generic;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Terrain;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Util;

public partial class BattleGrid : Node2D
{
    [ExportGroup("Tiles")]
    [Export] public TileMapLayer TerrainLayer;
    [Export(PropertyHint.Dir)] public string TerrainDbFolder = "res://Terrain";

    private Logger _logger = LogManager.For<BattleGrid>();
    
    private readonly Dictionary<string, TerrainType> _terrainById = new(StringComparer.Ordinal);
    private TerrainType? _defaultTerrain;

    // Optional per-cell cache (handy if you query a lot)
    private readonly Dictionary<Vector2I, TerrainType?> _terrainAtCellCache = new();
    
    public override void _Ready()
    {
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
    public void _loadTerrainDb(string folder)
    {
        _terrainById.Clear();

        var dir = DirAccess.Open(folder);
        if (dir == null)
        {
            _logger.Error($"[BattleGrid] Terrain folder not found: {folder}");
            return;
        }

        dir.ListDirBegin();
        var file = "";
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
    
    public Vector2 GetGlobalCenterPositionForCell(Vector2I cell)
    {
        var localPos = TerrainLayer.MapToLocal(cell);
        var tileSize = TerrainLayer.TileSet.TileSize;
        localPos += tileSize / 2;

        return TerrainLayer.ToGlobal(localPos);
    }

    public Vector2 GetGlobalPositionForCell(Vector2I cell)
    {
        var localPos = TerrainLayer.MapToLocal(cell);
        return TerrainLayer.ToGlobal(localPos);
    }
    
    /// <summary>
    /// Returns cell coordinates for a given globalPos
    /// </summary>
    /// <param name="globalPos"></param>
    /// <returns></returns>
    public Vector2I GetCellAtGlobalPosition(Vector2 globalPos)
    {
        var localPos = TerrainLayer.ToLocal(globalPos);
        return TerrainLayer.LocalToMap(localPos);
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

        var tileData = TerrainLayer.GetCellTileData(cell);
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
    
    
    /// <summary>Call this if the tile at a cell changes, to refresh cached terrain.</summary>
    public void InvalidateTerrainCacheAt(Vector2I cell)
    {
        _terrainAtCellCache.Remove(cell);
    }
    
    /// <summary>Add TerrainType associated with cell to cache for quick access when making many queries</summary>
    /// <param name="cell">Vector2I position of terrain</param>
    /// <param name="terrain">TerrainType object containing data</param>
    /// <returns></returns>
    private TerrainType? Cache(Vector2I cell, TerrainType? terrain)
    {
        _terrainAtCellCache[cell] = terrain;
        return terrain;
    }
    
    /// <summary>Clears terrain cache. Use if you change lots of tiles at once.</summary>
    public void ClearTerrainCache()
    {
        _terrainAtCellCache.Clear();
    }
    
    /// <summary>
    /// Returns first TerrainType from file in directory
    /// </summary>
    /// <returns></returns>
    private TerrainType? FirstTerrain()
    {
        foreach (var kv in _terrainById) return kv.Value;
        return null;
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