using Cysharp.Threading.Tasks;
using DG.Tweening;
using SRF;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
public enum EColor
{
    none = 0, // 모든 색상과 매치 될 수 없다.
    blue = 100,
    green = 200,
    orange = 300,
    purple = 400,
    red = 500,
    yellow = 600,
}
public enum EBlockType
{
    Empty = 0,
    normal = 100,
    top = 200,
    item_rocket = 300,
    item_boomerang = 400,
    item_tnt = 500,
    item_turtle = 550,
    item_ufo = 600,
}
public enum ERocketDir
{
    UpDown,
    RightUp,
    LeftUp,
}
public class HexBlock : MonoBehaviour
{
    public HexBlockContainer hexBlockContainer;
    [SerializeField] Animator topAnim;
    public int x;
    public int y;
    public int life;
    public bool isItemEffectDone;
    [SerializeField] private Image blockImage;
    public Canvas canvas;
    public EColor eColor;
    public EBlockType eBlockType;
    private ERocketDir rocketDir;
    private readonly static List<ERocketDir> rocketDirList= new List<ERocketDir>() { ERocketDir.UpDown,ERocketDir.LeftUp,ERocketDir.RightUp};
    public void Init(EColor eColor, EBlockType eBlockType)
    {
        isItemEffectDone = false;
        topAnim.enabled = false;
        this.eColor = eColor;
        this.eBlockType = eBlockType;
        switch (eBlockType)
        {
            case EBlockType.top:
                life = 2;
                GameManager.Instance.topList.Add(this);
                break;
            default:
                life = 1;
                break;
        }
        switch (eBlockType)
        {
            case EBlockType.top:
            case EBlockType.Empty:
                this.eColor = EColor.none;
                break;
            case EBlockType.item_rocket:
                rocketDir = rocketDirList.Random();
                switch (rocketDir)
                {
                    case ERocketDir.UpDown:
                        transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                        break;
                    case ERocketDir.RightUp:
                        transform.rotation = Quaternion.Euler(0f, 0f, 30f);
                        break;
                    case ERocketDir.LeftUp:
                        transform.rotation = Quaternion.Euler(0f, 0f, 120f);
                        break;
                }
                break;
        }
        UpdateBlockImage();
    }
    private HexBlockContainer FindSameXHexBlockContainer(Vector3 pos)
    {
        foreach (var hexBlockContainer in CollisionDetectManager.Instance.hexBlockContainerList)
        {
            if (Mathf.Abs(hexBlockContainer.transform.position.x - pos.x) <= 0.01f)
            {
                return hexBlockContainer;
            }
        }
        return null;
    }
    public async void SetUFOColor() //현재 위치에서 6종의 색상 중 하나로 바꼇을 때 가장 높은 점수인 색상으로 바뀐다.
    {
        if(eBlockType == EBlockType.item_ufo)
        {
            try
            {
                await UniTask.WaitWhile(() => !HexBlockContainer.IsAllBlockGenerated);
                int maxHintPoint = 0;
                EColor maxHintPointColor = EColor.none;
                foreach (var eColor in HexBlockContainer.EColorList)
                {
                    this.eColor = eColor;
                    var hintPoint = HexBlockContainer.GetHintPoint(new List<HexBlockContainer>() { hexBlockContainer });
                    if (hintPoint > maxHintPoint)
                    {
                        maxHintPoint = hintPoint;
                        maxHintPointColor = eColor;
                    }
                }
                this.eColor = maxHintPointColor;
            }
            catch
            {

            }
           
        }
    }
    public void ChangeHexBlockContainer(HexBlockContainer hexBlockContainer)
    {
        if (!ReferenceEquals(this.hexBlockContainer, null) && this.hexBlockContainer.hexBlock == this)
        {
            this.hexBlockContainer.hexBlock = null;
        }
        this.hexBlockContainer = hexBlockContainer;
        if(!ReferenceEquals(hexBlockContainer,null))
        {
            hexBlockContainer.hexBlock = this;
            x = hexBlockContainer.x;
            y = hexBlockContainer.y;
        }

        SetUFOColor();
    }
    public async UniTask SetHexBlockContainerWithMove(HexBlockContainer hexBlockContainer, float moveSpeed, bool isMoveDirectly = false, bool isTimeBase = false)
    {
        ChangeHexBlockContainer(hexBlockContainer);


        var sameXHexBlockContainer = FindSameXHexBlockContainer(transform.position);
        var xVector = hexBlockContainer.x - sameXHexBlockContainer.x;
        if (xVector < 0)
        {
            xVector = -1;
        }
        else if (xVector > 0)
        {
            xVector = 1;
        }
        List<Vector3> movingRouteList = new();
        movingRouteList.Add(transform.position);
        var isMoveDone = false;
        if (!isMoveDirectly)
        {
            while (true) //1. 현재 위치에서 가장 높은곳으로 이동한다. 2. vector방향으로 x축1칸,y축1칸 이동한다. 1번과 2번 반복한다. 만약 1번의 목적지가 hexBlockContainer와 같다면 이동 후 break
            {
                sameXHexBlockContainer = FindSameXHexBlockContainer(movingRouteList.Last());
                var topEmptyContainerSameColum = HexBlockContainer.GetTopEmptyHexBlockContainerInSameColum(sameXHexBlockContainer.x);
                if (sameXHexBlockContainer.x == hexBlockContainer.x ||ReferenceEquals(topEmptyContainerSameColum,null))//x좌표가 같은것중 가장 높은 블럭에 직선으로 떨어진다
                {
                    break;
                }

                movingRouteList.Add(topEmptyContainerSameColum.transform.position);

                movingRouteList.Add(HexBlockContainer.hexBlockContainerMatrix[topEmptyContainerSameColum.x + xVector, topEmptyContainerSameColum.y + 1].transform.position);
            }
        }

        movingRouteList.Add(hexBlockContainer.transform.position);

        for(int i = 0; i<movingRouteList.Count; i++)
        {
            isMoveDone = false;
            transform.DOMove(movingRouteList[i], moveSpeed).SetSpeedBased(!isTimeBase).OnComplete(() => isMoveDone = true);
            await UniTask.WaitWhile(() => !isMoveDone);
        }
    }
    private void UpdateBlockImage()
    {
        var spriteName = string.Empty;
        if(eBlockType == EBlockType.item_ufo)
        {
            spriteName = eBlockType.ToString();
        }
        else
        {
            spriteName = $"{(eColor == EColor.none ? string.Empty : eColor.ToString() + "_")}{(eBlockType == EBlockType.Empty ? string.Empty : eBlockType.ToString())}";
        }
        if (string.IsNullOrEmpty(spriteName))
        {
            blockImage.enabled = false;
        }
        else
        {
            blockImage.enabled = true;
            if (Application.isPlaying)
            {
                blockImage.sprite = SpriteManager.Instance.LoadSprite(spriteName);
            }
            else
            {
                blockImage.sprite = Addressables.LoadAssetAsync<Sprite>(spriteName).WaitForCompletion();
            }
            blockImage.SetNativeSize();
        }
    }
    private async UniTask EnableItemEffect(List<HexBlockContainer> destroyContainerByItemList)
    {
        if(destroyContainerByItemList.Count==0)
        {
            return;
        }
        float tweeningTime = 0.7f;
        foreach (var item in destroyContainerByItemList)
        {
            item.EnableHintEffect(true);
        }
        transform.localScale = Vector3.one * 1.5f;
        foreach (var hexBlockContainer in destroyContainerByItemList)
        {
            if (!ReferenceEquals(hexBlockContainer.hexBlock, null))
            {
                var projectile = PoolableManager.Instance.Instantiate(EPrefab.Projectile, transform.position, parentTransform: BlockEditor.Instance.transform);
                projectile.transform.DOMove(hexBlockContainer.transform.position, tweeningTime).OnComplete(() => { PoolableManager.Instance.Destroy(projectile); });
            }
        }

        await UniTask.Delay(Mathf.CeilToInt(tweeningTime*1000f));
        transform.localScale = Vector3.one;
        foreach (var hexBlockContainer in destroyContainerByItemList)
        {
            if (!ReferenceEquals(hexBlockContainer.hexBlock, null))
            {
                await hexBlockContainer.hexBlock.Damaged();
            }
        }
        GameManager.Instance.DisableAllHintEffect();
    }
    public static List<HexBlock> GetAllHexBlockList(bool isFindOnlyNormalBlock)
    {
        List<HexBlock> hexBlockList = new();
        foreach (var item in CollisionDetectManager.Instance.hexBlockContainerList)
        {
            if (!ReferenceEquals(item.hexBlock, null))
            {
                if (isFindOnlyNormalBlock)
                {
                    if (item.hexBlock.eBlockType == EBlockType.normal)
                    {
                        hexBlockList.Add(item.hexBlock);
                    }
                }
                else
                {
                    hexBlockList.Add(item.hexBlock);
                }
            }
        }
        return hexBlockList;
    }
    public async UniTask MergeItems(HexBlock movedItem)
    {
        HexBlockContainer hexBlockContainerCash = hexBlockContainer;
        //this는 가만히 있고, movedItem이 움직여서 함쳐진 상황이다.
        isItemEffectDone = true;
        movedItem.isItemEffectDone = true;
        movedItem.transform.DOMove(hexBlockContainer.transform.position, 0.1f);
        if (movedItem.eBlockType == this.eBlockType) 
        {
            switch (eBlockType)
            {
                case EBlockType.item_rocket:
                    await RocketItemEffect(hexBlockContainerCash.x, hexBlockContainerCash.y, rocketDir);
                    await RocketItemEffect(hexBlockContainerCash.x, hexBlockContainerCash.y, movedItem.rocketDir);
                    break;
                case EBlockType.item_boomerang:
                    await BoomerangItemEffect();
                    await BoomerangItemEffect();
                    await BoomerangItemEffect();
                    break;
                case EBlockType.item_tnt:
                    await TnTItemEffect(hexBlockContainer, true);
                    break;
                case EBlockType.item_turtle:
                    await AllDestroyItemEffect();
                    break;
                case EBlockType.item_ufo:
                    await AllDestroyItemEffect();
                    break;
            }
        }
        else
        {
            //UFO가 포함돼있다: 같은 색상의 모든 블럭이 아이템으로 변함
            if(eBlockType == EBlockType.item_ufo || movedItem.eBlockType == EBlockType.item_ufo)
            {
                var convertedList = new List<HexBlock>();
                EBlockType otherType = eBlockType == EBlockType.item_ufo ? movedItem.eBlockType : eBlockType;
                EColor otherColor = eBlockType == EBlockType.item_ufo ? movedItem.eColor : eColor;
                foreach (var item in CollisionDetectManager.Instance.hexBlockContainerList)
                {
                    if (!ReferenceEquals(item.hexBlock, null) && item.hexBlock.eColor == otherColor)
                    {
                        if(item.hexBlock.eBlockType == EBlockType.normal)
                        {
                            item.hexBlock.Init(otherColor, otherType);
                            convertedList.Add(item.hexBlock);
                            await UniTask.Delay(200);
                        }
                    }
                }
                foreach (var item in convertedList)
                {
                    await item.Damaged();
                }
            }
            //거북이가 포함돼있다: 거북이 강화효과
            else if (eBlockType == EBlockType.item_turtle || movedItem.eBlockType == EBlockType.item_turtle)
            {
                await TurtleItemEffect(hexBlockContainer ,true);
            }
            //부메랑 포함돼있다: 부메랑3개+그냥 아이템 효과
            else if (eBlockType == EBlockType.item_boomerang || movedItem.eBlockType == EBlockType.item_boomerang)
            {
                HexBlock otherType = eBlockType == EBlockType.item_boomerang ? movedItem : this;
                otherType.isItemEffectDone = false;
                await otherType.UseDefaultItemEffect(hexBlockContainer);
                await BoomerangItemEffect();
                await BoomerangItemEffect();
                await BoomerangItemEffect();
            }
            //TNT+ 로켓: 로켓3줄
            else if(eBlockType == EBlockType.item_rocket || movedItem.eBlockType == EBlockType.item_rocket)
            {
                HexBlock rocket = eBlockType == EBlockType.item_rocket ? this: movedItem;
                var destroyContainerByItemList = new List<HexBlockContainer>();
                destroyContainerByItemList.AddRange(HexBlockContainer.GetAllHexContainerBlockListInDirection(hexBlockContainer.x + 1, hexBlockContainer.y + 1, rocket.rocketDir));
                destroyContainerByItemList.AddRange(HexBlockContainer.GetAllHexContainerBlockListInDirection(hexBlockContainer.x, hexBlockContainer.y, rocket.rocketDir));
                destroyContainerByItemList.AddRange(HexBlockContainer.GetAllHexContainerBlockListInDirection(hexBlockContainer.x - 1, hexBlockContainer.y - 1, rocket.rocketDir));
                await EnableItemEffect(destroyContainerByItemList);
            }
            else
            {
                isItemEffectDone = false;
                movedItem.isItemEffectDone = false;
                await movedItem.UseDefaultItemEffect(hexBlockContainer);
                await UseDefaultItemEffect(hexBlockContainer);
            }
        }
        Destroy();
        movedItem.ChangeHexBlockContainer(hexBlockContainerCash);
        movedItem.Destroy();
    }
    private async UniTask RocketItemEffect(int x, int y , ERocketDir eRocketDir)
    {
        var destroyContainerByItemList = new List<HexBlockContainer>();
        destroyContainerByItemList.AddRange(HexBlockContainer.GetAllHexContainerBlockListInDirection(x, y, eRocketDir));
        await EnableItemEffect(destroyContainerByItemList);
    }
    private async UniTask BoomerangItemEffect()
    {
        var destroyContainerByItemList = new List<HexBlockContainer>();
        if (GameManager.Instance.topList.Count > 0)
        {
            destroyContainerByItemList.Add(GameManager.Instance.topList.Random().hexBlockContainer);
        }
        else
        {
            foreach (var item in CollisionDetectManager.Instance.hexBlockContainerList)
            {
                if (!ReferenceEquals(item.hexBlock, null))
                {
                    if (item.x == x && item.y == y)
                    {
                        continue;
                    }
                    if(item.hexBlock.eBlockType != EBlockType.normal)
                    {
                        continue;
                    }
                    destroyContainerByItemList.Add(item);
                    break;
                }
            }
        }
        await EnableItemEffect(destroyContainerByItemList);
    }
    private async UniTask TnTItemEffect(HexBlockContainer hexBlockContainer,bool isLargeArea)
    {
        var destroyContainerByItemList = HexBlockContainer.GetTnTAreaContainerList(hexBlockContainer, isLargeArea);
        await EnableItemEffect(destroyContainerByItemList);
    }
    private async UniTask AllDestroyItemEffect()
    {
        var destroyContainerByItemList = CollisionDetectManager.Instance.hexBlockContainerList.ToList();
        await EnableItemEffect(destroyContainerByItemList);
    }
    private async UniTask TurtleItemEffect(HexBlockContainer hexBlockContainer,bool isLargeArea)
    {
        var LargeDirList = new List<(int x, int y)>() { (-1, -3), (0, -4), (1, -3), (1, 3), (0, 4), (-1, 3),(2,-2),(2,0),(2,2),(-2,2),(-2,0),(-2,-2) };
        var destroyContainerByItemList = new List<HexBlockContainer>();
        //6방향 모두 파괴
        foreach (var item in rocketDirList)
        {
            destroyContainerByItemList.AddRange(HexBlockContainer.GetAllHexContainerBlockListInDirection(hexBlockContainer.x, hexBlockContainer.y, item));
        }
        if (isLargeArea)
        {
            foreach (var dir in LargeDirList)
            {
                int neighborIndexX = hexBlockContainer.x + dir.x;
                int neighborIndexY = hexBlockContainer.y + dir.y;
                int matrixWidth = HexBlockContainer.hexBlockContainerMatrix.GetLength(0);
                int matrixHeight = HexBlockContainer.hexBlockContainerMatrix.GetLength(1);

                HexBlockContainer neighborHexBlockContainer = null;

                // 인덱스가 유효한지 검사
                if (neighborIndexX >= 0 && neighborIndexX < matrixWidth && neighborIndexY >= 0 && neighborIndexY < matrixHeight)
                {
                    neighborHexBlockContainer = HexBlockContainer.hexBlockContainerMatrix[neighborIndexX, neighborIndexY];
                }

                if (ReferenceEquals(neighborHexBlockContainer, null))
                {
                    continue;
                }
                destroyContainerByItemList.Add(neighborHexBlockContainer);
            }
        }
        await EnableItemEffect(destroyContainerByItemList);
    }
    private async UniTask UseDefaultItemEffect(HexBlockContainer hexBlockContainer)
    {
        if (!isItemEffectDone)
        {
            var destroyContainerByItemList = new List<HexBlockContainer>();
            isItemEffectDone = true;
            switch (eBlockType)
            {
                case EBlockType.item_rocket:
                    await RocketItemEffect(hexBlockContainer.x,hexBlockContainer.y, rocketDir);
                    break;
                case EBlockType.item_boomerang: //팽이가 존재하면 팽이 공격, 없으면 랜덤한 블럭 공격
                    await BoomerangItemEffect();
                    break;
                case EBlockType.item_tnt:
                    await TnTItemEffect(hexBlockContainer, false);
                    break;
                case EBlockType.item_turtle:
                    await TurtleItemEffect(hexBlockContainer, false);
                    break;
                case EBlockType.item_ufo:
                    {
                        foreach (var item in CollisionDetectManager.Instance.hexBlockContainerList)
                        {
                            if (!ReferenceEquals(item.hexBlock, null) && item.hexBlock.eColor == eColor)
                            {
                                destroyContainerByItemList.Add(item);
                            }
                        }
                        await EnableItemEffect(destroyContainerByItemList);
                    }
                    break;
            }
        }
    }
    public async UniTask Damaged()
    {
        if(life == 0)
        {
            return;
        }
        life--;
        if(eBlockType == EBlockType.top && life == 1)
        {
            topAnim.enabled = true;
        }
        if (life == 0)
        {
            switch (eBlockType)
            {
                case EBlockType.item_rocket:
                case EBlockType.item_boomerang: //팽이가 존재하면 팽이 공격, 없으면 랜덤한 블럭 공격
                case EBlockType.item_tnt:
                case EBlockType.item_turtle:
                case EBlockType.item_ufo:
                    await UseDefaultItemEffect(hexBlockContainer);
                    break;
                case EBlockType.top:
                    GameManager.Instance.topList.Remove(this);
                    PoolableManager.Instance.Instantiate<HexBlock>(EPrefab.TopDistroyEffect, transform.position, parentTransform: BlockEditor.Instance.transform).PlayTopDestroyEffect();
                    break;
                case EBlockType.normal:
                    Color particleColor = Color.blue;
                    switch (eColor)
                    {
                        case EColor.blue:
                            particleColor = Color.blue;
                            break;
                        case EColor.green:
                            particleColor = Color.green;
                            break;
                        case EColor.orange:
                            particleColor = new Color(1.0f, 0.64f, 0.0f); // 오렌지 색상
                            break;
                        case EColor.purple:
                            particleColor = new Color(0.5f, 0.0f, 0.5f); // 퍼플 색상
                            break;
                        case EColor.red:
                            particleColor = Color.red;
                            break;
                        case EColor.yellow:
                            particleColor = Color.yellow;
                            break;
                    }
                    PoolableManager.Instance.Instantiate<AutoEnableParticleAndDistroyAfterEffectEnd>(EPrefab.NormalDestroyParticleEffect, transform.position, Vector3.one).Init(particleColor);
                    break;

            }

            Destroy();
        }
    }
    private void PlayTopDestroyEffect()
    {
        blockImage.enabled = true;
        topAnim.enabled = true;
        GameUtil.Instance.ParabolaMoveForParallel(transform, GameManager.Instance.TopUIRect.position,-85f , 1f,()=> { PoolableManager.Instance.Destroy(gameObject); }, false);
    }
    public async UniTask Merge(Vector3 mergePoint)
    {
        bool isMergeDone = false;
        if(life>=2)
        {
            await Damaged();
            isMergeDone = true;
        }
        else
        {
           // ChangeHexBlockContainer(null);
            transform.DOMove(mergePoint, 0.1f).OnComplete(async () => 
            {
                await Damaged();
                isMergeDone = true;
            });
        }
        await UniTask.WaitWhile(()=>!isMergeDone);
    }
    public void Destroy()
    {
        if (!ReferenceEquals(this.hexBlockContainer, null) && this.hexBlockContainer.hexBlock == this)
        {
            this.hexBlockContainer.hexBlock = null;
        }
        this.hexBlockContainer = null;
        PoolableManager.Instance.Destroy(gameObject);
    }

    private void OnValidate()
    {
        UpdateBlockImage();
    }
}
