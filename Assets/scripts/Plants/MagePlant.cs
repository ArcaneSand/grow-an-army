using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Mage Plant (Lotus) - Spawns homing projectiles that fly around like bees
/// Projectiles home to enemies and attack anything they touch
/// Higher stages spawn more projectiles
/// </summary>
public class MagePlant : PlantTurretBase
{
    [Header("Mage Plant - Homing Projectiles")]
    [SerializeField] private GameObject homingProjectilePrefab;
    [SerializeField] private int maxActiveProjectiles = 3; // Max bees at once
    [SerializeField] private float projectileSpawnInterval = 3f; // Spawn new bee every 3 seconds
    [SerializeField] private float projectileLifetime = 10f;
    
    private List<HomingProjectile> activeProjectiles = new List<HomingProjectile>();
    private float spawnTimer = 0f;
    
    protected override void Update()
    {
        base.Update();
        UpdateProjectileSpawning();
    }
    
    void UpdateProjectileSpawning()
    {
        // Clean up null projectiles
        activeProjectiles.RemoveAll(p => p == null);
        
        // Check if we should spawn more projectiles
        if (activeProjectiles.Count < maxActiveProjectiles)
        {
            spawnTimer -= Time.deltaTime;
            
            if (spawnTimer <= 0f)
            {
                spawnTimer = projectileSpawnInterval;
                SpawnHomingProjectile();
            }
        }
    }
    
    void SpawnHomingProjectile()
    {
        if (homingProjectilePrefab == null)
        {
            Debug.LogWarning($"{plantData.plantName} has no homing projectile prefab!");
            return;
        }
        
        // Spawn at plant position with random offset
        Vector2 spawnOffset = Random.insideUnitCircle * 0.5f;
        Vector3 spawnPos = transform.position + (Vector3)spawnOffset;
        
        GameObject projectileObj = Instantiate(homingProjectilePrefab, spawnPos, Quaternion.identity);
        HomingProjectile projectile = projectileObj.GetComponent<HomingProjectile>();
        
        if (projectile != null)
        {
            // Initialize with damage based on current stage
            float damage = currentStats.attackDamage;
            projectile.Initialize(damage, null); // Pass null as attacker since plant doesn't need credit
            
            activeProjectiles.Add(projectile);
            
            Debug.Log($"{plantData.plantName} spawned homing projectile (Stage {currentStage})");
        }
    }
    
    protected override void OnStageChanged()
    {
        base.OnStageChanged();
        
        // Increase max projectiles with stage
        switch (currentStage)
        {
            case 0: // Sprout
                maxActiveProjectiles = 1;
                projectileSpawnInterval = 5f;
                break;
            case 1: // Bud
                maxActiveProjectiles = 2;
                projectileSpawnInterval = 4f;
                break;
            case 2: // Rooted
                maxActiveProjectiles = 3;
                projectileSpawnInterval = 3f;
                break;
        }
        
        Debug.Log($"{plantData.plantName} stage {currentStage}: {maxActiveProjectiles} max projectiles");
    }
    
    protected override void UpdateCombat()
    {
        // Mage plant doesn't directly attack - it spawns projectiles
        // Override to disable normal combat
    }
    
    protected override void OnUprooted()
    {
        base.OnUprooted();
        
        // Destroy all active projectiles when uprooted
        foreach (HomingProjectile projectile in activeProjectiles)
        {
            if (projectile != null)
            {
                Destroy(projectile.gameObject);
            }
        }
        activeProjectiles.Clear();
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw spawn range
        Gizmos.color = new Color(1f, 0f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw lines to active projectiles
        if (Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            foreach (HomingProjectile projectile in activeProjectiles)
            {
                if (projectile != null)
                {
                    Gizmos.DrawLine(transform.position, projectile.transform.position);
                }
            }
        }
        
        // Show projectile count
        #if UNITY_EDITOR
        if (Application.isPlaying)
        {
            string status = $"Bees: {activeProjectiles.Count}/{maxActiveProjectiles}\nSpawn: {spawnTimer:F1}s";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, status);
        }
        #endif
    }
}
