#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class TMPLinkDetectorSetup : Editor
{
    [MenuItem("Tools/Add TMPLinkDetector to TMP Texts in 1_kds")]
    public static void AddTMPLinkDetectorToTMPTextsIn1Kds()
    {
        string folderPath = "Assets/1_kds";
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new string[] { folderPath });

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab.name == "DamageText" || prefab.name == "MoneyText" || prefab.name == "ToastMessage" || prefab.name == "TutorialManager")
                continue;

            GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            TMP_Text[] textsInPrefab = prefabInstance.GetComponentsInChildren<TMP_Text>(true);
            bool modified = false;

            foreach (TMP_Text text in textsInPrefab)
            {
                if (AddTMPLinkDetector(text.gameObject))
                {
                    modified = true;
                }
            }

            if (modified)
            {
                // 프리펩 변경사항을 저장합니다.
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
            }

            DestroyImmediate(prefabInstance);
        }

        // 씬 내의 모든 TMP_Text 컴포넌트 찾기 및 변경사항 적용
        TMP_Text[] textsInScene = FindObjectsOfType<TMP_Text>(true);
        bool sceneModified = false;
        foreach (TMP_Text text in textsInScene)
        {
            if (AddTMPLinkDetector(text.gameObject))
            {
                sceneModified = true;
            }
        }

        // 씬 변경사항이 있으면 씬을 저장합니다.
        if (sceneModified)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }
    }
   

    private static bool AddTMPLinkDetector(GameObject obj)
    {
        if (obj.GetComponent<TMPLinkDetector>() == null)
        {
            obj.AddComponent<TMPLinkDetector>();
            return true;
        }
        return false;
    }
}
#endif