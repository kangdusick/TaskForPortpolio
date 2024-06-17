using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCanvas : MonoBehaviour
{
    public static MainCanvas Instance;
    [HideInInspector] public SafeArea safeArea;
    [HideInInspector] public RectTransform rect;
    private void Awake()
    {
        Instance = this;
        safeArea = GetComponentInChildren<SafeArea>();
        rect = GetComponent<RectTransform>();
    }

}
