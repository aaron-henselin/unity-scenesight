using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using GitHub.Copilot.SDK;

internal sealed class HostRequest
{
    public string? Type { get; set; }
    public string? Prompt { get; set; }
    public string? Model { get; set; }
    public bool Streaming { get; set; }
    public string? CliUrl { get; set; }
    public string? RepoRoot { get; set; }
    public string? ExcludedPaths { get; set; }
    public int MaxFileSizeKb { get; set; }
    public int MaxSearchResults { get; set; }
    public string? Mode { get; set; }
    public int TimeoutSeconds { get; set; }
    public bool ToolDebug { get; set; }
    public string? ToolId { get; set; }
    public string? ToolName { get; set; }
    public string? ToolPayload { get; set; }
    public string? ImageBase64 { get; set; }
}

internal sealed class HostResponse
{
    public string Type { get; set; } = "status";
    public string? Content { get; set; }
    public string? Message { get; set; }
    public string? ToolId { get; set; }
    public string? ToolName { get; set; }
    public string? ToolPayload { get; set; }
}

internal sealed class WriteFileArgs
{
    [Description("Relative path under the repository root.")]
    public string? Path { get; set; }

    [Description("File contents to write.")]
    public string? Content { get; set; }

    [Description("Overwrite existing file when true.")]
    public bool Overwrite { get; set; } = true;
}

internal sealed class CaptureSceneSnapshotArgs
{
    [Description("Maximum width of the snapshot (pixels).")]
    public int MaxWidth { get; set; } = 1024;

    [Description("Maximum height of the snapshot (pixels).")]
    public int MaxHeight { get; set; } = 768;

    [Description("Snapshot focus mode: 'whole_scene' or 'selected_assets'.")]
    public string FocusMode { get; set; } = "whole_scene";
}

internal sealed class ListComponentsArgs
{
    [Description("Hierarchy path to the GameObject (preferred).")]
    public string? TargetPath { get; set; }

    [Description("Exact GameObject name to match when targetPath is not provided.")]
    public string? TargetName { get; set; }

    [Description("Optional scene name. Leave empty to use the active scene.")]
    public string? SceneName { get; set; }

    [Description("Include inactive objects when searching by name.")]
    public bool IncludeInactive { get; set; } = true;

    [Description("If true, apply to all name matches when targetPath is not provided.")]
    public bool ApplyToAllMatches { get; set; }
}

internal sealed class Vector3Data
{
    [Description("X component.")]
    public float X { get; set; }

    [Description("Y component.")]
    public float Y { get; set; }

    [Description("Z component.")]
    public float Z { get; set; }
}

internal sealed class ListGameObjectsArgs
{
    [Description("Optional case-insensitive substring to match against GameObject names. Leave empty to list all.")]
    public string? NameContains { get; set; }

    [Description("Optional scene name to search. Leave empty to use the active scene.")]
    public string? SceneName { get; set; }

    [Description("Include inactive objects.")]
    public bool IncludeInactive { get; set; } = true;

    [Description("Maximum results to return.")]
    public int MaxResults { get; set; } = 50;

    [Description("Optional component queries to include properties in results.")]
    public List<ComponentPropertyRequest>? Components { get; set; } = new();
}

internal sealed class ComponentPropertyRequest
{
    [Description("Component type name (simple or full name).")]
    public string? ComponentType { get; set; }

    [Description("Properties or fields to read. Leave empty to skip property data.")]
    public List<string>? Properties { get; set; } = new();
}

internal sealed class CreateGameObjectArgs
{
    [Description("Name of the GameObject. Defaults to 'New GameObject' if omitted.")]
    public string? Name { get; set; }

    [Description("Optional primitive type to create instead of an empty GameObject. Supported values: Cube, Sphere, Capsule, Cylinder, Plane, Quad.")]
    public string? PrimitiveType { get; set; }

    [Description("Optional scene name. Leave empty to use the active scene.")]
    public string? SceneName { get; set; }

    [Description("Optional parent path (e.g. 'Root/Child').")]
    public string? ParentPath { get; set; }

    [Description("Set active state when true.")]
    public bool SetActive { get; set; }

    [Description("Active state to apply when setActive is true.")]
    public bool Active { get; set; } = true;

    [Description("Set tag when true.")]
    public bool SetTag { get; set; }

    [Description("Tag to apply when setTag is true.")]
    public string? Tag { get; set; }

    [Description("Set layer when true.")]
    public bool SetLayer { get; set; }

    [Description("Layer to apply when setLayer is true.")]
    public int Layer { get; set; }

    [Description("Set world position when true.")]
    public bool SetPosition { get; set; }

    [Description("World position to apply when setPosition is true.")]
    public Vector3Data? Position { get; set; }

    [Description("Set local position when true.")]
    public bool SetLocalPosition { get; set; }

    [Description("Local position to apply when setLocalPosition is true.")]
    public Vector3Data? LocalPosition { get; set; }

    [Description("Set world rotation (Euler degrees) when true.")]
    public bool SetRotation { get; set; }

    [Description("World rotation (Euler degrees) to apply when setRotation is true.")]
    public Vector3Data? Rotation { get; set; }

    [Description("Set local rotation (Euler degrees) when true.")]
    public bool SetLocalRotation { get; set; }

    [Description("Local rotation (Euler degrees) to apply when setLocalRotation is true.")]
    public Vector3Data? LocalRotation { get; set; }

    [Description("Set local scale when true.")]
    public bool SetScale { get; set; }

    [Description("Local scale to apply when setScale is true.")]
    public Vector3Data? Scale { get; set; }
}

internal sealed class UpdateGameObjectArgs
{
    [Description("Hierarchy path to the GameObject (preferred).")]
    public string? TargetPath { get; set; }

    [Description("Exact GameObject name to match when targetPath is not provided.")]
    public string? TargetName { get; set; }

    [Description("Optional scene name. Leave empty to use the active scene.")]
    public string? SceneName { get; set; }

