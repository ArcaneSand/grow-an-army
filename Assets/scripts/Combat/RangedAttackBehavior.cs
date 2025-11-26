using UnityEngine;

/// <summary>
/// Ranged attack behavior - spawns projectile toward target
/// Used by: Archer
/// </summary>
public class RangedAttackBehavior : AttackBehavior
{
    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private Transform projectileSpawnPoint; // Optional spawn offset
    
    public override void Execute(UnitBase target, UnitBase attacker)
    {
        if (!CanExecute(target, attacker)) return;
        
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{attacker.name} has no projectile prefab assigned!");
            return;
        }
        
        // Spawn projectile
        Vector3 spawnPos = projectileSpawnPoint != null 
            ? projectileSpawnPoint.position 
            : attacker.transform.position;
        
        GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        
        if (projectile != null)
        {
            projectile.Initialize(target.transform.position, attackDamage, attacker, pierce: false);
        }
        
        // Trigger animation
        if (attacker.unitHandler != null)
        {
            attacker.unitHandler.Attack();
        }
        
        Debug.Log($"{attacker.name} shoots arrow at {target.name}!");
    }
    
    /// <summary>
    /// Set projectile prefab from UnitDataSO
    /// </summary>
    public void SetProjectilePrefab(GameObject prefab, float speed)
    {
        projectilePrefab = prefab;
        projectileSpeed = speed;
    }
}