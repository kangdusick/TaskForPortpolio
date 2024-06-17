using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class GameUtil : MonoBehaviour
{
    // 히어로의 기본 소재
    public Material heroDefaultMat;
    // 싱글톤 인스턴스
    public static GameUtil Instance;
    private static string _language;
    // 언어 설정 프로퍼티
    public static string language
    {
        get { return _language; }
        set
        {
            _language = value;
            ES3.Save(KeyPlayerPrefs.language, _language);
        }
    }

    // 초기화 작업을 수행
    private void Awake()
    {
        if (ReferenceEquals(Instance, null))
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (!ES3.KeyExists(KeyPlayerPrefs.language))
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Korean:
                    language = "kr";
                    break;
                default:
                    language = "en";
                    break;
            }
        }
        language = ES3.Load(KeyPlayerPrefs.language, defaultValue: "en");
    }

    // 프레임이 낮은지 확인
    public static bool IsLowFrame => 40f <= Time.timeScale / Time.deltaTime;

    // 등급에 따른 색상을 반환
    public Color GetGradeColor(int grade)
    {
        switch (grade)
        {
            case 1:
                return new Color(0.5f, 1f, 0.5f); // 연두색 (Lime Green)
            case 2:
                return Color.blue; // 파란색 (Blue)
            case 3:
                return new Color(0.5f, 0f, 0.5f); // 보라색 (Purple)
            default:
                return Color.white;
        }
    }

    // 카메라 외부의 랜덤 위치를 반환
    public Vector3 GetRandomPositionOutsideCamera()
    {
        Vector3 randomViewportPosition = Vector3.zero;
        int edge = Random.Range(0, 4); // 0: 위, 1: 아래, 2: 왼쪽, 3: 오른쪽

        switch (edge)
        {
            case 0: // 위쪽 가장자리
                randomViewportPosition = new Vector3(Random.Range(0f, 1f), 1.1f, Camera.main.nearClipPlane);
                break;
            case 1: // 아래쪽 가장자리
                randomViewportPosition = new Vector3(Random.Range(0f, 1f), -0.1f, Camera.main.nearClipPlane);
                break;
            case 2: // 왼쪽 가장자리
                randomViewportPosition = new Vector3(-0.1f, Random.Range(0f, 1f), Camera.main.nearClipPlane);
                break;
            case 3: // 오른쪽 가장자리
                randomViewportPosition = new Vector3(1.1f, Random.Range(0f, 1f), Camera.main.nearClipPlane);
                break;
        }

        // 뷰포트 좌표를 월드 좌표로 변환
        Vector3 worldPosition = Camera.main.ViewportToWorldPoint(randomViewportPosition);
        worldPosition.z = 0; // 2D 게임에서는 Z축을 0으로 설정
        return worldPosition;
    }

    // 카메라 내부의 랜덤 위치를 반환
    public Vector3 GetRandomPositionInsideCamera()
    {
        Vector3 viewportPoint = new Vector3(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), Camera.main.nearClipPlane);
        return (Camera.main.ViewportToWorldPoint(viewportPoint));
    }

    // 씬을 비동기 로드
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync("Transition", () =>
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }));
    }

    // 씬을 비동기로 로드하는 코루틴
    private IEnumerator LoadSceneAsync(string sceneName, Action onLoaded = null)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        yield return asyncOperation; // AsyncOperation이 완료될 때까지 대기
        yield return TimeManager.GetWaitForSeconds(1f);
        onLoaded?.Invoke(); // AsyncOperation 완료 후 onLoaded 호출
    }


    // 직선 이동을 처리하는 병렬 작업 구조체
    [BurstCompile]
    private struct StraightMoveJob : IJobParallelForTransform
    {
        public Vector3 startPos;
        public Vector3 targetPos;
        public float currentTime;
        public float TotalTime;
        public void Execute(int i, TransformAccess transform)
        {
            float next_calX = startPos.x + ((targetPos.x - startPos.x) / TotalTime * currentTime);
            float next_calY = startPos.y + ((targetPos.y - startPos.y) / TotalTime * currentTime);
            transform.position = new Vector3(next_calX, next_calY, 0f);
        }
    }

    // 포물선 이동을 처리하는 병렬 작업 구조체
    [BurstCompile]
    private struct ParabolaMoveJob : IJobParallelForTransform
    {
        public Vector3 startPos;
        public Vector3 targetPos;
        public Quaternion startAngle;
        public float currentTime;
        public float TotalTime;
        public float a;
        public float b;
        public float c;
        public bool isRotateWithTangent;
        public void Execute(int i, TransformAccess transform)
        {
            float next_calX = startPos.x - ((startPos.x - targetPos.x) / TotalTime * currentTime);
            float next_calY = a * Mathf.Pow(next_calX, 2) + b * next_calX + c;
            transform.position = new Vector3(next_calX, next_calY, 0f);

            if (isRotateWithTangent)
            {
                float slopeX = 1;
                float slopeY = 2 * a * next_calX + b;
                float angleRadians = Mathf.Atan2(slopeY, slopeX); // 기울기에 따른 각도 계산
                transform.rotation = Quaternion.Euler(0f, 0f, startAngle.eulerAngles.z + angleRadians * Mathf.Rad2Deg - 90);
            }
        }
    }

    // 포물선 길이를 계산하는 병렬 작업 구조체
    [BurstCompile]
    private struct ParabolaLengthJob : IJob
    {
        public float a;
        public float b;
        public float x1;
        public float x2;
        public int numIntervals;
        public NativeArray<float> length;
        public void Execute()
        {
            float deltaX = (x2 - x1) / numIntervals;
            float len = 0f;
            float x = x1;

            for (int i = 0; i < numIntervals; i++)
            {
                float yStart = Mathf.Sqrt(1f + Mathf.Pow(2 * a * x + b, 2));
                x += deltaX;
                float yEnd = Mathf.Sqrt(1f + Mathf.Pow(2 * a * x + b, 2));

                len += (yStart + yEnd) * 0.5f * deltaX;
            }
            length[0] = len;
        }
    }

    // 병렬로 직선 이동을 처리하는 메서드
    public CancellationTokenSource StraightMoveForParallel(Transform moveTarget, Vector3 destinePos, float totalTime, Action OnEndAction, bool isSpeedBase = false, bool isIgnoreTimeScale = false)
    {
        var cancleationTokenSource = new CancellationTokenSource();
        StraightMoveRoutine(moveTarget, destinePos, totalTime, OnEndAction, isSpeedBase, isIgnoreTimeScale, cancleationTokenSource).Forget();
        return cancleationTokenSource;
    }

    // 비동기적으로 직선 이동을 처리하는 코루틴
    private async UniTask StraightMoveRoutine(Transform moveTarget, Vector3 destinePos, float totalTime, Action onEndAction, bool isSpeedBase, bool isIgnoreTimeScale, CancellationTokenSource cancleationTokenSource)
    {
        bool isMoveDone = false;
        TransformAccessArray transformAccessArray = new TransformAccessArray(1);
        transformAccessArray.Add(moveTarget);

        try
        {
            if (isSpeedBase)
            {
                totalTime = Vector2.Distance(moveTarget.transform.position, destinePos) / totalTime;  // totalTime을 속도로 다시 계산
            }
            var straightJob = new StraightMoveJob
            {
                startPos = moveTarget.position,
                targetPos = destinePos,
                TotalTime = totalTime,
                currentTime = 0f,
            };

            while (straightJob.currentTime < straightJob.TotalTime)
            {
                cancleationTokenSource.Token.ThrowIfCancellationRequested();  // 취소 요청 검사

                straightJob.currentTime += isIgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                if (straightJob.currentTime >= straightJob.TotalTime)
                {
                    straightJob.currentTime = straightJob.TotalTime;
                    isMoveDone = true;
                }
                if (transformAccessArray.isCreated)
                {
                    straightJob.Schedule(transformAccessArray);
                }
                await UniTask.DelayFrame(1);
            }
        }
        finally
        {
            if (transformAccessArray.isCreated)
            {
                transformAccessArray.Dispose();
            }
            cancleationTokenSource.Dispose();
        }

        if (isMoveDone && moveTarget != null)
        {
            onEndAction?.Invoke();
        }
    }

    // 병렬로 포물선 이동을 처리하는 메서드
    public CancellationTokenSource ParabolaMoveForParallel(Transform moveTarget, Vector3 destinePos, float endAngle, float totalTime, Action OnEndAction, bool isRotateWithTangent, bool isSpeedBase = false, bool isIgnoreTimeScale = false)
    {
        var cancleationTokenSource = new CancellationTokenSource();
        ParabolaMoveRoutine(moveTarget, destinePos, endAngle, totalTime, OnEndAction, isRotateWithTangent, isSpeedBase, isIgnoreTimeScale, cancleationTokenSource).Forget();
        return cancleationTokenSource;
    }

    // 비동기적으로 포물선 이동을 처리하는 코루틴
    private async UniTask ParabolaMoveRoutine(Transform moveTarget, Vector3 destinePos, float endAngle, float totalTime, Action onEndAction, bool isRotateWithTangent, bool isSpeedBase, bool isIgnoreTimeScale, CancellationTokenSource cancleationTokenSource)
    {
        bool isMoveDone = false;
        TransformAccessArray transformAccessArray = new TransformAccessArray(1);
        transformAccessArray.Add(moveTarget);

        float slope = Mathf.Tan(endAngle / 180f * Mathf.PI);
        float equation_a = ((destinePos.y - moveTarget.position.y) - (slope * (destinePos.x - moveTarget.position.x))) / (destinePos.x * destinePos.x - moveTarget.position.x * moveTarget.position.x - (2 * destinePos.x * (destinePos.x - moveTarget.position.x)));
        float equation_b = slope - 2 * equation_a * destinePos.x;
        float equation_c = moveTarget.position.y - equation_a * moveTarget.position.x * moveTarget.position.x - equation_b * moveTarget.position.x;

        try
        {
            if (isSpeedBase)
            {
                NativeArray<float> lengthArray = new NativeArray<float>(1, Allocator.TempJob);
                try
                {
                    var minX = Math.Min(moveTarget.position.x, destinePos.x);
                    var maxX = Math.Max(moveTarget.position.x, destinePos.x);
                    var lengthJob = new ParabolaLengthJob
                    {
                        a = equation_a,
                        b = equation_b,
                        x1 = minX,
                        x2 = maxX,
                        numIntervals = 200,
                        length = lengthArray
                    };

                    JobHandle lengthJobHandle = lengthJob.Schedule();
                    await UniTask.DelayFrame(1);
                    lengthJobHandle.Complete();

                    float parabolaLength = lengthArray[0];
                    totalTime = parabolaLength / totalTime;  // totalTime을 속도로 다시 계산
                }
                finally
                {
                    lengthArray.Dispose();
                }
            }

            var parabolaJob = new ParabolaMoveJob
            {
                startPos = moveTarget.position,
                targetPos = destinePos,
                startAngle = moveTarget.rotation,
                TotalTime = totalTime,
                currentTime = 0f,
                a = equation_a,
                b = equation_b,
                c = equation_c,
                isRotateWithTangent = isRotateWithTangent
            };

            while (parabolaJob.currentTime < parabolaJob.TotalTime)
            {
                cancleationTokenSource.Token.ThrowIfCancellationRequested();  // 취소 요청 검사

                parabolaJob.currentTime += isIgnoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
                if (parabolaJob.currentTime >= parabolaJob.TotalTime)
                {
                    parabolaJob.currentTime = parabolaJob.TotalTime;
                    isMoveDone = true;
                }
                if (transformAccessArray.isCreated)
                {
                    parabolaJob.Schedule(transformAccessArray);
                }
                await UniTask.DelayFrame(1);
            }
        }
        finally
        {
            if (transformAccessArray.isCreated)
            {
                transformAccessArray.Dispose();
            }
            cancleationTokenSource.Dispose();
        }

        if (isMoveDone && moveTarget != null)
        {
            onEndAction?.Invoke();
        }
    }

    // 두 지점 간의 기울기를 계산
    public static float Cal_Slope(Vector2 from, Vector2 to)
    {
        float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;
        return angle;
    }

    // 2D 평면에서 두 지점 간의 거리 제곱을 계산
    public static float DistanceSquare2D(Vector2 pos1, Vector2 pos2)
    {
        return (pos1.x - pos2.x) * (pos1.x - pos2.x) + (pos1.y - pos2.y) * (pos1.y - pos2.y);
    }

    public static float DistanceSquare2D(Vector3 pos1, Vector3 pos2)
    {
        return (pos1.x - pos2.x) * (pos1.x - pos2.x) + (pos1.y - pos2.y) * (pos1.y - pos2.y);
    }

    public static float DistanceSquare2D(Vector2 pos1, Vector3 pos2)
    {
        return (pos1.x - pos2.x) * (pos1.x - pos2.x) + (pos1.y - pos2.y) * (pos1.y - pos2.y);
    }

    public static float DistanceSquare2D(Vector3 pos1, Vector2 pos2)
    {
        return (pos1.x - pos2.x) * (pos1.x - pos2.x) + (pos1.y - pos2.y) * (pos1.y - pos2.y);
    }

    // 이름을 코드명으로 변환
    public static string ConvertNameToCodeName(string name)
    {
        StringBuilder convertName = new();
        if (char.IsDigit(name[0]))
        {
            convertName.Append("_");
        }
        convertName.Append(name);
        return convertName.ToString().Replace('.', '_').Replace(' ', '_').Replace('-', '_').Replace('@', '_').Replace('(', '_').Replace(')', '_').Replace('/', '_').Replace('[', '_').Replace(']', '_');
    }

    // 월드 좌표를 캔버스 좌표로 변환
    public static Vector3 ConvertWorldPosToCanvasPos(Vector3 WorldPosition)
    {
        RectTransform rectTransform = MainCanvas.Instance.rect;
        Vector3 worldToScreenPoint = Camera.main.WorldToScreenPoint(WorldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, worldToScreenPoint, Camera.main, out Vector2 canvasPosition);
        return canvasPosition;
    }

    // 커스텀 해시를 생성
    public static int GenerateCustomHash(string input, int initialHash = 7)
    {
        const int prime = 31;
        int hash = initialHash;

        unchecked
        {
            foreach (char c in input)
            {
                hash = (hash * prime) ^ c;
            }
        }

        return hash;
    }

    // 문자열 리스트로 커스텀 해시 그룹을 생성
    public static List<int> GenerateCustomHashGroup(List<string> inputs)
    {
        var result = new List<int>();
        var hashSet = new HashSet<int>();
        foreach (var input in inputs)
        {
            int hash = GenerateCustomHash(input);
            while (hashSet.Contains(hash))
            {
                hash = GenerateCustomHash(input, hash + 1);
            }
            result.Add(hash);
            hashSet.Add(hash);
        }

        return result;
    }

    // 열거형을 생성
    public static string GenerateEnum(string enumName, List<string> nameList, bool isNoHash = false)
    {
        var EnumFormat =
@"public enum {0}
{{
    {1}
}}

";
        StringBuilder stringBuilder = new();
        var hashGroup = GenerateCustomHashGroup(nameList);
        for (int i = 0; i < nameList.Count; i++)
        {
            var originName = nameList[i];
            var convertedName = ConvertNameToCodeName(originName);
            Debug.Log(originName);
            stringBuilder.Append($"[Description(\"{originName}\")]\n");
            stringBuilder.Append($"\t{convertedName} = {(isNoHash ? i : hashGroup[i])},\n\t");
        }
        var enumFormat = string.Format(EnumFormat, enumName, stringBuilder.ToString());

        return enumFormat.ToString();
    }

    // 부모 오브젝트의 자식을 모두 파괴
    public void DestroyChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }



}
