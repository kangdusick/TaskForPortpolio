#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using File = System.IO.File;

public class ToolPoolableManagerGen : MonoBehaviour
{
    // PoolableManager 클래스를 자동으로 생성하는 문자열 형식입니다.
    // {0}은 Eprefab을 나타냅니다.
    private const string PoolableManagerFormat =
@"//ToolPoolableManagerGen 의해 자동으로 생성된 스크립트입니다..
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using UnityEngine;
using Component = UnityEngine.Component;
using UnityEngine.AddressableAssets;
{0}

public class AddEPrefab : MonoBehaviour
{{
    public EPrefab eprefab; // EPrefab 타입의 프리팹
    public bool isPoolable; // 풀링 가능 여부
    public ManagedAction OnDestroy = new(); // 객체 파괴 시 실행되는 액션
    public Coroutine destroyCoroutine; // 파괴 코루틴
}}

public class PoolableManager : MonoBehaviour
{{
    private Dictionary<EPrefab, GameObject> _originPrefabs = new(); // 원본 프리팹 딕셔너리
    public Dictionary<GameObject, AddEPrefab> goEprefabFinder = new(); // GameObject와 AddEPrefab 매핑
    private Dictionary<EPrefab, Stack<AddEPrefab>> _poolableObjects = new(); // 풀링 가능한 객체 스택
    public Dictionary<EPrefab, Transform> _poolableParent = new(); // 풀링 가능한 객체의 부모 트랜스폼
    private HashSet<EPrefab> _preloadedPrefabs = new HashSet<EPrefab>(); // 미리 로드된 프리팹 집합
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource(); // 취소 토큰 소스
    private static PoolableManager _instance; // 싱글톤 인스턴스
    public static PoolableManager Instance
    {{
        get
        {{
            if (ReferenceEquals(_instance, null))
            {{
                GameObject go = new GameObject(""PoolableManager"");
                _instance = go.AddComponent<PoolableManager>();
            }}
            return _instance;
        }}

        private set
        {{
            _instance = value;
        }}
    }}
    private Dictionary<EPrefab, UniTaskCompletionSource<GameObject>> _loadingTasks = new Dictionary<EPrefab, UniTaskCompletionSource<GameObject>>(); // 로딩 작업 딕셔너리

    public void PreLoadAsset(EPrefab ePrefab)
    {{
        if (!_preloadedPrefabs.Contains(ePrefab))
        {{
            _preloadedPrefabs.Add(ePrefab);
            LoadAssetAsync(ePrefab);
        }}
    }}
    
    // 동기식으로 자산을 로드하는 메서드
    private GameObject LoadAsset(EPrefab ePrefab)
    {{
        if (!_originPrefabs.ContainsKey(ePrefab))
        {{
            var go2 = Addressables.LoadAssetAsync<GameObject>(ePrefab.OriginName()).WaitForCompletion();
            _originPrefabs[ePrefab] = go2;
        }}
        return _originPrefabs[ePrefab];
    }}
    
    // 비동기식으로 자산을 로드하는 메서드
    private async UniTask<GameObject> LoadAssetAsync(EPrefab ePrefab)
    {{
        if (!_originPrefabs.ContainsKey(ePrefab))
        {{
            if (!_loadingTasks.TryGetValue(ePrefab, out var completionSource))
            {{
                completionSource = new UniTaskCompletionSource<GameObject>();
                _loadingTasks[ePrefab] = completionSource;

                try
                {{
                    var go2 = await Addressables.LoadAssetAsync<GameObject>(ePrefab.OriginName()).ToUniTask(cancellationToken: _cancellationTokenSource.Token);
                    _originPrefabs[ePrefab] = go2;
                    completionSource.TrySetResult(go2);
                }}
                catch (System.Exception e)
                {{
                    completionSource.TrySetException(e);
                    Debug.Log(""Loading of "" + ePrefab.OriginName() + "" was cancelled."");
                }}
                finally
                {{
                    _loadingTasks.Remove(ePrefab);
                }}
            }}

            try
            {{
                return await completionSource.Task;
            }}
            catch
            {{
                return null;
            }}
        }}
        else
        {{
            return _originPrefabs[ePrefab];
        }}
    }}
    
    // 프리팹을 인스턴스화하는 메서드들
    public GameObject Instantiate(string ePrefab, Vector3 position = default, Vector3 localScale = default, Quaternion rotation = default, Transform parentTransform = null)
    {{
        return Instantiate(ePrefab.ParseEnum<EPrefab>(), position, localScale, rotation, parentTransform);
    }}

    public GameObject Instantiate(EPrefab ePrefab, Vector3 position = default, Vector3 localScale = default, Quaternion rotation = default, Transform parentTransform = null)
    {{
        LoadAsset(ePrefab);
        GameObject go = GetOrCreateGameObject(ePrefab);
        SetupGameObject(go, position, rotation, localScale, parentTransform);
        return go;
    }}

    public T Instantiate<T>(string ePrefab, Vector3 position = default, Vector3 localScale = default, Quaternion rotation = default, Transform parentTransform = null) where T : Component
    {{
        return Instantiate<T>(ePrefab.ParseEnum<EPrefab>(), position, localScale, rotation, parentTransform);
    }}

    public T Instantiate<T>(EPrefab ePrefab, Vector3 position = default, Vector3 localScale = default, Quaternion rotation = default, Transform parentTransform = null) where T : Component
    {{
        LoadAsset(ePrefab);
        GameObject go = GetOrCreateGameObject(ePrefab);
        SetupGameObject(go, position, rotation, localScale, parentTransform);
        return go.GetCashComponent<T>();
    }}

    public async UniTask<GameObject> InstantiateAsync(string ePrefab, Vector3 position = default, Vector3 localScale = default, Quaternion rotation = default, Transform parentTransform = null)
    {{
        return await InstantiateAsync(ePrefab.ParseEnum<EPrefab>(), position, localScale, rotation, parentTransform);
    }}

    public async UniTask<GameObject> InstantiateAsync(EPrefab ePrefab, Vector3 position = default, Vector3 localScale = default, Quaternion rotation = default, Transform parentTransform = null)
    {{
        await LoadAssetAsync(ePrefab);

        GameObject go = await GetOrCreateGameObjectAsync(ePrefab);
        await UniTask.SwitchToMainThread();
        SetupGameObject(go, position, rotation, localScale, parentTransform);

        return go;
    }}

    public async UniTask<T> InstantiateAsync<T>(string ePrefab, Vector3 position = default, Vector3 localScale = default, Quaternion rotation = default, Transform parentTransform = null) where T : Component
    {{
        return await InstantiateAsync<T>(ePrefab.ParseEnum<EPrefab>(), position, localScale, rotation, parentTransform);
    }}

    public async UniTask<T> InstantiateAsync<T>(EPrefab ePrefab, Vector3 position = default, Vector3 localScale = default, Quaternion rotation = default, Transform parentTransform = null) where T : Component
    {{
        await LoadAssetAsync(ePrefab);

        GameObject go = await GetOrCreateGameObjectAsync(ePrefab);
        await UniTask.SwitchToMainThread();
        SetupGameObject(go, position, rotation, localScale, parentTransform);

        return go.GetCashComponent<T>();
    }}
   
    // 게임 오브젝트 설정 메서드
    private void SetupGameObject(GameObject go, Vector3 position, Quaternion rotation, Vector3 localScale, Transform parentTransform)
    {{
        if (parentTransform != null && go.transform.parent != parentTransform)
        {{
            go.transform.SetParent(parentTransform, worldPositionStays: false);
        }}

        go.transform.position = position;
        go.transform.rotation = rotation;

        if (localScale != default(Vector3))
        {{
            go.transform.localScale = localScale;
        }}
    }}
    
    // 비동기식으로 게임 오브젝트를 생성하거나 가져오는 메서드
    private async UniTask<GameObject> GetOrCreateGameObjectAsync(EPrefab ePrefab)
    {{
        if (!_poolableParent.ContainsKey(ePrefab))
        {{
            CreateNewPool(ePrefab);
        }}

        if (_poolableObjects[ePrefab].Count > 0)
        {{
            AddEPrefab addEprefab = null;
            do
            {{
                addEprefab = _poolableObjects[ePrefab].Pop();
                if (addEprefab.isPoolable)
                {{
                    addEprefab.isPoolable = false;
                    addEprefab.gameObject.SetActive(true);

                    if (addEprefab.destroyCoroutine != null)
                    {{
                        StopCoroutine(addEprefab.destroyCoroutine);
                        addEprefab.destroyCoroutine = null;
                    }}
                    addEprefab.OnDestroy = new();
                    return addEprefab.gameObject;
                }}
            }} while (_poolableObjects[ePrefab].Count > 0);

            return await CreateNewGameObjectAsync(ePrefab);
        }}
        else
        {{
            return await CreateNewGameObjectAsync(ePrefab);
        }}
    }}
    
    // 동기식으로 게임 오브젝트를 생성하거나 가져오는 메서드
    private GameObject GetOrCreateGameObject(EPrefab ePrefab)
    {{
        if (!_poolableParent.ContainsKey(ePrefab))
        {{
            CreateNewPool(ePrefab);
        }}

        if (_poolableObjects[ePrefab].Count > 0)
        {{
            AddEPrefab addEprefab = null;
            do
            {{
                addEprefab = _poolableObjects[ePrefab].Pop();
                if (addEprefab.isPoolable)
                {{
                    addEprefab.isPoolable = false;
                    addEprefab.gameObject.SetActive(true);

                    if (addEprefab.destroyCoroutine != null)
                    {{
                        StopCoroutine(addEprefab.destroyCoroutine);
                        addEprefab.destroyCoroutine = null;
                    }}
                    addEprefab.OnDestroy = new();
                    return addEprefab.gameObject;
                }}
            }} while (_poolableObjects[ePrefab].Count > 0);

            return CreateNewGameObject(ePrefab);
        }}
        else
        {{
            return CreateNewGameObject(ePrefab);
        }}
    }}
    
    // 새로운 풀을 생성하는 메서드
    private void CreateNewPool(EPrefab ePrefab)
    {{
        var newQueue = new Stack<AddEPrefab>();
        _poolableObjects.Add(ePrefab, newQueue);

        var parent = new GameObject($""{{ePrefab}}Parent"");
        parent.transform.SetParent(transform);
        _poolableParent.Add(ePrefab, parent.transform);
    }}
    
    // 비동기식으로 새로운 게임 오브젝트를 생성하는 메서드
    private async UniTask<GameObject> CreateNewGameObjectAsync(EPrefab ePrefab)
    {{
        var newGameObjectOperation = UnityEngine.Object.InstantiateAsync(_originPrefabs[ePrefab], _poolableParent[ePrefab].transform);
        await UniTask.WaitUntil(() => newGameObjectOperation.isDone);
        var addEPrefab = newGameObjectOperation.Result[0].AddComponent<AddEPrefab>();
        addEPrefab.eprefab = ePrefab;
        addEPrefab.isPoolable = false;
        goEprefabFinder.Add(newGameObjectOperation.Result[0], addEPrefab);

        return newGameObjectOperation.Result[0];
    }}
    
    // 동기식으로 새로운 게임 오브젝트를 생성하는 메서드
    private GameObject CreateNewGameObject(EPrefab ePrefab)
    {{
        var newGameObject = Instantiate(_originPrefabs[ePrefab], _poolableParent[ePrefab].transform, true);
        var addEPrefab = newGameObject.AddComponent<AddEPrefab>();
        addEPrefab.eprefab = ePrefab;
        addEPrefab.isPoolable = false;
        goEprefabFinder.Add(newGameObject, addEPrefab);

        return newGameObject;
    }}
    
    // 게임 오브젝트를 파괴하는 메서드
    public void Destroy(GameObject gameObj, float delay = 0f, Action onDestroyAction = null)
    {{
        StartCoroutine(DestroyRoutine(gameObj, delay, onDestroyAction));
    }}

    // 지연 파괴를 위한 코루틴
    private IEnumerator DestroyRoutine(GameObject gameObj, float delay = 0f, Action onDestroyAction = null)
    {{
        if (delay != 0f)
        {{
            yield return TimeManager.GetWaitForSeconds(delay);
        }}
        var addEPrefab = goEprefabFinder[gameObj];
        if(addEPrefab.isPoolable)
        {{
            yield break;
        }}
        addEPrefab.isPoolable = true;
        addEPrefab.OnDestroy.Invoke();
        _poolableObjects[addEPrefab.eprefab].Push(addEPrefab);
        gameObj.SetActive(false);
        onDestroyAction?.Invoke();
        addEPrefab.destroyCoroutine = StartCoroutine(DestroyAfterDelay(addEPrefab));
    }}

    // 일정 시간 후에 오브젝트를 파괴하는 코루틴
    private IEnumerator DestroyAfterDelay(AddEPrefab addEPrefab)
    {{
        yield return TimeManager.GetWaitForSeconds(120f);
        if (addEPrefab.isPoolable)
        {{
            var pool = _poolableObjects[addEPrefab.eprefab];
            if (pool.Contains(addEPrefab))
            {{
                var tempStack = new Stack<AddEPrefab>(pool.Count);
                while (pool.Count > 0)
                {{
                    var item = pool.Pop();
                    if (item != addEPrefab)
                    {{
                        tempStack.Push(item);
                    }}
                }}

                while (tempStack.Count > 0)
                {{
                    pool.Push(tempStack.Pop());
                }}

                UnityEngine.GameObject.Destroy(addEPrefab.gameObject);

                if (!_preloadedPrefabs.Contains(addEPrefab.eprefab) && pool.Count == 0)
                {{
                    ReleaseAsset(addEPrefab.eprefab);
                }}
            }}
        }}
    }}

    // 자식 오브젝트를 파괴하는 메서드
    public void DestroyWithChildren(GameObject gameObj, bool destroyOnlyChilden = false, float delay = 0f, Action onDestroyAction = null)
    {{
        StartCoroutine(DestroyWithChildrenRoutine(gameObj, destroyOnlyChilden, delay, onDestroyAction));
    }}

    // 자식 오브젝트 파괴를 위한 코루틴
    private IEnumerator DestroyWithChildrenRoutine(GameObject gameObj, bool destroyOnlyChilden = false, float delay = 0f, Action onDestroyAction = null)
    {{
        if (delay != 0f)
        {{
            yield return TimeManager.GetWaitForSeconds(delay);
        }}
        var addEPrefabs = gameObj.GetComponentsInChildren<AddEPrefab>(true);
        for (int i = 0; i < addEPrefabs.Length; i++)
        {{
            if (destroyOnlyChilden && i == 0)
            {{
                continue;
            }}
            if(addEPrefabs[i].isPoolable)
            {{
                continue;
            }}
            addEPrefabs[i].isPoolable = true;
            addEPrefabs[i].OnDestroy?.Invoke();
            _poolableObjects[addEPrefabs[i].eprefab].Push(addEPrefabs[i]);
            addEPrefabs[i].gameObject.SetActive(false);
        }}
        onDestroyAction?.Invoke();
    }}
    
    // 자산을 해제하는 메서드
    private void ReleaseAsset(EPrefab ePrefab)
    {{
        if (_originPrefabs.ContainsKey(ePrefab))
        {{
            Addressables.Release(_originPrefabs[ePrefab]);
            _originPrefabs.Remove(ePrefab);
        }}
    }}
    
    // PoolableManager가 파괴될 때 실행되는 메서드
    private void OnDestroy()
    {{
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        var dictkeyList = _originPrefabs.Keys.ToList();
        foreach (var ePrefab in dictkeyList)
        {{
            ReleaseAsset(ePrefab);
        }}
        _originPrefabs.Clear();
        Extensions.componentCache.Clear();
        Instance = null;
    }}
}}";

