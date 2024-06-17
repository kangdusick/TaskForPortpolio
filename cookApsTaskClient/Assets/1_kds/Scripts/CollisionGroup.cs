using System;
using System.Collections.Generic;
using UnityEngine;

public class CollisionGroup : CustomCollider2D
{
    private ManagedAction onTouching = new();
    private ManagedAction onTouchDown = new();
    private ManagedAction onTouchUp = new();

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (customColliderType == ECustomColliderType.Circle)
        {
            Gizmos.DrawWireSphere(Center, Radius);
        }
        else
        {
            Gizmos.DrawLine(Corners[0], Corners[1]);
            Gizmos.DrawLine(Corners[0], Corners[2]);
            Gizmos.DrawLine(Corners[2], Corners[3]);
            Gizmos.DrawLine(Corners[1], Corners[3]);
        }
    }
#endif
    public void InvokeOnTouching()
    {
        onTouching.Invoke();
    }
    public void InvokeOnTouchDown()
    {
        onTouchDown.Invoke();
    }
    public void InvokeOnTouchUp()
    {
        onTouchUp.Invoke();
    }
    private void EnableCollider2D(bool isEnable)
    {
        if(isEnable)
        {
            gameObject.layer = (int)ELayers.CollisionGroup;
        }
        else
        {
            gameObject.layer = (int)ELayers.Default;
        }
        switch (customColliderType)
        {
            case ECustomColliderType.Circle:
                circleCollider2D.enabled = isEnable;
                break;
            case ECustomColliderType.Box:
                boxCollider2D.enabled = isEnable;
                break;
            case ECustomColliderType.Capsule:
                capsuleCollider2D.enabled = isEnable;
                break;
        }
    }
    public void OnTouchDown(Action action)
    {
        TouchManager.Instance.OnTouchDownCollisionGroupSet.Add(this);
        EnableCollider2D(true);
        onTouchDown.AddListener(this, action);
    }
    public void OnTouchUp(Action action)
    {
        TouchManager.Instance.OnTouchUpCollisionGroupSet.Add(this);
        EnableCollider2D(true);
        onTouchUp.AddListener(this, action);
    }
    public void OnTouching(Action action)
    {
        TouchManager.Instance.OnTouchIngCollisionGroupSet.Add(this);
        EnableCollider2D(true);
        onTouching.AddListener(this, action);
    }
    public void RemoveOnTouchIng(Action action)
    {
        onTouching.RemoveListener(action);
        if(onTouching.listenersCnt <= 0)
        {
            TouchManager.Instance.OnTouchIngCollisionGroupSet.Remove(this);
        }
        DisableCollider2D();
    }
    public void RemoveOnTouchDown(Action action)
    {
        onTouchDown.RemoveListener(action);
        if (onTouchDown.listenersCnt <= 0)
        {
            TouchManager.Instance.OnTouchDownCollisionGroupSet.Remove(this);
        }
        DisableCollider2D();
    }
    public void RemoveOnTouchUp(Action action)
    {
        onTouchUp.RemoveListener(action);
        if (onTouchUp.listenersCnt <= 0)
        {
            TouchManager.Instance.OnTouchUpCollisionGroupSet.Remove(this);
        }
        DisableCollider2D();
    }
    private void DisableCollider2D()
    {
        if (onTouching.listenersCnt <= 0 && onTouchUp.listenersCnt <= 0 && onTouchDown.listenersCnt <= 0)
        {
            EnableCollider2D(false);
        }
    }
    private void OnEnable()
    {
        CollisionDetectManager.Instance.collisionGroupList.Add(this);
        currentCustomLayerTypeSet.OnAddedItem.AddListener(AddObservableList);
        currentCustomLayerTypeSet.OnRemovedItem.AddListener(AddObservableList);
        Refresh();
    }
    private void OnDisable()
    {
        currentCustomLayerTypeSet.OnAddedItem.RemoveListener(AddObservableList);
        currentCustomLayerTypeSet.OnRemovedItem.RemoveListener(AddObservableList);
        CollisionDetectManager.Instance.collisionGroupList.Remove(this);
    }
    List<ECustomLayerType> addedLayerList = new();
    private void SetListInnerRoutine<T>(ObservableCharacterMonobehaviourList<T> targetList,T target, bool isAdd) where T: CustomColliderMonobehaviour
    {
        if(isAdd)
        {
            targetList.Add(target);
        }
        else
        {
            targetList.Remove(target);
        }
    }
    public void AddObservableList(ECustomLayerType eCustomLayer)
    {
        var isAdd = currentCustomLayerTypeSet.Contains(eCustomLayer);
        if(isAdd) 
        {
            if(addedLayerList.Contains(eCustomLayer))
            {
                return;
            }
            else
            {
                addedLayerList.Add(eCustomLayer);
            }
        }
        else
        {
            if (!addedLayerList.Contains(eCustomLayer))
            {
                return;
            }
            else
            {
                addedLayerList.Remove(eCustomLayer);
            }
        }
        SetListInnerRoutine(CollisionDetectManager.Instance.characterMonobehaviourCollisionDict[eCustomLayer], characterMonobehaviour, isAdd);
        switch (eCustomLayer)
        {
            case ECustomLayerType.HexBlockContainer:
                SetListInnerRoutine(CollisionDetectManager.Instance.hexBlockContainerList, characterMonobehaviour.gameObject.GetCashComponent<HexBlockContainer>(), isAdd);
                break;
        }
    }
}
