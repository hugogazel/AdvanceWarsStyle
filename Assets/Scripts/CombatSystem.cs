using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    public bool Attack(UnitController attacker, UnitController target)
    {
        if (attacker == null || target == null)
        {
            Debug.LogWarning("❌ CombatSystem.Attack : Attaquant ou cible null !");
            return false;
        }
        if (!attacker.unitData.isPredator)
        {
            Debug.Log("❌ L'attaquant n'est pas un prédateur, attaque impossible !");
            return false;
        }
        if (!AreAdjacent(attacker.position, target.position))
        {
            Debug.Log("❌ Cible non adjacente, attaque impossible !");
            return false;
        }
        int damage = attacker.currentBiomass - target.currentBiomass;
        if (damage <= 0)
        {
            Debug.Log("⚠️ Pas assez de biomasse pour infliger des dégâts !");
            return false;
        }
        target.TakeDamage(damage);
        Debug.Log($"✅ {attacker.unitData.unitName} attaque {target.unitData.unitName} pour {damage} dégâts !");
        return true;
    }

    private bool AreAdjacent(Vector2Int posA, Vector2Int posB)
    {
        int dx = Mathf.Abs(posA.x - posB.x);
        int dy = Mathf.Abs(posA.y - posB.y);
        return (dx + dy) == 1;
    }
}




