using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasePopupPrefab : MonoBehaviour
{
    public RectTransform baseRect;
    public Canvas baseCanvas;
    public Image _dimImage;
    [SerializeField] protected Button closeBtn;
    private DOTweenAnimation openAnim = null;
    private readonly string openAnimId = "open";
    private readonly Color _dimColor = new Color(0f, 0f, 0f, 0.5882f);
    public bool isInSafeArea;
    public ESound openSound = ESound.OpenPopup;
    BasePopup basePopup;
    void Awake()
    {
        var doweenAnims = GetComponentsInChildren<DOTweenAnimation>();
        foreach (var anim in doweenAnims)
        {
            if (anim.id == openAnimId)
            {
                openAnim = anim;
                continue;
            }
        }
    }
    public void OpenPopup(BasePopup basePopup)
    {
        this.basePopup = basePopup;
        //SoundManager.Instance.PlaySound(openSound, ignoreTimeScale: openAnim.isIndependentUpdate);
        baseCanvas.sortingOrder = BasePopup.currentSortingOrder;

        baseRect = basePopup.GetComponent<RectTransform>();
        RectTransform parentRect = null;
        if (isInSafeArea)
        {
            parentRect = MainCanvas.Instance.safeArea.rect;
            baseRect.SetParent(parentRect);
            baseRect.sizeDelta = Vector2.zero;
            baseRect.anchorMax = parentRect.anchorMax;
            baseRect.anchorMin = parentRect.anchorMin;
        }
        else
        {
            parentRect = MainCanvas.Instance.rect;
            baseRect.SetParent(parentRect);
            baseRect.sizeDelta = parentRect.sizeDelta;
            baseRect.anchorMax = Vector2.one * 0.5f;
            baseRect.anchorMin = Vector2.one * 0.5f;
        }
        baseRect.localScale = Vector3.one;
        baseRect.anchoredPosition3D = Vector3.zero;



        closeBtn.onClick.RemoveListener(basePopup.OnClose);
        closeBtn.onClick.AddListener(basePopup.OnClose);

        PopTransition(true);
    }
    private void PopTransition(bool isOpen)
    {
        if (isOpen)
        {
            openAnim.DORestart();
            _dimImage.color = Color.clear;
            _dimImage.DOColor(_dimColor, openAnim.duration).SetUpdate(openAnim.isIndependentUpdate);
        }
        else
        {
            openAnim.DOPlayBackwards();// DORestartById(closeAnimId);
            _dimImage.color = _dimColor;
            _dimImage.DOColor(Color.clear, openAnim.duration).SetUpdate(openAnim.isIndependentUpdate).onComplete += OnPoolableDestroyAction;
        }
    }
    private void OnPoolableDestroyAction()
    {
        basePopup.OnPoolableDestroyAction();
    }
    public void OnClose()
    {
        PopTransition(false);
    }



}