    [Description("Include inactive objects when searching by name.")]
    public bool IncludeInactive { get; set; } = true;

    [Description("If true, apply updates to all name matches when targetPath is not provided.")]
    public bool ApplyToAllMatches { get; set; }

    [Description("Rename the GameObject when provided.")]
    public string? NewName { get; set; }

    [Description("Reparent to this path when provided.")]
    public string? ParentPath { get; set; }

    [Description("Set active state when true.")]
    public bool SetActive { get; set; }

    [Description("Active state to apply when setActive is true.")]
    public bool Active { get; set; } = true;

    [Description("Set tag when true.")]
    public bool SetTag { get; set; }

    [Description("Tag to apply when setTag is true.")]
    public string? Tag { get; set; }

    [Description("Set layer when true.")]
    public bool SetLayer { get; set; }

    [Description("Layer to apply when setLayer is true.")]
    public int Layer { get; set; }

    [Description("Set world position when true.")]
    public bool SetPosition { get; set; }

    [Description("World position to apply when setPosition is true.")]
    public Vector3Data? Position { get; set; }

    [Description("Set local position when true.")]
    public bool SetLocalPosition { get; set; }

    [Description("Local position to apply when setLocalPosition is true.")]
    public Vector3Data? LocalPosition { get; set; }

    [Description("Set world rotation (Euler degrees) when true.")]
    public bool SetRotation { get; set; }

    [Description("World rotation (Euler degrees) to apply when setRotation is true.")]
    public Vector3Data? Rotation { get; set; }

    [Description("Set local rotation (Euler degrees) when true.")]
    public bool SetLocalRotation { get; set; }

    [Description("Local rotation (Euler degrees) to apply when setLocalRotation is true.")]
    public Vector3Data? LocalRotation { get; set; }

    [Description("Set local scale when true.")]
    public bool SetScale { get; set; }

    [Description("Local scale to apply when setScale is true.")]
    public Vector3Data? Scale { get; set; }
}

internal sealed class DeleteGameObjectArgs
{
    [Description("Hierarchy path to the GameObject (preferred).")]
    public string? TargetPath { get; set; }

    [Description("Exact GameObject name to match when targetPath is not provided.")]
    public string? TargetName { get; set; }

    [Description("Optional scene name. Leave empty to use the active scene.")]
    public string? SceneName { get; set; }

    [Description("Include inactive objects when searching by name.")]
    public bool IncludeInactive { get; set; } = true;

    [Description("Delete all name matches when targetPath is not provided.")]
    public bool DeleteAllMatches { get; set; }
}

internal sealed class Vector4Data
{
    [Description("X component.")]
    public float X { get; set; }

    [Description("Y component.")]
    public float Y { get; set; }

    [Description("Z component.")]
    public float Z { get; set; }

    [Description("W component.")]
    public float W { get; set; }
}

internal sealed class ColorData
{
    [Description("Red component (0-1).")]
    public float R { get; set; }

    [Description("Green component (0-1).")]
    public float G { get; set; }

    [Description("Blue component (0-1).")]
    public float B { get; set; }

    [Description("Alpha component (0-1).")]
    public float A { get; set; } = 1f;
}

internal sealed class ComponentPropertyAssignment
{
    [Description("Field or property name.")]
    public string? Name { get; set; }

    [Description("Value type: string, int, float, bool, vector3, vector4, color, assetPath, guid, globalId, instanceId.")]
    public string? ValueType { get; set; }

    [Description("String value for valueType=string, assetPath, guid, globalId.")]
    public string? StringValue { get; set; }

    [Description("Integer value for valueType=int or instanceId.")]
    public int? IntValue { get; set; }

    [Description("Float value for valueType=float.")]
    public float? FloatValue { get; set; }

    [Description("Boolean value for valueType=bool.")]
    public bool? BoolValue { get; set; }

    [Description("Vector3 value for valueType=vector3.")]
    public Vector3Data? Vector3Value { get; set; }

    [Description("Vector4 value for valueType=vector4.")]
    public Vector4Data? Vector4Value { get; set; }

    [Description("Color value for valueType=color.")]
    public ColorData? ColorValue { get; set; }
}

internal sealed class AddComponentArgs
{
    [Description("Hierarchy path to the GameObject (preferred).")]
    public string? TargetPath { get; set; }

    [Description("Exact GameObject name to match when targetPath is not provided.")]
    public string? TargetName { get; set; }

    [Description("Optional scene name. Leave empty to use the active scene.")]
    public string? SceneName { get; set; }

    [Description("Include inactive objects when searching by name.")]
    public bool IncludeInactive { get; set; } = true;

    [Description("If true, apply to all name matches when targetPath is not provided.")]
    public bool ApplyToAllMatches { get; set; }

    [Description("Component type name (simple name, e.g. BoxCollider).")]
    public string? ComponentType { get; set; }
}

internal sealed class RemoveComponentArgs
{
    [Description("Hierarchy path to the GameObject (preferred).")]
    public string? TargetPath { get; set; }

    [Description("Exact GameObject name to match when targetPath is not provided.")]
    public string? TargetName { get; set; }

    [Description("Optional scene name. Leave empty to use the active scene.")]
    public string? SceneName { get; set; }

    [Description("Include inactive objects when searching by name.")]
    public bool IncludeInactive { get; set; } = true;

    [Description("If true, apply to all name matches when targetPath is not provided.")]
    public bool ApplyToAllMatches { get; set; }

    [Description("Component type name (simple name, e.g. BoxCollider).")]
    public string? ComponentType { get; set; }
}

internal sealed class SetComponentPropertiesArgs
{
    [Description("Hierarchy path to the GameObject (preferred).")]
    public string? TargetPath { get; set; }

    [Description("Exact GameObject name to match when targetPath is not provided.")]
    public string? TargetName { get; set; }

    [Description("Optional scene name. Leave empty to use the active scene.")]
    public string? SceneName { get; set; }

    [Description("Include inactive objects when searching by name.")]
    public bool IncludeInactive { get; set; } = true;

