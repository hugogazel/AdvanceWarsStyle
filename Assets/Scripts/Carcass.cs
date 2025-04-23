using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Carcass : MonoBehaviour
{
    // Valeur actuelle (restante) de la carcasse
    public int fillValue;
    // Valeur initiale (pour calculer la portion à consommer, ici 1/3)
    public int maxFillValue;
    // Position sur la grille, mise à jour lors du snap
    public Vector2Int gridPosition;
    // Indique que la carcasse est traversable (comme des unités alliées)
    public bool isTraversable = true;

/// Aligne (snap) la carcasse sur la grille en utilisant la groundTilemap.
public void SnapToGrid()
{
    GridManager gridManager = FindObjectOfType<GridManager>();
    if (gridManager != null && gridManager.groundTilemap != null)
    {
        // Obtenir la position de la cellule correspondant à la position actuelle
        Vector3Int cellPos = gridManager.groundTilemap.WorldToCell(transform.position);
        // Calculer la position centrée de la cellule
        Vector3 snappedPosition = gridManager.groundTilemap.GetCellCenterWorld(cellPos);
        transform.position = snappedPosition;
        gridPosition = new Vector2Int(cellPos.x, cellPos.y);
    }
    else
    {
        Debug.LogWarning("GridManager ou groundTilemap non trouvé dans Carcass.SnapToGrid()");
    }
}

private void Start()
{
    SnapToGrid();
}
}

