using System;
using UnityEngine;

public abstract class RestOptionBase : MonoBehaviour
{
    public abstract string TitleText { get; }
    public abstract void ApplyOption(Action onComplete);
}
