using UnityEngine;

public class TimeLineSoundManager : MonoBehaviour
{
    //BGM 중지
    public void PauseMiniGameBGM()
    {
        Manager.Instance.SoundManager.PauseBGM();
    }

    //BGM 재생
    public void ResumeMiniGameBGM()
    {
        Manager.Instance.SoundManager.ResumeBGM();
    }
}