    [Description("If true, apply to all name matches when targetPath is not provided.")]
    public bool ApplyToAllMatches { get; set; }

    [Description("Component type name (simple name, e.g. BoxCollider).")]
    public string? ComponentType { get; set; }

    [Description("Optional component instance IDs to target on the GameObject.")]
    public List<int>? ComponentInstanceIds { get; set; } = new();

    [Description("Properties to set.")]
    public List<ComponentPropertyAssignment> Properties { get; set; } = new();
}

internal sealed class CopilotHost : IAsyncDisposable
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly object _writeLock = new();
    private CopilotClient? _client;
    private CopilotSession? _session;
    private IDisposable? _subscription;
    private bool _streaming;
    private string _repoRoot = "";
    private HashSet<string> _excludedNames = new(StringComparer.OrdinalIgnoreCase);
    private int _maxFileSizeBytes = 200 * 1024;
    private int _maxSearchResults = 50;
    private string _modePrefix = "";
    private string _interactionMode = "ask";
    private bool _prefixSent;
    private int _timeoutSeconds = 180;
    private bool _toolDebug;
    private readonly object _attachmentLock = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string?>> _pendingToolCalls = new();
    private Task? _promptTask;
    private bool _turnHadSuccessfulMutation;
    private bool _turnHadSuccessfulPostMutationSnapshot;

    public static async Task Main()
    {
        var host = new CopilotHost();
        Console.CancelKeyPress += async (_, _) => await host.DisposeAsync();
        await host.RunAsync();
    }

    private async Task RunAsync()
    {
        string? line;
        while ((line = await Console.In.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            HostRequest? request = null;
            try
            {
                request = JsonSerializer.Deserialize<HostRequest>(line, _jsonOptions);
            }
            catch (Exception ex)
            {
                Send(new HostResponse { Type = "error", Message = ex.Message });
            }

            if (request == null || string.IsNullOrWhiteSpace(request.Type))
            {
                continue;
            }

            switch (request.Type)
            {
                case "start":
                    try
                    {
                        await HandleStartAsync(request);
                    }
                    catch (Exception ex)
                    {
                        Send(new HostResponse { Type = "error", Message = ex.Message });
                    }
                    break;
                case "prompt":
                    if (_promptTask != null && !_promptTask.IsCompleted)
                    {
                        Send(new HostResponse { Type = "error", Message = "A prompt is already running." });
                        break;
                    }

                    _promptTask = Task.Run(async () =>
                    {
                        try
                        {
                            await HandlePromptAsync(request);
                        }
                        catch (Exception ex)
                        {
                            Send(new HostResponse { Type = "error", Message = ex.Message });
                        }
                    });
                    break;
                case "tool_result":
                    HandleToolResult(request);
                    break;
                case "stop":
                    try
                    {
                        await HandleStopAsync();
                    }
                    catch (Exception ex)
                    {
                        Send(new HostResponse { Type = "error", Message = ex.Message });
                    }
                    break;
                default:
                    Send(new HostResponse { Type = "error", Message = $"Unknown request type '{request.Type}'." });
                    break;
            }
        }
    }

    private async Task HandleStartAsync(HostRequest request)
    {
        await HandleStopAsync();

        var model = string.IsNullOrWhiteSpace(request.Model) ? "claude-opus-4.5" : request.Model;
        _interactionMode = NormalizeMode(request.Mode);
        _streaming = request.Streaming;
        _prefixSent = false;
        _modePrefix = "";

        _repoRoot = NormalizeRepoRoot(request.RepoRoot);
        _excludedNames = BuildExcludedSet(request.ExcludedPaths);
        _maxFileSizeBytes = ClampMaxFileSize(request.MaxFileSizeKb);
        _maxSearchResults = ClampMaxSearchResults(request.MaxSearchResults);
        _timeoutSeconds = ClampTimeoutSeconds(request.TimeoutSeconds);
        _toolDebug = request.ToolDebug;

        CopilotClientOptions? clientOptions = null;
        if (!string.IsNullOrWhiteSpace(request.CliUrl))
        {
            clientOptions = new CopilotClientOptions { CliUrl = request.CliUrl, UseStdio = false };
        }

        _client = clientOptions == null ? new CopilotClient() : new CopilotClient(clientOptions);

        var sessionConfig = new SessionConfig
        {
            Model = model,
            Streaming = request.Streaming,
            Tools = BuildTools(_interactionMode)
        };

        var instructions = LoadInstructions();
        var systemMessage = BuildSystemMessage(_interactionMode, instructions);
        if (!string.IsNullOrWhiteSpace(systemMessage))
        {
            _modePrefix = systemMessage;
        }

        _session = await _client.CreateSessionAsync(sessionConfig);
        _subscription = _session.On(HandleEvent);

        Send(new HostResponse { Type = "status", Message = $"Session started (mode: {_interactionMode})." });
    }

    private async Task HandlePromptAsync(HostRequest request)
    {
        if (_session == null)
        {
            Send(new HostResponse { Type = "error", Message = "Session is not started." });
            return;
        }

        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            Send(new HostResponse { Type = "error", Message = "Prompt is empty." });
            return;
        }

        _turnHadSuccessfulMutation = false;
        _turnHadSuccessfulPostMutationSnapshot = false;

        var prompt = request.Prompt;
        if (!string.IsNullOrWhiteSpace(_modePrefix) && !_prefixSent)
        {
            prompt = $"{_modePrefix}\n\nUser: {prompt}";
            _prefixSent = true;
        }

        // Build message options, attaching image data when provided.
        var messageOptions = new MessageOptions { Prompt = prompt };
        if (!string.IsNullOrWhiteSpace(request.ImageBase64))
        {
            if (!TryCreateImageAttachment(request.ImageBase64, out var attachment, out var error, out var bytes))
            {
                Send(new HostResponse { Type = "error", Message = error });
                return;
            }

            messageOptions.Attachments = new List<UserMessageDataAttachmentsItem> { attachment };
            LogTool($"prompt image attachment path='{attachment.Path}' bytes={bytes}");
        }

        await SendPromptToSessionAsync(messageOptions);

        const int maxVerificationNudges = 2;
        var nudgeCount = 0;
        while (ShouldEnforceVisualVerification() && nudgeCount < maxVerificationNudges)
        {
            nudgeCount++;
            Send(new HostResponse { Type = "status", Message = "Verification required: capture a post-change snapshot before completion." });
            await SendPromptToSessionAsync(new MessageOptions
            {
                Prompt = "You made scene changes in this turn without visual verification. Continue working and do not conclude yet. "
                         + "Call capture_scene_snapshot with focusMode='selected_assets' for the changed asset(s), inspect the image result, "
                         + "and only then provide completion details. If the result is wrong, iterate until it is correct."
            });
        }

        Send(new HostResponse { Type = "done" });
    }

    private async Task HandleStopAsync()
    {
        try
        {
            _subscription?.Dispose();
        }
        catch
        {
            // Ignore subscription disposal errors.
        }
        finally
        {
            _subscription = null;
        }

        if (_session is IAsyncDisposable asyncSession)
        {
            await asyncSession.DisposeAsync();
        }
        else if (_session is IDisposable disposableSession)
        {
            disposableSession.Dispose();
        }

        _session = null;

        if (_client != null)
        {
            await _client.DisposeAsync();
            _client = null;
        }

        foreach (var entry in _pendingToolCalls)
        {
            entry.Value.TrySetCanceled();
        }

        _pendingToolCalls.Clear();

        Send(new HostResponse { Type = "status", Message = "Session stopped." });
    }

    private void HandleEvent(SessionEvent ev)
    {
        if (ev is AssistantMessageDeltaEvent deltaEvent)
        {
            if (_streaming && !string.IsNullOrWhiteSpace(deltaEvent.Data.DeltaContent))
            {
                Send(new HostResponse { Type = "delta", Content = deltaEvent.Data.DeltaContent });
            }

            return;
        }

        if (ev is AssistantMessageEvent)
        {
            // Non-streaming responses are sent from SendAndWait.
            return;
        }

        if (ev is SessionIdleEvent)
        {
            return;
        }

    }

    private ICollection<AIFunction> BuildTools(string mode)
    {
        var listFiles = AIFunctionFactory.Create(
            ([Description("Relative path under the repository root. Use '.' for root.")] string path) => ListFiles(path),
            "list_files",
            "List files and directories under a repository path.");

        var readFile = AIFunctionFactory.Create(
            ([Description("Relative path under the repository root.")] string path) => ReadFile(path),
            "read_file",
            "Read a text file from the repository.");

        var writeFile = AIFunctionFactory.Create(
            (WriteFileArgs args) => WriteFile(args),
            "write_file",
            "Create or overwrite a text file in the repository.");

        var searchText = AIFunctionFactory.Create(
            ([Description("Search query (plain text).")] string query,
                [Description("Relative path under the repository root. Use '.' for root.")] string path) => SearchText(query, path),
            "search_text",
            "Search for a string in the repository files.");

        var captureSceneSnapshot = AIFunctionFactory.Create(
            (CaptureSceneSnapshotArgs args) => CaptureSceneSnapshotAsync(args),
            "capture_scene_snapshot",
            "Capture the current Scene View snapshot and return a file path. Use focusMode='selected_assets' for target-focused verification, or 'whole_scene' for full context.");

        var listComponents = AIFunctionFactory.Create(
            (ListComponentsArgs args) => ListComponentsAsync(args),
            "list_components",
            "List all components on a GameObject in the active scene.");

        var listGameObjects = AIFunctionFactory.Create(
            (ListGameObjectsArgs args) => ListGameObjectsAsync(args),
            "list_game_objects",
            "List GameObjects in loaded Unity scenes (defaults to active scene).");

        var createGameObject = AIFunctionFactory.Create(
            (CreateGameObjectArgs args) => CreateGameObjectAsync(args),
            "create_game_object",
            "Create a GameObject in the active scene (or specified loaded scene). Supports optional primitiveType: Cube, Sphere, Capsule, Cylinder, Plane, Quad.");

        var addGameObject = AIFunctionFactory.Create(
            (CreateGameObjectArgs args) => AddGameObjectAsync(args),
            "add_game_object",
            "Alias for create_game_object. Supports optional primitiveType: Cube, Sphere, Capsule, Cylinder, Plane, Quad.");

        var updateGameObject = AIFunctionFactory.Create(
            (UpdateGameObjectArgs args) => UpdateGameObjectAsync(args),
            "update_game_object",
            "Update a GameObject in the active scene (or specified loaded scene).");

        var deleteGameObject = AIFunctionFactory.Create(
            (DeleteGameObjectArgs args) => DeleteGameObjectAsync(args),
            "delete_game_object",
            "Delete a GameObject in the active scene (or specified loaded scene).");

        var addComponent = AIFunctionFactory.Create(
            (AddComponentArgs args) => AddComponentAsync(args),
            "add_component",
            "Add a component to a GameObject in the active scene.");

        var removeComponent = AIFunctionFactory.Create(
            (RemoveComponentArgs args) => RemoveComponentAsync(args),
            "remove_component",
            "Remove a component from a GameObject in the active scene.");

        var setComponentProperties = AIFunctionFactory.Create(
            (SetComponentPropertiesArgs args) => SetComponentPropertiesAsync(args),
            "set_component_properties",
            "Set component properties on a GameObject in the active scene.");

        if (mode == "plan")
        {
            // Planning mode is read-only: inspect repository and scene state without mutations.
            return new List<AIFunction>
            {
                listFiles,
                readFile,
                searchText,
                captureSceneSnapshot,
                listComponents,
                listGameObjects
            };
        }

        return new List<AIFunction>
        {
            listFiles,
            readFile,
            writeFile,
            searchText,
            listGameObjects,
            createGameObject,
            addGameObject,
            updateGameObject,
            deleteGameObject,
            addComponent,
            removeComponent,
            setComponentProperties,
            captureSceneSnapshot,
            listComponents
        };
    }

    private object ListFiles(string? path)
    {
        try
        {
            LogTool($"list_files path='{path ?? "."}'");
            var directory = ResolvePath(path, expectDirectory: true);
            var entries = new List<object>();

            foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
            {
                if (IsExcluded(entry))
                {
                    continue;
                }

                var relative = Path.GetRelativePath(_repoRoot, entry);
                var isDirectory = Directory.Exists(entry);
                entries.Add(new { path = relative, type = isDirectory ? "directory" : "file" });
            }

            LogTool($"list_files -> {entries.Count} entries");
            return new
            {
                root = Path.GetRelativePath(_repoRoot, directory),
                entries
            };
        }
        catch (Exception ex)
        {
            LogTool($"list_files error: {ex.Message}");
            throw;
        }
    }

    private object ReadFile(string? path)
    {
        try
        {
            LogTool($"read_file path='{path ?? ""}'");
            var filePath = ResolvePath(path, expectDirectory: false);
            if (Directory.Exists(filePath))
            {
                return new { error = "Path is a directory." };
            }

            if (IsExcluded(filePath))
            {
                return new { error = "Path is excluded." };
            }

            var info = new FileInfo(filePath);
            if (!info.Exists)
            {
                return new { error = "File not found." };
            }

            var maxBytes = _maxFileSizeBytes;
            string content;
            var truncated = false;

            using (var stream = File.OpenRead(filePath))
            using (var reader = new StreamReader(stream))
            {
                if (info.Length > maxBytes)
                {
                    var buffer = new char[maxBytes];
                    var read = reader.ReadBlock(buffer, 0, maxBytes);
                    content = new string(buffer, 0, read) + "\n...truncated...";
                    truncated = true;
                }
                else
                {
                    content = reader.ReadToEnd();
                }
            }

            LogTool($"read_file -> {(truncated ? "truncated " : "")}{Math.Min(info.Length, maxBytes)} bytes");
            return new
            {
                path = Path.GetRelativePath(_repoRoot, filePath),
                content
            };
        }
        catch (Exception ex)
        {
            LogTool($"read_file error: {ex.Message}");
            throw;
        }
    }

    private object WriteFile(WriteFileArgs? args)
    {
        try
        {
            if (args == null)
            {
                return new { error = "Missing arguments." };
            }

            var relativePath = args.Path?.Trim();
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return new { error = "Path is required." };
            }

            var content = args.Content ?? "";
            var byteCount = Encoding.UTF8.GetByteCount(content);
            if (byteCount > _maxFileSizeBytes)
            {
                return new { error = $"Content exceeds max file size ({_maxFileSizeBytes} bytes)." };
            }

            var filePath = ResolveWritePath(relativePath, out var error);
            if (filePath == null)
            {
                return new { error };
            }

            if (Directory.Exists(filePath))
            {
                return new { error = "Path is a directory." };
            }

            var existed = File.Exists(filePath);
            if (existed && !args.Overwrite)
            {
                return new { error = "File already exists." };
            }

            File.WriteAllText(filePath, content);
            LogTool($"write_file -> {(existed ? "updated" : "created")} '{relativePath}' ({byteCount} bytes)");
            return new
            {
                path = Path.GetRelativePath(_repoRoot, filePath),
                created = !existed,
                overwritten = existed,
                bytes = byteCount
            };
        }
        catch (Exception ex)
        {
            LogTool($"write_file error: {ex.Message}");
            return new { error = ex.Message };
        }
    }

    private object SearchText(string? query, string? path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new { error = "Query is empty." };
            }

            LogTool($"search_text query='{query}' path='{path ?? "."}'");
            var directory = ResolvePath(path, expectDirectory: true);
            var results = new List<object>();

            foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            {
                if (IsExcluded(file))
                {
                    continue;
                }

                var info = new FileInfo(file);
                if (!info.Exists || info.Length > _maxFileSizeBytes)
                {
                    continue;
                }

                try
                {
                    var lineNumber = 0;
                    foreach (var line in File.ReadLines(file))
                    {
                        lineNumber++;
                        if (line.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            results.Add(new
                            {
                                path = Path.GetRelativePath(_repoRoot, file),
                                line = lineNumber,
                                text = line.Trim()
                            });

                            if (results.Count >= _maxSearchResults)
                            {
                                LogTool($"search_text -> {results.Count} results (cap reached)");
                                return new { query, results };
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore unreadable files.
                }
            }

            LogTool($"search_text -> {results.Count} results");
            return new { query, results };
        }
        catch (Exception ex)
        {
            LogTool($"search_text error: {ex.Message}");
            throw;
        }
    }

    private Task<object> CaptureSceneSnapshotAsync(CaptureSceneSnapshotArgs? args)
    {
        var safe = args ?? new CaptureSceneSnapshotArgs();
        safe.MaxWidth = Math.Clamp(safe.MaxWidth <= 0 ? 1024 : safe.MaxWidth, 1, 4096);
        safe.MaxHeight = Math.Clamp(safe.MaxHeight <= 0 ? 768 : safe.MaxHeight, 1, 4096);
        safe.FocusMode = string.Equals(safe.FocusMode, "selected_assets", StringComparison.OrdinalIgnoreCase)
            ? "selected_assets"
            : "whole_scene";
        LogTool($"capture_scene_snapshot request maxWidth={safe.MaxWidth} maxHeight={safe.MaxHeight} focusMode={safe.FocusMode}");
        return RequestUnityToolAsync("capture_scene_snapshot", safe);
    }

    private Task<object> ListComponentsAsync(ListComponentsArgs? args)
    {
        var safe = args ?? new ListComponentsArgs();
        safe.TargetPath = safe.TargetPath?.Trim();
        safe.TargetName = safe.TargetName?.Trim();
        LogTool($"list_components targetPath='{safe.TargetPath ?? ""}' targetName='{safe.TargetName ?? ""}'");
        return RequestUnityToolAsync("list_components", safe);
    }

    private Task<object> ListGameObjectsAsync(ListGameObjectsArgs? args)
    {
        var safe = args ?? new ListGameObjectsArgs();
        safe.NameContains = safe.NameContains?.Trim();
        safe.MaxResults = Math.Clamp(safe.MaxResults <= 0 ? 50 : safe.MaxResults, 1, 200);
        if (safe.Components != null)
        {
            foreach (var component in safe.Components)
            {
                if (component == null)
                {
                    continue;
                }

                component.ComponentType = component.ComponentType?.Trim();
                if (component.Properties != null)
                {
                    for (var i = 0; i < component.Properties.Count; i++)
                    {
                        component.Properties[i] = component.Properties[i]?.Trim();
                    }
                }
            }
        }
        LogListGameObjectsArgs(safe);
        return RequestUnityToolAsync("list_game_objects", safe);
    }

    private Task<object> CreateGameObjectAsync(CreateGameObjectArgs? args)
    {
        var safe = args ?? new CreateGameObjectArgs();
        safe.Name = string.IsNullOrWhiteSpace(safe.Name) ? "New GameObject" : safe.Name.Trim();
        safe.PrimitiveType = string.IsNullOrWhiteSpace(safe.PrimitiveType) ? null : safe.PrimitiveType.Trim();
        return RequestUnityToolAsync("create_game_object", safe);
    }

    private Task<object> AddGameObjectAsync(CreateGameObjectArgs? args)
    {
        return CreateGameObjectAsync(args);
    }

    private Task<object> AddComponentAsync(AddComponentArgs? args)
    {
        var safe = args ?? new AddComponentArgs();
        safe.TargetPath = safe.TargetPath?.Trim();
        safe.TargetName = safe.TargetName?.Trim();
        safe.ComponentType = safe.ComponentType?.Trim();
        return RequestUnityToolAsync("add_component", safe);
    }

    private Task<object> RemoveComponentAsync(RemoveComponentArgs? args)
    {
        var safe = args ?? new RemoveComponentArgs();
        safe.TargetPath = safe.TargetPath?.Trim();
        safe.TargetName = safe.TargetName?.Trim();
        safe.ComponentType = safe.ComponentType?.Trim();
        return RequestUnityToolAsync("remove_component", safe);
    }

    private Task<object> SetComponentPropertiesAsync(SetComponentPropertiesArgs? args)
    {
        var safe = args ?? new SetComponentPropertiesArgs();
        safe.TargetPath = safe.TargetPath?.Trim();
        safe.TargetName = safe.TargetName?.Trim();
        safe.ComponentType = safe.ComponentType?.Trim();
        LogComponentPropertyAssignments(safe);
        return RequestUnityToolAsync("set_component_properties", safe);
    }

    private Task<object> UpdateGameObjectAsync(UpdateGameObjectArgs? args)
    {
        var safe = args ?? new UpdateGameObjectArgs();
        safe.TargetPath = safe.TargetPath?.Trim();
        safe.TargetName = safe.TargetName?.Trim();
        safe.NewName = safe.NewName?.Trim();
        safe.ParentPath = safe.ParentPath?.Trim();
        return RequestUnityToolAsync("update_game_object", safe);
    }

    private Task<object> DeleteGameObjectAsync(DeleteGameObjectArgs? args)
    {
        var safe = args ?? new DeleteGameObjectArgs();
        safe.TargetPath = safe.TargetPath?.Trim();
        safe.TargetName = safe.TargetName?.Trim();
        return RequestUnityToolAsync("delete_game_object", safe);
    }

    private async Task<object> RequestUnityToolAsync(string toolName, object args)
    {
        var payload = JsonSerializer.Serialize(args, _jsonOptions);
        var id = Guid.NewGuid().ToString("N");
        var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingToolCalls[id] = tcs;

        LogTool($"{toolName} request");
        Send(new HostResponse
        {
            Type = "tool_request",
            ToolId = id,
            ToolName = toolName,
            ToolPayload = payload
        });

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_timeoutSeconds));
            using var _ = cts.Token.Register(() => tcs.TrySetCanceled());

            var resultPayload = await tcs.Task;
            if (string.IsNullOrWhiteSpace(resultPayload))
            {
                return new { error = "Unity tool returned an empty response." };
            }

            TrackTurnVerificationSignals(toolName, resultPayload);

            var result = JsonSerializer.Deserialize<object>(resultPayload, _jsonOptions);
            return result ?? new { error = "Unity tool returned an invalid response." };
        }
        catch (OperationCanceledException)
        {
            return new { error = "Unity tool request timed out." };
        }
        catch (Exception ex)
        {
            return new { error = ex.Message };
        }
        finally
        {
            _pendingToolCalls.TryRemove(id, out _);
        }
    }

    private string NormalizeRepoRoot(string? root)
    {
        var basePath = string.IsNullOrWhiteSpace(root) ? Directory.GetCurrentDirectory() : root;
        var full = Path.GetFullPath(basePath);
        if (!Directory.Exists(full))
        {
            throw new InvalidOperationException($"Repo root not found: {full}");
        }

        return full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static HashSet<string> BuildExcludedSet(string? excludedPaths)
    {
        var defaults = new[] { "Library", "Temp", "obj", "Logs", ".git", "CopilotSdkHost~", "UnityCopilot" };
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var value in defaults)
        {
            set.Add(value);
        }

        if (string.IsNullOrWhiteSpace(excludedPaths))
        {
            return set;
        }

        var parts = excludedPaths.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                set.Add(trimmed);
            }
        }

        return set;
    }

    private static int ClampMaxFileSize(int maxFileSizeKb)
    {
        if (maxFileSizeKb <= 0)
        {
            return 200 * 1024;
        }

        var clamped = Math.Clamp(maxFileSizeKb, 1, 1024);
        return clamped * 1024;
    }

    private static int ClampMaxSearchResults(int maxSearchResults)
    {
        if (maxSearchResults <= 0)
        {
            return 50;
        }

        return Math.Clamp(maxSearchResults, 1, 200);
    }

    private static int ClampTimeoutSeconds(int timeoutSeconds)
    {
        if (timeoutSeconds <= 0)
        {
            return 180;
        }

        return Math.Clamp(timeoutSeconds, 10, 600);
    }

    private void LogTool(string message)
    {
        if (!_toolDebug)
        {
            return;
        }

        Console.Error.WriteLine($"[CopilotSdkHost][Tool] {message}");
    }

    private bool TryCreateImageAttachment(string imageBase64, out UserMessageDataAttachmentsItemFile attachment, out string error, out int byteCount)
    {
        attachment = null;
        error = null;
        byteCount = 0;

        try
        {
            var payload = imageBase64.Trim();
            var commaIndex = payload.IndexOf(',');
            if (payload.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIndex >= 0)
            {
                payload = payload[(commaIndex + 1)..];
            }

            var bytes = Convert.FromBase64String(payload);
            byteCount = bytes.Length;

            var directory = Path.Combine(_repoRoot, ".copilot", "attachments");
            lock (_attachmentLock)
            {
                Directory.CreateDirectory(directory);
            }

            var fileName = $"scene_snapshot_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.png";
            var filePath = Path.Combine(directory, fileName);
            File.WriteAllBytes(filePath, bytes);

            attachment = new UserMessageDataAttachmentsItemFile
            {
                Path = filePath,
                DisplayName = "Scene Snapshot"
            };

            return true;
        }
        catch (Exception ex)
        {
            error = $"Failed to create image attachment: {ex.Message}";
            return false;
        }
    }

    private void LogComponentPropertyAssignments(SetComponentPropertiesArgs args)
    {
        if (!_toolDebug)
        {
            return;
        }

        var assignments = args.Properties;
        if (assignments == null || assignments.Count == 0)
        {
            LogTool("set_component_properties -> no properties provided");
            return;
        }

        var target = !string.IsNullOrWhiteSpace(args.TargetPath)
            ? $"path='{args.TargetPath}'"
            : $"name='{args.TargetName ?? ""}'";

        var componentType = string.IsNullOrWhiteSpace(args.ComponentType) ? "<missing>" : args.ComponentType;
        var items = new List<string>();
        foreach (var assignment in assignments)
        {
            if (assignment == null)
            {
                continue;
            }

            items.Add(FormatPropertyAssignment(assignment));
        }

        var instanceIds = args.ComponentInstanceIds != null && args.ComponentInstanceIds.Count > 0
            ? $" instanceIds=[{string.Join(", ", args.ComponentInstanceIds)}]"
            : "";

        LogTool($"set_component_properties {target} component='{componentType}'{instanceIds} props=[{string.Join(", ", items)}]");
    }

    private void LogListGameObjectsArgs(ListGameObjectsArgs args)
    {
        if (!_toolDebug)
        {
            return;
        }

        var nameContains = args.NameContains ?? "";
        var sceneName = args.SceneName ?? "";
        var includeInactive = args.IncludeInactive;
        var maxResults = args.MaxResults;

        var componentParts = new List<string>();
        if (args.Components != null)
        {
            foreach (var component in args.Components)
            {
                if (component == null)
                {
                    continue;
                }

                var type = component.ComponentType ?? "";
                var props = component.Properties == null || component.Properties.Count == 0
                    ? ""
                    : $"[{string.Join(", ", component.Properties)}]";
                componentParts.Add(string.IsNullOrWhiteSpace(props) ? type : $"{type}{props}");
            }
        }

        var componentSummary = componentParts.Count > 0
            ? $" components=[{string.Join(", ", componentParts)}]"
            : "";

        LogTool($"list_game_objects nameContains='{nameContains}' scene='{sceneName}' includeInactive={includeInactive} maxResults={maxResults}{componentSummary}");
    }

    private static string FormatPropertyAssignment(ComponentPropertyAssignment assignment)
    {
        var name = assignment.Name ?? "";
        var type = assignment.ValueType ?? "";
        var lower = type.Trim().ToLowerInvariant();

        return lower switch
        {
            "string" => $"{name}=\"{assignment.StringValue ?? ""}\"",
            "int" => $"{name}={assignment.IntValue ?? 0}",
            "float" => $"{name}={assignment.FloatValue ?? 0f}",
            "bool" => $"{name}={assignment.BoolValue ?? false}",
            "vector3" => $"{name}=({assignment.Vector3Value?.X ?? 0f},{assignment.Vector3Value?.Y ?? 0f},{assignment.Vector3Value?.Z ?? 0f})",
            "vector4" => $"{name}=({assignment.Vector4Value?.X ?? 0f},{assignment.Vector4Value?.Y ?? 0f},{assignment.Vector4Value?.Z ?? 0f},{assignment.Vector4Value?.W ?? 0f})",
            "color" => $"{name}=({assignment.ColorValue?.R ?? 0f},{assignment.ColorValue?.G ?? 0f},{assignment.ColorValue?.B ?? 0f},{assignment.ColorValue?.A ?? 1f})",
            "assetpath" => $"{name}=@\"{assignment.StringValue ?? ""}\"",
            "guid" => $"{name}=guid:{assignment.StringValue ?? ""}",
            "globalid" => $"{name}=globalId:{assignment.StringValue ?? ""}",
            "instanceid" => $"{name}=instanceId:{assignment.IntValue ?? 0}",
            _ => $"{name}({type})"
        };
    }

    private void HandleToolResult(HostRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ToolId))
        {
            Send(new HostResponse { Type = "error", Message = "Tool result missing toolId." });
            return;
        }

        if (_pendingToolCalls.TryRemove(request.ToolId, out var tcs))
        {
            tcs.TrySetResult(request.ToolPayload);
        }
        else
        {
            Send(new HostResponse { Type = "error", Message = $"No pending tool call for id '{request.ToolId}'." });
        }
    }


    private string ResolvePath(string? path, bool expectDirectory)
    {
        var relative = string.IsNullOrWhiteSpace(path) || path == "." ? "" : path;
        var combined = string.IsNullOrEmpty(relative)
            ? _repoRoot
            : Path.GetFullPath(Path.Combine(_repoRoot, relative));

        if (!IsUnderRoot(combined))
        {
            throw new InvalidOperationException("Path is outside the repository root.");
        }

        if (expectDirectory)
        {
            if (!Directory.Exists(combined))
            {
                throw new InvalidOperationException("Directory not found.");
            }
        }
        else if (!File.Exists(combined) && !Directory.Exists(combined))
        {
            throw new InvalidOperationException("Path not found.");
        }

        return combined;
    }

    private string? ResolveWritePath(string relativePath, out string error)
    {
        error = null;
        var combined = Path.GetFullPath(Path.Combine(_repoRoot, relativePath));

        if (!IsUnderRoot(combined))
        {
            error = "Path is outside the repository root.";
            return null;
        }

        if (IsExcluded(combined))
        {
            error = "Path is excluded.";
            return null;
        }

        var directory = Path.GetDirectoryName(combined);
        if (string.IsNullOrWhiteSpace(directory))
        {
            error = "Path is invalid.";
            return null;
        }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return combined;
    }

    private bool IsUnderRoot(string fullPath)
    {
        var root = _repoRoot;
        var rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase)
               || string.Equals(fullPath, root, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsExcluded(string fullPath)
    {
        var relative = Path.GetRelativePath(_repoRoot, fullPath);
        var segments = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        foreach (var segment in segments)
        {
            if (_excludedNames.Contains(segment))
            {
                return true;
            }
        }

        return false;
    }

    private string? LoadInstructions()
    {
        var path = Path.Combine(_repoRoot, ".github", "copilot-instructions.md");
        if (!File.Exists(path))
        {
            return null;
        }

        var info = new FileInfo(path);
        if (info.Length > 100 * 1024)
        {
            return null;
        }

        try
        {
            return File.ReadAllText(path);
        }
        catch
        {
            return null;
        }
    }

    private static string BuildSystemMessage(string? mode, string? instructions)
    {
        var normalized = NormalizeMode(mode);
        var basePrompt = normalized switch
        {
            "agent" => "You are an autonomous coding agent. Use the available tools to inspect the repository and propose concrete code changes. For any scene mutation, you must verify with capture_scene_snapshot (focusMode='selected_assets' for changed targets) before claiming completion. Ask clarifying questions when needed.",
            "plan" => "You are a planning assistant. Provide a short step-by-step plan only. You may use read-only tools to inspect files and scene state for clarification, including capture_scene_snapshot for visual checks. Do not make code changes or perform mutating tool actions.",
            _ => "You are a helpful assistant. Provide concise answers and ask clarifying questions when needed."
        };

        if (!string.IsNullOrWhiteSpace(instructions))
        {
            return $"{basePrompt}\n\nRepository instructions:\n{instructions}";
        }

        return basePrompt;
    }

    private static string NormalizeMode(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            return "ask";
        }

        var normalized = mode.Trim().ToLowerInvariant();
        return normalized switch
        {
            "agent" => "agent",
            "plan" => "plan",
            _ => "ask"
        };
    }

    private void Send(HostResponse response)
    {
        var json = JsonSerializer.Serialize(response, _jsonOptions);
        lock (_writeLock)
        {
            Console.Out.WriteLine(json);
            Console.Out.Flush();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await HandleStopAsync();
    }

    private async Task SendPromptToSessionAsync(MessageOptions messageOptions)
    {
        var response = await _session!.SendAndWaitAsync(messageOptions, TimeSpan.FromSeconds(_timeoutSeconds));
        if (!_streaming && response != null && !string.IsNullOrWhiteSpace(response.Data?.Content))
        {
            Send(new HostResponse { Type = "message", Content = response.Data.Content });
        }
    }

    private bool ShouldEnforceVisualVerification()
    {
        return string.Equals(_interactionMode, "agent", StringComparison.OrdinalIgnoreCase)
               && _turnHadSuccessfulMutation
               && !_turnHadSuccessfulPostMutationSnapshot;
    }

    private void TrackTurnVerificationSignals(string toolName, string resultPayload)
    {
        if (PayloadHasError(resultPayload))
        {
            return;
        }

        if (IsMutatingUnityTool(toolName))
        {
            _turnHadSuccessfulMutation = true;
            _turnHadSuccessfulPostMutationSnapshot = false;
            return;
        }

        if (string.Equals(toolName, "capture_scene_snapshot", StringComparison.OrdinalIgnoreCase)
            && _turnHadSuccessfulMutation)
        {
            _turnHadSuccessfulPostMutationSnapshot = true;
        }
    }

    private static bool IsMutatingUnityTool(string toolName)
    {
        return string.Equals(toolName, "create_game_object", StringComparison.OrdinalIgnoreCase)
               || string.Equals(toolName, "add_game_object", StringComparison.OrdinalIgnoreCase)
               || string.Equals(toolName, "update_game_object", StringComparison.OrdinalIgnoreCase)
               || string.Equals(toolName, "delete_game_object", StringComparison.OrdinalIgnoreCase)
               || string.Equals(toolName, "add_component", StringComparison.OrdinalIgnoreCase)
               || string.Equals(toolName, "remove_component", StringComparison.OrdinalIgnoreCase)
               || string.Equals(toolName, "set_component_properties", StringComparison.OrdinalIgnoreCase);
    }

    private static bool PayloadHasError(string payload)
    {
        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (!document.RootElement.TryGetProperty("error", out var errorElement))
            {
                return false;
            }

            return errorElement.ValueKind == JsonValueKind.String
                   && !string.IsNullOrWhiteSpace(errorElement.GetString());
        }
        catch
        {
            return false;
        }
    }
}
