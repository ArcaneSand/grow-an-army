using UnityEngine;

/// <summary>
/// Player controller - moves towards mouse position like lordz.io
/// The player is the sapling character
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stoppingDistance = 0.2f;
    
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;   
    
    [Header("Grass Bonuses")]
    [SerializeField] private float grassSpeedMultiplier = 1.3f;
    [SerializeField] private float grassHealPerSecond = 5f;
    
    // Components
    private Rigidbody2D rb;
    private Camera mainCamera;
    
    // State
    private Vector2 targetPosition;
    private bool isOnGrass = false;
    private float currentMoveSpeed;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rb.gravityScale = 0f;
        rb.linearDamping = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Setup collider
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<CircleCollider2D>();
        }
        col.radius = 0.3f;
        
        // Set layer
        gameObject.layer = LayerMask.NameToLayer("Player");
    }
    
    void Start()
    {
        mainCamera = Camera.main;
        targetPosition = transform.position;
        currentHealth = maxHealth;
    }
    
    void Update()
    {
        HandleInput();
        UpdateGrassHealing();
    }
    
    void FixedUpdate()
    {
        MoveTowardsTarget();
    }
    
    void HandleInput()
    {
        // Get mouse position in world space// Left mouse button held
        if (Input.GetMouseButton(0))
        {
            Vector3 pos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 newTarget = new Vector2(pos.x, pos.y);

            if (Vector2.Distance(newTarget, targetPosition) > 0.05f)
                targetPosition = newTarget;
        }

    }
    
    void MoveTowardsTarget()
    {
        Vector2 currentPos = transform.position;
        float distance = Vector2.Distance(currentPos, targetPosition);
        
        if (distance < 0.1f)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Calculate speed with grass bonus
        currentMoveSpeed = isOnGrass ? moveSpeed * grassSpeedMultiplier : moveSpeed;
        
        if (distance > stoppingDistance)
        {
            Vector2 direction = (targetPosition - currentPos).normalized;
            rb.linearVelocity = direction * currentMoveSpeed;
        }
        else
        {
            // Slow down when close to target
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 5f);
        }
    }
    
    void UpdateGrassHealing()
    {
        if (isOnGrass && currentHealth < maxHealth)
        {
            Heal(grassHealPerSecond * Time.deltaTime);
        }
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }
    
    void Die()
    {
        Debug.Log("Player died!");
        // Game over logic here
    }
    
    public void SetOnGrass(bool onGrass)
    {
        isOnGrass = onGrass;
    }
    
    public Vector2 GetPosition()
    {
        return transform.position;
    }
    
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }
    
    void OnDrawGizmos()
    {
        // Draw target position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPosition, 0.3f);
        
        // Draw line from player to target
        if (Application.isPlaying)
        {
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}
