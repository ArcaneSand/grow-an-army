using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;

/// <summary>
/// Manages a room with multiple enemy groups
/// Tracks enemies, unlocks door when cleared
/// UPDATED: Now supports room activation, camera confinement, and multi-room dungeons
/// </summary>
public class RoomManager : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] private string roomName = "Room 1"; // NEW: Display name for HUD
    [SerializeField] private GameObject doorObject; // Legacy - single door
    [SerializeField] private bool startLocked = true;
    [SerializeField] private bool isStartingRoom = false; // NEW: First room player enters
    
    [Header("Room Boundaries")]
    [SerializeField] private Vector2 roomCenter = Vector2.zero;
    [SerializeField] private Vector2 roomSize = new Vector2(20f, 20f);
    [SerializeField] private bool showRoomBounds = true;
    
    [Header("Camera Confiner (NEW)")]
    [SerializeField] private PolygonCollider2D cameraConfiner; // NEW: For Cinemachine Confiner2D
    [SerializeField] private bool autoCreateConfiner = true; // NEW: Auto-create camera bounds
    
    [Header("Room Connections (NEW)")]
    [SerializeField] private List<Door> doorsToNextRooms = new List<Door>(); // NEW: Multiple exits
    
    [Header("Enemy Groups")]
    [SerializeField] private List<EnemyGroup> enemyGroups = new List<EnemyGroup>();
    [SerializeField] private bool autoFindGroups = true; // Find groups in children
    [SerializeField] private BoxCollider2D roomCollider;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    private List<EnemyUnit> allEnemies = new List<EnemyUnit>();
    private bool roomCleared = false;
    private bool isActiveRoom = false; // NEW: Is player currently in this room?
    private int totalEnemies = 0;
    private int enemiesKilled = 0;
    
    void Start()
    {
        Initialize();
    }
    
    void Initialize()
    {
        // Set room center to this transform's position if not manually set
        if (roomCenter == Vector2.zero)
        {
            roomCenter = transform.position;
        }
        
        // NEW: Create camera confiner if needed
        if (autoCreateConfiner && cameraConfiner == null)
        {
            CreateCameraConfiner();
        }
        
        // Find enemy groups if auto-find enabled
        if (autoFindGroups)
        {
            FindEnemyGroups();
        }
        
        // Set boundaries for all groups
        SetGroupBoundaries();
        
        // Register all enemies from all groups
        RegisterAllEnemies();
        
        // Lock doors if needed
        if (startLocked)
        {
            LockDoors();
        }
        
        // NEW: Set as active if starting room
        if (isStartingRoom)
        {
            SetActiveRoom();
        }
        else
        {
            setEnemyActive(false);
        }
        
        Debug.Log($"[RoomManager] '{roomName}' initialized with {totalEnemies} enemies in {enemyGroups.Count} groups");
    }
    
    /// <summary>
    /// NEW: Create a polygon collider for camera confiner
    /// </summary>
    void CreateCameraConfiner()
    {
        GameObject confinerObj = new GameObject($"{gameObject.name}_CameraConfiner");
        confinerObj.transform.SetParent(transform);
        confinerObj.transform.localPosition = Vector3.zero;
        
        cameraConfiner = confinerObj.AddComponent<PolygonCollider2D>();
        cameraConfiner.isTrigger = true;
        
        // Create rectangular bounds
        Vector2 halfSize = roomSize * 0.5f;
        Vector2[] points = new Vector2[]
        {
            new Vector2(-halfSize.x, -halfSize.y),
            new Vector2(halfSize.x, -halfSize.y),
            new Vector2(halfSize.x, halfSize.y),
            new Vector2(-halfSize.x, halfSize.y)
        };
        
        cameraConfiner.SetPath(0, points);
        
        Debug.Log($"[RoomManager] Created camera confiner for '{roomName}'");
    }
    
    /// <summary>
    /// Set boundaries for all enemy groups in this room
    /// </summary>
    void SetGroupBoundaries()
    {
        foreach (EnemyGroup group in enemyGroups)
        {
            if (group == null) continue;
            
            // Groups will auto-detect from parent
        }
    }
    
    #region Group Management
    
    /// <summary>
    /// Find all EnemyGroup components in children
    /// </summary>
    void FindEnemyGroups()
    {
        enemyGroups.Clear();
        EnemyGroup[] groups = GetComponentsInChildren<EnemyGroup>();
        
        foreach (EnemyGroup group in groups)
        {
            enemyGroups.Add(group);
        }
        
        Debug.Log($"[RoomManager] Found {enemyGroups.Count} enemy groups in '{roomName}'");
    }
    
    /// <summary>
    /// Register all enemies from all groups
    /// </summary>
    void RegisterAllEnemies()
    {
        allEnemies.Clear();
        
        foreach (EnemyGroup group in enemyGroups)
        {
            if (group == null) continue;
            
            EnemyUnit[] groupEnemies = group.GetComponentsInChildren<EnemyUnit>();
            foreach (EnemyUnit enemy in groupEnemies)
            {
                RegisterEnemy(enemy);
            }
        }
        
        totalEnemies = allEnemies.Count;
    }
    
    /// <summary>
    /// Register single enemy
    /// </summary>
    public void RegisterEnemy(EnemyUnit enemy)
    {
        if (!allEnemies.Contains(enemy))
        {
            allEnemies.Add(enemy);
        }
    }
    
    #endregion
    
    #region Room Activation (NEW)
    
    /// <summary>
    /// NEW: Set this room as the active room (player entered)
    /// </summary>
    public void SetActiveRoom()
    {
        if (isActiveRoom) return;
        
        isActiveRoom = true;
        
        Debug.Log($"[RoomManager] '{roomName}' is now active");
        
        // Notify camera system
        if (CameraController.Instance != null)
        {
            CameraController.Instance.SetCurrentRoom(this);
        }
        
        // Notify UI
        if (GameUI.Instance != null)
        {
            GameUI.Instance.SetCurrentRoom(this);
        }
        
        setEnemyActive(true);
    }
    
    /// <summary>
    /// NEW: Deactivate this room (player left)
    /// </summary>
    public void DeactivateRoom()
    {
        isActiveRoom = false;
        Debug.Log($"[RoomManager] '{roomName}' deactivated");
    }
    
    private void setEnemyActive(bool isActive)
    {
        foreach (EnemyUnit enemy in allEnemies)
        {
            if (enemy != null)
            {   
                enemy.gameObject.SetActive(isActive);
            }
        }
    }
    #endregion
    
    #region Enemy Death Tracking
    
    /// <summary>
    /// Called by EnemyUnit when it dies
    /// </summary>
    public void OnEnemyDeath(EnemyUnit enemy)
    {
        allEnemies.Remove(enemy);
        enemiesKilled++;
        
        if (showDebugInfo)
        {
            Debug.Log($"[RoomManager] Enemy died in '{roomName}'. Remaining: {allEnemies.Count}/{totalEnemies}");
        }
        
        // NEW: Update UI if this is active room
        if (isActiveRoom && GameUI.Instance != null)
        {
            GameUI.Instance.UpdateRoomProgress();
        }
        
        // Check if room cleared
        if (allEnemies.Count == 0 && !roomCleared)
        {
            OnRoomCleared();
        }
    }
    
    /// <summary>
    /// Called when all enemies are dead
    /// </summary>
    void OnRoomCleared()
    {
        roomCleared = true;
        
        Debug.Log($"[RoomManager] '{roomName}' cleared! ({enemiesKilled} enemies killed)");
        
        // Unlock doors
        UnlockDoors();
        
        // Optional: Play victory sound, spawn rewards, etc.
        OnRoomClearEffects();
        
        // NEW: Update UI
        if (isActiveRoom && GameUI.Instance != null)
        {
            GameUI.Instance.UpdateRoomProgress();
        }
    }
    
    #endregion
    
    #region Door Control
    
    void LockDoors()
    {
        // NEW: Lock all doors to next rooms
        foreach (Door door in doorsToNextRooms)
        {
            if (door != null)
            {
                door.Lock();
            }
        }
        
        // Also lock legacy doorObject if set
        if (doorObject != null)
        {
            Door door = doorObject.GetComponent<Door>();
            if (door != null)
            {
                door.Lock();
            }
            else
            {
                doorObject.SetActive(true);
            }
        }
        
        Debug.Log($"[RoomManager] Doors locked in '{roomName}'");
    }
    
    void UnlockDoors()
    {
        // NEW: Unlock all doors to next rooms
        foreach (Door door in doorsToNextRooms)
        {
            if (door != null)
            {
                door.Unlock();
            }
        }
        
        // Also unlock legacy doorObject if set
        if (doorObject != null)
        {
            Door door = doorObject.GetComponent<Door>();
            if (door != null)
            {
                door.Unlock();
            }
            else
            {
                // No Door component - just disable collider
                Collider2D doorCollider = doorObject.GetComponent<Collider2D>();
                if (doorCollider != null)
                {
                    doorCollider.enabled = false;
                }
                
                // Change color to show it's unlocked
                SpriteRenderer doorSprite = doorObject.GetComponent<SpriteRenderer>();
                if (doorSprite != null)
                {
                    doorSprite.color = new Color(0.5f, 1f, 0.5f); // Green tint
                }
            }
        }
        
        Debug.Log($"[RoomManager] Doors unlocked in '{roomName}'!");
    }
    
    #endregion
    
    #region Effects
    
    /// <summary>
    /// Optional effects when room is cleared
    /// </summary>
    void OnRoomClearEffects()
    {
        // Play sound
        // AudioManager.PlaySound("RoomCleared");
        
        // Spawn rewards
        // RewardManager.SpawnRewards(transform.position);
        
        // Visual effect
        // ParticleEffect.Spawn("RoomClearEffect", transform.position);
    }
    
    #endregion
    
    #region Public Getters
    
    public bool IsCleared()
    {
        return roomCleared;
    }
    
    // NEW
    public bool IsActive()
    {
        return isActiveRoom;
    }
    
    public int GetTotalEnemies()
    {
        return totalEnemies;
    }
    
    public int GetRemainingEnemies()
    {
        return allEnemies.Count;
    }
    
    public int GetEnemiesKilled()
    {
        return enemiesKilled;
    }
    
    public float GetClearProgress()
    {
        if (totalEnemies == 0) return 1f;
        return (float)enemiesKilled / totalEnemies;
    }
    
    // NEW
    public PolygonCollider2D GetCameraConfiner()
    {
        return cameraConfiner;
    }
    
    // NEW
    public Vector2 GetRoomCenter()
    {
        return roomCenter;
    }
    
    // NEW
    public string GetRoomName()
    {
        return roomName;
    }
    
    #endregion
    
    #region Debug
    
    void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        // Draw room bounds
        if (showRoomBounds)
        {
            Gizmos.color = roomCleared ? new Color(0, 1, 0, 0.3f) : new Color(1, 0, 0, 0.3f);
            if (isActiveRoom && Application.isPlaying)
            {
                Gizmos.color = new Color(0, 0, 1, 0.3f); // Blue for active
            }
            
            Gizmos.DrawWireCube(roomCenter, roomSize);
            
            // Draw filled transparent box
            Color fillColor = roomCleared ? new Color(0, 1, 0, 0.05f) : new Color(1, 0, 0, 0.05f);
            if (isActiveRoom && Application.isPlaying)
            {
                fillColor = new Color(0, 0, 1, 0.1f);
            }
            Gizmos.color = fillColor;
            Gizmos.DrawCube(roomCenter, roomSize);
        }
        
        // Draw lines to all enemies
        if (Application.isPlaying)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            foreach (EnemyUnit enemy in allEnemies)
            {
                if (enemy != null)
                {
                    Gizmos.DrawLine(transform.position, enemy.transform.position);
                }
            }
        }
    }
    
    void OnGUI()
    {
        if (!showDebugInfo || !Application.isPlaying) return;
        
        // Show room status on screen
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        
        GUI.color = roomCleared ? Color.green : Color.white;
        if (isActiveRoom)
        {
            GUI.color = Color.cyan;
        }
        
        string status = isActiveRoom ? "\n[ACTIVE]" : "";
        GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 40, 100, 60), 
            $"{roomName}\n{allEnemies.Count}/{totalEnemies}{status}");
    }
    
    #endregion
}