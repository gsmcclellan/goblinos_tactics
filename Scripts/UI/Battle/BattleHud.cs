#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle;
using Goblinos.Scripts.Battle.Core;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Terrain;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Combat;
using Goblinos.Scripts.Units;
using Godot;
using Goblinos.Scripts.Util;
using BattleUnit = Goblinos.Scripts.Battle.Units.BattleUnit;
using SelectionController = Goblinos.Scripts.Battle.Controllers.SelectionController;
using TurnController = Goblinos.Scripts.Battle.Controllers.TurnController;

namespace Goblinos.Scripts.UI.Battle
{
    public partial class BattleHud : CanvasLayer
    {
        /** Signals */
        [Signal]
        public delegate void PrimaryActionFocusedEventHandler(int action);
        [Signal]
        public delegate void PrimaryActionSelectedEventHandler(int action);
        
        /** Components */
        [Export] private NodePath _panelsRootPath = null!;
        [Export] private NodePath _combatPreviewPath = null!;
        [Export] private NodePath _primaryActionSelectPath = null!;
        [Export] private NodePath _primaryActionConfirmPath = null!;
        
        [Export] private Label _turnNumberLabel = null!;
        [Export] private Button _endTurnButton = null!;
        [Export] private Control _leveledUpPanel = null!;

        private BattleController _battleController = null!;
        private GridCursor _cursor = null!;
        private Node _panelsRoot = null!;
        private PrimaryActionConfirm _primaryActionConfirm = null!;
        private PrimaryActionSelect _primaryActionSelect = null!;
        private SelectionController _selectionController = null!;
        private TurnController _turnController = null!;
        
        private readonly GobLogger _logger = GobLogManager.For<BattleHud>();
        
        /** Fields */
        private readonly List<IBattleHudPanel> _panels = new();

        private int _dialogBufferX = 220;
        private int _dialogBufferY = 160;

        /** Properties */
        public bool IsPrimaryActionSelectMenuActive => _primaryActionSelect.Visible;

        // ---------------------------------------------------------------------
        // Lifecycle / Setup Methods
        // ---------------------------------------------------------------------
        public override void _Ready()
        {
            _panelsRoot = GetNode(_panelsRootPath);
            _primaryActionConfirm = GetNode<PrimaryActionConfirm>(_primaryActionConfirmPath);
            _primaryActionSelect = GetNode<PrimaryActionSelect>(_primaryActionSelectPath);
            
            DebugUtil.Require(_panelsRoot != null, "[BattleHud] Not Initialized. _panelsRoot reference is required.");
            DebugUtil.Require(_primaryActionConfirm != null, "[BattleHud] Not Initialized. PrimaryActionSelect reference is required.");
            DebugUtil.Require(_primaryActionSelect != null, "[BattleHud] Not Initialized. PrimaryActionSelect reference is required.");
            
            HidePrimaryActionConfirm();
            HidePrimaryActionSelectMenu();
            
            CachePanels();
            WirePanels();
            
            _logger.Log("Ready", GobLogSeverity.Info, GobLogCategory.Initialization);
        }
        
        public void Bind(BattleController battleController, GridCursor cursor, SelectionController selectionController, TurnController turnController)
        {
            _logger.Log("Bind", GobLogSeverity.Info, GobLogCategory.Initialization);
            
            _battleController = battleController;
            _cursor = cursor;
            _selectionController = selectionController;
            _turnController = turnController;
            
            if (!DebugUtil.Require(_battleController != null, "[BattleHud] requires BattleController binding") ||
                !DebugUtil.Require(_cursor != null, "[BattleHud] requires GridCursor binding") ||
                !DebugUtil.Require(_selectionController != null, "[BattleHud] requires SelectionController binding") ||
                !DebugUtil.Require(_turnController != null, "[BattleHud] requires TurnController binding")
               )
                return;
            
            Debug.Assert(_turnNumberLabel != null, "[BattleHud] Missing Turn Number Label");
            Debug.Assert(_endTurnButton != null, "[BattleHud] Missing End Turn Button");
            
            _SubscribeToEvents();
        }
        
        public override void _ExitTree()
        {
            _logger.Log("_ExitTree", GobLogSeverity.Info, GobLogCategory.Exit);
            _UnsubscribeFromEvents();
        }
        
