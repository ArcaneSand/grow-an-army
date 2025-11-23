using UnityEngine;

/// <summary>
/// Represents a single cell in the grid
/// Can contain grass and/or a plant turret
/// </summary>
public class GridCell
{
    public Vector2Int gridPosition;
    public Vector2 worldPosition;
    
    public bool hasGrass = false;
    public bool hasPlant = false;
    
    public GrassTile grassTile;
    public PlantTurretBase plantTurret;
    
    public GridCell(Vector2Int gridPos, Vector2 worldPos)
    {
        gridPosition = gridPos;
        worldPosition = worldPos;
    }
    
    public bool CanPlacePlant()
    {
        return hasGrass && !hasPlant;
    }
    
    public void PlaceGrass(GrassTile grass)
    {
        hasGrass = true;
        grassTile = grass;
    }
    
    public void PlacePlant(PlantTurretBase plant)
    {
        if (!hasGrass)
        {
            Debug.LogWarning("Trying to place plant on cell without grass!");
            return;
        }
        
        hasPlant = true;
        plantTurret = plant;
    }
    
    public void RemovePlant()
    {
        hasPlant = false;
        plantTurret = null;
    }
    
    public void RemoveGrass()
    {
        hasGrass = false;
        grassTile = null;
        
        // Also remove plant if grass is removed
        if (hasPlant)
        {
            RemovePlant();
        }
    }
    
    public void Clear()
    {
        hasGrass = false;
        hasPlant = false;
        grassTile = null;
        plantTurret = null;
    }
}
