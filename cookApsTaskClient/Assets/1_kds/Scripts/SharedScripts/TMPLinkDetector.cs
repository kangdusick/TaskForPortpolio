using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using Sirenix.OdinInspector;
using UnityEngine.UI;

public class KMP
{
    // KMP 부분 일치 테이블 생성
    private static int[] BuildPartialMatchTable(string pattern)
    {
        int[] lps = new int[pattern.Length];
        int length = 0;
        lps[0] = 0; // lps[0]은 항상 0입니다.

        int i = 1;
        while (i < pattern.Length)
        {
            if (pattern[i] == pattern[length])
            {
                length++;
                lps[i] = length;
                i++;
            }
            else
            {
                if (length != 0)
                {
                    length = lps[length - 1];
                }
                else
                {
                    lps[i] = 0;
                    i++;
                }
            }
        }

        return lps;
    }

    // KMP 검색 알고리즘
    public static List<int> KMPSearch(string text, string pattern)
    {
        List<int> positions = new List<int>();
        int[] lps = BuildPartialMatchTable(pattern);

        int j = 0; // 패턴의 인덱스
        int i = 0; // 텍스트의 인덱스
        while (i < text.Length)
        {
            if (pattern[j] == text[i])
            {
                j++;
                i++;
            }

            if (j == pattern.Length)
            {
                positions.Add(i - j);
                j = lps[j - 1];
            }

            // 불일치 발생
            else if (i < text.Length && pattern[j] != text[i])
            {
                if (j != 0)
                    j = lps[j - 1];
                else
                    i = i + 1;
            }
        }

        return positions;//일치하는 문자가 있다면, 그 문자의 시작 인덱스들을 반환함
    }
}

[RequireComponent(typeof(TMP_Text))]
[DisallowMultipleComponent]
public class TMPLinkDetector : MonoBehaviour
{
    private TMP_Text tmpText;
    private BoxCollider2D boxCollider2D;
    private HashSet<RectTransform> maskRectSet = new();
    private bool isMaskRectExist;
    public static HashSet<string> popLinkInfoSet = new HashSet<string>();
    string savedLinkID;
    Canvas baseCanvas;
    float refreshTimer = 0f;

    private void Start()
    {
        savedLinkID = string.Empty;
        tmpText = GetComponent<TMP_Text>();
        boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
        boxCollider2D.isTrigger = true;
        gameObject.layer = (int)ELayers.TMPLink;
        UpdateColliderSizeAndOffset();
        TouchManager.Instance.OnTouchDownLayer[ELayers.TMPLink].RemoveListener(OnTouchDown);
        TouchManager.Instance.OnTouchDownLayer[ELayers.TMPLink].AddListener(this, OnTouchDown);
        TouchManager.Instance.OnTouchUpLayer[ELayers.TMPLink].RemoveListener(OnTouchUp);
        TouchManager.Instance.OnTouchUpLayer[ELayers.TMPLink].AddListener(this, OnTouchUp);

        baseCanvas = FindFirstParentCanvas(transform);
    }
    Canvas FindFirstParentCanvas(Transform current)
    {
        while (current.parent != null)
        {
            // Move to the parent object
            current = current.parent;
            // Try to find a Canvas component in the current parent
            Canvas canvas = current.GetComponent<Canvas>();
            if (canvas != null)
            {
                // Return the first found Canvas
                return canvas;
            }
        }
        // Return null if no Canvas is found
        return null;
    }
    private void OnTouchDown(RaycastHit2D hit, Vector2 screenPos)
    {
        if (baseCanvas.sortingOrder < BasePopup.currentSortingOrder)
        {
            return;
        }
        SaveTouchDownLinkID(screenPos);
    }
    private void OnTouchUp(RaycastHit2D hit, Vector2 screenPos)
    {
        if (baseCanvas.sortingOrder < BasePopup.currentSortingOrder)
        {
            return;
        }
        CheckLinkAndPopInfo(screenPos);
    }
    public void SetMaskRect(RectTransform maskRect)
    {
        maskRectSet.Add(maskRect);
        isMaskRectExist = true;
    }
    private static string ReplaceWithLinks(string originalText, string languageKey)
    {
        List<(LinkTable linkTable, int startIndex, int endIndex)> linkList = new();
        string originalTextLower = originalText.ToLower();

        foreach (var linkTable in TableManager.LinkTableDict.Values)
        {
            string linkText = languageKey == "kr" ? linkTable.kr : linkTable.en;
            string linkTextLower = linkText.ToLower();

            // KMP 알고리즘을 사용하여 모든 매칭 위치 찾기
            List<int> matchPositions = KMP.KMPSearch(originalTextLower, linkTextLower);

            foreach (int position in matchPositions)
            {
                int endIndex = position + linkText.Length - 1;
                linkList.Add((linkTable, position, endIndex));
            }
        }

        // 겹치는 링크 제거
        RemoveOverlappingLinks(ref linkList);

        // linkList를 startIndex 기준으로 정렬
        linkList.Sort((a, b) => a.startIndex.CompareTo(b.startIndex));

        // 교체로 인한 문자열 길이의 변화를 추적
        int lengthDelta = 0;

        foreach (var item in linkList)
        {
            string linkText = languageKey == "kr" ? item.linkTable.kr : item.linkTable.en;
            string replacement = $"<link={item.linkTable.key}><u>{linkText}</u></link>";

            // 현재 링크의 시작 및 끝 인덱스 조정
            int adjustedStartIndex = item.startIndex + lengthDelta;
            int adjustedEndIndex = item.endIndex + lengthDelta;

            // 교체 및 길이 변화 계산
            originalText = ReplaceTextAt(originalText, replacement, adjustedStartIndex, adjustedEndIndex);
            lengthDelta += replacement.Length - (item.endIndex - item.startIndex + 1);
        }

        return originalText;
    }

