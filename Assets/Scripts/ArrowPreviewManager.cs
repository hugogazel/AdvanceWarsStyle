using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ArrowPreviewManager : MonoBehaviour
{
    [Header("Heads")]
    public GameObject arrowHeadUpPrefab;
    public GameObject arrowHeadDownPrefab;
    public GameObject arrowHeadLeftPrefab;
    public GameObject arrowHeadRightPrefab;

    [Header("Segments")]
    public GameObject arrowHorizontalPrefab;
    public GameObject arrowVerticalPrefab;

    [Header("Corners (NW, NE, SW, SE)")]
    public GameObject arrowCornerNEPrefab;
    public GameObject arrowCornerNWPrefab;
    public GameObject arrowCornerSEPrefab;
    public GameObject arrowCornerSWPrefab;

    [Header("References")]
    public Tilemap groundTilemap;
    public GridManager gridManager; // Pour checker highlightedTiles

    private List<GameObject> currentArrows = new List<GameObject>();

    /// <summary>
    /// Affiche la trajectoire calculée (liste de GridCell) : segments ou coins sur [1..n-2], tête sur path[n-1].
    /// </summary>
    public void ShowPathPreview(List<GridCell> path)
    {
        ClearPathPreview();
        if (path == null || path.Count < 2) return;

        int n = path.Count;

        // Parcourir les cellules intermédiaires
        for (int i = 1; i < n - 1; i++)
        {
            if (!IsCellReachable(path[i])) continue;

            Vector2Int dIn = path[i].position - path[i - 1].position;
            Vector2Int dOut = path[i + 1].position - path[i].position;

            dIn = QuantizeDirection(dIn);
            dOut = QuantizeDirection(dOut);

            Vector3 worldPos = groundTilemap.GetCellCenterWorld(
                new Vector3Int(path[i].position.x, path[i].position.y, 0)
            );

            Debug.Log($"[Segment/Corner] Cell {i} at {path[i].position}: dIn={dIn}, dOut={dOut}");

            if (dIn != dOut)
            {
                // Virage
                GameObject corner = DetermineCornerPrefab(dIn, dOut);
                if (corner != null)
                {
                    currentArrows.Add(Instantiate(corner, worldPos, Quaternion.identity));
                }
            }
            else
            {
                // Segment rectiligne
                if (dIn.x != 0 && dIn.y == 0)
                    currentArrows.Add(Instantiate(arrowHorizontalPrefab, worldPos, Quaternion.identity));
                else if (dIn.y != 0 && dIn.x == 0)
                    currentArrows.Add(Instantiate(arrowVerticalPrefab, worldPos, Quaternion.identity));
            }
        }

        // Placer la tête sur la dernière cellule
        if (IsCellReachable(path[n - 1]))
        {
            Vector2Int lastDelta = path[n - 1].position - path[n - 2].position;
            lastDelta = QuantizeDirection(lastDelta);

            Debug.Log($"[Head] path[{n - 1}] at {path[n - 1].position}, lastDelta={lastDelta}");
            GameObject headPrefab = DetermineHeadPrefab(lastDelta);

            Vector3 headPos = groundTilemap.GetCellCenterWorld(
                new Vector3Int(path[n - 1].position.x, path[n - 1].position.y, 0)
            );
            currentArrows.Add(Instantiate(headPrefab, headPos, Quaternion.identity));
        }
    }

    private bool IsCellReachable(GridCell cell)
    {
        return gridManager.highlightedTiles.Contains(cell);
    }

    private Vector2Int QuantizeDirection(Vector2Int vec)
    {
        int x = Mathf.Clamp(vec.x, -1, 1);
        int y = Mathf.Clamp(vec.y, -1, 1);
        return new Vector2Int(x, y);
    }

    private GameObject DetermineHeadPrefab(Vector2Int delta)
    {
        if (delta == new Vector2Int(1, 0))
        {
            Debug.Log("=> Head: arrowHeadRightPrefab");
            return arrowHeadRightPrefab;
        }
        if (delta == new Vector2Int(-1, 0))
        {
            Debug.Log("=> Head: arrowHeadLeftPrefab");
            return arrowHeadLeftPrefab;
        }
        if (delta == new Vector2Int(0, 1))
        {
            Debug.Log("=> Head: arrowHeadUpPrefab");
            return arrowHeadUpPrefab;
        }
        if (delta == new Vector2Int(0, -1))
        {
            Debug.Log("=> Head: arrowHeadDownPrefab");
            return arrowHeadDownPrefab;
        }
        Debug.Log("=> Head: fallback arrowHeadUpPrefab");
        return arrowHeadUpPrefab;
    }

    /// <summary>
    /// Inversion globale : NW↔SE, NE↔SW
    /// </summary>
    private GameObject DetermineCornerPrefab(Vector2Int d1, Vector2Int d2)
    {
        Debug.Log($"DetermineCornerPrefab: d1={d1}, d2={d2}");

        // Inversion NW <-> SE, NE <-> SW

        // 1) Cas (1,0)->(0,1) => Au lieu de NW, on renvoie SE
        if (d1 == new Vector2Int(1, 0) && d2 == new Vector2Int(0, 1))
        {
            Debug.Log("=> corner SE (inversion NW->SE)");
            return arrowCornerSEPrefab;
        }
        // 2) (1,0)->(0,-1) => Au lieu de SW, on renvoie NE
        if (d1 == new Vector2Int(1, 0) && d2 == new Vector2Int(0, -1))
        {
            Debug.Log("=> corner NE (inversion SW->NE)");
            return arrowCornerNEPrefab;
        }
        // 3) (-1,0)->(0,1) => Au lieu de NE, on renvoie SW
        if (d1 == new Vector2Int(-1, 0) && d2 == new Vector2Int(0, 1))
        {
            Debug.Log("=> corner SW (inversion NE->SW)");
            return arrowCornerSWPrefab;
        }
        // 4) (-1,0)->(0,-1) => Au lieu de SE, on renvoie NW
        if (d1 == new Vector2Int(-1, 0) && d2 == new Vector2Int(0, -1))
        {
            Debug.Log("=> corner NW (inversion SE->NW)");
            return arrowCornerNWPrefab;
        }

        // 5) (0,1)->(1,0) => Au lieu de NW, on renvoie SE
        if (d1 == new Vector2Int(0, 1) && d2 == new Vector2Int(1, 0))
        {
            Debug.Log("=> corner SE (inversion NW->SE)");
            return arrowCornerSEPrefab;
        }
        // 6) (0,1)->(-1,0) => Au lieu de NE, on renvoie SW
        if (d1 == new Vector2Int(0, 1) && d2 == new Vector2Int(-1, 0))
        {
            Debug.Log("=> corner SW (inversion NE->SW)");
            return arrowCornerSWPrefab;
        }
        // 7) (0,-1)->(1,0) => Au lieu de SW, on renvoie NE
        if (d1 == new Vector2Int(0, -1) && d2 == new Vector2Int(1, 0))
        {
            Debug.Log("=> corner NE (inversion SW->NE)");
            return arrowCornerNEPrefab;
        }
        // 8) (0,-1)->(-1,0) => Au lieu de SE, on renvoie NW
        if (d1 == new Vector2Int(0, -1) && d2 == new Vector2Int(-1, 0))
        {
            Debug.Log("=> corner NW (inversion SE->NW)");
            return arrowCornerNWPrefab;
        }

        // Fallback
        Debug.Log("=> fallback: arrowHorizontalPrefab");
        return arrowHorizontalPrefab;
    }

    public void ClearPathPreview()
    {
        foreach (var arrow in currentArrows)
        {
            Destroy(arrow);
        }
        currentArrows.Clear();
    }
}







