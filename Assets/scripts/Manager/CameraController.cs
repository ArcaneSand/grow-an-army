using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Controls camera transitions between rooms
/// Works with Cinemachine Virtual Camera and Confiner2D
/// </summary>
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private CinemachineConfiner2D confiner;
    
    [Header("Camera Settings")]
    [SerializeField] private float transitionTime = 1f;
    [SerializeField] private bool smoothTransition = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    [SerializeField]private RoomManager currentRoom;
    
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
        SetupCinemachine();
    }
    
    void SetupCinemachine()
    {
        // Find virtual camera if not assigned
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineCamera>();
        }
        
        // Get or add confiner
        if (confiner == null && virtualCamera != null)
        {
            confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
            if (confiner == null)
            {
                confiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner2D>();
            }
        }
        
        if (virtualCamera == null)
        {
            Debug.LogError("[CameraController] No Cinemachine Virtual Camera found!");
        }
    }
    
    /// <summary>
    /// Set the current room and update camera confiner
    /// </summary>
    public void SetCurrentRoom(RoomManager room)
    {
        if (room == null) return;
        
        // Deactivate previous room
        if (currentRoom != null)
        {
            currentRoom.DeactivateRoom();
        }
        
        currentRoom = room;
        
        // Update camera confiner
        UpdateCameraConfiner();
        
        Debug.Log($"[CameraController] Now in room: {room.GetRoomName()}");
    }
    
    void UpdateCameraConfiner()
    {
        if (confiner == null || currentRoom == null) return;
        
        PolygonCollider2D roomConfiner = currentRoom.GetCameraConfiner();
        
        if (roomConfiner != null)
        {
            // Set confiner to room bounds
            confiner.BoundingShape2D = roomConfiner;
            
            // Invalidate cache to force update
            confiner.InvalidateBoundingShapeCache();
            
            Debug.Log($"[CameraController] Camera confined to {currentRoom.GetRoomName()}");
        }
        else
        {
            Debug.LogWarning($"[CameraController] Room {currentRoom.GetRoomName()} has no camera confiner!");
        }
    }
    
    /// <summary>
    /// Get the current active room
    /// </summary>
    public RoomManager GetCurrentRoom()
    {
        return currentRoom;
    }
    
    // void OnGUI()
    // {
    //     if (!showDebugInfo || !Application.isPlaying) return;
        
    //     if (currentRoom != null)
    //     {
    //         GUI.color = Color.cyan;
    //         GUI.Label(new Rect(10, 100, 300, 20), $"Current Room: {currentRoom.GetRoomName()}");
    //     }
    // }
}