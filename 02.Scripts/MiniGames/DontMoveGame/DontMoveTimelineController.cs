using UnityEngine;
using UnityEngine.Playables;
using System;

public class DontMoveTimelineController : MonoBehaviour
{
    //현재 PD
    private PlayableDirector _currentDirector;
    //현재 재생중인가? => 현재 PD가 null이 아님, 재생중인 상태
    public bool IsPlaying => _currentDirector != null && _currentDirector.state == PlayState.Playing;
    public event Action OnTimelineStopped;

    //타임라인 재생
    public void PlayTimeline(PlayableDirector director)
    {
        if (_currentDirector != null)
        {
            _currentDirector.stopped -= TimelineStoppedHandler;
            Destroy(_currentDirector.gameObject);
        }

        _currentDirector = director;
        _currentDirector.stopped += TimelineStoppedHandler;
        _currentDirector.Play();
    }

    //타임라인 중지(감지시)
    public void PauseTimeline()
    {
        if (_currentDirector != null)
            _currentDirector.Pause();
    }

    //타임라인 멈추기
    public void StopTimeline()
    {
        if (_currentDirector != null)
        {
            _currentDirector.stopped -= TimelineStoppedHandler;

            //타임라인 강제 중지 시에도 핸들러 호출
            TimelineStoppedHandler(_currentDirector);

            Destroy(_currentDirector.gameObject);
            _currentDirector = null;
        }
    }


    private void TimelineStoppedHandler(PlayableDirector pd)
    {
        OnTimelineStopped?.Invoke();
        //끝난 경우 해당 오브젝트 파괴
        if (pd != null) Destroy(pd.gameObject);

    }
}
