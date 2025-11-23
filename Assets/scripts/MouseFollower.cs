using UnityEngine;

public class MouseFollower : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f; // How fast to follow
    [SerializeField] private Vector2 offset = Vector2.zero; // Optional offset
    
    [Header("Boundaries (Optional)")]
    [SerializeField] private bool useBoundaries = false;
    [SerializeField] private float minX = -50f;
    [SerializeField] private float maxX = 50f;
    [SerializeField] private float minY = -50f;
    [SerializeField] private float maxY = 50f;
    
    private Camera cam;
    
    void Start()
    {
        cam = Camera.main;  
    }
    
    void Update()
    {
        // Get mouse world position
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 targetPos = new Vector2(mousePos.x, mousePos.y) + offset;
        
        // Apply boundaries if enabled
        if (useBoundaries)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);
            targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);
        }
        
        // Smoothly move to target
        Vector2 smoothPos = Vector2.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
        transform.position = new Vector3(smoothPos.x, smoothPos.y, transform.position.z);
    }
}
