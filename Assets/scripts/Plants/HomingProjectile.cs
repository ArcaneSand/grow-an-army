using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Homing Projectile - Flies around like a bee, homes to nearest enemy
/// Attacks enemies it touches with rate limiting (1 hit per second per enemy)
/// Used by Mage Plant (Lotus)
/// </summary>
public class HomingProjectile : MonoBehaviour
{
    [Header("Homing Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float homingStrength = 5f; // How strongly it homes
    [SerializeField] private float detectionRadius = 10f; // How far it can see enemies
    [SerializeField] private float wanderStrength = 2f; // Random movement when no target
    
    [Header("Combat Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float hitCooldownPerEnemy = 1f; // 1 second between hits on same enemy
    [SerializeField] private float lifetime = 10f; // Destroy after 10 seconds
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TrailRenderer trailRenderer;
    
    // Data
    private UnitBase attacker; // The plant that spawned this
    private UnitTeam attackerTeam;
    private Vector2 velocity;
    private Vector2 wanderTarget;
    private float wanderTimer = 0f;
    
    // Hit tracking - remember last hit time for each enemy
    private Dictionary<UnitBase, float> lastHitTimes = new Dictionary<UnitBase, float>();
    
    // Components
    private Rigidbody2D rb;
    private CircleCollider2D col;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.linearDamping = 0.5f; // Slight drag for smooth movement
        
        col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.3f;
            col.isTrigger = true;
        }
        
        // Initialize random velocity
        velocity = Random.insideUnitCircle.normalized * moveSpeed;
        
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        UpdateMovement();
        UpdateRotation();
        CleanupHitTracking();
    }
    
    void FixedUpdate()
    {
        // Apply velocity to rigidbody
        rb.linearVelocity = velocity;
    }
    
    /// <summary>
    /// Initialize projectile with damage and attacker
    /// </summary>
    public void Initialize(float damage, UnitBase attacker)
    {
        this.damage = damage;
        this.attacker = attacker;
        this.attackerTeam = attacker != null ? attacker.team : UnitTeam.Player;
        
        SetProjectileLayer();
    }
    
    void SetProjectileLayer()
    {
        if (attackerTeam == UnitTeam.Player)
        {
            gameObject.layer = LayerMask.NameToLayer("PlayerProjectile");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("EnemyProjectile");
        }
    }
    
    void UpdateMovement()
    {
        // Find nearest enemy
        UnitBase nearestEnemy = FindNearestEnemy();
        
        if (nearestEnemy != null)
        {
            // Home toward enemy
            Vector2 directionToEnemy = (nearestEnemy.transform.position - transform.position).normalized;
            velocity += directionToEnemy * homingStrength * Time.deltaTime;
        }
        else
        {
            // No enemy - wander randomly
            UpdateWander();
        }
        
        // Clamp speed
        velocity = Vector2.ClampMagnitude(velocity, moveSpeed);
    }
    
    void UpdateWander()
    {
        wanderTimer -= Time.deltaTime;
        
        if (wanderTimer <= 0f)
        {
            // Pick new random wander direction
            wanderTimer = Random.Range(0.5f, 2f);
            wanderTarget = Random.insideUnitCircle.normalized;
        }
        
        // Apply wander force
        velocity += wanderTarget * wanderStrength * Time.deltaTime;
    }
    
    void UpdateRotation()
    {
        // Rotate to face movement direction
        if (velocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }
    
    UnitBase FindNearestEnemy()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        
        UnitBase nearest = null;
        float nearestDistance = float.MaxValue;
        
        foreach (Collider2D col in colliders)
        {
            UnitBase unit = col.GetComponent<UnitBase>();
            
            if (unit != null && unit.team != attackerTeam && !unit.IsDead())
            {
                float distance = Vector2.Distance(transform.position, unit.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = unit;
                }
            }
        }
        
        return nearest;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.isTrigger) return;
        
        // Check if hit an enemy unit
        UnitBase unit = other.GetComponent<UnitBase>();
        
        if (unit != null && unit.team != attackerTeam && !unit.IsDead())
        {
            TryHitEnemy(unit);
        }
    }
    
    void TryHitEnemy(UnitBase enemy)
    {
        // Check if we can hit this enemy (cooldown check)
        if (lastHitTimes.ContainsKey(enemy))
        {
            float timeSinceLastHit = Time.time - lastHitTimes[enemy];
            if (timeSinceLastHit < hitCooldownPerEnemy)
            {
                // Still on cooldown for this enemy
                return;
            }
        }
        
        // Deal damage
        enemy.TakeDamage(damage, attacker);
        
        // Record hit time
        lastHitTimes[enemy] = Time.time;
        
        Debug.Log($"Homing projectile hit {enemy.name} for {damage} damage!");
        
        // Optional: Spawn hit effect
        OnHitEffect();
    }
    
    void OnHitEffect()
    {
        // Add particles, sound, etc.
    }
    
    void CleanupHitTracking()
    {
        // Remove dead/null enemies from tracking
        List<UnitBase> toRemove = new List<UnitBase>();
        
        foreach (var kvp in lastHitTimes)
        {
            if (kvp.Key == null || kvp.Key.IsDead())
            {
                toRemove.Add(kvp.Key);
            }
        }
        
        foreach (UnitBase unit in toRemove)
        {
            lastHitTimes.Remove(unit);
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw detection radius
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Draw velocity vector
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)velocity.normalized * 0.5f);
        }
    }
}
