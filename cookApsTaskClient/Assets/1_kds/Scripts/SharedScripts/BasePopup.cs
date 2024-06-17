using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks.Triggers;
using Sirenix.OdinInspector;

public class BasePopup : MonoBehaviour
{
    public static int currentSortingOrder;
    public static List<BasePopup> popupList = new();
    protected BasePopupPrefab basePopupPrefab;
    private bool _isClosed;
    public Canvas baseCanvas => basePopupPrefab.baseCanvas;
    protected virtual void Awake()
    {
        basePopupPrefab = GetComponentInChildren<BasePopupPrefab>();
    }
    public virtual void OpenPopup()
    {
        Debug.Log("Open " +  gameObject.name);
        //if (GameManager.isInGame)
        //{
        //    Time.timeScale = 0f;
        //}
        _isClosed = false;
        currentSortingOrder += 100;
        popupList.Add(this);
        basePopupPrefab.OpenPopup(this);
    }
    public virtual void OnClose()
    {
        if (_isClosed)
        {
            return;
        }
        currentSortingOrder -= 100;
        _isClosed = true;
        basePopupPrefab.OnClose();
    }
    public static T GetPopup<T>() where T : BasePopup
    {
        foreach (var item in popupList)
        {
            if (item.GetType() == typeof(T))
            {
                return (T)item;
            }
        }
        return null;
    }
    public virtual void OnPoolableDestroyAction()
    {
        popupList.Remove(this);
        PoolableManager.Instance.DestroyWithChildren(gameObject);
    }
    protected virtual void OnDestroy()
    {
        if (_isClosed)
        {
            return;
        }
        _isClosed = true;
        popupList.Remove(this);
    }
}