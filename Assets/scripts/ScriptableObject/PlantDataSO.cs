using UnityEngine;

/// <summary>
/// ScriptableObject that holds all stats and data for a plant turret type
/// Plants have 3 growth stages with different stats
/// </summary>
[CreateAssetMenu(fileName = "NewPlantData", menuName = "Plant Game/Plant Data")]
public class PlantDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string plantName = "Grass Plant";
    public PlantType plantType = PlantType.Grass;
    
    [Header("Visual")]
    public Sprite[] growthStageSprites = new Sprite[3]; // Sprout, Bud, Rooted
    public Color plantColor = Color.green;
    public float plantSize = 1f;
    
    [Header("Growth Timing")]
    [Tooltip("Time to grow from Stage 0 (Sprout) to Stage 1 (Bud)")]
    public float stage0To1Time = 5f;
    [Tooltip("Time to grow from Stage 1 (Bud) to Stage 2 (Rooted)")]
    public float stage1To2Time = 8f;
    
    [Header("Turret Stats - Stage 0 (Sprout)")]
    public TurretStageStats stage0Stats = new TurretStageStats
    {
        health = 30f,
        attackDamage = 5f,
        attackSpeed = 1.5f,
        attackRange = 2f,
        specialEffectStrength = 0.3f
    };
    
    [Header("Turret Stats - Stage 1 (Bud)")]
    public TurretStageStats stage1Stats = new TurretStageStats
    {
        health = 60f,
        attackDamage = 12f,
        attackSpeed = 1.2f,
        attackRange = 2.5f,
        specialEffectStrength = 0.6f
    };
    
    [Header("Turret Stats - Stage 2 (Rooted)")]
    public TurretStageStats stage2Stats = new TurretStageStats
    {
        health = 100f,
        attackDamage = 20f,
        attackSpeed = 1f,
        attackRange = 3f,
        specialEffectStrength = 1f
    };
    
    [Header("Uproot Conversion")]
    [Tooltip("How many troops spawn when uprooted at each stage")]
    public int[] troopsSpawnedPerStage = new int[3] { 3, 2, 1 }; // Early uproot = more troops
    
    [Tooltip("The ally unit prefab/data to spawn when uprooted")]
    public UnitDataSO allyUnitToSpawn;
    
    [Header("Special Abilities")]
    [Tooltip("Does this plant have a special passive effect?")]
    public bool hasPassiveEffect = false;
    
    [Tooltip("Type of special effect (slow, heal, spawn projectile, etc)")]
    public PlantSpecialEffect specialEffect = PlantSpecialEffect.None;
    
    public TurretStageStats GetStatsForStage(int stage)
    {
        switch (stage)
        {
            case 0: return stage0Stats;
            case 1: return stage1Stats;
            case 2: return stage2Stats;
            default: return stage2Stats;
        }
    }
    
    public float GetGrowthTimeForStage(int stage)
    {
        switch (stage)
        {
            case 0: return stage0To1Time;
            case 1: return stage1To2Time;
            default: return 0f;
        }
    }
}

/// <summary>
/// Stats for a single growth stage of a plant turret
/// </summary>
[System.Serializable]
public class TurretStageStats
{
    public float health = 50f;
    public float attackDamage = 10f;
    public float attackSpeed = 1f;  // Attacks per second
    public float attackRange = 2.5f;
    public float specialEffectStrength = 1f; // Multiplier for special effects
}

/// <summary>
/// Types of special effects plants can have
/// </summary>
public enum PlantSpecialEffect
{
    None,           // No special effect
    Slow,           // Slows enemies (Grass)
    HealAura,       // Heals nearby allies (Grass)
    Damage,         // Deals damage (Rose)
    Projectile,     // Shoots projectiles (Flower)
    HomingBee,      // Spawns homing bees (Sunflower)
    AOE,            // Area damage (Log)
    HighDPS         // High damage per second (Dragon Flower)
}
