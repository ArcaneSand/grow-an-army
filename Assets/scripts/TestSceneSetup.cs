using UnityEngine;

/// <summary>
/// Simple test script to set up a test scene with player, allies, and enemies
/// Tests the modified grass spawning system
/// Attach this to an empty GameObject in your test scene
/// 
/// Keyboard Shortcuts:
/// A - Spawn ally at mouse position
/// E - Spawn enemy at mouse position
/// G - Spawn grass at mouse position
/// </summary>
public class TestSceneSetup : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private int initialAllyCount = 3;
    [SerializeField] private int enemyCount = 5;
    
    [Header("Prefab References (Optional)")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject allyUnitPrefab;
    [SerializeField] private GameObject enemyUnitPrefab;
    
    [Header("ScriptableObject References")]
    [SerializeField] private UnitDataSO villagerData;
    
    [Header("Spawn Positions")]
    [SerializeField] private Vector2 playerSpawnPos = Vector2.zero;
    [SerializeField] private Vector2 enemySpawnCenter = new Vector2(10f, 0f);
    [SerializeField] private float enemySpawnRadius = 3f;
    
    private PlayerController spawnedPlayer;
    
    void Start()
    {
        if (autoSetupOnStart)
        {
            SetupTestScene();
        }
    }
    
    [ContextMenu("Setup Test Scene")]
    public void SetupTestScene()
    {
        Debug.Log("Setting up test scene...");
        
        SpawnPlayer();
        SpawnInitialAllies();
        SpawnEnemies();
        SetupCamera();
        
        Debug.Log("Test scene setup complete!");
        Debug.Log("Press A to spawn ally, E to spawn enemy, G to test grass spawn");
    }
    
    void SpawnPlayer()
    {
        if (playerPrefab != null)
        {
            GameObject playerObj = Instantiate(playerPrefab, playerSpawnPos, Quaternion.identity);
            spawnedPlayer = playerObj.GetComponent<PlayerController>();
        }
        else
        {
            GameObject playerObj = new GameObject("Player");
            playerObj.transform.position = playerSpawnPos;
            spawnedPlayer = playerObj.AddComponent<PlayerController>();
            
            // Add visual
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(playerObj.transform);
            visual.transform.localPosition = Vector3.zero;
            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(Color.green);
        }
        
        Debug.Log($"Player spawned at {playerSpawnPos}");
    }
    
    void SpawnInitialAllies()
    {
        for (int i = 0; i < initialAllyCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * 2f;
            Vector2 spawnPos = playerSpawnPos + offset;
            SpawnAllyUnit(spawnPos);
        }
        
        Debug.Log($"Spawned {initialAllyCount} ally units");
    }
    
    void SpawnEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            float angle = (360f / enemyCount) * i * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(
                Mathf.Cos(angle) * enemySpawnRadius,
                Mathf.Sin(angle) * enemySpawnRadius
            );
            
            Vector2 spawnPos = enemySpawnCenter + offset;
            SpawnEnemyUnit(spawnPos);
        }
        
        Debug.Log($"Spawned {enemyCount} enemy units at {enemySpawnCenter}");
    }
    
    GameObject SpawnAllyUnit(Vector2 position)
    {
        GameObject unitObj;
        
        if (allyUnitPrefab != null)
        {
            unitObj = Instantiate(allyUnitPrefab, position, Quaternion.identity);
        }
        else
        {
            unitObj = new GameObject("AllyUnit");
            unitObj.transform.position = position;
            
            AllyUnit unit = unitObj.AddComponent<AllyUnit>();
            unit.unitData = villagerData;
            unit.team = UnitTeam.Player;
            
            // Add visual
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(unitObj.transform);
            visual.transform.localPosition = Vector3.zero;
            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(new Color(0.2f, 1f, 0.2f)); // Bright green
            unit.spriteRenderer = sr;
            unit.visualTransform = visual.transform;
        }
        
        return unitObj;
    }
    
    GameObject SpawnEnemyUnit(Vector2 position)
    {
        GameObject unitObj;
        
        if (enemyUnitPrefab != null)
        {
            unitObj = Instantiate(enemyUnitPrefab, position, Quaternion.identity);
        }
        else
        {
            unitObj = new GameObject("EnemyUnit");
            unitObj.transform.position = position;
            
            EnemyUnit unit = unitObj.AddComponent<EnemyUnit>();
            unit.unitData = villagerData;
            unit.team = UnitTeam.Enemy;
            
            // Add visual
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(unitObj.transform);
            visual.transform.localPosition = Vector3.zero;
            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(new Color(1f, 0.3f, 0.3f)); // Red
            unit.spriteRenderer = sr;
            unit.visualTransform = visual.transform;
        }
        
        return unitObj;
    }
    
    void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(playerSpawnPos.x, playerSpawnPos.y, -10f);
            mainCam.orthographicSize = 10f;
        }
    }
    
    Sprite CreateCircleSprite(Color color)
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = distance <= radius ? color : Color.clear;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
    
    void Update()
    {
        // A - Spawn ally at mouse position
        if (Input.GetKeyDown(KeyCode.A))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            SpawnAllyUnit(new Vector2(mousePos.x, mousePos.y));
            Debug.Log("Spawned ally unit at mouse position");
        }
        
        // E - Spawn enemy at mouse position
        if (Input.GetKeyDown(KeyCode.E))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            SpawnEnemyUnit(new Vector2(mousePos.x, mousePos.y));
            Debug.Log("Spawned enemy unit at mouse position");
        }
        
        // G - Test grass spawn (uses NEW modified system)
        if (Input.GetKeyDown(KeyCode.G))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (GrassManager.Instance != null)
            {
                // NEW: TrySpawnGrassAndPlant (no radius parameter)
                GrassManager.Instance.TrySpawnGrassAndPlant(
                    new Vector2(mousePos.x, mousePos.y),
                    PlantType.Grass
                );
                Debug.Log("Tested grass spawn at mouse position (80% chance)");
            }
            else
            {
                Debug.LogWarning("GrassManager not found! Make sure it exists in the scene.");
            }
        }
    }
}