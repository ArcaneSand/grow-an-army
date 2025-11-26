using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages a group of enemies that move and react together
/// Groups wander randomly, chase attackers, then disengage easily
/// </summary>
public class EnemyGroup : MonoBehaviour
{
    [Header("Group Settings")]
    [SerializeField] private float groupRadius = 2f; // How spread out the group is
    [SerializeField] private float movementSpeed = 1.5f;
    [SerializeField] private float waypointReachDistance = 1f;
    
    [Header("Behavior Settings")]
    [SerializeField] private float wanderInterval = 3f; // How often to pick new random direction
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float chaseRange = 10f; // How far to chase
    [SerializeField] private float disengageTime = 3f; // Give up after 3 seconds
    [SerializeField] private float retreatDistance = 5f; // Pull back this far when disengaging
    
    [Header("Boundary Settings")]
    [SerializeField] private bool useBoundaries = true;
    [SerializeField] private Vector2 boundaryCenter = Vector2.zero; // Auto-set from parent Room
    [SerializeField] private Vector2 boundarySize = new Vector2(20f, 20f); // Width x Height
    private BoxCollider2D roomCollider;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    // Group state
    public enum GroupState
    {
        Wandering,  // Moving randomly
        Alerted,    // Moving toward last known attacker position
        Chasing,    // Actively pursuing attacker
        Retreating  // Pulling back after losing aggro
    }
    
    private GroupState currentState = GroupState.Wandering;
    private List<EnemyUnit> groupMembers = new List<EnemyUnit>();
    
    // FIXED: Separate virtual center from unit positions
    private Vector2 virtualGroupCenter;  // Logical center (stable, moves smoothly)
    private Vector2 targetPosition;      // Where virtual center is moving toward
    private Vector2 lastKnownAttackerPosition;
    private Transform currentAttacker;
    
    // Timers
    private float wanderTimer;
    private float disengageTimer;
    private Vector2 retreatTarget;
    
    void Start()
    {
        // Try to get boundary from parent RoomManager
        RoomManager parentRoom = GetComponentInParent<RoomManager>();
        if (parentRoom != null && useBoundaries)
        {
            // Get room bounds if available
            roomCollider = parentRoom.GetComponent<BoxCollider2D>();
            if (roomCollider != null)
            {
                boundaryCenter = roomCollider.bounds.center;
                //boundarySize = roomCollider.bounds.size;
                boundarySize = new Vector2(25,15);
            }
            else
            {
                // Use room transform position as center
                boundaryCenter = parentRoom.transform.position;
            }
            
            Debug.Log($"EnemyGroup '{gameObject.name}' using boundaries: Center={boundaryCenter}, Size={boundarySize}");
        }
        
        // FIXED: Initialize virtual center at spawn position (stable)
        virtualGroupCenter = transform.position;
        targetPosition = virtualGroupCenter;
        wanderTimer = wanderInterval;
        
        // Find all child enemy units
        FindGroupMembers();
        
        // Pick initial waypoint within bounds
        PickRandomWaypoint();
    }
    
    void Update()
    {
        UpdateGroupState();
        UpdateGroupMovement();
    }
    
    #region Group Management
    
    /// <summary>
    /// Find all EnemyUnit children and register them
    /// </summary>
    void FindGroupMembers()
    {
        EnemyUnit[] enemies = GetComponentsInChildren<EnemyUnit>();
        foreach (EnemyUnit enemy in enemies)
        {
            RegisterEnemy(enemy);
        }
        
        Debug.Log($"EnemyGroup '{gameObject.name}' has {groupMembers.Count} members");
    }
    
    /// <summary>
    /// Add enemy to this group
    /// </summary>
    public void RegisterEnemy(EnemyUnit enemy)
    {
        if (!groupMembers.Contains(enemy))
        {
            groupMembers.Add(enemy);
            enemy.SetGroup(this);
        }
    }
    
