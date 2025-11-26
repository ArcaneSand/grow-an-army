using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using System.Collections;

/// <summary>
/// Base class for all units (player troops, enemies)
/// Handles movement, combat, health, and detection
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public abstract class UnitBase : MonoBehaviour
{
    [Header("Unit Setup")]
    public UnitDataSO unitData;
    public UnitTeam team;
    
    [Header("Visual Components")]
    public SpriteRenderer spriteRenderer;
    public Transform visualTransform;
    public UnitHandler unitHandler;
    
    [Header("Combat System")]
    public AttackBehavior attackBehavior;

    // Current stats
    [SerializeField] protected float currentHealth;
    protected float attackTimer;
    protected bool isDead = false;
    protected bool isTurnRight = true;
    private bool isWalking = false;
    private float walkStateChangeDelay = 0.2f; 
    private float walkStateTimer = 0f;
    float stopRadius = 0.1f;
    float startRadius = 0.2f;
    // Components
    protected Rigidbody2D rb;
    protected CircleCollider2D detectionCollider;
    protected CircleCollider2D physicsCollider;
    
    // Combat
    protected UnitBase currentTarget;
    protected List<UnitBase> enemiesInRange = new List<UnitBase>();
    
    // Movement
    protected Vector2 moveDirection;
    protected Vector2 targetPosition;

    [Header("Visual Feedback")]
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;
    private Color originalColor; // Store once at start
    private Coroutine currentFlashCoroutine; // Track active coroutine

    #region Unity Lifecycle
    
    protected virtual void Awake()
    {
        InitializeComponents();
    }
    
    protected virtual void Start()
    {
        InitializeUnit();
        InitializeAttackBehavior();
    }
    
    protected virtual void Update()
    {
        if (isDead) return;
        
        UpdateAttackTimer();
        UpdateCombat();
        UpdateMovement();
    }
     
    protected virtual void FixedUpdate()
    {
        if (isDead) return;
        
        ApplyMovement();
    }
    
    #endregion
    
    #region Initialization
    
    protected virtual void InitializeComponents()
    {
        // Get or add Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 10f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Setup sprite renderer
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        // Setup colliders
        SetupColliders();
    }
    
    protected virtual void SetupColliders()
    {
        // Physics collider (for blocking movement)
        CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();
        
        if (colliders.Length == 0)
        {
            physicsCollider = gameObject.AddComponent<CircleCollider2D>();
            detectionCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        else if (colliders.Length == 1)
        {
            physicsCollider = colliders[0];
            detectionCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        else
        {
            physicsCollider = colliders[0];
            detectionCollider = colliders[1];
        }
        
        // Physics collider - small, for pushing
        physicsCollider.radius = 0.3f;
        physicsCollider.isTrigger = false;
        
        // Detection collider - large, for finding enemies
        detectionCollider.radius = 1f; // Will be set from unitData
        detectionCollider.isTrigger = true;
    }
    
    protected virtual void InitializeUnit()
    {
        if (unitData == null)
        {
            Debug.LogError($"Unit {gameObject.name} is missing UnitDataSO!");
            return;
        }
        
        // Set stats

        currentHealth = unitData.maxHealth;
        team = GetTeam();
        attackTimer = 0f;
        
        
        // Set visual
        if (spriteRenderer != null)
        {
            if (unitData.unitSprite != null)
            {
                spriteRenderer.sprite = unitData.unitSprite;
            }
            
            // Color based on team (overrides unitData.unitColor)
            spriteRenderer.color = GetTeamColor();
            originalColor = spriteRenderer.color;
        }
        
        // Scale visual
        if (visualTransform != null)
        {
            visualTransform.localScale = Vector3.one * unitData.unitSize;
        }
        
        // Set detection radius
        if (detectionCollider != null)
        {
            detectionCollider.radius = unitData.detectionRadius;
        }
        
        // Set layer based on team
        SetupTeamLayer();
    }
    
     protected virtual void InitializeAttackBehavior()
    {
        if (attackBehavior == null)
        {
            attackBehavior = GetComponent<AttackBehavior>();
        }
        
        if (attackBehavior != null && unitData != null)
        {
            attackBehavior.Initialize(unitData);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has no AttackBehavior component! Combat will not work.");
        }
    }

    protected virtual Color GetTeamColor()
    {

        // Automatically color units based on team
        switch (team)
        {
            case UnitTeam.Player:
                return new Color(0.2f, 1f, 0.2f); // Bright green for player
            case UnitTeam.Enemy:
                return Color.white;
                //return new Color(1f, 0.2f, 0.2f); // Red for enemies
            default:
                return Color.white;
        }
    }
    
    protected virtual UnitTeam GetTeam()
    {
        return team;
    }
    protected virtual void SetupTeamLayer()
    {
        // Set layer so units can detect opposite team
        if (team == UnitTeam.Player)
        {
            gameObject.layer = LayerMask.NameToLayer("PlayerUnit");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("EnemyUnit");
        }
    }
    
    #endregion
    
    #region Movement
    
    protected abstract void UpdateMovement();
    
protected virtual void ApplyMovement()
{
    bool isAttemptingToMove = moveDirection.sqrMagnitude > 0.01f;
    float modSpeed = GetTeam() == UnitTeam.Enemy ? unitData.moveSpeed * 0.6f : unitData.moveSpeed;
    if (isAttemptingToMove)
        rb.linearVelocity = moveDirection * modSpeed;
    else
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.fixedDeltaTime * 2f);

    UpsateWalkingState();
    UpdateFacingByMovement();
}  

private void UpsateWalkingState()
{
    float velocityMagnitude = rb.linearVelocity.sqrMagnitude;
    float walkingThreshold = 1f;  // ~1 unit/sec velocity = walking
    float stoppedThreshold = 0.25f;


    walkStateTimer += Time.fixedDeltaTime;

    if (walkStateTimer <= 0f)
    {
        if (!isWalking && velocityMagnitude > walkingThreshold)
        {
            isWalking = true;
            walkStateTimer = walkStateChangeDelay;
        }
        // Currently walking, check if should stop
        else if (isWalking && velocityMagnitude < stoppedThreshold)
        {
            isWalking = false;
            walkStateTimer = walkStateChangeDelay;
        }
        
    }
    unitHandler?.Walk(isWalking);
}
    
protected virtual void MoveTowards(Vector2 target)
{
    Vector2 currentPos = transform.position;
    float distance = Vector2.Distance(currentPos, target);

    // Only stop if we are REALLY close
    if (distance <= stopRadius)
    {
        moveDirection = Vector2.zero;
        return;
    }

    // Only move if sufficiently far
    if (distance >= startRadius)
    {
        moveDirection = (target - currentPos).normalized;
        return;
    }
}

    protected void UpdateFacingByMovement()
{
    if (moveDirection.x > 0.1f)
        visualTransform.localScale = new Vector3(1, 1, 1);
    else if (moveDirection.x < -0.1f)
        visualTransform.localScale = new Vector3(-1, 1, 1);
}
    
    #endregion
    
    #region Combat
    
    protected virtual void UpdateCombat()
    {
        // Find target if we don't have one
        if (currentTarget == null || currentTarget.IsDead())
        {
            FindNearestEnemy();
        }
        if (currentTarget != null && !currentTarget.IsDead())
    {
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);
        
        // If target moved outside detection range, forget it
        if (distanceToTarget > unitData.detectionRadius)
        {
            currentTarget = null;
            FindNearestEnemy(); // Try to find a closer enemy
        }
    }
        // Attack if in range
        if (currentTarget != null && CanAttack())
        {
            float distanceToTarget = Vector2.Distance(transform.position, currentTarget.transform.position);
            
            if (distanceToTarget <= unitData.attackRange)
            {
                PerformAttack(currentTarget);
            }
        }
        UpdateFacingByTarget();
    }
    protected void UpdateFacingByTarget()
    {
        if (currentTarget == null) return;

        float dx = currentTarget.transform.position.x - transform.position.x;

        if (dx > 0.1f)
            visualTransform.localScale = new Vector3(1, 1, 1);
        else if (dx < -0.1f)
            visualTransform.localScale = new Vector3(-1, 1, 1);
    }
    protected virtual void FindNearestEnemy()
    {
        currentTarget = null;
        float closestDistance = float.MaxValue;
        
        // Clean up dead enemies from list
        enemiesInRange.RemoveAll(e => e == null || e.IsDead());
        
        foreach (UnitBase enemy in enemiesInRange)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentTarget = enemy;
            }
        }
    }
    
    protected virtual bool CanAttack()
    {
        return attackTimer <= 0f;
    }
    
    protected virtual void UpdateAttackTimer()
    {
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }
    }
    
    protected virtual void PerformAttack(UnitBase target)
    {
        if (target == null || target.IsDead()) return;
        if (attackBehavior == null) return;
        
        // Use attack behavior to execute attack
        attackBehavior.Execute(target, this);
        
        // Reset attack timer
        attackTimer = unitData.attackCooldown;
        
        // Visual feedback (handled by AttackBehavior now)
        OnAttackPerformed(target);
    }
    
    
    protected virtual void OnAttackPerformed(UnitBase target)
    {
        unitHandler?.Attack();
        // Override in child classes for attack animations/effects
        Debug.Log($"{unitData.unitName} attacks {target.unitData.unitName} for {unitData.attackDamage} damage!");
    }
    

    #endregion
    
    #region Health & Damage
    public virtual void SetHealthPercent(float percent)
    {
        currentHealth = unitData.maxHealth * Mathf.Clamp01(percent);
        //Debug.Log($"{unitData.unitName} health set to {currentHealth}/{unitData.maxHealth} ({percent * 100f}%)");
    }
    public virtual void TakeDamage(float damage, UnitBase attacker)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
        OnDamageTaken(damage, attacker);
        
        if (currentHealth <= 0f)
        {
            Die(attacker);
        }
    }
    
    protected virtual void OnDamageTaken(float damage, UnitBase attacker)
    {
        // Visual feedback - flash red or show damage number
        if (spriteRenderer != null)
        {
            FlashDamage();
        }
    }
    
    protected virtual void Die(UnitBase killer)
    {
        if (isDead) return;
        
        isDead = true;
        
        OnDeath(killer);
        
        // Destroy after a short delay
        Destroy(gameObject, 0.1f);
    }
    
    protected virtual void OnDeath(UnitBase killer)
    {
        // Override in child classes for death effects
        Debug.Log($"{unitData.unitName} died!");
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    public float GetHealthPercent()
    {
        return currentHealth / unitData.maxHealth;
    }
    
    #endregion
    
    #region Collision Detection
    
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // Detect enemies entering range
        UnitBase otherUnit = other.GetComponent<UnitBase>();
        
        if (otherUnit != null && otherUnit.team != this.team && !otherUnit.IsDead())
        {
            if (!enemiesInRange.Contains(otherUnit))
            {
                enemiesInRange.Add(otherUnit);
            }
        }
    }
    
    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        // Remove enemies that left range
        UnitBase otherUnit = other.GetComponent<UnitBase>();
        
        if (otherUnit != null)
        {
            enemiesInRange.Remove(otherUnit);
        }
    }
    
    #endregion
    
    #region Visual Effects
    
    protected void FlashDamage()
    {
        if (spriteRenderer == null) return;
        
        // CRITICAL: Stop any existing flash coroutine
        if (currentFlashCoroutine != null)
        {
            StopCoroutine(currentFlashCoroutine);
        }
        
        // Start new flash
        currentFlashCoroutine = StartCoroutine(FlashRed());
    }

    protected IEnumerator FlashRed()
    {
        // Flash red
        spriteRenderer.color = damageFlashColor;
        
        // Wait
        yield return new WaitForSeconds(flashDuration);
        
        // Restore to original (stored at start, not now!)
        spriteRenderer.color = originalColor;
        
        // Clear reference
        currentFlashCoroutine = null;
    }

    
    #endregion
    
    #region Debug
    
    protected virtual void OnDrawGizmosSelected()
    {
        if (unitData == null) return;
        
        // Draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, unitData.detectionRadius);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, unitData.attackRange);
        
        // Draw line to current target
        if (currentTarget != null && Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }
    
    #endregion
}