    // 에디터 메뉴에 'ToolPoolableManageGen' 항목을 추가하고, 선택 시 코드 생성 작업을 수행합니다.
    [MenuItem("Tools/ToolPoolableManageGen")]
    public async static void Gen()
    {
        StringBuilder EPrefabString = new StringBuilder();
        int goCnt = 0;
        var loadResourceLocationHandle = Addressables.LoadResourceLocationsAsync("prefab", typeof(GameObject));
        await UniTask.WaitWhile(() => !loadResourceLocationHandle.IsDone);
        var locationList = loadResourceLocationHandle.Result;
        List<string> nameList = new();
        foreach (var location in locationList)
        {
            {
                Addressables.LoadAssetAsync<GameObject>(location).Completed += (handle) =>
                {
                    {
                        var go = handle.Result;

                        var originName = go.name;
                        nameList.Add(originName);

                        goCnt++;
                    }
                };
            }
        }
        await UniTask.WaitWhile(() => goCnt != locationList.Count);
        var eprefabFormat = GameUtil.GenerateEnum("EPrefab", nameList);
        var filePath = $"{Application.dataPath}/1_kds/Scripts/codegen/PoolableManager.cs";
        File.WriteAllText(filePath, string.Format(PoolableManagerFormat, eprefabFormat));
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        CompilationPipeline.RequestScriptCompilation();
    }
}
#endif
