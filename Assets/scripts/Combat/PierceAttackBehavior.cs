using UnityEngine;

/// <summary>
/// Pierce attack behavior - projectile that goes through multiple enemies
/// Used by: Mage
/// </summary>
public class PierceAttackBehavior : AttackBehavior
{
    [Header("Pierce Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 12f;
    [SerializeField] private float projectileLifetime = 2f;
    [SerializeField] private int maxPierceTargets = 5; // Max enemies to hit
    [SerializeField] private Transform projectileSpawnPoint;
    
    public override void Execute(UnitBase target, UnitBase attacker)
    {
        if (!CanExecute(target, attacker)) return;
        
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{attacker.name} has no pierce projectile prefab!");
            return;
        }
        
        // Calculate direction to target
        Vector2 direction = (target.transform.position - attacker.transform.position).normalized;
        
        // Spawn projectile
        Vector3 spawnPos = projectileSpawnPoint != null 
            ? projectileSpawnPoint.position 
            : attacker.transform.position;
        
        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        
        if (projectile != null)
        {
            projectile.InitializeWithDirection(direction, attackDamage, attacker, pierce: true);
        }
        
        // Trigger animation
        if (attacker.unitHandler != null)
        {
            attacker.unitHandler.Attack();
        }
        
        Debug.Log($"{attacker.name} shoots piercing projectile!");
    }
    
    /// <summary>
    /// Set projectile prefab from UnitDataSO
    /// </summary>
    public void SetProjectilePrefab(GameObject prefab, float speed)
    {
        projectilePrefab = prefab;
        projectileSpeed = speed;
    }
    
    public void SetMaxPierceTargets(int max)
    {
        maxPierceTargets = max;
    }
}