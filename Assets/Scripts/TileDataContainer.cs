using UnityEngine;

[CreateAssetMenu(menuName = "Custom Tiles/TileDataContainer")]
public class TileDataContainer : ScriptableObject
{
    public VariantTileData[] allTileData;
}

[System.Serializable]
public class VariantTileData
{
    public string terrainName;             // "Forest", "Mountain", etc. (pour debug)
    public Sprite[] sprites;              // toutes les variations visuelles de cette catégorie
    public int movementCost;
    public int resources;
    // Tout ce que tu veux stocker de plus
}

