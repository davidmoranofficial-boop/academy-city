# Academy City

David's game, built in Unity 6.3 (Universal 3D).

- `Assets/AcademyCity` holds the game. Unity's editor script `Assets/AcademyCity/Editor/AcademyAutoUpdate.cs` pulls this repository whenever Unity regains focus and copies `Assets/AcademyCity` and `Tools` into the project, so a push to `main` is delivered.
- `Tools` holds build tools (art, audio, tests) that run outside Unity.

## Try it
Press **Play** in any scene. You get a white marble floor, Hanson and a follow camera (built from code in `Assets/AcademyCity/Runtime/Playground.cs`).

- WASD or arrow keys: move (jog). Hold Shift to sprint, Ctrl to walk.
- Mouse: look around. Scroll: zoom. Esc frees the mouse, click to capture it again.

Hanson is put together in Blender from free CC0 parts (see `CREDITS.md`, scripts in `Tools/Characters`). He uses the toon shader `Resources/Shaders/AcademyToon.shader`.
