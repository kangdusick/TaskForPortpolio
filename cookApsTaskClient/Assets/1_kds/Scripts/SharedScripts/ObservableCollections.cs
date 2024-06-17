using Spine;
using SRF;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ObservableHashSet<T> : IDisposable, IEnumerable<T>
{
    private HashSet<T> set;

    public ManagedAction OnChanged = new();
    public ManagedAction OnAdded = new();
    public ManagedAction<T> OnAddedItem = new();
    public ManagedAction<T> OnRemovedItem = new();
    public ObservableHashSet()
    {
        set = new HashSet<T>();
    }

    protected virtual void Changed()
    {
        OnChanged.Invoke();
    }

    public int Count => set.Count;
    public T Random()
    {
        return set.RandomElement();
    }
    public void Clear()
    {
        if (set.Count > 0) // Only invoke Changed if there are items to clear
        {
            var setList = set.ToList();
            for(int i = 0; i< setList.Count; i++)
            {
                Remove(setList[i]);
            }
            Changed();  // Call Changed when the set is cleared
        }
    }

    public bool Add(T item)
    {
        bool added = set.Add(item);
        if (added)
        {
            OnAdded.Invoke();
            OnAddedItem.Invoke(item);
            Changed();  // Call Changed when an item is added
        }
        return added;
    }

    public bool Contains(T item)
    {
        return set.Contains(item);
    }

    public bool Remove(T item)
    {
        bool removed = set.Remove(item);
        if (removed)
        {
            OnRemovedItem.Invoke(item);
            Changed();  // Call Changed when an item is removed
        }
        return removed;
    }

    public List<T> ToList()
    {
        return set.ToList();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return set.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Dispose()
    {
        Clear();
        set = null;
    }
}
public class ObservableCharacterMonobehaviourList<T> : ObservableList<T> where T: CustomColliderMonobehaviour
{
    public ManagedAction<T> OnDie = new();
    public override bool Remove(T item)
    {
        if(ReferenceEquals(item, null))
        {
            return false;
        }
        bool isRemoved = base.Remove(item);
        if (isRemoved && item.IsDie && !item.isIgnoreDieEvent)
        {
            OnDie.Invoke(item);
        }
        return isRemoved;
    }
}
public class ObservableList<T> : IDisposable, IEnumerable<T>
{
    private List<T> list;

    public ManagedAction OnChanged = new();
    public ManagedAction<T> OnRemovedItem = new();
    public ManagedAction<T> OnAddedItem = new();  // Event to notify when an item is added
    public ManagedAction<T, T, int> OnItemUpdated = new();

    public ObservableList()
    {
        list = new List<T>();
    }

    public int Count => list.Count;

    public T this[int index]
    {
        get => list[index];
        set
        {
            var oldItem = list[index];
            if (!EqualityComparer<T>.Default.Equals(oldItem, value))
            {
                list[index] = value;
                OnItemUpdated.Invoke(oldItem, value, index);
                Changed();  // Call Changed when an item is updated
            }
        }
    }

    protected virtual void Changed()
    {
        OnChanged.Invoke();
    }

    public void Clear()
    {
        if (list.Count > 0)
        {
            for(int i = 0; i<list.Count; i++)
            {
                Remove(list[i]);
            }
            Changed();  // Call Changed when the list is cleared
        }
    }

    public void Add(T item)
    {
        list.Add(item);
        OnAddedItem.Invoke(item);  // Notify subscribers that an item was added
        Changed();  // Call Changed when an item is added
    }

    public bool Contains(T item)
    {
        return list.Contains(item);
    }

    public virtual bool Remove(T item)
    {
        bool removed = list.Remove(item);
        if (removed)
        {
            OnRemovedItem.Invoke(item);
            Changed();  // Call Changed when an item is removed
        }
        return removed;
    }

    public void Dispose()
    {
        Clear();
        list = null;
    }

    public IEnumerator<T> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int IndexOf(T item)
    {
        return list.IndexOf(item);
    }
    public T Find(Predicate<T> match)
    {
        if (match == null)
        {
            throw new ArgumentNullException(nameof(match));
        }

        foreach (T item in list)
        {
            if (match(item))
            {
                return item;
            }
        }

        return default(T);  // Return the default value for the type T if no match is found
    }
    public List<T> FindAll(Predicate<T> match)
    {
        return list.FindAll(match);
    }
    public T Random()
    {
        return list.Random();
    }
}
public class ObservableDictionary<TKey, TValue> : IDisposable, IEnumerable<KeyValuePair<TKey, TValue>>
{
    protected Dictionary<TKey, TValue> dictionary;

    public ManagedAction OnChanged = new();
    public ManagedAction<TKey> OnChangedItem = new();

    public ObservableDictionary()
    {
        dictionary = new Dictionary<TKey, TValue>();
    }

    public virtual TValue this[TKey key]
    {
        get => dictionary[key];
        set
        {
            bool existed = dictionary.ContainsKey(key);
            TValue oldValue = existed ? dictionary[key] : default;
            dictionary[key] = value;
            if (!Equals(oldValue, value))
            {
                ChangedValue(key); // Notify about the specific key's value change
            }
            Changed(); // Notify about general dictionary change
        }
    }

    protected virtual void ChangedValue(TKey key)
    {
        OnChangedItem.Invoke(key);
    }

    protected virtual void Changed()
    {
        OnChanged.Invoke();
    }

    public void Add(TKey key, TValue value)
    {
        dictionary.Add(key, value);
        ChangedValue(key); // Notify about the new key's value
        Changed();
    }

    public bool Remove(TKey key)
    {
        if (dictionary.TryGetValue(key, out TValue value) && dictionary.Remove(key))
        {
            ChangedValue(key); // Notify about the removal of the key's value
            Changed();
            return true;
        }
        return false;
    }

    public void Clear()
    {
        if (dictionary.Count > 0)
        {
            dictionary.Clear();
            Changed(); // Notify about clearing the dictionary
            // Note: In this case, calling ChangedValue for each key is not efficient or necessary
        }
    }

    public bool ContainsKey(TKey key)
    {
        return dictionary.ContainsKey(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        return dictionary.TryGetValue(key, out value);
    }

    public void Dispose()
    {
        Clear();
        dictionary = null;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}