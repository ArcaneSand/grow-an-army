using UnityEngine;

/// <summary>
/// Represents a grass tile on the ground
/// Provides speed boost and healing to the player
/// Visual representation of restored land
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class GrassTile : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color grassColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
    
    [Header("Buffs")]
    [SerializeField] private float healPerSecond = 5f;
    [SerializeField] private float speedMultiplier = 1.3f;
    
    [Header("Animation")]
    [SerializeField] private float growDuration = 0.3f;
    [SerializeField] private AnimationCurve growCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private GridCell myGridCell;
    private float growTimer = 0f;
    private Vector3 targetScale;
    
    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
    
    public void Initialize(GridCell cell)
    {
        myGridCell = cell;
        
        // Setup visual
        if (spriteRenderer != null)
        {
            spriteRenderer.color = grassColor;
            spriteRenderer.sortingOrder = -1; // Behind everything
        }
        
        // Start grow animation
        targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
        growTimer = 0f;
    }
    
    void Update()
    {
        // Grow animation
        if (growTimer < growDuration)
        {
            growTimer += Time.deltaTime;
            float t = growTimer / growDuration;
            float curveValue = growCurve.Evaluate(t);
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, curveValue);
        }
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        // Check if player is on this grass
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetOnGrass(true);
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.SetOnGrass(false);
        }
    }
    
    public float GetHealPerSecond() => healPerSecond;
    public float GetSpeedMultiplier() => speedMultiplier;
    
    void OnDestroy()
    {
        // Update grid cell
        if (myGridCell != null)
        {
            myGridCell.RemoveGrass();
        }
    }
}
