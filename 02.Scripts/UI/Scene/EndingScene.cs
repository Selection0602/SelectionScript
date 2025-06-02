using UnityEngine;

public class EndingScene : SceneBase
{
    [SerializeField] private EndingUI endingUI;
    private IEndingView _view;
    protected override void OnStart(object data)
    {
        _view = endingUI;
        if (data is int endingIndex)
        {
            _view.Initialize(endingIndex);
        }
    }
}
