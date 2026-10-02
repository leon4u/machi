# Tomoru Machi (Unity 6 mobile game)

- Game rules live in `Assets/Machi/Core` (pure C#, `noEngineReferences`). Keep UnityEngine out of Core; Unity views only call `Game` methods and listen to its events.
- After changing Core or `game_config.json`, run `cd Tests/CoreTests && dotnet run` (no NuGet packages; includes a bot that must finish Chapter 1 with the shipped config).
- Unity code must compile on Unity 6 and use C# 9 at most. The Unity editor is not available in cloud sessions, so say so when a change has not been run in Unity.
- UI is built from code (`UiKit`), no prefabs. Content is data-driven from `Resources/Config/*.json`.
- 3D models come from `Tools/Blender` scripts (`pip install bpy==4.2.0`, then `python3 Tools/Blender/build_town.py <out>`). Layout uses the `blender` coordinates in `town_layout.json`; see `TownView` for the conversion.
- Design docs are in `Design/docs` (Chinese). S1 scope: Merge + restoration + story only; visitor / shop / resident systems are S2–S4 and must not be built yet.
- The project owner writes in Chinese; user-facing text and docs are Chinese.
