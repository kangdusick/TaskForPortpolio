using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum ECustomLayerType
{
    HexBlockContainer = 0,
}
public enum ECustomColliderType
{
    Circle,
    Box,
    Capsule,
}
public class CollisionDetectManager : MonoBehaviour
{
    private static CollisionDetectManager _instance;
    public static CollisionDetectManager Instance
    {
        get
        {
            if (ReferenceEquals(_instance, null))
            {
                GameObject go = new GameObject("CollisionDetectManager");
                _instance = go.AddComponent<CollisionDetectManager>();
            }
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }
    public CollisionDetectManager()
    {
        foreach (ECustomLayerType item in Enum.GetValues(typeof(ECustomLayerType)))
        {
            characterMonobehaviourCollisionDict[item] = new();
        }
    }
    public Dictionary<ECustomLayerType, ObservableCharacterMonobehaviourList<CustomColliderMonobehaviour>> characterMonobehaviourCollisionDict = new();
    public ObservableCharacterMonobehaviourList<HexBlockContainer> hexBlockContainerList = new();

    public List<DetectGroup> detectGroupList = new();
    public List<CollisionGroup> collisionGroupList = new();
    private void FixedUpdate()
    {
        var collisionCash = collisionGroupList.ToArray();
        for (int i = 0; i < collisionCash.Length; i++)
        {
            collisionCash[i].AreaUpdate();
        }
        var detectCash = detectGroupList.ToArray();
        for (int i = 0; i < detectCash.Length; i++)
        {
            detectCash[i].AreaUpdate();
            detectCash[i].SetInRangeCollisionGroup();
            detectCash[i].SetNearestTarget();
        }
    }
}
public class CustomCollider2D : MonoBehaviour
{
    public CustomColliderMonobehaviour characterMonobehaviour { get; private set; }
    protected CircleCollider2D circleCollider2D;
    protected BoxCollider2D boxCollider2D;
    protected CapsuleCollider2D capsuleCollider2D;
    [SerializeField] protected List<ECustomLayerType> layerPreset;
    protected HashSet<ECustomLayerType> customLayerTypeInitSet = new();
    [HideInInspector] public ObservableHashSet<ECustomLayerType> currentCustomLayerTypeSet = new();
    private bool isPresetAwake;
    private bool isAwake;
    protected ECustomColliderType customColliderType;
    protected float custumScaler = 1f;
    [HideInInspector]public float Scaler = 1f;
    public Vector2 Center { get; set; }
    public float Radius { get; set; }
    public Vector2[] Corners { get; private set; }
    public (float Radius, Vector2 Center) GetCustomColliderArea(CircleCollider2D circleCollider)
    {
        float worldScale = Mathf.Abs(circleCollider.transform.lossyScale.x)* custumScaler* Scaler; // x 또는 y 중 하나를 사용, 가정은 동일한 scale입니다.
        float Radius = circleCollider.radius * worldScale;

        // 회전을 고려하여 올바른 중심 위치를 계산합니다.
        Vector2 worldOffset = circleCollider.transform.TransformVector(circleCollider.offset);
        Vector2 Center = (Vector2)circleCollider.transform.position + worldOffset;
        return (Radius, Center);
    }

