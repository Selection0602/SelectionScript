using System.Collections.Generic;
using UnityEngine;

public class DebuffUI : MonoBehaviour
{
    [SerializeField] private DebuffIcon _debuffIconPrefab;

    private ObjectPool<DebuffIcon> _debuffIcons;
    private readonly int _poolSize = 10;

    private List<DebuffIcon> _currentDebuffIcons = new();

    private void Awake()
    {
        _debuffIcons = new ObjectPool<DebuffIcon>(_debuffIconPrefab.gameObject);
        _debuffIcons.SetParent(transform);
        _debuffIcons.InitializePool(_poolSize);
    }

    public void AddDebuffIcon(Debuff turnEffect)
    {
        var icon = _debuffIcons.Get();
        icon.transform.localPosition = Vector3.zero;
        icon.transform.localScale = Vector3.one;
        icon.Data = turnEffect;
        _currentDebuffIcons.Add(icon);
    }

    public void RemoveDebuffIcon(Debuff debuff)
    {
        for (int i = _currentDebuffIcons.Count - 1; i >= 0; i--)
        {
            var icon = _currentDebuffIcons[i];
            if (icon.Data == debuff)
            {
                _debuffIcons.Return(icon);
                _currentDebuffIcons.RemoveAt(i);
                break;
            }
        }
    }

    public void RemoveAllDebuffIcon()
    {
        foreach (var icon in _currentDebuffIcons)
        {
            _debuffIcons.Return(icon);
        }
        _currentDebuffIcons.Clear();
    }
}
