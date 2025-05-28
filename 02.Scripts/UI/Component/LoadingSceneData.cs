using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LoadingSceneData
{
    public AssetLabelMapping[] mappings;
    public float tipChangeInterval;
    public string nextSceneName;
    public object payload;
}