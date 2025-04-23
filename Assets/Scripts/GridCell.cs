using UnityEngine;
using UnityEngine.Tilemaps;

public enum TerrainType
{
    Plain,      // Terrain normal
    Forest,     // Forêt, mouvement pénalisé
    Mountain,   // Montagne, déplacement difficile
    Water       // Eau, généralement infranchissable
}

public class GridCell
{
    public Vector2Int position;      // Position dans la grille
    public bool isWalkable;          // Indique si l'unité peut se déplacer ici
    public TerrainType terrain;      // Type de terrain de la case
    public int resources;            // Consommables par les herbivores
    private Tilemap tilemap;         // Référence à la Tilemap associée
    public Vector3Int cellPosition;  // Position de la case dans la Tilemap

    public GridCell(Vector2Int pos, Tilemap tilemap)
    {
        this.position = pos;
        this.isWalkable = true;
        this.terrain = TerrainType.Plain; // Valeur par défaut (peut être modifiée ensuite)
        this.tilemap = tilemap;
        this.cellPosition = new Vector3Int(pos.x, pos.y, 0);
        this.resources = 0;
    }

    public void ResetHighlight()
    {
        if (tilemap != null)
        {
            tilemap.SetColor(cellPosition, Color.white); // Rétablit la couleur d'origine
        }
    }
}






