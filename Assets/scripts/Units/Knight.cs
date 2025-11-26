using UnityEngine;

/// <summary>
/// Knight unit handler - handles knight-specific animations
/// Uses melee attack like Villager but with heavier animations
/// </summary>
public class Knight : UnitHandler
{
    public Animator knightAnimator;
    public Animator weaponAnimator; // Shield/sword animations
    
    public override void Walk(bool isWalking)
    {
        if (knightAnimator != null)
        {
            knightAnimator.SetBool("isWalking", isWalking);
        }
    }
    
    public override void Attack()
    {
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("Attack");
        }
        
        // Optional: Play heavy attack sound
        // AudioManager.PlaySound("KnightAttack");
    }
}