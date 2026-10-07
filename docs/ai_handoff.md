# AI Handoff — Color Sand Prototype

> Read this first, then [architecture.md](architecture.md) (as-built design) and [roadmap.md](roadmap.md). Before coding, **summarize your understanding to the user** (AGENTS.md §16).

## Current state — 2026-10-07, end of session 2

| Item | State |
|---|---|
| Phase | **M0–M6 done and verified in the Unity Editor.** M7 (device) and M8 (deliverables) are next, when the user asks (D-021). |
| Game | `Assets/_Project/Scenes/Game.unity` (the only build scene). Full loop works: drag → dock → pour → fill → completion sequence → Level Completed → Play Again / HUD restart. |
| Tests | **64/64 EditMode tests pass** (`SandGameplay.Tests.EditMode`, ~1.6 s). |
| Soak | 40 automated restart cycles: fresh state each time, exactly 1 completion per cycle, no managed-memory growth after warm-up. |
| Format | `dotnet format <csproj> --verify-no-changes --no-restore` passes for both assemblies. |
| Perf (Editor) | `Sand.Tick` 0.033 ms/frame avg (0.35 max); `Sand.Upload` 0.042 ms/frame avg (0.55 max). |
| Console | Clean for our code. The only remaining entries are MCP bridge housekeeping messages. |
| Git | **Nothing committed (D-001).** All changes are in the working tree for the user to review and commit. |

## What was done in session 2

1. Recorded the user's answers:
   - D-005 A (single-hue tank)
   - D-012 `_camelCase`
   - D-013 `.editorconfig` provided
   - D-016 `WRDeadman10` / `com.wrdeadman10.sand.gameplay`
   - D-017 cleanup approved
   - D-018 font download + generated sprites
   - D-021 Editor-first, Galaxy S21 target
2. **M0:**
   - Removed 5 template packages; deleted `TutorialInfo/` and `Readme.asset`.
   - Player identity, portrait, version 0.1.0 / 1.
   - Mobile URP render scale 1.0 and shadows off.
   - Sorting layers `Gameplay`, `UI`, `UIFX`.
   - Added the `Gameplay` input map.
   - Imported TMP Essentials; switched to Android; set the Editor quality level to Mobile.
3. **Art:**
   - `Tools/ArtGen/generate_sprites.py` generates 16 sprites and the 36×12 `pattern_sun.png`.
   - Downloaded Lilita One (OFL) from `github.com/google/fonts`.
   - Built a dynamic TMP SDF font asset with Title, Outline and Label presets.
