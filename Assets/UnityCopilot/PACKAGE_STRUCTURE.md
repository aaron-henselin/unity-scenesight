# Unity Copilot Package Structure

This document describes the Unity Package Manager compliant structure for the Unity Copilot plugin.

## Package Information

- **Package Name**: `com.yourcompany.unitycopilot`
- **Display Name**: Unity Copilot
- **Version**: 1.0.0
- **Unity Version**: 2021.3 or later

## Directory Structure

```
Assets/UnityCopilot/
├── package.json                    # Package manifest (UPM requirement)
├── README.md                       # Package documentation
├── CHANGELOG.md                    # Version history
├── LICENSE.md                      # License information
├── .gitignore                      # Git ignore rules
│
├── Editor/                         # Editor-only scripts
│   ├── UnityCopilot.Editor.asmdef # Editor assembly definition
│   └── Test.cs                     # Example editor window
│
├── Runtime/                        # Runtime scripts (for future use)
│   ├── UnityCopilot.Runtime.asmdef # Runtime assembly definition
│   └── .gitkeep                    # Keeps empty folder in git
│
├── Tests/                          # Unit and integration tests
│   └── .gitkeep                    # Keeps empty folder in git
│
└── Documentation~/                 # Documentation files (ignored by Unity)
    └── index.md                    # Main documentation
```

## Assembly Definitions

### Editor Assembly (`UnityCopilot.Editor.asmdef`)
- **Name**: UnityCopilot.Editor
- **Root Namespace**: YourCompany.UnityCopilot.Editor
- **Platform**: Editor only
- **Purpose**: Contains all editor-only functionality

### Runtime Assembly (`UnityCopilot.Runtime.asmdef`)
- **Name**: UnityCopilot.Runtime
- **Root Namespace**: YourCompany.UnityCopilot
- **Platform**: All platforms
- **Purpose**: Contains runtime functionality (when needed)

## Key Files

### package.json
Defines the package metadata for Unity Package Manager, including:
- Package identifier
- Version number
- Dependencies
- Unity version requirements
- Package type and keywords

### Assembly Definition Files (.asmdef)
Define compilation units for Unity's assembly system:
- Separate compilation for faster builds
- Explicit dependency management
- Platform-specific compilation

## Usage

### Current Setup (Embedded Package)
The package is currently structured in the `Assets/UnityCopilot/` folder as an embedded package. This allows for:
- Easy development and testing
- Source control integration
- Immediate use in the project

### Future Options

#### Option 1: Move to Packages folder
Move the entire `UnityCopilot` folder to `Packages/com.yourcompany.unitycopilot/` and reference it in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.yourcompany.unitycopilot": "file:../Packages/com.yourcompany.unitycopilot"
  }
}
```

#### Option 2: Git URL Installation
Host on a Git repository and install via URL in Package Manager or manifest.json:

```json
{
  "dependencies": {
    "com.yourcompany.unitycopilot": "https://github.com/yourcompany/unitycopilot.git#v1.0.0"
  }
}
```

#### Option 3: Custom Registry
Publish to a custom UPM registry for enterprise distribution.

## Compliance Checklist

✅ package.json with proper metadata
✅ README.md for documentation
✅ CHANGELOG.md for version tracking
✅ LICENSE.md for licensing information
✅ Assembly definition files for proper compilation
✅ Proper folder structure (Runtime, Editor, Tests, Documentation~)
✅ Namespace conventions following package identifier
✅ Unity Package Manager compatible structure

## Next Steps

1. Add more functionality to the Editor scripts
2. Create runtime components as needed
3. Add unit tests in the Tests folder
4. Expand documentation
5. Consider moving to Packages folder for cleaner separation
6. Set up CI/CD for automated testing and publishing

## References

- [Unity Package Manager Documentation](https://docs.unity3d.com/Manual/Packages.html)
- [Creating Custom Packages](https://docs.unity3d.com/Manual/CustomPackages.html)
- [Assembly Definitions](https://docs.unity3d.com/Manual/ScriptCompilationAssemblyDefinitionFiles.html)

