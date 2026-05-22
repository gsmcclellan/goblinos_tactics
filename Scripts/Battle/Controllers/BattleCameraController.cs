using System;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Core;
using Goblinos.Scripts.Battle.Core.Types;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Controllers;

/// <summary>
/// Owns battle camera movement, zoom, and bounds clamping.
/// This controller is intentionally separate from BattleController so
/// battle flow and viewport behavior remain independent.
/// </summary>
public partial class BattleCameraController : Node, IInputHandler
{
    /** Components */
    private readonly GobLogger _logger = GobLogManager.For<BattleCameraController>();

    [Export]
    private Camera2D _camera;

    private BattleGrid _grid;
    private GridCursor _cursor;
    
    /** Fields */
    private Rect2 _cameraWorldBounds;
    private Vector2 _heldKeyboardInputDirection;
    
    
    
    [Export] private float _keyboardPanSpeed = 200.0f;
    [Export] private float _dragPanMultiplier = 0.25f;
    [Export] private Vector2 _defaultZoom = new(2f, 2f);
    [Export] private Vector2 _maxZoom = new(4f, 4f);
    [Export] private float _zoomStep = 0.5f;
    [Export] private int _autoPanBufferCells = 1;

    private bool _isCameraInputEnabled = true;
    private bool _isDragPanning;

    public bool BlocksLowerInputHandlers { get; } = false;
    
    // ---------------------------------------------------------------------
    // Lifecycle / Callback Methods
    // ---------------------------------------------------------------------

    public override void _Ready()
    {
        DebugUtil.Require(_camera != null, $"[{nameof(BattleCameraController)}] Initialization failed - no {nameof(_camera)}");
    }

    // Additional setup with necessary linked components from BattleController
    public void Bind(BattleGrid grid, GridCursor cursor)
    {
        _logger.Log("Bind", GobLogSeverity.Info, GobLogCategory.Initialization);

        _grid = grid;
        _cursor = cursor;
        
        Debug.Assert(_camera != null, $"[{nameof(BattleCameraController)}] Camera must be assigned.");
        Debug.Assert(_grid != null, $"[{nameof(BattleCameraController)}] {nameof(BattleGrid)} must be bound.");
        Debug.Assert(_cursor != null, $"[{nameof(BattleCameraController)}] {nameof(GridCursor)} must be bound.");
        
        Rect2 worldBounds = _grid.GetMapRectGlobal();
        _cameraWorldBounds = worldBounds;
        Debug.Assert(_cameraWorldBounds.Size.X > 0, $"[{nameof(BattleCameraController)}] Camera bounds width must be positive.");
        Debug.Assert(_cameraWorldBounds.Size.Y > 0, $"[{nameof(BattleCameraController)}] Camera bounds height must be positive.");
        
        _SubscribeToEvents();
        
        ApplyZoomedInDefault();
        ApplyCameraLimits();
    }
    
    public override void _ExitTree()
    {
        _logger.Log("_ExitTree", GobLogSeverity.Info, GobLogCategory.Exit);
        _UnsubscribeFromEvents();
    }
    
    private void _SubscribeToEvents()
    {
        _logger.Log($"{nameof(_SubscribeToEvents)}", GobLogSeverity.Info, GobLogCategory.Initialization);
        _cursor.GridCursorFocusChanged += OnGridCursorFocusChanged;
    }

    private void _UnsubscribeFromEvents()
    {
        _logger.Log($"{nameof(_UnsubscribeFromEvents)}", GobLogSeverity.Info, GobLogCategory.Exit);
        _cursor.GridCursorFocusChanged -= OnGridCursorFocusChanged;
    }
    
    // ---------------------------------------------------------------------
    // Input Handling
    // ---------------------------------------------------------------------
    
    public bool HandleRoutedInput(InputEvent e)
    {
        // keyboard panning
        if (e.IsActionPressed("camera_pan_up"))    { return HandleKeyboardPanPressed(InputDirection.Up); }
        if (e.IsActionPressed("camera_pan_right"))    { return HandleKeyboardPanPressed(InputDirection.Right); }
        if (e.IsActionPressed("camera_pan_down"))    { return HandleKeyboardPanPressed(InputDirection.Down); }
        if (e.IsActionPressed("camera_pan_left"))    { return HandleKeyboardPanPressed(InputDirection.Left); }

        if (e.IsActionReleased("camera_pan_up") || e.IsActionReleased("camera_pan_right") ||
            e.IsActionReleased("camera_pan_down") || e.IsActionReleased("camera_pan_left"))
            return HandleKeyboardPanReleased();

        if (e.IsActionPressed("camera_drag_pan"))
        {
            _isDragPanning = true;
            return true;
        }
        
        if (e.IsActionReleased("camera_drag_pan"))
        {
            _isDragPanning = false;
            return true;
        }
        
        // Camera zoom
        if (e.IsActionPressed("camera_zoom_in"))
        {
            ZoomIn();
            return true;
        }

        if (e.IsActionPressed("camera_zoom_out"))
        {
            ZoomOut();
            return true;
        }
        
        // Mouse - Motion
        if (e is InputEventMouseMotion mme && _isDragPanning)
        {
            return HandleDragPan(mme);
        }

        return false;
    }
    
