using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using SRF;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class CombinationGenerator<T>
{
    List<T> list;
    int length;
    public CombinationGenerator(List<T> list, int length)
    {
        this.list = list;
        this.length = length;
    }
    public IEnumerable<List<T>> GetCombinations()
    {
        return GetCombinations(0, new List<T>());
    }
    private IEnumerable<List<T>> GetCombinations(int startIndex, List<T> combination)
    {
        if (combination.Count == length)
        {
            yield return new List<T>(combination);
            yield break;
        }
        for (int i = startIndex; i < list.Count; i++)
        {
            combination.Add(list[i]);
            foreach (var item in GetCombinations(i + 1, combination))
            {
                yield return combination;
            }
            combination.RemoveAt(combination.Count - 1);
        }

    }
}
class UnionFind
{
    int[] parent;
    int[] rank;
    int[] size;

    public UnionFind(int n)
    {
        parent = new int[n];
        rank = new int[n];
        size = new int[n];
        for (int i = 0; i < n; i++)
        {
            parent[i] = i;
            size[i] = 1;
        }
    }

    public int Find(int x)
    {
        if (parent[x] != x)
        {
            parent[x] = Find(parent[x]);
        }
        return parent[x];
    }

    public void Union(int x, int y)
    {
        int rootX = Find(x);
        int rootY = Find(y);

        if (rootX == rootY) return;

        if (rank[rootX] < rank[rootY])
        {
            parent[rootX] = rootY;
            size[rootY] += size[rootX];
        }
        else if (rank[rootX] > rank[rootY])
        {
            parent[rootY] = rootX;
            size[rootX] += size[rootY];
        }
        else
        {
            parent[rootY] = rootX;
            rank[rootX]++;
            size[rootX] += size[rootY];
        }
    }

    public List<int> GetUnionList(int x)
    {
        var list = new List<int>();
        var rootx = Find(x);
        for (int i = 0; i < parent.Length; i++)
        {
            if (Find(parent[i]) == rootx)
            {
                list.Add(i);
            }
        }
        return list;
    }
    public int GetSize(int x)
    {
        int rootx = Find(x);
        return size[rootx];
    }
}
public class HexBlockLine
{
    public int a; //y = ax + b
    public int b;
    public List<HexBlock> hexBlockList = new();
    public HexBlockLine(int a, int b)
    {
        this.a = a;
        this.b = b;
    }
}
public enum EDirIndex
{
    Down = 0,
    Up = 1,
    RightDown = 2,
    LeftUp = 3,
    RightUp = 4,
    LeftDown = 5,
}
public class HexBlockContainer : CustomColliderMonobehaviour
{
    private static HexBlockContainer touchDownedHexBlockContainer;
    public static HexBlockContainer[,] hexBlockContainerMatrix;
    private static List<HexBlockContainer> highestHexBlockContainerEachColumnList;
    public static readonly List<EColor> EColorList = new List<EColor>() { EColor.blue, EColor.green, EColor.purple, EColor.red, EColor.orange, EColor.yellow };
    public readonly static (int x, int y)[] dirList = new (int x, int y)[6] { (0, 2), (0, -2), (1, 1), (-1, -1), (1, -1), (-1, 1) };
    public HexBlock hexBlock;
    [SerializeField] Image hintEffectImage;
    public int x;
    public int y;
    const int entranceIndexX = 3;
    const int entranceIndexY = 1;
    public const float hexWidth = 100f;
    public const float hexHeight = 88.28125f;
    const float exchangeMoveSpeed = 400f;
    const float newBlockMoveSpeed = 600f;
    private bool _isTouchStart;
    public static bool IsWhileExChange { get; private set; }
    private bool IsTouchStart
    {
        get { return _isTouchStart; }
        set
        {
            _isTouchStart = value;
            if (_isTouchStart)
            {
                touchDownedHexBlockContainer = this;
            }
            else
            {
                touchDownedHexBlockContainer = null;
            }
        }
    }
    public static bool IsAllBlockGenerated
    {
        get 
        {
            foreach (var item in CollisionDetectManager.Instance.hexBlockContainerList)
            {
                if(ReferenceEquals(item.hexBlock,null))
                {
                    return false;
                }
            }
            return true;
        }
    }
    public void EnableHintEffect(bool isEnableHintEffect)
    {
        hintEffectImage.enabled = isEnableHintEffect;
    }
    
    public static HexBlockContainer GetTopEmptyHexBlockContainerInSameColum(int xIndex)
    {
        HexBlockContainer topContainer = null;
        bool isEmptyContainerExist = false;
        for (int j = hexBlockContainerMatrix.GetLength(1) - 1; j >= 0; j--)
        {
            if (!ReferenceEquals(hexBlockContainerMatrix[xIndex, j], null))
            {
                topContainer = hexBlockContainerMatrix[xIndex, j];
                break;
            }
        }
        for (int j = hexBlockContainerMatrix.GetLength(1) - 1; j >= 0; j--)
        {
            if (!ReferenceEquals(hexBlockContainerMatrix[xIndex, j], null) && topContainer.y >= hexBlockContainerMatrix[xIndex, j].y && ReferenceEquals(hexBlockContainerMatrix[xIndex, j].hexBlock, null))
            {
                topContainer = hexBlockContainerMatrix[xIndex, j];
                isEmptyContainerExist = true;
                break;
            }
        }
        if (!isEmptyContainerExist)
        {
            topContainer = null;
        }
        return topContainer;
    }
    public static HexBlockContainer GetEmptyUpperHexBlockContainerInSameColum(HexBlockContainer emptyBlockContainer) //빈 블럭 위에 떠있는 가장 가까운 블럭 찾기
    {
        HexBlockContainer upperEmptyBlock = null;
        var xIndex = emptyBlockContainer.x;
        for (int j = emptyBlockContainer.y - 1; j >= 0; j--)
        {
            if (!ReferenceEquals(hexBlockContainerMatrix[xIndex, j], null) && !ReferenceEquals(hexBlockContainerMatrix[xIndex, j].hexBlock, null))
            {
                upperEmptyBlock = hexBlockContainerMatrix[xIndex, j];
                break;
            }
        }
        return upperEmptyBlock;
    }
    public static void InitHexBlockContainerMatrix(int width, int height)
    {
        hexBlockContainerMatrix = new HexBlockContainer[width, height];
        highestHexBlockContainerEachColumnList = new();
        var hexBlockContainerList = GameObject.FindGameObjectsWithTag(ETag.HexBlockContainer.ToString());
        foreach (var hexBlockContainerGo in hexBlockContainerList)
        {
            if (hexBlockContainerGo.activeInHierarchy)
            {
                var hexBlockContainer = hexBlockContainerGo.GetComponent<HexBlockContainer>();
                hexBlockContainerMatrix[hexBlockContainer.x, hexBlockContainer.y] = hexBlockContainer;

                if (!highestHexBlockContainerEachColumnList.Any(hsdf => hsdf.x == hexBlockContainer.x))
                {
                    highestHexBlockContainerEachColumnList.Add(hexBlockContainer);
                }
                else
                {
                    var highestContainer = highestHexBlockContainerEachColumnList.Find(h => h.x == hexBlockContainer.x);
                    var highestContainerIndex = highestHexBlockContainerEachColumnList.IndexOf(highestContainer);
                    if (highestContainer.y > hexBlockContainer.y)//y좌표가 작을수록 높이 있는 블럭이다.
                    {
                        highestHexBlockContainerEachColumnList[highestContainerIndex] = hexBlockContainer;
                    }
                }
            }
            
        }

        // 입구로부터 멀수록 앞에 오도록 정렬, 같다면 x가 작을수록 앞에 오도록 정렬
        highestHexBlockContainerEachColumnList.Sort(new Comparison<HexBlockContainer>((x, y) =>
        {
            int comparison = (math.abs(y.x - entranceIndexX)).CompareTo(math.abs(x.x - entranceIndexX));
            if (comparison == 0)
            {
                return x.x.CompareTo(y.x);
            }
            return comparison;
        }));
    }

