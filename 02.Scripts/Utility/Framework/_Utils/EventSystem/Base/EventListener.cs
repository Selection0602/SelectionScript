using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class EventListener<T> : MonoBehaviour
{
    [SerializeField] private EventChannel<T> eventChannel;
    [SerializeField] private UnityEvent<T> unityEvent;

    protected void Awake()
    {
        eventChannel.Register(this);
    }

    protected void OnDestroy()
    {
        eventChannel.Unregister(this);
    }

    public void Raise(T value)
    {
        unityEvent?.Invoke(value);
    }
}

public abstract class EventListener<T1, T2> : MonoBehaviour
{
    [SerializeField] private EventChannel<T1, T2> eventChannel;
    [SerializeField] private UnityEvent<T1, T2> unityEvent;

    protected void Awake()
    {
        eventChannel.Register(this);
    }

    protected void OnDestroy()
    {
        eventChannel.Unregister(this);
    }

    public void Raise(T1 value1, T2 value2)
    {
        unityEvent?.Invoke(value1, value2);
    }
}

public class EventListener : EventListener<Empty>
{
}