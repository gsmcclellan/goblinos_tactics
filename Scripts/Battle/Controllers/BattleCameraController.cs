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
    private readonly GobLogger _logger = GobLogManager.For<BattleCameraController>();

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
    
    private Rect2 _cameraWorldBounds;
    
    // ---------------------------------------------------------------------
    // Lifecycle / Callback Methods
    // ---------------------------------------------------------------------

    public override void _Ready()
    {
        DebugUtil.Require(_camera != null, $"[{nameof(BattleCameraController)}] Initialization failed - no {nameof(_camera)}");
        _camera.Zoom = _defaultZoom;
    }

    // Additional setup with necessary linked components from BattleController
    public void Bind(Rect2 worldBounds)
    {
        _logger.Log("Bind", GobLogSeverity.Info, GobLogCategory.Initialization);

        _cameraWorldBounds = worldBounds;
        
        Debug.Assert(_camera != null, $"[{nameof(BattleCameraController)}] Camera must be assigned.");
        Debug.Assert(_cameraWorldBounds.Size.X > 0.0f, $"[{nameof(BattleCameraController)}] Camera bounds width must be positive.");
        Debug.Assert(_cameraWorldBounds.Size.Y > 0.0f, $"[{nameof(BattleCameraController)}] Camera bounds height must be positive.");

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

    public bool HandleKeyboardPanPressed(InputDirection dir)
    {
        _logger.Log("HandleKeyboardPanPressed", GobLogSeverity.Trace, GobLogCategory.Input);
        if (dir == InputDirection.None)
            return false;
        
        _heldKeyboardInputDirection = _readKeyboardInputDirection();
        
        return true;
    }

    public bool HandleKeyboardPanReleased()
    {
        _logger.Log("HandleKeyboardPanReleased", GobLogSeverity.Trace, GobLogCategory.Input);
        _heldKeyboardInputDirection = _readKeyboardInputDirection();

        return true;
    }

    public void ClearKeyboardPan() // Use this when opening menus / want to stop panning.
    {
        _heldKeyboardInputDirection = Vector2.Zero;
    }
    
    // ---------------------------------------------------------------------
    // Private Helper Methods
    // ---------------------------------------------------------------------

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
    private void HandleDragPan(InputEventMouseMotion mouseMotionEvent)
    {
        _logger.Log("HandleDragPan", GobLogSeverity.Extra, GobLogCategory.Input);

        Vector2 dragOffset = -mouseMotionEvent.Relative * _dragPanMultiplier * _camera.Zoom.X;
        _camera.GlobalPosition += dragOffset;
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
    }

    /// <summary>
    /// Immediately clamps the camera to the map limits and clears smoothing drift.
    /// </summary>
    private void ClampCameraPositionImmediate()
    {
        _logger.Log("ClampCameraPositionImmediate", GobLogSeverity.Info, GobLogCategory.UiNavigation);

        Vector2 clampedPosition = _camera.GlobalPosition;
        clampedPosition.X = Mathf.Clamp(clampedPosition.X, _cameraWorldBounds.Position.X, _cameraWorldBounds.End.X);
        clampedPosition.Y = Mathf.Clamp(clampedPosition.Y, _cameraWorldBounds.Position.Y, _cameraWorldBounds.End.Y);

        _camera.GlobalPosition = clampedPosition;
        _camera.ResetSmoothing();
    }
}