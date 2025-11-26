using UnityEngine;

/// <summary>
/// Goal trigger - detects when any ally unit reaches the goal
/// Place this on the final door/exit
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class GoalTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool requiresAllUnits = false; // Win when ALL units reach goal?
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private Color gizmoColor = Color.green;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject winEffectPrefab;
    [SerializeField] private bool playSoundOnWin = true;
    
    private Collider2D triggerCollider;
    private bool goalReached = false;
    
    void Start()
    {
        // Ensure collider is a trigger
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
        
        Debug.Log($"[GoalTrigger] Goal initialized at {transform.position}");
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {   
        Debug.Log("[GoalTrigger] Trigger entered by " + other.name);
        if (goalReached) return; // Already won
        
        // Check if an ally unit entered
        AllyUnit ally = other.GetComponent<AllyUnit>();
        
        if (ally != null && !ally.IsDead())
        {
            OnAllyReachedGoal(ally);
        }
    }
    
    void OnAllyReachedGoal(AllyUnit ally)
    {
        if (requiresAllUnits)
        {
            // Check if all units have reached goal
            // (This would require tracking which units arrived)
            // For now, just win on first unit
            Debug.Log($"[GoalTrigger] {ally.name} reached goal!");
        }
        
        Debug.Log($"[GoalTrigger] GOAL REACHED by {ally.name}!");
        
        goalReached = true;
        
        // Spawn win effect
        if (winEffectPrefab != null)
        {
            Instantiate(winEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Play sound
        if (playSoundOnWin)
        {
            // AudioManager.PlaySound("Victory");
        }
        
        // Notify game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerReachedGoal();
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmo) return;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = gizmoColor;
            
            if (col is BoxCollider2D box)
            {
                Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
                
                // Draw filled transparent box
                Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
                Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
            }
            else if (col is CircleCollider2D circle)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
            
            // Draw label
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, "GOAL");
            #endif
        }
    }
}
