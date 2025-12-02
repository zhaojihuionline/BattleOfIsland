# Copilot Instructions — URP_QF_Hot_2022

Purpose: provide concise, actionable guidance so an AI coding agent can be immediately productive in this Unity project.

1) Big picture
- This is a Unity (URP) repository with a split between engine assets and a hotfix/hybrid-CLR workflow. Key roots:
  - `Assets/` — game code and Unity assets (gameplay, scenes, plugins).
  - `HotfixOutput/HotUpdateDlls/` — compiled hotfix assemblies (DLLs) intended for runtime hot-reload via HybridCLR.
  - `HybridCLRData/` and `HybridCLRGenerate` — HybridCLR support and metadata generation.
  - `*.csproj` + `URP_QF_Hot_2022.sln` — generated C# projects and solution for IDEs.

2) Where to look first (high-value files)
- Gameplay & hotfix code: `Assets/Scripts/Hotfix/GameLogic/` (example: `BPathMove.cs`).
- Hotfix pipeline: `HotfixOutput/HotUpdateDlls/`, `Hotfix.csproj`.
- HybridCLR docs/metadata: `HybridCLRData/hybridclr_repo/README_EN.md` and files under `HybridCLRData/`.
- Solution/project-level: `URP_QF_Hot_2022.sln`, `Assembly-CSharp.csproj`.

3) Project-specific workflows (discoverable patterns)
- Hotfix build flow: edit C# hotfix sources under `Assets/Scripts/Hotfix/` or the separate hotfix project, build a hotfix assembly, then place the resulting DLLs into `HotfixOutput/HotUpdateDlls/`. The runtime loader expects those DLLs and HybridCLR metadata to be present.
- HybridCLR metadata: `HybridCLRGenerate` / `HybridCLRData` hold generation artifacts. Read `HybridCLRData/hybridclr_repo/README_EN.md` before changing CLR/hot-reload steps.
- Opening the project: use Unity Editor (recommended) with `URP_QF_Hot_2022.sln` available for quick IDE navigation.

4) Build & run notes (how humans do this)
- Editor: open the project in Unity Hub or run Unity Editor with `-projectPath` to load the project.
- Command-line (example placeholder — replace Unity path & build method):
  ```powershell
  & 'C:\Program Files\Unity\Hub\Editor\<VERSION>\Editor\Unity.exe' -projectPath 'e:\Unity\SVN\code\client\URP_QF_Hot_2022' -quit -batchmode -executeMethod MyBuildClass.PerformBuild -logFile build.log;
  ```
- Hotfix assembly generation is project-specific; search for editor scripts or CI scripts that call HybridCLR generation (look under `HybridCLRGenerate/`, `Assets/Editor/` and `HotfixOutput/`).

5) Common libraries & conventions
- Frameworks present: `QFramework`, `UniTask`, `DOTween`, `AstarPathfinding` and `HybridCLR`. Look for their usage patterns across `Assets/Scripts`.
- Naming: hotfix code is separated under `Hotfix` paths; runtime/engine code sits in other `Assets/Scripts` subfolders.

6) Integration points and runtime expectations
- Hot-update loader: runtime code expects prebuilt hotfix DLLs in `HotfixOutput/HotUpdateDlls/` and matching HybridCLR metadata under `HybridCLRData/`.
- Native plugins / AOT: `HybridCLRData/AssembliesPostIl2CppStrip/` and `StrippedAOTDllsTempProj` indicate special handling for AOT/IL2CPP builds — avoid changing AOT-related files without testing.

7) Useful quick searches (examples an agent should run)
- Find hotfix entrypoints: search for `Hotfix`, `LoadHotfix`, `HotUpdate`, `HotUpdateDlls`.
- HybridCLR hooks: search for `HybridCLR`, `Generate`, `HotfixOutput`.
- Assembly boundaries: inspect `*.csproj` files to understand compile-time separation.

8) Code style & safety notes (project-specific)
- Do not assume editor helper methods exist — check `Assets/Editor/` or `HybridCLRGenerate/` before referencing build helper names.
- Avoid modifying files under `Library/`, `Temp/`, or `obj/` — these are generated.

9) Where to add tests / verification
- This repo does not expose a standard test harness. For quick verification, run in the Editor and exercise the scene(s). For hotfix changes, place compiled DLLs into `HotfixOutput/HotUpdateDlls/` and verify runtime loader behavior in Editor or instructed test scene.

10) If you are merging existing agent guidance
- No top-level `.github/copilot-instructions.md` detected — this file is newly added. If you have internal AGENT.md files elsewhere, merge their concrete commands and paths into section 3 and 4.

---
If any area is ambiguous or you want me to include exact build commands (CI scripts or the project has a known build class), point me to the build script or editor helper and I will add concrete, runnable commands.
