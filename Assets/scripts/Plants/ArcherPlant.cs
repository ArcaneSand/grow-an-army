using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Archer Plant (Rose) - Long range attacks with projectiles
/// Fires arrows at enemies from far away
/// Higher stages have faster attack speed and longer range
/// </summary>
public class ArcherPlant : PlantTurretBase
{
    [Header("Archer Plant - Ranged Combat")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float extraRange = 5f; // Bonus range beyond base
    
    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        
        // Archer plants have extended range
        if (detectionCollider != null && currentStats != null)
        {
            detectionCollider.radius = currentStats.attackRange + extraRange;
        }
    }
    
    protected override void SetStage(int stage)
    {
        base.SetStage(stage);
        
        // Update detection range when stage changes
        if (detectionCollider != null && currentStats != null)
        {
            detectionCollider.radius = currentStats.attackRange + extraRange;
        }
    }
    
    protected override void PerformAttack(UnitBase target)
    {
        if (target == null || target.IsDead()) return;
        
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{plantData.plantName} has no projectile prefab!");
            return;
        }
        
        // Spawn projectile
        Vector3 spawnPos = projectileSpawnPoint != null 
            ? projectileSpawnPoint.position 
            : transform.position;
        
        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(target.transform.position, currentStats.attackDamage, null, pierce: false);
        }
        
        // Reset attack timer
        float cooldown = 1f / currentStats.attackSpeed;
        attackTimer = cooldown;
        
        // Visual/audio feedback
        OnAttackPerformed(target);
        
        Debug.Log($"{plantData.plantName} (Stage {currentStage}) shoots arrow at {target.name}!");
    }
    
    protected override void OnStageChanged()
    {
        base.OnStageChanged();
        
        // Archer gets better range and attack speed with growth
        Debug.Log($"{plantData.plantName} stage {currentStage}: Range={currentStats.attackRange + extraRange}, Speed={currentStats.attackSpeed}/s");
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw extended attack range
        if (currentStats != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, currentStats.attackRange + extraRange);
        }
        
        // Draw line to current target
        if (Application.isPlaying && currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
}
