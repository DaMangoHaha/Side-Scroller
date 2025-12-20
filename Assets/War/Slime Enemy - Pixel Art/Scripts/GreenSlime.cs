using UnityEngine;

public class GreenSlime : SlimeBase
{
    // Green Slime has no extra behavior
    // It only plays Idle and scrolls left

    protected override void Awake()
    {
        base.Awake();
        if (anim != null)
            anim.SetTrigger("Idle"); // start in idle animation
    }

    protected override void DoBehavior()
    {
        // No special actions. Stay idle.
        // SlimeBase handles the left movement.
    }

    protected override void OnHitPlayer()
    {
        Die();
        SoundManager.Instance.PlaySound2D("Damage");
    }
}