    /// <summary>
    /// Remove enemy from group (when they die)
    /// </summary>
    public void UnregisterEnemy(EnemyUnit enemy)
    {
        groupMembers.Remove(enemy);
        
        // Check if group is empty
        if (groupMembers.Count == 0)
        {
            Debug.Log($"EnemyGroup '{gameObject.name}' is empty - destroying");
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Get target position for group members to move toward
    /// FIXED: Returns stable virtual center, not calculated average
    /// </summary>
    public Vector2 GetGroupTarget()
    {
        return virtualGroupCenter;
    }
    
    /// <summary>
    /// Get actual center (average position of all members)
    /// Used only for display/stats, NOT for movement
    /// </summary>
    public Vector2 GetActualCenter()
    {
        if (groupMembers.Count == 0) return transform.position;
        
        Vector2 center = Vector2.zero;
        foreach (EnemyUnit enemy in groupMembers)
        {
            if (enemy != null)
            {
                center += (Vector2)enemy.transform.position;
            }
        }
        return center / groupMembers.Count;
    }
    
    /// <summary>
    /// Get virtual group center (stable position for movement)
    /// </summary>
    public Vector2 GetGroupCenter()
    {
        return virtualGroupCenter;
    }
    
    #endregion
    
    #region Group Behavior
    
    void UpdateGroupState()
    {
        // FIXED: Don't recalculate center - virtual center is stable
        // (Only use GetActualCenter() for display/debug purposes)
        
        switch (currentState)
        {
            case GroupState.Wandering:
                UpdateWandering();
                break;
                
            case GroupState.Alerted:
                UpdateAlerted();
                break;
                
            case GroupState.Chasing:
                UpdateChasing();
                break;
                
            case GroupState.Retreating:
                UpdateRetreating();
                break;
        }
    }
    
    void UpdateWandering()
    {
        wanderTimer -= Time.deltaTime;
        
        // Pick new random direction periodically
        if (wanderTimer <= 0f)
        {
            wanderTimer = wanderInterval;
            PickRandomWaypoint();
            EnforceBoundaries();
        }
        
        // FIXED: Check virtual center, not calculated center
        // Check if reached waypoint
        if (Vector2.Distance(virtualGroupCenter, targetPosition) < waypointReachDistance)
        {
            PickRandomWaypoint();
        }
    }
    
    void UpdateAlerted()
    {
        // Move toward last known attacker position
        targetPosition = lastKnownAttackerPosition;
        
        // FIXED: Check virtual center
        // Check if reached alert position
        if (Vector2.Distance(virtualGroupCenter, lastKnownAttackerPosition) < 2f)
        {
            // Arrived at position, start retreating
            StartRetreating();
        }
        
        // Check if attacker came back in range
        if (currentAttacker != null)
        {
            float distance = Vector2.Distance(virtualGroupCenter, currentAttacker.position);
            if (distance <= chaseRange)
            {
                StartChasing(currentAttacker);
            }
        }
    }
    
    void UpdateChasing()
    {
        if (currentAttacker == null)
        {
            StartRetreating();
            return;
        }
        
        // Update target to attacker position
        targetPosition = currentAttacker.position;
        lastKnownAttackerPosition = currentAttacker.position;
        
        // FIXED: Check virtual center distance
        // Check distance
        float distance = Vector2.Distance(virtualGroupCenter, currentAttacker.position);
        
        // Too far - disengage
        if (distance > chaseRange)
        {
            disengageTimer -= Time.deltaTime;
            
            if (disengageTimer <= 0f)
            {
                Debug.Log($"Group '{gameObject.name}' disengaging - target too far");
                StartRetreating();
            }
        }
        else
        {
            // Reset disengage timer if back in range
            disengageTimer = disengageTime;
        }
    }
    
    void UpdateRetreating()
    {
        // Move to retreat position
        targetPosition = retreatTarget;
        
        // FIXED: Check virtual center
        // Check if reached retreat position
        if (Vector2.Distance(virtualGroupCenter, retreatTarget) < 1f)
        {
            // Resume wandering
            StartWandering();
        }
    }
    
    #endregion
    
    #region State Transitions
    
    void StartWandering()
    {
        currentState = GroupState.Wandering;
        currentAttacker = null;
        wanderTimer = wanderInterval;
        PickRandomWaypoint();
        Debug.Log($"Group '{gameObject.name}' wandering");
    }
    
    void StartAlerted(Vector2 attackerPosition, Transform attacker)
    {
        currentState = GroupState.Alerted;
        lastKnownAttackerPosition = attackerPosition;
        currentAttacker = attacker;
        targetPosition = attackerPosition;
        Debug.Log($"Group '{gameObject.name}' alerted to position {attackerPosition}");
    }
    
    void StartChasing(Transform attacker)
    {
        currentState = GroupState.Chasing;
        currentAttacker = attacker;
        disengageTimer = disengageTime;
        Debug.Log($"Group '{gameObject.name}' chasing {attacker.name}");
    }
    
    void StartRetreating()
    {
        currentState = GroupState.Retreating;
        
        // FIXED: Calculate retreat from virtual center
        // Calculate retreat position (away from last known attacker)
        if (currentAttacker != null)
        {
            Vector2 awayDirection = (virtualGroupCenter - (Vector2)currentAttacker.position).normalized;
            retreatTarget = virtualGroupCenter + awayDirection * retreatDistance;
        }
        else
        {
            retreatTarget = virtualGroupCenter;
        }
        
        currentAttacker = null;
        Debug.Log($"Group '{gameObject.name}' retreating to {retreatTarget}");
    }
    
    #endregion
    
    #region Movement
    
    void UpdateGroupMovement()
    {
        // FIXED: Move virtual center smoothly toward target
        // This is stable and independent of unit positions
        float speed = (currentState == GroupState.Chasing) ? chaseSpeed : movementSpeed;
        
        Vector2 direction = (targetPosition - virtualGroupCenter).normalized;
        virtualGroupCenter = Vector2.MoveTowards(virtualGroupCenter, targetPosition, speed * Time.deltaTime);
        
        // Update transform position for gizmo visualization
        transform.position = virtualGroupCenter;
    }
    
    void PickRandomWaypoint()
    {
        // FIXED: Pick from stable virtual center
        // Pick random point around current position
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(3f, 8f);
        Vector2 newTarget = virtualGroupCenter + randomDirection * randomDistance;
        
        // Clamp to boundaries if enabled
        if (useBoundaries)
        {
            newTarget = ClampToBoundaries(newTarget);
        }
        
        targetPosition = newTarget;
        
        Debug.Log($"Group '{gameObject.name}' picked new waypoint: {targetPosition}");
    }
    
    /// <summary>
    /// Clamp position to stay within room boundaries
    /// </summary>
    Vector2 ClampToBoundaries(Vector2 position)
    {
        float halfWidth = boundarySize.x * 0.5f;
        float halfHeight = boundarySize.y * 0.5f;
        
        float clampedX = Mathf.Clamp(position.x, boundaryCenter.x - halfWidth, boundaryCenter.x + halfWidth);
        float clampedY = Mathf.Clamp(position.y, boundaryCenter.y - halfHeight, boundaryCenter.y + halfHeight);
        
        return new Vector2(clampedX, clampedY);
    }
    
    /// <summary>
    /// Check if position is within boundaries
    /// </summary>
    bool IsWithinBoundaries(Vector2 position)
    {
        if (!useBoundaries) return true;
        
        float halfWidth = boundarySize.x * 0.5f;
        float halfHeight = boundarySize.y * 0.5f;
        
        return position.x >= boundaryCenter.x - halfWidth &&
               position.x <= boundaryCenter.x + halfWidth &&
               position.y >= boundaryCenter.y - halfHeight &&
               position.y <= boundaryCenter.y + halfHeight;
    }
    
    bool IsInsideCollider(Vector2 position)
    {
        return roomCollider.OverlapPoint(position);
    }

    void EnforceBoundaries()
    {
        foreach (EnemyUnit enemy in groupMembers)
        {
            if (!IsWithinBoundaries(enemy.transform.position))
            {
                // Teleport back!
                enemy.transform.position = boundaryCenter;
            }
        }
    }
    #endregion
    
    #region Aggro System
    
    /// <summary>
    /// Called by EnemyUnit when it takes damage
    /// </summary>
    public void OnMemberAttacked(UnitBase attacker)
    {
        if (attacker == null) return;
        
        Debug.Log($"Group '{gameObject.name}' member attacked by {attacker.name}");
        
        switch (currentState)
        {
            case GroupState.Wandering:
                // First attack - become alerted
                StartAlerted(attacker.transform.position, attacker.transform);
                break;
                
            case GroupState.Alerted:
            case GroupState.Retreating:
                // Already alerted - check if close enough to chase
                float distance = Vector2.Distance(virtualGroupCenter, attacker.transform.position);
                if (distance <= chaseRange)
                {
                    StartChasing(attacker.transform);
                }
                break;
                
            case GroupState.Chasing:
                // Update last known position
                lastKnownAttackerPosition = attacker.transform.position;
                disengageTimer = disengageTime; // Reset timer
                break;
        }
    }
    
    #endregion
    
    #region Debug
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw boundaries (room limits)
        if (useBoundaries)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f); // Cyan transparent
            Gizmos.DrawWireCube(boundaryCenter, boundarySize);
            
            // Draw corner markers
            Gizmos.color = Color.cyan;
            float halfWidth = boundarySize.x * 0.5f;
            float halfHeight = boundarySize.y * 0.5f;
            Gizmos.DrawSphere(boundaryCenter + new Vector2(-halfWidth, -halfHeight), 0.2f);
            Gizmos.DrawSphere(boundaryCenter + new Vector2(halfWidth, -halfHeight), 0.2f);
            Gizmos.DrawSphere(boundaryCenter + new Vector2(-halfWidth, halfHeight), 0.2f);
            Gizmos.DrawSphere(boundaryCenter + new Vector2(halfWidth, halfHeight), 0.2f);
        }
        
        // FIXED: Draw virtual group center (stable - what units follow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(virtualGroupCenter, 0.5f);
        
        // Draw actual center (for debug comparison)
        if (Application.isPlaying)
        {
            Vector2 actualCenter = GetActualCenter();
            Gizmos.color = new Color(1, 0.5f, 0, 0.5f); // Orange, transparent
            Gizmos.DrawWireSphere(actualCenter, 0.3f);
        }
        
        // Draw target position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPosition, 0.3f);
        Gizmos.DrawLine(virtualGroupCenter, targetPosition);
        
        // Draw group radius
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(virtualGroupCenter, groupRadius);
        
        // Draw chase range
        if (currentState == GroupState.Chasing || currentState == GroupState.Alerted)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireSphere(virtualGroupCenter, chaseRange);
        }
        
        // Draw state
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(virtualGroupCenter + Vector2.up * 2f, $"State: {currentState}\nMembers: {groupMembers.Count}");
        #endif
    }
    
    #endregion
}