using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class DetectGroup : CustomCollider2D
{
    [HideInInspector]public List<CollisionGroup> InRangeCollisionGroupList = new(); //찾는 타겟 중에 공격 범위 안에 들어온 리스트
    private Dictionary<ECustomLayerType, ManagedAction<CollisionGroup>> OnAttackRangeEnterAction = new();
    public Dictionary<ECustomLayerType,int> detectPriorityDict= new();
    public List<int> detectPriorityDictPreset = new();
    CharacterRoot characterRoot { get; set; }
    bool isCharacterRootExist;
    private List<CollisionGroup> beforeCollisionGroupList = new();
    [HideInInspector] public HashSet<CustomColliderMonobehaviour> currentDetectTargetSet = new(); //공격 범위 바깥까지 포함한 모든 찾는 타겟 리스트
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
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
    private void OnEnable()
    {
        CollisionDetectManager.Instance.detectGroupList.Add(this);
    }
    public void AddListener_OnAttackRangeEnter(MonoBehaviour actor, Action<CollisionGroup> action)
    {
        var currentCustomLayerTypeList = currentCustomLayerTypeSet.ToArray();
        for (int i = 0;i<currentCustomLayerTypeList.Length;i++)
        {
            if(!OnAttackRangeEnterAction.ContainsKey(currentCustomLayerTypeList[i]))
            {
                OnAttackRangeEnterAction[currentCustomLayerTypeList[i]] = new ManagedAction<CollisionGroup>();
            }
            OnAttackRangeEnterAction[currentCustomLayerTypeList[i]].RemoveListener(action);
            OnAttackRangeEnterAction[currentCustomLayerTypeList[i]].AddListener(actor, action);
        }
    }
    public override void Init(CharacterTable characterTable)
    {
        base.Init(characterTable);
        RefreshDetectPriority(characterTable);
        //if (isCharacterRootExist && characterRoot.isExistCharacterAnimation)
        //{
        //    characterRoot.characterAnimation.OnChangeAnimState.AddListener(this,OnChangeAnimState);
        //}
    }
    public void RefreshDetectPriority(CharacterTable characterTable)
    {
        for (int i = 0; i < detectPriorityDictPreset.Count; i++)
        {
            detectPriorityDict[layerPreset[i]] = detectPriorityDictPreset[i];
        }
        if (!ReferenceEquals(characterTable, null))
        {
            for (int i = 0; i < characterTable.detectorType.Count; i++)
            {
                if (characterTable.detectPriority.Count > i)
                {
                    detectPriorityDict[(ECustomLayerType)characterTable.detectorType[i]] = characterTable.detectPriority[i];
                }
            }
        }
    }
    public void SetDetectTargetSet()
    {
        var currentDetectingPriority = int.MinValue;
        currentDetectTargetSet.Clear();
        foreach (var detectPriorityKeyValue in detectPriorityDict)
        {
            if(CollisionDetectManager.Instance.characterMonobehaviourCollisionDict[detectPriorityKeyValue.Key].Count > 0&& currentCustomLayerTypeSet.Contains(detectPriorityKeyValue.Key))
            {
                if(detectPriorityKeyValue.Value >= currentDetectingPriority)
                {
                    if(currentDetectingPriority != detectPriorityKeyValue.Value)
                    {
                        currentDetectTargetSet.Clear();
                    }
                    currentDetectingPriority = detectPriorityKeyValue.Value;
                    currentDetectTargetSet.AddRange(CollisionDetectManager.Instance.characterMonobehaviourCollisionDict[detectPriorityKeyValue.Key]);
                }
            }
        }

    }
    private void OnChangeAnimState()
    {
        //if (characterRoot.characterAnimation.IsWhileAttackAnim)
        //{
        //    custumScaler = 2f;
        //}
        //else
        //{
            custumScaler = 1f;
        //}
    }
    public void SetInRangeCollisionGroup()
    {
        if (isCharacterRootExist //&& !characterRoot.IsLookingForTarget)
            )
        {
            beforeCollisionGroupList.Clear();
            InRangeCollisionGroupList.Clear();
            return;
        }
        beforeCollisionGroupList.Clear();
        beforeCollisionGroupList.AddRange(InRangeCollisionGroupList);
        InRangeCollisionGroupList.Clear();
        SetDetectTargetSet();

        foreach (var item in currentDetectTargetSet)
        {
            if (Intersects(item.collisionGroup))
            {
                InRangeCollisionGroupList.Add(item.collisionGroup);
            }
        }

        // 새로운 충돌 그룹 중 이전에 없던 것들 찾아서 OnAttackRangeEnter 호출
        foreach (var collisionGroup in InRangeCollisionGroupList)
        {
            if (!beforeCollisionGroupList.Contains(collisionGroup))
            {
                OnAttackRangeEnter(collisionGroup);
            }
        }

        // 이전 충돌 그룹 중에서 더 이상 존재하지 않는 것들 찾아서 OnAttackRangeExit 호출
        foreach (var collisionGroup in beforeCollisionGroupList)
        {
            if (!InRangeCollisionGroupList.Contains(collisionGroup))
            {
                OnAttackRangeExit(collisionGroup);
            }
        }
    }
    public void SetNearestTarget()
    {
        //if (isCharacterRootExist)
        //{
        //    if (!characterRoot.IsLookingForTarget)
        //    {
        //        characterRoot.TargetCharacterMonobehaviour = null;
        //        return;
        //    }
        //    var nearestCollisionGroup = FindNearestCollisionGroup();
        //    if (ReferenceEquals(nearestCollisionGroup, null))
        //    {
        //        characterRoot.TargetCharacterMonobehaviour = null;
        //    }
        //    else
        //    {
        //        characterRoot.TargetCharacterMonobehaviour = nearestCollisionGroup.characterMonobehaviour;
        //    }
        //}
    }

    private CollisionGroup FindNearestCollisionGroup()
    {
        if (currentDetectTargetSet.Count == 0)
        {
            return null;
        }
        else
        {
            return currentDetectTargetSet.MinBy(x => GameUtil.DistanceSquare2D(transform.position, x.transform.position)).collisionGroup;
        }
    }
    protected override void Refresh()
    {
        base.Refresh();
        foreach (var item in customLayerTypeInitSet)
        {
            if (!detectPriorityDict.ContainsKey(item))
            {
                detectPriorityDict[item] = 0;
            }
        }
         isCharacterRootExist = characterMonobehaviour is CharacterRoot;
        if (isCharacterRootExist)
        {
            characterRoot = characterMonobehaviour as CharacterRoot;
        }
    }
    private void OnAttackRangeEnter(CollisionGroup collision)
    {
        var currentCustomLayerTypeList = collision.currentCustomLayerTypeSet.ToArray();
        for (int i = 0; i<currentCustomLayerTypeList.Length; i++) 
        {
            if (OnAttackRangeEnterAction.ContainsKey(currentCustomLayerTypeList[i]))
            {
                OnAttackRangeEnterAction[currentCustomLayerTypeList[i]].Invoke(collision);
            }
        }
    }
    public void OnAttackRangeExit(CollisionGroup collision)
    {
        //if (isCharacterRootExist && characterRoot.isExistCharacterAnimation && characterRoot.characterAnimation.IsWhileAttackAnim && characterRoot.characterAnimation.IsBeforeAttackImpact && ReferenceEquals(characterRoot.TargetCharacterMonobehaviour, collision.characterMonobehaviour))
        //{
        //    SetNearestTarget();
        //    if (InRangeCollisionGroupList.Count == 0)//공격 도중, 임팩트 이전에 타겟이 공격 범위에서 벗어나고, 공격 범위 내에 공격할수있는 대상이 없는 경우
        //    {
        //        characterRoot.AttackCancleOn();
        //    }
        //}
    }
    private void OnDisable()
    {
        //if (isCharacterRootExist && characterRoot.isExistCharacterAnimation)
        //{
        //    characterRoot.characterAnimation.OnChangeAnimState.RemoveListener(OnChangeAnimState);
        //}
        CollisionDetectManager.Instance.detectGroupList.Remove(this);
    }

}
