using UnityEngine;

/// <summary>
/// Giant unit handler - handles giant-specific animations
/// Large AOE melee unit that damages multiple enemies
/// </summary>
public class Giant : UnitHandler
{
    public Animator giantAnimator;
    public Animator weaponAnimator; // Giant club/hammer animations
    
    [Header("Giant Specific")]
    public ParticleSystem groundSlamEffect; // Visual effect for AOE attack
    
    public override void Walk(bool isWalking)
    {
        // if (giantAnimator != null)
        // {
        //     giantAnimator.SetBool("isWalking", isWalking);
        // }
    }
    
    public override void Attack()
    {
        // Trigger heavy AOE attack animation
        // if (giantAnimator != null)
        // {
        //     giantAnimator.SetTrigger("Slam");
        // }
        
        if (weaponAnimator != null)
        {
            weaponAnimator.SetTrigger("Attack");
        }   
        
        // Play ground slam effect
        if (groundSlamEffect != null)
        {
            groundSlamEffect.Play();
        }
        
        // Optional: Camera shake for impact
        // CameraShake.Shake(0.3f, 0.2f);
        
        // Optional: Play slam sound
        // AudioManager.PlaySound("GiantSlam");
    }
}