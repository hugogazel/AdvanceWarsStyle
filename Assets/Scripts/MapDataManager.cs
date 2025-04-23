using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MapDataManager : MonoBehaviour
{
    public TileDataContainer tileDataContainer;

    // Dictionnaire qui mappe un Sprite → la config associée
    private Dictionary<Sprite, VariantTileData> dataDict;

    void Awake()
    {
        dataDict = new Dictionary<Sprite, VariantTileData>();

        // On parcourt chaque catégorie
        foreach (var variant in tileDataContainer.allTileData)
        {
            // Pour chaque sprite dans la catégorie, on l’ajoute au dico
            foreach (var sprite in variant.sprites)
            {
                if (sprite != null && !dataDict.ContainsKey(sprite))
                {
                    dataDict.Add(sprite, variant);
                }
            }
        }
    }

    public VariantTileData GetTileData(TileBase tileBase)
    {
        // On suppose que ce TileBase est un Tile "classique"
        var tile = tileBase as Tile;
        if (tile == null) return null;

        var sprite = tile.sprite;
        if (sprite != null && dataDict.TryGetValue(sprite, out VariantTileData data))
        {
            return data;
        }

        return null;
    }
}

