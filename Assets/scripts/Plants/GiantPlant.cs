using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Giant Plant (Cactus) - AOE melee attacks around self
/// Damages all enemies within range simultaneously
/// Higher stages have larger AOE and more damage
/// </summary>
public class GiantPlant : PlantTurretBase
{
    [Header("Giant Plant - AOE Melee")]
    [SerializeField] private float aoeRadius = 3f;
    [SerializeField] private bool showAOEIndicator = true;
    [SerializeField] private GameObject aoeEffectPrefab; // Optional visual effect
    
    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        
        // Set detection radius to AOE radius
        if (detectionCollider != null)
        {
            detectionCollider.radius = aoeRadius;
        }
    }
    
    protected override void SetStage(int stage)
    {
        base.SetStage(stage);
        
        // Update AOE radius based on stage
        switch (currentStage)
        {
            case 0: // Sprout
                aoeRadius = 2f;
                break;
            case 1: // Bud
                aoeRadius = 2.5f;
                break;
            case 2: // Rooted
                aoeRadius = 3f;
                break;
        }
        
        // Update detection collider
        if (detectionCollider != null)
        {
            detectionCollider.radius = aoeRadius;
        }
    }
    
    protected override void PerformAttack(UnitBase target)
    {
        // For AOE, we attack ALL enemies in range, not just one
        List<UnitBase> targets = FindAllEnemiesInRange();
        
        if (targets.Count == 0) return;
        
        // Deal damage to all found targets
        foreach (UnitBase enemy in targets)
        {
            if (enemy != null && !enemy.IsDead())
            {
                enemy.TakeDamage(currentStats.attackDamage, null);
            }
        }
        
        // Reset attack timer
        float cooldown = 1f / currentStats.attackSpeed;
        attackTimer = cooldown;
        
        // Spawn visual effect
        if (aoeEffectPrefab != null)
        {
            GameObject effect = Instantiate(aoeEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        
        Debug.Log($"{plantData.plantName} (Stage {currentStage}) AOE hits {targets.Count} enemies!");
    }
    
    /// <summary>
    /// Find all enemies within AOE radius
    /// </summary>
    List<UnitBase> FindAllEnemiesInRange()
    {
        List<UnitBase> targets = new List<UnitBase>();
        
        // Use existing enemiesInRange list (populated by triggers)
        foreach (UnitBase enemy in enemiesInRange)
        {
            if (enemy != null && !enemy.IsDead())
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance <= aoeRadius)
                {
                    targets.Add(enemy);
                }
            }
        }
        
        return targets;
    }
    
    protected override void UpdateCombat()
    {
        UpdateAttackTimer();
        
        // For AOE plant, attack if ANY enemies in range
        if (enemiesInRange.Count > 0 && CanAttack())
        {
            PerformAttack(null); // Pass null since we attack all
        }
    }
    
    protected override void OnStageChanged()
    {
        base.OnStageChanged();
        
        Debug.Log($"{plantData.plantName} stage {currentStage}: AOE radius={aoeRadius}");
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw AOE radius
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
        
        // Draw filled circle when attacking
        if (Application.isPlaying && enemiesInRange.Count > 0 && CanAttack())
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
            Gizmos.DrawSphere(transform.position, aoeRadius);
        }
        
        // Draw lines to all enemies in range
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            foreach (UnitBase enemy in enemiesInRange)
            {
                if (enemy != null)
                {
                    Gizmos.DrawLine(transform.position, enemy.transform.position);
                }
            }
        }
        
        // Show enemy count
        #if UNITY_EDITOR
        if (Application.isPlaying)
        {
            string status = $"Enemies: {enemiesInRange.Count}\nAOE: {aoeRadius:F1}";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, status);
        }
        #endif
    }
}
