using UnityEngine;

/// <summary>
/// Mage unit handler - handles mage-specific animations
/// Ranged unit that shoots piercing projectiles
/// </summary>
public class Mage : UnitHandler
{
    public Animator mageAnimator;
    public Animator staffAnimator; // Staff/wand animations
    
    [Header("Mage Specific")]
    public Transform spellSpawnPoint; // Where spells spawn from
    public ParticleSystem castingEffect; // Visual effect while casting
    
    public override void Walk(bool isWalking)
    {
    //     if (mageAnimator != null)
    //     {
    //         mageAnimator.SetBool("isWalking", isWalking);
    //     }
    }
    
    public override void Attack()
    {
        // Trigger casting animation
        // if (mageAnimator != null)
        // {
        //     mageAnimator.SetTrigger("Cast");
        // }
        
        // if (staffAnimator != null)
        // {
        //     staffAnimator.SetTrigger("Channel");
        // }
        
        // Play casting effect
        if (castingEffect != null)
        {
            castingEffect.Play();
        }
        
        // Optional: Play magic sound
        // AudioManager.PlaySound("MagicCast");
    }
    
    /// <summary>
    /// Get spell spawn position for projectile spawning
    /// </summary>
    public Vector3 GetSpellSpawnPosition()
    {
        return spellSpawnPoint != null ? spellSpawnPoint.position : transform.position;
    }
}