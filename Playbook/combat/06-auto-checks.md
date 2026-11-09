# Automatic checks (Claude's "eyes")

Run these on every sim fight (batch mode) BEFORE any clip goes to David. Log each failure with the fight time, the move and the fighter, and save a frame of it. A clip only goes out when the failures are fixed or explained.

Sample bones every frame (Animator humanoid bones: Hips, LeftFoot, RightFoot, LeftToes, RightToes, LeftHand, RightHand, LeftLowerLeg, RightLowerLeg, Head, Spine/Chest).

| # | Check | How to measure | Fail when (starting thresholds, tune later) |
|---|---|---|---|
| 1 | Facing before striking | Angle between the attacker's forward (hips) and the direction to the target, at strike start | > 35 degrees |
| 2 | Planted foot sliding | A foot is "planted" when its height is < 3 cm and its speed was < 0.2 m/s the previous frame; while planted, measure horizontal drift | > 2 cm drift during one plant |
| 3 | One kicking leg | During a kick's strike phase, the standing foot must stay planted (check 2) | Both feet off the ground (> 5 cm) at the same time, except jumps/launches |
| 4 | Contact reached | At the contact frame, distance from the striking surface (fist = hand bone; shin = midpoint of the lower leg) to the target zone (head/chest/thigh above the knee) | > 12 cm (whiff that is shown as a hit) or limb inside the body by > 8 cm (clipping) |
| 5 | Right surface for low kicks | Which point is closest to the thigh at contact: shin midpoint or foot | Foot closer than shin |
| 6 | Steps match movement | Root distance moved vs the summed step length of the feet over a move | Mismatch > 25% (moonwalk/skate) |
| 7 | Feet not crossing | Signed distance of the feet across the stance line | Feet cross in fighting range |
| 8 | Defence timing | Defence pose begins before the attacker's contact frame | Starts after contact |
| 9 | Hit stop present | Both fighters' animation speed ~0 for the hit-stop window on landed hits | Missing or only one fighter frozen |
| 10 | Camera framing | Both fighters' bounding boxes inside the viewport (exchange camera), feet included | Any part off-screen at a contact frame |
| 11 | UI on screen | Damage numbers / Vision text rect inside the safe area and not covering a face at contact | Off-screen or covering |
| 12 | Fight length and balance | From many sims: number of exchanges, win split | Median outside 6 to 10 exchanges; win split worse than 60/40 for even stats |

## Frame sheets
For each move type, export a 6-frame sheet (face, load, strike, contact, recover, settle) next to the key poses in 01-strikes.md, and compare it by eye before sending.

## Implementation notes
- Matching movement speed to clip speed (or root motion) fixes most sliding; foot IK locking is the polish layer (Footwerk, Epic speed planting).
- Write the checks as an editor script that runs after the batch sim and writes `Demo/Checks/report_<date>.txt` plus failure frames.

## Code
`code/FightChecks.cs` (written 9 Nov, not yet compiled): covers checks 1 to 10 and logs fight results for check 12. Check 11 (UI) still to add. Hook points: `Register`, `OnStrikeStart`, `OnContact`, `OnStrikeEnd`, `OnDefenceStart`, `OnHitStop`, `OnMoveStart/End`, `OnFightEnd`, `WriteReport`.
