using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UnitSpawnDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Nom exact de la layer pour les unités")]
    public string unitLayerName = "Unit";

    [Header("Zone morte (cases Manhattan) autour des ennemis, free-drop initial")]
    public int freeDropDeadZone = 5;

    private Camera mainCamera;
    private GridManager gridManager;
    private GameObject spawnedUnit;
    private UnitController previewController;    // <--- nouvelle référence
    private bool dragging = false;
    private bool wasFirstEverDrop = false;

    // Liste pré-calculée des cases valides pendant le drag
    private List<Vector2Int> validSpawnPositions = new List<Vector2Int>();

    private static readonly Vector2Int[] directions4 = {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };

    void Awake()
    {
        if (SceneManager.GetActiveScene().name != "FirstMapScene")
        {
            enabled = false;
            return;
        }

        mainCamera = Camera.main;
        gridManager = FindObjectOfType<GridManager>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        gridManager.ClearHighlightedTiles();

        if (TurnManager.Instance.hasDeployedThisTurn)
            return;

        wasFirstEverDrop = !FindObjectsOfType<UnitController>()
            .Any(u => u.team == TurnManager.Instance.currentTeam);

        // ——— Instanciation de la preview ———
        var ui = GetComponent<UnitCardUI>();
        if (ui == null || ui.unitPrefab == null)
            return;

        spawnedUnit = Instantiate(ui.unitPrefab);
        if (spawnedUnit.TryGetComponent<UnitController>(out var uc))
        {
            uc.team = TurnManager.Instance.currentTeam;
            uc.unitData = ui.unitData;
            previewController = uc;    // <--- on conserve la référence
        }

        // ——— Construction de la liste des cases valides ———
        validSpawnPositions.Clear();
        var tm = TurnManager.Instance;
        bool freeAllowed = wasFirstEverDrop &&
            ((tm.currentTeam == Team.J1Team && !tm.j1FreeDropUsed) ||
             (tm.currentTeam == Team.J2Team && !tm.j2FreeDropUsed));

        if (freeAllowed)
        {
            // Free-drop : toutes les cases libres hors zone morte
            var enemies = FindObjectsOfType<UnitController>()
                .Where(u => u.team != tm.currentTeam)
                .ToList();

            var bounds = gridManager.groundTilemap.cellBounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (!gridManager.IsTileReachable(pos) ||
                        gridManager.GetUnitAtPosition(pos) != null)
                        continue;

                    bool inDeadZone = enemies.Any(u =>
                        Mathf.Abs(u.position.x - pos.x) +
                        Mathf.Abs(u.position.y - pos.y)
                        <= freeDropDeadZone);

                    if (!inDeadZone)
                        validSpawnPositions.Add(pos);
                }

            gridManager.HighlightInvalidDeployTiles(
                tm.currentTeam,
                freeDropDeadZone
            );
        }
        else
        {
            // ——— Adjacency-only, en excluant la preview ———
            var ownUnits = FindObjectsOfType<UnitController>()
                .Where(u =>
                    u.team == tm.currentTeam &&
                    !u.hasActed &&
                    u != previewController         // <--- ici on exclut la preview
                )
                .ToList();

            validSpawnPositions = ownUnits
                .SelectMany(u => directions4.Select(d => u.position + d))
                .Distinct()
                .Where(pos =>
                    gridManager.IsTileReachable(pos) &&
                    gridManager.GetUnitAtPosition(pos) == null
                )
                .ToList();

            var highlightCells = validSpawnPositions
                .Select(p => gridManager.GetCell(p))
                .Where(c => c != null)
                .ToList();

            gridManager.HighlightReachableTiles(
                highlightCells,
                gridManager.defaultHighlightColor
            );
        }

        dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || spawnedUnit == null)
            return;

        Vector3 screenPos = new Vector3(
            eventData.position.x,
            eventData.position.y,
            -mainCamera.transform.position.z
        );
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        var cell = gridManager.groundTilemap.WorldToCell(worldPos);
        spawnedUnit.transform.position =
            gridManager.groundTilemap.GetCellCenterWorld(cell);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging || spawnedUnit == null)
            return;
        dragging = false;

        // Si déjà déployé ce tour, on annule direct
        if (TurnManager.Instance.hasDeployedThisTurn)
        {
            Destroy(spawnedUnit);
            gridManager.ClearHighlightedTiles();
            previewController = null;
            return;
        }

        // Calcul de la case de drop
        Vector3 screenPos = new Vector3(
            eventData.position.x,
            eventData.position.y,
            -mainCamera.transform.position.z
        );
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        Vector3Int cell3 = gridManager.groundTilemap.WorldToCell(worldPos);
        Vector2Int gridPos = new Vector2Int(cell3.x, cell3.y);

        if (validSpawnPositions.Contains(gridPos))
        {
            // Snap final
            spawnedUnit.transform.position =
                gridManager.groundTilemap.GetCellCenterWorld(cell3);

            // Positionne et GRİSE l'unité en même temps
            if (spawnedUnit.TryGetComponent<UnitController>(out var uc2))
            {
                uc2.position = gridPos;
                uc2.MarkAsWaiting();    // ← remplace uc2.hasActed = true + gère la couleur grisée
            }

            ApplyLayerAndCollider();

            // Suppression de la carte UI et retrait du deck
            if (eventData.pointerDrag != null)
            {
                var draggedUi = eventData.pointerDrag.GetComponent<UnitCardUI>();
                if (draggedUi != null)
                {
                    if (TurnManager.Instance.currentTeam == Team.J1Team)
                        TurnManager.Instance.player1Deck.Remove(draggedUi.unitData);
                    else
                        TurnManager.Instance.player2Deck.Remove(draggedUi.unitData);
                }
                Destroy(eventData.pointerDrag);
            }

            // Maj du TurnManager
            TurnManager.Instance.hasDeployedThisTurn = true;
            if (wasFirstEverDrop)
            {
                if (TurnManager.Instance.currentTeam == Team.J1Team)
                    TurnManager.Instance.j1FreeDropUsed = true;
                else
                    TurnManager.Instance.j2FreeDropUsed = true;
            }
        }
        else
        {
            Destroy(spawnedUnit);
        }

        // Cleanup
        spawnedUnit = null;
        previewController = null;
        gridManager.ClearHighlightedTiles();
    }



    private void ApplyLayerAndCollider()
    {
        int layer = LayerMask.NameToLayer(unitLayerName);
        if (layer < 0)
            layer = LayerMask.NameToLayer("Default");
        foreach (var t in spawnedUnit.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
        if (spawnedUnit.GetComponentInChildren<Collider2D>() == null)
            spawnedUnit.AddComponent<CircleCollider2D>();
    }
}
































