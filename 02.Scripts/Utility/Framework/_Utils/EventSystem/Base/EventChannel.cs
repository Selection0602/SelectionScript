using System.Collections.Generic;
using UnityEngine;

public abstract class EventChannel<T> : ScriptableObject
{
    private readonly HashSet<EventListener<T>> observers = new();

    public void Invoke(T value)
    {
        foreach (var observer in observers)
        {
            observer.Raise(value);
        }
    }

    public void Register(EventListener<T> observer) => observers.Add(observer);
    public void Unregister(EventListener<T> observer) => observers.Remove(observer);
}

public abstract class EventChannel<T1, T2> : ScriptableObject
{
    private readonly HashSet<EventListener<T1, T2>> observers = new();

    public void Invoke(T1 value1, T2 value2)
    {
        foreach (var observer in observers)
        {
            observer.Raise(value1, value2);
        }
    }

    public void Register(EventListener<T1, T2> observer) => observers.Add(observer);
    public void Unregister(EventListener<T1, T2> observer) => observers.Remove(observer);
}


public readonly struct Empty
{
}

[CreateAssetMenu(menuName = "Event/EventChannel")]
public class EventChannel : EventChannel<Empty>
{
}