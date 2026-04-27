using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Combat;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.UI.Battle;
using Goblinos.Scripts.Units;
using Goblinos.Scripts.Units.Stats;
using Goblinos.Scripts.Units.Stats.Types;
using Goblinos.Scripts.Units.Types;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Units;

public partial class BattleUnit : Area2D
{
    /** Signals */
    [Signal]
    public delegate void HitPointsChangedEventHandler(int newValue, int oldValue);
    
    /** Components */
    private readonly GobLogger _logger = GobLogManager.For<BattleUnit>();

    [Export] private Sprite2D _imageSprite;
    [Export] private ProgressBar _hpBar;
    [Export] private PackedScene _floatingTextScene;
    
    private Sprite2D _isSelectedNode;
    
    /** Fields */
    private int _currentHitPoints;
    private Unit _unit;
    
    // UnitData class - TODO
    /** Properties */
    public Unit Unit
    {
        get => _unit;
        set
        {
            if (_unit != null)
                _unit.Stats.StatsChanged -= OnStatsChanged;
            _unit = value;
            if (_unit != null)
                _unit.Stats.StatsChanged += OnStatsChanged;
        }
    }

    public int CurrentHitPoints => _currentHitPoints;
    public List<StatModifier> BattleModifiers { get; } = [];
    public List<CombatCondition> Conditions { get; } = [];

    public UnitActivationState State { get; private set; } = UnitActivationState.Ready;
    
    /** Computed properties */
    public bool CanAct => State is UnitActivationState.Ready or UnitActivationState.Activated;
    
    /** Facade Properties */
    public String Id => Unit.Id;
    public string UnitName => Unit.UnitName;
    public AbilityDefinition Ability => Unit.Ability;
    public int AbilityMagnitude => Unit.Ability.MagnitudeStat.HasValue ? GetStat(Unit.Ability.MagnitudeStat.Value) : Ability.Magnitude;
    public RangeBand AttackRange => Unit.AttackRange; // TODO - base on weapon.
    public bool IsFriendly => Unit.IsFriendly;
    public int Level => Unit.Level;
    
    /** Conditions */
    public bool IsMovementDisabled => Conditions.Any(cond => cond.Type == CombatConditionType.DisableMovement);
    public bool IsDefeated => CurrentHitPoints <= 0;
    
    /** Stats */
    public DerivedStats Stats => DerivedStatsCalculator.Build(Unit.Stats, Level);
    public int MaxHitPoints => Stats.MaxHitPoints;
    public int Movement => (IsMovementDisabled) ? 0 : GetStat(StatName.Movement);

    
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
        
