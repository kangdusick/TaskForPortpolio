//ToolSpriteManagerGen 의해 자동으로 생성된 스크립트입니다..
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Linq;
using System.Threading;
public enum ESprite
{
    [Description("blue_normal")]
	blue_normal = -240477941,
	[Description("yellow_normal")]
	yellow_normal = -883872501,
	[Description("purple_normal")]
	purple_normal = -520773663,
	[Description("red_normal")]
	red_normal = 1623121414,
	[Description("green_normal")]
	green_normal = 1045191502,
	[Description("top")]
	top = 163476,
	[Description("blue_item_boomerang")]
	blue_item_boomerang = 1408948538,
	[Description("green_item_boomerang")]
	green_item_boomerang = 1998037087,
	[Description("orange_item_boomerang")]
	orange_item_boomerang = -538196208,
	[Description("purple_item_boomerang")]
	purple_item_boomerang = 445920128,
	[Description("red_item_boomerang")]
	red_item_boomerang = -2102091353,
	[Description("yellow_item_boomerang")]
	yellow_item_boomerang = -1211273926,
	[Description("blue_item_rocket")]
	blue_item_rocket = -455500138,
	[Description("green_item_rocket")]
	green_item_rocket = 328653629,
	[Description("orange_item_rocket")]
	orange_item_rocket = -58085816,
	[Description("purple_item_rocket")]
	purple_item_rocket = 133833720,
	[Description("red_item_rocket")]
	red_item_rocket = -1133999915,
	[Description("yellow_item_rocket")]
	yellow_item_rocket = -1339061610,
	[Description("blue_item_tnt")]
	blue_item_tnt = 1153499768,
	[Description("green_item_tnt")]
	green_item_tnt = -835011323,
	[Description("orange_item_tnt")]
	orange_item_tnt = 1971376218,
	[Description("purple_item_tnt")]
	purple_item_tnt = -1628252950,
	[Description("red_item_tnt")]
	red_item_tnt = 669345213,
	[Description("yellow_item_tnt")]
	yellow_item_tnt = 1326618232,
	[Description("blue_item_turtle")]
	blue_item_turtle = -132911712,
	[Description("green_item_turtle")]
	green_item_turtle = 639477439,
	[Description("orange_item_turtle")]
	orange_item_turtle = -121171258,
	[Description("purple_item_turtle")]
	purple_item_turtle = 93650550,
	[Description("red_item_turtle")]
	red_item_turtle = -1082976441,
	[Description("yellow_item_turtle")]
	yellow_item_turtle = -1016473184,
	[Description("item_ufo")]
	item_ufo = 310103559,
	[Description("orange_normal")]
	orange_normal = -788875471,
	
}



public class SpriteManager : MonoBehaviour
{
    private static SpriteManager _instance;
    public static SpriteManager Instance
    {
       
        get
        {
            if (ReferenceEquals(_instance ,null))
            {
                GameObject spriteManagerGameObject = new GameObject("SpriteManager");
                _instance = spriteManagerGameObject.AddComponent<SpriteManager>();
            }
            return _instance;
        }

        private set
        {
            _instance = value;
        }
    }
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private Dictionary<ESprite, Sprite> spriteDict = new();
    public Sprite LoadSprite(string eSprite)
    {
        return LoadSprite(eSprite.ParseEnum<ESprite>());
    }
    public Sprite LoadSprite(ESprite eSprite)
    {
        Sprite sprite;
        if (spriteDict.ContainsKey(eSprite))
        {
            sprite = spriteDict[eSprite];
        }
        else
        {
            var go = Addressables.LoadAssetAsync<Sprite>(eSprite.OriginName()).WaitForCompletion();
            spriteDict[eSprite] = go;
            sprite = spriteDict[eSprite];
        }
        return sprite;
    }
    public async UniTask<Sprite> LoadSpriteAsync(ESprite eSprite)
    {
        Sprite sprite;
        if (spriteDict.ContainsKey(eSprite))
        {
            sprite = spriteDict[eSprite];
        }
        else
        {
            try
            {
                var go = await Addressables.LoadAssetAsync<Sprite>(eSprite.OriginName()).ToUniTask(cancellationToken: _cancellationTokenSource.Token);
                spriteDict[eSprite] = go;
                sprite = spriteDict[eSprite];
            }
            catch (System.Exception e)
            {
                Debug.Log("Loading of" +eSprite.OriginName() +"was cancelled.");

                return null;
            }
        }

        return sprite;
    }
    private void ReleaseAsset(ESprite eSprite) 
    {
        if(spriteDict.ContainsKey(eSprite))
        {
            Addressables.Release(spriteDict[eSprite]);
            spriteDict.Remove(eSprite);
        }
    }
    private void OnDestroy()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        var dictkeyList = spriteDict.Keys.ToList();
        foreach (var eSprite in dictkeyList)
        {
            ReleaseAsset(eSprite);
        }
        spriteDict.Clear();
        Instance = null;
    }
}