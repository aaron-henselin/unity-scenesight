using UnityEditor;
using UnityEngine;

namespace YourCompany.UnityCopilot.Editor
{
    public class Test : EditorWindow
    {
        [MenuItem("Window/Unity Copilot/Test Window")]
        public static void ShowWindow()
        {
            GetWindow<Test>("Unity Copilot Test");
        }

        private void OnGUI()
        {
            GUILayout.Label("Unity Copilot Test Window", EditorStyles.boldLabel);
            GUILayout.Space(10);
            GUILayout.Label("This is a placeholder editor window.");
        }
    }
}
