using System;
using System.Collections.Generic;
using UnityEngine;
class ActionCleanUp : MonoBehaviour
{
    private List<Action> removeListenerActions = new List<Action>();

    public void AddRemoveAction(Action removeListener)
    {
        removeListenerActions.Add(removeListener);
    }

    void OnDestroy()
    {
        foreach (var action in removeListenerActions)
        {
            action?.Invoke();
        }
    }
}
public struct ListenerInfo
{
    public WeakReference<MonoBehaviour> MonoBehaviourReference;
    public Action Action;

    public ListenerInfo(MonoBehaviour monoBehaviour, Action action)
    {
        MonoBehaviourReference = new WeakReference<MonoBehaviour>(monoBehaviour);
        Action = action;
    }
}
public class ManagedAction
{
    public int listenersCnt => listeners.Count;
    private List<ListenerInfo> listeners = new List<ListenerInfo>();
    private Action action;
    public void AddListener(Action listener)
    {
        action += listener;
    }
    public void AddListener(MonoBehaviour owner, Action listener)
    {
        listeners.Add(new ListenerInfo(owner, listener));

        ActionCleanUp cleanUpComponent = owner.gameObject.GetComponent<ActionCleanUp>();
        if (cleanUpComponent == null)
        {
            cleanUpComponent = owner.gameObject.AddComponent<ActionCleanUp>();
        }

        cleanUpComponent.AddRemoveAction(() => RemoveListener(listener));
    }

    public void RemoveListener(Action listener)
    {
        action -= listener;

        listeners.RemoveAll(info =>
        {
            if (info.Action == listener)
            {
                return true;
            }
            else
            {
                if (info.MonoBehaviourReference.TryGetTarget(out MonoBehaviour mb))
                {
                    return mb.gameObject == null;
                }
                return false;
            }
        });
    }

    public void Invoke()
    {
        action?.Invoke();

        List<Action> currentListeners = new List<Action>();
        foreach (var info in listeners)
        {
            if (info.MonoBehaviourReference.TryGetTarget(out MonoBehaviour mb) && mb.gameObject.activeInHierarchy)
            {
                currentListeners.Add(info.Action);
            }
        }

        foreach (var listener in currentListeners)
        {
            listener.Invoke();
        }
    }
}
public struct ListenerInfo<T>
{
    public WeakReference<MonoBehaviour> MonoBehaviourReference;
    public Action<T> Action;

    public ListenerInfo(MonoBehaviour monoBehaviour, Action<T> action)
    {
        MonoBehaviourReference = new WeakReference<MonoBehaviour>(monoBehaviour);
        Action = action;
    }
}
public struct ListenerInfo<T1, T2>
{
    public WeakReference<MonoBehaviour> MonoBehaviourReference;
    public Action<T1, T2> Action;

    public ListenerInfo(MonoBehaviour monoBehaviour, Action<T1, T2> action)
    {
        MonoBehaviourReference = new WeakReference<MonoBehaviour>(monoBehaviour);
        Action = action;
    }
}
public struct ListenerInfo<T1, T2, T3>
{
    public WeakReference<MonoBehaviour> MonoBehaviourReference;
    public Action<T1, T2, T3> Action;

    public ListenerInfo(MonoBehaviour monoBehaviour, Action<T1, T2, T3> action)
    {
        MonoBehaviourReference = new WeakReference<MonoBehaviour>(monoBehaviour);
        Action = action;
    }
}

public class ManagedAction<T>
{
    private List<ListenerInfo<T>> listeners = new List<ListenerInfo<T>>();
    private Action<T> action;
    public void AddListener(Action<T> listener)
    {
        action += listener;
    }
    public void AddListener(MonoBehaviour owner, Action<T> listener)
    {
        listeners.Add(new ListenerInfo<T>(owner, listener));
        ActionCleanUp cleanUpComponent = owner.gameObject.GetComponent<ActionCleanUp>();
        if (cleanUpComponent == null)
        {
            cleanUpComponent = owner.gameObject.AddComponent<ActionCleanUp>();
        }

        cleanUpComponent.AddRemoveAction(() => RemoveListener(listener));
    }

    public void RemoveListener(Action<T> listener)
    {
        action -= listener;

        listeners.RemoveAll(info =>
        {
            if (info.Action == listener)
            {
                return true;
            }
            else
            {
                if (info.MonoBehaviourReference.TryGetTarget(out MonoBehaviour mb))
                {
                    return mb == null || mb.gameObject == null;
                }
                return false;
            }
        });
    }

