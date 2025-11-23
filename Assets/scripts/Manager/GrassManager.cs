using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Modified Grass Manager - Spawns grass + plant together on single cell
/// Uses chance-based spawning and nearest-cell search
/// </summary>
public class GrassManager : MonoBehaviour
{
    //To do add spawn chance base on unit
    public static GrassManager Instance { get; private set; }
    
    [Header("Prefabs")]
    [SerializeField] private GameObject grassTilePrefab;
    
    [Header("Plant Prefabs - Map PlantType to Prefabs")]
    [SerializeField] private PlantPrefabMapping[] plantPrefabs;
    
    [Header("References")]
    [SerializeField] private Transform grassContainer;
    [SerializeField] private Transform plantContainer;
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnChance = 0.8f; // 80% chance to spawn
    [SerializeField] private int maxSearchRadius = 10; // Max distance to search for empty cell
    
    private Dictionary<PlantType, GameObject> plantPrefabLookup;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        InitializeContainers();
        BuildPlantPrefabLookup();
    }
    
    void InitializeContainers()
    {
        if (grassContainer == null)
        {
            GameObject container = new GameObject("GrassContainer");
            container.transform.SetParent(transform);
            grassContainer = container.transform;
        }
        
        if (plantContainer == null)
        {
            GameObject container = new GameObject("PlantContainer");
            container.transform.SetParent(transform);
            plantContainer = container.transform;
        }
    }
    
    void BuildPlantPrefabLookup()
    {
        plantPrefabLookup = new Dictionary<PlantType, GameObject>();
        
        if (plantPrefabs != null)
        {
            foreach (var mapping in plantPrefabs)
            {
                if (mapping.prefab != null)
                {
                    plantPrefabLookup[mapping.plantType] = mapping.prefab;
                }
            }
        }
    }
    
    /// <summary>
    /// Main function called when an enemy dies
    /// Rolls chance, finds nearest empty cell, spawns grass + plant together
    /// </summary>
    public void TrySpawnGrassAndPlant(Vector2 deathPosition, PlantType plantType)
    {
        // Roll the spawn chance
        float roll = Random.Range(0f, 1f);
        if (roll > spawnChance)
        {
            Debug.Log($"Spawn chance failed ({roll:F2} > {spawnChance:F2}). No grass spawned.");
            return;
        }
        
        // Find nearest empty cell
        GridCell targetCell = FindNearestEmptyCell(deathPosition);
        
        if (targetCell == null)
        {
            Debug.LogWarning($"Could not find empty cell near {deathPosition} within radius {maxSearchRadius}");
            return;
        }
        
        // Spawn grass + plant together on the same cell
        SpawnGrassTile(targetCell);
        SpawnPlantTurret(targetCell, plantType);
        
        Debug.Log($"Spawned {plantType} at grid {targetCell.gridPosition} (world: {targetCell.worldPosition})");
    }
    
    /// <summary>
    /// Finds the nearest grid cell that doesn't have grass
    /// Uses Breadth-First Search for true "nearest" distance
    /// </summary>
    GridCell FindNearestEmptyCell(Vector2 worldPosition)
    {
        if (GridManager.Instance == null) return null;
        
        // Get the starting grid cell
        GridCell startCell = GridManager.Instance.GetCellAtWorldPos(worldPosition);
        if (startCell == null) return null;
        
        // If the starting cell is empty, use it immediately
        if (!startCell.hasGrass)
        {
            return startCell;
        }
        
        // BFS to find nearest empty cell
        Queue<GridCell> queue = new Queue<GridCell>();
        HashSet<GridCell> visited = new HashSet<GridCell>();
        
        queue.Enqueue(startCell);
        visited.Add(startCell);
        
        // Get grid data for this position
        GridData gridData = GridManager.Instance.GetGridAtWorldPos(worldPosition);
        if (gridData == null) return null;
        
        int searchSteps = 0;
        int maxSteps = maxSearchRadius * maxSearchRadius * 4; // Approximate max cells to check
        
        while (queue.Count > 0 && searchSteps < maxSteps)
        {
            searchSteps++;
            GridCell current = queue.Dequeue();
            
            // Check if this cell is empty
            if (!current.hasGrass)
            {
                return current;
            }
            
            // Add neighbors to queue (4-directional: up, down, left, right)
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // Up
                new Vector2Int(0, -1),  // Down
                new Vector2Int(-1, 0),  // Left
                new Vector2Int(1, 0)    // Right
            };
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = current.gridPosition + dir;
                GridCell neighbor = GridManager.Instance.GetCell(neighborPos, gridData);
                
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        
        Debug.LogWarning($"BFS exhausted after {searchSteps} steps. No empty cell found.");
        return null;
    }
    
    /// <summary>
    /// Spawns a grass tile on the given cell
    /// </summary>
    void SpawnGrassTile(GridCell cell)
    {
        GameObject grassObj;
        
        if (grassTilePrefab != null)
        {
            grassObj = Instantiate(grassTilePrefab, cell.worldPosition, Quaternion.identity, grassContainer);
        }
        else
        {
            // Fallback: Create grass tile manually
            grassObj = new GameObject($"Grass_{cell.gridPosition.x}_{cell.gridPosition.y}");
            grassObj.transform.position = cell.worldPosition;
            grassObj.transform.SetParent(grassContainer);
            
            // Add sprite renderer
            SpriteRenderer sr = grassObj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
            sr.sortingOrder = -1;
            
            // Create a simple square sprite
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            sr.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
            
            // Scale to fit grid cell
            float cellSize = GridManager.Instance.GetCellSize();
            grassObj.transform.localScale = Vector3.one * cellSize;
            
            // Add trigger collider for player detection
            BoxCollider2D col = grassObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;
            
            // Add grass tile component
            grassObj.AddComponent<GrassTile>();
        }
        
        // Get or add grass tile component
        GrassTile tile = grassObj.GetComponent<GrassTile>();
        if (tile == null)
        {
            tile = grassObj.AddComponent<GrassTile>();
        }
        
        tile.Initialize(cell);
        cell.PlaceGrass(tile);
    }
    
    /// <summary>
    /// Spawns a plant turret on the given cell
    /// </summary>
    void SpawnPlantTurret(GridCell cell, PlantType plantType)
    {
        if (!cell.hasGrass)
        {
            Debug.LogWarning("Trying to spawn plant on cell without grass!");
            return;
        }
        
        // Get the appropriate prefab
        GameObject prefab = GetPlantPrefab(plantType);
        
        if (prefab == null)
        {
            Debug.LogWarning($"No prefab found for PlantType {plantType}. Cannot spawn plant turret.");
            return;
        }
        
        // Instantiate the plant
        GameObject plantObj = Instantiate(prefab, cell.worldPosition, Quaternion.identity, plantContainer);
        
        // Register with grid
        PlantTurretBase plantComponent = plantObj.GetComponent<PlantTurretBase>();
        if (plantComponent != null)
        {
            // Plant will register itself in its Start() method
            cell.PlacePlant(plantComponent);
        }
    }
    
    GameObject GetPlantPrefab(PlantType plantType)
    {
        if (plantPrefabLookup != null && plantPrefabLookup.ContainsKey(plantType))
        {
            return plantPrefabLookup[plantType];
        }
        return null;
    }
    
    /// <summary>
    /// Clear all grass and plants (useful for room transitions)
    /// </summary>
    public void ClearAllGrass()
    {
        if (grassContainer != null)
        {
            foreach (Transform child in grassContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    public void ClearAllPlants()
    {
        if (plantContainer != null)
        {
            foreach (Transform child in plantContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    /// <summary>
    /// Set the spawn chance (for difficulty tuning)
    /// </summary>
    public void SetSpawnChance(float chance)
    {
        spawnChance = Mathf.Clamp01(chance);
    }
}

/// <summary>
/// Helper class to map PlantType to prefabs in inspector
/// </summary>
[System.Serializable]
public class PlantPrefabMapping
{
    public PlantType plantType;
    public GameObject prefab;
}

/// <summary>
/// Enum for different plant types
/// </summary>
