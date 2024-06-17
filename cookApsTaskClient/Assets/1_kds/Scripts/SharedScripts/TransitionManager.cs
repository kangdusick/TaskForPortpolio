using DG.Tweening;
using SRF;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionManager : MonoBehaviour
{
    public DOTweenAnimation fadeAnim;
    private void Awake()
    {
        DontDestroyOnLoad(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
        BasePopup.popupList.Clear();
        BasePopup.currentSortingOrder = 0;
        TMPLinkDetector.popLinkInfoSet.Clear();
        Time.timeScale = 1f;
        CollisionDetectManager.Instance = null;
    }
    private void OnSceneLoaded(Scene scene,LoadSceneMode loadSceneMode)
    {
        if(scene.name != "Transition")
        {
            Debug.Log("Scene Loaded: " + scene.name);
            fadeAnim.DOPlay();
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
    public void BgFadeDone()
    {
        Destroy(gameObject);
    }
}
