using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TouchManager : MonoBehaviour
{
    private static TouchManager _instance;
    public static TouchManager Instance
    {

        get
        {
            if (ReferenceEquals(_instance, null))
            {
                GameObject go = new GameObject("TouchManager");
                _instance = go.AddComponent<TouchManager>();
            }
            return _instance;
        }

        private set
        {
            _instance = value;
        }
    }
    public Dictionary<ELayers, ManagedAction<RaycastHit2D, Vector2>> OnTouchDownLayer = new();
    public Dictionary<ELayers, ManagedAction<RaycastHit2D, Vector2>> OnTouchIngLayer = new();
    public Dictionary<ELayers, ManagedAction<RaycastHit2D, Vector2>> OnTouchUpLayer = new();
    public HashSet<CollisionGroup> OnTouchIngCollisionGroupSet = new();
    public HashSet<CollisionGroup> OnTouchDownCollisionGroupSet = new();
    public HashSet<CollisionGroup> OnTouchUpCollisionGroupSet = new();
    public ManagedAction<Vector2> OnTouchDown = new();
    public ManagedAction<Vector2> OnTouchIng = new();
    public ManagedAction<Vector2> OnTouchUp = new();
    public bool isTouching;
    public Vector3 mouseWorldPos;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneUnloaded -= OnSceneUnLoaded;
        SceneManager.sceneUnloaded += OnSceneUnLoaded;
        Refresh();
    }
    private void Refresh()
    {
        foreach (ELayers item in Enum.GetValues(typeof(ELayers)))
        {
            OnTouchDownLayer[item] = new();
            OnTouchIngLayer[item] = new();
            OnTouchUpLayer[item] = new();
        }
        OnTouchDown = new();
        OnTouchIng = new();
        OnTouchUp = new();
        isTouching = false;
    }

    private void LateUpdate()
    {
        if(BasePopup.popupList.Count>0)
        {
            return;
        }
        if (Input.touchCount > 0) //터치패드 환경
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    TouchDown(touch.position);
                }
                if (touch.phase == TouchPhase.Moved)
                {
                    TouchIng(Input.mousePosition);
                }
                if (touch.phase == TouchPhase.Ended) // 손가락을 뗐을 때
                {
                    TouchUp(touch.position);
                }

            }
        }
        else //마우스 환경
        {
            if (Input.GetMouseButtonDown(0))
            {
                TouchDown(Input.mousePosition);
            }
            if (Input.GetMouseButton(0))
            {
                TouchIng(Input.mousePosition);
            }
            if (Input.GetMouseButtonUp(0)) // 마우스 환경, 마우스 버튼을 뗐을 때
            {
                TouchUp(Input.mousePosition);
            }
        }
    }
    private void TouchIng(Vector2 screenPos)
    {
        mouseWorldPos = Camera.main.ScreenToWorldPoint(screenPos);
        mouseWorldPos.z = 0f;

        var mousHits = Physics2D.RaycastAll(mouseWorldPos, Camera.main.transform.forward, 20000);

        foreach (var hit in mousHits)
        {
            OnTouchIngLayer[(ELayers)hit.collider.gameObject.layer].Invoke(hit, screenPos);
            if(hit.collider.gameObject.layer == (int)ELayers.CollisionGroup)
            {
                var collisionGroup = hit.collider.gameObject.GetCashComponent<CollisionGroup>();
                if (!ReferenceEquals(collisionGroup, null) && OnTouchIngCollisionGroupSet.Contains(collisionGroup))
                {
                    collisionGroup.InvokeOnTouching();
                }
            }
        }
        OnTouchIng.Invoke(screenPos);
    }
    private void TouchDown(Vector2 screenPos)
    {
        isTouching = true;
        mouseWorldPos = Camera.main.ScreenToWorldPoint(screenPos);
        mouseWorldPos.z = 0f;
        var mousHits = Physics2D.RaycastAll(mouseWorldPos, Camera.main.transform.forward, 20000);

        foreach (var hit in mousHits)
        {
            OnTouchDownLayer[(ELayers)hit.collider.gameObject.layer].Invoke(hit, screenPos);
            if (hit.collider.gameObject.layer == (int)ELayers.CollisionGroup)
            {
                var collisionGroup = hit.collider.gameObject.GetCashComponent<CollisionGroup>();
                if (!ReferenceEquals(collisionGroup, null) && OnTouchDownCollisionGroupSet.Contains(collisionGroup))
                {
                    collisionGroup.InvokeOnTouchDown();
                }
            }
        }
        OnTouchDown.Invoke(screenPos);
    }
    private void TouchUp(Vector2 screenPos)
    {
        isTouching = false;
        mouseWorldPos = Camera.main.ScreenToWorldPoint(screenPos);
        mouseWorldPos.z = 0f;
        var mousHits = Physics2D.RaycastAll(mouseWorldPos, Camera.main.transform.forward, 20000);

        foreach (var hit in mousHits)
        {
            OnTouchUpLayer[(ELayers)hit.collider.gameObject.layer].Invoke(hit, screenPos);
            if (hit.collider.gameObject.layer == (int)ELayers.CollisionGroup)
            {
                var collisionGroup = hit.collider.gameObject.GetCashComponent<CollisionGroup>();
                if (!ReferenceEquals(collisionGroup, null) && OnTouchUpCollisionGroupSet.Contains(collisionGroup))
                {
                    collisionGroup.InvokeOnTouchUp();
                }
            }
        }
        OnTouchUp.Invoke(screenPos);
    }
    private void OnSceneUnLoaded(Scene scene)
    {
        Refresh();
       
    }
}
