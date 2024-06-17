using UnityEngine;
using System.Collections;
public class MaskTMPLinkDetector : MonoBehaviour
{
    RectTransform maskRect;
    private int totalChildCount;
    private void Awake()
    {
        maskRect = GetComponent<RectTransform>();
    }
    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(CheckRoutine());
    }
    IEnumerator CheckRoutine()
    {
        while (true)
        {
            int currentCount = GetTotalChildCount(transform);
            if (currentCount != totalChildCount)
            {
                totalChildCount = currentCount;
                CheckForTMPLinkDetector();
            }
            yield return TimeManager.GetWaitForSeconds(1f);
        }
    }
    private void CheckForTMPLinkDetector()
    {
        var linkDetectorlist = transform.GetComponentsInChildren<TMPLinkDetector>();
        foreach (var linkDetector in linkDetectorlist)
        {
            linkDetector.SetMaskRect(maskRect);
        }
    }
    private int GetTotalChildCount(Transform parent)
    {
        int count = parent.childCount;
        foreach (Transform child in parent)
        {
            count += GetTotalChildCount(child);
        }
        return count;
    }
}