        private void _SubscribeToEvents()
        {
            _logger.Log("SubscribeToEvents", GobLogSeverity.Info, GobLogCategory.Initialization);
            if (_primaryActionSelect == null)
                throw new InvalidOperationException("[BattleHud] Bind called before _Ready. _primaryActionSelect is not initialized.");

            _battleController.InputStateChanged += OnBattleControllerInputStateChanged;
            _battleController.CombatPreviewUpdated += OnCombatPreviewUpdated;
            
            _cursor.GridCursorFocusChanged += OnHoveredCellChanged;

            _endTurnButton.Pressed += OnEndTurnButtonPressed;
            
            _primaryActionSelect.ActionFocused += OnPrimaryActionFocused;
            _primaryActionSelect.ActionSelected += OnPrimaryActionSelected;
            
            _selectionController.HoveredTerrainChanged += OnHoveredTerrainChanged;
            _selectionController.HoveredUnitChanged += OnHoveredUnitChanged;
            _selectionController.SelectedUnitChanged += OnSelectedUnitChanged;

            _turnController.TurnStarted += OnTurnStarted;
            
            var closeButton = _leveledUpPanel.GetNode<Button>("VBoxContainer/HBoxContainer/CloseButton");
            if (closeButton != null)
                closeButton.Pressed += HideLeveledUpDetails;
        }

        private void _UnsubscribeFromEvents()
        {
            _logger.Log("UnsubscribeFromEvents", GobLogSeverity.Info, GobLogCategory.Exit);
            
            _battleController.InputStateChanged -= OnBattleControllerInputStateChanged;
            _battleController.CombatPreviewUpdated -= OnCombatPreviewUpdated;
            
            _cursor.GridCursorFocusChanged -= OnHoveredCellChanged;
            
            _endTurnButton.Pressed -= OnEndTurnButtonPressed;
            
            _primaryActionSelect.ActionFocused -= OnPrimaryActionFocused;
            _primaryActionSelect.ActionSelected -= OnPrimaryActionSelected;

            _selectionController.HoveredTerrainChanged -= OnHoveredTerrainChanged;
            _selectionController.HoveredUnitChanged -= OnHoveredUnitChanged;
            _selectionController.SelectedUnitChanged -= OnSelectedUnitChanged;
            
            _turnController.TurnStarted -= OnTurnStarted;
        }

        private void CachePanels()
        {
            _panels.Clear();
            // _panelsRoot ??= GetNode(_panelsRootPath);
            
            foreach (Node child in _panelsRoot.GetChildren())
            {
                if (child is IBattleHudPanel panel)
                    _panels.Add(panel);
            }
            
            _logger.Log($"CachePanels count={_panels.Count}", GobLogSeverity.Info, GobLogCategory.Initialization);
        }

        private void WirePanels()
        {
            _logger.Log("WirePanels count=" + _panels.Count, GobLogSeverity.Info, GobLogCategory.Initialization);
        }
        
        // ---------------------------------------------------------------------
        // Public Methods
        // ---------------------------------------------------------------------
        public void HidePrimaryActionConfirm()
        {
            _primaryActionConfirm.Visible = false;
        }
        
        public void HidePrimaryActionSelectMenu()
        {
            _primaryActionSelect.Visible = false;
            _primaryActionSelect.ReleaseFocus();
        }

        public void ShowPrimaryActionConfirm(UnitActivationContext unitActivation)
        {
            _primaryActionConfirm.SetContext(unitActivation);
            _primaryActionConfirm.Visible = true;

        }
        
        /// <summary>
        /// Shows the primary action menu, disables actions with no valid targets, and focuses the first enabled action.
        /// </summary>
        public void ShowPrimaryActionSelectMenu(BattleUnit actingUnit, Vector2 globalPosition, PrimaryActionValidTargetsPreview? previews)
        {
            _primaryActionSelect.Visible = true;
            ClampPositionToVisibleScreen(_primaryActionSelect, globalPosition);
            
            foreach (var actionType in PrimaryActionInfo.PrimaryActionOrder)
            {
                if (actionType == PrimaryActionType.None)
                    continue;

                var requiresTarget = PrimaryActionInfo.RequiresTarget(actionType);
                var hasTargets = previews != null && previews.HasTargets(actionType);
                var isNoneAbility = actionType == PrimaryActionType.Ability &&
                                    actingUnit.Ability.Type == AbilityType.None;
                var isItem = actionType == PrimaryActionType.Item; // TODO - enable items

                _primaryActionSelect.SetActionEnabled(actionType, !isNoneAbility && !isItem && (!requiresTarget || hasTargets));
            }
            
            // Pick a deterministic "top" action (don’t rely on enum order)
            if (!_primaryActionSelect.TryFocusFirstEnabled(PrimaryActionInfo.PrimaryActionOrder))
                _primaryActionSelect.ReleaseFocus();
        }

