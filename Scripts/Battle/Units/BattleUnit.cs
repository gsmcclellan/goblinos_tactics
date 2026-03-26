using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.UI.Battle;
using Goblinos.Scripts.Units;
using Goblinos.Scripts.Units.Stats;
using Goblinos.Scripts.Units.Stats.Types;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Units;

public partial class BattleUnit : Area2D
{
    /** Signals */
    [Signal]
    public delegate void HitPointsChangedEventHandler(int newValue, int oldValue);
    
    /** Components */
    private readonly Logger _logger = LogManager.For<BattleUnit>();

    [Export] private Sprite2D _imageSprite;
    [Export] private ProgressBar _hpBar;
    [Export] private PackedScene _floatingDamageScene;
    
    private Sprite2D _isSelectedNode;
    
    /** Fields */
    private int _currentHitPoints;
    
    // UnitData class - TODO
    /** Properties */
    private Unit _unit;

    public int CurrentHitPoints => _currentHitPoints;
    public List<StatModifier> BattleModifiers { get; } = [];

    public UnitActivationState State { get; private set; } = UnitActivationState.Ready;
    
    /** Facade Properties */
    public int GetStat(StatName statName) => Stats.Get(statName);
    public RangeBand AttackRange => Unit.AttackRange; // TODO - base on weapon.
    public bool CanAct => State != UnitActivationState.Exhausted;
    public String Id => _unit.Id;
    public bool IsDefeated => CurrentHitPoints <= 0;
    public bool IsFriendly => _unit.IsFriendly;
    public int MaxHitPoints => _unit.Stats.BaseStats.MaxHitPoints;
    public int Movement => _unit.Stats.BaseStats.Movement;
    public UnitStats Stats => _unit.Stats;
    public Unit Unit => _unit;
    public string UnitName => _unit.UnitName;
    
    // Realtime Properties
    private bool _isSelected = false;

    public BattleUnit(Unit unit)
    {
        _unit = unit;
        SetHitPoints(MaxHitPoints);
    }

    public BattleUnit()
    {
    }
    
    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------

    public override void _Ready()
    {
        
        _isSelectedNode = GetNode<Sprite2D>("IsSelectedNode");
        
        Debug.Assert(_imageSprite != null, "No Sprite Node");
        Debug.Assert(_isSelectedNode != null, "No Selected Display Node");
        
        SubscribeToEvents();
        
        _logger.Log("Ready", LogSeverity.Info, LogCategory.Initialization);
    }

    public override void _ExitTree()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        HitPointsChanged += OnHitPointsChanged;
    }
    
    private void UnsubscribeFromEvents()
    {
        HitPointsChanged -= OnHitPointsChanged;
    }
    
    /// <summary>
    /// Binds a persistent Unit to this battle instance and initializes battle-only state.
    /// </summary>
    public void Bind(Unit unit)
    {
        _logger.Log($"Bind " + unit.UnitName, LogSeverity.Info, LogCategory.UnitLifecycle);

        _unit = unit;
        SetHitPoints(unit.Stats.BaseStats.MaxHitPoints);
        
        // Set sprite image.
        if (!DebugUtil.Require(_unit.ImageFilePath != null, "Sprite missing"))
            return;

        var texture = GD.Load<Texture2D>(_unit.ImageFilePath);
        
        if (!DebugUtil.Require(texture != null, $"Failed to load texture: {_unit.ImageFilePath}"))
            return;

        _imageSprite.Texture = texture;
        
        Refresh();
    }
    
    // ---------------------------------------------------------------------
    // Public Methods
    // ---------------------------------------------------------------------
    
    /// <summary>
    /// Applies damage to CurrentHitpoints.
    /// </summary>
    public async void ApplyDamage(int damage)
    {
        _logger.Log("[BattleUnit] ApplyDamage " + damage, LogSeverity.Info, LogCategory.UnitLifecycle);
        if (!DebugUtil.Require(damage >= 0, "Battle Calculation error - negative damage"))
            return;
        
        await DisplayFloatingDamage(damage);
        SetHitPoints(CurrentHitPoints - damage);
        
    }
    
    public void Select()
    {
        ToggleSelected(true);
    }

    public void Deselect()
    {
        ToggleSelected(false);
    }

    public void SetActivationState(UnitActivationState state)
    {
        _logger.Log($"{nameof(SetActivationState)} state={state}", LogSeverity.Trace, LogCategory.UnitLifecycle);
        State = state;
        SetExhaustedVisual(state == UnitActivationState.Exhausted);
    }

    public void ToggleSelected(bool? force = null)
    {
        _isSelected = force ?? !_isSelected;
        UpdateSelectionUi();
    }
    
    // ---------------------------------------------------------------------
    // Signal / Event Handlers
    // ---------------------------------------------------------------------
    
    private void OnHitPointsChanged(int newValue, int oldValue)
    {
        var delta = newValue - oldValue;
        _logger.Log("OnHitPointsChanged delta=" + delta, LogSeverity.Info, LogCategory.UnitStats);

        if (!DebugUtil.Require(_hpBar != null, "HP Bar missing."))
            return;

        _hpBar.Value = newValue;
    }
    
    // ---------------------------------------------------------------------
    // Private Methods
    // ---------------------------------------------------------------------

    private async Task DisplayFloatingDamage(int damage)
    {
        _logger.Log("DisplayFloatingDamage damage=" + damage, LogSeverity.Trace, LogCategory.CombatResolution);
        if (!DebugUtil.Require(_floatingDamageScene != null, "Floating Damage Label scene not instantiated."))
            return;

        var floatingDamageText = _floatingDamageScene.Instantiate<FloatingDamageText>();
        AddChild(floatingDamageText);
        floatingDamageText.GlobalPosition = GlobalPosition;
        await floatingDamageText.ShowValue(damage);
    }

    private void Refresh()
    {
        if (!DebugUtil.Require(_unit != null, "Refresh failed. Null unit.") ||
            !DebugUtil.Require(_hpBar != null, "Refresh failed. Null HP Bar.")
           )
            return;
        
        _logger.Log("Refresh", LogSeverity.Trace, LogCategory.UnitStats);

        _hpBar.MaxValue = MaxHitPoints;
        _hpBar.Value = CurrentHitPoints;
    }
    
    private void SetExhaustedVisual(bool exhausted)
    {
        Modulate = exhausted 
            ? new Color(0.6f, 0.6f, 0.6f)
            : Colors.White;
    }

    private void SetHitPoints(int newValue)
    {
        var clamped = Math.Clamp(newValue, 0, MaxHitPoints);
        if (clamped == CurrentHitPoints)
            return;

        var old = _currentHitPoints;
        _currentHitPoints = clamped;

        var delta = _currentHitPoints - old;
        _logger.Log("[BattleUnit] HitPoints changed: " + old + " -> " + _currentHitPoints,
            LogSeverity.Info, LogCategory.UnitStats);

        EmitSignal(SignalName.HitPointsChanged, _currentHitPoints, old);
    }

    private void UpdateSelectionUi()
    {
        _isSelectedNode.Visible = _isSelected;
    }
}

