using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AOE attack behavior - damages all enemies in radius
/// Used by: Giant
/// </summary>
public class AOEAttackBehavior : AttackBehavior
{
    [Header("AOE Settings")]
    [SerializeField] private float aoeRadius = 2f;
    [SerializeField] private LayerMask enemyLayer; // Which layer to hit
    [SerializeField] private bool showAOEIndicator = true;
    [SerializeField] private float indicatorDuration = 0.3f;

    [SerializeField] private AudioClip aoeAttackSound;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject aoeEffectPrefab; // Optional visual effect
    
    public override void Execute(UnitBase target, UnitBase attacker)
    {
        // For AOE, we don't need specific target, just check one is in range
        if (target == null || attacker == null) return;
        
        // Find all enemies in AOE radius
        List<UnitBase> targets = FindTargetsInRadius(attacker);
        
        if (targets.Count == 0) return;
        
        // Deal damage to all found targets
        foreach (UnitBase enemy in targets)
        {
            if (enemy != null && !enemy.IsDead())
            {
                enemy.TakeDamage(attackDamage, attacker);
            }
        }
        
        // Trigger animation
        if (attacker.unitHandler != null)
        {
            attacker.unitHandler.Attack();
        }

        SoundManager.Instance.PlaySoundFX(aoeAttackSound, attacker.transform);
        
        // Spawn visual effect
        if (aoeEffectPrefab != null)
        {
            GameObject effect = Instantiate(aoeEffectPrefab, attacker.transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        
        Debug.Log($"{attacker.name} AOE attack hits {targets.Count} enemies!");
    }
    
    /// <summary>
    /// Find all enemies in AOE radius
    /// </summary>
    private List<UnitBase> FindTargetsInRadius(UnitBase attacker)
    {
        List<UnitBase> targets = new List<UnitBase>();
        
        // Find all colliders in radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attacker.transform.position, aoeRadius);
        
        foreach (Collider2D col in colliders)
        {
            UnitBase unit = col.GetComponent<UnitBase>();
            
            // Check if enemy and not dead
            if (unit != null && unit.team != attacker.team && !unit.IsDead())
            {
                targets.Add(unit);
            }
        }
        
        return targets;
    }
    
    /// <summary>
    /// Initialize with AOE radius from data
    /// </summary>
    public override void Initialize(UnitDataSO data)
    {
        base.Initialize(data);
        
        // Set AOE radius from data if available
        // aoeRadius can be stored in UnitDataSO
    }
    
    public void SetAOERadius(float radius)
    {
        aoeRadius = radius;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw AOE radius
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}