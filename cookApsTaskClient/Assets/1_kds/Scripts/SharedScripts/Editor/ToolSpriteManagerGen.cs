#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using File = System.IO.File;

public class ToolSpriteManagerGen : MonoBehaviour
{
    //{0}: ESprite
    private const string SpriteManagerFormat =
@"//ToolSpriteManagerGen 의해 자동으로 생성된 스크립트입니다..
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Linq;
using System.Threading;
{0}

public class SpriteManager : MonoBehaviour
{{
    private static SpriteManager _instance;
    public static SpriteManager Instance
    {{
       
        get
        {{
            if (ReferenceEquals(_instance ,null))
            {{
                GameObject spriteManagerGameObject = new GameObject(""SpriteManager"");
                _instance = spriteManagerGameObject.AddComponent<SpriteManager>();
            }}
            return _instance;
        }}

        private set
        {{
            _instance = value;
        }}
    }}
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private Dictionary<ESprite, Sprite> spriteDict = new();
    public Sprite LoadSprite(string eSprite)
    {{
        return LoadSprite(eSprite.ParseEnum<ESprite>());
    }}
    public Sprite LoadSprite(ESprite eSprite)
    {{
        Sprite sprite;
        if (spriteDict.ContainsKey(eSprite))
        {{
            sprite = spriteDict[eSprite];
        }}
        else
        {{
            var go = Addressables.LoadAssetAsync<Sprite>(eSprite.OriginName()).WaitForCompletion();
            spriteDict[eSprite] = go;
            sprite = spriteDict[eSprite];
        }}
        return sprite;
    }}
    public async UniTask<Sprite> LoadSpriteAsync(ESprite eSprite)
    {{
        Sprite sprite;
        if (spriteDict.ContainsKey(eSprite))
        {{
            sprite = spriteDict[eSprite];
        }}
        else
        {{
            try
            {{
                var go = await Addressables.LoadAssetAsync<Sprite>(eSprite.OriginName()).ToUniTask(cancellationToken: _cancellationTokenSource.Token);
                spriteDict[eSprite] = go;
                sprite = spriteDict[eSprite];
            }}
            catch (System.Exception e)
            {{
                Debug.Log(""Loading of"" +eSprite.OriginName() +""was cancelled."");

                return null;
            }}
        }}

        return sprite;
    }}
    private void ReleaseAsset(ESprite eSprite) 
    {{
        if(spriteDict.ContainsKey(eSprite))
        {{
            Addressables.Release(spriteDict[eSprite]);
            spriteDict.Remove(eSprite);
        }}
    }}
    private void OnDestroy()
    {{
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        var dictkeyList = spriteDict.Keys.ToList();
        foreach (var eSprite in dictkeyList)
        {{
            ReleaseAsset(eSprite);
        }}
        spriteDict.Clear();
        Instance = null;
    }}
}}";

    [MenuItem("Tools/ToolSpriteManageGen")]
    public async static void Gen()
    {
        StringBuilder ESpriteString = new StringBuilder();
        int goCnt = 0;
        var loadResourceLocation = Addressables.LoadResourceLocationsAsync("sprite", typeof(Sprite));
        await loadResourceLocation.Task;
        var locationList = loadResourceLocation.Result;
        List<string> nameList = new();
        foreach (var location in locationList)
        {
            Addressables.LoadAssetAsync<Sprite>(location).Completed += (handle) =>
            {
                var go = handle.Result;

                var originName = go.name;
                nameList.Add(originName);
                goCnt++;
            };
            
        }

        await Task.Run(() =>
        {
            while (goCnt != locationList.Count)
            {
                // goCnt가 locationList.Count와 같아질 때까지 대기
            }
        });

        var espriteFormat = GameUtil.GenerateEnum("ESprite",nameList);
        var filePath = $"{Application.dataPath}/1_kds/Scripts/codegen/SpriteManager.cs";
        File.WriteAllText(filePath, string.Format(SpriteManagerFormat, espriteFormat));
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        CompilationPipeline.RequestScriptCompilation();

    }
}
#endif