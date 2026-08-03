# Unity Project Context

<!-- unity-onboarding:generated:start -->

## Project Summary

- Project root: `E:/Unity/HexaForgeTeam`
- Project: DuskBlade, a scene-based Unity action game with first-party code split mainly across `Assets/_Data`, `Assets/Ben Ho`, and `Assets/Trung`.
- Last analyzed: 2026-08-01
- Last analyzed commit: `479a55813496dde5ca5e0d6982da33652a48f044` (`fix navmesh`)

## Confirmed Environment

- Unity version: 2022.3.62f3 (revision `96770f904ca7`)
- Render pipeline: Universal Render Pipeline 14.0.12
- Input system: Unity Input System 1.14.0 with generated `InputActions.cs`
- Target platforms: Windows and Android are documented test targets; active Editor build target was not verified.

## Important Packages And Frameworks

| Area | Finding | Confidence | Evidence |
| --- | --- | --- | --- |
| Async | UniTask from Git | Confirmed | `Packages/manifest.json` |
| Input | Unity Input System 1.14.0 | Confirmed | `Packages/manifest.json`, `Assets/_Data/InputSystem/InputActions.cs` |
| Rendering | URP 14.0.12, Shader Graph and VFX Graph | Confirmed | `Packages/manifest.json` |
| UI | uGUI, TextMesh Pro and Unity UI Extensions | Confirmed | `Packages/manifest.json` |
| Backend | PlayFab SDK is present in `Assets/PlayFabSDK` and used by login/data scripts | Confirmed | `Assets/PlayFabSDK`, `Assets/Trung/Scripts/Core/Login` |
| Tests | Unity Test Framework 1.1.33 | Confirmed | `Packages/manifest.json`, `Assets/Tests` |
| Audio | Unity Audio with `MainMixer` groups for Music, SFX, Dialogue and Collision | Confirmed | `Assets/Ben Ho/Audio/MainMixer.mixer` |

## Directory Structure

| Path | Purpose | Confidence | Evidence |
| --- | --- | --- | --- |
| `Assets/_Data/Scripts` | Main runtime gameplay and shared systems | Confirmed | Managers, player, combat, UI and audio scripts |
| `Assets/Ben Ho/Scripting` | Menu, settings, localization and lobby presentation code | Confirmed | Representative scripts and scenes |
| `Assets/Trung/Scripts` | Login, PlayFab, run flow, character and feature code | Confirmed | Representative scripts and scenes |
| `Assets/Tests/EditMode` | Editor/reference validation tests | Confirmed | `EditModeTests.asmdef` |
| `Assets/Tests/PlayMode` | Runtime integration tests | Confirmed | `DuskBlade.PlayModeTests.asmdef` |
| `Assets/Procedural Worlds`, `Assets/FImpossible Creations`, `Assets/PlayFabSDK` | Imported/vendor code | Confirmed | Package structure and assembly definitions |

## Assembly Boundaries

| Assembly | Responsibility | Key references | Notes |
| --- | --- | --- | --- |
| `Assembly-CSharp` | Most first-party runtime code | Unity modules and installed packages | No first-party runtime asmdef was found. |
| `DuskBlade.PlayModeTests` | PlayMode integration tests | Unity Test Runner | Enabled under `UNITY_INCLUDE_TESTS`. |
| `DuskBlade.EditModeTests` | EditMode/reference tests | PlayMode test assembly, Unity UI, test runners | Editor-only. |
| Vendor assemblies | Gaia, PlayFab and Odin modules | Package-specific | Keep first-party changes outside vendor folders. |

## Scenes And Startup Flow

- Enabled build scenes start with `Assets/_Data/Scenes/LongDemoScene.unity`, followed by UI Menu, Tutorial, Login, Loading, LobbyMain, Run and Main Map scenes.
- Likely startup scene: `LongDemoScene` by Build Settings index; product login flow also explicitly loads `Login Scene`, `Loading Scene` and `LobbyMain Scene`.
- Scene loading flow: both single and additive `SceneManager.LoadSceneAsync` calls are used.

