using UnityEngine;

/// <summary>
/// Enemy units that belong to a group and follow group behavior
/// Individual combat when player is in range
/// Reports damage to group for group-wide aggro
/// </summary>
public class EnemyUnit : UnitBase
{
    [Header("Enemy AI Settings")]
    [SerializeField] private float aggroRange = 10f;
    [SerializeField] private float groupFollowRadius = 2f; // Stay within this distance of group target
    [SerializeField] private float chaseTimeout = 5f; // Give up chase after 5 seconds if can't reach
    [SerializeField] private float targetLostDistance = 15f; // Forget target if this far away
    [SerializeField] private float allyDetectionInterval = 0.5f; // How often to scan for allies
    
    private EnemyGroup myGroup;
    private bool hasIndividualAggro = false;
    private float currentChaseTime = 0f;
    private float lastAttackTime = 0f;
    private float allyDetectionTimer = 0f;
    
    protected override void Start()
    {
        base.Start();
        
        // Group will be set by EnemyGroup.RegisterEnemy()
    }
    
    protected override UnitTeam GetTeam()
    {
        return UnitTeam.Enemy;
    }
    
    /// <summary>
    /// Set this enemy's group (called by EnemyGroup)
    /// </summary>
    public void SetGroup(EnemyGroup group)
    {
        myGroup = group;
        Debug.Log($"{gameObject.name} joined group {group.gameObject.name}");
    }

    protected override void UpdateMovement()
    {
        // Priority 1: Chase and attack current target if we have one
        if (currentTarget != null && !currentTarget.IsDead())
        {
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);
            
            // Check if target is too far away - give up
            if (distanceToTarget > targetLostDistance)
            {
                Debug.Log($"{gameObject.name} lost target - too far away");
                currentTarget = null;
                currentChaseTime = 0f;
                return;
            }
            
            // Move toward target if NOT in attack range
            if (distanceToTarget > unitData.attackRange)
            {
                MoveTowards(currentTarget.transform.position);
                
                // Update chase timer
                currentChaseTime += Time.deltaTime;
                
                // Give up if chasing too long without reaching
                if (currentChaseTime > chaseTimeout)
                {
                    Debug.Log($"{gameObject.name} gave up chase - timeout");
                    currentTarget = null;
                    currentChaseTime = 0f;
                }
            }
            else
            {
                // In attack range - stop moving and attack
                moveDirection = Vector2.zero;
                currentChaseTime = 0f; // Reset timer when in range
            }
            return;
        }
        
        // No target - reset chase timer
        currentChaseTime = 0f;
        
        // Priority 2: Individual aggro - look for nearby AllyUnits
        allyDetectionTimer -= Time.deltaTime;
        if (allyDetectionTimer <= 0f)
        {
            allyDetectionTimer = allyDetectionInterval;
            
            // Find nearest ally unit
            AllyUnit nearestAlly = FindNearestAllyUnit();
            if (nearestAlly != null)
            {
                float distanceToAlly = Vector2.Distance(transform.position, nearestAlly.transform.position);
                if (distanceToAlly <= aggroRange)
                {
                    hasIndividualAggro = true;
                    MoveTowards(nearestAlly.transform.position);
                    return;
                }
            }
        }
        
        // Priority 3: Follow group
        if (myGroup != null)
        {
            FollowGroup();
        }
        else
        {
            // No group - stay still or wander
            moveDirection = Vector2.zero;
        }
        
        hasIndividualAggro = false;
    }
    
    /// <summary>
    /// Find the nearest AllyUnit within detection range
    /// </summary>
    AllyUnit FindNearestAllyUnit()
    {
        AllyUnit[] allAllies = FindObjectsOfType<AllyUnit>();
        AllyUnit nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach (AllyUnit ally in allAllies)
        {
            if (ally == null || ally.IsDead()) continue;
            
            float distance = Vector2.Distance(transform.position, ally.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = ally;
            }
        }
        
        return nearest;
    }
    
    /// <summary>
    /// Follow the group's target position
    /// </summary>
    void FollowGroup()
    {
        Vector2 groupTarget = myGroup.GetGroupTarget();
        float distanceToGroupTarget = Vector2.Distance(transform.position, groupTarget);
        
        // Only move if far from group target
        if (distanceToGroupTarget > groupFollowRadius)
        {
            MoveTowards(groupTarget);
        }
        else
        {
            // Close enough to group position
            moveDirection = Vector2.zero;
        }
    }
    
    /// <summary>
    /// Override to report damage to group and reset chase timer
    /// </summary>
    public override void TakeDamage(float damage, UnitBase attacker)
    {
        base.TakeDamage(damage, attacker);
        
        // Reset chase timer on fresh attack
        currentChaseTime = 0f;
        
        // Report to group that we were attacked
        if (myGroup != null && attacker != null)
        {
            myGroup.OnMemberAttacked(attacker);
        }
    }
    
    protected override void OnDeath(UnitBase killer)
    {
        base.OnDeath(killer);
        
        // Spawn grass and plant when enemy dies
        SpawnGrassAndPlant();
        
        // Notify group
        if (myGroup != null)
        {
            myGroup.UnregisterEnemy(this);
        }
        
        // Notify room manager
        RoomManager currentRoom = GetComponentInParent<RoomManager>();
        if (currentRoom != null)
        {
            currentRoom.OnEnemyDeath(this);
        }
    }
    
    void SpawnGrassAndPlant()
    {
        if (unitData == null) return;
        
        if (GrassManager.Instance != null)
        {
            GrassManager.Instance.TrySpawnGrassAndPlant(transform.position, unitData.plantToSpawn);
        }
    }
    
    protected override void OnAttackPerformed(UnitBase target)
    {
        base.OnAttackPerformed(target);
        
        // Add visual feedback for enemy attacks
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw aggro range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        
        // Draw target lost distance
        Gizmos.color = new Color(1, 0, 0, 0.2f);
        Gizmos.DrawWireSphere(transform.position, targetLostDistance);
        
        // Draw group follow radius
        if (myGroup != null)
        {
            Gizmos.color = Color.blue;
            Vector2 groupTarget = myGroup.GetGroupTarget();
            Gizmos.DrawWireSphere(groupTarget, groupFollowRadius);
            Gizmos.DrawLine(transform.position, groupTarget);
        }
        
        // Show chase timer status
        #if UNITY_EDITOR
        if (Application.isPlaying && currentTarget != null)
        {
            float timeLeft = chaseTimeout - currentChaseTime;
            string status = $"Chase: {timeLeft:F1}s left";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, status);
        }
        #endif
    }
}