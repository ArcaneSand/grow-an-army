using UnityEngine;

/// <summary>
/// Abstract base class for different attack behaviors
/// Strategy pattern - separates attack logic from unit logic
/// </summary>
public abstract class AttackBehavior : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] protected float attackRange = 1.5f;
    [SerializeField] protected float attackDamage = 10f;
    
    /// <summary>
    /// Execute the attack on target
    /// </summary>
    /// <param name="target">Enemy to attack</param>
    /// <param name="attacker">Unit performing the attack</param>
    public abstract void Execute(UnitBase target, UnitBase attacker);
    
    /// <summary>
    /// Check if attack can be executed (range check, etc)
    /// </summary>
    public virtual bool CanExecute(UnitBase target, UnitBase attacker)
    {
        if (target == null || target.IsDead()) return false;
        if (attacker == null || attacker.IsDead()) return false;
        
        float distance = Vector2.Distance(attacker.transform.position, target.transform.position);
        return distance <= attackRange;
    }
    
    /// <summary>
    /// Initialize with data from UnitDataSO
    /// </summary>
    public virtual void Initialize(UnitDataSO data)
    {
        if (data == null) return;
        
        attackRange = data.attackRange;
        attackDamage = data.attackDamage;
    }
}