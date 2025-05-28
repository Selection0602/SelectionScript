using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEngine;

public class Effect : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 애니메이션 재생
    /// </summary>
    /// <param name="hash">재생 할 애니메이션의 해쉬 값</param>
    public void PlayEffect(int hash)
    {
        _animator.Play(hash);
    }

    /// <summary>
    /// 애니메이션 완료 대기
    /// </summary>
    public async Task WaitForAnimationComplete()
    {
        // 현재 애니메이션의 길이를 가져와서 해당 시간만큼 대기
        float length = _animator.GetCurrentAnimatorStateInfo(0).length;
        await UniTask.Delay(Mathf.RoundToInt(length * 300));
    }
}
