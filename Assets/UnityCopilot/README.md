# Unity Copilot

Unity Copilot is a plugin that provides AI-powered development assistance and code generation capabilities for Unity projects.

## Features

- AI-powered code assistance
- Editor integrations
- Development workflow enhancements
- Scene tooling via Copilot SDK, including `create_game_object` with optional primitive creation

## Installation

### Install via Package Manager (Embedded Package)

This package is currently set up as an embedded package in your project's `Assets` folder.

### Requirements

- Unity 2021.3 or later

## Usage

Access Unity Copilot features through the Unity Editor menu and windows.

### Scene Tooling: Primitive Creation

When using the Copilot SDK scene tools, `create_game_object` now supports optional primitive creation through `primitiveType`.

- `primitiveType` is optional
- Supported values: `Cube`, `Sphere`, `Capsule`, `Cylinder`, `Plane`, `Quad`
- When omitted, an empty GameObject is created

Example tool payload:

```json
{
  "name": "Floor",
  "primitiveType": "Plane",
  "setPosition": true,
  "position": { "x": 0, "y": 0, "z": 0 }
}
```

## Documentation

### Core Documentation
- **[AGENTS.md](AGENTS.md)** - Comprehensive guide to the agent system
- **[QUICKSTART.md](QUICKSTART.md)** - Quick start guide for developers
- **[PACKAGE_STRUCTURE.md](PACKAGE_STRUCTURE.md)** - Package structure details
- **[Documentation~/](Documentation~/index.md)** - API reference and guides

### Key Concepts
- Agent-based architecture
- Natural language processing
- Context-aware code generation
- Intelligent refactoring
- Custom agent development

## Support

For issues, questions, or contributions, please visit the project repository.

## License

See [LICENSE.md](LICENSE.md) for details.

