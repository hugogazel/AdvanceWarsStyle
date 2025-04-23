using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class FogOfWarManager : MonoBehaviour
{
    public static FogOfWarManager Instance { get; private set; }

    [Header("Références Tilemaps")]
    public Tilemap groundTilemap;   // la Tilemap du sol
    public Tilemap fogTilemap;      // la Tilemap du brouillard (au-dessus)

    [Header("Tile de brouillard")]
    public TileBase fogTile;        // tuile sombre (opaque partiellement)

    private HashSet<Vector2Int> visibleCells = new HashSet<Vector2Int>();

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        // Au démarrage, on couvre tout
        CoverAll();
    }

    /// <summary>
    /// Recouvre entièrement la carte de brouillard
    /// </summary>
    public void CoverAll()
    {
        fogTilemap.ClearAllTiles();
        var bounds = groundTilemap.cellBounds;
        foreach (var pos in bounds.allPositionsWithin)
        {
            if (groundTilemap.HasTile(pos))
                fogTilemap.SetTile(pos, fogTile);
        }
    }

    /// <summary>
    /// Met à jour le brouillard en fonction des unités amies
    /// </summary>
    /// <param name="friendlyUnits">liste des unités de ton équipe</param>
    public void UpdateFog(IEnumerable<UnitController> friendlyUnits)
    {
        // 1. Recouvre tout
        CoverAll();
        visibleCells.Clear();

        // 2. Pour chaque unité, découvre les cases dans son champ de vision
        foreach (var unit in friendlyUnits)
        {
            var centre = new Vector2Int(unit.position.x, unit.position.y);
            var range = unit.unitData.visionRange;
            foreach (var cell in ComputeVisionCells(centre, range))
            {
                visibleCells.Add(cell);
                var tilePos = new Vector3Int(cell.x, cell.y, 0);
                fogTilemap.SetTile(tilePos, null);
            }
        }

        // 3. Masquer/afficher les unités ennemies
        foreach (var enemy in FindObjectsOfType<UnitController>()
                     .Where(u => u.team != friendlyUnits.First().team))
        {
            bool visible = visibleCells.Contains(enemy.position);
            enemy.gameObject.SetActive(visible);
        }
    }

    /// <summary>
    /// Renvoie la liste des positions de cases visibles (Manhattan) autour de centre
    /// </summary>
    private IEnumerable<Vector2Int> ComputeVisionCells(Vector2Int centre, int range)
    {
        var cells = new List<Vector2Int>();
        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                // option Manhattan ou Euclidienne :
                if (Mathf.Abs(dx) + Mathf.Abs(dy) <= range)
                {
                    var c = centre + new Vector2Int(dx, dy);
                    if (groundTilemap.HasTile(new Vector3Int(c.x, c.y, 0)))
                        cells.Add(c);
                }
            }
        }
        return cells;
    }
}