    public (Vector2[] corners ,Vector2 center) GetCustomColliderArea(BoxCollider2D boxCollider)
    {
        Vector2 center = (Vector2)(boxCollider.transform.position +
                         boxCollider.transform.TransformVector(boxCollider.offset));
        Vector2 size = boxCollider.size * custumScaler* Scaler;
        size.x *= Mathf.Abs(boxCollider.transform.lossyScale.x);
        size.y *= Mathf.Abs(boxCollider.transform.lossyScale.y);

        float angle = -boxCollider.transform.eulerAngles.z; // Z-axis rotation in degrees
        float rad = angle * Mathf.Deg2Rad; // Convert degrees to radians

        // Calculate the rotation matrix
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        Vector2[] corners = new Vector2[4];
        corners[0] = center + new Vector2(-size.x * cos - size.y * sin, size.x * sin - size.y * cos) * 0.5f;
        corners[1] = center + new Vector2(size.x * cos - size.y * sin, -size.x * sin - size.y * cos) * 0.5f;
        corners[2] = center + new Vector2(-size.x * cos + size.y * sin, size.x * sin + size.y * cos) * 0.5f;
        corners[3] = center + new Vector2(size.x * cos + size.y * sin, -size.x * sin + size.y * cos) * 0.5f;

        return (corners,center);
    }
    public (Vector2[] corners, Vector2 center) GetCustomColliderArea(CapsuleCollider2D capsuleCollider)
    {
        Vector2 center = (Vector2)(capsuleCollider.transform.position +
                         capsuleCollider.transform.TransformVector(capsuleCollider.offset));
        Vector2 scale = new Vector2(Mathf.Abs(capsuleCollider.transform.lossyScale.x), Mathf.Abs(capsuleCollider.transform.lossyScale.y))* custumScaler* Scaler;
        Vector2 size = new Vector2(capsuleCollider.size.x * scale.x, capsuleCollider.size.y * scale.y);

        float angle = -capsuleCollider.transform.eulerAngles.z; // Z-axis rotation in degrees
        float rad = angle * Mathf.Deg2Rad; // Convert degrees to radians

        // Calculate the rotation matrix
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        Vector2[] corners = new Vector2[4];
        corners[0] = center + new Vector2(-size.x * cos - size.y * sin, size.x * sin - size.y * cos) * 0.5f;
        corners[1] = center + new Vector2(size.x * cos - size.y * sin, -size.x * sin - size.y * cos) * 0.5f;
        corners[2] = center + new Vector2(-size.x * cos + size.y * sin, size.x * sin + size.y * cos) * 0.5f;
        corners[3] = center + new Vector2(size.x * cos + size.y * sin, -size.x * sin + size.y * cos) * 0.5f;

        return (corners, center);
    }
    public void AreaUpdate()
    {
        if (currentCustomLayerTypeSet.Count == 0)
        {
            return;
        }
        switch (customColliderType)
        {
            case ECustomColliderType.Circle:
                var circleArea = GetCustomColliderArea(circleCollider2D);
                this.Radius = circleArea.Radius;
                this.Center = circleArea.Center;
                break;
            case ECustomColliderType.Box:
                var boxArea = GetCustomColliderArea(boxCollider2D);
                for (int i = 0; i < boxArea.corners.Length; i++)
                {
                    Corners[i] = boxArea.corners[i];
                }
                Center = boxArea.center;
                break;
            case ECustomColliderType.Capsule:
                var capsuleArea = GetCustomColliderArea(capsuleCollider2D);
                for (int i = 0; i < capsuleArea.corners.Length; i++)
                {
                    Corners[i] = capsuleArea.corners[i];
                }
                Center = capsuleArea.center;
                break;
        }
    }
    protected virtual void Awake()
    {
        Scaler = 1f;
        Corners = new Vector2[4];
        circleCollider2D = null;
        capsuleCollider2D = null;
        boxCollider2D = null;
        if (TryGetComponent(out circleCollider2D))
        {
            customColliderType = ECustomColliderType.Circle;
            circleCollider2D.enabled = false;
            circleCollider2D.isTrigger = true;
        }
        else if (TryGetComponent(out capsuleCollider2D))
        {
            customColliderType = ECustomColliderType.Capsule;
            capsuleCollider2D.enabled = false;
            capsuleCollider2D.isTrigger = true;
        }
        else if (TryGetComponent(out boxCollider2D))
        {
            customColliderType = ECustomColliderType.Box;
            boxCollider2D.enabled = false;
            boxCollider2D.isTrigger = true;
        }
    }

