using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BlockEditor : MonoBehaviour
{
    public static BlockEditor Instance;
    public int width;
    public int height;
    private void Awake()
    {
        Instance = this;
        HexBlockContainer.InitHexBlockContainerMatrix(width, height);
    }
#if UNITY_EDITOR
    [Button]
    public void MakeBlockContainer()
    {
        HexBlockContainer[] existingBlockContainers = FindObjectsOfType<HexBlockContainer>();
        foreach (HexBlockContainer block in existingBlockContainers)
        {
            Undo.DestroyObjectImmediate(block.gameObject);
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if ((i + j) % 2 == 1)
                {
                    continue;
                }

                // Addressables를 사용하여 프리팹 로드
                GameObject blockPrefab = Addressables.LoadAssetAsync<GameObject>(EPrefab.HexBlockContainer.OriginName()).WaitForCompletion();

                // 프리팹 인스턴스화
                GameObject blockObj = (GameObject)PrefabUtility.InstantiatePrefab(blockPrefab, transform);

                // HexBlock 컴포넌트 가져오기
                HexBlockContainer block = blockObj.GetComponent<HexBlockContainer>();
                if (block != null)
                {
                    block.EditorInit(i, j);
                    block.gameObject.name = $"HexBlockContainer({i},{j})";
                    Undo.RegisterCreatedObjectUndo(block.gameObject, "Create HexBlockContainer");
                }
            }
        }
    }
#endif
}
