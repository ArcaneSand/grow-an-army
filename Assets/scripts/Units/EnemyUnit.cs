using UnityEngine;

/// <summary>
/// Enemy units that chase and attack the player and their allies
/// Inherits combat behavior from UnitBase
/// Modified to use new grass spawning system with chance-based mechanics
/// </summary>
public class EnemyUnit : UnitBase
{
    
    protected override void Start()
    {
        base.Start();
    }
    
    protected override UnitTeam GetTeam()
    {
        return UnitTeam.Enemy;
    }

    protected override void UpdateMovement()
    {

    }
    
    protected override void OnDeath(UnitBase killer)
    {
        base.OnDeath(killer);
        
        // Try to spawn grass and plant (with chance)
        TrySpawnGrassAndPlant();
        
        // Notify room manager
        RoomManager currentRoom = GetComponentInParent<RoomManager>();
        if (currentRoom != null)
        {
            currentRoom.OnEnemyDeath(this);
        }
    }
    
    /// <summary>
    /// Attempts to spawn grass + plant at nearest empty cell
    /// Uses chance-based system from GrassManager
    /// </summary>
    void TrySpawnGrassAndPlant()
    {
        if (unitData == null)
        {
            Debug.LogWarning("EnemyUnit missing UnitDataSO - cannot spawn grass/plant");
            return;
        }
        
        if (GrassManager.Instance == null)
        {
            Debug.LogWarning("GrassManager not found - cannot spawn grass/plant");
            return;
        }
        
        // Call the new method that handles chance rolling and nearest-cell search
        GrassManager.Instance.TrySpawnGrassAndPlant(transform.position, unitData.plantToSpawn);
    }
    
    protected override void OnAttackPerformed(UnitBase target)
    {
        base.OnAttackPerformed(target);
        
        // Add visual feedback for enemy attacks
        // TODO: Add attack animation/particles
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw aggro range
        Gizmos.color = Color.cyan;
    }
}