## Architecture

| Pattern | Finding | Confidence | Evidence |
| --- | --- | --- | --- |
| Runtime composition | MonoBehaviour-centric scene composition | Confirmed | Representative first-party scripts and scenes |
| Global systems | Custom `Singleton<T>` based on `LoadComponents` | Confirmed | `Assets/_Data/Scripts/DesignPattern/Singleton.cs` |
| Data assets | ScriptableObjects are used for configuration/data | Confirmed | Audio, safe-zone and item data types |
| Persistence | PlayerPrefs for local settings; PlayFab for account/game data | Confirmed | Audio menu and PlayFab scripts |
| UI | `MenuBase`/`UIManager` menu system | Confirmed | `Assets/_Data/Scripts/UI`, `Assets/Ben Ho/Scripting/Menu` |

## Coding Conventions

- Namespace style: most first-party runtime scripts use the global namespace; tests use `DuskBlade.Tests`.
- Serialized fields: predominantly `[SerializeField] private`; older scripts also contain public Inspector fields.
- Lifecycle: `LoadComponents` separates editor `OnValidate` loading from runtime `Awake` loading.
- Async: coroutines and Unity async scene operations are common; UniTask is available.
- Comments/docs: concise comments, with Vietnamese documentation in some shared utilities and test reports.

## Testing And Validation

- EditMode tests: reference and asset validation under `Assets/Tests/EditMode`.
- PlayMode tests: integration suites for player, enemy, combat, camera, map collision, UI and console errors.
- CI/build validation: documented Unity CLI commands and Windows/Android matrix in `Docs/TESTING.md`; no CI workflow was verified.

## Available Unity Tooling

| Capability | Status | Evidence |
| --- | --- | --- |
| Unity Editor MCP capabilities | unavailable | No Unity MCP tool is exposed in this Codex session. |
| Repository inspection | available | Local workspace and PowerShell access. |
| Unity CLI validation | available | Unity 2022.3.62f3 at `D:/Unity/2022.3.62f3/Editor/Unity.exe`; targeted EditMode tests ran successfully on 2026-08-01. |
| Test Runner | available in project | Unity Test Framework and test assemblies are present. |

## Important Constraints

- Preserve untracked user audio assets under `Assets/MonstersSFX` and `Assets/_Data/Resources/Sounds/BG`.
- Avoid editing serialized scenes/prefabs unless required and validated through Unity.
- New audio settings should remain compatible with the `Audio.*` PlayerPrefs keys used by `AudioMenu`.
- Reuse exposed `MainMixer` parameters: `MasterVolume`, `MusicVolume`, `SoundEffectsVolume`, `DialogueVolume`, and `CollisionVolume`.

## Unknowns And Confidence

- The active platform and current Unity Console state are unknown without an Editor connection.
- Build Settings order and the explicit login flow provide two startup signals; the intended shipping boot scene should be confirmed before release work.
- No `AudioManager` component reference was found in serialized scenes or prefabs during this analysis; setup must be verified in the Unity Editor.

## Source Files Inspected

- `ProjectSettings/ProjectVersion.txt`
- `ProjectSettings/EditorBuildSettings.asset`
- `Packages/manifest.json`
- `Packages/packages-lock.json`
- `README.md`
- `Docs/TESTING.md`
- `Assets/_Data/Scripts/DesignPattern/Singleton.cs`
- `Assets/_Data/Scripts/Utils/LoadComponents.cs`
- `Assets/_Data/Scripts/Audio/*`
- `Assets/Ben Ho/Scripting/Menu/SettingMenu.cs`
- `Assets/Ben Ho/Scripting/SoundManager.cs`
- `Assets/Ben Ho/Audio/MainMixer.mixer`
- `Assets/Tests/EditMode/EditModeTests.asmdef`
- `Assets/Tests/PlayMode/DuskBlade.PlayModeTests.asmdef`

<!-- unity-onboarding:generated:end -->
