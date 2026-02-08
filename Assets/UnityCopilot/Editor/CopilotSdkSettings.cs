using UnityEditor;

namespace YourCompany.UnityCopilot.Editor
{
    [FilePath("UserSettings/UnityCopilotSdkSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class CopilotSdkSettings : ScriptableSingleton<CopilotSdkSettings>
    {
        public string HostExecutablePath = "";
        public string HostArguments = "";
        public string CliUrl = "";
        public string RepoRoot = "";
        public string ExcludedPaths = "Library;Temp;obj;Logs;.git";
        public int MaxFileSizeKb = 200;
        public int MaxSearchResults = 50;
        public string InteractionMode = "ask";
        public int TimeoutSeconds = 180;
        public bool ToolDebug = false;
        public string Model = "claude-opus-4.5";
        public bool Streaming = true;
        public bool RenderMarkdown = true;
        public int SnapshotMaxWidth = 1024;
        public int SnapshotMaxHeight = 768;
        public string SnapshotCaptureMode = "whole_scene";

        public static CopilotSdkSettings Get()
        {
            return instance;
        }

        public void Save()
        {
            Save(true);
        }
    }
}
