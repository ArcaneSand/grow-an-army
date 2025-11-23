using UnityEngine;

/// <summary>
/// ScriptableObject that holds all stats and data for a unit type
/// Used by both enemies and allies
/// </summary>
[CreateAssetMenu(fileName = "NewUnitData", menuName = "Plant Game/Unit Data")]
public class UnitDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string unitName = "Villager";
    public UnitType unitType = UnitType.Villager;
    
    [Header("Visual")]
    public Sprite unitSprite;
    public Color unitColor = Color.white;
    public float unitSize = 1f;
    
    [Header("Movement")]
    [Tooltip("How fast the unit moves")]
    public float moveSpeed = 3f;
    [Tooltip("How close unit gets to target before stopping")]
    public float stoppingDistance = 0.5f;
    
    [Header("Combat Stats")]
    public float maxHealth = 50f;
    public float attackDamage = 10f;
    public float attackRange = 1.5f;
    [Tooltip("Time between attacks in seconds")]
    public float attackCooldown = 1f;
    
    [Header("Detection")]
    [Tooltip("Radius to detect enemies")]
    public float detectionRadius = 3f;
    
    [Header("Plant Conversion (for enemies only)")]
    [Tooltip("How many grass tiles spawn when this enemy dies")]
    public int grassSpawnRadius = 1;
    public PlantType plantToSpawn = PlantType.Grass;
}

/// <summary>
/// Types of units in the game
/// </summary>
public enum UnitType
{
    Villager,
    Knight,
    Archer,
    Mage,
    Giant,
    Dragon
}

/// <summary>
/// Types of plants/turrets
/// </summary>
public enum PlantType
{
    Grass,
    Rose,
    Flower,
    Sunflower,
    Log,
    DragonFlower
}

/// <summary>
/// Which team the unit belongs to
/// </summary>
public enum UnitTeam
{
    Player,
    Enemy
}