    public void Invoke(T arg)
    {
        action?.Invoke(arg);
        List<Action<T>> currentListeners = new List<Action<T>>();
        foreach (var info in listeners)
        {
            if (info.MonoBehaviourReference.TryGetTarget(out MonoBehaviour mb) && mb.gameObject.activeInHierarchy)
            {
                currentListeners.Add(info.Action);
            }
        }

        foreach (var listener in currentListeners)
        {
            listener.Invoke(arg);
        }
    }
}

public class ManagedAction<T1, T2>
{
    private List<ListenerInfo<T1, T2>> listeners = new List<ListenerInfo<T1, T2>>();
    private Action<T1, T2> action;
    public void AddListener(Action<T1, T2> listener)
    {
        action += listener;
    }
    public void AddListener(MonoBehaviour owner, Action<T1, T2> listener)
    {
        listeners.Add(new ListenerInfo<T1, T2>(owner, listener));

        ActionCleanUp cleanUpComponent = owner.gameObject.GetComponent<ActionCleanUp>();
        if (cleanUpComponent == null)
        {
            cleanUpComponent = owner.gameObject.AddComponent<ActionCleanUp>();
        }

        cleanUpComponent.AddRemoveAction(() => RemoveListener(listener));
    }

    public void RemoveListener(Action<T1, T2> listener)
    {
        action -= listener;
        listeners.RemoveAll(info =>
        {
            if (info.Action == listener)
            {
                return true;
            }
            else
            {
                if (info.MonoBehaviourReference.TryGetTarget(out MonoBehaviour mb))
                {
                    return mb == null || mb.gameObject == null;
                }
                return false;
            }
        });
    }

    public void Invoke(T1 arg1, T2 arg2)
    {
        action?.Invoke(arg1, arg2);
        // 현재 활성화된 리스너들만 호출하기 위해 복사본을 만듭니다.
        var activeListeners = new List<Action<T1, T2>>();
        foreach (var info in listeners)
        {
            // MonoBehaviour의 WeakReference를 확인하고, GameObject가 활성화되어 있으면 리스트에 추가합니다.
            if (info.MonoBehaviourReference.TryGetTarget(out MonoBehaviour mb) && mb.gameObject.activeInHierarchy)
            {
                activeListeners.Add(info.Action);
            }
        }

        // 복사된 리스트를 사용하여 리스너들을 호출합니다.
        // 이는 원본 리스트를 순회하는 동안 수정이 발생하는 것을 방지합니다.
        foreach (var listener in activeListeners)
        {
            listener(arg1, arg2);
        }
    }
}
public class ManagedAction<T1, T2, T3>
{
    private List<ListenerInfo<T1, T2, T3>> listeners = new List<ListenerInfo<T1, T2, T3>>();
    private Action<T1, T2, T3> action;

    public void AddListener(Action<T1, T2, T3> listener)
    {
        action += listener;
    }

    public void AddListener(MonoBehaviour owner, Action<T1, T2, T3> listener)
    {
        listeners.Add(new ListenerInfo<T1, T2, T3>(owner, listener));

        ActionCleanUp cleanUpComponent = owner.gameObject.GetComponent<ActionCleanUp>();
        if (cleanUpComponent == null)
        {
            cleanUpComponent = owner.gameObject.AddComponent<ActionCleanUp>();
        }

        cleanUpComponent.AddRemoveAction(() => RemoveListener(listener));
    }

    public void RemoveListener(Action<T1, T2, T3> listener)
    {
        action -= listener;

        listeners.RemoveAll(info =>
        {
            if (info.Action == listener)
            {
                return true;
            }
            else
            {
                if (info.MonoBehaviourReference.TryGetTarget(out MonoBehaviour mb))
                {
                    return mb == null || mb.gameObject == null;
                }
                return false;
            }
        });
    }

    public void Invoke(T1 arg1, T2 arg2, T3 arg3)
    {
        action?.Invoke(arg1, arg2, arg3);

        List<Action<T1, T2, T3>> currentListeners = new List<Action<T1, T2, T3>>();
        foreach (var info in listeners)
        {
            if (info.MonoBehaviourReference.TryGetTarget(out MonoBehaviour mb) && mb.gameObject.activeInHierarchy)
            {
                currentListeners.Add(info.Action);
            }
        }

        foreach (var listener in currentListeners)
        {
            listener.Invoke(arg1, arg2, arg3);
        }
    }
}