    private static string ReplaceTextAt(string text, string replacement, int startIndex, int endIndex)
    {
        string before = text.Substring(0, startIndex);
        string after = text.Substring(endIndex + 1);
        return before + replacement + after;
    }

    private static void RemoveOverlappingLinks(ref List<(LinkTable linkTable, int startIndex, int endIndex)> linkList)
    {
        for (int i = 0; i < linkList.Count; i++)
        {
            for (int j = i + 1; j < linkList.Count; j++)
            {
                if (IsOverlapping(linkList[i], linkList[j]))
                {
                    // 겹치는 경우, 길이가 짧은 링크를 제거
                    if (linkList[i].endIndex - linkList[i].startIndex <= linkList[j].endIndex - linkList[j].startIndex)
                    {
                        linkList.RemoveAt(i);
                        i--; // 인덱스 조정
                        break; // 현재 링크(i)가 변경되었으므로 루프를 중단하고 다음 링크로 이동
                    }
                    else
                    {
                        linkList.RemoveAt(j);
                        j--; // 인덱스 조정
                    }
                }
            }
        }
    }

    private static bool IsOverlapping((LinkTable linkText, int startIndex, int endIndex) link1, (LinkTable linkText, int startIndex, int endIndex) link2)
    {
        return link1.startIndex <= link2.endIndex && link1.endIndex >= link2.startIndex;
    }
    public static void UpdateLanguageTablesWithLinks()
    {
        foreach (var languageTable in TableManager.LanguageTableDict.Values)
        {
            if (KMP.KMPSearch(languageTable.key, "_NoLink").Count == 0) //_NoLink가 붙어있으면 링크 안붙임
            {
                languageTable.kr = ReplaceWithLinks(languageTable.kr, "kr");
                languageTable.en = ReplaceWithLinks(languageTable.en, "en");
            }
        }
    }
    private string GetLinkID(Vector2 screenPoint)
    {
        if (IsInsideMask(screenPoint))
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(tmpText, screenPoint, Camera.main);
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = tmpText.textInfo.linkInfo[linkIndex];
                return linkInfo.GetLinkID();
            }
        }
        return string.Empty;
    }
    public void SaveTouchDownLinkID(Vector2 screenPoint)
    {
        savedLinkID = GetLinkID(screenPoint);
    }
    public void CheckLinkAndPopInfo(Vector2 screenPoint)
    {
        if (savedLinkID == string.Empty)
        {
            return;
        }
        var linkID = GetLinkID(screenPoint);
        if (linkID != savedLinkID || linkID == string.Empty)
        {
            return;
        }
        savedLinkID = string.Empty;
        var title = string.Empty;
        var descText = string.Empty;
            var eLinkTable = linkID.ParseEnum<ELinkTable>();
            var linkTable = TableManager.LinkTableDict[eLinkTable];
            switch (GameUtil.language)
            {
                case "kr":
                    title = linkTable.kr;
                    break;
                default:
                    title = linkTable.en;
                    break;
            }
            descText = string.Empty;
            switch (eLinkTable)
            {
                default:
                    descText = (linkTable.LanguageKey).LocalIzeText();
                    break;
            }
        

        if (!popLinkInfoSet.Contains(linkID))
        {
            //popLinkInfoSet.Add(linkID);
            //var popCommon = PoolableManager.Instance.Instantiate<PopCommon>(EPrefab.PopCommon);
            //popCommon.OpenPopup(title, descText);
            //PoolableManager.Instance.goEprefabFinder[popCommon.gameObject].OnDestroy.AddListener(this, () => { OnDestroyPopup(linkID); });
        }
    }
    private void OnDestroyPopup(string linkID)
    {
        popLinkInfoSet.Remove(linkID);
    }
    private void Update()
    {
        refreshTimer -= Time.unscaledDeltaTime;
        if (refreshTimer <= 0f)
        {
            refreshTimer = 1f;
            UpdateColliderSizeAndOffset();
        }
    }
    [Button]
    private void UpdateColliderSizeAndOffset()
    {
        boxCollider2D.enabled = false;
        boxCollider2D.enabled = true;
        RectTransform rectTransform = tmpText.rectTransform;
        RectTransform parentRectTransform = rectTransform.parent as RectTransform;

        parentRectTransform = rectTransform.parent.GetComponent<RectTransform>();

        // 실제 크기를 계산합니다.
        float width = parentRectTransform.rect.width * (rectTransform.anchorMax.x - rectTransform.anchorMin.x);
        float height = parentRectTransform.rect.height * (rectTransform.anchorMax.y - rectTransform.anchorMin.y);
        if (rectTransform.anchorMin.x == rectTransform.anchorMax.x)
        {
            width = rectTransform.sizeDelta.x;
        }
        if (rectTransform.anchorMin.y == rectTransform.anchorMax.y)
        {
            height = rectTransform.sizeDelta.y;
        }
        boxCollider2D.size = new Vector2(width, height);
        boxCollider2D.offset = new Vector2((0.5f - rectTransform.pivot.x) * width, (0.5f - rectTransform.pivot.y) * height);
    }
    private bool IsInsideMask(Vector2 screenPoint) //마스크 안에 있는 글씨만 터치되도록
    {
        if (isMaskRectExist)
        {
            foreach (var item in maskRectSet)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(item, screenPoint, Camera.main))
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            return true;
        }
    }
}