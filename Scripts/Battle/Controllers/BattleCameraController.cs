using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Controllers;

/// <summary>
/// Owns battle camera movement, zoom, and bounds clamping.
/// This controller is intentionally separate from BattleController so
/// battle flow and viewport behavior remain independent.
/// </summary>
public partial class BattleCameraController : Node
{
    private readonly Logger _logger = LogManager.For<BattleCameraController>();

    [Export]
    private Camera2D _camera;

    private Vector2 _heldKeyboardInputDirection;
    
    
    
    
    
    
    [Export]
    private float _keyboardPanSpeed = 1200.0f;
    [Export]
    private float _dragPanMultiplier = 1.0f;
    [Export]
    private Vector2 _defaultZoom = new(2f, 2f);
    [Export]
    private bool _enablePositionSmoothing = true;
    [Export]
    private float _positionSmoothingSpeed = 8.0f;

    private bool _isCameraInputEnabled = true;
    private bool _isDragPanning;
    private Rect2 _worldBounds = new(Vector2.Zero, Vector2.Zero);

    public override void _Ready()
    {
        DebugUtil.Require(_camera != null, $"[{nameof(BattleCameraController)}] Initialization failed - no {nameof(_camera)}");
        _camera.Zoom = _defaultZoom;
    }
    
    /// <summary>
    /// Processes camera panning input each frame.
    /// </summary>
    public override void _Process(double delta)
    {
        _logger.Log("Process Camera Input", LogSeverity.Extra, LogCategory.Input);
        

        HandleKeyboardPan((float)delta);
    }

    public bool HandleKeyboardPanPressed(InputDirection dir)
    {
        _logger.Log("HandleKeyboardPanPressed", LogSeverity.Trace, LogCategory.Input);
        if (dir == InputDirection.None)
            return false;
        
        _heldKeyboardInputDirection = _readKeyboardInputDirection();
        
        return true;
    }

    public bool HandleKeyboardPanReleased()
    {
        _logger.Log("HandleKeyboardPanReleased", LogSeverity.Trace, LogCategory.Input);
        _heldKeyboardInputDirection = _readKeyboardInputDirection();

        return true;
    }

    public void ClearKeyboardPan() // Use this when opening menus / want to stop panning.
    {
        _heldKeyboardInputDirection = Vector2.Zero;
    }

    private Vector2 _readKeyboardInputDirection()
    {
        return Input.GetVector(
            "camera_pan_left",
            "camera_pan_right",
            "camera_pan_up",
            "camera_pan_down");
    }

    /// <summary>
    /// Applies camera movement from keyboard directional input.
    /// </summary>
    private void HandleKeyboardPan(float delta)
    {
        _logger.Log("HandleKeyboardPan", LogSeverity.Extra, LogCategory.Input);

        Vector2 inputDirection = _heldKeyboardInputDirection;

        if (inputDirection == Vector2.Zero)
            return;
        
        Vector2 zoomAdjustedMovement = inputDirection * _keyboardPanSpeed * delta * _camera.Zoom.X;
        _camera.GlobalPosition += zoomAdjustedMovement;
    }

    /// <summary>
    /// Applies camera movement opposite the mouse drag direction.
    /// </summary>
    private void HandleDragPan(InputEventMouseMotion mouseMotionEvent)
    {
        _logger.Log("HandleDragPan", LogSeverity.Extra, LogCategory.Input);

        Vector2 dragOffset = -mouseMotionEvent.Relative * _dragPanMultiplier * _camera.Zoom.X;
        _camera.GlobalPosition += dragOffset;
    }

    /// <summary>
    /// Applies Camera2D hard limits from the configured world bounds.
    /// </summary>
    private void ApplyCameraLimits()
    {
        _logger.Log("ApplyCameraLimits", LogSeverity.Info, LogCategory.Initialization);

        if (!DebugUtil.Require(_camera != null, "[BattleCameraController] Cannot apply limits without a camera."))
            return;

        _camera.LimitLeft = Mathf.RoundToInt(_worldBounds.Position.X);
        _camera.LimitTop = Mathf.RoundToInt(_worldBounds.Position.Y);
        _camera.LimitRight = Mathf.RoundToInt(_worldBounds.End.X);
        _camera.LimitBottom = Mathf.RoundToInt(_worldBounds.End.Y);
    }

    /// <summary>
    /// Immediately clamps the camera to the map limits and clears smoothing drift.
    /// </summary>
    private void ClampCameraPositionImmediate()
    {
        _logger.Log("ClampCameraPositionImmediate", LogSeverity.Info, LogCategory.UiNavigation);

        Vector2 clampedPosition = _camera.GlobalPosition;
        clampedPosition.X = Mathf.Clamp(clampedPosition.X, _worldBounds.Position.X, _worldBounds.End.X);
        clampedPosition.Y = Mathf.Clamp(clampedPosition.Y, _worldBounds.Position.Y, _worldBounds.End.Y);

        _camera.GlobalPosition = clampedPosition;
        _camera.ResetSmoothing();
    }
}