using System;
using System.Collections.Generic;
using UnityEditor;

namespace YourCompany.UnityCopilot.Editor
{
    [FilePath("UserSettings/UnityCopilotSessionState.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class CopilotSdkSessionState : ScriptableSingleton<CopilotSdkSessionState>
    {
        [Serializable]
        internal sealed class MessageEntry
        {
            public string Role;
            public string Content;
            public string ImageBase64;
            public bool ImageExpanded;
            public bool IsToolCall;
            public string ToolName;
            public string ToolPayload;
            public string ToolResultPayload;
            public bool ToolDetailsExpanded;
        }

        public string SessionId = Guid.NewGuid().ToString("N");
        public List<MessageEntry> Messages = new();
        public string Prompt = "";
        public string Thinking = "";
        public string Error = "";
        public string Status = "";
        public bool WasAwaitingResponse;
        public bool HostWasRunning;
        public bool ResponseInterrupted;
        public string LastSubmittedPrompt = "";
        public string LastSubmittedImageBase64 = "";
        public string AttachedSnapshotBase64 = "";
        public string AttachedSnapshotFocusMode = "whole_scene";

        public static CopilotSdkSessionState Get()
        {
            return instance;
        }

        public void SaveState()
        {
            Save(true);
        }
    }
}
