using UnityEngine;

/// <summary>
/// Door that connects two rooms
/// Locks/unlocks based on room clear status
/// Triggers room transition when player passes through
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private RoomManager targetRoom; // Room this door leads to
    [SerializeField] private bool isLocked = true;
    [SerializeField] private bool isFinalGoal = false; // Is this the winning goal?
    
    [Header("Visual")]
    [SerializeField] private SpriteRenderer doorSprite;
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;
    [SerializeField] private GameObject lockIcon; // Optional lock visual
    
    [Header("Audio")]
    [SerializeField] private bool playUnlockSound = true;
    [SerializeField] private AudioClip unlockSound;
    
    private Collider2D doorCollider;
    private bool wasLocked = true;
    
    void Start()
    {
        doorCollider = GetComponent<Collider2D>();
        
        if (doorSprite == null)
        {
            doorSprite = GetComponent<SpriteRenderer>();
        }
        
        UpdateVisuals();
    }
    
    /// <summary>
    /// Lock this door
    /// </summary>
    public void Lock()
    {
        isLocked = true;
        wasLocked = true;
        
        if (doorCollider != null)
        {
            doorCollider.isTrigger = false; // Keep as trigger but check lock state
        }
        
        UpdateVisuals();
        
        Debug.Log($"[Door] {gameObject.name} locked");
    }
    
    /// <summary>
    /// Unlock this door
    /// </summary>
    public void Unlock()
    {
        if (!wasLocked) return; // Already unlocked
        
        isLocked = false;
        
        if (doorCollider != null)
        {
            doorCollider.isTrigger = true; // Stays trigger for room transition
        }
        doorSprite.sprite = null; 
        UpdateVisuals();
        
        // Play unlock effect
        if (playUnlockSound)
        {
            SoundManager.Instance.PlaySoundFX(unlockSound, transform);
        }
        
        Debug.Log($"[Door] {gameObject.name} unlocked");
    }
    
    void UpdateVisuals()
    {
        if (doorSprite != null)
        {
            doorSprite.color = isLocked ? lockedColor : unlockedColor;
        }
        
        if (lockIcon != null)
        {
            lockIcon.SetActive(isLocked);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player entered
        AllyUnit ally = other.GetComponent<AllyUnit>();
        
        if (ally != null)
        {
            if (isLocked)
            {
                // Door is locked - don't let through
                Debug.Log($"[Door] {gameObject.name} is locked!");
                // Optional: Show "Door Locked" message
                return;
            }
            
            // Door is unlocked
            OnPlayerPassThrough(ally);
        }
    }
    
    void OnPlayerPassThrough(AllyUnit ally)
    {
        if (isFinalGoal)
        {
            // This is the final goal - win the game!
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerReachedGoal();
            }
            Debug.Log($"[Door] Player reached final goal!");
        }
        else if (targetRoom != null)
        {
            // Transition to next room
            targetRoom.SetActiveRoom();
            Debug.Log($"[Door] Player entered {targetRoom.GetRoomName()}");
        }
    }
    
    /// <summary>
    /// Check if door is locked
    /// </summary>
    public bool IsLocked()
    {
        return isLocked;
    }
    
    void OnDrawGizmos()
    {
        // Draw door position
        Gizmos.color = isLocked ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        
        // Draw line to target room
        if (targetRoom != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetRoom.GetRoomCenter());
            
            // Draw label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, 
                isFinalGoal ? "GOAL" : $"→ {targetRoom.GetRoomName()}");
            #endif
        }
    }
}