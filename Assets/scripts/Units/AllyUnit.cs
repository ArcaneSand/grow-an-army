using UnityEngine;

/// <summary>
/// Ally units that follow formation slots managed by ArmyFormationManager
/// Units move to their assigned slot position, not directly to player
/// This prevents pushing and creates smooth formation movement
/// </summary>
public class AllyUnit : UnitBase
{
    private Camera cam;
    protected override void Start()
    {
        base.Start();
        cam = Camera.main;
         if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterAllyUnit(this);
        }
    }
    
    void OnDestroy()
    {
         if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterAllyUnit(this);
        }
    }
    
    protected override UnitTeam GetTeam()
    {
        return UnitTeam.Player;
    }
    
    protected override void UpdateMovement()
    {
        // Get mouse world position
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 target = new Vector2(mousePos.x, mousePos.y);
        
        // Move toward mouse
        MoveTowards(target);
    }

    protected override void ApplyMovement()
    {
        // Use base movement for normal cases
        base.ApplyMovement();

    
    }
    
    protected override void OnDeath(UnitBase killer)
    {
        base.OnDeath(killer);
    }
    
    protected override void OnAttackPerformed(UnitBase target)
    {
        base.OnAttackPerformed(target);
        
        // Add visual feedback for attacks
        // Could add simple animation or particle effect here
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, unitData.maxHealth);
        
        // Optional: Show heal effect
        // Could add floating text or particle effect here
    }
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();   
    }
}