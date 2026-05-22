# BUGS:

- [x] haste can go on exhausted units - should only be units which can act. Also prevent double hasting.
- [x] haste doesn't expire after turn
- [x] Primary action menu cut off if too close to bot edge of screen. Need to clamp in camera bounds.
- [x] Terrain preview blocks cell selection near bottom right of map. Likely same with unit preview on bot left.
- [ ] Primary Action menu should clear battle hud previews.
- [x] enemy preview shows able to attack move only target 
  - STR:
      1. disable archer
      2. hover preview the archer
- [ ] Attacking enemy should wake it up - maybe wake up enemy if they are in friend's attack range.
- [ ] Attack preview shouldn't cover target cell
- [ ] Primary Action confirm panel and battle preview both show when select target for attack.
- [x] wall should block attack preview
- [ ] prevent push targeting unit if cell behind unit occupied.
- [ ] after performing attack, waiting for animations to finish. User is able to act while results have not fully resolved.
- [ ] when enemies calculate closest cell to friend, they should use moveable distance (not go through walls)
- [x] Healthbar not updated when max health changes.