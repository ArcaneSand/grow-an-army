using UnityEngine;

/// <summary>
/// Archer unit handler - handles archer-specific animations
/// Ranged unit that shoots arrows
/// </summary>
public class Archer : UnitHandler
{
    public Animator archerAnimator;
    public Animator bowAnimator; // Bow draw/release animations
    
    [Header("Archer Specific")]
    public Transform arrowSpawnPoint; // Where arrows spawn from
    
    public override void Walk(bool isWalking)
    {
        if (archerAnimator != null)
        {
            archerAnimator.SetBool("isWalking", isWalking);
        }
    }
    
    public override void Attack()
    {
        // Trigger shooting animation
        // if (archerAnimator != null)
        // {
        //     archerAnimator.SetTrigger("Attack");
        // }
        
        if (bowAnimator != null)
        {
            bowAnimator.SetTrigger("Attack");
        }
        
        // Optional: Play bow sound
        // AudioManager.PlaySound("BowShoot");
    }
    
    /// <summary>
    /// Get arrow spawn position for projectile spawning
    /// </summary>
    public Vector3 GetArrowSpawnPosition()
    {
        return arrowSpawnPoint != null ? arrowSpawnPoint.position : transform.position;
    }
}