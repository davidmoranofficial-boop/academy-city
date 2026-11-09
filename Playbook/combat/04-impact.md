# Impact: making hits feel heavy

## Hit stop (freeze on contact)
- Freeze BOTH fighters for a few frames at the contact frame. Freezing only the target looks like a hiccup.
- Rule-of-thumb lengths (blogs, not research): about 2 to 5 frames at 60 fps for normal hits (~0.03 to 0.08 s), longer for heavy or Surged hits, the longest for finishers. Time it in seconds, not frames, so high frame rates don't break it.
- Street Fighter (Capcom seminar): after the hit stop, the attacker plays out the rest of the strike and recovery while the target plays hit stun.
- Trigger once per impact.

## Camera shake
- Short, decaying shake scaled to the hit's power (square the strength so light hits barely shake and heavy hits land hard).
- No shake on blocks except heavy ones.

## Hit reactions (target)
- Direction matters: a head hook turns the head sideways, a jab snaps it back, a body shot folds the target at the waist, a low kick buckles the knee/drops the weight onto the other leg.
- Strength stat: bigger reaction, slide back on the floor (feet leave skid marks), launch only for big gaps or Surged ki attacks.
- Toughness stat: smaller flinch.
- The target's feet react too (a step to catch balance). A reaction with planted, unmoving feet looks fake.

## Anime layer (Academy City's look)
- **Impact frame**: 1 to 3 frames of a high-contrast flash or inverted image at contact, only for big hits. Too many = noise.
- **Smear**: stretch the striking limb along its path for 1 to 2 frames during the strike phase. It sells speed without more animation frames.
- **Speed lines**: follow the motion path and get denser as speed rises (ties into the Speed stat blur).
- **Air burst**: a ring/gust coming out of the BACK of the target on big hits (David's Dragon Ball request), plus dust on the floor.
- Keep effects consistent across the fight so they don't overwhelm the action.

## Layering checklist for a heavy hit
Wind-up tell → fast strike with smear → contact: hit stop (both) + impact frame + flash on the hit body part + air burst + shake → target reaction + skid → attacker recovers.
