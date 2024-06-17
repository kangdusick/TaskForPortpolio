using System.Collections;
using UnityEngine;

public class SafeArea : MonoBehaviour
{
    [HideInInspector] public RectTransform rect;
    Rect _safeArea;
    Vector2 _minAnchor;
    Vector2 _maxAnchor;
    void Awake()
    {
        StartCoroutine(RotationDetect());
    }
    IEnumerator RotationDetect()
    {
        //화면이 회전했을때 자동으로 ui 재설정
        ScreenOrientation beforeOrientation = 0;
        while (true)
        {
            if (beforeOrientation != Screen.orientation)
            {
                beforeOrientation = Screen.orientation;
                SetSafeArea();
            }
            yield return TimeManager.GetWaitForSeconds(2f);
        }
    }
    public void SetSafeArea()
    {
        rect = GetComponent<RectTransform>();
        rect.sizeDelta = Vector2.zero;
        _safeArea = Screen.safeArea;
        _minAnchor = _safeArea.position;
        _maxAnchor = _minAnchor + _safeArea.size;

        _minAnchor.x /= Screen.width;
        _minAnchor.y /= Screen.height;
        _maxAnchor.x /= Screen.width;
        _maxAnchor.y /= Screen.height;

        rect.anchorMin = _minAnchor;
        rect.anchorMax = _maxAnchor;
    }
}
