# Orbit Dash

A tiny hypercasual mobile game built in Unity 6 (URP 2D) as a portfolio piece: dodge asteroids by
switching between concentric orbit rings around a central sun, with one input — tap.

![gameplay GIF placeholder](docs/gameplay.gif)
*(insert a 10-15s gameplay GIF here before publishing the portfolio page)*

**Play it:** [itch.io link — insert after hosting the WebGL build]

---

## What this project demonstrates

Orbit Dash is deliberately small in scope so the code stays readable end to end, but it's built to
show a specific set of engineering habits recruiters look for in a Unity codebase:

- **Object pooling**, written from scratch (`Utils/ObjectPool.cs`) instead of relying on
  `Instantiate`/`Destroy` in a loop — obstacles are recycled every run.
- **ScriptableObject-driven configuration** (`Data/`): ring layout, obstacle behaviour and the
  difficulty ramp are all tunable assets, not hardcoded constants — a designer could balance the
  whole game without touching a script.
- **A small, explicit state machine** (`Core/GameManager.cs`) driving Menu / Playing / Paused /
  GameOver, instead of scattered booleans.
- **Decoupled communication via C# events** (`Core/GameEvents.cs`): gameplay systems (obstacles,
  score, audio, camera shake, particles, haptics) never hold references to each other or to the UI —
  they all react to the same small set of static events. Adding a new reaction to "player got hit"
  never requires touching the class that raises it.
- **The new Input System**, used both as a raw polled action (`Core/TapInputReader.cs`, works with
  touch/mouse/pen through the generic `Pointer` device) and through `InputSystemUIInputModule` for
  UI navigation.
