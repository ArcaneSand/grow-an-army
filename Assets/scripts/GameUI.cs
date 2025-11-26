using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages game UI - win/lose screens, restart buttons
/// </summary>
public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }
    
    [Header("UI Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject hudPanel; // Optional HUD
    
    [Header("Win Screen")]
    [SerializeField] private TextMeshProUGUI winTitle;
    [SerializeField] private TextMeshProUGUI winMessage;
    [SerializeField] private Button winRestartButton;
    [SerializeField] private Button winQuitButton;
    
    [Header("Lose Screen")]
    [SerializeField] private TextMeshProUGUI loseTitle;
    [SerializeField] private TextMeshProUGUI loseMessage;
    [SerializeField] private Button loseRestartButton;
    [SerializeField] private Button loseQuitButton;
    
    [Header("HUD Elements (Optional)")]
    [SerializeField] private TextMeshProUGUI allyCountText;
    [SerializeField] private TextMeshProUGUI plantCountText;

    [Header("Room HUD (NEW)")]
    [SerializeField] private TextMeshProUGUI roomNameText; // NEW: Shows current room name
    [SerializeField] private TextMeshProUGUI roomProgressText; // NEW: Shows "Enemies: 3/10"
    
    private RoomManager currentRoom; // NEW: Track current room
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
        // Hide all screens at start
        HideAllScreens();
        
        // Setup button listeners
        SetupButtons();
        
        // Show HUD if available
        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
        }
    }
    
    void Update()
    {
        // Update HUD
        UpdateHUD();
    }
    
    void SetupButtons()
    {
        // Win screen buttons
        if (winRestartButton != null)
        {
            winRestartButton.onClick.AddListener(OnRestartClicked);
        }
        if (winQuitButton != null)
        {
            winQuitButton.onClick.AddListener(OnQuitClicked);
        }
        
        // Lose screen buttons
        if (loseRestartButton != null)
        {
            loseRestartButton.onClick.AddListener(OnRestartClicked);
        }
        if (loseQuitButton != null)
        {
            loseQuitButton.onClick.AddListener(OnQuitClicked);
        }
    }
    
    #region Screen Control
    
    public void ShowWinScreen()
    {
        HideAllScreens();
        hudPanel.SetActive(false);
        
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            
            if (winTitle != null)
            {
                winTitle.text = "VICTORY!";
            }
            
            if (winMessage != null)
            {
                winMessage.text = "You reached the goal!\n\nCongratulations!";
            }
        }
        
        // Pause game (optional)
        // Time.timeScale = 0f;
        
        Debug.Log("[GameUI] Win screen displayed");
    }
    
    public void ShowLoseScreen()
    {
        HideAllScreens();
        hudPanel.SetActive(false);
        if (losePanel != null)
        {
            losePanel.SetActive(true);
            
            if (loseTitle != null)
            {
                loseTitle.text = "GAME OVER";
            }
            
            if (loseMessage != null)
            {
                loseMessage.text = "All your units have fallen.\n\nTry again?";
            }
        }
        
        // Pause game (optional)
        // Time.timeScale = 0f;
        
        Debug.Log("[GameUI] Lose screen displayed");
    }
    
    void HideAllScreens()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        
        if (losePanel != null)
        {
            losePanel.SetActive(false);
        }
    }
    public void SetCurrentRoom(RoomManager room)
    {
        currentRoom = room;
        UpdateRoomProgress();
        
        Debug.Log($"[GameUI] Now displaying: {room.GetRoomName()}");
    }

    public void UpdateRoomProgress()
    {
        if (currentRoom == null) return;
        
        // Update room name
        if (roomNameText != null)
        {
            roomNameText.text = currentRoom.GetRoomName();
            
            // Change color based on status
            if (currentRoom.IsCleared())
            {
                roomNameText.color = Color.green;
            }
            else
            {
                roomNameText.color = Color.white;
            }
        }
        
        // Update room progress
        if (roomProgressText != null)
        {
            int remaining = currentRoom.GetRemainingEnemies();
            int total = currentRoom.GetTotalEnemies();
            
            if (remaining == 0 && total > 0)
            {
                roomProgressText.text = "CLEAR!";
                roomProgressText.color = Color.green;
            }
            else
            {
                roomProgressText.text = $"Enemies: {remaining}/{total}";
                
                // Change color based on progress
                if (remaining <= total * 0.3f && remaining > 0)
                {
                    roomProgressText.color = Color.yellow; // Almost done
                }
                else
                {
                    roomProgressText.color = Color.white;
                }
            }
        }
    }
    #endregion
    
    #region HUD
    
    void UpdateHUD()
    {
        if (GameManager.Instance == null) return;
        
        if (allyCountText != null)
        {
            int allyCount = GameManager.Instance.GetAllyCount();
            allyCountText.text = $"Units: {allyCount}";
            
            // Color red if low
            if (allyCount <= 3)
            {
                allyCountText.color = Color.red;
            }
            else
            {
                allyCountText.color = Color.white;
            }
        }
        
        if (plantCountText != null)
        {
            int plantCount = GameManager.Instance.GetPlantCount();
            plantCountText.text = $"Plants: {plantCount}";
        }
    }
    
    #endregion
    
    #region Button Handlers
    
    void OnRestartClicked()
    {
        Debug.Log("[GameUI] Restart button clicked");
        
        // Unpause if paused
        Time.timeScale = 1f;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
    
    void OnQuitClicked()
    {
        Debug.Log("[GameUI] Quit button clicked");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
    
    #endregion
}
