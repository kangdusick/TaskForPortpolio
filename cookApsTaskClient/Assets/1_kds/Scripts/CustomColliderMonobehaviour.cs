using System;
using UnityEngine;
public class CustomColliderMonobehaviour : MonoBehaviour
{
    [HideInInspector] public CollisionGroup collisionGroup = null;
    [HideInInspector] public DetectGroup detectGroup = null;
    private bool _isDie;
    [HideInInspector] public bool isIgnoreDieEvent;
    [HideInInspector]
    public bool IsDie
    {
        get { return _isDie; }
        set
        {
            _isDie = value;
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y);
            if (isExistCollisionGroup)
            {
                if (IsDie)
                {
                    collisionGroup.currentCustomLayerTypeSet.Clear();
                }
                else
                {
                    isIgnoreDieEvent = false;
                    collisionGroup.RefreshJustCustomLayerSet();
                }
            }

        }
    }
    protected bool isExistCollisionGroup = true;
    protected bool isExistDetectGroup = true;
    protected virtual void Awake()
    {
        InitCollisionGroup();
        InitDetectGroup();
    }
    private void InitCollisionGroup()
    {
        if (isExistCollisionGroup && ReferenceEquals(collisionGroup, null))
        {
            try
            {
                collisionGroup = GetComponentInChildren<CollisionGroup>();
                collisionGroup.PresetInit(this);
            }
            catch (Exception e)
            {
                isExistCollisionGroup = false;
            }
        }
    }
    private void InitDetectGroup()
    {
        if (isExistDetectGroup && ReferenceEquals(detectGroup, null))
        {
            try
            {
                detectGroup = GetComponentInChildren<DetectGroup>();
                detectGroup.PresetInit(this);
            }
            catch (Exception e)
            {
                isExistDetectGroup = false;
            }
        }
    }
}
