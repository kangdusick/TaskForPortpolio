using UnityEngine;
using UnityEditor;
using System.IO;

public class RemoveRigidbodiesFromPrefabs : Editor
{
    [MenuItem("Tools/Remove Rigidbodies from Prefabs")]
    private static void RemoveRigidbodies()
    {
        // 폴더 경로 설정
        string folderPath = "Assets/1_kds";
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab", new string[] { folderPath });

        // 해당 폴더의 모든 프리팹 검색
        int removedCount = 0;

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(prefab.name== "ChemicalOrc")
            {
                Debug.Log("ads");
            }
            // 프리팹 로드
            if (prefab != null)
            {
                // 프리팹의 부모 Transform에서 Rigidbody 컴포넌트 검색 및 제거
                Rigidbody2D rb = prefab.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // 변경 사항을 적용하기 위해 Prefab 모드로 열기
                    string prefabAssetPath = AssetDatabase.GetAssetPath(prefab);
                    GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabAssetPath);

                    // Rigidbody 컴포넌트 제거
                    Rigidbody2D rbInstance = prefabInstance.GetComponent<Rigidbody2D>();
                    if (rbInstance != null)
                    {
                        Undo.DestroyObjectImmediate(rbInstance);
                        PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabAssetPath);
                        removedCount++;
                    }

                    // Prefab 모드 종료
                    PrefabUtility.UnloadPrefabContents(prefabInstance);
                }
            }
        }

        // 결과 로깅
        Debug.Log($"Removed {removedCount} Rigidbody components from prefabs in '{folderPath}'");
        AssetDatabase.SaveAssets(); // 모든 변경 사항 저장
        AssetDatabase.Refresh();   // 에디터 새로고침
    }
}
