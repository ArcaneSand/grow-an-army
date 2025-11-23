using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base class for all plant turrets
/// Handles growth through 3 stages, combat, and uproot conversion
/// Plants are stationary and grid-based
/// Auto-uproots after being fully grown for 3 seconds
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlantTurretBase : MonoBehaviour
{
    [Header("Plant Setup")]
    public PlantDataSO plantData;
    
    [Header("Visual Components")]
    public SpriteRenderer spriteRenderer;
    public Transform visualTransform;
    public GameObject plantUnit;
    
    [Header("Current State")]
    [SerializeField] protected int currentStage = 0; // 0=Sprout, 1=Bud, 2=Rooted
    [SerializeField] protected float growthTimer = 0f;
    
    [Header("Auto-Uproot Settings")]
    [SerializeField] private float autoUprootDelay = 3f; // Time after fully grown before auto-uproot
    private float fullyGrownTimer = 0f;
    
    // Combat
    protected List<UnitBase> enemiesInRange = new List<UnitBase>();
    protected UnitBase currentTarget;
    protected float attackTimer = 0f;
    
    // Grid reference
    protected GridCell myGridCell;
    protected Vector2Int gridPosition;
    
    // Components
    protected CircleCollider2D detectionCollider;
    protected TurretStageStats currentStats;
    
    protected bool isFullyGrown = false;
    
    #region Initialization
    
    protected virtual void Awake()
    {
        InitializeComponents();
    }
    
    protected virtual void Start()
    {
        InitializePlant();
    }
    
    protected virtual void InitializeComponents()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Setup detection collider
        detectionCollider = gameObject.AddComponent<CircleCollider2D>();
        detectionCollider.isTrigger = true;
        
        // Make sure visual transform exists
        if (visualTransform == null)
        {
            visualTransform = transform;
        }
    }
    
    protected virtual void InitializePlant()
    {
        if (plantData == null)
        {
            Debug.LogError($"Plant {gameObject.name} is missing PlantDataSO!");
            return;
        }
        
        // Set to stage 0
        SetStage(0);
        
        // Register with grid
        RegisterWithGrid();
    }
    
    protected void RegisterWithGrid()
    {
        if (GridManager.Instance != null)
        {
            myGridCell = GridManager.Instance.GetCellAtWorldPos(transform.position);
            if (myGridCell != null)
            {
                myGridCell.PlacePlant(this);
                gridPosition = myGridCell.gridPosition;
            }
        }
    }
    
    #endregion
    
    #region Growth System
    
    protected virtual void Update()
    {
        UpdateGrowth();
        UpdateCombat();
    }
    
    protected virtual void UpdateGrowth()
    {
        // Check for auto-uproot when fully grown
        if (isFullyGrown)
        {
            AutoUprootCountdown();
            return; // Don't update growth timer anymore
        }
        
        // Update growth timer
        growthTimer += Time.deltaTime;
        
        float requiredTime = plantData.GetGrowthTimeForStage(currentStage);
        
        if (growthTimer >= requiredTime)
        {
            GrowToNextStage();
        }
    }

    /// <summary>
    /// Counts down 3 seconds after fully grown, then auto-uproots
    /// </summary>
    private void AutoUprootCountdown()
    {
        fullyGrownTimer += Time.deltaTime;
        
        if (fullyGrownTimer >= autoUprootDelay)
        {
            Debug.Log($"{plantData.plantName} auto-uprooting after {autoUprootDelay} seconds at max stage!");
            Uproot();
        }
    }

    protected virtual void GrowToNextStage()
    {
        if (currentStage < 2)
        {
            currentStage++;
            growthTimer = 0f;
            SetStage(currentStage);
            
            OnStageChanged();
            
            if (currentStage == 2)
            {
                isFullyGrown = true;
                fullyGrownTimer = 0f; // Reset countdown timer
                OnFullyGrown();
            }
        }
    }
    
    protected virtual void SetStage(int stage)
    {
        currentStage = Mathf.Clamp(stage, 0, 2);
        currentStats = plantData.GetStatsForStage(currentStage);
        
        // Update visual
        UpdateVisual();
        
        // Update detection range
        if (detectionCollider != null)
        {
            detectionCollider.radius = currentStats.attackRange;
        }
    }
    
    protected virtual void UpdateVisual()
    {
        if (spriteRenderer == null || plantData == null) return;
        
        // Set sprite for current stage
        if (plantData.growthStageSprites != null && currentStage < plantData.growthStageSprites.Length)
        {
            spriteRenderer.sprite = plantData.growthStageSprites[currentStage];
        }
        
        // Set color
        spriteRenderer.color = plantData.plantColor;
        
        // Set size
        if (visualTransform != null)
        {
            visualTransform.localScale = Vector3.one * plantData.plantSize;
        }
    }
    
    protected virtual void OnStageChanged()
    {
        Debug.Log($"{plantData.plantName} grew to stage {currentStage}");
    }
    
    protected virtual void OnFullyGrown()
    {
        Debug.Log($"{plantData.plantName} is fully grown! Will auto-uproot in {autoUprootDelay} seconds...");
    }
    
    #endregion
    
    #region Combat System
    
    protected virtual void UpdateCombat()
    {
        UpdateAttackTimer();
        
        // Find target if we don't have one
        if (currentTarget == null || currentTarget.IsDead())
        {
            FindNearestEnemy();
        }
        
        // Attack if we have a target and can attack
        if (currentTarget != null && CanAttack())
        {
            PerformAttack(currentTarget);
        }
    }
    
    protected virtual void FindNearestEnemy()
    {
        currentTarget = null;
        float closestDistance = float.MaxValue;
        
        // Clean up dead enemies
        enemiesInRange.RemoveAll(e => e == null || e.IsDead());
        
        foreach (UnitBase enemy in enemiesInRange)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance && distance <= currentStats.attackRange)
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
        
        // Deal damage
        target.TakeDamage(currentStats.attackDamage, null);
        
        // Reset attack timer
        float cooldown = 1f / currentStats.attackSpeed; // Convert attacks/sec to cooldown
        attackTimer = cooldown;
        
        // Visual/audio feedback
        OnAttackPerformed(target);
    }
    
    protected virtual void OnAttackPerformed(UnitBase target)
    {
        // Override in child classes for specific attack effects
        Debug.Log($"{plantData.plantName} (Stage {currentStage}) attacks {target.unitData.unitName} for {currentStats.attackDamage} damage!");
    }
    
    #endregion
    
    #region Uproot System
    
    /// <summary>
    /// Uproot this plant and spawn ally troops based on current stage
    /// </summary>
    public virtual void Uproot()
    {
        int troopCount = GetTroopCountForCurrentStage();
        
        Debug.Log($"Uprooting {plantData.plantName} at stage {currentStage} - spawning {troopCount} troops");
        
        // Spawn ally troops
        SpawnAllyTroops(troopCount);
        
        // Update grid before destroying
        OnUprooted();
        
        // Destroy this plant
        Destroy(gameObject);
    }
    
    protected virtual void SpawnAllyTroops(int count)
    {
        if (plantData.allyUnitToSpawn == null)
        {
            Debug.LogWarning($"Cannot spawn troops - {plantData.plantName} has no allyUnitToSpawn set!");
            return;
        }
        
        for (int i = 0; i < count; i++)
        {
            // Spawn in circle around plant position
            float angle = (360f / count) * i * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1f;
            Vector2 spawnPos = (Vector2)transform.position + offset;
            
            // Create ally unit
            Instantiate(plantUnit, spawnPos, Quaternion.identity);
        }
    }
    
    protected virtual void OnUprooted()
    {
        // Update grid
        if (myGridCell != null)
        {
            myGridCell.RemovePlant();
        }
    }
    
    public int GetTroopCountForCurrentStage()
    {
        if (plantData.troopsSpawnedPerStage != null && currentStage < plantData.troopsSpawnedPerStage.Length)
        {
            return plantData.troopsSpawnedPerStage[currentStage];
        }
        return 1;
    }
    
    #endregion
    
    #region Collision Detection
    
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // Detect enemy units
        UnitBase unit = other.GetComponent<UnitBase>();
        
        if (unit != null && unit.team == UnitTeam.Enemy && !unit.IsDead())
        {
            if (!enemiesInRange.Contains(unit))
            {
                enemiesInRange.Add(unit);
            }
        }
    }
    
    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        UnitBase unit = other.GetComponent<UnitBase>();
        
        if (unit != null)
        {
            enemiesInRange.Remove(unit);
        }
    }
    
    #endregion
    
    #region Getters
    
    public int GetCurrentStage() => currentStage;
    public float GetGrowthProgress()
    {
        if (isFullyGrown) return 1f;
        float requiredTime = plantData.GetGrowthTimeForStage(currentStage);
        return requiredTime > 0 ? growthTimer / requiredTime : 1f;
    }
    public bool IsFullyGrown() => isFullyGrown;
    
    /// <summary>
    /// Get remaining time before auto-uproot (0 if not fully grown)
    /// </summary>
    public float GetAutoUprootTimeRemaining()
    {
        if (!isFullyGrown) return 0f;
        return Mathf.Max(0f, autoUprootDelay - fullyGrownTimer);
    }
    
    #endregion
    
    #region Debug
    
    protected virtual void OnDrawGizmosSelected()
    {
        if (currentStats == null && plantData != null)
        {
            currentStats = plantData.GetStatsForStage(currentStage);
        }
        
        if (currentStats != null)
        {
            // Draw attack range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, currentStats.attackRange);
            
            // Draw line to target
            if (currentTarget != null && Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            }
        }
    }
    
    #endregion
}