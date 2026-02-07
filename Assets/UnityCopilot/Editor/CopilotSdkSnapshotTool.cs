using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YourCompany.UnityCopilot.Editor
{
    /// <summary>
    /// Utility for capturing Scene View snapshots and encoding them for the Copilot API.
    /// </summary>
    public static class CopilotSdkSnapshotTool
    {
        public enum SnapshotFocusMode
        {
            WholeScene,
            SelectedAssets
        }

        private struct SceneViewCameraState
        {
            public Vector3 Pivot;
            public Quaternion Rotation;
            public float Size;
            public bool Orthographic;
        }

        public static SnapshotFocusMode ParseFocusMode(string focusMode)
        {
            if (string.Equals(focusMode, "selected_assets", StringComparison.OrdinalIgnoreCase))
            {
                return SnapshotFocusMode.SelectedAssets;
            }

            return SnapshotFocusMode.WholeScene;
        }

        public static string GetFocusModeKey(SnapshotFocusMode focusMode)
        {
            return focusMode == SnapshotFocusMode.SelectedAssets ? "selected_assets" : "whole_scene";
        }

        public static string GetFocusModeDisplayName(SnapshotFocusMode focusMode)
        {
            return focusMode == SnapshotFocusMode.SelectedAssets ? "Selected Assets" : "Whole Scene";
        }

        /// <summary>
        /// Captures a snapshot of the Scene View camera.
        /// </summary>
        /// <param name="maxWidth">Maximum width of the output image.</param>
        /// <param name="maxHeight">Maximum height of the output image.</param>
        /// <param name="focusMode">How the Scene View should be framed for capture.</param>
        /// <returns>Base64-encoded PNG image, or null if capture failed.</returns>
        public static string CaptureSceneViewBase64(
            int maxWidth = 1024,
            int maxHeight = 768,
            SnapshotFocusMode focusMode = SnapshotFocusMode.WholeScene)
        {
            var screenshot = CaptureSceneViewTexture(maxWidth, maxHeight, focusMode);
            if (screenshot == null)
            {
                return null;
            }

            try
            {
                var pngBytes = screenshot.EncodeToPNG();
                return Convert.ToBase64String(pngBytes);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(screenshot);
            }
        }

        /// <summary>
        /// Captures a snapshot and returns it as a Texture2D for preview purposes.
        /// </summary>
        /// <param name="maxWidth">Maximum width of the output image.</param>
        /// <param name="maxHeight">Maximum height of the output image.</param>
        /// <param name="focusMode">How the Scene View should be framed for capture.</param>
        /// <returns>Texture2D snapshot, or null if capture failed. Caller must destroy.</returns>
        public static Texture2D CaptureSceneViewTexture(
            int maxWidth = 256,
            int maxHeight = 192,
            SnapshotFocusMode focusMode = SnapshotFocusMode.WholeScene)
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                return null;
            }

            var camera = sceneView.camera;
            if (camera == null)
            {
                return null;
            }

            var viewWidth = (int)sceneView.position.width;
            var viewHeight = (int)sceneView.position.height;

            if (viewWidth <= 0 || viewHeight <= 0)
            {
                return null;
            }

            var scale = Mathf.Min(
                (float)maxWidth / viewWidth,
                (float)maxHeight / viewHeight,
                1f
            );

            var targetWidth = Mathf.Max(1, Mathf.RoundToInt(viewWidth * scale));
            var targetHeight = Mathf.Max(1, Mathf.RoundToInt(viewHeight * scale));

            RenderTexture renderTexture = null;
            RenderTexture previousTarget = null;
            Texture2D screenshot = null;

            var previousState = CaptureSceneViewCameraState(sceneView);
            var reframed = TryFrameSceneView(sceneView, focusMode);

            try
            {
                renderTexture = new RenderTexture(viewWidth, viewHeight, 24, RenderTextureFormat.ARGB32);
                renderTexture.antiAliasing = 1;
                renderTexture.Create();

                previousTarget = camera.targetTexture;
                try
                {
                    camera.targetTexture = renderTexture;
                    camera.Render();
                }
                finally
                {
                    camera.targetTexture = previousTarget;
                }

                var previousActive = RenderTexture.active;
                RenderTexture.active = renderTexture;

                screenshot = new Texture2D(viewWidth, viewHeight, TextureFormat.RGB24, false);
                screenshot.ReadPixels(new Rect(0, 0, viewWidth, viewHeight), 0, 0);
                screenshot.Apply();

                RenderTexture.active = previousActive;

                if (targetWidth != viewWidth || targetHeight != viewHeight)
                {
                    var resized = ResizeTexture(screenshot, targetWidth, targetHeight);
                    UnityEngine.Object.DestroyImmediate(screenshot);
                    screenshot = resized;
                }

                screenshot.hideFlags = HideFlags.HideAndDontSave;
                return screenshot;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to capture Scene View snapshot: {ex.Message}");

                if (screenshot != null)
                {
                    UnityEngine.Object.DestroyImmediate(screenshot);
                }

                return null;
            }
            finally
            {
                if (reframed)
                {
                    RestoreSceneViewCameraState(sceneView, previousState);
                }

                if (renderTexture != null)
                {
                    renderTexture.Release();
                    UnityEngine.Object.DestroyImmediate(renderTexture);
                }
            }
        }

        /// <summary>
        /// Checks if the Scene View is available for capture.
        /// </summary>
        public static bool IsSceneViewAvailable()
        {
            var sceneView = SceneView.lastActiveSceneView;
            return sceneView != null && sceneView.camera != null;
        }

        private static SceneViewCameraState CaptureSceneViewCameraState(SceneView sceneView)
        {
            return new SceneViewCameraState
            {
                Pivot = sceneView.pivot,
                Rotation = sceneView.rotation,
                Size = sceneView.size,
                Orthographic = sceneView.orthographic
            };
        }

        private static void RestoreSceneViewCameraState(SceneView sceneView, SceneViewCameraState state)
        {
            sceneView.pivot = state.Pivot;
            sceneView.rotation = state.Rotation;
            sceneView.size = state.Size;
            sceneView.orthographic = state.Orthographic;
            sceneView.Repaint();
        }

        private static bool TryFrameSceneView(SceneView sceneView, SnapshotFocusMode focusMode)
        {
            if (focusMode == SnapshotFocusMode.SelectedAssets)
            {
                if (!TryCalculateSelectionBounds(out var selectionBounds))
                {
                    Debug.LogWarning("Snapshot focus mode is Selected Assets, but no valid scene selection exists. Using current Scene View framing.");
                    return false;
                }

                FrameSceneViewToBounds(sceneView, selectionBounds);
                return true;
            }

            if (!TryCalculateActiveSceneBounds(out var sceneBounds))
            {
                Debug.LogWarning("Unable to compute active scene bounds for snapshot capture. Using current Scene View framing.");
                return false;
            }

            FrameSceneViewToBounds(sceneView, sceneBounds);
            return true;
        }

        private static void FrameSceneViewToBounds(SceneView sceneView, Bounds bounds)
        {
            var largestExtent = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
            var targetSize = Mathf.Max(0.5f, largestExtent * 2.5f);

            sceneView.pivot = bounds.center;
            sceneView.size = targetSize;
            sceneView.Repaint();
        }

        private static bool TryCalculateSelectionBounds(out Bounds bounds)
        {
            bounds = new Bounds();
            var hasBounds = false;
            var selected = Selection.gameObjects;
            if (selected == null || selected.Length == 0)
            {
                return false;
            }

            for (var i = 0; i < selected.Length; i++)
            {
                var selectedObject = selected[i];
                if (selectedObject == null)
                {
                    continue;
                }

                if (!selectedObject.scene.IsValid() || !selectedObject.scene.isLoaded)
                {
                    continue;
                }

                EncapsulateGameObjectBounds(selectedObject, ref bounds, ref hasBounds);
            }

            return hasBounds;
        }

        private static bool TryCalculateActiveSceneBounds(out Bounds bounds)
        {
            bounds = new Bounds();
            var hasBounds = false;
            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || !activeScene.isLoaded)
            {
                return false;
            }

            var roots = activeScene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var root = roots[i];
                if (root == null)
                {
                    continue;
                }

                EncapsulateGameObjectBounds(root, ref bounds, ref hasBounds);
            }

            return hasBounds;
        }

        private static void EncapsulateGameObjectBounds(GameObject go, ref Bounds bounds, ref bool hasBounds)
        {
            var objectHasBounds = false;

            var renderers = go.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                EncapsulateBounds(renderers[i].bounds, ref bounds, ref hasBounds);
                objectHasBounds = true;
            }

            var colliders = go.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                EncapsulateBounds(colliders[i].bounds, ref bounds, ref hasBounds);
                objectHasBounds = true;
            }

            var collider2Ds = go.GetComponentsInChildren<Collider2D>(true);
            for (var i = 0; i < collider2Ds.Length; i++)
            {
                EncapsulateBounds(collider2Ds[i].bounds, ref bounds, ref hasBounds);
                objectHasBounds = true;
            }

            if (objectHasBounds)
            {
                return;
            }

            // Fallback when objects have no renderer/collider bounds.
            var transforms = go.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var positionBounds = new Bounds(transforms[i].position, Vector3.one * 0.1f);
                EncapsulateBounds(positionBounds, ref bounds, ref hasBounds);
            }
        }

        private static void EncapsulateBounds(Bounds candidate, ref Bounds bounds, ref bool hasBounds)
        {
            if (!hasBounds)
            {
                bounds = candidate;
                hasBounds = true;
                return;
            }

            bounds.Encapsulate(candidate);
        }

        private static Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            var result = new Texture2D(targetWidth, targetHeight, source.format, false);
            var pixels = new Color[targetWidth * targetHeight];

            var incX = 1.0f / targetWidth;
            var incY = 1.0f / targetHeight;

            for (var y = 0; y < targetHeight; y++)
            {
                for (var x = 0; x < targetWidth; x++)
                {
                    pixels[y * targetWidth + x] = source.GetPixelBilinear(incX * x, incY * y);
                }
            }

            result.SetPixels(pixels);
            result.Apply();
            return result;
        }
    }
}
