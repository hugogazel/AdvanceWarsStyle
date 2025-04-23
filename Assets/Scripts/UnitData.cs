using UnityEngine;

public enum Team
{
    Red,
    Blue
}

[CreateAssetMenu(fileName = "NewUnitData", menuName = "Game/UnitData")]
public class UnitData : ScriptableObject
{
    public string unitName;       // Nom de l'unité
    public Sprite unitSprite;     // Sprite de l’unité (peut servir à la carte)
    public int maxMovePoints;     // Points de déplacement max
    public int biomass;           // Biomasse (utilisée pour les interactions et la victoire)
    public bool isPredator;       // Vrai si l’unité est un prédateur
    public bool isHerbivore;      // Vrai si l’unité est un herbivore
    public bool isOmnivore;       // Vrai si l’unité est un omnivore
    public Team team;             // Equipe par défaut (Red ou Blue)

    public int lifePoints = 10;   // Points de vie de l’unité

    // Ajoute ce champ pour pouvoir instancier l’unité depuis le deck
    public GameObject unitPrefab;

    [Header("Fog of War")]
    [Tooltip("Rayon de vision en nombre de cases")]
    public int visionRange = 3;

    [Header("Artwork pour CardInfoPanel")]
    public Sprite artwork;
}