    /// <summary>
    /// Processes camera panning input each frame.
    /// </summary>
    public override void _Process(double delta)
    {
        _logger.Log("Process Camera Input", GobLogSeverity.Extra, GobLogCategory.Input);
        
        HandleKeyboardPan((float)delta);
    }
    
    // ---------------------------------------------------------------------
    // Public Methods
    // ---------------------------------------------------------------------

    public void CenterOnCell(Vector2I cell)
    {
        _logger.Log($"{nameof(CenterOnCell)} cell={cell}", GobLogSeverity.Trace, GobLogCategory.UiNavigation);
        var globalPos = _grid.GetGlobalCenterPositionForCell(cell);
        _camera.GlobalPosition = globalPos;
    }

    public void RepositionToIncludeCell(Vector2I cell)
    {
        _logger.Log($"{nameof(RepositionToIncludeCell)} cell={cell}", GobLogSeverity.Trace, GobLogCategory.UiNavigation);
        
        Vector2 cellTopLeft = _grid.GetGlobalTopLeftPositionForCell(cell);
        Vector2 cellSize = new Vector2I(GlobalSettings.TileSize, GlobalSettings.TileSize);
        Rect2 cellRect = new(cellTopLeft, cellSize);
        
        var visibleWorldRect = GetCameraVisibleWorldRect();
        var newCameraPosition = visibleWorldRect.GetCenter();

        var panBuffer = _autoPanBufferCells * GlobalSettings.TileSize;

        var leftLimit = visibleWorldRect.Position.X + panBuffer;
        var rightLimit = visibleWorldRect.End.X - panBuffer;
        var upLimit = visibleWorldRect.Position.Y + panBuffer;
        var downLimit = visibleWorldRect.End.Y - panBuffer;
        
        if (cellRect.Position.X < leftLimit)
            newCameraPosition.X -= leftLimit - cellRect.Position.X;
        else if (cellRect.End.X > rightLimit)
            newCameraPosition.X += cellRect.End.X - rightLimit;

        if (cellRect.Position.Y < upLimit)
            newCameraPosition.Y -= upLimit - cellRect.Position.Y;
        else if (cellRect.End.Y > visibleWorldRect.End.Y)
            newCameraPosition.Y += cellRect.End.Y - downLimit;
            
        
        _camera.GlobalPosition = newCameraPosition;
    }

    public void ScrollByCell(InputDirection dir, int numCells = 1)
    {
        _logger.Log($"{nameof(ScrollByCell)} dir={dir}, numCells={numCells}", GobLogSeverity.Trace, GobLogCategory.UiNavigation);
        var globalPosChange = numCells * GlobalSettings.TileSize;

        _camera.GlobalPosition += globalPosChange * InputUtil.InputDirectionToVector2I(dir);
    }

    public void ZoomIn() => ZoomTo(_camera.Zoom + new Vector2(_zoomStep, _zoomStep));
    public void ZoomOut() => ZoomTo(_camera.Zoom - new Vector2(_zoomStep, _zoomStep));
    
    // ---------------------------------------------------------------------
    // Event Handlers
    // ---------------------------------------------------------------------

    private void OnGridCursorFocusChanged(Vector2I newCell, Vector2I oldCell, int gridCursorFocusSource)
    {
        if ((GridCursorFocusSource)gridCursorFocusSource == GridCursorFocusSource.Mouse)
            return;
        
        RepositionToIncludeCell(newCell);
    }
    
    // ---------------------------------------------------------------------
    // Private Methods
    // ---------------------------------------------------------------------

    /// <summary>
    /// Returns the camera's currently visible world-space rectangle,
    /// accounting for viewport size and zoom.
    /// </summary>
    private Rect2 GetCameraVisibleWorldRect()
    {
        _logger.Log(nameof(GetCameraVisibleWorldRect), GobLogSeverity.Extra, GobLogCategory.UiNavigation);

        Vector2 viewportSize = _camera.GetViewportRect().Size;
        Vector2 visibleWorldSize = viewportSize / _camera.Zoom;
        Vector2 topLeft = _camera.GlobalPosition - (visibleWorldSize * 0.5f);
        
        Vector2 clampedTopLeft = new Vector2(Math.Clamp(topLeft.X, _camera.LimitLeft, _camera.LimitRight - visibleWorldSize.X),
            Math.Clamp(topLeft.Y, _camera.LimitTop, _camera.LimitBottom - visibleWorldSize.Y));

        return new Rect2(clampedTopLeft, visibleWorldSize);
    }
    
    private bool HandleKeyboardPanPressed(InputDirection dir)
    {
        _logger.Log("HandleKeyboardPanPressed", GobLogSeverity.Trace, GobLogCategory.Input);
        if (dir == InputDirection.None)
            return false;
        
        _heldKeyboardInputDirection = _readKeyboardInputDirection();
        
        return true;
    }

