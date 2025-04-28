using UnityEngine;

// On renomme l'enum pour J1Team / J2Team
public enum Team
{
    J1Team,
    J2Team
}
public enum Sex
{
    Male,
    Female
}

[CreateAssetMenu(fileName = "NewUnitData", menuName = "Game/UnitData")]
public class UnitData : ScriptableObject
{
    public string unitName;       // Nom de l'unité
    public Sprite unitSprite;     // Sprite de l’unité (pour la carte)
    public int maxMovePoints;     // Points de déplacement max
    public int biomass;           // Biomasse (interactions/victoire)
    public bool isPredator;
    public bool isHerbivore;
    public bool isOmnivore;

    [Header("Team Settings")]
    [Tooltip("Équipe par défaut (utile si vous instanciez hors deck)")]
    public Team defaultTeam;

    public int lifePoints = 10;   // Points de vie
    public GameObject unitPrefab; // Prefab de l'unité

    [Header("Fog of War")]
    [Tooltip("Rayon de vision en cases")]
    public int visionRange = 3;

    [Header("Artwork pour CardInfoPanel")]
    public Sprite artwork;

    [Header("Reproduction")]
    public Sex sex;  // ♂ ou ♀
}







