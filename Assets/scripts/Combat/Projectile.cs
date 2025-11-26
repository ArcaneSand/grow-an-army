using UnityEngine;

/// <summary>
/// Projectile that flies toward target and deals damage on hit
/// Used by ranged and pierce attacks
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private bool pierce = false; // Pierce through enemies?
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TrailRenderer trailRenderer;

    [SerializeField] private AudioClip hitSound;
    
    // Data
    private float damage;
    private UnitBase attacker;
    private UnitTeam attackerTeam;
    private Vector2 direction;
    private System.Collections.Generic.HashSet<UnitBase> hitTargets = new System.Collections.Generic.HashSet<UnitBase>();
    
    private Rigidbody2D rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rb.gravityScale = 0f;
        
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }
    
    void Update()
    {
        // Move projectile
        transform.position += (Vector3)direction * speed * Time.deltaTime;
        
        // Rotate to face direction
        if (direction.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }
    
    /// <summary>
    /// Initialize projectile with target and damage
    /// </summary>
    public void Initialize(Vector2 targetPosition, float damage, UnitBase attacker, bool pierce = false)
    {
        this.damage = damage;
        this.attacker = attacker;
        this.attackerTeam = attacker != null ? attacker.team : UnitTeam.Player;
        this.pierce = pierce;
        
        // Calculate direction
        direction = (targetPosition - (Vector2)transform.position).normalized;
    }
    
    /// <summary>
    /// Initialize projectile with direction (for AOE/pierce)
    /// </summary>
    public void InitializeWithDirection(Vector2 direction, float damage, UnitBase attacker, bool pierce = false)
    {
        this.damage = damage;
        this.attacker = attacker;
        this.attackerTeam = attacker.team;
        this.pierce = pierce;
        this.direction = direction.normalized;
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
    
    void OnTriggerEnter2D(Collider2D other)
    {
            if (other.isTrigger) return;
        // Check if hit an enemy unit
        UnitBase unit = other.GetComponent<UnitBase>();
        
        if (unit != null && unit.team != attackerTeam && !unit.IsDead())
        {
            // Check if already hit this target (for pierce)
            if (hitTargets.Contains(unit)) return;
            
            // Deal damage
            unit.TakeDamage(damage, attacker);
            hitTargets.Add(unit);
            
            Debug.Log($"Projectile hit {unit.name} for {damage} damage!");

            SoundManager.Instance.PlaySoundFX(hitSound, transform);
            
            // Destroy if not piercing
            if (!pierce)
            {
                DestroyProjectile();
            }
        }
    }
    
    void DestroyProjectile()
    {
        // Optional: Spawn hit effect here
        Destroy(gameObject);
    }
    
    void OnDrawGizmos()
    {
        // Draw direction arrow
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction * 0.5f);
        }
    }
}