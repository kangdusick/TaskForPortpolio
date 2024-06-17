using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopCommon : BasePopup
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text descText;
    [SerializeField] Button OkBtn;
    [SerializeField] RectTransform viewPortRect;
    [SerializeField] List<RectTransform> rebuildRectList;
    [SerializeField] LayoutElement descLayoutElement;
    [SerializeField] ContentSizeFitter viewPortContentSizeFitter;
    public void OpenPopup(string title, string desc, Action OnClickOk = null)
    {
        titleText.text = title;

        descLayoutElement.preferredWidth = -1;
        descText.text = desc;
        viewPortContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        foreach (var item in rebuildRectList)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
        }
        if (descText.rectTransform.sizeDelta.x > 1000f)
        {
            descLayoutElement.preferredWidth = 1000f;
        }
        if(descText.rectTransform.sizeDelta.y>400f)
        {
            viewPortContentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            viewPortRect.sizeDelta = new Vector2(viewPortRect.sizeDelta.x,400f);
        }
        foreach (var item in rebuildRectList)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
        }

        base.OpenPopup();
        OkBtn.onClick.RemoveAllListeners();
        OkBtn.onClick.AddListener(() =>
        {
            OnClickOk?.Invoke();
            //OnClose();
        });

    }
    public override void OnClose()
    {
        base.OnClose();
        Application.Quit();
    }
}