    private void Start()
    {
        ReplacePresetToPoolingObject();
        AddTouchListener();
        IsTouchStart = false;
        IsWhileExChange = false;
    }
    public static int GetHintPoint(List<HexBlockContainer> changeCombination)
    {
       
        var matchCaseList = FindMatchCaseList(changeCombination);
        int hintPoint = 0; //포인트가 높을수록 좋은 자리이다. 높은 점수의 힌트를 줄것
        for (int i = 0; i < matchCaseList.Count; i++)
        {
            var matchCase = matchCaseList[i];
            hintPoint += matchCase.destroyBlockSet.Count;

            //아이템 점수 체크
            if (matchCase.itemType != EBlockType.Empty)
            {
                hintPoint += (int)matchCase.itemType;
            }
        }
        return hintPoint;
    }
    public static List<HexBlockContainer> GetHintExchangePoint()
    {
        List<HexBlockContainer> hintList = new();

        foreach (var item in CollisionDetectManager.Instance.hexBlockContainerList) //합칠수있는아이템 존재
        {
            if (!ReferenceEquals(item.hexBlock, null) && item.hexBlock.eBlockType.ToString().Contains("item"))
            {
                var neighborList = GetNeighborContainerBlockList(item);
                foreach (var neighbor in neighborList)
                {
                    if (!ReferenceEquals(neighbor.hexBlock, null) && neighbor.hexBlock.eBlockType.ToString().Contains("item"))
                    {
                        hintList.Add(item);
                        hintList.Add(neighbor);
                        return hintList;
                    }
                }

            }
        }

        int maxHintPoint = 0;
        //모든 블럭에서 조합을 만든다. 색상과 타입이 모두 같지 않다면 뒤집어보고 파괴된 블럭이 존재하는지 체크한다.
        var combinationGen = new CombinationGenerator<HexBlockContainer>(CollisionDetectManager.Instance.hexBlockContainerList.ToList(), 2);
        foreach (var changeCombination in combinationGen.GetCombinations())
        {
            var containerA = changeCombination[0];
            var containerB = changeCombination[1];
            var hexblockA = containerA.hexBlock;
            var hexblockB = containerB.hexBlock;
            if (hexblockA.eColor == hexblockB.eColor || !IsNeighbor(hexblockA, hexblockB))
            {
                continue;
            }

            hexblockA.ChangeHexBlockContainer(containerB);
            hexblockB.ChangeHexBlockContainer(containerA);

            int hintPoint =  GetHintPoint(changeCombination); //포인트가 높을수록 좋은 자리이다. 높은 점수의 힌트를 줄것
            if (maxHintPoint < hintPoint)
            {
                maxHintPoint = hintPoint;
                hintList.Clear();
                hintList.Add(containerA);
                hintList.Add(containerB);
            }

            hexblockA.ChangeHexBlockContainer(containerA);
            hexblockB.ChangeHexBlockContainer(containerB);
        }
        return hintList;
    }
    private void ReplacePresetToPoolingObject()
    {
        var hexBlockPresetColor = hexBlock.eColor;
        var hexBlockPresetType = hexBlock.eBlockType;
        Destroy(hexBlock.gameObject); //프리셋으로 저장했던거 파괴 후 오브젝트 풀링으로 재생성
        PoolableManager.Instance.InstantiateAsync<HexBlock>(EPrefab.HexBlock, transform.position, Vector3.one, parentTransform: BlockEditor.Instance.transform).ContinueWithNullCheck(x =>
        {
            x.Init(hexBlockPresetColor, hexBlockPresetType);
            x.SetHexBlockContainerWithMove(this, 1000f);
        });
    }
    private void AddTouchListener()
    {
        collisionGroup.OnTouchDown(OnTouchDown);
        collisionGroup.OnTouching(OnTouching);
        TouchManager.Instance.OnTouchUp.AddListener(this, OnTouchUp);
    }
    public void EditorInit(int x, int y)
    {
        this.x = x;
        this.y = y;
        transform.position = new Vector3(x * hexWidth * 0.75f, -y * hexHeight * 0.5f, 0f);
    }
#if UNITY_EDITOR
    [Button]
    private void MakeHexBlockInEditor(EColor eColor, EBlockType eBlockType)
    {
        if (hexBlock != null)
        {
            Undo.DestroyObjectImmediate(hexBlock.gameObject);
        }

        // Addressables를 사용하여 프리팹 로드
        GameObject blockPrefab = Addressables.LoadAssetAsync<GameObject>(EPrefab.HexBlock.OriginName()).WaitForCompletion();

        // 프리팹 인스턴스화
        GameObject blockObj = (GameObject)PrefabUtility.InstantiatePrefab(blockPrefab, GameObject.FindGameObjectWithTag(ETag.BlockEditor.ToString()).transform);

        // HexBlock 컴포넌트 가져오기
        hexBlock = blockObj.GetComponent<HexBlock>();
        if (hexBlock != null)
        {
            hexBlock.transform.position = transform.position;
            hexBlock.Init(eColor, eBlockType);
            hexBlock.SetHexBlockContainerWithMove(this, 1000f);
            Undo.RegisterCreatedObjectUndo(hexBlock.gameObject, "Create HexBlock");
        }
    }
#endif
    private void OnTouchDown()
    {
        if (!ReferenceEquals(touchDownedHexBlockContainer, null) || IsWhileExChange || ReferenceEquals(hexBlock, null))
        {
            return;
        }
        IsTouchStart = true;
    }
    private void OnTouching()
    {
        if (ReferenceEquals(touchDownedHexBlockContainer, null) || IsWhileExChange || ReferenceEquals(hexBlock, null))
        {
            return;
        }
        GameManager.Instance.DisableAllHintEffect();
        if (!ReferenceEquals(touchDownedHexBlockContainer, this))
        {
            ExchangeHexBlock(touchDownedHexBlockContainer.hexBlock, hexBlock);
        }
    }
    private void OnTouchUp(Vector2 screenPos)
    {
        IsTouchStart = false;
    }

