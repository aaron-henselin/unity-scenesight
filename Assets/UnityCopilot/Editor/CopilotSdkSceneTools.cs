using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YourCompany.UnityCopilot.Editor
{
    internal static class CopilotSdkSceneTools
    {
        [Serializable]
        internal sealed class FindGameObjectsArgs
        {
            public string nameContains;
            public string sceneName;
            public bool includeInactive = true;
            public int maxResults = 50;
            public List<ComponentPropertyRequest> components = new();
        }

        [Serializable]
        internal sealed class ListComponentsArgs
        {
            public string targetPath;
            public string targetName;
            public string sceneName;
            public bool includeInactive = true;
            public bool applyToAllMatches;
        }

        [Serializable]
        internal sealed class CaptureSceneSnapshotArgs
        {
            public int maxWidth = 1024;
            public int maxHeight = 768;
            public string focusMode = "whole_scene";
        }

        [Serializable]
        internal sealed class CaptureSceneSnapshotResult
        {
            public bool captured;
            public string path;
            public string displayName;
            public int byteCount;
            public string error;
            public string warning;
        }

        [Serializable]
        internal sealed class ComponentPropertyRequest
        {
            public string componentType;
            public List<string> properties = new();
        }

        [Serializable]
        internal struct Vector3Data
        {
            public float x;
            public float y;
            public float z;

            public Vector3 ToVector3()
            {
                return new Vector3(x, y, z);
            }
        }

        [Serializable]
        internal sealed class CreateGameObjectArgs
        {
            public string name;
            public string primitiveType;
            public string sceneName;
            public string parentPath;
            public bool setActive;
            public bool active = true;
            public bool setTag;
            public string tag;
            public bool setLayer;
            public int layer;
            public bool setPosition;
            public Vector3Data position;
            public bool setLocalPosition;
            public Vector3Data localPosition;
            public bool setRotation;
            public Vector3Data rotation;
            public bool setLocalRotation;
            public Vector3Data localRotation;
            public bool setScale;
            public Vector3Data scale;
        }

        [Serializable]
        internal sealed class UpdateGameObjectArgs
        {
            public string targetPath;
            public string targetName;
            public string sceneName;
            public bool includeInactive = true;
            public bool applyToAllMatches;
            public string newName;
            public string parentPath;
            public bool setActive;
            public bool active = true;
            public bool setTag;
            public string tag;
            public bool setLayer;
            public int layer;
            public bool setPosition;
            public Vector3Data position;
            public bool setLocalPosition;
            public Vector3Data localPosition;
            public bool setRotation;
            public Vector3Data rotation;
            public bool setLocalRotation;
            public Vector3Data localRotation;
            public bool setScale;
            public Vector3Data scale;
        }

        [Serializable]
        internal sealed class DeleteGameObjectArgs
        {
            public string targetPath;
            public string targetName;
            public string sceneName;
            public bool includeInactive = true;
            public bool deleteAllMatches;
        }

        [Serializable]
        internal struct Vector4Data
        {
            public float x;
            public float y;
            public float z;
            public float w;

            public Vector4 ToVector4()
            {
                return new Vector4(x, y, z, w);
            }
        }

        [Serializable]
        internal struct ColorData
        {
            public float r;
            public float g;
            public float b;
            public float a;

            public Color ToColor()
            {
                return new Color(r, g, b, a);
            }
        }

        [Serializable]
        internal sealed class ComponentPropertyAssignment
        {
            public string name;
            public string valueType;
            public string stringValue;
            public int intValue;
            public float floatValue;
            public bool boolValue;
            public Vector3Data vector3Value;
            public Vector4Data vector4Value;
            public ColorData colorValue;
        }

        [Serializable]
        internal sealed class ComponentInfo
        {
            public string componentType;
            public int instanceId;
            public List<ComponentPropertyAssignment> properties = new();
        }

        [Serializable]
        internal sealed class AddComponentArgs
        {
            public string targetPath;
            public string targetName;
            public string sceneName;
            public bool includeInactive = true;
            public bool applyToAllMatches;
            public string componentType;
        }

        [Serializable]
        internal sealed class RemoveComponentArgs
        {
            public string targetPath;
            public string targetName;
            public string sceneName;
            public bool includeInactive = true;
            public bool applyToAllMatches;
            public string componentType;
        }

        [Serializable]
        internal sealed class SetComponentPropertiesArgs
        {
            public string targetPath;
            public string targetName;
            public string sceneName;
            public bool includeInactive = true;
            public bool applyToAllMatches;
            public string componentType;
            public List<int> componentInstanceIds = new();
            public List<ComponentPropertyAssignment> properties = new();
        }

        [Serializable]
        internal sealed class SceneObjectInfo
        {
            public string name;
            public string path;
            public string scene;
            public bool activeSelf;
            public bool activeInHierarchy;
            public string tag;
            public int layer;
            public int instanceId;
            public string globalId;
            public List<ComponentInfo> components = new();
        }

        [Serializable]
        internal sealed class FindGameObjectsResult
        {
            public string query;
            public string sceneName;
            public bool includeInactive;
            public int maxResults;
            public int matchCount;
            public bool truncated;
            public List<SceneObjectInfo> matches = new();
            public string error;
            public string warning;
        }

        [Serializable]
        internal sealed class ListComponentsResult
        {
            public int objectCount;
            public List<SceneObjectInfo> objects = new();
            public string error;
            public string warning;
        }

        [Serializable]
        internal sealed class CreateGameObjectResult
        {
            public bool created;
            public SceneObjectInfo objectInfo;
            public string error;
            public string warning;
        }

        [Serializable]
        internal sealed class UpdateGameObjectResult
        {
            public bool updated;
            public int updatedCount;
            public List<SceneObjectInfo> objects = new();
            public string error;
            public string warning;
        }

        [Serializable]
        internal sealed class DeleteGameObjectResult
        {
            public int deletedCount;
            public List<string> deletedPaths = new();
            public string error;
            public string warning;
        }

        [Serializable]
        internal sealed class AddComponentResult
        {
            public int addedCount;
            public List<SceneObjectInfo> objects = new();
            public string error;
            public string warning;
        }

        [Serializable]
        internal sealed class RemoveComponentResult
        {
            public int removedCount;
            public List<SceneObjectInfo> objects = new();
            public string error;
            public string warning;
        }

        [Serializable]
        internal sealed class SetComponentPropertiesResult
        {
            public int updatedCount;
            public List<SceneObjectInfo> objects = new();
            public string error;
            public string warning;
        }

        [Serializable]
        internal sealed class ToolError
        {
            public string error;
        }

        public static FindGameObjectsResult FindGameObjects(FindGameObjectsArgs args)
        {
            var result = new FindGameObjectsResult();
            if (args == null)
            {
                result.error = "Missing arguments.";
                return result;
            }

            var query = args.nameContains ?? "";

            var includeInactive = args.includeInactive;
            var maxResults = Mathf.Clamp(args.maxResults <= 0 ? 50 : args.maxResults, 1, 200);
            var sceneFilter = args.sceneName ?? "";

            result.query = query;
            result.includeInactive = includeInactive;
            result.maxResults = maxResults;

            var warning = (string)null;
            if (!TryBuildComponentQueries(args.components, out var componentQueries, out var includeAllComponents, out var componentError, ref warning))
            {
                result.error = componentError;
                return result;
            }

            var scenesToSearch = new List<Scene>();
            if (string.IsNullOrWhiteSpace(sceneFilter))
            {
                var activeScene = SceneManager.GetActiveScene();
                if (!activeScene.IsValid() || !activeScene.isLoaded)
                {
                    result.error = "No active scene is loaded.";
                    return result;
                }

                result.sceneName = activeScene.name;
                scenesToSearch.Add(activeScene);
            }
            else
            {
                var namedScene = SceneManager.GetSceneByName(sceneFilter);
                if (!namedScene.IsValid() || !namedScene.isLoaded)
                {
                    result.error = $"Scene '{sceneFilter}' is not loaded.";
                    return result;
                }

                result.sceneName = namedScene.name;
                scenesToSearch.Add(namedScene);
            }

            foreach (var scene in scenesToSearch)
            {
                var roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    var transforms = root.GetComponentsInChildren<Transform>(includeInactive);
                    foreach (var transform in transforms)
                    {
                        var go = transform.gameObject;
                        if (!string.IsNullOrWhiteSpace(query)
                            && go.name.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            continue;
                        }

                        result.matches.Add(BuildSceneObjectInfo(go, scene.name, componentQueries, includeAllComponents, ref warning));

                        if (result.matches.Count >= maxResults)
                        {
                            result.truncated = true;
                            result.matchCount = result.matches.Count;
                            result.warning = warning;
                            return result;
                        }
                    }
                }
            }

            result.matchCount = result.matches.Count;
            result.warning = warning;
            return result;
        }

        public static ListComponentsResult ListComponents(ListComponentsArgs args)
        {
            var result = new ListComponentsResult();
            if (args == null)
            {
                result.error = "Missing arguments.";
                return result;
            }

            if (!TryGetScene(args.sceneName, out var scene, out var error))
            {
                result.error = error;
                return result;
            }

            var targets = ResolveTargets(scene, args.targetPath, args.targetName, args.includeInactive, args.applyToAllMatches, out error);
            if (targets == null)
            {
                result.error = error;
                return result;
            }

            var warning = (string)null;
            foreach (var target in targets)
            {
                var info = BuildSceneObjectInfo(target, scene.name);
                info.components = BuildComponentInfos(target, null, includeAllComponents: true, ref warning);
                result.objects.Add(info);
            }

            result.objectCount = result.objects.Count;
            result.warning = warning;
            return result;
        }

        public static CaptureSceneSnapshotResult CaptureSceneSnapshot(CaptureSceneSnapshotArgs args)
        {
            var result = new CaptureSceneSnapshotResult();
            if (args == null)
            {
                result.error = "Missing arguments.";
                return result;
            }

            if (!CopilotSdkSnapshotTool.IsSceneViewAvailable())
            {
                result.error = "Scene View is not available. Open a Scene View window first.";
                return result;
            }

            var maxWidth = Mathf.Clamp(args.maxWidth <= 0 ? 1024 : args.maxWidth, 1, 4096);
            var maxHeight = Mathf.Clamp(args.maxHeight <= 0 ? 768 : args.maxHeight, 1, 4096);
            var focusMode = CopilotSdkSnapshotTool.ParseFocusMode(args.focusMode);
            var base64 = CopilotSdkSnapshotTool.CaptureSceneViewBase64(maxWidth, maxHeight, focusMode);
            if (string.IsNullOrWhiteSpace(base64))
            {
                result.error = "Failed to capture Scene View snapshot.";
                return result;
            }

            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(base64);
            }
            catch (Exception ex)
            {
                result.error = $"Snapshot data was invalid: {ex.Message}";
                return result;
            }

            var root = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            var directory = Path.Combine(root, ".copilot", "attachments");
            Directory.CreateDirectory(directory);

            var fileName = $"scene_snapshot_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.png";
            var filePath = Path.Combine(directory, fileName);
            File.WriteAllBytes(filePath, bytes);

            result.captured = true;
            result.path = filePath;
            result.displayName = $"Scene Snapshot ({CopilotSdkSnapshotTool.GetFocusModeDisplayName(focusMode)})";
            result.byteCount = bytes.Length;
            return result;
        }

        public static CreateGameObjectResult CreateGameObject(CreateGameObjectArgs args)
        {
            var result = new CreateGameObjectResult();
            if (args == null)
            {
                result.error = "Missing arguments.";
                return result;
            }

            if (!TryGetScene(args.sceneName, out var scene, out var error))
            {
                result.error = error;
                return result;
            }

            var name = string.IsNullOrWhiteSpace(args.name) ? "New GameObject" : args.name.Trim();
            var primitiveTypeText = args.primitiveType?.Trim();
            var hasPrimitive = !string.IsNullOrWhiteSpace(primitiveTypeText);
            var warning = (string)null;

            PrimitiveType primitiveType = default;
            if (hasPrimitive && !TryParsePrimitiveType(primitiveTypeText, out primitiveType))
            {
                result.error = $"Unsupported primitiveType '{primitiveTypeText}'. Supported values: Cube, Sphere, Capsule, Cylinder, Plane, Quad.";
                return result;
            }

            // Fallback for model/tool calls that pass name = "Cube" etc. but omit primitiveType.
            if (!hasPrimitive && TryParsePrimitiveType(name, out var inferredPrimitive))
            {
                primitiveType = inferredPrimitive;
                hasPrimitive = true;
                warning = $"primitiveType was inferred from name '{name}'.";
            }

            var go = hasPrimitive ? GameObject.CreatePrimitive(primitiveType) : new GameObject(name);
            go.name = name;
            
            Undo.RegisterCreatedObjectUndo(go, "Create GameObject");
            SceneManager.MoveGameObjectToScene(go, scene);

            if (!string.IsNullOrWhiteSpace(args.parentPath))
            {
                var parent = FindGameObjectByPath(scene, args.parentPath);
                if (parent == null)
                {
                    result.error = $"Parent path '{args.parentPath}' not found.";
                    Undo.DestroyObjectImmediate(go);
                    return result;
                }

                Undo.SetTransformParent(go.transform, parent.transform, "Set Parent");
            }

            ApplyTransform(go.transform, args.setPosition, args.position, args.setLocalPosition, args.localPosition,
                args.setRotation, args.rotation, args.setLocalRotation, args.localRotation, args.setScale, args.scale);

            if (args.setActive)
            {
                go.SetActive(args.active);
            }

            if (args.setTag)
            {
                if (!TrySetTag(go, args.tag, out error))
                {
                    result.error = error;
                    return result;
                }
            }

            if (args.setLayer)
            {
                go.layer = args.layer;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            result.created = true;
            result.objectInfo = BuildSceneObjectInfo(go, scene.name);
            result.warning = warning;
            return result;
        }

        public static UpdateGameObjectResult UpdateGameObject(UpdateGameObjectArgs args)
        {
            var result = new UpdateGameObjectResult();
            if (args == null)
            {
                result.error = "Missing arguments.";
                return result;
            }

            if (!TryGetScene(args.sceneName, out var scene, out var error))
            {
                result.error = error;
                return result;
            }

            var targets = ResolveTargets(scene, args.targetPath, args.targetName, args.includeInactive, args.applyToAllMatches, out error);
            if (targets == null)
            {
                result.error = error;
                return result;
            }

            foreach (var target in targets)
            {
                Undo.RecordObject(target.transform, "Update Transform");
                Undo.RecordObject(target, "Update GameObject");

                if (!string.IsNullOrWhiteSpace(args.newName))
                {
                    target.name = args.newName.Trim();
                }

                if (!string.IsNullOrWhiteSpace(args.parentPath))
                {
                    var parent = FindGameObjectByPath(scene, args.parentPath);
                    if (parent == null)
                    {
                        result.error = $"Parent path '{args.parentPath}' not found.";
                        return result;
                    }

                    Undo.SetTransformParent(target.transform, parent.transform, "Set Parent");
                }

                ApplyTransform(target.transform, args.setPosition, args.position, args.setLocalPosition, args.localPosition,
                    args.setRotation, args.rotation, args.setLocalRotation, args.localRotation, args.setScale, args.scale);

                if (args.setActive)
                {
                    target.SetActive(args.active);
                }

                if (args.setTag)
                {
                    if (!TrySetTag(target, args.tag, out error))
                    {
                        result.error = error;
                        return result;
                    }
                }

                if (args.setLayer)
                {
                    target.layer = args.layer;
                }

                result.objects.Add(BuildSceneObjectInfo(target, scene.name));
            }

            result.updatedCount = result.objects.Count;
            result.updated = result.updatedCount > 0;
            EditorSceneManager.MarkSceneDirty(scene);
            return result;
        }

        public static DeleteGameObjectResult DeleteGameObject(DeleteGameObjectArgs args)
        {
            var result = new DeleteGameObjectResult();
            if (args == null)
            {
                result.error = "Missing arguments.";
                return result;
            }

            if (!TryGetScene(args.sceneName, out var scene, out var error))
            {
                result.error = error;
                return result;
            }

            var targets = ResolveTargets(scene, args.targetPath, args.targetName, args.includeInactive, args.deleteAllMatches, out error);
            if (targets == null)
            {
                result.error = error;
                return result;
            }

            foreach (var target in targets)
            {
                result.deletedPaths.Add(BuildPath(target.transform));
                Undo.DestroyObjectImmediate(target);
            }

            result.deletedCount = result.deletedPaths.Count;
            EditorSceneManager.MarkSceneDirty(scene);
            return result;
        }

        public static AddComponentResult AddComponent(AddComponentArgs args)
        {
            var result = new AddComponentResult();
            if (args == null)
            {
                result.error = "Missing arguments.";
                return result;
            }

            if (!TryGetScene(args.sceneName, out var scene, out var error))
            {
                result.error = error;
                return result;
            }

            if (!TryResolveComponentType(args.componentType, out var type, out error))
            {
                result.error = error;
                return result;
            }

            var targets = ResolveTargets(scene, args.targetPath, args.targetName, args.includeInactive, args.applyToAllMatches, out error);
            if (targets == null)
            {
                result.error = error;
                return result;
            }

            var disallowMultiple = Attribute.IsDefined(type, typeof(DisallowMultipleComponent));
            var addedCount = 0;
            foreach (var target in targets)
            {
                if (disallowMultiple && target.GetComponent(type) != null)
                {
                    result.warning = $"Component '{type.Name}' already exists on '{target.name}'.";
                    result.objects.Add(BuildSceneObjectInfo(target, scene.name));
                    continue;
                }

                Undo.AddComponent(target, type);
                addedCount++;
                result.objects.Add(BuildSceneObjectInfo(target, scene.name));
            }

            result.addedCount = addedCount;
            EditorSceneManager.MarkSceneDirty(scene);
            return result;
        }

        public static RemoveComponentResult RemoveComponent(RemoveComponentArgs args)
        {
            var result = new RemoveComponentResult();
            if (args == null)
            {
                result.error = "Missing arguments.";
                return result;
            }

            if (!TryGetScene(args.sceneName, out var scene, out var error))
            {
                result.error = error;
                return result;
            }

            if (!TryResolveComponentType(args.componentType, out var type, out error))
            {
                result.error = error;
                return result;
            }

            var targets = ResolveTargets(scene, args.targetPath, args.targetName, args.includeInactive, args.applyToAllMatches, out error);
            if (targets == null)
            {
                result.error = error;
                return result;
            }

            var removed = 0;
            foreach (var target in targets)
            {
                var components = target.GetComponents(type);
                if (components == null || components.Length == 0)
                {
                    continue;
                }

                foreach (var component in components)
                {
                    Undo.DestroyObjectImmediate(component);
                    removed++;
                }

                result.objects.Add(BuildSceneObjectInfo(target, scene.name));
            }

            result.removedCount = removed;
            if (removed == 0)
            {
                result.warning = $"No components of type '{type.Name}' were found.";
            }

            EditorSceneManager.MarkSceneDirty(scene);
            return result;
        }

        public static SetComponentPropertiesResult SetComponentProperties(SetComponentPropertiesArgs args)
        {
            var result = new SetComponentPropertiesResult();
            if (args == null)
            {
                result.error = "Missing arguments.";
                return result;
            }

            if (!TryGetScene(args.sceneName, out var scene, out var error))
            {
                result.error = error;
                return result;
            }

            if (!TryResolveComponentType(args.componentType, out var type, out error))
            {
                result.error = error;
                return result;
            }

            var targets = ResolveTargets(scene, args.targetPath, args.targetName, args.includeInactive, args.applyToAllMatches, out error);
            if (targets == null)
            {
                result.error = error;
                return result;
            }

            if (args.properties == null || args.properties.Count == 0)
            {
                result.error = "No properties specified.";
                return result;
            }

            var updated = 0;
            foreach (var target in targets)
            {
                var components = target.GetComponents(type);
                if (components == null || components.Length == 0)
                {
                    result.warning = $"Component '{type.Name}' not found on '{target.name}'.";
                    continue;
                }

                var filteredComponents = components;
                if (args.componentInstanceIds != null && args.componentInstanceIds.Count > 0)
                {
                    var matches = new List<Component>();
                    foreach (var component in components)
                    {
                        if (args.componentInstanceIds.Contains(component.GetInstanceID()))
                        {
                            matches.Add(component);
                        }
                    }

                    if (matches.Count == 0)
                    {
                        result.warning = $"No '{type.Name}' components matched the requested instance IDs on '{target.name}'.";
                        continue;
                    }

                    filteredComponents = matches.ToArray();
                }

                foreach (var component in filteredComponents)
                {
                    Undo.RecordObject(component, "Set Component Properties");
                    if (!ApplyPropertyAssignments(component, args.properties, out error))
                    {
                        result.error = error;
                        return result;
                    }

                    updated++;
                }

                result.objects.Add(BuildSceneObjectInfo(target, scene.name));
            }

            result.updatedCount = updated;
            EditorSceneManager.MarkSceneDirty(scene);
            return result;
        }

        private static SceneObjectInfo BuildSceneObjectInfo(GameObject go, string sceneName)
        {
            var warning = (string)null;
            return BuildSceneObjectInfo(go, sceneName, null, includeAllComponents: false, ref warning);
        }

        private static SceneObjectInfo BuildSceneObjectInfo(GameObject go, string sceneName, List<ComponentQuery> componentQueries, bool includeAllComponents, ref string warning)
        {
            var globalId = GlobalObjectId.GetGlobalObjectIdSlow(go);
            var info = new SceneObjectInfo
            {
                name = go.name,
                path = BuildPath(go.transform),
                scene = sceneName,
                activeSelf = go.activeSelf,
                activeInHierarchy = go.activeInHierarchy,
                tag = go.tag,
                layer = go.layer,
                instanceId = go.GetInstanceID(),
                globalId = globalId.ToString()
            };

            if ((componentQueries != null && componentQueries.Count > 0) || includeAllComponents)
            {
                info.components = BuildComponentInfos(go, componentQueries, includeAllComponents, ref warning);
            }

            return info;
        }

        private sealed class ComponentQuery
        {
            public string componentType;
            public Type type;
            public List<string> properties;
        }

        private static bool TryBuildComponentQueries(List<ComponentPropertyRequest> requests, out List<ComponentQuery> queries, out bool includeAllComponents, out string error, ref string warning)
        {
            queries = new List<ComponentQuery>();
            includeAllComponents = false;
            error = null;

            if (requests == null || requests.Count == 0)
            {
                return true;
            }

            foreach (var request in requests)
            {
                if (request == null)
                {
                    continue;
                }

                var typeName = request.componentType?.Trim();
                if (string.IsNullOrWhiteSpace(typeName))
                {
                    warning ??= "Component request missing componentType.";
                    continue;
                }

                if (string.Equals(typeName, "*", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(typeName, "all", StringComparison.OrdinalIgnoreCase))
                {
                    includeAllComponents = true;
                    continue;
                }

                if (!TryResolveComponentType(typeName, out var type, out error))
                {
                    return false;
                }

                queries.Add(new ComponentQuery
                {
                    componentType = typeName,
                    type = type,
                    properties = request.properties ?? new List<string>()
                });
            }

            return true;
        }

        private static List<ComponentInfo> BuildComponentInfos(GameObject go, List<ComponentQuery> queries, bool includeAllComponents, ref string warning)
        {
            var results = new Dictionary<Component, ComponentInfo>();

            void EnsureComponent(Component component)
            {
                if (component == null || results.ContainsKey(component))
                {
                    return;
                }

                results[component] = new ComponentInfo
                {
                    componentType = component.GetType().FullName,
                    instanceId = component.GetInstanceID()
                };
            }

            if (includeAllComponents)
            {
                var allComponents = go.GetComponents<Component>();
                if (allComponents != null)
                {
                    foreach (var component in allComponents)
                    {
                        EnsureComponent(component);
                    }
                }
            }

            if (queries != null)
            {
                foreach (var query in queries)
                {
                    var components = go.GetComponents(query.type);
                    if (components == null || components.Length == 0)
                    {
                        continue;
                    }

                    foreach (var component in components)
                    {
                        EnsureComponent(component);
                        var info = results[component];

                        if (query.properties != null && query.properties.Count > 0)
                        {
                            foreach (var property in query.properties)
                            {
                                if (string.IsNullOrWhiteSpace(property))
                                {
                                    continue;
                                }

                                if (!TryGetMemberValue(component, property.Trim(), out var assignment, out var error))
                                {
                                    warning ??= error;
                                    continue;
                                }

                                info.properties.Add(assignment);
                            }
                        }
                    }
                }
            }

            return new List<ComponentInfo>(results.Values);
        }

        private static bool TryGetScene(string sceneName, out Scene scene, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                scene = SceneManager.GetActiveScene();
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    error = "No active scene is loaded.";
                    return false;
                }

                return true;
            }

            scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                error = $"Scene '{sceneName}' is not loaded.";
                return false;
            }

            return true;
        }

        private static bool TryResolveComponentType(string typeName, out Type type, out string error)
        {
            error = null;
            type = null;
            if (string.IsNullOrWhiteSpace(typeName))
            {
                error = "Component type is required.";
                return false;
            }

            var matches = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch
                {
                    continue;
                }

                foreach (var candidate in types)
                {
                    if (candidate == null || candidate.IsAbstract)
                    {
                        continue;
                    }

                    if (!typeof(Component).IsAssignableFrom(candidate))
                    {
                        continue;
                    }

                    if (string.Equals(candidate.Name, typeName, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(candidate.FullName, typeName, StringComparison.OrdinalIgnoreCase))
                    {
                        matches.Add(candidate);
                    }
                }
            }

            if (matches.Count == 0)
            {
                error = $"Component type '{typeName}' was not found.";
                return false;
            }

            if (matches.Count > 1)
            {
                error = $"Component type '{typeName}' is ambiguous. Use a full name. Matches: {string.Join(", ", matches.ConvertAll(t => t.FullName))}";
                return false;
            }

            type = matches[0];
            return true;
        }

        private static List<GameObject> ResolveTargets(Scene scene, string targetPath, string targetName, bool includeInactive, bool allowMultiple, out string error)
        {
            error = null;
            if (!string.IsNullOrWhiteSpace(targetPath))
            {
                var target = FindGameObjectByPath(scene, targetPath);
                if (target == null)
                {
                    error = $"Target path '{targetPath}' not found.";
                    return null;
                }

                return new List<GameObject> { target };
            }

            if (string.IsNullOrWhiteSpace(targetName))
            {
                error = "Provide targetPath or targetName.";
                return null;
            }

            var matches = FindGameObjectsByName(scene, targetName, includeInactive);
            if (matches.Count == 0)
            {
                error = $"No GameObject named '{targetName}' found.";
                return null;
            }

            if (matches.Count > 1 && !allowMultiple)
            {
                error = $"Multiple GameObjects named '{targetName}' found. Provide targetPath or enable applyToAllMatches/deleteAllMatches.";
                return null;
            }

            return matches;
        }

        private static GameObject FindGameObjectByPath(Scene scene, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
            {
                return null;
            }

            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                if (!string.Equals(root.name, segments[0], StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (segments.Length == 1)
                {
                    return root;
                }

                var current = root.transform;
                for (var i = 1; i < segments.Length; i++)
                {
                    current = FindChildByName(current, segments[i]);
                    if (current == null)
                    {
                        return null;
                    }
                }

                return current.gameObject;
            }

            return null;
        }

        private static Transform FindChildByName(Transform parent, string name)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                if (string.Equals(child.name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return child;
                }
            }

            return null;
        }

        private static List<GameObject> FindGameObjectsByName(Scene scene, string name, bool includeInactive)
        {
            var results = new List<GameObject>();
            var roots = scene.GetRootGameObjects();
            foreach (var root in roots)
            {
                var transforms = root.GetComponentsInChildren<Transform>(includeInactive);
                foreach (var transform in transforms)
                {
                    if (string.Equals(transform.name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(transform.gameObject);
                    }
                }
            }

            return results;
        }

        private static void ApplyTransform(
            Transform transform,
            bool setPosition,
            Vector3Data position,
            bool setLocalPosition,
            Vector3Data localPosition,
            bool setRotation,
            Vector3Data rotation,
            bool setLocalRotation,
            Vector3Data localRotation,
            bool setScale,
            Vector3Data scale)
        {
            if (setPosition)
            {
                transform.position = position.ToVector3();
            }

            if (setLocalPosition)
            {
                transform.localPosition = localPosition.ToVector3();
            }

            if (setRotation)
            {
                transform.rotation = Quaternion.Euler(rotation.ToVector3());
            }

            if (setLocalRotation)
            {
                transform.localRotation = Quaternion.Euler(localRotation.ToVector3());
            }

            if (setScale)
            {
                transform.localScale = scale.ToVector3();
            }
        }

        private static bool ApplyPropertyAssignments(Component component, List<ComponentPropertyAssignment> assignments, out string error)
        {
            error = null;
            var type = component.GetType();
            foreach (var assignment in assignments)
            {
                if (assignment == null || string.IsNullOrWhiteSpace(assignment.name))
                {
                    continue;
                }

                var memberName = assignment.name;
                if (!TryResolveMember(type, memberName, true, out var field, out var property, out error))
                {
                    return false;
                }

                var targetType = field != null ? field.FieldType : property.PropertyType;
                if (!TryConvertValue(assignment, targetType, out var value, out error))
                {
                    return false;
                }

                if (field != null)
                {
                    field.SetValue(component, value);
                }
                else if (property != null)
                {
                    property.SetValue(component, value, null);
                }
            }

            EditorUtility.SetDirty(component);
            return true;
        }

        private static bool TryGetMemberValue(Component component, string name, out ComponentPropertyAssignment assignment, out string error)
        {
            assignment = null;
            error = null;

            var type = component.GetType();
            if (!TryResolveMember(type, name, false, out var field, out var property, out error))
            {
                return false;
            }

            object value;
            Type valueType;
            try
            {
                if (field != null)
                {
                    valueType = field.FieldType;
                    value = field.GetValue(component);
                }
                else
                {
                    valueType = property.PropertyType;
                    value = property.GetValue(component, null);
                }
            }
            catch (Exception ex)
            {
                error = $"Failed to read member '{name}' on '{type.Name}': {ex.Message}";
                return false;
            }

            return TrySerializeValue(name, value, valueType, out assignment, out error);
        }

        private static bool TrySerializeValue(string name, object value, Type targetType, out ComponentPropertyAssignment assignment, out string error)
        {
            assignment = new ComponentPropertyAssignment { name = name };
            error = null;

            if (value == null)
            {
                assignment.valueType = "string";
                assignment.stringValue = "null";
                return true;
            }

            if (targetType.IsEnum)
            {
                assignment.valueType = "string";
                assignment.stringValue = value.ToString();
                return true;
            }

            if (targetType == typeof(string))
            {
                assignment.valueType = "string";
                assignment.stringValue = value as string ?? "";
                return true;
            }

            if (targetType == typeof(int))
            {
                assignment.valueType = "int";
                assignment.intValue = (int)value;
                return true;
            }

            if (targetType == typeof(float))
            {
                assignment.valueType = "float";
                assignment.floatValue = (float)value;
                return true;
            }

            if (targetType == typeof(double))
            {
                assignment.valueType = "float";
                assignment.floatValue = Convert.ToSingle(value);
                return true;
            }

            if (targetType == typeof(bool))
            {
                assignment.valueType = "bool";
                assignment.boolValue = (bool)value;
                return true;
            }

            if (targetType == typeof(Vector2))
            {
                var v = (Vector2)value;
                assignment.valueType = "vector3";
                assignment.vector3Value = new Vector3Data { x = v.x, y = v.y, z = 0f };
                return true;
            }

            if (targetType == typeof(Vector3))
            {
                var v = (Vector3)value;
                assignment.valueType = "vector3";
                assignment.vector3Value = new Vector3Data { x = v.x, y = v.y, z = v.z };
                return true;
            }

            if (targetType == typeof(Vector4))
            {
                var v = (Vector4)value;
                assignment.valueType = "vector4";
                assignment.vector4Value = new Vector4Data { x = v.x, y = v.y, z = v.z, w = v.w };
                return true;
            }

            if (targetType == typeof(Quaternion))
            {
                var q = (Quaternion)value;
                assignment.valueType = "vector4";
                assignment.vector4Value = new Vector4Data { x = q.x, y = q.y, z = q.z, w = q.w };
                return true;
            }

            if (targetType == typeof(Color))
            {
                var c = (Color)value;
                assignment.valueType = "color";
                assignment.colorValue = new ColorData { r = c.r, g = c.g, b = c.b, a = c.a };
                return true;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(targetType))
            {
                var obj = value as UnityEngine.Object;
                if (obj == null)
                {
                    assignment.valueType = "string";
                    assignment.stringValue = "null";
                    return true;
                }

                if (AssetDatabase.Contains(obj))
                {
                    var path = AssetDatabase.GetAssetPath(obj);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        assignment.valueType = "assetPath";
                        assignment.stringValue = path;
                        return true;
                    }
                }

                var globalId = GlobalObjectId.GetGlobalObjectIdSlow(obj);
                var globalIdText = globalId.ToString();
                if (!string.IsNullOrWhiteSpace(globalIdText))
                {
                    assignment.valueType = "globalId";
                    assignment.stringValue = globalIdText;
                    return true;
                }

                assignment.valueType = "instanceId";
                assignment.intValue = obj.GetInstanceID();
                return true;
            }

            error = $"Unsupported value type '{targetType.Name}' for member '{name}'.";
            return false;
        }

        private static bool TryResolveMember(Type type, string name, bool requireWritable, out System.Reflection.FieldInfo field, out System.Reflection.PropertyInfo property, out string error)
        {
            error = null;
            field = null;
            property = null;

            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            var candidateField = type.GetField(name, flags);
            if (candidateField == null)
            {
                foreach (var f in type.GetFields(flags))
                {
                    if (string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        candidateField = f;
                        break;
                    }
                }
            }

            if (candidateField != null)
            {
                var isPublic = candidateField.IsPublic;
                var isSerialized = Attribute.IsDefined(candidateField, typeof(SerializeField));
                if (isPublic || isSerialized)
                {
                    field = candidateField;
                    return true;
                }
            }

            var candidateProperty = type.GetProperty(name, flags);
            if (candidateProperty == null)
            {
                foreach (var p in type.GetProperties(flags))
                {
                    if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                    {
                        candidateProperty = p;
                        break;
                    }
                }
            }

            if (candidateProperty != null && candidateProperty.CanRead && (!requireWritable || candidateProperty.CanWrite))
            {
                property = candidateProperty;
                return true;
            }

            error = requireWritable
                ? $"Member '{name}' not found or not writable on '{type.Name}'."
                : $"Member '{name}' not found or not readable on '{type.Name}'.";
            return false;
        }

        private static bool TryConvertValue(ComponentPropertyAssignment assignment, Type targetType, out object value, out string error)
        {
            value = null;
            error = null;

            var valueType = assignment.valueType?.Trim().ToLowerInvariant() ?? "";

            if (targetType.IsEnum)
            {
                if (valueType == "string" && !string.IsNullOrWhiteSpace(assignment.stringValue))
                {
                    if (Enum.TryParse(targetType, assignment.stringValue, true, out var enumValue))
                    {
                        value = enumValue;
                        return true;
                    }
                }

                if (valueType == "int")
                {
                    value = Enum.ToObject(targetType, assignment.intValue);
                    return true;
                }

                error = $"Invalid enum value for '{targetType.Name}'.";
                return false;
            }

            if (targetType == typeof(string))
            {
                value = assignment.stringValue ?? "";
                return true;
            }

            if (targetType == typeof(int))
            {
                if (valueType == "int")
                {
                    value = assignment.intValue;
                    return true;
                }

                if (valueType == "float")
                {
                    value = Mathf.RoundToInt(assignment.floatValue);
                    return true;
                }
            }

            if (targetType == typeof(float))
            {
                if (valueType == "float")
                {
                    value = assignment.floatValue;
                    return true;
                }

                if (valueType == "int")
                {
                    value = assignment.intValue;
                    return true;
                }
            }

            if (targetType == typeof(double))
            {
                if (valueType == "float")
                {
                    value = assignment.floatValue;
                    return true;
                }

                if (valueType == "int")
                {
                    value = (double)assignment.intValue;
                    return true;
                }
            }

            if (targetType == typeof(bool))
            {
                if (valueType == "bool")
                {
                    value = assignment.boolValue;
                    return true;
                }
            }

            if (targetType == typeof(Vector2))
            {
                if (valueType == "vector3")
                {
                    var v = assignment.vector3Value.ToVector3();
                    value = new Vector2(v.x, v.y);
                    return true;
                }
            }

            if (targetType == typeof(Vector3))
            {
                if (valueType == "vector3")
                {
                    value = assignment.vector3Value.ToVector3();
                    return true;
                }
            }

            if (targetType == typeof(Vector4))
            {
                if (valueType == "vector4")
                {
                    value = assignment.vector4Value.ToVector4();
                    return true;
                }

                if (valueType == "vector3")
                {
                    var v = assignment.vector3Value.ToVector3();
                    value = new Vector4(v.x, v.y, v.z, 0f);
                    return true;
                }
            }

            if (targetType == typeof(Quaternion))
            {
                if (valueType == "vector3")
                {
                    value = Quaternion.Euler(assignment.vector3Value.ToVector3());
                    return true;
                }

                if (valueType == "vector4")
                {
                    var v = assignment.vector4Value.ToVector4();
                    value = new Quaternion(v.x, v.y, v.z, v.w);
                    return true;
                }
            }

            if (targetType == typeof(Color))
            {
                if (valueType == "color")
                {
                    value = assignment.colorValue.ToColor();
                    return true;
                }
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(targetType))
            {
                if (valueType == "assetpath")
                {
                    var path = assignment.stringValue?.Trim();
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        error = "Asset path is required.";
                        return false;
                    }

                    var asset = AssetDatabase.LoadAssetAtPath(path, targetType);
                    if (asset == null)
                    {
                        error = $"Asset at '{path}' was not found or does not match type '{targetType.Name}'.";
                        return false;
                    }

                    value = asset;
                    return true;
                }

                if (valueType == "guid")
                {
                    var guid = assignment.stringValue?.Trim();
                    if (string.IsNullOrWhiteSpace(guid))
                    {
                        error = "GUID is required.";
                        return false;
                    }

                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        error = $"GUID '{guid}' did not resolve to an asset path.";
                        return false;
                    }

                    var asset = AssetDatabase.LoadAssetAtPath(path, targetType);
                    if (asset == null)
                    {
                        error = $"Asset at '{path}' was not found or does not match type '{targetType.Name}'.";
                        return false;
                    }

                    value = asset;
                    return true;
                }

                if (valueType == "globalid")
                {
                    var idText = assignment.stringValue?.Trim();
                    if (string.IsNullOrWhiteSpace(idText))
                    {
                        error = "GlobalObjectId is required.";
                        return false;
                    }

                    if (!GlobalObjectId.TryParse(idText, out var globalId))
                    {
                        error = $"GlobalObjectId '{idText}' is invalid.";
                        return false;
                    }

                    var obj = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalId);
                    if (obj == null)
                    {
                        error = $"GlobalObjectId '{idText}' did not resolve to an object.";
                        return false;
                    }

                    if (!targetType.IsInstanceOfType(obj))
                    {
                        error = $"Object '{obj.name}' is not assignable to '{targetType.Name}'.";
                        return false;
                    }

                    value = obj;
                    return true;
                }

                if (valueType == "instanceid")
                {
                    var id = assignment.intValue;
                    if (id == 0)
                    {
                        error = "InstanceId is required.";
                        return false;
                    }

                    var obj = EditorUtility.InstanceIDToObject(id);
                    if (obj == null)
                    {
                        error = $"InstanceId '{id}' did not resolve to an object.";
                        return false;
                    }

                    if (!targetType.IsInstanceOfType(obj))
                    {
                        error = $"Object '{obj.name}' is not assignable to '{targetType.Name}'.";
                        return false;
                    }

                    value = obj;
                    return true;
                }
            }

            error = $"Unsupported value type '{valueType}' for member type '{targetType.Name}'.";
            return false;
        }

        private static bool TrySetTag(GameObject go, string tag, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(tag))
            {
                error = "Tag is empty.";
                return false;
            }

            try
            {
                go.tag = tag;
                return true;
            }
            catch (Exception ex)
            {
                error = $"Invalid tag '{tag}': {ex.Message}";
                return false;
            }
        }

        private static bool TryParsePrimitiveType(string text, out PrimitiveType primitiveType)
        {
            primitiveType = default;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (Enum.TryParse(text.Trim(), true, out PrimitiveType parsed))
            {
                primitiveType = parsed;
                return true;
            }

            return false;
        }

        private static string BuildPath(Transform transform)
        {
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
    }
}
