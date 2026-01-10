# Battle Input Flow
This document describes the **order of `BattleInputState` states** and what different inputs do in each state.
> Goal: make it easy to reason about *what the player is doing*, *what input is allowed*, and *which controller owns the behavior*.

---

## Terminology

- **Focused Cell**: the cell currently under the `GridCursor`.
- **Hovered Unit/Terrain**: derived from Focused Cell (SelectionController).
- **Selected Unit**: the unit the player is currently “acting with”.
- **Unit Activation**: a temporary context for planning a unit’s move + action before committing.
- **Primary Action**: the unit’s main action (attack/ability/item) for the activation.
- **Preview Overlay**: movement / action tiles shown on the grid.

---
## State list (actual enum order)

These map **directly** to the `BattleInputState` enum:

1. **FreeSelect**
2. **MoveTargeting**
3. **PrimaryActionSelect**
4. **PrimaryActionTargeting**
5. **PrimaryActionConfirm**

---

## State transition diagram (text)

- **FreeSelect**
    - Select unit → **MoveTargeting** (or **PrimaryActionSelect** if no-move actions are allowed)

- **MoveTargeting**
    - Choose valid move target → **PrimaryActionSelect**
    - Choose valid attack target -> **PrimaryActionSelect** with Attack hovered
    - Choose invalid target (outside move or attack range) → FreeSelect (abort activation)
    - Cancel → **FreeSelect**

- **PrimaryActionSelect**
    - Choose valid action → **PrimaryActionTargeting**
    - Cancel → **FreeSelect** - If move planned, undo
    - Action select menu navigation (attack, item, trade, wait)
    - Possible sub-menus (inventory, caravan)
    - No confirm step for open, wait -> skip to resolution

- **PrimaryActionTargeting**
  - Cursor moves between valid targets only
  - Cancel -> **FreeSelect** resetting move & selected action
  - Confirm -> **PrimaryActionConfirm**

- **PrimaryActionConfirm**
    - Show preview for ability resolutions (attack)
      - Combat preview, contain weapon switching
      - Item use preview - ex, heal shows current, final, total hp values.
    - Confirm → resolve + return to **FreeSelect**
    - Cancel → **FreeSelect** resetting move & selected action and any targets

---
## Input types to document

This document treats inputs as categories. Map them to your actual Godot actions.

- **Cursor Move**: dpad/arrow keys, stick, mouse hover
- **Confirm**: left click / A / Enter / Spacebar
- **Cancel**: right click / B / Escape
- **Secondary**: e.g., “inspect”, “toggle UI”, “cycle targets”, “rotate facing”, etc.
- **End Turn / Next Unit**: optional

### Others to be added (buttons not final):
- Show enemy move range (select)
- Show unit info - Y
- Menu - X
- Cycle units - RB/LB

### Global Cancel Rule
- Cancel → FreeSelect (abort activation)
- Clear selected acting unit, action, targets, overlays
- Attempt to undo move (if undoable)

---


## State-by-state behavior

### 1) FreeSelect

**Intent**: default battle interaction state. Player may select units or inspect the grid.

**On Enter**
- Clear existing UnitActivationContext. If unit is hovered, show preview.

**Inputs**
- Cursor Move → updates Focused Cell; 
  - GridCursor emits focus signal
  - SelectionController emits hovered signals
  - Show move/attack range preview on hover friendly unit
- Confirm on selectable unit → select unit and transition to **MoveTargeting**
- Confirm on empty cell → no op
- Cancel → no-op
- Secondary (inspect) → show terrain/unit info
- cycle units (next/previous) **TODO:** decide inputs -> go to next / previous
---


### 2) MoveTargeting

**Intent**: player is choosing where the selected unit will move.

**On Enter**
- Create UnitActivationContext
- Show movement & attack preview overlay

**Inputs**
- Cursor Move → preview path to focused cell
- Confirm on reachable cell → set move, setup state for undo; transition to **PrimaryActionSelect**
- Confirm on unreachable cell → FreeSelect (abort activation)
- Confirm on occupied cell (friendly) -> select target, reset move unit activation context & preview with new selected unit.
  - Stay in **MoveTargeting**
- Confirm on occupied cell (enemy) -> move in range, skip to **PrimaryActionSelect** with attack hovered
- Cancel → FreeSelect (abort activation)

**Notes**
- Movement is applied in this phase but stored in UnitActivationContext. 
- Turn is atomic so if primary action not completed, undo move.

---


### 3) PrimaryActionSelect

**Intent**: player is selecting the unit’s primary action.

**On Enter**
- Ensure UnitActivationContext exists
- Determine available primary actions
- Show action range / AOE preview overlays

**Inputs**
- Cursor Move → move between menu items
- Confirm → Selects action & moves to **PrimaryActionTargeting**
- Cancel → FreeSelect (abort activation)

**Notes**
- If multiple steps are required (sub-menu), remain in this state until complete.

---


### 4) PrimaryActionTargeting

**Intent**: Select valid targets for primary action choice.

**On Enter**
- Shows valid target preview

**Inputs**
- Confirm → Select target, move to **PrimaryActionConfirm**
- Cancel → FreeSelect (abort activation)
- Cursor Move → moves between legal targets

**Notes**

---

### 5) PrimaryActionConfirm

**Intent**: final confirmation before committing the planned move + action.

**On Enter**
- Lock previews into a stable state
- BattleHud displays confirmation UI

**Inputs**
- Confirm → commit activation; resolve move + action; transition to **FreeSelect**
- Cancel → FreeSelect (abort activation)
- Cursor Move → ignored or inspection-only

**Notes**
- All validation gates should live here (cannot commit invalid plans).

---