- A UI layer built entirely from code (`UI/`, see [UI architecture](#ui-architecture-note) below) —
  no hand-tweaked Canvas prefab to keep in sync with the scripts.

## Gameplay

- A single tap/click switches the player to the next orbit ring (inner → middle → outer → inner…).
- Asteroids spawn at increasing frequency and speed the longer you survive.
- Score = 1 point per second survived. Top 5 scores are saved locally (PlayerPrefs, JSON).
- Menu → Play → Pause anytime → Game Over → Replay/Menu, all with a short fade transition.

## Project structure

```
Assets/_Project/
  Scripts/
    Core/        GameManager (state machine), GameEvents, GameState, TapInputReader, AudioPreferences
    Gameplay/     OrbitController, ObstacleSpawner, Obstacle, ScoreManager, CameraShake,
                  ImpactParticles, HapticsFeedback, PlayerMarker
    UI/           UIManager, MenuScreen, PauseScreen, GameOverScreen, HudView, SafeAreaFitter
    Data/         OrbitLayoutConfig, ObstacleConfig, DifficultyCurveConfig (ScriptableObjects)
    Audio/        AudioManager
    Save/         SaveSystem
    Utils/        ObjectPool<T>, UIFactory, RuntimeCircleSpriteFactory, PlaceholderCircleVisual
    Editor/       BuildScript (command-line WebGL/Android build entry points)
  Data/Configs/   The actual ScriptableObject asset instances (tune these in the Inspector)
  Prefabs/        Obstacle.prefab (pooled)
  Scenes/         Game.unity (single scene; UI/state fully code-driven, no scene reload needed)
```

## UI architecture note

The Canvas, EventSystem and every screen (Menu/Pause/GameOver/HUD) are built at runtime in code
(`UIFactory` + `UIManager`), not authored as a Canvas prefab in the Editor. This was a deliberate
trade-off for this project: it kept the whole UI reliably reproducible and scriptable end to end.
The cost is that it's less drag-and-drop than a typical Unity UI workflow — reskinning it with real
art (see below) means pointing the factory at real sprites/fonts rather than dragging assets onto
Inspector fields. Worth knowing if you extend this project.

## Placeholder art

Right now the player, sun and obstacles are flat-color circles generated at runtime
(`Utils/RuntimeCircleSpriteFactory.cs`), and the UI is flat-color rectangles. This was intentional —
it let every gameplay/UI system be built and tested without waiting on art. Swapping in real art:

- **Player/Sun/Obstacle**: each has a `PlaceholderCircleVisual` component with an `Override Sprite`
  field in the Inspector — assign a real sprite there and the generated circle is skipped entirely.
  No code changes needed.
- **UI (buttons/panels/icons)**: since the UI is code-built, reskinning it means editing
  `UIFactory`/the screen scripts to reference imported sprites (e.g. via a small `Resources` folder
  or a serialized "UI Theme" ScriptableObject) rather than a simple drag-and-drop. Budget a short
  follow-up pass for this once assets are imported.
- **Particles**: `Gameplay/ImpactParticles.cs` builds a basic burst from a flat white dot
  (`Sprites/Default` shader) — assign a nicer texture/material to the `ParticleSystemRenderer` for a
  more convincing impact.

### Recommended free asset packs

Chosen from what's currently live on the Asset Store / kenney.nl at the time of writing (Sept 2026);
worth a quick check before importing in case a pack was renamed or superseded.

**Sprites (space / planets / asteroids)** — all CC0, free
- [Space Shooter Remastered](https://kenney.nl/assets/space-shooter-remastered) (Kenney) — 295
  assets: ships, lasers, asteroids, UI numbers. The direct successor to the older "Space Shooter
  Redux" pack.
- [Planets](https://kenney.nl/assets/planets) (Kenney) — 50 2D planet sprites, good for the sun and
  ring decoration.
- [Simple Space](https://kenney.nl/assets/simple-space) (Kenney) — 48 assets, flatter/more
  minimalist style if you want a cleaner look than Space Shooter Remastered.

**UI** — all CC0, free
- [UI Pack](https://kenney.nl/assets/ui-pack) (Kenney) — 430 assets, generic buttons/panels/sliders.
- [UI Pack - Sci-Fi](https://kenney.nl/assets/ui-pack-sci-fi) (Kenney) — 130 assets, thematically
  matches a space game better than the generic pack.
- [Game Icons](https://kenney.nl/assets/game-icons) (Kenney) — 105 icons (pause, replay, sound
  on/off, etc.) for the HUD/menu buttons.

**Particles**
- [Particle Pack](https://assetstore.unity.com/packages/vfx/particles/particle-pack-127325) (Unity
  Technologies, free) or the higher-rated
  [Legacy Particle Pack](https://assetstore.unity.com/packages/vfx/particles/legacy-particle-pack-73777)
  (also free). **Note:** both predate URP as the default pipeline — after import, re-assign any
  Built-in-RP-only shaders/materials to URP-compatible ones (e.g. `Universal Render Pipeline/Particles/Unlit`).
- Or skip the Asset Store entirely and keep tuning `ImpactParticles.cs` — it already builds a working
  burst from pure code, no import needed.

**Audio (SFX)** — all CC0, free
- [Digital Audio](https://kenney.nl/assets/digital-audio) (Kenney) — 60 SFX, good for the tap sound.
- [Impact Sounds](https://kenney.nl/assets/impact-sounds) (Kenney) — 130 SFX, good for the collision.
- [Sci-Fi Sounds](https://kenney.nl/assets/sci-fi-sounds) (Kenney) — 70 SFX, good for the dodge cue.

**Music** — Kenney doesn't currently have a dedicated looping-music pack live; for a CC0/royalty-free
background loop, [Kevin MacLeod's incompetech.com](https://incompetech.com/) (CC-BY, credit required)
is the standard go-to, or search itch.io for a CC0 loop bundle (e.g. Tallbeard Studios' free loop
pack).

**Font**
- [Orbitron](https://fonts.google.com/specimen/Orbitron) or
  [Press Start 2P](https://fonts.google.com/specimen/Press+Start+2P) (Google Fonts, OFL license) —
  download the TTF and create a TMP Font Asset (`Window > TextMeshPro > Font Asset Creator`).

Once imported, assign clips/sprites/fonts to the `AudioManager` Inspector fields (all currently
empty and silently no-op) and the `PlaceholderCircleVisual.overrideSprite` fields.

## Setup

1. Unity **6000.5.10f1** (Unity 6 LTS) or newer, with the **2D URP** template.
2. Open the project, let Unity import; if prompted, run
   `Window > TextMeshPro > Import TMP Essential Resources` once.
3. Open `Assets/_Project/Scenes/Game.unity` and press Play — mouse click doubles as tap in the Editor.

## Building

WebGL Build Support is installed; Android Build Support (SDK/NDK/OpenJDK) needs to be added via
Unity Hub before an Android build can run. `Assets/_Project/Editor/BuildScript.cs` exposes two
command-line entry points:

```
Unity.exe -batchmode -quit -projectPath . -executeMethod OrbitDash.EditorTools.BuildScript.BuildWebGL
Unity.exe -batchmode -quit -projectPath . -executeMethod OrbitDash.EditorTools.BuildScript.BuildAndroid
```

(Close the Editor first — Unity refuses a second instance on a project that's already open.) Output
goes to `Builds/WebGL/` and `Builds/Android/OrbitDash.apk`, both git-ignored.

For the portfolio: host the WebGL build on [itch.io](https://itch.io) (set it as an embedded HTML5
game) so it's playable directly from the portfolio page without installing anything, and keep the
APK as a downloadable link for a mobile demo.

## Design notes / known simplifications

- **"Dodge" SFX**: fires whenever an obstacle completes its lap without hitting the player, rather
  than a strict close-proximity check — see the comment in `ObstacleSpawner.HandleLapCompleted`.
- **UI raycast vs. gameplay taps**: `TapInputReader` checks `EventSystem.IsPointerOverGameObject()`
  before firing, so tapping the in-game Pause button doesn't also switch rings underneath it.
- Single-scene architecture: Menu/Playing/Paused/GameOver are UI overlays + enabled/disabled gameplay
  systems, not separate Unity scenes — avoids scene-load hitches for a game this small.
