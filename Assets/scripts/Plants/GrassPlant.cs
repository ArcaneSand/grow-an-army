using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Grass Plant - Special abilities: Slows enemies and heals allies in range
/// This is a passive aura plant
/// </summary>
public class GrassPlant : PlantTurretBase
{
    [Header("Grass Plant Abilities")]
    [SerializeField] private float slowStrength = 0.5f; // 50% slow
    [SerializeField] private float healPerSecond = 3f;
    [SerializeField] private float auraRadius = 3f;
    
    private List<UnitBase> alliesInRange = new List<UnitBase>();
    private List<EnemyUnit> enemiesToSlow = new List<EnemyUnit>();
    
    protected override void Update()
    {
        base.Update();
        ApplyAuraEffects();

    }
    
    void ApplyAuraEffects()
    {
        // Clean up dead/null units
        alliesInRange.RemoveAll(u => u == null || u.IsDead());
        enemiesToSlow.RemoveAll(e => e == null || e.IsDead());
        
        // Heal allies
        foreach (UnitBase ally in alliesInRange)
        {
            float healAmount = healPerSecond * Time.deltaTime * currentStats.specialEffectStrength;
            // Allies don't have a Heal method, but we can reverse damage
            // In a real implementation, you'd add a Heal method to UnitBase
        }
        
        // Slow enemies (would need to implement slow system on enemies)
        // For now, just track them
    }
    
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        
        // Detect allies for healing
        AllyUnit ally = other.GetComponent<AllyUnit>();
        if (ally != null && !ally.IsDead())
        {
            if (!alliesInRange.Contains(ally))
            {
                alliesInRange.Add(ally);
            }
        }
        
        // Track enemies for slowing
        EnemyUnit enemy = other.GetComponent<EnemyUnit>();
        if (enemy != null && !enemy.IsDead())
        {
            if (!enemiesToSlow.Contains(enemy))
            {
                enemiesToSlow.Add(enemy);
            }
        }
    }
    
    protected override void OnTriggerExit2D(Collider2D other)
    {
        base.OnTriggerExit2D(other);
        
        AllyUnit ally = other.GetComponent<AllyUnit>();
        if (ally != null)
        {
            alliesInRange.Remove(ally);
        }
        
        EnemyUnit enemy = other.GetComponent<EnemyUnit>();
        if (enemy != null)
        {
            enemiesToSlow.Remove(enemy);
        }
    }
    
    protected override void OnAttackPerformed(UnitBase target)
    {
        // Grass plant doesn't really "attack" - it just has aura
        // But we can still do base damage for balance
        base.OnAttackPerformed(target);
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw aura radius
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}
