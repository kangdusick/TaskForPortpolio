using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static bool isInGame => SceneManager.GetActiveScene().name == "Game";
    public static GameManager Instance;
    public TMP_Text leftMoveText;
    public static bool isAutoTestMode;
    [SerializeField] TMP_Text leftTopText;
    [SerializeField] TMP_Text frameText;
    [SerializeField] DOTweenAnimation gameClearTextAnim;
    [SerializeField] DOTweenAnimation toyPartyTextAnim;
    [SerializeField] DOTweenAnimation dimImageAnim;
    public RectTransform TopUIRect;
    private int _leftTopCnt;
    public int LeftTopCnt
    {
        get { return _leftTopCnt; }
        set
        {
            _leftTopCnt = value;
            leftTopText.text = _leftTopCnt.ToString();
          
        }
    }
    private int _leftMoveCnt;
    public int LeftMoveCnt
    {
        get { return _leftMoveCnt; }
        set 
        {
            _leftMoveCnt = value;
            leftMoveText.text = _leftMoveCnt.ToString();
            if(LeftMoveCnt<=0 && LeftTopCnt >0)
            {
                if(isAutoTestMode)
                {
                    GameUtil.Instance.LoadScene("Game");
                }
                else
                {
                    PoolableManager.Instance.Instantiate<PopCommon>(EPrefab.PopCommon).OpenPopup("패배","이동 횟수가 부족해요!", () => { GameUtil.Instance.LoadScene("Game"); });
                }
            }
        }
    }
    public ObservableList<HexBlock> topList = new(); // 팽이 리스트
    public float hintCooldown;
    private void Awake()
    {
        Application.targetFrameRate = 60;
        Instance = this;
        LeftMoveCnt = 20;
        topList.OnChanged.AddListener(this, () => { LeftTopCnt = topList.Count; });
        EnableGameClearText(false);
        EnableToyPartyText(false);
        if(isAutoTestMode)
        {
            Time.timeScale = 4f;
        }
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float screenRate = screenWidth / screenHeight;
        float targetWidth = 1080f;
        float targetHeight = 1920f;
        float targetRate = targetWidth / targetHeight;
        Camera.main.orthographicSize = 600f * targetRate / screenRate;
    }
    private void Start()
    {
        DisableAllHintEffect();
    }
    [Button]
    void EnableAutoTestMode()
    {
        isAutoTestMode = true;
        Time.timeScale = 4f;
    }
    public void DisableAllHintEffect()
    {
        if(isAutoTestMode)
        {
            hintCooldown = 0.1f*Time.timeScale;
        }
        else
        {
            hintCooldown = 1f;
        }
        foreach (var item in CollisionDetectManager.Instance.hexBlockContainerList)
        {
            item.EnableHintEffect(false);
        }
    }
    private void Update()
    {
        if(!HexBlockContainer.IsWhileExChange)
        {
            hintCooldown -= Time.deltaTime;
        }
        if (hintCooldown <= 0f && (LeftTopCnt >0) && LeftMoveCnt>0 )
        {
            DisableAllHintEffect();
            var a = HexBlockContainer.GetHintExchangePoint();
            foreach (var item in a)
            {
                item.EnableHintEffect(true);
            }
            if(isAutoTestMode)
            {
                a[0].ExchangeHexBlock(a[0].hexBlock, a[1].hexBlock);
                DisableAllHintEffect();
            }
        }
        frameText.text = (1f / Time.deltaTime).ToString("F1");
    }
    public void EnableGameClearText(bool isEnable)
    {
        dimImageAnim.gameObject.SetActive(isEnable);
        dimImageAnim.DORestart();
        gameClearTextAnim.gameObject.SetActive(isEnable);
        gameClearTextAnim.DORestart();

    }
    public void EnableToyPartyText(bool isEnable)
    {
        dimImageAnim.gameObject.SetActive(isEnable);
        dimImageAnim.DORestart();
        toyPartyTextAnim.gameObject.SetActive(isEnable);
        toyPartyTextAnim.DORestart();
    }
}
