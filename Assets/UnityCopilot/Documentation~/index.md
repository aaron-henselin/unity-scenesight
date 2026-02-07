# Unity Copilot Documentation

Welcome to the Unity Copilot documentation. Unity Copilot is an AI-powered development assistant that uses specialized agents to enhance your Unity development workflow.

## Table of Contents

- [Getting Started](#getting-started)
- [Agent System](#agent-system)
- [Features](#features)
- [API Reference](#api-reference)

## Getting Started

Unity Copilot is designed to enhance your Unity development workflow with AI-powered assistance through specialized agents.

### Installation

The package is installed as an embedded package in your Unity project.

### First Steps

1. Open Unity Editor
2. Open the Agent Console: `Window > Unity Copilot > Agent Console`
3. Use natural language to describe what you want to create
4. Review and apply AI-generated solutions

### Basic Usage Example

```
User: "Create a health system with damage and healing"
→ Code Generation Agent creates complete implementation
→ Testing Agent generates unit tests
→ Documentation Agent adds XML comments
```

## Agent System

Unity Copilot uses specialized AI agents for different development tasks. Each agent is an expert in a specific domain.

### Available Agents

- **Code Generation Agent**: Creates Unity scripts from natural language
- **Refactoring Agent**: Improves code quality and structure
- **Testing Agent**: Generates unit and integration tests
- **Documentation Agent**: Creates XML comments and documentation
- **Architecture Agent**: Designs project structure
- **Gameplay Pattern Agent**: Implements common game patterns

For detailed information, see [AGENTS.md](../AGENTS.md).

### How Agents Work

Agents collaborate to solve complex tasks:
1. Natural language input is analyzed
2. Appropriate agents are selected
3. Agents work together or sequentially
4. Results are presented for review
5. Changes are applied to your project

## Features

### AI-Powered Code Generation

Generate Unity-specific code using natural language:
- MonoBehaviour scripts
- ScriptableObjects
- Editor scripts
- Custom inspectors
- Shader code (planned)

### Intelligent Refactoring

Improve existing code automatically:
- Apply SOLID principles
- Implement design patterns
- Optimize performance
- Fix code smells
- Update naming conventions

### Automated Testing

Generate comprehensive test coverage:
- Unit tests
- PlayMode tests
- EditMode tests
- Integration tests
- Mock object generation

### Context-Aware Assistance

Agents learn from your project:
- Existing code patterns
- Naming conventions
- Architecture decisions
- Used frameworks
- Team preferences

## API Reference

### Agent API

Documentation for creating custom agents and extending Unity Copilot functionality.

#### Creating a Custom Agent

```csharp
using YourCompany.UnityCopilot.Editor;

[CustomAgent("MyCustomAgent")]
public class MyCustomAgent : CopilotAgent
{
    public override string AgentName => "My Custom Agent";
    public override string Description => "Does custom things";
    
    public override AgentResponse ProcessRequest(AgentRequest request)
    {
        // Your agent logic here
        return new AgentResponse(/* ... */);
    }
}
```

#### Agent Base Classes

- `CopilotAgent` - Base class for all agents
- `CodeGenerationAgent` - Base for code generation agents
- `RefactoringAgent` - Base for refactoring agents
- `AnalysisAgent` - Base for code analysis agents

### Editor API

Access Unity Copilot features programmatically:

```csharp
using YourCompany.UnityCopilot.Editor;

// Invoke an agent
var response = CopilotAPI.InvokeAgent("CodeGeneration", request);

// Get agent suggestions
var suggestions = CopilotAPI.GetSuggestions(context);

// Apply code changes
CopilotAPI.ApplyChanges(changes);
```

### Copilot SDK Scene Tools

`create_game_object` supports creating either an empty GameObject or a Unity primitive.

- `primitiveType` is optional
- Supported values: `Cube`, `Sphere`, `Capsule`, `Cylinder`, `Plane`, `Quad`
- If `primitiveType` is omitted, the tool creates an empty GameObject

Example `create_game_object` payload:

```json
{
  "name": "SpawnMarker",
  "primitiveType": "Sphere",
  "setLocalScale": true,
  "localScale": { "x": 0.5, "y": 0.5, "z": 0.5 }
}
```

## Best Practices

### When to Use Agents

✅ **Recommended**:
- Boilerplate code generation
- Initial implementation scaffolding
- Refactoring large codebases
- Generating test coverage
- Learning new Unity patterns

⚠️ **Use with Caution**:
- Complex game-specific logic
- Performance-critical code
- Security-sensitive implementations

### Code Review

Always review AI-generated code:
- Verify correctness
- Check performance implications
- Ensure it follows project conventions
- Test thoroughly
- Understand what the code does

## Troubleshooting

### Common Issues

**Agent not responding**
- Check Unity Console for errors
- Restart the Agent Console window
- Verify package installation

**Generated code has errors**
- Provide more context in your request
- Review and refine the prompt
- Use the Refactoring Agent to fix issues

**Agent suggestions don't match project style**
- Configure agent settings
- Provide examples of preferred style
- Create custom agents for project-specific patterns

## Additional Resources

- [AGENTS.md](../AGENTS.md) - Comprehensive agent system guide
- [README.md](../README.md) - Package overview
- [QUICKSTART.md](../QUICKSTART.md) - Quick start guide

## Support

For issues, questions, or feature requests, please visit the project repository or contact support.

---

*Unity Copilot - Intelligent Development Assistance for Unity*

