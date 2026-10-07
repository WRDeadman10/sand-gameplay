# Decisions — Color Sand Prototype

> Update for every major decision (AGENTS.md §16). Format: context → options → decision → consequences.
> Status values: ✅ Accepted · 🟡 Provisional (works now; confirm later) · ⏸ Pending user

## Index

| ID | Topic | Status |
|---|---|---|
| [D-001](#d-001) | Never commit to git , Only Antigravity is allowed to commit and push | ✅ Accepted (user) |
| [D-002](#d-002) | Sand simulation approach: CPU cellular automaton | ✅ Accepted |
| [D-003](#d-003) | Two grids (tank + box interior) joined by an outlet transfer | ✅ Accepted |
| [D-004](#d-004) | Rendering: one point-filtered Texture2D per grid | ✅ Accepted |
| [D-005](#d-005) | Tank content and the box's color rule | ✅ Accepted (user): **Option A** |
| [D-006](#d-006) | Interaction: drag-to-dock (open/close the path) | ✅ Accepted |
| [D-007](#d-007) | Fill accounting and completion assist | ✅ Accepted |
| [D-008](#d-008) | Fixed-tick simulation and frame-rate policy | ✅ Accepted |
| [D-009](#d-009) | In-place restart (no scene reload) | ✅ Accepted |
| [D-010](#d-010) | No third-party tween library | ✅ Accepted |
| [D-011](#d-011) | Input System actions, reuse the existing asset, polling | ✅ Accepted |
| [D-012](#d-012) | Private field naming (`_camelCase` vs `camelCase`) | ✅ Accepted (user): `_camelCase` |
| [D-013](#d-013) | Missing `.editorconfig` | ✅ Resolved: user added the original file |
| [D-014](#d-014) | Folder, namespace, and assembly layout | ✅ Accepted |
| [D-015](#d-015) | UI stack: UGUI + TextMeshPro | ✅ Accepted |
| [D-016](#d-016) | Android identity and build settings | ✅ Accepted (user): `WRDeadman10` / `com.wrdeadman10.sand.gameplay` |
| [D-017](#d-017) | Template cleanup (unused packages and files) | ✅ Accepted (user) |
| [D-018](#d-018) | Art, fonts, and the 2D Sprite package | ✅ Accepted (user): download an OFL font; generate sprites |
| [D-019](#d-019) | Default level layout and grain resolution | ✅ Accepted (tunable) |
| [D-020](#d-020) | Editor tooling package `com.coplaydev.unity-mcp` in the submission | 🟡 Kept for development (user uses it); revisit in M8 |
| [D-021](#d-021) | Editor-first engineering; Samsung Galaxy S21 as the device target | ✅ Accepted (user) |
| [D-022](#d-022) | `Simulation` namespace split out of `Core` | ✅ Accepted |
| [D-023](#d-023) | Box interior geometry, spawn spread, gradual assist | ✅ Accepted |
| [D-024](#d-024) | Fill patterns: Solid + PixelArt only (no Bands mode) | ✅ Accepted |
| [D-025](#d-025) | Packages added outside the agent's actions are left in place | 🟡 Kept; revisit in M8 |
| [D-026](#d-026) | Scene assembled by editor scripting; Tank and Container prefabs | ✅ Accepted |
| [D-027](#d-027) | `InputSystem_Actions.inputactions` keeps its existing format | ✅ Accepted |
| [D-028](#d-028) | Editor test setup: Mobile quality, S21 Game view, run-in-background | ✅ Accepted |
| [D-029](#d-029) | MCP's Roslyn DLLs in `Assets/Plugins/Roslyn` are enabled for all platforms | ⏸ Pending user (recommend Editor-only before M7) |

---

<a id="d-001"></a>
## D-001 — Never commit to git , Only Antigravity is allowed to commit and push

- **Date:** 2026-10-07 · **Status:** ✅ Accepted (explicit user instruction)
- **Context:** The repo is on `main` with everything staged and no commits. The user said: "Don't commit to git ever".
- **Decision:** Agents never run `git commit`, `git push`, `--amend`, or any other history-writing command. CODE_FORMAT_STANDARDS.md's "before commit" checks are still run. The work is then handed back for the user to commit.
- **Consequences:** Every milestone ends with files changed but uncommitted. The handoff lists the modified files so the user can review and commit.

<a id="d-002"></a>
## D-002 — Sand simulation approach

- **Date:** 2026-10-07 · **Status:** ✅ Accepted
- **Context:** The brief allows particles, a custom grid, ECS/DOTS, or compute shaders, and judges the result plus reasonable performance. The reference's sand is visibly pixel-grain based: grains fall, slide, pile at ~45°, and patterns deform.

| Option | Pros | Cons |
|---|---|---|
| **CPU cellular automaton** (`ushort` grid) | Exactly the reference look; deterministic and unit-testable; cheap at ≤ 20 k cells; runs on every Android GPU; no packages; easy to explain | CPU-bound if grids grow (escalation: Burst) |
| ParticleSystem with collisions | Built-in | No stable piles; no per-grain persistence; hard to count fill |
| Rigidbody2D per grain | "Real" physics | Thousands of bodies is far too slow on mobile; jitter; violates the brief's guidance on per-particle objects |
| Compute-shader automaton | Massive grain counts | Needs ES 3.1/Vulkan compute; GPU→CPU readback to count fill; harder to test; overkill |
| DOTS/ECS | Scales | Heavy setup for 2 days; extra packages; harder to explain |

- **Decision:** CPU cellular automaton ([architecture.md §4](architecture.md#4-sand-simulation-core)).
- **Consequences:** Grids stay small; per-grain tone gives the look; Burst is the documented escalation path, only if M7 profiling demands it.

<a id="d-003"></a>
## D-003 — Two grids joined by an outlet transfer

- **Date:** 2026-10-07 · **Status:** ✅ Accepted
- **Context:** The box moves (it is dragged, and it flies away at completion) while it holds sand. The tank is static.
- **Options:**
  - **(a) One world grid:** moving the box means moving its cells through the grid, which is complex and error-prone during drags.
  - **(b) Tank grid + box-local interior grid, joined by an outlet:** the box's sand moves with its transform for free.
  - **(c) A logical fill bar only:** fails "the container visibly fills with sand".
- **Decision:** (b). Accepted grains in the tank's bottom row inside the outlet span go into a preallocated queue and spawn in the box's headroom rows ([architecture.md §7](architecture.md#7-the-path-docking-outlet-transfer)).
- **Consequences:** The tank and box have different cell counts, so the visual spawn uses a ratio (D-007). The headroom rows keep the stream visible across the rim.

<a id="d-004"></a>
## D-004 — Rendering: one point-filtered Texture2D per grid

- **Date:** 2026-10-07 · **Status:** ✅ Accepted
- **Options:** a texture per grid (the chosen option) · a mesh quad per grain (heavy vertex uploads) · GPU instancing (draw complexity) · a particle renderer (not grid-aligned).
- **Decision:** `Texture2D` RGBA32 with the Point filter, filled through `GetPixelData<Color32>` (no allocation), `Apply` only when the grid's `Version` changed. The sprite uses PPU = G.
- **Consequences:** Requires Render Scale 1.0 on the Mobile URP asset to stay crisp (part of T0.7). Upload cost is trivial (≈ 35 KB/frame at most).

<a id="d-005"></a>
## D-005 — Tank content and the box's color rule

- **Date:** 2026-10-07 · **Status:** ⏸ **Pending user** · **Recommended: Option A**
- **Context:** In the reference, a box only accepts sand of its own color, and a non-matching grain over the opening blocks the flow (Level 3, 0:49–0:51). With **one** box, a multi-color tank can never fully drain, and buried matching grains can become unreachable.

| Option | Tank | Box accepts | Pros | Cons |
|---|---|---|---|---|
| **A — Single-hue tank** *(recommended)* | One color in several tones: a light/dark pixel-art or band pattern | Its color (`MatchColor`) | Always completable; 100 % = tank empty; faithful color rule; tone patterns still deform visibly as sand flows | No color-filter puzzle; less colorful |
| B — Rainbow box | Multi-color pattern (like reference Levels 3, 6, 9) | Any color (`AcceptAny`) | Most colorful; the box fills with sand-art strata | Breaks the reference's color-match rule |
| C — Faithful filter | Multi-color pattern | Its color only (`MatchColor`) | Most faithful mechanic | Other colors stay in the tank forever; trapped grains need the assist; more tuning time; the end state looks unfinished |

- **Decision (user, 2026-10-07):** **Option A**, one color in several shades, always completable. The code keeps `ContainerColorRule` (`MatchColor` / `AcceptAny`) and a palette-color mapping in `TankFillPattern`, so B/C remain a data change.
- **Consequences:** the default level is a yellow tank with an amber/cream "sun" tone pattern (`PixelArt` mode, tone mapping) and a yellow 2×2 box. 100 % coincides with an empty tank.

<a id="d-006"></a>
## D-006 — Interaction: drag-to-dock

- **Date:** 2026-10-07 · **Status:** ✅ Accepted
- **Context:** The brief asks for touch/mouse input that "controls where/how the sand reaches the container" or "opens/closes the path". The reference does exactly this with a box dragged under the tank (Level 1 tutorial, 0:00–0:13; pausing at 0:29.5).
- **Options:** drag-to-dock (the chosen option) · tap a valve/gate on the tank · hold to pour.
- **Decision:** Drag the box on the board grid. Being docked under the tank opens the path. Sliding along the dock row moves the drain point. Pulling away closes it. The box keeps its sand when it undocks.
- **Consequences:** The dock state is computed every frame from the continuous position ([architecture.md §7.1](architecture.md#71-dock-detection-dockresolver-pure-static)). Side piles force repositioning, which makes the interaction meaningful even with one box.

<a id="d-007"></a>
## D-007 — Fill accounting and completion assist

- **Date:** 2026-10-07 · **Status:** ✅ Accepted
- **Decision:**
  1. `Capacity` = the count of acceptable grains in the tank at level start.
  2. `%` counts grains as they leave the tank.
  3. The visual spawn uses the accumulator ratio `interiorCells / capacity`.
  4. `CompletionThreshold` = 1.0.
  5. While docked with `FillRatio ≥ 0.97`, the remaining acceptable grains are absorbed. The reference also completes from 99 % (Level 3, 0:42–0:43).
  6. The interior is topped up at completion.
- **Consequences:** `%` is monotonic, and 100 % always coincides with a visually full box. No corner-grain hunting.

<a id="d-008"></a>
## D-008 — Fixed-tick simulation and frame-rate policy

- **Date:** 2026-10-07 · **Status:** ✅ Accepted
- **Decision:** 120 ticks/s via an accumulator, at most 4 ticks per frame (excess dropped), and settled grids sleep. `Application.targetFrameRate = 60` is set at boot, because mobile platforms default to about 30 FPS.
- **Consequences:** Behaviour is independent of the frame rate, and there is no spiral of death. Fall speed is 5 board cells/s at G = 24 (tunable).

<a id="d-009"></a>
## D-009 — In-place restart

- **Date:** 2026-10-07 · **Status:** ✅ Accepted
- **Options:**
  - **Reset in place** through `LevelController.Restart()` (the chosen option).
  - `SceneManager.LoadScene`: simpler, but slower, with a visible flicker, and it hides reset bugs. Kept as a fallback.
- **Consequences:** Every subsystem exposes a reset/initialize path, and event subscriptions are symmetric. Verified by the 10-cycle test (T5.7).

<a id="d-010"></a>
## D-010 — No third-party tween library

- **Date:** 2026-10-07 · **Status:** ✅ Accepted (the user may override)
- **Options:** coroutines + serialized `AnimationCurve`s + a small `Easing` class (the chosen option) · DOTween (popular, but not on the Unity registry, and it would need a README entry) · LeanTween.
- **Consequences:** Zero dependencies; feel is tunable in the Inspector; `CompletionSequencer.Stop()` is responsible for restoring transforms.

<a id="d-011"></a>
## D-011 — Input System actions, reusing the existing asset

- **Date:** 2026-10-07 · **Status:** ✅ Accepted
- **Context:** Active input handling is Input System only. `Assets/InputSystem_Actions.inputactions` already exists (Player + UI maps). AGENTS.md §8: reuse existing systems.
- **Decision:** Add a `Gameplay` map (`PointerPosition` → `<Pointer>/position`, `PointerPress` → `<Pointer>/press`), referenced via `InputActionReference` and **polled in `Update`**. Presses that start over UI are rejected.
- **Consequences:** One code path covers mouse, pen, and touch, with no UI-query warnings from input callbacks.

<a id="d-012"></a>
## D-012 — Private field naming

- **Date:** 2026-10-07 · **Status:** ✅ Accepted (confirmed by the user)
- **Context:** AGENTS.md §3 says "Private fields → camelCase". CODE_FORMAT_STANDARDS.md says "Private non-serialized fields: `_camelCase` (underscore prefix required)" and "`[SerializeField] private` → `camelCase`". The user said to follow CODE_FORMAT_STANDARDS.md while coding, and it is the more specific rule.
- **Decision:** `_camelCase` for private non-serialized fields (instance and static non-readonly); `camelCase` for `[SerializeField] private` fields; PascalCase for public members, constants, and `static readonly` fields.
- **Consequences:** The user-provided `.editorconfig` (D-013) has no private-field rule, so this decision stands. Its Unity-serialized-field rule (`lower_camel_case_style`) matches.

<a id="d-013"></a>
## D-013 — Missing `.editorconfig`

- **Date:** 2026-10-07 · **Status:** ✅ Resolved: the user added the original `D:\Unity\sand gameplay\.editorconfig`
- **Key rules it adds:**
  - `.cs` files: UTF-8 BOM, CRLF, 4 spaces, **`insert_final_newline = false`** (no trailing newline).
  - `csharp_new_line_before_open_brace = none` (K&R).
  - `var` and target-typed `new()` are errors; expression-bodied properties are errors.
  - Modifier order `public, private, protected, internal, file, new, static, abstract, virtual, sealed, readonly, override, async, unsafe, volatile, extern, required`.
  - `dotnet_separate_import_directive_groups = true`.
  - `.asmdef`, `.json`, and `.inputactions` files use **tab** indentation.
- **Original context:** AGENTS.md and CODE_FORMAT_STANDARDS.md both call the root `.editorconfig` the primary rule source, but none existed during the analysis. The only one in the tree is inside `Library/PackageCache/com.unity.ide.rider…`, which is a package file and not ours. Without one, `dotnet format` applies Roslyn defaults, **including Allman braces**, which would rewrite K&R code.
- **Options:**
  - **(a)** The user supplies the original `.editorconfig` from the source project. Preferred, because it is the true standard.
  - **(b)** Generate one that encodes AGENTS.md + CODE_FORMAT_STANDARDS.md (K&R, `csharp_style_var_* = false:error`, `csharp_style_implicit_object_creation_when_type_is_apparent = false`, `csharp_style_expression_bodied_* = false`, naming rules, CRLF, UTF-8 BOM, 4 spaces).
- **Rule until resolved:** never run `dotnet format`.

<a id="d-014"></a>
## D-014 — Folder, namespace, and assembly layout

- **Date:** 2026-10-07 · **Status:** ✅ Accepted
- **Decision:**
  - All our content goes under `Assets/_Project/`.
  - Root namespace `SandGameplay`, with sub-namespaces per folder.
  - Two asmdefs: `SandGameplay.Runtime` and `SandGameplay.Tests.EditMode` ([architecture.md §15](architecture.md#15-folder-assembly-and-namespace-layout)).
- **Consequences:** Faster compiles, isolated tests, and a clean separation from template files. The csproj names change, so the format commands target them.

<a id="d-015"></a>
## D-015 — UI stack

- **Date:** 2026-10-07 · **Status:** ✅ Accepted
- **Decision:**
  - UGUI + TextMeshPro (TMP is bundled in com.unity.ugui 2.0.0, so no extra package).
  - Canvas in Screen Space – Camera, so FX can sort above the UI.
  - A world-space `TextMeshPro` for the box's `%` label.
- **Alternative:** UI Toolkit, which has less mature runtime animation and world-space support for this use. Not chosen.

<a id="d-016"></a>
## D-016 — Android identity and build settings

- **Date:** 2026-10-07 · **Status:** ✅ Accepted (user)
- **Identity:** company **`WRDeadman10`**, Android package id **`com.wrdeadman10.sand.gameplay`**. The product name stays `sand gameplay` because the user did not ask for a change.
- **Accepted:**
  - Keep IL2CPP, ARM64, minSdk 25, target = highest installed, Vulkan + GLES3.
  - Lock the orientation to **portrait** (currently auto-rotation).
  - Version 0.1.0 / code 1.
  - Debug-signed APK for the test.
- **Security:** if a release keystore is ever used, the keystore file and its passwords stay out of the repo.

<a id="d-017"></a>
## D-017 — Template cleanup

- **Date:** 2026-10-07 · **Status:** ✅ Accepted (user)
- **Approved scope:**
  - Remove unused packages: `com.unity.visualscripting`, `com.unity.multiplayer.center`, `com.unity.ai.navigation`, `com.unity.collab-proxy`, `com.unity.timeline`.
  - Delete the template files `Assets/TutorialInfo/` and `Assets/Readme.asset`.
  - Remove `SampleScene` from the build (keep the file unless told otherwise).
- **Keep:** URP, Input System, UGUI, Test Framework, and the IDE packages.
- **Benefits:** smaller APK, faster compiles, a cleaner zip deliverable.
- **Risk:** low and reversible, since packages can be re-added in the Package Manager. Deleting files is the user's call.

<a id="d-018"></a>
## D-018 — Art, fonts, and the 2D Sprite package

- **Date:** 2026-10-07 · **Status:** ✅ Accepted (user: "download a free-licensed cartoon font, also download sprite and make yourself, whatever works")
- **Plan:** generate all sprites with a Python/Pillow script (no third-party art needed); download one OFL font from the official Google Fonts repository; set 9-slice borders by import script, so `com.unity.2d.sprite` is not required.
- **Original proposal:**
  - **Self-made simple sprites:** rounded panels, bevelled box, glass tank, star/sparkle particles. Generated as PNGs and 9-sliced.
  - **Add the first-party `com.unity.2d.sprite` package** for the Sprite Editor, where 9-slice borders are set. Without it, borders can only be set from script.
  - **Optionally a rounded cartoon font** under the SIL Open Font License (e.g. *Lilita One* or *Fredoka* from Google Fonts). Downloading it needs the user's approval, and it must be listed in the README. Fallback: TMP's bundled LiberationSans SDF.
  - **No store or ripped assets.** Nothing from the reference game is copied.

<a id="d-019"></a>
## D-019 — Default level layout and grain resolution

- **Date:** 2026-10-07 · **Status:** ✅ Accepted (values are tunable in M2)
- **Decision:**
  - Board 6 × 7 cells.
  - Tank 6 × 2 cells, full width on top, ~75 % filled.
  - Box 2 × 2 starting at (2, 0).
  - G = 24 grains per cell (tank 144 × 48; box interior 40 × 40 plus 4 headroom rows).
  - No walls.
- **Rationale:** A 2-wide box under a 6-wide tank leaves side piles, so the player has to slide the box along the dock row. That makes the single-box interaction meaningful, and it matches reference Level 7 (~2:47). G = 24 sits near the reference density (~25–33 grains per cell) at low cost.

<a id="d-020"></a>
## D-020 — Editor tooling package in the submission

- **Date:** 2026-10-07 · **Status:** ⏸ **Pending user**
- **Context:** During the analysis session, `Packages/manifest.json` gained `"com.coplaydev.unity-mcp": "https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main"`. It pulls `com.unity.nuget.newtonsoft-json` 3.2.2 in through `packages-lock.json`. The agent did not make this change. It is the Unity MCP bridge, used for editor automation.
- **Concerns for the deliverable:**
  1. The brief requires third-party packages to be listed in the README.
  2. `#main` is unpinned, so the zipped project would resolve whatever `main` is when it is opened. That breaks reproducibility.
  3. It is not needed to run the game.
- **Options:**
  - **(a)** Keep it during development and remove it from the manifest in the submitted zip (preferred).
  - **(b)** Pin it to a commit hash or tag and list it in the README as editor-only tooling.
- **Decision (so far):** the user uses the bridge, which is live on port 6400, so it **stays during development**. Removal or pinning for the submitted zip is decided in M8 (task T8.6). Default: remove it from the submitted manifest.

<a id="d-021"></a>
## D-021 — Editor-first engineering; Samsung Galaxy S21 as the device target

- **Date:** 2026-10-07 · **Status:** ✅ Accepted (user: "Focus on Engineering in Unity Editor for now, we would be using Samsung S21")
- **Decision:** M0–M6 are developed and verified in the Unity Editor (Game view at 1080×2400 portrait, the S21's aspect). On-device work (T4.10 first APK, M7 profiling) is deferred until the user asks for it.
- **S21 notes:** 1080×2400 (20:9) and a 120 Hz-capable display. We still target 60 FPS (`targetFrameRate = 60`) for thermals and battery.

<a id="d-022"></a>
## D-022 — `Simulation` namespace split out of `Core`

- **Date:** 2026-10-07 · **Status:** ✅ Accepted
- **Context:** `PourSprayVfx` (Presentation) needs the runner's `TransferredThisFrame`, and `LevelController` (Core) depends on Presentation. With the runner in Core, the two namespaces would depend on each other.
- **Decision:** `FixedTickClock`, `SandFlowSimulation`, and `SandSimulationRunner` live in `SandGameplay.Simulation`. `Easing` moved to `SandGameplay.Utilities` and the drag controller to `SandGameplay.Interaction` for the same reason (Container ↔ Presentation / Board ↔ Container).
- **Consequences:** The namespace graph has no cycles (see [architecture.md §15](architecture.md#15-folder-assembly-and-namespace-layout)). The tick pipeline itself is a pure class with its own tests.

<a id="d-023"></a>
## D-023 — Box interior geometry, spawn spread, gradual assist

- **Date:** 2026-10-07 · **Status:** ✅ Accepted (values tunable on the `Container` prefab and `DefaultSimulation`)
- **Decision:**
  1. The box walls are 4 grains (sides), 5 (front face), and 3 (rim), so a 2×2 box has a 40×40 interior plus 3 rim rows where poured grains appear and fall.
  2. Visual spawns get a random ±3 column spread so the fill rises evenly.
  3. The assist absorbs at most 24 grains per tick (lowest rows first) through the normal queue, so the last stranded grains drain over a few frames instead of vanishing at once.
- **Consequences:** The pour is visible crossing the rim. 100 % is always reached once the tank has been mostly drained. Verified in Play mode: middle dock gives 52 %, left pile 75 %, right pile 100 %.

<a id="d-024"></a>
## D-024 — Fill patterns: Solid + PixelArt only

- **Date:** 2026-10-07 · **Status:** ✅ Accepted
- **Decision:** `TankFillPattern` implements `Solid` and `PixelArt` (`ToneOfFillColor` or `NearestPaletteColor` mapping). The planned `Bands` mode was dropped, because a pixel-art texture expresses any stripe or layer and AGENTS.md §11 forbids unused abstractions.
- **Consequences:** New tank looks are made by drawing a small PNG (Read/Write, point filter, no NPOT scaling), for example by adding a function to `Tools/ArtGen/generate_sprites.py`.

<a id="d-025"></a>
## D-025 — Packages added outside the agent's actions are left in place

- **Date:** 2026-10-07 · **Status:** 🟡 Kept; revisit in M8
- **Context:** During the session, `com.unity.cinemachine`, `com.unity.cloud.gltfast`, `com.unity.probuilder`, and `com.unity.visualeffectgraph` appeared in the manifest alongside the MCP bridge (D-020). They were not on the D-017 cleanup list, and the game does not use them.
- **Decision:** Do not remove them without the user's approval (they may serve the MCP tooling).
- **Consequences:** Larger compile and import times, and possibly a larger APK (VFX Graph and ProBuilder runtime code). Re-evaluate together with D-020 before the submission build.

<a id="d-026"></a>
## D-026 — Scene assembled by editor scripting; Tank and Container prefabs

- **Date:** 2026-10-07 · **Status:** ✅ Accepted
- **Decision:**
  - `Game.unity`, its materials, data assets, and font asset were created through in-memory editor scripts over the Unity MCP bridge, wiring private serialized fields with `SerializedObject`.
  - No builder script is kept in the project (AGENTS.md §9: no unnecessary files). The scene and prefabs are the source of truth.
  - `Tank` and `Container` are prefabs, because they are the reusable gameplay entities.
- **Consequences:** Edit the scene normally in the Editor. Every reference is validated at runtime (`HasValidSetup`, `HasReferences`) with clear errors.

<a id="d-027"></a>
## D-027 — `InputSystem_Actions.inputactions` keeps its existing format

- **Date:** 2026-10-07 · **Status:** ✅ Accepted
- **Context:** `.editorconfig` asks for tab indentation in `.inputactions`, but the template file uses 4 spaces and LF.
- **Decision:** The `Gameplay` map was appended in the file's existing style (a 48-line diff) instead of reformatting the whole template file. CODE_FORMAT_STANDARDS.md: "Do not auto-apply style changes outside requested scope".

<a id="d-028"></a>
## D-028 — Editor test setup

- **Date:** 2026-10-07 · **Status:** ✅ Accepted
- **Decision:**
  - The Editor's active quality level is **Mobile** (the Android default), so the Editor renders with `Mobile_RPAsset`.
  - The Game view has a custom **1080×2400** size ("Galaxy S21").
  - Agent-driven Play-mode tests set `Application.runInBackground = true` at runtime only (not saved), because the unfocused Editor otherwise pauses the game loop.
- **Consequences:** Editor visuals and framing match the target phone. Play-mode testing over MCP works while the Editor window is in the background.

<a id="d-029"></a>
## D-029 — MCP's Roslyn DLLs in `Assets/Plugins/Roslyn`

- **Date:** 2026-10-07 · **Status:** ⏸ **Pending user**
- **Context:** `Assets/Plugins/Roslyn/` holds `Microsoft.CodeAnalysis*.dll`, `System.Collections.Immutable.dll`, `System.Reflection.Metadata.dll`, and `System.Runtime.CompilerServices.Unsafe.dll` (≈ 12.7 MB). They were placed at 11:12 by the Unity MCP setup (the C# compiler behind its `execute_code` tool), not by this project's code. Their `.meta` files contain only a GUID, so Unity uses the default plugin settings: **enabled for every platform, Android included**.
- **Risk:** The Android build would ship these assemblies and run them through IL2CPP, which means a larger APK, a slower build, and possible IL2CPP or stripping errors.
- **Recommendation (before M7):** mark them **Editor-only** in the Plugin Inspector (uncheck *Any Platform*, check *Editor*), or move them under an `Editor/` folder. The MCP bridge only runs in the Editor, so this does not affect it. Not changed yet because the files belong to the user's tooling.
