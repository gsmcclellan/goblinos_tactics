using Godot;
using System;

public partial class RoomGenerator : Node
{

    private int RoomTileWidth = 12;
    private int RoomTileHeight = 12;

    private int MapWidth = 7;
    private int MapHeight = 7;

    private int RoomsToGenerate = 12;

    private int RoomCount = 0; // number of existing rooms

    private Vector2I FirstRoomPosition;
    private bool RoomsInstantiated;

    public override void _Ready()
    {
        // Initialize empty map
        // generate map
        // Instantiate Rooms
        
        
        
    }
}
