using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace YourCompany.UnityCopilot.Editor
{
    public class CopilotSdkWindow : EditorWindow
    {
        private sealed class ChatMessage
        {
            public string Role;
            public string Content;
            public string ImageBase64;
            public Texture2D ImagePreview;
            public bool ImageExpanded;
            public bool IsToolCall;
            public string ToolName;
            public string ToolPayload;
            public string ToolResultPayload;
            public bool ToolDetailsExpanded;
        }

        private sealed class ModelOption
        {
            public string Name;
            public string Id;

            public ModelOption(string name, string id)
            {
                Name = name;
                Id = id;
            }
        }

        private static readonly ModelOption[] ModelOptions =
        {
            new ModelOption("GPT-4.1", "gpt-4.1"),
            new ModelOption("GPT-5", "gpt-5"),
            new ModelOption("GPT-5 mini", "gpt-5-mini"),
            new ModelOption("GPT-5-Codex", "gpt-5-codex"),
            new ModelOption("GPT-5.1", "gpt-5.1"),
            new ModelOption("GPT-5.1-Codex", "gpt-5.1-codex"),
            new ModelOption("GPT-5.1-Codex-Mini", "gpt-5.1-codex-mini"),
            new ModelOption("GPT-5.1-Codex-Max", "gpt-5.1-codex-max"),
            new ModelOption("GPT-5.2", "gpt-5.2"),
            new ModelOption("GPT-5.2-Codex", "gpt-5.2-codex"),
            new ModelOption("Claude Haiku 4.5", "claude-haiku-4.5"),
            new ModelOption("Claude Opus 4.1", "claude-opus-4.1"),
            new ModelOption("Claude Opus 4.5", "claude-opus-4.5"),
            new ModelOption("Claude Opus 4.6", "claude-opus-4.6"),
            new ModelOption("Claude Sonnet 4", "claude-sonnet-4"),
            new ModelOption("Claude Sonnet 4.5", "claude-sonnet-4.5"),
            new ModelOption("Gemini 2.5 Pro", "gemini-2.5-pro"),
            new ModelOption("Gemini 3 Flash", "gemini-3-flash"),
            new ModelOption("Gemini 3 Pro", "gemini-3-pro"),
            new ModelOption("Grok Code Fast 1", "grok-code-fast-1"),
            new ModelOption("Raptor mini", "raptor-mini")
        };

        private static readonly string[] ModelLabels = BuildModelLabels();
        private const string PromptControlName = "CopilotPrompt";
        private static readonly string[] SnapshotModeKeys = { "whole_scene", "selected_assets" };
        private static readonly string[] SnapshotModeLabels = { "Whole Scene", "Selected Assets" };

        // Models known to support vision/image input
        private static readonly HashSet<string> VisionModels = new(StringComparer.OrdinalIgnoreCase)
        {
            "gpt-4.1",
            "gpt-5",
            "gpt-5.1",
            "gpt-5.2",
            "claude-sonnet-4",
            "claude-sonnet-4.5",
            "claude-opus-4.1",
            "claude-opus-4.5",
            "claude-opus-4.6",
            "gemini-2.5-pro",
            "gemini-3-flash",
            "gemini-3-pro"
        };

        private CopilotSdkHostClient _client;
        private readonly List<ChatMessage> _messages = new();
        private Vector2 _scroll;
        private bool _scrollToBottom;
        private string _prompt = "";
        private string _status = "Host not running.";
        private string _error = "";
        private string _thinking = "";
        private int _pendingAssistantIndex = -1;
        private bool _isStarting;
        private bool _isAwaitingResponse;
        private int _spinnerIndex;
        private double _lastSpinnerUpdate;
        private bool _settingsExpanded = true;
        private GUIStyle _assistantBubbleStyle;
        private GUIStyle _userBubbleStyle;
        private GUIStyle _thinkingBubbleStyle;
        private GUIStyle _toolBubbleStyle;
        private Texture2D _assistantBubbleTexture;
        private Texture2D _userBubbleTexture;
        private Texture2D _thinkingBubbleTexture;
        private Texture2D _toolBubbleTexture;

        // Snapshot attachment
        private string _attachedSnapshotBase64;
        private Texture2D _attachedSnapshotPreview;
        private CopilotSdkSnapshotTool.SnapshotFocusMode _attachedSnapshotFocusMode = CopilotSdkSnapshotTool.SnapshotFocusMode.WholeScene;
        private string _lastSubmittedPrompt = "";
        private string _lastSubmittedImageBase64 = "";
        private bool _responseInterruptedByReload;
        private bool _switchedPlanToAgentThisSend;

        [MenuItem("Window/Unity Copilot/Copilot SDK")]
        public static void ShowWindow()
        {
            GetWindow<CopilotSdkWindow>("Copilot SDK");
        }

        private void OnEnable()
        {
            _client = new CopilotSdkHostClient();
            _client.OnStatus += status => _status = status;
            _client.OnAssistantDelta += AppendAssistantDelta;
            _client.OnAssistantMessage += AppendAssistantMessage;
            _client.OnAssistantComplete += CompleteAssistant;
            _client.OnThinkingDelta += AppendThinkingDelta;
            _client.OnToolCall += AppendToolCall;
            _client.OnError += error =>
            {
                _error = error;
                _isAwaitingResponse = false;
                Debug.LogError($"Copilot SDK Host Error: {error}");
                PersistSessionState();
            };
            RestoreSessionState();
            EditorApplication.update += UpdateSpinner;
            AssemblyReloadEvents.beforeAssemblyReload += HandleBeforeAssemblyReload;

            if (CopilotSdkSessionState.Get().HostWasRunning)
            {
                _ = StartSessionAsync();
            }
        }

        private void OnDisable()
        {
            PersistSessionState(markResponseInterrupted: _isAwaitingResponse && EditorApplication.isCompiling);
            DestroyMessageTextures();
            if (_attachedSnapshotPreview != null)
            {
                DestroyImmediate(_attachedSnapshotPreview);
                _attachedSnapshotPreview = null;
            }
            _client?.Dispose();
            _client = null;
            EditorApplication.update -= UpdateSpinner;
            AssemblyReloadEvents.beforeAssemblyReload -= HandleBeforeAssemblyReload;
        }

        private void OnGUI()
        {
            EnsureStyles();
            DrawSettingsSection();
            GUILayout.Space(8);
            DrawStatus();
            GUILayout.Space(8);
            DrawConversation();
            GUILayout.Space(8);
            DrawPrompt();
        }

        private void DrawSettingsSection()
        {
            _settingsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_settingsExpanded, "Settings");
            if (_settingsExpanded)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    DrawSettings();
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawSettings()
        {
            var settings = CopilotSdkSettings.Get();
            EditorGUILayout.LabelField("GitHub Copilot SDK", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            settings.HostExecutablePath = EditorGUILayout.TextField("Host Path", settings.HostExecutablePath);
            settings.HostArguments = EditorGUILayout.TextField("Host Args", settings.HostArguments);
            settings.CliUrl = EditorGUILayout.TextField("CLI Url", settings.CliUrl);
            settings.RepoRoot = EditorGUILayout.TextField("Repo Root", settings.RepoRoot);
            settings.ExcludedPaths = EditorGUILayout.TextField("Exclude Paths", settings.ExcludedPaths);
            settings.MaxFileSizeKb = EditorGUILayout.IntField("Max File KB", settings.MaxFileSizeKb);
            settings.MaxSearchResults = EditorGUILayout.IntField("Max Search Results", settings.MaxSearchResults);
            settings.TimeoutSeconds = EditorGUILayout.IntField("Timeout (sec)", settings.TimeoutSeconds);
            settings.ToolDebug = EditorGUILayout.Toggle("Tool Debug", settings.ToolDebug);
            settings.Model = DrawModelPopup(settings.Model);
            settings.Streaming = EditorGUILayout.Toggle("Streaming", settings.Streaming);
            settings.RenderMarkdown = EditorGUILayout.Toggle("Render Markdown", settings.RenderMarkdown);

            if (EditorGUI.EndChangeCheck())
            {
                settings.Save();
            }

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(_client == null || _client.IsRunning || _isStarting))
            {
                if (GUILayout.Button("Start Host"))
                {
                    _ = StartSessionAsync();
                }
            }

            using (new EditorGUI.DisabledScope(_client == null || !_client.IsRunning))
            {
                if (GUILayout.Button("Stop Host"))
                {
                    _ = StopSessionAsync();
                }
            }

            if (GUILayout.Button("Use Default Host Path"))
            {
                settings.HostExecutablePath = GetDefaultHostPath();
                settings.Save();
            }

            if (GUILayout.Button("Use Project Root"))
            {
                settings.RepoRoot = GetProjectRoot();
                settings.Save();
            }

            if (GUILayout.Button("Clear Chat"))
            {
                ResetConversationState();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox(
                "Host Path should point at the self-contained Copilot SDK host executable. " +
                "Leave CLI Url empty to let the SDK manage the CLI process, or set it to connect to a running --headless Copilot CLI.\n" +
                "Exclude Paths is a semicolon-separated list of folder names to skip when browsing the repo.",
                MessageType.None);
        }

        private void DrawStatus()
        {
            var status = _client != null && _client.IsRunning ? _status : "Host not running.";
            EditorGUILayout.LabelField("Status", status);

            if (_responseInterruptedByReload)
            {
                EditorGUILayout.HelpBox(
                    "The previous response was interrupted by script compilation. You can retry the last prompt.",
                    MessageType.Warning);

                using (new EditorGUI.DisabledScope(_isAwaitingResponse || string.IsNullOrWhiteSpace(_lastSubmittedPrompt)))
                {
                    if (GUILayout.Button("Retry Last Prompt"))
                    {
                        RetryInterruptedPrompt();
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(_error))
            {
                EditorGUILayout.HelpBox(_error, MessageType.Error);
            }
        }

        private void DrawConversation()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Conversation", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Export Chat", GUILayout.Width(90)))
                {
                    ExportConversationToMarkdown();
                }
                if (GUILayout.Button("New Chat", GUILayout.Width(90)))
                {
                    _ = StartNewChatAsync();
                }
            }

            if (_scrollToBottom)
            {
                _scroll.y = float.MaxValue;
            }
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));

            if (!string.IsNullOrWhiteSpace(_thinking))
            {
                DrawBubble(new ChatMessage { Content = _thinking }, null, false, _thinkingBubbleStyle);
            }

            foreach (var message in _messages)
            {
                var isUser = string.Equals(message.Role, "You", StringComparison.OrdinalIgnoreCase);
                var bubbleStyle = isUser
                    ? _userBubbleStyle
                    : message.IsToolCall ? _toolBubbleStyle : _assistantBubbleStyle;
                var icon = message.IsToolCall ? GetToolIcon(message.ToolName) : null;
                DrawBubble(message, icon, isUser, bubbleStyle);
            }

            EditorGUILayout.EndScrollView();
            if (_scrollToBottom && Event.current.type == EventType.Repaint)
            {
                _scrollToBottom = false;
            }
        }

        private void DrawPrompt()
        {
            EditorGUILayout.LabelField("Prompt", EditorStyles.boldLabel);
            HandlePromptSubmit();
            GUI.SetNextControlName(PromptControlName);
            EditorGUI.BeginChangeCheck();
            _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.MinHeight(60));
            if (EditorGUI.EndChangeCheck())
            {
                PersistSessionState();
            }

            // Draw snapshot attachment UI
            DrawSnapshotAttachment();

            var settings = CopilotSdkSettings.Get();
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(_isAwaitingResponse || _isStarting))
            {
                var selectedMode = DrawModePopup(settings.InteractionMode);
                if (!string.Equals(selectedMode, settings.InteractionMode, StringComparison.OrdinalIgnoreCase))
                {
                    settings.InteractionMode = selectedMode;
                    settings.Save();
                    PersistSessionState();
                    _ = ApplyModeChangeAsync(selectedMode);
                }
            }

            if (ShouldShowProceedButton(settings))
            {
                if (GUILayout.Button("Proceed", GUILayout.Width(80)))
                {
                    _prompt = "proceed";
                    SendPrompt();
                }
            }

            GUILayout.FlexibleSpace();

            if (_isAwaitingResponse)
            {
                var icon = EditorGUIUtility.IconContent($"WaitSpin{_spinnerIndex:00}");
                GUILayout.Label(icon, GUILayout.Width(16), GUILayout.Height(16));
                GUILayout.Space(6);
            }

            using (new EditorGUI.DisabledScope(_client == null || _isStarting))
            {
                if (GUILayout.Button("Send"))
                {
                    SendPrompt();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private async Task ApplyModeChangeAsync(string selectedMode)
        {
            _status = $"Mode set to '{selectedMode}'.";
            _error = "";
            PersistSessionState();

            if (_client == null || !_client.IsRunning || _isStarting || _isAwaitingResponse)
            {
                Repaint();
                return;
            }

            _status = $"Restarting host to apply mode '{selectedMode}'...";
            PersistSessionState();
            Repaint();

            try
            {
                await StopSessionAsync();
                await StartSessionAsync();
            }
            catch (Exception ex)
            {
                _error = ex.Message;
                Debug.LogException(ex);
                PersistSessionState();
                Repaint();
            }
        }

        private bool ShouldShowProceedButton(CopilotSdkSettings settings)
        {
            if (settings == null || !string.Equals(settings.InteractionMode, "plan", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (_isAwaitingResponse || _isStarting || !string.IsNullOrWhiteSpace(_thinking))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(_prompt))
            {
                return false;
            }

            for (var i = _messages.Count - 1; i >= 0; i--)
            {
                var message = _messages[i];
                if (message == null || message.IsToolCall)
                {
                    continue;
                }

                if (string.Equals(message.Role, "Copilot", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (string.Equals(message.Role, "You", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }

            return false;
        }

        private void DrawSnapshotAttachment()
        {
            var settings = CopilotSdkSettings.Get();
            var currentMode = NormalizeSnapshotMode(settings.SnapshotCaptureMode);
            var modeIndex = currentMode == "selected_assets" ? 1 : 0;

            EditorGUILayout.BeginHorizontal();

            var selectedModeIndex = EditorGUILayout.Popup(modeIndex, SnapshotModeLabels, GUILayout.Width(120));
            if (selectedModeIndex != modeIndex)
            {
                settings.SnapshotCaptureMode = SnapshotModeKeys[selectedModeIndex];
                settings.Save();
            }

            if (GUILayout.Button("Attach Snapshot", GUILayout.Width(120)))
            {
                AttachSceneSnapshot();
            }

            // Show preview if attached
            if (_attachedSnapshotPreview != null)
            {
                GUILayout.Space(8);

                // Preview thumbnail
                var previewRect = GUILayoutUtility.GetRect(48, 36, GUILayout.Width(48), GUILayout.Height(36));
                GUI.DrawTexture(previewRect, _attachedSnapshotPreview, ScaleMode.ScaleToFit);

                GUILayout.Space(4);
                GUILayout.Label("Attached", EditorStyles.miniLabel);

                // Remove button
                if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(18)))
                {
                    ClearAttachedSnapshot();
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            // Vision model warning
            if (_attachedSnapshotBase64 != null && !IsVisionModelSelected())
            {
                EditorGUILayout.HelpBox(
                    $"The selected model '{settings.Model}' may not support vision/images. " +
                    "Consider using a vision-capable model like GPT-4.1, Claude Sonnet 4+, or Gemini.",
                    MessageType.Warning);
            }
        }

        private void AttachSceneSnapshot()
        {
            if (!CopilotSdkSnapshotTool.IsSceneViewAvailable())
            {
                EditorUtility.DisplayDialog(
                    "Scene View Not Available",
                    "Please open a Scene View window before capturing a snapshot.",
                    "OK");
                return;
            }

            var settings = CopilotSdkSettings.Get();
            var focusMode = CopilotSdkSnapshotTool.ParseFocusMode(NormalizeSnapshotMode(settings.SnapshotCaptureMode));

            // Capture base64 for sending
            var base64 = CopilotSdkSnapshotTool.CaptureSceneViewBase64(
                settings.SnapshotMaxWidth,
                settings.SnapshotMaxHeight,
                focusMode);

            if (string.IsNullOrEmpty(base64))
            {
                EditorUtility.DisplayDialog(
                    "Capture Failed",
                    "Failed to capture Scene View snapshot. Check the console for details.",
                    "OK");
                return;
            }

            // Clear previous preview
            ClearAttachedSnapshot();

            _attachedSnapshotBase64 = base64;
            _attachedSnapshotFocusMode = focusMode;

            // Capture smaller preview for display
            _attachedSnapshotPreview = CopilotSdkSnapshotTool.CaptureSceneViewTexture(96, 72, focusMode);
            PersistSessionState();

            Repaint();
        }

        private void ClearAttachedSnapshot()
        {
            _attachedSnapshotBase64 = null;
            _attachedSnapshotFocusMode = CopilotSdkSnapshotTool.SnapshotFocusMode.WholeScene;

            if (_attachedSnapshotPreview != null)
            {
                DestroyImmediate(_attachedSnapshotPreview);
                _attachedSnapshotPreview = null;
            }

            PersistSessionState();
        }

        private bool IsVisionModelSelected()
        {
            var settings = CopilotSdkSettings.Get();
            return VisionModels.Contains(settings.Model);
        }

        private static string NormalizeSnapshotMode(string mode)
        {
            return string.Equals(mode, "selected_assets", StringComparison.OrdinalIgnoreCase)
                ? "selected_assets"
                : "whole_scene";
        }

        private string BuildAttachedSnapshotLabel()
        {
            var focusName = CopilotSdkSnapshotTool.GetFocusModeDisplayName(_attachedSnapshotFocusMode);
            return $"Scene snapshot attached ({focusName})";
        }

        private async Task StartSessionAsync()
        {
            _isStarting = true;
            _error = "";
            _status = "Starting host...";

            try
            {
                var settings = CopilotSdkSettings.Get();
                if (string.IsNullOrWhiteSpace(settings.HostExecutablePath))
                {
                    var defaultPath = GetDefaultHostPath();
                    settings.HostExecutablePath = defaultPath;
                    settings.Save();
                }

                if (string.IsNullOrWhiteSpace(settings.RepoRoot))
                {
                    settings.RepoRoot = GetProjectRoot();
                    settings.Save();
                }

                if (!await EnsureHostExecutableAsync(settings))
                {
                    return;
                }

                await _client.StartAsync(settings);
                _status = "Host started.";
                PersistSessionState();
            }
            catch (Exception ex)
            {
                _error = ex.Message;
                _status = "Host failed to start.";
                Debug.LogException(ex);
                PersistSessionState();
            }
            finally
            {
                _isStarting = false;
                Repaint();
            }
        }

        private async Task<bool> EnsureHostExecutableAsync(CopilotSdkSettings settings)
        {
            if (settings == null || string.IsNullOrWhiteSpace(settings.HostExecutablePath))
            {
                _error = "Host executable path is not configured.";
                _status = "Host failed to start.";
                return false;
            }

            if (File.Exists(settings.HostExecutablePath))
            {
                return true;
            }

            var repoRoot = string.IsNullOrWhiteSpace(settings.RepoRoot) || !Directory.Exists(settings.RepoRoot)
                ? GetProjectRoot()
                : settings.RepoRoot;
            var buildScript = Path.Combine(repoRoot, "build.ps1");
            if (!File.Exists(buildScript))
            {
                _error = $"Copilot host executable was not found and build script is missing at '{buildScript}'.";
                _status = "Host build script missing.";
                return false;
            }

            _status = "Host executable missing. Building...";
            PersistSessionState();
            Repaint();

            var stdout = "";
            var stderr = "";
            var buildError = "";
            var buildSucceeded = await Task.Run(() =>
                RunBuildScript(buildScript, repoRoot, out stdout, out stderr, out buildError));
            if (!buildSucceeded)
            {
                _error = string.IsNullOrWhiteSpace(buildError)
                    ? "Copilot host build failed. See console for details."
                    : buildError;
                _status = "Host build failed.";
                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    Debug.Log(stdout);
                }
                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    Debug.LogError(stderr);
                }
                return false;
            }

            if (!File.Exists(settings.HostExecutablePath))
            {
                var defaultPath = GetDefaultHostPath();
                if (File.Exists(defaultPath))
                {
                    settings.HostExecutablePath = defaultPath;
                    settings.Save();
                }
            }

            if (!File.Exists(settings.HostExecutablePath))
            {
                _error = "Copilot host executable was not found after build.";
                _status = "Host failed to start.";
                return false;
            }

            return true;
        }

        private static bool RunBuildScript(
            string scriptPath,
            string workingDirectory,
            out string stdout,
            out string stderr,
            out string buildError)
        {
            var isWindows = Application.platform == RuntimePlatform.WindowsEditor;
            var shell = isWindows ? "powershell" : "pwsh";
            var args = isWindows
                ? $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\""
                : $"-NoProfile -File \"{scriptPath}\"";

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = shell,
                Arguments = args,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            try
            {
                using var process = System.Diagnostics.Process.Start(startInfo);
                if (process == null)
                {
                    stdout = "";
                    stderr = "";
                    buildError = "Failed to start the build process.";
                    return false;
                }

                var stdoutTask = process.StandardOutput.ReadToEndAsync();
                var stderrTask = process.StandardError.ReadToEndAsync();
                process.WaitForExit();
                stdout = stdoutTask.GetAwaiter().GetResult();
                stderr = stderrTask.GetAwaiter().GetResult();
                buildError = "";
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                stdout = "";
                stderr = "";
                buildError = $"Failed to run build script: {ex.Message}";
                return false;
            }
        }

        private async Task StopSessionAsync()
        {
            _error = "";
            _status = "Stopping host...";

            try
            {
                await _client.StopAsync();
                _status = "Host stopped.";
                PersistSessionState();
            }
            catch (Exception ex)
            {
                _error = ex.Message;
                _status = "Host failed to stop.";
                Debug.LogException(ex);
                PersistSessionState();
            }
            finally
            {
                Repaint();
            }
        }

        private async Task StartNewChatAsync()
        {
            ResetConversationState();

            if (_client == null)
            {
                _error = "Copilot host is not initialized.";
                return;
            }

            if (_isStarting)
            {
                while (_isStarting)
                {
                    await Task.Delay(50);
                }
            }

            if (_client.IsRunning)
            {
                await StopSessionAsync();
                await StartSessionAsync();
            }
        }

        private async void SendPrompt()
        {
            if (string.IsNullOrWhiteSpace(_prompt))
            {
                return;
            }

            var rawPrompt = _prompt.Trim();
            if (!await TryAutoSwitchPlanToAgentAsync())
            {
                return;
            }

            await EnsureHostRunningAsync();
            if (_client == null || !_client.IsRunning)
            {
                _error = string.IsNullOrWhiteSpace(_error) ? "Host is not running." : _error;
                return;
            }

            var prompt = rawPrompt;
            var imageBase64 = _attachedSnapshotBase64;

            _prompt = "";
            _error = "";
            _thinking = "";
            _isAwaitingResponse = true;
            _responseInterruptedByReload = false;

            var selectionContext = BuildSelectionContext();
            if (!string.IsNullOrWhiteSpace(selectionContext))
            {
                prompt = $"{selectionContext}\n\n{prompt}";
            }

            if (_switchedPlanToAgentThisSend && LooksLikeProceedInstruction(rawPrompt))
            {
                prompt = BuildPlanToAgentProceedPrompt(rawPrompt);
            }

            _lastSubmittedPrompt = prompt;
            _lastSubmittedImageBase64 = imageBase64 ?? "";

            // Show that image was attached in the message
            var displayContent = imageBase64 != null
                ? $"[{BuildAttachedSnapshotLabel()}]\n{prompt}"
                : prompt;
            _messages.Add(new ChatMessage
            {
                Role = "You",
                Content = displayContent,
                ImageBase64 = imageBase64 ?? "",
                ImagePreview = CreateTextureFromBase64(imageBase64),
                ImageExpanded = false
            });
            _pendingAssistantIndex = -1;
            RequestScrollToBottom();
            PersistSessionState();

            try
            {
                if (imageBase64 != null)
                {
                    await _client.SendPromptWithImageAsync(prompt, imageBase64);
                }
                else
                {
                    await _client.SendPromptAsync(prompt);
                }
            }
            catch (Exception ex)
            {
                _error = ex.Message;
                Debug.LogException(ex);
                PersistSessionState();
            }

            Repaint();
        }

        private async Task<bool> TryAutoSwitchPlanToAgentAsync()
        {
            _switchedPlanToAgentThisSend = false;
            var settings = CopilotSdkSettings.Get();
            if (!string.Equals(settings.InteractionMode, "plan", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!LooksLikeProceedInstruction(_prompt))
            {
                return true;
            }

            settings.InteractionMode = "agent";
            settings.Save();
            _status = "Switching mode from Plan to Agent.";
            _error = "";
            PersistSessionState();

            try
            {
                if (_client != null && _client.IsRunning)
                {
                    await StopSessionAsync();
                }

                await StartSessionAsync();
                _switchedPlanToAgentThisSend = _client != null && _client.IsRunning;
                return _switchedPlanToAgentThisSend;
            }
            catch (Exception ex)
            {
                _error = ex.Message;
                Debug.LogException(ex);
                PersistSessionState();
                return false;
            }
        }

        private static bool LooksLikeProceedInstruction(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return false;
            }

            var normalized = prompt.Trim().ToLowerInvariant();
            if (normalized == "proceed"
                || normalized == "continue"
                || normalized == "go ahead"
                || normalized == "do it"
                || normalized == "implement")
            {
                return true;
            }

            return normalized.StartsWith("proceed ", StringComparison.Ordinal)
                || normalized.StartsWith("continue ", StringComparison.Ordinal)
                || normalized.StartsWith("go ahead ", StringComparison.Ordinal)
                || normalized.StartsWith("implement ", StringComparison.Ordinal)
                || normalized.StartsWith("do it ", StringComparison.Ordinal);
        }

        private string BuildPlanToAgentProceedPrompt(string originalPrompt)
        {
            var context = BuildRecentConversationContextForHandoff();
            if (string.IsNullOrWhiteSpace(context))
            {
                return originalPrompt;
            }

            return "Context from the prior planning conversation:\n"
                   + context
                   + "\n\nUser instruction after switching to agent mode: "
                   + originalPrompt
                   + "\n\nProceed with implementation using that context.";
        }

        private string BuildRecentConversationContextForHandoff()
        {
            const int maxMessages = 8;
            const int maxCharsPerMessage = 600;
            var entries = new List<string>();

            for (var i = _messages.Count - 1; i >= 0 && entries.Count < maxMessages; i--)
            {
                var message = _messages[i];
                if (message == null || message.IsToolCall || string.IsNullOrWhiteSpace(message.Content))
                {
                    continue;
                }

                if (!string.Equals(message.Role, "You", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(message.Role, "Copilot", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var content = message.Content.Trim();
                if (content.Length > maxCharsPerMessage)
                {
                    content = content.Substring(0, maxCharsPerMessage) + "...";
                }

                var role = string.Equals(message.Role, "You", StringComparison.OrdinalIgnoreCase) ? "User" : "Assistant";
                entries.Add($"{role}: {content}");
            }

            if (entries.Count == 0)
            {
                return "";
            }

            entries.Reverse();
            var builder = new StringBuilder();
            for (var i = 0; i < entries.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append('\n');
                }

                builder.Append(entries[i]);
            }

            return builder.ToString();
        }

        private void AppendToolCall(string toolName, string toolPayload, string toolResultPayload)
        {
            // Split assistant streaming into segments around tool calls so bubbles
            // appear in chronological order at the point each tool was requested.
            _pendingAssistantIndex = -1;

            var imageBase64 = TryExtractCaptureSnapshotImageBase64(toolName, toolResultPayload);
            _messages.Add(new ChatMessage
            {
                Role = "Tool",
                Content = BuildToolCallSummary(toolName, toolPayload, toolResultPayload),
                ImageBase64 = imageBase64 ?? "",
                ImagePreview = CreateTextureFromBase64(imageBase64),
                IsToolCall = true,
                ToolName = toolName ?? "",
                ToolPayload = toolPayload ?? "",
                ToolResultPayload = toolResultPayload ?? ""
            });

            RequestScrollToBottom();
            PersistSessionState();
            Repaint();
        }

        private void AppendAssistantDelta(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (!CanAppendToPendingAssistantMessage())
            {
                _pendingAssistantIndex = -1;
            }

            if (_pendingAssistantIndex < 0)
            {
                _messages.Add(new ChatMessage { Role = "Copilot", Content = "" });
                _pendingAssistantIndex = _messages.Count - 1;
            }

            _messages[_pendingAssistantIndex].Content += text;
            RequestScrollToBottom();
            PersistSessionState();
            Repaint();
        }

        private void CompleteAssistant()
        {
            _pendingAssistantIndex = -1;
            _isAwaitingResponse = false;

            // Auto-clear attached snapshot after response completes
            ClearAttachedSnapshot();
            PersistSessionState();

            Repaint();
        }
        
        private void AppendAssistantMessage(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (!CanAppendToPendingAssistantMessage())
            {
                _pendingAssistantIndex = -1;
            }

            if (_pendingAssistantIndex < 0)
            {
                _messages.Add(new ChatMessage { Role = "Copilot", Content = "" });
                _pendingAssistantIndex = _messages.Count - 1;
            }

            _messages[_pendingAssistantIndex].Content += text;
            RequestScrollToBottom();
            PersistSessionState();
            CompleteAssistant();
        }

        private bool CanAppendToPendingAssistantMessage()
        {
            if (_pendingAssistantIndex < 0 || _pendingAssistantIndex >= _messages.Count)
            {
                return false;
            }

            if (_pendingAssistantIndex != _messages.Count - 1)
            {
                return false;
            }

            var pending = _messages[_pendingAssistantIndex];
            if (pending == null || pending.IsToolCall)
            {
                return false;
            }

            return string.Equals(pending.Role, "Copilot", StringComparison.OrdinalIgnoreCase);
        }

        private void AppendThinkingDelta(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            _thinking += text;
            PersistSessionState();
            Repaint();
        }

        private void HandlePromptSubmit()
        {
            var currentEvent = Event.current;
            if (currentEvent.type != EventType.KeyDown)
            {
                return;
            }

            if (currentEvent.keyCode != KeyCode.Return && currentEvent.keyCode != KeyCode.KeypadEnter)
            {
                return;
            }

            if (currentEvent.shift)
            {
                return;
            }

            if (!string.Equals(GUI.GetNameOfFocusedControl(), PromptControlName, StringComparison.Ordinal))
            {
                return;
            }

            currentEvent.Use();
            SendPrompt();
        }

        private void RequestScrollToBottom()
        {
            _scrollToBottom = true;
        }

        private void ResetConversationState()
        {
            DestroyMessageTextures();
            _messages.Clear();
            _pendingAssistantIndex = -1;
            _prompt = "";
            _thinking = "";
            _error = "";
            _isAwaitingResponse = false;
            ClearAttachedSnapshot();
            RequestScrollToBottom();
            PersistSessionState();
        }

        private void ExportConversationToMarkdown()
        {
            if (_messages.Count == 0)
            {
                EditorUtility.DisplayDialog("Export Chat", "No messages to export.", "OK");
                return;
            }

            var defaultName = $"CopilotChat_{DateTime.Now:yyyy-MM-dd_HHmmss}.md";
            var exportDir = GetExportDirectory();
            var path = EditorUtility.SaveFilePanel("Export Chat to Markdown", exportDir, defaultName, "md");

            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                var settings = CopilotSdkSettings.Get();
                var sb = new StringBuilder();
                var exportRoot = Path.GetDirectoryName(path) ?? exportDir;
                var imageDirName = $"{Path.GetFileNameWithoutExtension(path)}_images";
                var imageDir = Path.Combine(exportRoot, imageDirName);
                var imageIndex = 0;
                void AppendSnapshotImage(string base64)
                {
                    if (string.IsNullOrWhiteSpace(base64))
                    {
                        return;
                    }

                    imageIndex++;
                    if (!Directory.Exists(imageDir))
                    {
                        Directory.CreateDirectory(imageDir);
                    }

                    var imageFileName = $"snapshot_{imageIndex:000}.png";
                    var imagePath = Path.Combine(imageDir, imageFileName);
                    if (TryWriteBase64Image(base64, imagePath, out var imageError))
                    {
                        var relativePath = Path.GetRelativePath(exportRoot, imagePath)
                            .Replace("\\", "/");
                        sb.AppendLine($"![Snapshot {imageIndex}]({relativePath})");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to export snapshot image: {imageError}");
                        sb.AppendLine("![Snapshot](image export failed)");
                    }
                    sb.AppendLine();
                }
                sb.AppendLine("# Copilot Chat Export");
                sb.AppendLine();
                sb.AppendLine($"**Date:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}  ");
                sb.AppendLine($"**Model:** {settings.Model}  ");
                sb.AppendLine($"**Mode:** {settings.InteractionMode}  ");
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();

                foreach (var message in _messages)
                {
                    if (message.IsToolCall)
                    {
                        sb.AppendLine($"### 🔧 Tool: {message.ToolName}");
                        sb.AppendLine();
                        AppendSnapshotImage(message.ImageBase64);
                        sb.AppendLine("<details>");
                        sb.AppendLine("<summary>Tool Details</summary>");
                        sb.AppendLine();
                        if (!string.IsNullOrWhiteSpace(message.ToolPayload))
                        {
                            sb.AppendLine("**Input:**");
                            sb.AppendLine("```json");
                            sb.AppendLine(message.ToolPayload);
                            sb.AppendLine("```");
                            sb.AppendLine();
                        }
                        if (!string.IsNullOrWhiteSpace(message.ToolResultPayload))
                        {
                            sb.AppendLine("**Result:**");
                            sb.AppendLine("```json");
                            sb.AppendLine(message.ToolResultPayload);
                            sb.AppendLine("```");
                        }
                        sb.AppendLine();
                        sb.AppendLine("</details>");
                        sb.AppendLine();
                    }
                    else
                    {
                        var roleEmoji = string.Equals(message.Role, "You", StringComparison.OrdinalIgnoreCase) ? "👤" : "🤖";
                        sb.AppendLine($"## {roleEmoji} {message.Role}");
                        sb.AppendLine();

                        AppendSnapshotImage(message.ImageBase64);

                        if (!string.IsNullOrWhiteSpace(message.Content))
                        {
                            sb.AppendLine(message.Content);
                            sb.AppendLine();
                        }
                    }

                    sb.AppendLine("---");
                    sb.AppendLine();
                }

                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
                _status = $"Chat exported to {Path.GetFileName(path)}";

                if (EditorUtility.DisplayDialog("Export Complete", $"Chat exported to:\n{path}\n\nOpen containing folder?", "Open Folder", "Close"))
                {
                    EditorUtility.RevealInFinder(path);
                }
            }
            catch (Exception ex)
            {
                _error = $"Export failed: {ex.Message}";
                Debug.LogException(ex);
            }
        }

        private static bool TryWriteBase64Image(string base64, string path, out string error)
        {
            if (string.IsNullOrWhiteSpace(base64))
            {
                error = "No image data provided.";
                return false;
            }

            var trimmed = base64.Trim();
            var markerIndex = trimmed.IndexOf("base64,", StringComparison.OrdinalIgnoreCase);
            if (markerIndex >= 0)
            {
                trimmed = trimmed.Substring(markerIndex + "base64,".Length);
            }

            try
            {
                var bytes = Convert.FromBase64String(trimmed);
                File.WriteAllBytes(path, bytes);
                error = "";
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static string GetExportDirectory()
        {
            var exportDir = Path.Combine(Application.dataPath, "..", "ChatExports");
            if (!Directory.Exists(exportDir))
            {
                Directory.CreateDirectory(exportDir);
            }
            return Path.GetFullPath(exportDir);
        }

        private void HandleBeforeAssemblyReload()
        {
            PersistSessionState(markResponseInterrupted: _isAwaitingResponse);
        }

        private void PersistSessionState(bool markResponseInterrupted = false)
        {
            var state = CopilotSdkSessionState.Get();
            state.Messages.Clear();

            for (var i = 0; i < _messages.Count; i++)
            {
                var message = _messages[i];
                state.Messages.Add(new CopilotSdkSessionState.MessageEntry
                {
                    Role = message.Role ?? "",
                    Content = message.Content ?? "",
                    ImageBase64 = message.ImageBase64 ?? "",
                    ImageExpanded = message.ImageExpanded,
                    IsToolCall = message.IsToolCall,
                    ToolName = message.ToolName ?? "",
                    ToolPayload = message.ToolPayload ?? "",
                    ToolResultPayload = message.ToolResultPayload ?? "",
                    ToolDetailsExpanded = message.ToolDetailsExpanded
                });
            }

            state.Prompt = _prompt ?? "";
            state.Thinking = _thinking ?? "";
            state.Error = _error ?? "";
            state.Status = _status ?? "";
            state.WasAwaitingResponse = _isAwaitingResponse;
            state.HostWasRunning = _client != null && _client.IsRunning;
            state.ResponseInterrupted = markResponseInterrupted || _responseInterruptedByReload;
            state.LastSubmittedPrompt = _lastSubmittedPrompt ?? "";
            state.LastSubmittedImageBase64 = _lastSubmittedImageBase64 ?? "";
            state.AttachedSnapshotBase64 = _attachedSnapshotBase64 ?? "";
            state.AttachedSnapshotFocusMode = CopilotSdkSnapshotTool.GetFocusModeKey(_attachedSnapshotFocusMode);
            state.SaveState();
        }

        private void RestoreSessionState()
        {
            var state = CopilotSdkSessionState.Get();
            DestroyMessageTextures();
            _messages.Clear();

            if (state.Messages != null)
            {
                for (var i = 0; i < state.Messages.Count; i++)
                {
                    var saved = state.Messages[i];
                    if (saved == null)
                    {
                        continue;
                    }

                    _messages.Add(new ChatMessage
                    {
                        Role = saved.Role ?? "Copilot",
                        Content = saved.Content ?? "",
                        ImageBase64 = saved.ImageBase64 ?? "",
                        ImagePreview = CreateTextureFromBase64(saved.ImageBase64),
                        ImageExpanded = saved.ImageExpanded,
                        IsToolCall = saved.IsToolCall,
                        ToolName = saved.ToolName ?? "",
                        ToolPayload = saved.ToolPayload ?? "",
                        ToolResultPayload = saved.ToolResultPayload ?? "",
                        ToolDetailsExpanded = saved.ToolDetailsExpanded
                    });
                }
            }

            _pendingAssistantIndex = -1;
            _prompt = state.Prompt ?? "";
            _thinking = state.Thinking ?? "";
            _error = state.Error ?? "";
            _status = string.IsNullOrWhiteSpace(state.Status) ? _status : state.Status;
            _lastSubmittedPrompt = state.LastSubmittedPrompt ?? "";
            _lastSubmittedImageBase64 = state.LastSubmittedImageBase64 ?? "";
            _attachedSnapshotBase64 = state.AttachedSnapshotBase64 ?? "";
            _attachedSnapshotFocusMode = CopilotSdkSnapshotTool.ParseFocusMode(state.AttachedSnapshotFocusMode);
            RestoreAttachedSnapshotPreviewFromState();

            _isAwaitingResponse = false;
            _responseInterruptedByReload = state.ResponseInterrupted || state.WasAwaitingResponse;
            if (_responseInterruptedByReload)
            {
                _status = "Session restored after script compile.";
            }

            RequestScrollToBottom();
        }

        private void RestoreAttachedSnapshotPreviewFromState()
        {
            if (_attachedSnapshotPreview != null)
            {
                DestroyImmediate(_attachedSnapshotPreview);
                _attachedSnapshotPreview = null;
            }

            if (string.IsNullOrWhiteSpace(_attachedSnapshotBase64))
            {
                return;
            }

            try
            {
                var bytes = Convert.FromBase64String(_attachedSnapshotBase64);
                _attachedSnapshotPreview = CreateTextureFromBytes(bytes);
            }
            catch
            {
                _attachedSnapshotPreview = null;
            }
        }

        private void DestroyMessageTextures()
        {
            for (var i = 0; i < _messages.Count; i++)
            {
                var preview = _messages[i].ImagePreview;
                if (preview != null)
                {
                    DestroyImmediate(preview);
                    _messages[i].ImagePreview = null;
                }
            }
        }

        private static Texture2D CreateTextureFromBase64(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
            {
                return null;
            }

            try
            {
                var bytes = Convert.FromBase64String(base64);
                return CreateTextureFromBytes(bytes);
            }
            catch
            {
                return null;
            }
        }

        private static Texture2D CreateTextureFromBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            var texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
            if (!texture.LoadImage(bytes, markNonReadable: false))
            {
                DestroyImmediate(texture);
                return null;
            }

            texture.hideFlags = HideFlags.HideAndDontSave;
            return texture;
        }

        private async void RetryInterruptedPrompt()
        {
            if (string.IsNullOrWhiteSpace(_lastSubmittedPrompt))
            {
                return;
            }

            await EnsureHostRunningAsync();
            if (_client == null || !_client.IsRunning)
            {
                _error = string.IsNullOrWhiteSpace(_error) ? "Host is not running." : _error;
                PersistSessionState();
                return;
            }

            _error = "";
            _thinking = "";
            _isAwaitingResponse = true;
            _responseInterruptedByReload = false;
            _pendingAssistantIndex = -1;
            PersistSessionState();

            try
            {
                if (!string.IsNullOrWhiteSpace(_lastSubmittedImageBase64))
                {
                    await _client.SendPromptWithImageAsync(_lastSubmittedPrompt, _lastSubmittedImageBase64);
                }
                else
                {
                    await _client.SendPromptAsync(_lastSubmittedPrompt);
                }
            }
            catch (Exception ex)
            {
                _error = ex.Message;
                Debug.LogException(ex);
                PersistSessionState();
            }
        }

        private static string BuildSelectionContext()
        {
            var active = Selection.activeGameObject;
            if (active == null)
            {
                return "";
            }

            var path = BuildHierarchyPath(active.transform);
            var sceneName = active.scene.IsValid() ? active.scene.name : "";
            var selectionCount = Selection.gameObjects?.Length ?? 0;
            var countSuffix = selectionCount > 1 ? $" (and {selectionCount - 1} more selected)" : "";
            return $"Selected GameObject: {path} (scene: {sceneName}, instanceId: {active.GetInstanceID()}){countSuffix}";
        }

        private static string BuildToolCallLabel(string toolName)
        {
            var display = toolName switch
            {
                "create_game_object" => "Create GameObject",
                "add_game_object" => "Add GameObject",
                "update_game_object" => "Update GameObject",
                "delete_game_object" => "Delete GameObject",
                "add_component" => "Add Component",
                "remove_component" => "Remove Component",
                "set_component_properties" => "Set Component Properties",
                "list_game_objects" => "List GameObjects",
                "list_components" => "List Components",
                "capture_scene_snapshot" => "Capture Scene Snapshot",
                _ => toolName ?? "Tool Call"
            };

            return display;
        }

        private static string BuildToolCallSummary(string toolName, string toolPayload, string toolResultPayload)
        {
            var label = BuildToolCallLabel(toolName);
            if (string.Equals(toolName, "list_game_objects", StringComparison.OrdinalIgnoreCase))
            {
                var details = TryFormatListGameObjectsPayload(toolPayload);
                return string.IsNullOrWhiteSpace(details) ? label : $"{label}\n{details}";
            }

            return label;
        }

        private static string GetToolIcon(string toolName)
        {
            return toolName switch
            {
                "create_game_object" => "📦",
                "add_game_object" => "📦",
                "update_game_object" => "🛠",
                "delete_game_object" => "🗑",
                "add_component" => "🧩",
                "remove_component" => "➖",
                "set_component_properties" => "⚙",
                "list_game_objects" => "🔎",
                "list_components" => "📋",
                "capture_scene_snapshot" => "📷",
                _ => "🔧"
            };
        }

        [Serializable]
        private sealed class ListGameObjectsPayload
        {
            public string nameContains;
            public string sceneName;
            public bool includeInactive = true;
            public int maxResults = 50;
        }

        private static string TryFormatListGameObjectsPayload(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
            {
                return "";
            }

            try
            {
                var parsed = JsonUtility.FromJson<ListGameObjectsPayload>(payload);
                if (parsed == null)
                {
                    return "";
                }

                var name = string.IsNullOrWhiteSpace(parsed.nameContains) ? "(all)" : parsed.nameContains;
                var scene = string.IsNullOrWhiteSpace(parsed.sceneName) ? "(active scene)" : parsed.sceneName;
                return $"query: {name}, scene: {scene}, includeInactive: {parsed.includeInactive}, maxResults: {parsed.maxResults}";
            }
            catch
            {
                return "";
            }
        }

        [Serializable]
        private sealed class CaptureSceneSnapshotResultPayload
        {
            public bool captured;
            public string path;
            public string displayName;
        }

        private static string FormatToolPayloadForDisplay(string toolName, string requestPayload, string resultPayload)
        {
            var text = "";
            if (!string.IsNullOrWhiteSpace(requestPayload))
            {
                text += $"Request:\n{requestPayload}";
            }

            if (!string.IsNullOrWhiteSpace(resultPayload))
            {
                if (text.Length > 0)
                {
                    text += "\n\n";
                }

                text += $"Result:\n{resultPayload}";
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            var maxChars = 2000;
            if (text.Length <= maxChars)
            {
                return text;
            }

            return $"{text.Substring(0, maxChars)}\n... (truncated)";
        }

        private static string TryExtractCaptureSnapshotImageBase64(string toolName, string toolResultPayload)
        {
            if (!string.Equals(toolName, "capture_scene_snapshot", StringComparison.OrdinalIgnoreCase))
            {
                return "";
            }

            if (string.IsNullOrWhiteSpace(toolResultPayload))
            {
                return "";
            }

            try
            {
                var parsed = JsonUtility.FromJson<CaptureSceneSnapshotResultPayload>(toolResultPayload);
                if (parsed == null || !parsed.captured || string.IsNullOrWhiteSpace(parsed.path))
                {
                    return "";
                }

                if (!File.Exists(parsed.path))
                {
                    return "";
                }

                var bytes = File.ReadAllBytes(parsed.path);
                return Convert.ToBase64String(bytes);
            }
            catch
            {
                return "";
            }
        }

        private static string BuildHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return "";
            }

            var segments = new List<string>();
            var current = transform;
            while (current != null)
            {
                segments.Add(current.name);
                current = current.parent;
            }

            segments.Reverse();
            return string.Join("/", segments);
        }

        private async Task EnsureHostRunningAsync()
        {
            if (_client == null)
            {
                _error = "Copilot host is not initialized.";
                return;
            }

            if (_client.IsRunning)
            {
                return;
            }

            if (_isStarting)
            {
                while (_isStarting)
                {
                    await Task.Delay(50);
                }

                return;
            }

            await StartSessionAsync();
        }

        private void UpdateSpinner()
        {
            if (!_isAwaitingResponse)
            {
                return;
            }

            var now = EditorApplication.timeSinceStartup;
            if (now - _lastSpinnerUpdate < 0.08)
            {
                return;
            }

            _lastSpinnerUpdate = now;
            _spinnerIndex = (_spinnerIndex + 1) % 12;
            Repaint();
        }

        private static string GetDefaultHostPath()
        {
            var platform = Application.platform;
            var exeName = platform switch
            {
                RuntimePlatform.WindowsEditor => "CopilotSdkHost.exe",
                RuntimePlatform.OSXEditor => "CopilotSdkHost",
                RuntimePlatform.LinuxEditor => "CopilotSdkHost",
                _ => "CopilotSdkHost"
            };

            var arch = RuntimeInformation.ProcessArchitecture;
            var rid = platform switch
            {
                RuntimePlatform.WindowsEditor => "win-x64",
                RuntimePlatform.OSXEditor => arch == Architecture.Arm64 ? "osx-arm64" : "osx-x64",
                RuntimePlatform.LinuxEditor => "linux-x64",
                _ => "win-x64"
            };

            var path = Path.Combine(
                Application.dataPath,
                "UnityCopilot",
                "Editor",
                "CopilotSdkHost~",
                "publish",
                rid,
                exeName);

            return path;
        }

        private static string GetProjectRoot()
        {
            var dataPath = Application.dataPath;
            var directory = Directory.GetParent(dataPath);
            return directory?.FullName ?? dataPath;
        }

        private static string DrawModePopup(string currentMode)
        {
            var modes = new[] { "ask", "agent", "plan" };
            var labels = new[] { "Ask", "Agent", "Plan" };
            var index = Array.IndexOf(modes, currentMode);
            if (index < 0)
            {
                index = 0;
            }

            index = EditorGUILayout.Popup("Mode", index, labels);
            return modes[index];
        }

        private static string DrawModelPopup(string currentModel)
        {
            var index = 0;
            for (var i = 0; i < ModelOptions.Length; i++)
            {
                if (string.Equals(ModelOptions[i].Id, currentModel, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }

            index = EditorGUILayout.Popup("Model", index, ModelLabels);
            return ModelOptions[index].Id;
        }

        private static string[] BuildModelLabels()
        {
            var labels = new string[ModelOptions.Length];
            for (var i = 0; i < ModelOptions.Length; i++)
            {
                labels[i] = ModelOptions[i].Name;
            }

            return labels;
        }

        private void DrawBubble(ChatMessage message, string icon, bool isUser, GUIStyle bubbleStyle)
        {
            var content = message.Content;
            var image = message.ImagePreview;
            if (string.IsNullOrWhiteSpace(content) && image == null)
            {
                return;
            }

            // Convert markdown to Unity rich text for assistant messages (if enabled)
            var settings = CopilotSdkSettings.Get();
            var displayContent = (!isUser && settings.RenderMarkdown) 
                ? MarkdownToRichText.Convert(content) 
                : content;

            var maxWidth = Mathf.Max(200f, position.width * 0.7f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (isUser)
                {
                    GUILayout.FlexibleSpace();
                }

                using (new EditorGUILayout.VerticalScope(bubbleStyle, GUILayout.MaxWidth(maxWidth)))
                {
                    if (image != null)
                    {
                        var width = message.ImageExpanded
                            ? Mathf.Min(maxWidth - 16f, 720f)
                            : Mathf.Min(maxWidth - 16f, 320f);
                        var aspect = image.height > 0 ? (float)image.width / image.height : 1f;
                        var height = width / Mathf.Max(0.1f, aspect);
                        var rect = GUILayoutUtility.GetRect(width, height, GUILayout.MaxWidth(width), GUILayout.Height(height));
                        GUI.DrawTexture(rect, image, ScaleMode.ScaleToFit);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(message.ImageExpanded ? "Collapse" : "Expand", GUILayout.Width(70)))
                            {
                                message.ImageExpanded = !message.ImageExpanded;
                                PersistSessionState();
                            }
                        }
                        GUILayout.Space(6);
                    }

                    if (!string.IsNullOrWhiteSpace(displayContent))
                    {
                        var contentStyle = new GUIStyle(EditorStyles.label)
                        {
                            wordWrap = true,
                            richText = true
                        };
                        var text = string.IsNullOrWhiteSpace(icon) ? displayContent : $"{icon} {displayContent}";
                        GUILayout.Label(text, contentStyle);
                    }

                    if (message.IsToolCall && !string.IsNullOrWhiteSpace(message.ToolPayload))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(message.ToolDetailsExpanded ? "Hide Details" : "Show Details", GUILayout.Width(95)))
                            {
                                message.ToolDetailsExpanded = !message.ToolDetailsExpanded;
                                PersistSessionState();
                            }
                        }

                        if (message.ToolDetailsExpanded)
                        {
                            var detailText = FormatToolPayloadForDisplay(message.ToolName, message.ToolPayload, message.ToolResultPayload);
                            var detailStyle = new GUIStyle(EditorStyles.textArea)
                            {
                                wordWrap = true
                            };
                            GUILayout.TextArea(detailText, detailStyle, GUILayout.MinHeight(56f), GUILayout.MaxHeight(260f));
                        }
                    }
                }

                if (!isUser)
                {
                    GUILayout.FlexibleSpace();
                }
            }
            GUILayout.Space(6);
        }

        private void EnsureStyles()
        {
            if (_assistantBubbleStyle != null
                && _userBubbleStyle != null
                && _thinkingBubbleStyle != null
                && _toolBubbleStyle != null
                && _assistantBubbleTexture != null
                && _userBubbleTexture != null
                && _thinkingBubbleTexture != null
                && _toolBubbleTexture != null)
            {
                return;
            }

            _assistantBubbleTexture = CreateSolidTexture(new Color(0.18f, 0.18f, 0.18f, 1f));
            _userBubbleTexture = CreateRoundedTexture(12, new Color(0.16f, 0.45f, 0.86f, 1f));
            _thinkingBubbleTexture = CreateSolidTexture(new Color(0.25f, 0.25f, 0.25f, 1f));
            _toolBubbleTexture = CreateSolidTexture(new Color(0.20f, 0.24f, 0.16f, 1f));

            _assistantBubbleStyle = CreateBubbleStyle(_assistantBubbleTexture);
            _userBubbleStyle = CreateBubbleStyle(_userBubbleTexture, rounded: true);
            _thinkingBubbleStyle = CreateBubbleStyle(_thinkingBubbleTexture);
            _toolBubbleStyle = CreateBubbleStyle(_toolBubbleTexture);
        }

        private static GUIStyle CreateBubbleStyle(Texture2D background, bool rounded = false)
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                wordWrap = true,
                richText = true,
                alignment = TextAnchor.UpperLeft,
                padding = new RectOffset(12, 12, 8, 8),
                margin = new RectOffset(4, 4, 2, 2)
            };

            ApplyBackground(style.normal, background, Color.white);
            ApplyBackground(style.hover, background, Color.white);
            ApplyBackground(style.active, background, Color.white);
            ApplyBackground(style.focused, background, Color.white);
            ApplyBackground(style.onNormal, background, Color.white);
            ApplyBackground(style.onHover, background, Color.white);
            ApplyBackground(style.onActive, background, Color.white);
            ApplyBackground(style.onFocused, background, Color.white);

            if (rounded)
            {
                var radius = Mathf.Max(2, background.width / 4);
                style.border = new RectOffset(radius, radius, radius, radius);
            }

            return style;
        }

        private static void ApplyBackground(GUIStyleState state, Texture2D background, Color textColor)
        {
            state.background = background;
            state.textColor = textColor;
        }

        private static Texture2D CreateSolidTexture(Color color)
        {
            var tex = new Texture2D(1, 1);
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateRoundedTexture(int radius, Color color)
        {
            var size = radius * 2 + 2;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.hideFlags = HideFlags.HideAndDontSave;

            var center = new Vector2(radius + 0.5f, radius + 0.5f);
            var maxDist = radius + 0.5f;

            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var dx = x - center.x;
                    var dy = y - center.y;
                    var dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist <= maxDist)
                    {
                        tex.SetPixel(x, y, color);
                    }
                    else
                    {
                        tex.SetPixel(x, y, new Color(color.r, color.g, color.b, 0f));
                    }
                }
            }

            tex.Apply();
            return tex;
        }
    }
}
