using UnityEngine;

/// <summary>
/// Handles player input for uprooting ALL plants at once
/// Press uproot key (default: Right Click or U) to uproot all plants and spawn troops
/// </summary>
public class UprootHandler : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private KeyCode uprootKey = KeyCode.U; // Press U to uproot all
    [SerializeField] private bool allowRightClick = true; // Also allow right click
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject uprootIndicatorPrefab;
    
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        HandleUprootInput();
    }
    
    void HandleUprootInput()
    {
        bool uprootPressed = Input.GetKeyDown(uprootKey);
        
        if (allowRightClick)
        {
            uprootPressed |= Input.GetMouseButtonDown(1); // Right click
        }
        
        if (uprootPressed)
        {
            UprootAllPlants();
        }
    }
    
    /// <summary>
    /// Uproot ALL plants in the scene and spawn their troops
    /// </summary>
    void UprootAllPlants()
    {
        // Find all plants in the scene
        PlantTurretBase[] allPlants = FindObjectsOfType<PlantTurretBase>();
        
        if (allPlants.Length == 0)
        {
            Debug.Log("No plants to uproot!");
            return;
        }
        
        int plantsUprooted = 0;
        int troopsSpawned = 0;
        
        // Uproot each plant
        foreach (PlantTurretBase plant in allPlants)
        {
            if (plant != null)
            {
                int troopCount = plant.GetTroopCountForCurrentStage();
                troopsSpawned += troopCount;
                
                // Show visual effect
                ShowUprootEffect(plant.transform.position);
                
                // Uproot the plant (spawns troops and destroys plant)
                plant.Uproot();
                
                plantsUprooted++;
            }
        }
        
        Debug.Log($"Uprooted {plantsUprooted} plants and spawned {troopsSpawned} troops!");
    }
    
    void ShowUprootEffect(Vector2 position)
    {
        // Simple visual effect for uprooting
        if (uprootIndicatorPrefab != null)
        {
            GameObject effect = Instantiate(uprootIndicatorPrefab, position, Quaternion.identity);
            Destroy(effect, 1f);
        }
    }
    
    /// <summary>
    /// Update counts for UI display
    /// </summary>
    

}