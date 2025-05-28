using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class JumpScareController : MonoBehaviour
{
    public GameObject Panel;            //뒷배경
    public GameObject MonsterImage;     //몬스터 이미지
    public Image DarkPanel;             //어두운 배경
    public AudioSource ScreamSound;       //비명소리

    [Header("실패시")]
    public UnityEvent FailGame;


    private void Start()
    {
        Panel.SetActive(false);
        MonsterImage.SetActive(false);
        DarkPanel.gameObject.SetActive(false);

        MonsterImage.transform.localScale = Vector3.one;
    }

    public void StartJumpScare(float FirstWaitTime, float ScaleTime, float SecondWaitTime)
    {
        Panel.SetActive(true);
        MonsterImage.SetActive(true);
        DarkPanel.gameObject.SetActive(true);
        StartCoroutine(ShowJumpScare(FirstWaitTime, ScaleTime, SecondWaitTime));
    }

    public IEnumerator ShowJumpScare(float FirstWaitTime, float ScaleTime, float SecondWaitTime)
    {
        yield return new WaitForSeconds(1f);
        DarkPanel.DOFade(0f, 2f);
        yield return new WaitForSeconds(FirstWaitTime);
        ScreamSound.Play();
        MonsterImage.transform.DOScale(new Vector3(6f, 6f, 6f), ScaleTime);
        yield return new WaitForSeconds(SecondWaitTime);
        DarkPanel.DOFade(1f, 1f);
        yield return new WaitForSeconds(1f);
        FailGame?.Invoke();
    }
}