    private bool HandleKeyboardPanReleased()
    {
        _logger.Log("HandleKeyboardPanReleased", GobLogSeverity.Trace, GobLogCategory.Input);
        _heldKeyboardInputDirection = _readKeyboardInputDirection();

        return true;
    }

    private static Vector2 _readKeyboardInputDirection()
    {
        return Input.GetVector(
            "camera_pan_left",
            "camera_pan_right",
            "camera_pan_up",
            "camera_pan_down");
    }

    private void ApplyZoomedInDefault()
    {
        ZoomTo(_defaultZoom);
    }

    /// <summary>
    /// Applies camera movement from keyboard directional input.
    /// </summary>
    private void HandleKeyboardPan(float delta)
    {
        _logger.Log("HandleKeyboardPan", GobLogSeverity.Extra, GobLogCategory.Input);

        Vector2 inputDirection = _heldKeyboardInputDirection;

        if (inputDirection == Vector2.Zero)
            return;
        
        Vector2 zoomAdjustedMovement = inputDirection * _keyboardPanSpeed * delta * _camera.Zoom.X;
        _camera.GlobalPosition += zoomAdjustedMovement;
    }

    /// <summary>
    /// Applies camera movement opposite the mouse drag direction.
    /// </summary>
    private bool HandleDragPan(InputEventMouseMotion mouseMotionEvent)
    {
        // TODO - slow this down based on zoom
        _logger.Log("HandleDragPan", GobLogSeverity.Extra, GobLogCategory.Input);
        if (!_isDragPanning)
            return false;

        Vector2 dragOffset = -mouseMotionEvent.Relative * _dragPanMultiplier * _camera.Zoom.X;
        _camera.GlobalPosition += dragOffset;

        return true;
    }

    /// <summary>
    /// Applies Camera2D hard limits from the configured world bounds.
    /// </summary>
    private void ApplyCameraLimits()
    {
        _logger.Log("ApplyCameraLimits", GobLogSeverity.Info, GobLogCategory.Initialization);

        if (!DebugUtil.Require(_camera != null, "[BattleCameraController] Cannot apply limits without a camera."))
            return;

        _camera.LimitLeft = Mathf.RoundToInt(_cameraWorldBounds.Position.X);
        _camera.LimitTop = Mathf.RoundToInt(_cameraWorldBounds.Position.Y);
        _camera.LimitRight = Mathf.RoundToInt(_cameraWorldBounds.End.X);
        _camera.LimitBottom = Mathf.RoundToInt(_cameraWorldBounds.End.Y);

        ZoomTo(_camera.Zoom);
        
        // *** FIX: Snap logical position into valid bounds so there's no dead travel ***
        Vector2 viewportSize    = _camera.GetViewportRect().Size;
        Vector2 visibleWorldSize = viewportSize / _camera.Zoom;

        var minCameraX = _camera.LimitLeft + visibleWorldSize.X * 0.5f;
        var maxCameraX = _camera.LimitRight - visibleWorldSize.X * 0.5f;
        var minCameraY = _camera.LimitTop + visibleWorldSize.Y * 0.5f;
        var maxCameraY = _camera.LimitBottom - visibleWorldSize.Y * 0.5f;

        if (minCameraX > maxCameraX)
        {
            float centerX = (_camera.LimitLeft + _camera.LimitRight) * 0.5f;
            minCameraX = centerX;
            maxCameraX = centerX;
        }
        
        if (minCameraY > maxCameraY)
        {
            float centerY = (_camera.LimitTop + _camera.LimitBottom) * 0.5f;
            minCameraY = centerY;
            maxCameraY = centerY;
        }
        
        _camera.GlobalPosition = new Vector2(
            Math.Clamp(_camera.GlobalPosition.X, minCameraX, maxCameraX),
            Math.Clamp(_camera.GlobalPosition.Y, minCameraY, maxCameraY)
        );
    }

    /// <summary>
    /// Applies minimum zoom level so that camera is not larger than visible world size.
    /// </summary>
    private void ZoomTo(Vector2 targetZoom)
    {
        Vector2 viewportSize = _camera.GetViewportRect().Size;

        float minZoomX = Math.Min(viewportSize.X / _cameraWorldBounds.Size.X, _maxZoom.X);
        float minZoomY = Math.Min(viewportSize.Y / _cameraWorldBounds.Size.Y, _maxZoom.Y);

        var minZoom = Math.Min(minZoomX, minZoomY);
        
        float safeZoom = Math.Max(minZoomX, minZoomY);

        _camera.Zoom = new Vector2(
            Math.Clamp(targetZoom.X, minZoom, _maxZoom.X),
            Math.Clamp(targetZoom.Y, minZoom, _maxZoom.Y)
        );
        
        // TODO - make sure aspect ratio stays the same here
        // TODO - apply when changing window size
    }
}