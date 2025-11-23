using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Placeholder for Phase 5 - Room System
/// Will manage enemies in a room and handle room progression
/// </summary>
public class RoomManager : MonoBehaviour
{
    private List<EnemyUnit> activeEnemies = new List<EnemyUnit>();
    
    public void OnEnemyDeath(EnemyUnit enemy)
    {
        // TODO: Implement in Phase 5
        activeEnemies.Remove(enemy);
        Debug.Log($"[RoomManager] Enemy died. Remaining: {activeEnemies.Count}");
        
        if (activeEnemies.Count == 0)
        {
            Debug.Log("[RoomManager] Room cleared!");
        }
    }
    
    public void RegisterEnemy(EnemyUnit enemy)
    {
        if (!activeEnemies.Contains(enemy))
        {
            activeEnemies.Add(enemy);
        }
    }
}