    protected virtual void InitcustomLayerTypeList(List<int> layerList)
    {
        foreach (var eLayer in layerList)
        {
            customLayerTypeInitSet.Add((ECustomLayerType)eLayer);
        }
    }
    public void PresetInit(CustomColliderMonobehaviour characterMonobehaviour)
    {
        if (!isPresetAwake)
        {
            isPresetAwake = true;
            InitcustomLayerTypeList(layerPreset.Select(x => (int)x).ToList());
        }
        this.characterMonobehaviour = characterMonobehaviour;
        Refresh();
    }
    protected virtual void Refresh()
    {
        RefreshJustCustomLayerSet();
    }
    public void RefreshJustCustomLayerSet()
    {
        currentCustomLayerTypeSet.Clear();
        foreach (var item in customLayerTypeInitSet)
        {
            currentCustomLayerTypeSet.Add(item);
        }
    }
    public void Init(List<int> layerList)
    {
        if (!isAwake)
        {
            isAwake = true;
            InitcustomLayerTypeList(layerList);
        }
        Refresh();
    }
    public virtual void Init(CharacterTable CharacterTable)
    {
        if (this is DetectGroup)
        {
            Init(CharacterTable.detectorType);
        }
        else
        {
            Init(CharacterTable.collisionType);
        }
    }
    public bool Intersects(CustomCollider2D other)
    {
        switch (customColliderType)
        {
            case ECustomColliderType.Circle:
                switch (other.customColliderType)
                {
                    case ECustomColliderType.Circle:
                        float radiusSum = Radius + other.Radius;
                        return (Center - other.Center).sqrMagnitude <= radiusSum * radiusSum;
                    case ECustomColliderType.Box:
                    case ECustomColliderType.Capsule:
                        return SATCollision.CheckCircleRectangleCollision(this, other);
                    default:
                        return false;
                }
            case ECustomColliderType.Box:
            case ECustomColliderType.Capsule:
                switch (other.customColliderType)
                {
                    case ECustomColliderType.Circle:
                        return SATCollision.CheckCircleRectangleCollision(other, this);
                    case ECustomColliderType.Box:
                    case ECustomColliderType.Capsule:
                        return SATCollision.CheckRectangleRectangleCollision(this, other);
                    default:
                        return false;
                }
        }
        return false;
    }
}
public static class SATCollision
{
    public static bool CheckRectangleRectangleCollision(CustomCollider2D rect1, CustomCollider2D rect2)
    {
        Vector2[] axes = GetAxes(rect1.Corners.Concat(rect2.Corners).ToArray());
        foreach (Vector2 axis in axes)
        {
            if (!OverlapOnAxis(axis, rect1.Corners, rect2.Corners))
                return false;
        }
        return true;
    }

    public static bool CheckCircleRectangleCollision(CustomCollider2D circle, CustomCollider2D rect)
    {
        // 원의 축 (원의 중심에서 사각형의 가장 가까운 꼭지점까지의 벡터)
        Vector2 axisFromCircleCenter = (ClosestPointOnRect(circle.Center, rect.Corners) - circle.Center).normalized;
        Vector2[] axes = GetAxes(rect.Corners);
        Array.Resize(ref axes, axes.Length + 1);
        axes[axes.Length - 1] = axisFromCircleCenter;

        foreach (Vector2 axis in axes)
        {
            if (!OverlapOnAxisCircle(axis, rect.Corners, circle.Center, circle.Radius))
                return false;
        }
        return true;
    }

    private static Vector2[] GetAxes(Vector2[] corners)
    {
        // 사각형의 모든 변에 대한 법선 축을 계산
        List<Vector2> axes = new List<Vector2>();
        for (int i = 0; i < corners.Length; i++)
        {
            Vector2 edge = corners[i] - corners[(i + 1) % corners.Length];
            Vector2 normal = new Vector2(-edge.y, edge.x).normalized;
            axes.Add(normal);
        }
        return axes.ToArray();
    }

    private static bool OverlapOnAxis(Vector2 axis, Vector2[] corners1, Vector2[] corners2)
    {
        // 사각형을 축에 투영하고 겹치는지 확인
        (float minA, float maxA) = ProjectPolygon(axis, corners1);
        (float minB, float maxB) = ProjectPolygon(axis, corners2);
        return minA <= maxB && minB <= maxA;
    }

    private static bool OverlapOnAxisCircle(Vector2 axis, Vector2[] corners, Vector2 circleCenter, float radius)
    {
        // 사각형과 원을 축에 투영하고 겹치는지 확인
        (float minRect, float maxRect) = ProjectPolygon(axis, corners);
        float circleProjection = Vector2.Dot(circleCenter, axis);
        float minCircle = circleProjection - radius;
        float maxCircle = circleProjection + radius;
        return minRect <= maxCircle && minCircle <= maxRect;
    }

    private static (float min, float max) ProjectPolygon(Vector2 axis, Vector2[] corners)
    {
        // 다각형을 축에 투영
        float min = Vector2.Dot(axis, corners[0]);
        float max = min;
        for (int i = 1; i < corners.Length; i++)
        {
            float projection = Vector2.Dot(axis, corners[i]);
            if (projection < min) min = projection;
            else if (projection > max) max = projection;
        }
        return (min, max);
    }

    private static Vector2 ClosestPointOnRect(Vector2 point, Vector2[] corners)
    {
        // 사각형에 대한 가장 가까운 점을 계산
        Vector2 closest = corners[0];
        float closestDistanceSq = (point - closest).sqrMagnitude;

        for (int i = 1; i < corners.Length; i++)
        {
            float distanceSq = (point - corners[i]).sqrMagnitude;
            if (distanceSq < closestDistanceSq)
            {
                closest = corners[i];
                closestDistanceSq = distanceSq;
            }
        }
        return closest;
    }
}