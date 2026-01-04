#nullable enable
using Godot;
using System.Collections.Generic;

namespace Goblinos.Scripts.Battle.Terrain;

[GlobalClass]
public partial class TerrainType : Resource
{
    [Export] public string Id { get; set; } = "";               // "forest", "road"
    [Export] public string DisplayName { get; set; } = "";      // "Forest"
    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = "";

    // UI / visualization
    [Export] public Texture2D? Icon { get; set; }
    [Export] public Color OverlayColor { get; set; } = Colors.White;

    // Core rules
    [Export] public float MoveCost { get; set; } = 1;
    [Export] public int DefenseBonus { get; set; } = 0;
    [Export] public bool BlocksLos { get; set; } = false;
    [Export] public bool BlocksCursor { get; set; } = false;

    // Optional: lightweight tags (for logic like "flammable", "water", etc.)
    [Export] public Godot.Collections.Array<string> Tags { get; set; } = new();
    
    public bool BlocksMovement => MoveCost == 0;

    public bool HasTag(string tag)
    {
        for (int i = 0; i < Tags.Count; i++)
            if (Tags[i] == tag) return true;
        return false;
    }
}