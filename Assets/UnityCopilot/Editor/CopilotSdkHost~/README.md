# Copilot SDK Host (Editor-Only)

This is a standalone .NET 8 host that runs the GitHub Copilot SDK outside of Unity.
Unity communicates with the host over stdio, so the SDK can target .NET 8 without
breaking Unity's managed runtime.

## Build (Win-x64, Self-Contained)

From this folder:

```powershell
dotnet restore
dotnet publish -c Release -r win-x64 --self-contained true -o publish/win-x64
```

The Unity editor window expects the executable at:

```
Assets/UnityCopilot/Editor/CopilotSdkHost~/publish/win-x64/CopilotSdkHost.exe
```

## Adding macOS/Linux Later

You can publish additional runtimes alongside `win-x64`:

```powershell
dotnet publish -c Release -r osx-x64 --self-contained true -o publish/osx-x64
dotnet publish -c Release -r osx-arm64 --self-contained true -o publish/osx-arm64
dotnet publish -c Release -r linux-x64 --self-contained true -o publish/linux-x64
```

Unity will choose the matching runtime automatically when you click
`Use Default Host Path` in the Copilot SDK window.