    private async UniTask<int> OnMoveBlock(List<HexBlockContainer> movedHexContainerList, int sumOfTotalDestroyBlockCnt)
    {
        IsWhileExChange = true;
        List<UniTask> moveTaskeList = new();
        var movedHexContainerColorList = new List<EColor>();
        for (int i = 0; i < movedHexContainerList.Count; i++)
        {
            movedHexContainerColorList.Add(movedHexContainerList[i].hexBlock.eColor);
        }
        var matchCaseList = FindMatchCaseList(movedHexContainerList);
        int totalDestroyBlockCnt = 0;
        for (int i = 0; i < matchCaseList.Count; i++)
        {
            var matchCase = matchCaseList[i];
            totalDestroyBlockCnt += matchCase.destroyBlockSet.Count;

            HashSet<HexBlock> damagedTopBlockSet = new(); //매치된 블럭 근처에 팽이가 있으면 데미지를 준다.
            foreach (var destroyedBlock in matchCase.destroyBlockSet)
            {
                if(ReferenceEquals(destroyedBlock.hexBlockContainer,null))
                {
                    continue;
                }
                foreach (var item in GetNeighborContainerBlockList(destroyedBlock.hexBlockContainer))
                {
                    if(ReferenceEquals(item.hexBlock,null))
                    {
                        continue;
                    }
                    if(item.hexBlock.eBlockType == EBlockType.top)
                    {
                        damagedTopBlockSet.Add(item.hexBlock);
                    }
                } 
            }
            foreach (var item in damagedTopBlockSet)
            {
                await item.Damaged();
            }

            //새로운 아이템 만들기
            if (matchCase.itemType != EBlockType.Empty)
            {
                HexBlockContainer itemContainer = null;
                if (movedHexContainerList.Count == 2)//직접 옮겨서 아이템이 생성된 경우
                {
                    itemContainer = movedHexContainerList[movedHexContainerColorList.IndexOf(matchCase.itemColor)];

                }
                else //새로 생성되고 이동한 블럭들이 자동으로 머지된 경우
                {
                    itemContainer = matchCase.destroyBlockSet.RandomElement().hexBlockContainer;
                }

                foreach (var hexBlock in matchCase.destroyBlockSet)
                {
                    moveTaskeList.Add(hexBlock.Merge(itemContainer.transform.position));
                }
                await UniTask.WhenAll(moveTaskeList);
                moveTaskeList.Clear();
                var makedItem = PoolableManager.Instance.Instantiate<HexBlock>(EPrefab.HexBlock, itemContainer.transform.position, parentTransform: BlockEditor.Instance.transform);
                makedItem.Init(matchCase.itemColor, matchCase.itemType);
                makedItem.ChangeHexBlockContainer(itemContainer);
            }
            else
            {
                foreach (var hexBlock in matchCase.destroyBlockSet)
                {
                    await hexBlock.Damaged();
                }
            }
        }
        await SortBlocksAndGenerateNewBlocks();

        sumOfTotalDestroyBlockCnt += totalDestroyBlockCnt;
        if (totalDestroyBlockCnt > 0)
        {
            sumOfTotalDestroyBlockCnt += await OnMoveBlock(CollisionDetectManager.Instance.hexBlockContainerList.ToList(), sumOfTotalDestroyBlockCnt);
        }

        IsWhileExChange = false;
        return sumOfTotalDestroyBlockCnt;
    }
    private async UniTask SortBlocksAndGenerateNewBlocks()
    {
        IsWhileExChange = true;
        List<int> GetExistEmptyColumnList()
        {
            List<int> existEmptyColumnList = new List<int>();
            for (int i = 0; i < BlockEditor.Instance.width; i++)
            {
                var emptyTop = GetTopEmptyHexBlockContainerInSameColum(i);
                if (ReferenceEquals(emptyTop, null)) //빈공간이 없다
                {
                    continue;
                }
                else
                {
                    existEmptyColumnList.Add(i);
                }
            }
            return existEmptyColumnList;
        }
        List<int> existEmptyColumnList = GetExistEmptyColumnList();

        //기존 블럭이 동시에 흘러 내려가기
        //x=0, y =  11부터 시작해서 만약 아래칸이 비어있다면, 비어있지 않은 칸을 찾을때까지 수직아아래로 내려가기.
        var moveTaskeList = new List<UniTask>();
        foreach (var emptyXIndex in existEmptyColumnList)
        {
            while (true)
            {
                var emptyTop = GetTopEmptyHexBlockContainerInSameColum(emptyXIndex);
                var nextBlock = GetEmptyUpperHexBlockContainerInSameColum(emptyTop);
                if (ReferenceEquals(nextBlock, null))
                {
                    break;
                }
                moveTaskeList.Add(nextBlock.hexBlock.SetHexBlockContainerWithMove(emptyTop, 0.2f, isTimeBase: true));
            }
        }
        await UniTask.WhenAll(moveTaskeList);
        moveTaskeList.Clear();
        //입구에서 흘러나오는 블럭에 방해되는 블럭들이 있다면, 해당 방향으로 떨어진다.
        for (int i = 0; i < highestHexBlockContainerEachColumnList.Count; i++)
        {
            if (ReferenceEquals(highestHexBlockContainerEachColumnList[i].hexBlock, null))
            {
                continue;
            }
            existEmptyColumnList = GetExistEmptyColumnList();
            List<int> blockedEmptySpaceList = new();
            foreach (var emptyColumnIndex in existEmptyColumnList) //입구에서 빈공간의 방향벡터와 입구에서 장애물의 방향벡터가 같고, 입구에서 장애물까지의 거리보다 입구에서 빈공간까지의 거리가 더 긴 경우를 모두 찾는다.
            {
                var vector_EmptySpace = entranceIndexX - emptyColumnIndex;
                var vectorObstacle = entranceIndexX - highestHexBlockContainerEachColumnList[i].x;
                if (vector_EmptySpace * vectorObstacle >= 0 && math.abs(vector_EmptySpace) > math.abs(vectorObstacle))  //방향벡터 곱이 양수면 같은 방향이다.
                {
                    blockedEmptySpaceList.Add(emptyColumnIndex); // highestHexBlockContainerEachColumnList[i]가 emptyColumnIndex로 가는 길을 막고있다.
                }
            }
            if (blockedEmptySpaceList.Count == 0)
            {
                continue;
            }
            HexBlockContainer lowestEmptyPositionContainer = highestHexBlockContainerEachColumnList[i];
            foreach (var emptySpaceXIndex in blockedEmptySpaceList) // highestHexBlockContainerEachColumnList[i]가 막고있는 빈공간 중에 가장 블럭이 낮게 쌓여있는 위치로 이동
            {
                var topEmptyHexBlockContainer = GetTopEmptyHexBlockContainerInSameColum(emptySpaceXIndex); //null인경우 빈공간이 없다는 뜻
                if (topEmptyHexBlockContainer.y > lowestEmptyPositionContainer.y) //y가 클수록 더 낮은 위치에 있다.
                {
                    lowestEmptyPositionContainer = topEmptyHexBlockContainer;
                }
            }
            moveTaskeList.Add(highestHexBlockContainerEachColumnList[i].hexBlock.SetHexBlockContainerWithMove(lowestEmptyPositionContainer, newBlockMoveSpeed));
        }

        //새로운 블럭 생성 후 흘러내려가기, 생성순서: y가 가장 큰(가장 블럭이 낮게 쌓여있는)곳, 높이가 같은게 있다면 입구에서 가까운곳부터 먼저 블럭이 생성되어 이동한다.
        var spawnPos = hexBlockContainerMatrix[entranceIndexX, entranceIndexY].transform.position + Vector3.up * hexHeight;
        var spawnedNewNormalBlockList = new List<HexBlock>();
        var spawnedNewNormalBlockContainerList = new List<HexBlockContainer>();
        while (true)
        {
            //await UniTask.DelayFrame(1);
            HexBlockContainer lowestEmptyPositionContainer = null;
            existEmptyColumnList = GetExistEmptyColumnList();
            if (existEmptyColumnList.Count == 0) //이제 빈공간이 없다.
            {
                if (GetHintExchangePoint().Count == 0) //더이상 바꿀수있는 블럭이 없다면 다시 블럭 생성
                {
                    foreach (var spawnedBlock in spawnedNewNormalBlockList)
                    {
                        spawnedBlock.Destroy();
                    }
                    spawnedNewNormalBlockList.Clear();
                    spawnedNewNormalBlockContainerList.Clear();
                    Debug.Log("바꿀 수 있는 블럭이 없어서 다시 랜덤 돌림");
                    continue;
                }
                else
                {
                    Debug.Log("블럭 새로 생성 완료");
                    break;
                }
            }
            foreach (var emptyColumnXIndex in existEmptyColumnList)
            {
                var emptyTop = GetTopEmptyHexBlockContainerInSameColum(emptyColumnXIndex);
                //해당 위치로 이동할 수 있는지 확인(해당 위치로 가는 길목의 emptyTop이 다음 x좌표의 emptyTop의 y인덱스보다 큰게 하나라도 있으면 이동 못한다)
                int moveVectorX = emptyColumnXIndex - entranceIndexX;
                int checkXiteration = entranceIndexX;
                int checkYIndex = entranceIndexY;
                bool isCantMoveThere = false;
                if (moveVectorX != 0)
                {
                    do
                    {
                        var checkEmptyTop = GetTopEmptyHexBlockContainerInSameColum(checkXiteration);
                        if (ReferenceEquals(checkEmptyTop, null) || checkEmptyTop.y < checkYIndex)
                        {
                            isCantMoveThere = true;
                            break;
                        }
                        checkYIndex = checkEmptyTop.y;
                        checkXiteration = moveVectorX > 0 ? checkXiteration + 1 : checkXiteration - 1;

                    } while (checkXiteration != emptyColumnXIndex);
                }
                if (isCantMoveThere)
                {
                    continue;
                }

                if (ReferenceEquals(lowestEmptyPositionContainer, null))
                {
                    lowestEmptyPositionContainer = emptyTop;
                }
                else
                {
                    if (lowestEmptyPositionContainer.y == emptyTop.y)  //높이가 같은게 있다면 입구에서 X좌표가 가까운곳부터 먼저 블럭이 생성되어 이동한다.
                    {
                        var distanceFromSpawnPosA = math.abs(lowestEmptyPositionContainer.x - entranceIndexX);
                        var distanceFromSpawnPosB = math.abs(emptyTop.x - entranceIndexX);
                        if (distanceFromSpawnPosA > distanceFromSpawnPosB)
                        {
                            lowestEmptyPositionContainer = emptyTop;
                        }
                    }
                    else if (lowestEmptyPositionContainer.y < emptyTop.y)
                    {
                        lowestEmptyPositionContainer = emptyTop;

                    }
                }
            }
            var spawnedHexBlock = PoolableManager.Instance.Instantiate<HexBlock>(EPrefab.HexBlock, spawnPos, parentTransform: BlockEditor.Instance.transform);
            spawnedNewNormalBlockList.Add(spawnedHexBlock);
            spawnedNewNormalBlockContainerList.Add(lowestEmptyPositionContainer);
            spawnedHexBlock.Init(EColorList.Random(), EBlockType.normal);
            spawnedHexBlock.ChangeHexBlockContainer(lowestEmptyPositionContainer);
        }
        foreach (var spawnedHexBlock in spawnedNewNormalBlockList)
        {
            spawnedHexBlock.ChangeHexBlockContainer(null);
        }
        for (int i = 0; i < spawnedNewNormalBlockList.Count; i++)
        {
            var container = spawnedNewNormalBlockContainerList[i];
            var spawnedHexBlock = spawnedNewNormalBlockList[i];
            moveTaskeList.Add(spawnedHexBlock.SetHexBlockContainerWithMove(container, newBlockMoveSpeed));
            await UniTask.Delay(Mathf.CeilToInt(1000f * hexHeight / newBlockMoveSpeed));
        }


        await UniTask.WhenAll(moveTaskeList);
    }
    public async void ExchangeHexBlock(HexBlock hexBlockA, HexBlock hexBlockB)
    {
        if (!IsNeighbor(hexBlockA, hexBlockB))
        {
            return;
        }
        var hexBlockContainerACash = hexBlockA.hexBlockContainer;
        var hexBlockContainerBCash = hexBlockB.hexBlockContainer;

        IsWhileExChange = true;
        foreach (var hexBlockContainer in CollisionDetectManager.Instance.hexBlockContainerList)
        {
            hexBlockContainer.OnTouchUp(TouchManager.Instance.mouseWorldPos);
        }

        int totalDestroyBlockCnt = 0;
        if (hexBlockA.eBlockType.ToString().Contains("item") && hexBlockB.eBlockType.ToString().Contains("item"))
        {
            await hexBlockB.MergeItems(hexBlockA);
            await UniTask.Delay(100);
            IsWhileExChange = true;
            await SortBlocksAndGenerateNewBlocks();
            await UniTask.Delay(100);
            IsWhileExChange = true;
            await OnMoveBlock(CollisionDetectManager.Instance.hexBlockContainerList.ToList(), 0);
            IsWhileExChange = true;
            totalDestroyBlockCnt = 1;
        }
        else
        {
            await UniTask.WhenAll(hexBlockA.SetHexBlockContainerWithMove(hexBlockContainerBCash, exchangeMoveSpeed, true), hexBlockB.SetHexBlockContainerWithMove(hexBlockContainerACash, exchangeMoveSpeed, true));
            var movedHexContainerList = new List<HexBlockContainer>() { hexBlockA.hexBlockContainer, hexBlockB.hexBlockContainer };
            totalDestroyBlockCnt = await OnMoveBlock(movedHexContainerList, 0);
        }

        if (totalDestroyBlockCnt == 0) //파괴된 블럭이 없으면 다시 제자리로 복귀
        {
            await UniTask.WhenAll(hexBlockA.SetHexBlockContainerWithMove(hexBlockContainerACash, exchangeMoveSpeed, true), hexBlockB.SetHexBlockContainerWithMove(hexBlockContainerBCash, exchangeMoveSpeed, true));
        }
        else
        {
            if (GameManager.Instance.LeftTopCnt <= 0)
            {
                IsWhileExChange = true;
                //남아있는 아이템 전부 파괴, 남은 이동횟수만큼 랜덤 로켓 추가, 로켓 전부 파괴
                async UniTask UseAllItemBlocks()
                {
                    IsWhileExChange = true;
                    var damagedDelay = new List<UniTask>();
                    foreach (var item in CollisionDetectManager.Instance.hexBlockContainerList)
                    {
                        if (!ReferenceEquals(item.hexBlock, null) && item.hexBlock.eBlockType.ToString().Contains("item"))
                        {
                            damagedDelay.Add(item.hexBlock.Damaged());
                        }
                    }
                    await UniTask.WhenAll(damagedDelay);
                }
                //레벨 클리어 딤처리
                GameManager.Instance.EnableGameClearText(true);
                await UniTask.Delay(1000);
                GameManager.Instance.EnableGameClearText(false);
                await UseAllItemBlocks();
                await SortBlocksAndGenerateNewBlocks();
                await OnMoveBlock(CollisionDetectManager.Instance.hexBlockContainerList.ToList(), 0);
                IsWhileExChange = true;
                //토이파티 딤처리
                GameManager.Instance.EnableToyPartyText(true);
                await UniTask.Delay(1000);
                var allHexBlockList = HexBlock.GetAllHexBlockList(true);
                while (GameManager.Instance.LeftMoveCnt>0)
                {
                    GameManager.Instance.LeftMoveCnt--;
                    var randBlock = allHexBlockList.Random();
                    allHexBlockList.Remove(randBlock);

                    var projectile = PoolableManager.Instance.Instantiate(EPrefab.Projectile, GameManager.Instance.leftMoveText.transform.position, parentTransform: BlockEditor.Instance.transform);
                    projectile.transform.DOMove(randBlock.transform.position, 0.2f).OnComplete(() =>
                    {
                        randBlock.Init(randBlock.eColor, EBlockType.item_rocket);
                        randBlock.canvas.sortingOrder = 3;
                        PoolableManager.Instance.Destroy(projectile);
                    });
                    await UniTask.Delay(200);
                }
                GameManager.Instance.EnableToyPartyText(false);
                await UseAllItemBlocks();
                await SortBlocksAndGenerateNewBlocks();
                IsWhileExChange = true;
                if(GameManager.isAutoTestMode)
                {
                    GameUtil.Instance.LoadScene("Game");
                }
                else
                {
                    PoolableManager.Instance.Instantiate<PopCommon>(EPrefab.PopCommon).OpenPopup("승리", "모든 팽이를 제거했습니다", () => { GameUtil.Instance.LoadScene("Game"); });
                }
            }
            else
            {
                GameManager.Instance.LeftMoveCnt--;
            }
        }
        IsWhileExChange = false;
    }
    private static List<(HashSet<HexBlock> destroyBlockSet, EColor itemColor, EBlockType itemType)> FindMatchCaseList(List<HexBlockContainer> movedHexBlockContainerList)
    {
        var matchCaseList = new List<(HashSet<HexBlock> destroyBlockSet, EColor itemColor, EBlockType itemType)>();
        UnionFind[] unionFindList = new UnionFind[movedHexBlockContainerList.Count];
        for (int i = 0; i < unionFindList.Length; i++)
        {
            unionFindList[i] = new UnionFind(CollisionDetectManager.Instance.hexBlockContainerList.Count + 1);
        }

        for (int i = 0; i < movedHexBlockContainerList.Count; i++)
        {
            var hexBlockContainer = movedHexBlockContainerList[i];
            Union(hexBlockContainer, unionFindList[i]);
        }
        

        for (int i = 0; i < movedHexBlockContainerList.Count; i++)
        {
            var unionList = unionFindList[i].GetUnionList(CollisionDetectManager.Instance.hexBlockContainerList.IndexOf(movedHexBlockContainerList[i]));
            var matchCase = MatchCase(unionList);
            bool isAlreadyAddedCase = false;
            foreach (var addedMatchCase in matchCaseList) //이미 동일한 매치케이스가 추가됐으면 건너띄기
            {
                if (addedMatchCase.destroyBlockSet.Contains(matchCase.destroyBlockSet.FirstOrDefault()))
                {
                    isAlreadyAddedCase = true;
                    break;
                }
            }
            if(isAlreadyAddedCase)
            {
                continue;
            }
            matchCaseList.Add(matchCase);
        }
        return matchCaseList;
    }
    public static bool IsPointInTriangle(HexBlock p, HexBlock p0, HexBlock p1, HexBlock p2)
    {
        float Area(HexBlock a, HexBlock b, HexBlock c)
        {
            return 0.5f * Math.Abs(a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y));
        }