        private void ClampPositionToVisibleScreen(Control node, Vector2 globalPosition)
        {
            Vector2 screenPosition = GetViewport().GetCanvasTransform() * globalPosition;
            
            // Get viewport bounds
            Rect2 viewportRect = GetViewport().GetVisibleRect();
            _primaryActionSelect.ResetSize();
            Vector2 menuSize = _primaryActionSelect.Size;
            
            // Clamp position so the menu stays fully inside the screen
            float clampedX = Mathf.Clamp(
                screenPosition.X,
                viewportRect.Position.X + _dialogBufferX,
                viewportRect.End.X - menuSize.X - _dialogBufferX
            );

            float clampedY = Mathf.Clamp(
                screenPosition.Y,
                viewportRect.Position.Y + _dialogBufferX,
                viewportRect.End.Y - menuSize.Y - _dialogBufferY
            );
            
            node.GlobalPosition = new Vector2(clampedX, clampedY);
        }

        public void DisplayLeveledUpDetails(UnitLeveledUpEvent details)
        {
            _leveledUpPanel.GetNode<Label>("VBoxContainer/MarginContainer/Label").Text = details.ToString();
            _leveledUpPanel.Show();
        }

        public void HideLeveledUpDetails()
        {
            _leveledUpPanel.Hide();
        }
        
        // ---------------------------------------------------------------------
        // Signal / Event Callbacks
        // ---------------------------------------------------------------------

        private void OnBattleControllerInputStateChanged(int s)
        {
            var state = (BattleInputState) s;
            _logger.Log($"OnBattleControllerInputStateChanged - state={state.ToString()}", GobLogSeverity.Info, GobLogCategory.UiNavigation);
            
            var node = GetNode<Label>("AdditionalElements/Panel/BattleControllerInputState"); // TODO - make this a IBattleHudPanel (or add to terrain info)
            node.Text = state.ToString();
            
            _panels.ForEach(panel => panel.OnBattleInputStateChanged(s));
        }

        private void OnCombatPreviewUpdated(CombatPreview? combatPreview)
        {
            _logger.Log($"OnCombatPreviewUpdated", GobLogSeverity.Extra, GobLogCategory.UiNavigation);
            _panels.ForEach(panel => panel.OnCombatPreviewUpdated(combatPreview));
        }

        private void OnEndTurnButtonPressed()
        {
            _logger.Log("OnEndTurnButtonPressed", GobLogSeverity.Info, GobLogCategory.UiNavigation);
            _battleController.RequestEndTurn();
        }
        
        private void OnHoveredCellChanged(Vector2I newCell, Vector2I oldCell, int gridCursorFocusSource)
        {
            _logger.Log($"[{nameof(OnHoveredCellChanged)}] newCell={newCell}, oldCell={oldCell}", GobLogSeverity.Trace, GobLogCategory.UiNavigation);
            foreach (var panel in _panels)
                panel.OnHoveredCellChanged(newCell, oldCell);
        }
        
        private void OnHoveredTerrainChanged(TerrainType? terrain)
        {
            _logger.Log("OnHoveredTerrainChanged", GobLogSeverity.Trace, GobLogCategory.UiNavigation);
            foreach (var panel in _panels)
                panel.OnHoveredTerrainChanged(terrain);
        }
        
        private void OnHoveredUnitChanged(Node? hoveredUnit)
        {
            _logger.Log("OnHoveredUnitChanged", GobLogSeverity.Trace, GobLogCategory.UiNavigation);
            if (hoveredUnit != null && hoveredUnit is not BattleUnit)
                throw new InvalidCastException("Unit is wrong type, expect BattleUnit");
            
            foreach (var panel in _panels)
                panel.OnHoveredUnitChanged(hoveredUnit as BattleUnit);
        }

        private void OnSelectedUnitChanged(Node? selectedUnit)
        {
            _logger.Log("OnSelectedUnitChanged", GobLogSeverity.Trace, GobLogCategory.UiNavigation);
            if (selectedUnit != null && selectedUnit is not BattleUnit)
                throw new InvalidCastException("Unit is wrong type, expect BattleUnit");
            if (selectedUnit != null)
                HideLeveledUpDetails();
            foreach (var panel in _panels)
                panel.OnSelectedUnitChanged(selectedUnit as BattleUnit);
        }
        
        private void OnPrimaryActionFocused(int action)
        {
            _logger.Log("OnPrimaryActionFocused", GobLogSeverity.Trace, GobLogCategory.Signal);
            EmitSignal(SignalName.PrimaryActionFocused, action);
        }

        private void OnPrimaryActionSelected(int action)
        {
            _logger.Log("OnPrimaryActionSelected", GobLogSeverity.Trace, GobLogCategory.Signal);
            EmitSignal(SignalName.PrimaryActionSelected, action);
        }
        
        private void OnTurnStarted(BattleSide activeSide, int turnNumber)
        {
            _turnNumberLabel.Text = turnNumber.ToString();
        }
    }
}
