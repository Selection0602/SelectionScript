using System.Threading.Tasks;
using UnityEngine;

public class EnemyAnimController : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public async Task PlayAttackAnim()
    {
        _animator.SetTrigger("IsAttack");
        await WaitForAnimationEnd("Attack");
    }

    private async Task WaitForAnimationEnd(string animationName)
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        while (!stateInfo.IsName(animationName))
        {
            await Task.Yield();
            stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        }
        while (stateInfo.IsName(animationName) && stateInfo.normalizedTime < 1.0f)
        {
            await Task.Yield();
            stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        }
    }

    public async Task PlayDeathAnim()
    {
        BlockerManager.Instance.Active();
        _animator.SetTrigger("IsDead");
        await WaitForAnimationEnd("Death");
        BlockerManager.Instance.InActive();
    }

    public void StopAnimation()
    {
        _animator.enabled = false;
    }
}
