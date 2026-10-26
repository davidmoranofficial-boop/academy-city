# Academy City

David's game, built in Unity 6.3 (Universal 3D).

- `Assets/AcademyCity` holds the game. Unity's editor script `Assets/AcademyCity/Editor/AcademyAutoUpdate.cs` pulls this repository whenever Unity regains focus and copies `Assets/AcademyCity` and `Tools` into the project, so a push to `main` is delivered.
- `Tools` holds build tools (art, audio, tests) that run outside Unity.
