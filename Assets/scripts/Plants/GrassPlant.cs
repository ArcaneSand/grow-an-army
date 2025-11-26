using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Grass Plant (Villager) - Heals allies in range
/// Passive healing aura that affects all allies
/// Grows through 3 stages, healing strength increases
/// </summary>
public class GrassPlant : PlantTurretBase
{
    [Header("Grass Plant - Healing")]
    [SerializeField] private float healPerSecond = 5f;
    [SerializeField] private float healRadius = 4f;
    [SerializeField] private bool showHealEffect = true;
    [SerializeField] private ParticleSystem healEffectPrefab;
    
    private List<AllyUnit> alliesInRange = new List<AllyUnit>();
    private float healEffectTimer = 0f;
    
    protected override void Update()
    {
        base.Update();
        UpdateHealing();
    }
    
    void UpdateHealing()
    {
        // Clean up dead/null units
        alliesInRange.RemoveAll(u => u == null || u.IsDead());
        
        if (alliesInRange.Count == 0) return;
        
        // Calculate heal amount based on stage
        float healAmount = healPerSecond * Time.deltaTime * currentStats.specialEffectStrength;
        
        // Heal all allies in range
        foreach (AllyUnit ally in alliesInRange)
        {
            if (ally != null && !ally.IsDead())
            {
                ally.Heal(healAmount);
            }
        }
        
        // Visual effect timer
        if (showHealEffect)
        {
            healEffectTimer -= Time.deltaTime;
            if (healEffectTimer <= 0f)
            {
                healEffectTimer = 0.5f;
                ShowHealEffect();
            }
        }
    }
    
    void ShowHealEffect()
    {
        // Spawn healing particles or visual effect
        // For now, just debug
        if (alliesInRange.Count > 0)
        {
            Debug.Log($"{plantData.plantName} healing {alliesInRange.Count} allies");
            if (healEffectPrefab != null)
            {
                ParticleSystem obj = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
                Destroy(obj.gameObject, 2f);
            }
        }
    }
    
    protected override void InitializeComponents()
    {
        base.InitializeComponents();
        
        // Set detection radius to heal radius
        if (detectionCollider != null)
        {
            detectionCollider.radius = healRadius;
        }
    }
    
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // Detect allies for healing (don't call base - we don't attack)
        AllyUnit ally = other.GetComponent<AllyUnit>();
        if (ally != null && !ally.IsDead())
        {
            if (!alliesInRange.Contains(ally))
            {
                alliesInRange.Add(ally);
                Debug.Log($"{plantData.plantName} now healing {ally.name}");
            }
        }
    }
    
    protected override void OnTriggerExit2D(Collider2D other)
    {
        AllyUnit ally = other.GetComponent<AllyUnit>();
        if (ally != null)
        {
            alliesInRange.Remove(ally);
        }
    }
    
    protected override void UpdateCombat()
    {
        // Grass plant doesn't attack - it only heals
        // Override to disable combat
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw heal radius
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, healRadius);
        
        // Draw lines to allies being healed
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            foreach (AllyUnit ally in alliesInRange)
            {
                if (ally != null)
                {
                    Gizmos.DrawLine(transform.position, ally.transform.position);
                }
            }
        }
    }
}