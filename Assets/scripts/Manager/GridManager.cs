using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton that manages the grid system for plant placement
/// Handles grid-to-world coordinate conversion
/// Modified to support nearest-cell search for grass spawning
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    public List<GridData> grids = new List<GridData>();
    
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 50;
    [SerializeField] private int gridHeight = 50;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector2[] gridOrigins;
    
    [Header("Debug")]
    [SerializeField] private bool showGridGizmos = false;
    [SerializeField] private bool showOccupiedCells = true;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        InitializeGrids();
    }
    
    void InitializeGrids()
    {
        if (gridOrigins == null || gridOrigins.Length == 0)
        {
            Debug.LogWarning("No grid origins defined! Creating default grid at (0,0)");
            gridOrigins = new Vector2[] { Vector2.zero };
        }
        
        foreach (Vector2 origin in gridOrigins)
        {
            GridData data = new GridData();
            data.origin = origin;
            data.width = gridWidth;
            data.height = gridHeight;
            data.cells = new GridCell[gridWidth, gridHeight];

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    Vector2 worldPos = GridToWorld(gridPos, data);

                    data.cells[x, y] = new GridCell(gridPos, worldPos);
                }
            }

            grids.Add(data);
        }

        Debug.Log($"Initialized {grids.Count} grids.");
    }
    
    #region Coordinate Conversion
    
    public GridData GetGridAtWorldPos(Vector2 worldPos)
    {
        foreach (GridData grid in grids)
        {
            if (worldPos.x >= grid.origin.x && worldPos.x < grid.origin.x + grid.width * cellSize &&
                worldPos.y >= grid.origin.y && worldPos.y < grid.origin.y + grid.height * cellSize)
            {
                return grid;
            }
        }
        return null;
    }

    public Vector2Int WorldToGrid(Vector2 worldPos, GridData data)
    {
        Vector2 local = worldPos - data.origin;

        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);

        return new Vector2Int(x, y);
    }

    public Vector2 GridToWorld(Vector2Int gridPos, GridData data)
    {
        return data.origin + new Vector2(
            gridPos.x * cellSize + cellSize * 0.5f,
            gridPos.y * cellSize + cellSize * 0.5f
        );
    }
    
    #endregion
    
    #region Grid Cell Access
    
    public bool IsValid(GridData data, Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < data.width &&
               pos.y >= 0 && pos.y < data.height;
    } 
    
    public GridCell GetCell(Vector2Int gridPos, GridData data)
    {
        if (!IsValid(data, gridPos)) return null;
        return data.cells[gridPos.x, gridPos.y];
    }

    public GridCell GetCellAtWorldPos(Vector2 worldPos)
    {
        GridData data = GetGridAtWorldPos(worldPos);
        if (data == null) return null;

        Vector2Int pos = WorldToGrid(worldPos, data);
        return GetCell(pos, data);
    }
    
    #endregion
    
    #region Area Queries
    
    /// <summary>
    /// Get all cells within a circular radius
    /// Used for area effects and queries
    /// </summary>
    public List<GridCell> GetCellsInRadius(Vector2 worldCenter, int radius)
    {
        List<GridCell> result = new List<GridCell>();

        GridData data = GetGridAtWorldPos(worldCenter);
        if (data == null) return result;

        Vector2Int center = WorldToGrid(worldCenter, data);

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy > radius * radius) continue;

                Vector2Int pos = center + new Vector2Int(dx, dy);

                GridCell cell = GetCell(pos, data);
                if (cell != null) result.Add(cell);
            }
        }

        return result;
    }

    /// <summary>
    /// Find nearest empty cell (no grass) using BFS
    /// This is the primary method used by GrassManager
    /// </summary>
    public GridCell FindNearestEmptyCell(Vector2 worldPosition, int maxRadius = 10)
    {
        GridCell startCell = GetCellAtWorldPos(worldPosition);
        if (startCell == null) return null;
        
        // If starting cell is empty, return it immediately
        if (!startCell.hasGrass)
        {
            return startCell;
        }
        
        // BFS to find nearest empty cell
        Queue<GridCell> queue = new Queue<GridCell>();
        HashSet<GridCell> visited = new HashSet<GridCell>();
        
        queue.Enqueue(startCell);
        visited.Add(startCell);
        
        GridData gridData = GetGridAtWorldPos(worldPosition);
        if (gridData == null) return null;
        
        int searchSteps = 0;
        int maxSteps = maxRadius * maxRadius * 4;
        
        while (queue.Count > 0 && searchSteps < maxSteps)
        {
            searchSteps++;
            GridCell current = queue.Dequeue();
            
            // Found an empty cell
            if (!current.hasGrass)
            {
                return current;
            }
            
            // Check 4 neighbors (up, down, left, right)
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
                GridCell neighbor = GetCell(neighborPos, gridData);
                
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        
        return null; // No empty cell found
    }
    
    /// <summary>
    /// Find nearest empty GRASS cell (has grass but no plant)
    /// Used for plant placement on existing grass
    /// </summary>
    public GridCell FindNearestEmptyGrassCell(Vector2 worldPosition, int maxRadius = 10)
    {
        GridCell startCell = GetCellAtWorldPos(worldPosition);
        if (startCell == null) return null;
        
        // If starting cell has grass and no plant, return it
        if (startCell.hasGrass && !startCell.hasPlant)
        {
            return startCell;
        }
        
        // BFS to find nearest grass cell without plant
        Queue<GridCell> queue = new Queue<GridCell>();
        HashSet<GridCell> visited = new HashSet<GridCell>();
        
        queue.Enqueue(startCell);
        visited.Add(startCell);
        
        GridData gridData = GetGridAtWorldPos(worldPosition);
        if (gridData == null) return null;
        
        int searchSteps = 0;
        int maxSteps = maxRadius * maxRadius * 4;
        
        while (queue.Count > 0 && searchSteps < maxSteps)
        {
            searchSteps++;
            GridCell current = queue.Dequeue();
            
            // Found a grass cell without plant
            if (current.hasGrass && !current.hasPlant)
            {
                return current;
            }
            
            // Check 4 neighbors
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0),
                new Vector2Int(1, 0)
            };
            
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = current.gridPosition + dir;
                GridCell neighbor = GetCell(neighborPos, gridData);
                
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        
        return null;
    }
    
    #endregion
    
    #region Debug Visualization
    
    void OnDrawGizmos()
    {
        if (!showGridGizmos || grids == null) return;

        foreach (GridData grid in grids)
        {
            // Draw grid lines
            Gizmos.color = new Color(1, 1, 1, 0.2f);
            
            for (int x = 0; x <= grid.width; x++)
            {
                Vector2 start = grid.origin + new Vector2(x * cellSize, 0);
                Vector2 end = grid.origin + new Vector2(x * cellSize, grid.height * cellSize);
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= grid.height; y++)
            {
                Vector2 start = grid.origin + new Vector2(0, y * cellSize);
                Vector2 end = grid.origin + new Vector2(grid.width * cellSize, y * cellSize);
                Gizmos.DrawLine(start, end);
            }
            
            // Draw occupied cells
            if (showOccupiedCells && Application.isPlaying)
            {
                for (int x = 0; x < grid.width; x++)
                {
                    for (int y = 0; y < grid.height; y++)
                    {
                        GridCell cell = grid.cells[x, y];
                        if (cell.hasGrass)
                        {
                            // Green for grass only
                            Gizmos.color = new Color(0, 1, 0, 0.3f);
                            if (cell.hasPlant)
                            {
                                // Yellow for grass + plant
                                Gizmos.color = new Color(1, 1, 0, 0.5f);
                            }
                            
                            Vector2 cellCenter = cell.worldPosition;
                            Gizmos.DrawCube(cellCenter, Vector3.one * cellSize * 0.8f);
                        }
                    }
                }
            }
        }
    }
    
    #endregion
    
    public float GetCellSize() => cellSize;
}

[System.Serializable]
public class GridData
{
    public Vector2 origin;
    public GridCell[,] cells;
    public int width;
    public int height;
}