using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class RemoveMissingScripts : Editor
{
    [MenuItem("Tools/Remove All Missing Script Components")]
    private static void RemoveAllMissingScriptComponentssadf()
    {

        Object[] deepSelectedObjects = EditorUtility.CollectDeepHierarchy(Selection.gameObjects);

        Debug.Log(deepSelectedObjects.Length);

        int componentCount = 0;
        int gameObjectCount = 0;

        foreach (Object obj in deepSelectedObjects)
        {
            if (obj is GameObject go)
            {
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);

                //Debug.LogFormat("<color=cyan>{0}</color>", count);

                if (count > 0)
                {
                    Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");

                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

                    componentCount += count;
                    gameObjectCount++;
                }

            }
        }

    }
    [MenuItem("Tools/Remove Missing Scripts in Current Scene and 1_kds Prefabs")]
    private static void RemoveAllMissingScriptComponents()
    {
        string folderPath = "Assets/1_kds";
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new string[] { folderPath });

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            DeleteMissingScript(prefab);
        }


        GameObject[] textsInScene = FindObjectsOfType<GameObject>(true);
        bool sceneModified = false;
        foreach (GameObject text in textsInScene)
        {
            DeleteMissingScript(text);
        }

        // 씬 변경사항이 있으면 씬을 저장합니다.
        if (sceneModified)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }
    }
    private static void DeleteMissingScript(GameObject gameObject)
    {
        GameObject[] rootObjects = new GameObject[] { gameObject };

        int componentCount = 0;
        int gameObjectCount = 0;

        foreach (GameObject go in rootObjects)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);

            if (count > 0)
            {
                Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");

                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

                componentCount += count;
                gameObjectCount++;
            }
        }

        Debug.Log($"Removed {componentCount} missing script components from {gameObjectCount} game objects.");
    }
}
