using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game manager - tracks win/loss conditions
/// Handles game state and scene restart
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game State")]
    [SerializeField] private bool gameOver = false;
    [SerializeField] private bool playerWon = false;
    
    [Header("Unit Tracking")]
    private List<AllyUnit> allAllyUnits = new List<AllyUnit>();
    private List<PlantTurretBase> allPlants = new List<PlantTurretBase>();
    
    [Header("Settings")]
    [SerializeField] private float autoUprootDelay = 1f; // Wait before auto-uproot
    [SerializeField] private float gameOverDelay = 2f; // Wait before showing game over
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    private bool hasTriedAutoUproot = false;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Start()
    {
        // Find all ally units at start
        RefreshAllyUnits();
        RefreshPlants();
        SoundManager.Instance.PlayNormalLoop();
        
        Debug.Log($"[GameManager] Game started with {allAllyUnits.Count} allies, {allPlants.Count} plants");
    }
    
    #region Unit Tracking
    
    /// <summary>
    /// Register an ally unit
    /// </summary>
    public void RegisterAllyUnit(AllyUnit unit)
    {
        if (!allAllyUnits.Contains(unit))
        {
            allAllyUnits.Add(unit);
            Debug.Log($"[GameManager] Registered ally: {unit.name}. Total: {allAllyUnits.Count}");
        }
    }
    
    /// <summary>
    /// Unregister an ally unit (when it dies)
    /// </summary>
    public void UnregisterAllyUnit(AllyUnit unit)
    {
        allAllyUnits.Remove(unit);
        Debug.Log($"[GameManager] Ally died: {unit.name}. Remaining: {allAllyUnits.Count}");
        
        // Check if all units are dead
        if (allAllyUnits.Count == 0 && !gameOver)
        {
            OnAllUnitsDead();
        }
    }
    
    /// <summary>
    /// Register a plant
    /// </summary>
    public void RegisterPlant(PlantTurretBase plant)
    {
        if (!allPlants.Contains(plant))
        {
            allPlants.Add(plant);
            Debug.Log($"[GameManager] Registered plant: {plant.name}. Total: {allPlants.Count}");
        }
    }
    
    /// <summary>
    /// Unregister a plant (when uprooted)
    /// </summary>
    public void UnregisterPlant(PlantTurretBase plant)
    {
        allPlants.Remove(plant);
        Debug.Log($"[GameManager] Plant removed: {plant.name}. Remaining: {allPlants.Count}");
        
        // If we tried auto-uproot and now have units again, reset flag
        if (hasTriedAutoUproot && allAllyUnits.Count > 0)
        {
            hasTriedAutoUproot = false;
        }
        
        // If we're waiting for plants to uproot and none left, check game over
        if (hasTriedAutoUproot && allPlants.Count == 0 && allAllyUnits.Count == 0)
        {
            Invoke("CheckGameOver", gameOverDelay);
        }
    }
    
    void RefreshAllyUnits()
    {
        allAllyUnits.Clear();
        AllyUnit[] units = FindObjectsOfType<AllyUnit>();
        foreach (AllyUnit unit in units)
        {
            allAllyUnits.Add(unit);
        }
    }
    
    void RefreshPlants()
    {
        allPlants.Clear();
        PlantTurretBase[] plants = FindObjectsOfType<PlantTurretBase>();
        foreach (PlantTurretBase plant in plants)
        {
            allPlants.Add(plant);
        }
    }
    
    #endregion
    
    #region Win/Loss Conditions
    
    /// <summary>
    /// Called when player reaches the goal
    /// </summary>
    public void OnPlayerReachedGoal()
    {
        if (gameOver) return;
        
        gameOver = true;
        playerWon = true;
        SoundManager.Instance.PlayWinLoop();
        Debug.Log("[GameManager] PLAYER WON!");
        
        // Show win screen
        if (GameUI.Instance != null)
        {
            GameUI.Instance.ShowWinScreen();
        }
    }
    
    /// <summary>
    /// Called when all ally units are dead
    /// </summary>
    void OnAllUnitsDead()
    {
        Debug.Log("[GameManager] All units dead! Checking for plants...");
        
        // Clean up null/destroyed plants
        allPlants.RemoveAll(p => p == null);
        
        if (allPlants.Count > 0)
        {
            // Try to uproot all plants to get units back
            Debug.Log($"[GameManager] Attempting auto-uproot of {allPlants.Count} plants");
            hasTriedAutoUproot = true;
            Invoke("TryAutoUprootPlants", autoUprootDelay);
        }
        else
        {
            // No plants to uproot - immediate game over
            Debug.Log("[GameManager] No plants available. Game over!");
            Invoke("ShowGameOver", gameOverDelay);
        }
    }
    
    /// <summary>
    /// Auto-uproot all plants when all units are dead
    /// </summary>
    void TryAutoUprootPlants()
    {
        Debug.Log("[GameManager] Auto-uprooting all plants...");
        
        // Create copy of list since Uproot() will modify it
        List<PlantTurretBase> plantsToUproot = new List<PlantTurretBase>(allPlants);
        
        foreach (PlantTurretBase plant in plantsToUproot)
        {
            if (plant != null)
            {
                plant.Uproot();
            }
        }
        
        // Wait a moment for units to spawn, then check if we have any
        Invoke("CheckAfterUproot", 0.5f);
    }
    
    void CheckAfterUproot()
    {
        // Refresh unit count
        RefreshAllyUnits();
        
        if (allAllyUnits.Count > 0)
        {
            Debug.Log($"[GameManager] Auto-uproot successful! {allAllyUnits.Count} units spawned");
            hasTriedAutoUproot = false; // Can try again if they all die
        }
        else
        {
            Debug.Log("[GameManager] Auto-uproot failed - no units spawned. Game over!");
            Invoke("ShowGameOver", gameOverDelay);
        }
    }
    
    void CheckGameOver()
    {
        if (allAllyUnits.Count == 0 && allPlants.Count == 0)
        {
            ShowGameOver();
        }
    }
    
    void ShowGameOver()
    {
        if (gameOver) return; // Already handled
        
        gameOver = true;
        playerWon = false;
        SoundManager.Instance.PlayLoseLoop();
        
        Debug.Log("[GameManager] GAME OVER - PLAYER LOST!");
        
        // Show game over screen
        if (GameUI.Instance != null)
        {
            GameUI.Instance.ShowLoseScreen();
        }
    }
    
    #endregion
    
    #region Game Control
    
    /// <summary>
    /// Restart the current scene
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("[GameManager] Restarting game...");
        
        // Reload current scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    
    /// <summary>
    /// Quit the game
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[GameManager] Quitting game...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    #endregion
    
    #region Getters
    
    public bool IsGameOver() => gameOver;
    public bool DidPlayerWin() => playerWon;
    public int GetAllyCount() => allAllyUnits.Count;
    public int GetPlantCount() => allPlants.Count;
    
    #endregion
    
    #region Debug
    
    void OnGUI()
    {
        if (!showDebugInfo || !Application.isPlaying) return;
        
        GUI.color = Color.white;
        GUI.Label(new Rect(10, 10, 300, 60), 
            $"Allies: {allAllyUnits.Count}\n" +
            $"Plants: {allPlants.Count}\n" +
            $"Game Over: {gameOver} | Won: {playerWon}");
    }
    
    #endregion
}