        _logger.Log("Ready", GobLogSeverity.Info, GobLogCategory.Initialization);
    }

    public override void _ExitTree()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        HitPointsChanged += OnCurrentHitPointsChanged;
    }
    
    private void UnsubscribeFromEvents()
    {
        HitPointsChanged -= OnCurrentHitPointsChanged;
        
        if (Unit != null)
            Unit.Stats.StatsChanged -= OnStatsChanged;
    }
    
    /// <summary>
    /// Binds a persistent Unit to this battle instance and initializes battle-only state.
    /// </summary>
    public void Bind(Unit unit)
    {
        _logger.Log($"Bind " + unit.UnitName, GobLogSeverity.Info, GobLogCategory.UnitLifecycle);

        Unit = unit;
        SetHitPoints(Stats.MaxHitPoints);
        
        if (!IsFriendly)
            State = UnitActivationState.Dormant;
        
        // Set sprite image.
        if (!DebugUtil.Require(Unit.ImageFilePath != null, "Sprite missing"))
            return;

        var texture = GD.Load<Texture2D>(Unit.ImageFilePath);
        
        if (!DebugUtil.Require(texture != null, $"Failed to load texture: {Unit.ImageFilePath}"))
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
    public async Task ApplyDamage(int damage)
    {
        _logger.Log("[BattleUnit] ApplyDamage " + damage, GobLogSeverity.Info, GobLogCategory.UnitLifecycle);
        if (!DebugUtil.Require(damage >= 0, "Battle Calculation error - negative damage"))
            return;
        SetHitPoints(CurrentHitPoints - damage);
        // await DisplayFloatingHitPointChange(damage);
    }
    
    /// <summary>
    /// Applies healing to CurrentHitpoints.
    /// </summary>
    public async Task ApplyHealing(int healAmount)
    {
        _logger.Log($"{nameof(ApplyDamage)} amount={healAmount}", GobLogSeverity.Info, GobLogCategory.UnitLifecycle);
        DebugUtil.Require(healAmount >= 0, "Battle Calculation error - negative healing");

        healAmount = Math.Clamp(healAmount, 0, MaxHitPoints - CurrentHitPoints);
        
        SetHitPoints(CurrentHitPoints + healAmount);
        await DisplayFloatingHitPointChange(healAmount);
    }

    public void ApplyCondition(CombatCondition condition)
    {
        var existingCondition = Conditions.Find(cond => cond.Type == condition.Type);

        if (existingCondition == null)
            Conditions.Add(condition);
        else
            existingCondition.AddStacks(condition.Stacks);
    }

    public bool HasCondition(CombatConditionId id) => Conditions.Any(cond => cond.Id == id);

    public void ApplyStatModifier(StatModifier statMod)
    {
        var existingMod = BattleModifiers.Find(mod => mod.Equals(statMod));
        if (existingMod != null)
            existingMod.Add(statMod);
        else
            BattleModifiers.Add(statMod);
    }

    public bool HasStatModifier(string modifierId) => BattleModifiers.Any(sm => sm.Id == modifierId);

    public int GetStat(StatName statName)
    {
        // TODO - more complex calculations - split to pre/post op. - move to derived stats
        var statValue = Stats.Get(statName);
        var statMods = BattleModifiers.Where(sm => sm.StatName == statName);
        return statMods.Aggregate(statValue, (acc, x) => acc + x.Value);
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
        _logger.Log($"{nameof(SetActivationState)} state={state}", GobLogSeverity.Trace, GobLogCategory.UnitLifecycle);
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
    public void OnRoundEnded()
    {
        BattleModifiers.RemoveAll(bm => bm.ExpiresAt == ExpirationTime.EndOfRound);

        // Expire conditions
        foreach (var condition in Conditions.Where(c => c.ExpiresAt == ExpirationTime.EndOfRound))
        {
            // TODO
        }
    }
    
    private void OnCurrentHitPointsChanged(int newValue, int oldValue)
    {
        var delta = newValue - oldValue;
        _logger.Log("OnHitPointsChanged delta=" + delta, GobLogSeverity.Info, GobLogCategory.UnitStats);

        _updateHitPointsBar();
    }

    private void OnStatsChanged(IReadOnlyList<StatName> updatedStats)
    {
        if (updatedStats.Contains(StatName.MaxHitPoints))
            _updateHitPointsBar();
    }
    
    // ---------------------------------------------------------------------
    // Private Methods
    // ---------------------------------------------------------------------

    private void _updateHitPointsBar()
    {
        if (!DebugUtil.Require(_hpBar != null, "HP Bar missing."))
            return;

        _hpBar.Value = CurrentHitPoints;
        _hpBar.MaxValue = MaxHitPoints;
    }

    private async Task DisplayFloatingHitPointChange(int amount) 
        // TODO - move to battle or UI so not reliant on unit existing / color
        // TODO - add color change for healing.
    {
        _logger.Log($"{nameof(DisplayFloatingHitPointChange)} amount={amount}", GobLogSeverity.Trace, GobLogCategory.CombatResolution);
        if (!DebugUtil.Require(_floatingTextScene != null, "Floating HitPoints Label scene not instantiated."))
            return;

        var floatingDamageText = _floatingTextScene.Instantiate<FloatingText>();
        AddChild(floatingDamageText);
        floatingDamageText.GlobalPosition = GlobalPosition;
        await floatingDamageText.ShowValue(GlobalPosition, amount);
    }

    private void Refresh()
    {
        if (!DebugUtil.Require(Unit != null, "Refresh failed. Null unit.") ||
            !DebugUtil.Require(_hpBar != null, "Refresh failed. Null HP Bar.")
           )
            return;
        
        _logger.Log("Refresh", GobLogSeverity.Trace, GobLogCategory.UnitStats);

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
            GobLogSeverity.Info, GobLogCategory.UnitStats);

        EmitSignal(SignalName.HitPointsChanged, _currentHitPoints, old);
    }

    private void UpdateSelectionUi()
    {
        _isSelectedNode.Visible = _isSelected;
    }
    
    
}

