#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.AddressableAssets;
using File = System.IO.File;

public class ToolSoundManagerGen : MonoBehaviour
{
    //{0}: ESound
    private const string SoundManagerFormat =
@"//ToolSoundManagerGen 의해 자동으로 생성된 스크립트입니다..
using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.ComponentModel;
using System;
{0}

public enum ESoundType
{{
    SFX,
    BackGround
}}

public class SoundManager : MonoBehaviour
{{
    private static SoundManager _instance;
    public static SoundManager Instance
    {{

        get
        {{
            if (ReferenceEquals(_instance, null))
            {{
                GameObject go = new GameObject(""SoundManager"");
                _instance = go.AddComponent<SoundManager>();
                go.transform.SetParent(Camera.main.transform);
                go.transform.position = Camera.main.transform.position;
                sountTypeDict.Clear();
                originVolume.Clear();
                eSoundList.Clear();
                soundCooldownDict.Clear();
                foreach (ESoundType item in Enum.GetValues(typeof(ESoundType)))
                {{
                    sountTypeDict.Add(item, new HashSet<AudioSource>());
                }}
                foreach (ESound item in Enum.GetValues(typeof(ESound)))
                {{
                    eSoundList.Add(item);
                    soundCooldownDict.Add(item, -1f);
                }}
                Bg_Volume = ES3.Load(KeyPlayerPrefs.BgSound, 1f);
                SFX_Volume = ES3.Load(KeyPlayerPrefs.SfxSound, 1f);
            }}
            return _instance;
        }}

        private set
        {{
            _instance = value;
        }}
    }}
    private static List<ESound> eSoundList = new();
    private Dictionary<ESound, AudioClip> soundDict = new();
    private static Dictionary<ESound, float> soundCooldownDict = new();
    private Dictionary<AudioSource, Coroutine> playSoundRoutineDict = new();
    private static Dictionary<AudioSource, float> originVolume = new();
    private static Dictionary<ESoundType, HashSet<AudioSource>> sountTypeDict = new();
    private const float soundCooldown = 0.1f;
    private static float _SFX_Volume;
    public static float SFX_Volume
    {{
        get {{ return _SFX_Volume; }}
        set
        {{
            _SFX_Volume = value;
            ES3.Save(KeyPlayerPrefs.SfxSound, _SFX_Volume);
            foreach (var audioSource in sountTypeDict[ESoundType.SFX])
            {{
                audioSource.volume = originVolume[audioSource] * _SFX_Volume;
            }}
        }}
    }}
    private static float _bg_Volume;
    public static float Bg_Volume
    {{
        get {{ return _bg_Volume; }}
        set
        {{
            _bg_Volume = value;
            ES3.Save(KeyPlayerPrefs.BgSound, _bg_Volume);
            foreach (var audioSource in sountTypeDict[ESoundType.BackGround])
            {{
                audioSource.volume = originVolume[audioSource] * _bg_Volume;
            }}
        }}
    }}
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private Queue<AudioSource> poolableAudioSources = new Queue<AudioSource>();
    public AudioSource PlaySound(ESound eSound, float pitch = 1f, float volume = 1f, bool isLoop = false, bool ignoreTimeScale = false, ESoundType eSoundType = ESoundType.SFX)
    {{
        if (!soundDict.ContainsKey(eSound))
        {{
            var go = Addressables.LoadAssetAsync<AudioClip>(eSound.OriginName()).WaitForCompletion();
            soundDict[eSound] = go;
        }}
        if (soundCooldownDict[eSound] >= 0f)
        {{
            return null;
        }}
        AudioSource audioSource;
        if (poolableAudioSources.Count == 0)
        {{
            audioSource = gameObject.AddComponent<AudioSource>();
        }}
        else
        {{
            audioSource = poolableAudioSources.Dequeue();
        }}
        playSoundRoutineDict[audioSource] = StartCoroutine(PlaySoundRoutine(eSound, audioSource, eSoundType, pitch, volume, isLoop, ignoreTimeScale));

        return audioSource;
    }}
    public async UniTask<AudioSource> PlaySoundAsync(ESound eSound, float pitch = 1f, float volume = 1f, bool isLoop = false, bool ignoreTimeScale = false, ESoundType eSoundType = ESoundType.SFX)
    {{
        if (!soundDict.ContainsKey(eSound))
        {{
            try
            {{
                var go = await Addressables.LoadAssetAsync<AudioClip>(eSound.OriginName()).ToUniTask(cancellationToken: _cancellationTokenSource.Token);
                soundDict[eSound] = go;
            }}
            catch (System.Exception e)
            {{
                Debug.Log(""Loading of"" + eSound.OriginName() + ""was cancelled."");

                return null;
            }}
        }}
        if (soundCooldownDict[eSound]>=0f)
        {{
            return null;
        }}
        AudioSource audioSource;
        if (poolableAudioSources.Count == 0)
        {{
            audioSource = gameObject.AddComponent<AudioSource>();
        }}
        else
        {{
            audioSource = poolableAudioSources.Dequeue();
        }}
        playSoundRoutineDict[audioSource] = StartCoroutine(PlaySoundRoutine(eSound, audioSource, eSoundType, pitch, volume, isLoop, ignoreTimeScale));

        return audioSource;
    }}
    IEnumerator PlaySoundRoutine(ESound eSound, AudioSource audioSource, ESoundType eSoundType, float pitch = 1f, float volume = 1f, bool isLoop = false, bool ignoreTimeScale = false)
    {{
        audioSource.clip = soundDict[eSound];
        audioSource.pitch = pitch;
        switch (eSoundType)
        {{
            case ESoundType.SFX:
                audioSource.volume = volume * SFX_Volume;
                break;
            case ESoundType.BackGround:
                audioSource.volume = volume * Bg_Volume;
                break;
        }}
        sountTypeDict[eSoundType].Add(audioSource);
        originVolume[audioSource] = volume;
        soundCooldownDict[eSound] = soundCooldown;
        audioSource.loop = isLoop;
        audioSource.time = 0f;
        audioSource.Play();
        float StoppedTime = 0f;
        bool isStoppedByTimeScale = false;

        while (audioSource.isPlaying || isLoop || isStoppedByTimeScale)
        {{
            if (!ignoreTimeScale && Time.timeScale <= 0.01f && !isStoppedByTimeScale)
            {{
                isStoppedByTimeScale = true;
                StoppedTime = audioSource.time;
                audioSource.Stop();
            }}
            else if (!ignoreTimeScale && Time.timeScale >= 0.01f && isStoppedByTimeScale)
            {{
                isStoppedByTimeScale = false;
                audioSource.time = StoppedTime;
                audioSource.Play();
            }}
            yield return null;
        }}

        StopSound(audioSource);
    }}
    public void StopSound(AudioSource audioSource)
    {{
        if (!ReferenceEquals(audioSource,null) && playSoundRoutineDict.ContainsKey(audioSource))
        {{
            StopCoroutine(playSoundRoutineDict[audioSource]);
            foreach (var item in sountTypeDict.Values)
            {{
                item.Remove(audioSource);
            }}
            audioSource.Stop();
            audioSource.clip = null;
            poolableAudioSources.Enqueue(audioSource);
            playSoundRoutineDict.Remove(audioSource);
        }}
    }}
    private void ReleaseAsset(ESound eSound)
    {{
        if (soundDict.ContainsKey(eSound))
        {{
            Addressables.Release(soundDict[eSound]);
            soundDict.Remove(eSound);
        }}
    }}
    private void Update()
    {{
        for (int i = 0; i < eSoundList.Count; i++)
        {{
            soundCooldownDict[eSoundList[i]] -= Time.deltaTime;
        }}
    }}
    private void OnDestroy()
    {{
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        var dictkeyList = soundDict.Keys.ToList();
        foreach (var eSprite in dictkeyList)
        {{
            ReleaseAsset(eSprite);
        }}
        soundDict.Clear();
        Instance = null;
    }}
}}";

    [MenuItem("Tools/ToolSoundManageGen")]
    public async static void Gen()
    {
        StringBuilder ESoundList = new StringBuilder();
        int goCnt = 0;
        var loadResourceLocationHandle = Addressables.LoadResourceLocationsAsync("sound", typeof(AudioClip));
        await UniTask.WaitWhile(()=>!loadResourceLocationHandle.IsDone);
        var locationList = loadResourceLocationHandle.Result;
        List<string> nameList = new();
        foreach (var location in locationList)
        {
            Addressables.LoadAssetAsync<AudioClip>(location).Completed += (handle) =>
            {
                var go = handle.Result;

                var originName = go.name;
                nameList.Add(originName);
                goCnt++;
            };

        }

        await UniTask.WaitWhile(()=> goCnt != locationList.Count);

        var eSoundFormat = GameUtil.GenerateEnum("ESound", nameList);
        var filePath = $"{Application.dataPath}/1_kds/Scripts/codegen/SoundManager.cs";
        File.WriteAllText(filePath, string.Format(SoundManagerFormat, eSoundFormat));
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        CompilationPipeline.RequestScriptCompilation();

    }
}
#endif