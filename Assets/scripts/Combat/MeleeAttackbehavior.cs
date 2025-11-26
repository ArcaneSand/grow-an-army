using UnityEngine;

/// <summary>
/// Melee attack behavior - instant damage to single target
/// Used by: Villager, Knight
/// </summary>
public class MeleeAttackBehavior : AttackBehavior
{
    [Header("Melee Settings")]
    [SerializeField] private float knockbackForce = 2f;
    [SerializeField] private bool applyKnockback = false;
    [SerializeField] private AudioClip meleeAttackSound;
    
    public override void Execute(UnitBase target, UnitBase attacker)
    {
        if (!CanExecute(target, attacker)) return;
        
        // Deal instant damage
        target.TakeDamage(attackDamage, attacker);
        
        // Optional knockback
        if (applyKnockback)
        {
            ApplyKnockback(target, attacker);
        }
        
        // Trigger animation via handler
        if (attacker.unitHandler != null)
        {
            attacker.unitHandler.Attack();
        }

        SoundManager.Instance.PlaySoundFX(meleeAttackSound, attacker.transform);
        
        Debug.Log($"{attacker.name} melee attacks {target.name} for {attackDamage} damage!");
    }
    
    private void ApplyKnockback(UnitBase target, UnitBase attacker)
    {
        Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
        if (targetRb == null) return;
        
        Vector2 knockbackDir = (target.transform.position - attacker.transform.position).normalized;
        targetRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
    }
}