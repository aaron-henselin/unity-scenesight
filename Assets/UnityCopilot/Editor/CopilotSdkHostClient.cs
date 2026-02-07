using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace YourCompany.UnityCopilot.Editor
{
    [Serializable]
    internal sealed class CopilotSdkHostRequest
    {
        public string type;
        public string prompt;
        public string model;
        public bool streaming;
        public string cliUrl;
        public string repoRoot;
        public string excludedPaths;
        public int maxFileSizeKb;
        public int maxSearchResults;
        public string mode;
        public int timeoutSeconds;
        public bool toolDebug;
        public string toolId;
        public string toolName;
        public string toolPayload;
        public string imageBase64;
    }

    [Serializable]
    internal sealed class CopilotSdkHostResponse
    {
        public string type;
        public string content;
        public string message;
        public string toolId;
        public string toolName;
        public string toolPayload;
    }

    internal sealed class CopilotSdkHostClient : IDisposable
    {
        private Process _process;
        private StreamWriter _stdin;
        private CancellationTokenSource _cts;
        private Task _stdoutTask;
        private Task _stderrTask;
        private readonly object _writeLock = new();

        public bool IsRunning => _process != null && !_process.HasExited;

        public event Action<string> OnStatus;
        public event Action<string> OnAssistantDelta;
        public event Action<string> OnAssistantMessage;
        public event Action OnAssistantComplete;
        public event Action<string> OnThinkingDelta;
        public event Action<string, string, string> OnToolCall;
        public event Action<string> OnError;

        public async Task StartAsync(CopilotSdkSettings settings)
        {
            if (IsRunning)
            {
                return;
            }

            var startInfo = BuildStartInfo(settings);
            _process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            _process.Start();

            _stdin = _process.StandardInput;
            _cts = new CancellationTokenSource();

            _stdoutTask = Task.Run(() => ReadStdoutAsync(_cts.Token));
            _stderrTask = Task.Run(() => ReadStderrAsync(_cts.Token));

            await SendAsync(new CopilotSdkHostRequest
            {
                type = "start",
                model = settings.Model,
                streaming = settings.Streaming,
                cliUrl = settings.CliUrl,
                repoRoot = settings.RepoRoot,
                excludedPaths = settings.ExcludedPaths,
                maxFileSizeKb = settings.MaxFileSizeKb,
                maxSearchResults = settings.MaxSearchResults,
                mode = settings.InteractionMode,
                timeoutSeconds = settings.TimeoutSeconds,
                toolDebug = settings.ToolDebug
            });
        }

        public Task SendPromptAsync(string prompt)
        {
            return SendAsync(new CopilotSdkHostRequest
            {
                type = "prompt",
                prompt = prompt
            });
        }

        public Task SendPromptWithImageAsync(string prompt, string imageBase64)
        {
            return SendAsync(new CopilotSdkHostRequest
            {
                type = "prompt",
                prompt = prompt,
                imageBase64 = imageBase64
            });
        }

        public async Task StopAsync()
        {
            if (!IsRunning)
            {
                return;
            }

            try
            {
                await SendAsync(new CopilotSdkHostRequest { type = "stop" });
            }
            catch
            {
                // Ignore send errors on shutdown.
            }

            _cts?.Cancel();

            try
            {
                if (!_process.WaitForExit(2000))
                {
                    _process.Kill();
                }
            }
            catch
            {
                // Ignore process kill errors.
            }

            _process?.Dispose();
            _process = null;
            _stdin = null;
        }

        private async Task SendAsync(CopilotSdkHostRequest request)
        {
            if (!IsRunning || _stdin == null)
            {
                throw new InvalidOperationException("Copilot SDK host is not running.");
            }

            var json = JsonUtility.ToJson(request);
            lock (_writeLock)
            {
                _stdin.WriteLine(json);
                _stdin.Flush();
            }

            await Task.CompletedTask;
        }

        private async Task ReadStdoutAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _process != null && !_process.HasExited)
            {
                var line = await _process.StandardOutput.ReadLineAsync();
                if (line == null)
                {
                    break;
                }

                CopilotSdkHostResponse message = null;
                try
                {
                    message = JsonUtility.FromJson<CopilotSdkHostResponse>(line);
                }
                catch (Exception ex)
                {
                    CopilotSdkDispatcher.Post(() => OnError?.Invoke(ex.Message));
                }

                if (message == null || string.IsNullOrEmpty(message.type))
                {
                    continue;
                }

                CopilotSdkDispatcher.Post(() => HandleMessage(message));
            }
        }

        private async Task ReadStderrAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested && _process != null && !_process.HasExited)
            {
                var line = await _process.StandardError.ReadLineAsync();
                if (line == null)
                {
                    break;
                }

                var captured = line;
                CopilotSdkDispatcher.Post(() =>
                {
                    if (IsToolLog(captured))
                    {
                        UnityEngine.Debug.Log(captured);
                    }
                    else
                    {
                        OnError?.Invoke(captured);
                    }
                });
            }
        }

        private void HandleMessage(CopilotSdkHostResponse message)
        {
            switch (message.type)
            {
                case "status":
                    OnStatus?.Invoke(message.message ?? message.content ?? "");
                    break;
                case "delta":
                    if (!string.IsNullOrEmpty(message.content))
                    {
                        OnAssistantDelta?.Invoke(message.content);
                    }
                    break;
                case "message":
                    if (!string.IsNullOrEmpty(message.content))
                    {
                        OnAssistantMessage?.Invoke(message.content);
                    }
                    OnAssistantComplete?.Invoke();
                    break;
                case "thinking_delta":
                    if (!string.IsNullOrEmpty(message.content))
                    {
                        OnThinkingDelta?.Invoke(message.content);
                    }
                    break;
                case "thinking_message":
                    if (!string.IsNullOrEmpty(message.content))
                    {
                        OnThinkingDelta?.Invoke(message.content);
                    }
                    break;
                case "done":
                    OnAssistantComplete?.Invoke();
                    break;
                case "tool_request":
                    HandleToolRequest(message);
                    break;
                case "error":
                    OnError?.Invoke(message.message ?? "Unknown error from Copilot SDK host.");
                    break;
            }
        }

        private void HandleToolRequest(CopilotSdkHostResponse message)
        {
            if (string.IsNullOrEmpty(message.toolId) || string.IsNullOrEmpty(message.toolName))
            {
                OnError?.Invoke("Tool request missing toolId or toolName.");
                return;
            }

            try
            {
                switch (message.toolName)
                {
                    case "list_game_objects":
                        var args = string.IsNullOrWhiteSpace(message.toolPayload)
                            ? new CopilotSdkSceneTools.FindGameObjectsArgs()
                            : JsonUtility.FromJson<CopilotSdkSceneTools.FindGameObjectsArgs>(message.toolPayload);
                        var result = CopilotSdkSceneTools.FindGameObjects(args);
                        var payload = JsonUtility.ToJson(result);
                        SendToolResult(message.toolId, message.toolName, message.toolPayload, payload);
                        break;
                    case "create_game_object":
                        var createArgs = string.IsNullOrWhiteSpace(message.toolPayload)
                            ? new CopilotSdkSceneTools.CreateGameObjectArgs()
                            : JsonUtility.FromJson<CopilotSdkSceneTools.CreateGameObjectArgs>(message.toolPayload);
                        var createResult = CopilotSdkSceneTools.CreateGameObject(createArgs);
                        var createPayload = JsonUtility.ToJson(createResult);
                        SendToolResult(message.toolId, message.toolName, message.toolPayload, createPayload);
                        break;
                    case "update_game_object":
                        var updateArgs = string.IsNullOrWhiteSpace(message.toolPayload)
                            ? new CopilotSdkSceneTools.UpdateGameObjectArgs()
                            : JsonUtility.FromJson<CopilotSdkSceneTools.UpdateGameObjectArgs>(message.toolPayload);
                        var updateResult = CopilotSdkSceneTools.UpdateGameObject(updateArgs);
                        var updatePayload = JsonUtility.ToJson(updateResult);
                        SendToolResult(message.toolId, message.toolName, message.toolPayload, updatePayload);
                        break;
                    case "delete_game_object":
                        var deleteArgs = string.IsNullOrWhiteSpace(message.toolPayload)
                            ? new CopilotSdkSceneTools.DeleteGameObjectArgs()
                            : JsonUtility.FromJson<CopilotSdkSceneTools.DeleteGameObjectArgs>(message.toolPayload);
                        var deleteResult = CopilotSdkSceneTools.DeleteGameObject(deleteArgs);
                        var deletePayload = JsonUtility.ToJson(deleteResult);
                        SendToolResult(message.toolId, message.toolName, message.toolPayload, deletePayload);
                        break;
                    case "add_component":
                        var addArgs = string.IsNullOrWhiteSpace(message.toolPayload)
                            ? new CopilotSdkSceneTools.AddComponentArgs()
                            : JsonUtility.FromJson<CopilotSdkSceneTools.AddComponentArgs>(message.toolPayload);
                        var addResult = CopilotSdkSceneTools.AddComponent(addArgs);
                        var addPayload = JsonUtility.ToJson(addResult);
                        SendToolResult(message.toolId, message.toolName, message.toolPayload, addPayload);
                        break;
                    case "remove_component":
                        var removeArgs = string.IsNullOrWhiteSpace(message.toolPayload)
                            ? new CopilotSdkSceneTools.RemoveComponentArgs()
                            : JsonUtility.FromJson<CopilotSdkSceneTools.RemoveComponentArgs>(message.toolPayload);
                        var removeResult = CopilotSdkSceneTools.RemoveComponent(removeArgs);
                        var removePayload = JsonUtility.ToJson(removeResult);
                        SendToolResult(message.toolId, message.toolName, message.toolPayload, removePayload);
                        break;
                    case "set_component_properties":
                        var setArgs = string.IsNullOrWhiteSpace(message.toolPayload)
                            ? new CopilotSdkSceneTools.SetComponentPropertiesArgs()
                            : JsonUtility.FromJson<CopilotSdkSceneTools.SetComponentPropertiesArgs>(message.toolPayload);
                        var setResult = CopilotSdkSceneTools.SetComponentProperties(setArgs);
                        var setPayload = JsonUtility.ToJson(setResult);
                        SendToolResult(message.toolId, message.toolName, message.toolPayload, setPayload);
                        break;
                    case "list_components":
                        var listArgs = string.IsNullOrWhiteSpace(message.toolPayload)
                            ? new CopilotSdkSceneTools.ListComponentsArgs()
                            : JsonUtility.FromJson<CopilotSdkSceneTools.ListComponentsArgs>(message.toolPayload);
                        var listResult = CopilotSdkSceneTools.ListComponents(listArgs);
                        var listPayload = JsonUtility.ToJson(listResult);
                        SendToolResult(message.toolId, message.toolName, message.toolPayload, listPayload);
                        break;
                    case "capture_scene_snapshot":
                        var captureArgs = string.IsNullOrWhiteSpace(message.toolPayload)
                            ? new CopilotSdkSceneTools.CaptureSceneSnapshotArgs()
                            : JsonUtility.FromJson<CopilotSdkSceneTools.CaptureSceneSnapshotArgs>(message.toolPayload);
                        var captureResult = CopilotSdkSceneTools.CaptureSceneSnapshot(captureArgs);
                        var capturePayload = JsonUtility.ToJson(captureResult);
                        SendToolResult(message.toolId, message.toolName, message.toolPayload, capturePayload);
                        break;
                    default:
                        var unknownPayload = JsonUtility.ToJson(new CopilotSdkSceneTools.ToolError
                        {
                            error = $"Unknown tool '{message.toolName}'."
                        });
                        SendToolResult(message.toolId, message.toolName, message.toolPayload, unknownPayload);
                        break;
                }
            }
            catch (Exception ex)
            {
                var errorPayload = JsonUtility.ToJson(new CopilotSdkSceneTools.ToolError
                {
                    error = ex.Message
                });
                SendToolResult(message.toolId, message.toolName, message.toolPayload, errorPayload);
            }
        }

        private void SendToolResult(string toolId, string toolName, string requestPayload, string resultPayload)
        {
            OnToolCall?.Invoke(toolName ?? "", requestPayload ?? "", resultPayload ?? "");
            _ = SendAsync(new CopilotSdkHostRequest
            {
                type = "tool_result",
                toolId = toolId,
                toolPayload = resultPayload
            });
        }

        private static ProcessStartInfo BuildStartInfo(CopilotSdkSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.HostExecutablePath))
            {
                throw new InvalidOperationException("Host executable path is not configured.");
            }

            var workingDirectory = Path.GetDirectoryName(settings.HostExecutablePath);
            if (!string.IsNullOrWhiteSpace(settings.RepoRoot) && Directory.Exists(settings.RepoRoot))
            {
                workingDirectory = settings.RepoRoot;
            }

            return new ProcessStartInfo
            {
                FileName = settings.HostExecutablePath,
                Arguments = settings.HostArguments ?? "",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory
            };
        }

        private static bool IsToolLog(string message)
        {
            return !string.IsNullOrEmpty(message)
                   && message.StartsWith("[CopilotSdkHost][Tool]", StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            try
            {
                StopAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogException(ex);
            }
        }
    }
}
