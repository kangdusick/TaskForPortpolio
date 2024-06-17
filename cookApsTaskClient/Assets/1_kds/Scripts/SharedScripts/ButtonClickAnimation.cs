using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
[RequireComponent(typeof(Button))]
public class ButtonClickAnimation : MonoBehaviour, IPointerDownHandler
{
    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        transform.DOPause();
        transform.DORewind();
        transform.DOKill();
        transform.DOScale(1f, 0.5f).SetRelative().SetEase(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, -0.2f), new Keyframe(0.6f, 0.2f) , new Keyframe(1,0) ));
    }
}
