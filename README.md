# SceneSight: A Vision-Enabled Copilot for Unity

This repository brings vision-enabled Copilot workflows into Unity Editor development.

The goal is to make AI assistance scene-aware, not just code-aware. The agent can inspect project files, inspect scene state, mutate scene objects/components, capture snapshots, and verify visual outcomes before claiming completion.

## Goals

- Combine code understanding and scene understanding in one workflow.
- Enable targeted scene editing through tool calls.
- Enforce visual verification for scene mutations with snapshots.
- Support planning workflows that can still inspect files and scene state.

## Conventional MCP vs SceneSight

Conventional MCP-style integrations are mainly tool-call orchestration:

- Agent reads text/tool outputs and decides next actions.
- Scene correctness is inferred from tool success, not visually confirmed.
- Human users often need to babysit, re-prompt, and manually catch visual mistakes.

SceneSight adds a computer-vision feedback loop to re-steer the agent:

- The agent captures visual state before/after changes.
- It evaluates what is actually in the scene, not only what tools reported.
- It is pushed to continue iterating when visual verification is missing or wrong.
- Focused snapshots (`selected_assets`) reduce ambiguity and improve correction speed.

In short:

- Conventional approach: "tool succeeded, assume done."
- SceneSight approach: "visually verify, then decide done."

## Feature Checklist

| Capability | Standalone agents | Traditional + MCP | SceneSight |
|---|---|---|---|
| 🤖 Natural-language tasking | ✅ | ✅ | ✅ |
| 📁 File read/search tooling | ✅ | ✅ | ✅ |
| 🧱 Scene graph tooling (list/create/update/delete) | ❌ | ✅ | ✅ |
| 🧩 Component introspection/edit tooling | ❌ | ✅ | ✅ |
| 📸 Snapshot capture integrated in agent loop | ❌ | 🟡 Optional | ✅ |
| 🎯 Target-focused visual capture (`selected_assets`) | ❌ | 🟡 Rare | ✅ |
| ✅ Visual verification required before completion | ❌ | ❌ | ✅ |
| 🔁 Automatic re-steering after visual mismatch | ❌ | 🟡 Limited | ✅ |
| 🧑‍💻 Reduced babysitting for scene correctness | ❌ | 🟡 Partial | ✅ |
| 🗺️ Plan mode with scene/file inspection | ⚪ N/A | 🟡 Varies | ✅ |

## What Is Included

- Unity Editor chat window (`CopilotSdkWindow`).
- External .NET host process (`CopilotSdkHost`) that runs the Copilot SDK.
- Unity scene tools (list/create/update/delete objects, component tools, snapshots).
- Snapshot capture modes:
  - `whole_scene`
  - `selected_assets`
- Primitive-aware object creation for `create_game_object` and `add_game_object`.

## Prerequisites

- Unity (project-compatible version).
- .NET SDK (for building the host; .NET 8+ recommended).
- GitHub Copilot SDK installed and available to the host runtime.
- GitHub Copilot access/auth configured for the runtime you use.
- Windows for the default `build.ps1` flow shown below.

### Where to get GitHub Copilot SDK

For this Unity integration, the host is .NET-based, so install:

1. GitHub Copilot CLI (install + auth):
   - https://docs.github.com/en/copilot/how-tos/set-up/install-copilot-cli
2. .NET package used by this host:
   - `dotnet add package GitHub.Copilot.SDK`

Optional references for other runtimes are documented in:

- `Assets/UnityCopilot/Documentation~/github-copilot-getting-started.md`

## Installation and Run

### 1. Build the Copilot host executable

From `Assets/UnityCopilot/Editor/CopilotSdkHost~`:

```powershell
./build.ps1
```

`build.ps1` runs:

```powershell
dotnet restore CopilotSdkHost.csproj
dotnet publish CopilotSdkHost.csproj -c Release -r win-x64 --self-contained true -o publish/win-x64
```

Build Windows + macOS in one invocation:

```powershell
./build.ps1 -Runtimes win-x64,osx-x64,osx-arm64
```

Expected output executable:

`Assets/UnityCopilot/Editor/CopilotSdkHost~/publish/win-x64/CopilotSdkHost.exe`

### 2. Open the Unity project

Open this repository in Unity and let scripts compile.

### 3. Open the Copilot window

In Unity menu:

`Window -> Unity Copilot -> Copilot SDK`

### 4. Configure host path (first run)

In the Copilot window Settings:

- Click `Use Default Host Path`, or
- set `Host Path` manually to:
  - `Assets/UnityCopilot/Editor/CopilotSdkHost~/publish/win-x64/CopilotSdkHost.exe`

Optional:

- Click `Use Project Root` to set repo root.
- Choose model and interaction mode (`ask`, `agent`, `plan`).

### 5. Start host and chat

- Click `Start Host`.
- Send prompts in the chat input.

## Modes

- `ask`: general assistant behavior.
- `plan`: planning-only behavior with read-only tools, including snapshot capture for clarification.
- `agent`: execution mode with mutating tools enabled.

When switching plan to execution with "proceed", the window can auto-switch to `agent` and carry context forward.

## Verification Policy

For scene-changing work in `agent` mode:

1. make scene mutation(s)
2. capture post-change snapshot
3. verify visual result
4. continue iterating until correct

Host-side guardrails require snapshot verification after successful scene mutation before final completion.

## Snapshot Usage

Use `capture_scene_snapshot` with:

- `focusMode: "whole_scene"` for broad context
- `focusMode: "selected_assets"` for target-focused verification

Snapshots are shown as image bubbles in chat history and can be expanded.

## Common Issues

- Host does not start:
  - rebuild host via `./build.ps1`
  - confirm `Host Path` points to the published executable
- Agent seems in wrong mode:
  - switch mode in UI and ensure host restarts with the selected mode
  - check status line for reported mode
- Plan mode cannot mutate:
  - expected behavior; switch to `agent` to execute changes

## Key Paths

- Root docs: `README.md`
- Unity package docs: `Assets/UnityCopilot/README.md`
- Host project: `Assets/UnityCopilot/Editor/CopilotSdkHost~/`
- Host build script: `Assets/UnityCopilot/Editor/CopilotSdkHost~/build.ps1`
- Agent guidance: `.github/copilot-instructions.md`
