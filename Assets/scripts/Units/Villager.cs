using UnityEngine;

public class Villager : UnitHandler
{
    public Animator villagerAnimator;
    public Animator weaponAnimator;

    public override void Walk(bool isWalking)
    {
        if (villagerAnimator != null)
        {
            villagerAnimator.SetBool("isWalking", isWalking);
        }
    }

    public override void Attack()
    {
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("Attack");
        }
    }
}
