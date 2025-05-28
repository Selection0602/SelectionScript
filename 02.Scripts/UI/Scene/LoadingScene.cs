using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingScene : SceneBase
{
    [SerializeField] private LoadingController loadingController;

    protected override void OnStart(object data)
    {
        base.OnStart(data);

        if (data is LoadingSceneData loadData)
        {
            loadingController.Setup(loadData);
        }
        else
        {
            Debug.LogError("LoadingSceneData가 전달되지 않았습니다!");
        }
    }
}