        float areaTotal = Area(p0, p1, p2);
        float area1 = Area(p, p1, p2);
        float area2 = Area(p0, p, p2);
        float area3 = Area(p0, p1, p);

        return Math.Abs(areaTotal - (area1 + area2 + area3)) < 0.01f;
    }
    private static (HashSet<HexBlock> destroyBlockSet, EColor itemColor, EBlockType itemType) MatchCase(List<int> unionHexBlockContainerIndexList)
    {

        List<HexBlock> GetUnionHexBlockList(List<int> unionHexBlockContainerIndexList)
        {
            var unionHexBlockList = new List<HexBlock>();
            for (int i = 0; i < unionHexBlockContainerIndexList.Count; i++)
            {
                unionHexBlockList.Add(CollisionDetectManager.Instance.hexBlockContainerList[unionHexBlockContainerIndexList[i]].hexBlock);
            }
            return unionHexBlockList;
        }
        List<(int a, int b)> GetEquationList(HexBlock hexBlock)
        {
            var equationList = new List<(int a, int b)>();
            //x = b 형태의 직선함수
            int a = 0;
            int b = hexBlock.x;
            equationList.Add((a, b));

            //y = x + b 형태의 직선함수
            a = 1;
            b = hexBlock.y - hexBlock.x;
            equationList.Add((a, b));


            //y = -x +b 형태의 직선함수
            a = -1;
            b = hexBlock.y + hexBlock.x;
            equationList.Add((a, b));
            return equationList;
        }
        var destroyBlockSet = new HashSet<HexBlock>();

        var unionHexBlockList = GetUnionHexBlockList(unionHexBlockContainerIndexList);


        #region FindLineMatchList 특정 방향으로 일렬로 배열되었는지 확인
        var hexBlockLineList_Upper2 = new List<HexBlockLine>(); //직선의 길이가 2 이상일 때(점이 2개 이상으로 이루어진 직선)
        var hexBlockLineList_Upper4 = new List<HexBlockLine>(); //직선의 길이가 4 이상일 때(점이 3개 이상으로 이루어진 직선)
        Dictionary<(int a, int b), List<HexBlock>> equation_HexblockListDict = new();
        for (int i = 0; i < unionHexBlockList.Count; i++)
        {
            var equationList = GetEquationList(unionHexBlockList[i]);
            foreach (var key in equationList)
            {
                if (!equation_HexblockListDict.ContainsKey(key))
                {
                    equation_HexblockListDict[key] = new List<HexBlock>();
                }
                equation_HexblockListDict[key].Add(unionHexBlockList[i]);
            }
        }

        foreach (var equation_HexblockList in equation_HexblockListDict)
        {
            var lineUnion = new UnionFind(CollisionDetectManager.Instance.hexBlockContainerList.Count + 1);
            var combinationGenerator = new CombinationGenerator<HexBlock>(equation_HexblockList.Value, 2);
            foreach (var combinationHexBlockList in combinationGenerator.GetCombinations())
            {
                var hexBlockA = combinationHexBlockList[0];
                var hexBlockB = combinationHexBlockList[1];
                if (IsNeighbor(hexBlockA, hexBlockB)) //직선에 점의 개수가 2개 이상이고, 모든 점이 이웃이어야 한다. 
                {
                    lineUnion.Union(CollisionDetectManager.Instance.hexBlockContainerList.IndexOf(hexBlockA.hexBlockContainer), CollisionDetectManager.Instance.hexBlockContainerList.IndexOf(hexBlockB.hexBlockContainer));
                }
            }

            var addedLineUnionSet = new HashSet<int>();
            foreach (var hexBlock in equation_HexblockList.Value)
            {
                int blockIndex = CollisionDetectManager.Instance.hexBlockContainerList.IndexOf(hexBlock.hexBlockContainer);
                var nodeCnt = lineUnion.GetSize(blockIndex);
                if (nodeCnt >= 2 && !addedLineUnionSet.Contains(lineUnion.Find(blockIndex)))
                {
                    addedLineUnionSet.Add(lineUnion.Find(blockIndex));

                    var lineUnionHexBlockList = GetUnionHexBlockList(lineUnion.GetUnionList(blockIndex));
                    var hexBlockLine = new HexBlockLine(equation_HexblockList.Key.a, equation_HexblockList.Key.b);
                    hexBlockLine.hexBlockList.AddRange(lineUnionHexBlockList);
                    hexBlockLineList_Upper2.Add(hexBlockLine);

                    if (nodeCnt >= 3)
                    {
                        hexBlockLineList_Upper4.Add(hexBlockLine);
                    }
                }
            }
        }
        #endregion

        #region FindVYMatch TNT와 등껍질이 만들어지는지 확인
        var isVMatchSuccess = false;
        var isYMatchSuccess = false;
        //길이가 3 이상인 직선매치가 2개 이상인 경우 교점을 찾는다
        Dictionary<(int x, int y), List<HexBlockLine>> intersectingCntDict = new(); //key: 2개 이상 직선이 교차하는 점의 좌표 value: 교차하는 각 직선
        var intersectingBlockList2 = new List<HexBlock>(); //직선이 2개 이상 교차하는 블럭
        var intersectingBlockList3 = new List<HexBlock>(); //직선이 3개 이상 교차하는 블럭

        foreach (var hexBlockLine in hexBlockLineList_Upper2)
        {
            if (hexBlockLine.hexBlockList.Count >= 2)
            {
                // 교점을 찾기 위한 블럭 카운트
                foreach (var hexBlock in hexBlockLine.hexBlockList)
                {
                    var key = (hexBlock.x, hexBlock.y);
                    if (!intersectingCntDict.ContainsKey(key))
                    {
                        intersectingCntDict[key] = new List<HexBlockLine>();
                    }
                    intersectingCntDict[key].Add(hexBlockLine);
                }

                // 교점인 블럭 찾기
                foreach (var block in intersectingCntDict)
                {
                    if (block.Value.Count >= 2)
                    {
                        intersectingBlockList2.Add(hexBlockContainerMatrix[block.Key.x, block.Key.y].hexBlock);
                    }
                    if (block.Value.Count >= 3)
                    {
                        intersectingBlockList3.Add(hexBlockContainerMatrix[block.Key.x, block.Key.y].hexBlock);
                    }
                }
            }
        }



        //직선이 2개 이상 교차할 때 교차점에서 각 직선의 끝점 최대 길이가 4이상 2개 이상이면 V매치 성공 (TNT)
        if (intersectingBlockList2.Count > 0)
        {
            foreach (var intersectingHexBlock in intersectingBlockList2)
            {
                var distanceUpper4Cnt = 0;
                foreach (var hexBlockLine in intersectingCntDict[(intersectingHexBlock.x, intersectingHexBlock.y)])
                {
                    var maxDistanceInThisLine = 0;
                    foreach (var hexBlock in hexBlockLine.hexBlockList)
                    {
                        var distance = math.abs((intersectingHexBlock.x - hexBlock.x)) + math.abs((intersectingHexBlock.y - hexBlock.y));
                        maxDistanceInThisLine = math.max(maxDistanceInThisLine, distance);
                    }

                    if (maxDistanceInThisLine >= 4)
                    {
                        distanceUpper4Cnt++;
                        if (distanceUpper4Cnt >= 2)
                        {
                            isVMatchSuccess = true;
                            break;
                        }
                    }
                }
                if (isVMatchSuccess)
                {
                    break;
                }
            }
        }

        //직선이 3개 이상 교차할 때 교차점에서 각 직선의 끝점 최대 길이가 2이상인게 2개 이상이고 길이가 4이상인게 1개 이상이고, 세 점으로 만든 삼각형 내부에 교점이 존재 Y매치 성공(등껍질)
        if (intersectingBlockList3.Count > 0)
        {
            foreach (var intersectingHexBlock in intersectingBlockList3)
            {
                List<HexBlock> distanceUpper2BlockList = new();
                List<HexBlock> distanceUpper4BlockList = new();
                foreach (var hexBlockLine in intersectingCntDict[(intersectingHexBlock.x, intersectingHexBlock.y)])
                {
                    foreach (var hexBlock in hexBlockLine.hexBlockList)
                    {
                        var distance = math.abs((intersectingHexBlock.x - hexBlock.x)) + math.abs((intersectingHexBlock.y - hexBlock.y));
                        if (distance >= 4)
                        {
                            distanceUpper4BlockList.Add(hexBlock);
                        }
                        else if (distance >= 2)
                        {
                            distanceUpper2BlockList.Add(hexBlock);
                        }
                    }
                }

                if (distanceUpper4BlockList.Count + distanceUpper2BlockList.Count >= 3 && distanceUpper4BlockList.Count >= 1)
                {
                    bool isContainsdistanceUpper4Block = false;
                    List<HexBlock> sumBlockList = new();
                    sumBlockList.AddRange(distanceUpper4BlockList);
                    sumBlockList.AddRange(distanceUpper2BlockList);
                    var combinationGenerator = new CombinationGenerator<HexBlock>(sumBlockList, 3); // 3개의 점 조합을 추출한다.
                    foreach (var hexCombinationList in combinationGenerator.GetCombinations())
                    {
                        foreach (var item in hexCombinationList)
                        {
                            if (distanceUpper4BlockList.Contains(item))
                            {
                                isContainsdistanceUpper4Block = true;
                                break;
                            }
                        }
                        if (!isContainsdistanceUpper4Block) //길이가 4 이상인 점이 하나도 없으면 다른 조합으로 넘어간다.
                        {
                            continue;
                        }
                        //길이가 4 이상인 점이 하나, 길이가 2 이상인 점이 2개인게 보장된다.

                        //세 점으로 만든 삼각형 내부에 교점이 존재해야 Y모양이 보장된다.
                        if (IsPointInTriangle(intersectingHexBlock, hexCombinationList[0], hexCombinationList[1], hexCombinationList[2]))
                        {
                            foreach (var hexBlock in hexCombinationList)
                            {
                                destroyBlockSet.Add(hexBlock);
                            }
                            isYMatchSuccess = true;
                            break;
                        }
                    }
                    if (isYMatchSuccess)
                    {
                        break;
                    }
                }
            }
        }
        #endregion

        #region FindRhombusMatch 부메랑 만들어지는지 확인
        bool isBoomerang = false;
        // 마름모 형태 매치 찾기
        (int x, int y)[,] rhombusDir = new (int x, int y)[,]
        {
            { (0 , -2),(1 , -3),(1 , -1) } ,
            { (1 , -1),(2 , 0),(1 , 1) } ,
            { (1 , 1),(1 , 3),(0 , 2) } ,
            { (1 , 1),(0 , 2),(-1 , 1) } ,
            { (0 , 2),(-1 , 3),(-1 , 1) } ,
            { (-1 , 1),(-2 , 0),(-1 , -1) } ,
            { (-1 , -1),(-1 , -3),(0 , -2) } ,
            { (-1 , -1),(0 , -2),(1 , -1) } ,
        };
        foreach (var standardBlock in unionHexBlockList)
        {
            for (int i = 0; i < rhombusDir.GetLength(0); i++)
            {
                if ((unionHexBlockList.Any(block => block.x == standardBlock.x + rhombusDir[i, 0].x && block.y == standardBlock.y + rhombusDir[i, 0].y) &&
                unionHexBlockList.Any(block => block.x == standardBlock.x + rhombusDir[i, 1].x && block.y == standardBlock.y + rhombusDir[i, 1].y) &&
                unionHexBlockList.Any(block => block.x == standardBlock.x + rhombusDir[i, 2].x && block.y == standardBlock.y + rhombusDir[i, 2].y)))
                {
                    isBoomerang = true;
                    destroyBlockSet.Add(standardBlock);
                    destroyBlockSet.Add(hexBlockContainerMatrix[standardBlock.x + rhombusDir[i, 0].x, standardBlock.y + rhombusDir[i, 0].y].hexBlock);
                    destroyBlockSet.Add(hexBlockContainerMatrix[standardBlock.x + rhombusDir[i, 1].x, standardBlock.y + rhombusDir[i, 1].y].hexBlock);
                    destroyBlockSet.Add(hexBlockContainerMatrix[standardBlock.x + rhombusDir[i, 2].x, standardBlock.y + rhombusDir[i, 2].y].hexBlock);
                    break;
                }
            }
            if (isBoomerang)
            {
                break;
            }
        }
        #endregion

        var itemColor = unionHexBlockList[0].eColor;
        var itemType = EBlockType.Empty;
        foreach (var hexBlockLine in hexBlockLineList_Upper4)
        {
            if (hexBlockLine.hexBlockList.Count >= 5)
            {
                if (itemType < EBlockType.item_ufo)
                {
                    itemType = EBlockType.item_ufo;
                }
            }
            if (hexBlockLine.hexBlockList.Count == 4)
            {
                if (itemType < EBlockType.item_rocket)
                {
                    itemType = EBlockType.item_rocket;
                }
            }
            foreach (var block in hexBlockLine.hexBlockList)
            {
                destroyBlockSet.Add(block);
            }
        }

        if (isVMatchSuccess)
        {
            if (itemType < EBlockType.item_tnt)
            {
                itemType = EBlockType.item_tnt;
            }
        }

        if (isYMatchSuccess)
        {
            if (itemType < EBlockType.item_turtle)
            {
                itemType = EBlockType.item_turtle;
            }
        }

        if (isBoomerang)
        {
            if (itemType < EBlockType.item_boomerang)
            {
                itemType = EBlockType.item_boomerang;
            }
        }

        return (destroyBlockSet, itemColor, itemType);
    }
    private static List<HexBlockContainer> GetNeighborContainerBlockList(HexBlockContainer hexBlockContainer)
    {
        var neighborList = new List<HexBlockContainer>();
        foreach (var dir in dirList)
        {
            int neighborIndexX = hexBlockContainer.x + dir.x;
            int neighborIndexY = hexBlockContainer.y + dir.y;
            int matrixWidth = hexBlockContainerMatrix.GetLength(0);
            int matrixHeight = hexBlockContainerMatrix.GetLength(1);

            HexBlockContainer neighborHexBlockContainer = null;

            // 인덱스가 유효한지 검사
            if (neighborIndexX >= 0 && neighborIndexX < matrixWidth && neighborIndexY >= 0 && neighborIndexY < matrixHeight)
            {
                neighborHexBlockContainer = hexBlockContainerMatrix[neighborIndexX, neighborIndexY];
            }

            if (ReferenceEquals(neighborHexBlockContainer, null))
            {
                continue;
            }
            neighborList.Add(neighborHexBlockContainer);
        }
        return neighborList;
    }
    public static List<HexBlockContainer> GetAllHexContainerBlockListInDirection(int x,int y, ERocketDir eRocketDir) //a는 ay=x+b의 a에 해당한다. (기울기)
    {
        var lineList = new List<HexBlockContainer>();
        int a = 0;
        if(eRocketDir == ERocketDir.UpDown)
        {
            a = 0;
        }
        else if(eRocketDir == ERocketDir.RightUp )
        {
            a = -1;
        }
        else
        {
            a = 1;
        }

        int b = a * y - x;
        foreach (var item in CollisionDetectManager.Instance.hexBlockContainerList)
        {
            if(a*item.y == item.x+b)
            {
                lineList.Add(item);
            }
        }
        return lineList;
    }
    public static List<HexBlockContainer> GetTnTAreaContainerList(HexBlockContainer hexBlockContainer, bool isLargeArea)
    {
        var TNTAreaList = new List<HexBlockContainer>();
        var tntDirList = new List<(int x, int y)>() { (0, 2), (0, -2), (1, 1), (-1, -1), (1, -1), (-1, 1) , (2,-2),(2,0),(2,2) , (-2, -2), (-2, 0), (-2, 2) };
        var tntLargeDirList = new List<(int x, int y)>() { (-2, -4), (-1, -3), (0, -4), (1, -3), (2, -4), (2, 4), (1, 3), (0, 4), (-1, 3), (-2, 4) };

        var dirList = tntDirList;
        if(isLargeArea)
        {
            dirList.AddRange(tntLargeDirList);
        }
        foreach (var dir in dirList)
        {
            int neighborIndexX = hexBlockContainer.x + dir.x;
            int neighborIndexY = hexBlockContainer.y + dir.y;
            int matrixWidth = hexBlockContainerMatrix.GetLength(0);
            int matrixHeight = hexBlockContainerMatrix.GetLength(1);

            HexBlockContainer neighborHexBlockContainer = null;

            // 인덱스가 유효한지 검사
            if (neighborIndexX >= 0 && neighborIndexX < matrixWidth && neighborIndexY >= 0 && neighborIndexY < matrixHeight)
            {
                neighborHexBlockContainer = hexBlockContainerMatrix[neighborIndexX, neighborIndexY];
            }

            if (ReferenceEquals(neighborHexBlockContainer, null))
            {
                continue;
            }
            TNTAreaList.Add(neighborHexBlockContainer);
        }
        return TNTAreaList;
    }
   
    private static void Union(HexBlockContainer hexBlockContainer, UnionFind unionFind)
    {
        bool IsCanUnion(HexBlock standardContainer, HexBlock neighborhexBlock)
        {
            if (standardContainer.eColor == EColor.none || neighborhexBlock.eColor == EColor.none )
            {
                return false;
            }
            if (standardContainer.eColor == neighborhexBlock.eColor )
            {
                return true;
            }
            return false;
        }
        var neighborList = GetNeighborContainerBlockList(hexBlockContainer);
        var standardHexBlockContainerIndex = CollisionDetectManager.Instance.hexBlockContainerList.IndexOf(hexBlockContainer);
        foreach (var neighborHexBlockContainer in neighborList)
        {
            var neighborHexBlockContainerIndex = CollisionDetectManager.Instance.hexBlockContainerList.IndexOf(neighborHexBlockContainer);

            if ((unionFind.Find(standardHexBlockContainerIndex) != unionFind.Find(neighborHexBlockContainerIndex)) && IsCanUnion(hexBlockContainer.hexBlock, neighborHexBlockContainer.hexBlock) )
            {
                unionFind.Union(standardHexBlockContainerIndex, neighborHexBlockContainerIndex);
                Union(neighborHexBlockContainer, unionFind);
            }
        }
    }
    private static bool IsNeighbor(HexBlock hexBlockA, HexBlock hexBlockB)
    {
        if ((Math.Abs(hexBlockA.x - hexBlockB.x) + Math.Abs(hexBlockA.y - hexBlockB.y)) == 2 && Math.Abs(hexBlockA.x - hexBlockB.x) != 2)
        {
            return true;
        }
        return false;
    }

    private void OnValidate()
    {
        EditorInit(x, y);
    }
}
