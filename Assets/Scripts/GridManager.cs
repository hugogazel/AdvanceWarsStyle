using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Tilemap environmentTilemap;
    public Tilemap obstacleTilemap;
    public Tilemap highlightTilemap; // Tilemap dédiée au surlignage
    public AnimatedTile highlightAnimatedTile;
    public List<UnitData> availableUnits;
    public UnitActionPanel actionPanel;
    public Color defaultHighlightColor = new Color(0f, 0f, 1f, 0.3f);

    [Header("Highlight pour déploiement")]
    public Color invalidDeployColor = new Color(0f, 0f, 0f, 0.5f);

    // Référence au MapDataManager pour récupérer la config des tuiles
    public MapDataManager mapDataManager;

    private Dictionary<Vector2Int, GridCell> grid = new Dictionary<Vector2Int, GridCell>();
    public UnitController selectedUnit;
    public List<GridCell> highlightedTiles = new List<GridCell>();

    void Start()
    {
        InitializeGrid();
    }

    void InitializeGrid()
    {
        BoundsInt bounds = groundTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (groundTilemap.HasTile(cellPosition))
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    GridCell newCell = new GridCell(gridPos, groundTilemap);

                    // Détermine si la case est marchable (non bloquée par un obstacle)
                    newCell.isWalkable = !obstacleTilemap.HasTile(cellPosition);

                    int totalResources = 0;
                    TerrainType terrain = TerrainType.Plain;

                    // Lire les données de la groundTilemap
                    if (groundTilemap.HasTile(cellPosition))
                    {
                        TileBase groundTile = groundTilemap.GetTile(cellPosition);
                        VariantTileData groundData = mapDataManager.GetTileData(groundTile);
                        if (groundData != null)
                        {
                            totalResources += groundData.resources;
                            terrain = TerrainType.Plain;
                        }
                    }

                    // Lire les données de l'environmentTilemap
                    if (environmentTilemap != null && environmentTilemap.HasTile(cellPosition))
                    {
                        TileBase envTile = environmentTilemap.GetTile(cellPosition);
                        VariantTileData envData = mapDataManager.GetTileData(envTile);
                        if (envData != null)
                        {
                            totalResources += envData.resources;
                            string terrainName = envData.terrainName.ToLower();
                            if (terrainName.Contains("forest"))
                                terrain = TerrainType.Forest;
                            else if (terrainName.Contains("mountain"))
                                terrain = TerrainType.Mountain;
                            else if (terrainName.Contains("water"))
                                terrain = TerrainType.Water;
                            else
                                terrain = TerrainType.Plain;
                        }
                        else
                        {
                            terrain = TerrainType.Plain;
                        }
                    }

                    newCell.terrain = terrain;
                    newCell.resources = totalResources;

                    grid[gridPos] = newCell;
                }
            }
        }
    }

    // -------------------------------
    // PATHFINDING (Dijkstra) prenant en compte les carcasses traversables
    // -------------------------------
    public List<GridCell> FindPath(UnitController mover, Vector2Int start, Vector2Int target)
    {
        if (!grid.ContainsKey(start) || !grid.ContainsKey(target))
        {
            Debug.Log("❌ Point de départ ou d'arrivée invalide.");
            return new List<GridCell>();
        }

        GridCell startCell = grid[start];
        GridCell targetCell = grid[target];

        var cameFrom = new Dictionary<GridCell, GridCell>();
        var costSoFar = new Dictionary<GridCell, int>();
        var frontier = new List<KeyValuePair<GridCell, int>> { new KeyValuePair<GridCell, int>(startCell, 0) };

        cameFrom[startCell] = null;
        costSoFar[startCell] = 0;

        while (frontier.Count > 0)
        {
            frontier.Sort((a, b) => a.Value.CompareTo(b.Value));
            var currentPair = frontier[0];
            frontier.RemoveAt(0);
            GridCell current = currentPair.Key;

            if (current == targetCell)
                break;

            foreach (GridCell neighbor in GetNeighbors(current.position))
            {
                if (!neighbor.isWalkable)
                    continue;

                var occupyingUnit = GetUnitAtPosition(neighbor.position);
                if (occupyingUnit != null && occupyingUnit != mover && !mover.IsAllied(occupyingUnit))
                    continue;

                var carcassAtNeighbor = GetCarcassAtPosition(neighbor.position);
                if (carcassAtNeighbor != null && !carcassAtNeighbor.isTraversable)
                    continue;

                int newCost = costSoFar[current] + GetMovementCost(neighbor);
                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    frontier.Add(new KeyValuePair<GridCell, int>(neighbor, newCost));
                    cameFrom[neighbor] = current;
                }
            }
        }

        var path = new List<GridCell>();
        if (!cameFrom.ContainsKey(targetCell))
        {
            Debug.Log("❌ Aucun chemin valide trouvé !");
            return path;
        }

        for (GridCell pathCell = targetCell; pathCell != null; pathCell = cameFrom[pathCell])
            path.Insert(0, pathCell);

        Debug.Log("📏 Longueur du chemin : " + path.Count);
        return path;
    }

    // -------------------------------
    // CALCUL DES CASES ACCESSIBLES (ComputeReachableTiles)
    // -------------------------------
    public List<GridCell> ComputeReachableTiles(UnitController unit, Vector2Int startPos)
    {
        var reachableTiles = new List<GridCell>();
        var queue = new Queue<GridCell>();
        var moveCost = new Dictionary<GridCell, int>();

        int maxRange = unit.unitData.maxMovePoints;
        if (!grid.ContainsKey(startPos))
            return reachableTiles;

        GridCell startCell = grid[startPos];
        queue.Enqueue(startCell);
        moveCost[startCell] = 0;

        while (queue.Count > 0)
        {
            GridCell current = queue.Dequeue();
            foreach (GridCell neighbor in GetNeighbors(current.position))
            {
                if (!neighbor.isWalkable)
                    continue;

                var occ = GetUnitAtPosition(neighbor.position);
                if (occ != null && occ != unit && !unit.IsAllied(occ))
                    continue;

                var carcassAtNeighbor = GetCarcassAtPosition(neighbor.position);
                if (carcassAtNeighbor != null && !carcassAtNeighbor.isTraversable)
                    continue;

                int tileCost = GetMovementCost(neighbor);
                int newCost = moveCost[current] + tileCost;

                if (newCost <= maxRange)
                {
                    if (!moveCost.ContainsKey(neighbor) || newCost < moveCost[neighbor])
                    {
                        moveCost[neighbor] = newCost;
                        queue.Enqueue(neighbor);
                        if (!reachableTiles.Contains(neighbor))
                            reachableTiles.Add(neighbor);
                    }
                }
            }
        }
        return reachableTiles;
    }

    private int GetMovementCost(GridCell cell)
    {
        Vector3Int cellPos = new Vector3Int(cell.position.x, cell.position.y, 0);
        if (environmentTilemap != null && environmentTilemap.HasTile(cellPos))
        {
            var envTile = environmentTilemap.GetTile(cellPos);
            var data = mapDataManager.GetTileData(envTile);
            if (data != null)
                return data.movementCost;
        }

        switch (cell.terrain)
        {
            case TerrainType.Plain: return 1;
            case TerrainType.Forest: return 2;
            case TerrainType.Mountain: return 2;
            case TerrainType.Water: return 999;
            default: return 1;
        }
    }

    // ----------------------
    // Méthodes utilitaires
    // ----------------------
    public GridCell GetCell(Vector2Int position) =>
        grid.ContainsKey(position) ? grid[position] : null;

    public List<GridCell> GetNeighbors(Vector2Int position)
    {
        var neighbors = new List<GridCell>();
        var dirs = new[] {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };
        foreach (var d in dirs)
            if (grid.ContainsKey(position + d))
                neighbors.Add(grid[position + d]);
        return neighbors;
    }

    public UnitController GetUnitAtPosition(Vector2Int position)
    {
        foreach (var unit in FindObjectsOfType<UnitController>())
            if (unit.position == position)
                return unit;
        return null;
    }

    public void HighlightReachableTiles(List<GridCell> tiles, Color customColor)
    {
        highlightedTiles = tiles;
        foreach (var cell in tiles)
        {
            var pos = new Vector3Int(cell.position.x, cell.position.y, 0);
            highlightTilemap.SetTile(pos, highlightAnimatedTile);
        }
    }

    public void ClearHighlightedTiles()
    {
        highlightTilemap.ClearAllTiles();
        highlightedTiles.Clear();
    }

    public bool IsTileReachable(Vector2Int position) =>
        grid.ContainsKey(position) && grid[position].isWalkable;

    // ----------------------------------
    // ZONE MORTE POUR LE PREMIER DROP
    // ----------------------------------

    /// <summary>
    /// Grise toutes les cases dans un rayon de Manhattan ≤ deadZone
    /// autour de chaque unité ennemie du team donné.
    /// </summary>
    public void HighlightInvalidDeployTiles(Team team, int deadZone)
    {
        ClearHighlightedTiles();
        foreach (var cell in grid.Values)
        {
            if (!cell.isWalkable) continue;

            bool inDeadZone = FindObjectsOfType<UnitController>()
                .Where(u => u.team != team)
                .Any(u =>
                    Mathf.Abs(u.position.x - cell.position.x)
                  + Mathf.Abs(u.position.y - cell.position.y)
                  <= deadZone
                );

            if (inDeadZone)
            {
                highlightTilemap.SetTile(cell.cellPosition, highlightAnimatedTile);
                highlightTilemap.SetColor(cell.cellPosition, invalidDeployColor);
            }
        }
    }

    /// <summary>
    /// Supprime tous les surlignages de déploiement.
    /// </summary>
    public void ClearInvalidDeployHighlights()
    {
        ClearHighlightedTiles();
    }

    public void ConsumeTileResources(Vector2Int cellPosition, int amount)
    {
        if (!grid.ContainsKey(cellPosition)) return;
        var cell = grid[cellPosition];
        cell.resources = Mathf.Max(0, cell.resources - amount);
        if (cell.resources == 0)
            groundTilemap.RefreshTile(cell.cellPosition);
    }

    public Carcass GetCarcassAtPosition(Vector2Int position)
    {
        foreach (var carc in FindObjectsOfType<Carcass>())
        {
            var cellPos = groundTilemap.WorldToCell(carc.transform.position);
            if (cellPos.x == position.x && cellPos.y == position.y)
                return carc;
        }
        return null;
    }

    public List<GridCell> GetAllCells() =>
        new List<GridCell>(grid.Values);
}


















