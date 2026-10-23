
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class JobsPrefabInserter : EditorWindow
{
    [SerializeField] private GameObject jobsPrefab;

    [MenuItem("Tools/Stalls/Jobs Prefab Inserter")]
    private static void OpenWindow()
    {
        GetWindow<JobsPrefabInserter>("Jobs Inserter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Stall Jobs Prefab Inserter",
            EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "Adds or fixes the Jobs object under:\n" +
            "Stall -> Interaction Canvas -> Panel\n\n" +
            "Places Jobs immediately after Pdf and resets " +
            "its RectTransform position.",
            MessageType.Info);

        jobsPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Jobs Prefab",
            jobsPrefab,
            typeof(GameObject),
            false);

        EditorGUILayout.Space(10);

        GUI.enabled = jobsPrefab != null;

        if (GUILayout.Button("Process Current Scene", GUILayout.Height(35)))
        {
            ProcessCurrentScene();
        }

        if (GUILayout.Button("Process ALL Scenes in Assets", GUILayout.Height(35)))
        {
            ProcessAllScenes();
        }

        GUI.enabled = true;
    }

    private void ProcessCurrentScene()
    {
        if (jobsPrefab == null)
            return;

        Scene scene = SceneManager.GetActiveScene();

        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("No valid loaded scene found.");
            return;
        }

        int count = ProcessScene(scene);

        if (count > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        Debug.Log(
            $"Jobs Inserter: Processed scene '{scene.name}'. " +
            $"Updated {count} stall(s).");
    }

    private void ProcessAllScenes()
    {
        if (jobsPrefab == null)
            return;

        // Protect unsaved changes in currently open scenes.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        string originalScenePath =
            SceneManager.GetActiveScene().path;

        string[] sceneGuids = AssetDatabase.FindAssets(
            "t:Scene",
            new[] { "Assets" });

        List<string> scenePaths = new List<string>();

        foreach (string guid in sceneGuids)
        {
            scenePaths.Add(AssetDatabase.GUIDToAssetPath(guid));
        }

        int totalUpdated = 0;
        int processedScenes = 0;

        try
        {
            foreach (string scenePath in scenePaths)
            {
                Scene scene = EditorSceneManager.OpenScene(
                    scenePath,
                    OpenSceneMode.Single);

                if (!scene.IsValid() || !scene.isLoaded)
                    continue;

                int count = ProcessScene(scene);

                if (count > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);

                    if (EditorSceneManager.SaveScene(scene))
                        totalUpdated += count;
                }

                processedScenes++;
            }
        }
        finally
        {
            // Return to the scene that was active before processing.
            if (!string.IsNullOrEmpty(originalScenePath))
            {
                EditorSceneManager.OpenScene(
                    originalScenePath,
                    OpenSceneMode.Single);
            }
        }

        AssetDatabase.SaveAssets();

        Debug.Log(
            $"Jobs Inserter complete. Processed {processedScenes} " +
            $"scenes and updated {totalUpdated} stalls.");
    }

    private int ProcessScene(Scene scene)
    {
        int updated = 0;

        GameObject[] roots = scene.GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            Transform[] allTransforms =
                root.GetComponentsInChildren<Transform>(true);

            foreach (Transform stall in allTransforms)
            {
                if (!stall.name.StartsWith("Stall "))
                    continue;

                Transform canvas = FindDirectChild(
                    stall, "Interaction Canvas");

                if (canvas == null)
                    continue;

                Transform panel = FindDirectChild(canvas, "Panel");

                if (panel == null)
                    continue;

                Transform pdf = FindDirectChild(panel, "Pdf");
                Transform close = FindDirectChild(panel, "Close");

                if (pdf == null || close == null)
                {
                    Debug.LogWarning(
                        $"Skipped '{stall.name}' in '{scene.name}': " +
                        "Pdf or Close was not found under Panel.",
                        stall.gameObject);
                    continue;
                }

                Transform jobs = FindDirectChild(panel, "Jobs");

                // Reuse an existing Jobs object instead of duplicating it.
                if (jobs == null)
                {
                    GameObject instance =
                        (GameObject)PrefabUtility.InstantiatePrefab(
                            jobsPrefab, scene);

                    if (instance == null)
                    {
                        Debug.LogError(
                            $"Failed to instantiate Jobs for {stall.name}.");
                        continue;
                    }

                    Undo.RegisterCreatedObjectUndo(
                        instance, "Insert Jobs Prefab");

                    instance.name = "Jobs";
                    instance.transform.SetParent(panel, false);
                    jobs = instance.transform;
                }

                // Put Jobs directly after Pdf.
                jobs.SetSiblingIndex(pdf.GetSiblingIndex() + 1);

                // Reset the UI transform.
                RectTransform rect =
                    jobs.GetComponent<RectTransform>();

                if (rect != null)
                {
                    rect.anchoredPosition3D = Vector3.zero;
                    rect.localPosition = Vector3.zero;
                    rect.localRotation = Quaternion.identity;
                    rect.localScale = Vector3.one;
                }
                else
                {
                    Debug.LogWarning(
                        $"Jobs under {stall.name} has no RectTransform.",
                        jobs.gameObject);
                }

                // Confirm that Jobs is before Close.
                if (jobs.GetSiblingIndex() >= close.GetSiblingIndex())
                {
                    jobs.SetSiblingIndex(close.GetSiblingIndex());
                }

                EditorUtility.SetDirty(jobs.gameObject);
                updated++;
            }
        }

        return updated;
    }

    private static Transform FindDirectChild(
        Transform parent, string childName)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name == childName)
                return child;
        }

        return null;
    }
}