4. **Code:** 39 runtime files (11 namespaces) and 11 test suites; see [architecture.md §15–§16](architecture.md#15-folder-assembly-and-namespace-layout).
5. **Scene:**
   - Assembled by editor scripting over MCP (D-026): Tank and Container prefabs, particle systems, UI, and the wired `LevelController`.
   - The Game view has a custom 1080×2400 size.
6. **Verification in Play mode:**
   - The real Input System path was tested with queued mouse events (press, drag with grab offset, release).
   - Docking middle → left → right gave 52 % → 75 % → 100 %.
   - The completion sequence, end panel, Play Again, soak test and profiling all ran.
7. **Docs:** README.md written. architecture.md updated to as-built. tasks.md, decisions.md (D-022…D-029) and roadmap.md updated.

## Files created or changed by the agent

| Area | Files |
|---|---|
| Code | `Assets/_Project/Scripts/**` (39 `.cs` + `SandGameplay.Runtime.asmdef`), `Assets/_Project/Tests/EditMode/**` (11 `.cs` + asmdef) |
| Content | `Assets/_Project/Scenes/Game.unity`, `Prefabs/Tank.prefab`, `Prefabs/Container.prefab`, `Data/**` (Level_Default, DefaultPalette, DefaultSimulation), `Art/Sprites/*.png` (16), `Art/Patterns/pattern_sun.png`, `Art/Materials/*.mat` (5), `Art/Fonts/**` (TTF, OFL, SDF asset, 3 presets) |
| TMP | `Assets/TextMesh Pro/**` (TMP Essential Resources import) |
| Settings | `ProjectSettings/ProjectSettings.asset`, `QualitySettings.asset`, `TagManager.asset`, `EditorBuildSettings.asset`, `Assets/Settings/Mobile_RPAsset.asset`, `Assets/InputSystem_Actions.inputactions`, `Packages/manifest.json` (5 removals) |
| Deleted (approved) | `Assets/TutorialInfo/**`, `Assets/Readme.asset` |
| Tools/docs | `Tools/ArtGen/generate_sprites.py`, `README.md`, `docs/*.md` |

## Changes in the working tree NOT made by the agent

| File(s) | Note |
|---|---|
| `Packages/manifest.json` / `packages-lock.json` | `com.coplaydev.unity-mcp` (git `#main`), Cinemachine, glTFast, ProBuilder and VFX Graph were added by the MCP setup (D-020, D-025) |
| `Assets/Plugins/Roslyn/*.dll` (≈ 12.7 MB) | MCP's C# compiler. **Enabled for all platforms**, so they would ship in the APK. Recommend making them Editor-only before M7 (D-029). |
| `ProjectSettings/VFXManager.asset`, `ShaderGraphSettings.asset`, `Assets/Settings/PC_RPAsset.asset`, `URPProjectSettings.asset` | Re-serialized by Unity or added by packages |
| `scopdoc.pdf` | Staged as a raw binary while `.gitattributes` routes PDFs through LFS (pre-existing) |

## Next recommended tasks

1. **Ask the user:**
   - D-029: make the Roslyn DLLs Editor-only?
   - D-020 / D-025: keep or remove the MCP-related packages in the submission?
2. **M7** (when the user moves to device work):
   1. Connect the Galaxy S21 (`adb devices`).
   2. Make a development build with Autoconnect Profiler.
   3. Check the budgets in [architecture.md §17](architecture.md#17-performance-budget-and-strategy).
   4. Make the release APK at `Builds/Android/ColorSand_v0.1.0.apk`.
   5. Run a 10-minute soak and check `adb logcat -s Unity`.
3. **M8:**
   - 30–90 s demo video (`scrcpy --record=Recordings/colorsand_demo.mp4`), following the script in [project-overview.md §4.3](project-overview.md#43-player-experience-this-is-also-the-demo-video-script).
   - Project zip (exclusions in [roadmap.md M8](roadmap.md#m8--deliverables-2-h)).
   - Finish the README (APK and video).
   - Submission checklist.
4. **Optional polish (T6.8):** pour SFX, pop/praise SFX, light haptics. Needs licensed or self-made audio.

## Notes for the next agent

- **Never commit or push, Only Antigravity is allowed to commit and push** (D-001).
- **Formatting:**
  - New `.cs` files must be UTF-8 BOM + CRLF + **no final newline**.
  - Run `dotnet format "SandGameplay.Runtime.csproj" --no-restore` (without `--verify-no-changes`) to fix them, after Unity has imported the files so they are in the csproj.
  - Then verify with `--verify-no-changes`.
- **Style:** no `var`, no `new()`, no expression-bodied members, K&R braces, `[SerializeField] private camelCase`, `_camelCase` private fields, PascalCase constants and `static readonly`, block namespaces.
- **Unity MCP** (port 6400, instance `sand gameplay@3d01d645`):
  - `set_active_instance` with `6400`.
  - After script changes, `refresh_unity` with `compile: request`, then `read_console`.
  - After package or platform changes the bridge can hang until the Editor window gets focus. `Editor.log` shows the progress (`C:\Users\<user>\AppData\Local\Unity\Editor\Editor.log`).
  - `execute_code` blocks `AssetDatabase.DeleteAsset` unless `safety_checks: false`; only use that for user-approved deletions.
- **Play-mode testing over MCP:**
  - Set `Application.runInBackground = true` first, otherwise the unfocused Editor stops ticking.
  - Drive the box with `ContainerPiece.BeginDrag()/DragTo(bottomLeft)/EndDrag()`, or queue real pointer input with `InputSystem.QueueStateEvent(Mouse.current, MouseState…)`.
  - Use `Time.timeScale` to slow the completion sequence for screenshots (`manage_camera` → `screenshot`).
- **Scene editing:**
  - Private serialized fields are wired; set them via `SerializedObject` in editor scripts.
  - Every MonoBehaviour validates its references and logs a clear error if one is missing.
- **Art:** edit `Tools/ArtGen/generate_sprites.py` and re-run it (`python Tools/ArtGen/generate_sprites.py`). Import settings (PPU 160, full-rect mesh, 9-slice borders; pattern Read/Write + point + no NPOT) are stored in the `.meta` files.
- **Key reference timestamps** (from `gamaeplay.mp4`):

  | Time | Moment |
  |---|---|
  | 0:06.5–0:09.0 | dock → full |
  | 0:09.0–0:10.5 | completion pop/fly/praise |
  | 0:29.5 | undock pauses the flow |
  | 0:39–0:54 | color-filter drain |
  | 2:47 | side piles |
