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

    private GameObject prefabToSpawn;
    private GameObject spawnedUnit;
    private bool dragging = false;

    // Pour savoir si c’était le tout premier drop du joueur
    private bool wasFirstEverDrop = false;

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
        // 1) Toujours vider les anciens surlignages
        gridManager.ClearHighlightedTiles();

        // 2) Bloquer si on a déjà déployé ce tour
        if (TurnManager.Instance.hasDeployedThisTurn)
            return;

        // 3) Déterminer si c'est le 1er drop EVER
        wasFirstEverDrop = !FindObjectsOfType<UnitController>()
            .Any(u => u.team == TurnManager.Instance.currentTeam);

        // 4) Instancier la preview
        var ui = GetComponent<UnitCardUI>();
        if (ui == null || ui.unitPrefab == null)
            return;
        prefabToSpawn = ui.unitPrefab;
        spawnedUnit = Instantiate(prefabToSpawn);
        if (spawnedUnit.TryGetComponent<UnitController>(out var uc))
        {
            uc.team = TurnManager.Instance.currentTeam;
            uc.unitData = ui.unitData;
        }

        // 5) Choisir le surlignage
        bool freeAllowed = wasFirstEverDrop
            && (TurnManager.Instance.currentTeam == Team.J1Team
                ? !TurnManager.Instance.j1FreeDropUsed
                : !TurnManager.Instance.j2FreeDropUsed);

        if (freeAllowed)
        {
            // zone morte autour des ennemis
            gridManager.HighlightInvalidDeployTiles(
                TurnManager.Instance.currentTeam,
                freeDropDeadZone
            );
        }
        else
        {
            // adjacency-only, exclut la preview et toute unité (alliée ou non)
            var allies = FindObjectsOfType<UnitController>()
                           .Where(u => u.gameObject != spawnedUnit
                                    && u.team == TurnManager.Instance.currentTeam
                                    && !u.hasActed);

            var valid = allies
                .SelectMany(u => directions4.Select(d => u.position + d))
                .Distinct()
                .Where(pos => gridManager.IsTileReachable(pos))
                // *** NOUVEAU : exclure les tuiles occupées ***
                .Where(pos => gridManager.GetUnitAtPosition(pos) == null)
                .Select(pos => gridManager.GetCell(pos))
                .Where(cell => cell != null)
                .ToList();

            gridManager.HighlightReachableTiles(valid, gridManager.defaultHighlightColor);
        }

        dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || spawnedUnit == null) return;

        Vector3 screenPos = new Vector3(
            eventData.position.x,
            eventData.position.y,
            -mainCamera.transform.position.z
        );
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        var cell = gridManager.groundTilemap.WorldToCell(worldPos);
        var center = gridManager.groundTilemap.GetCellCenterWorld(cell);
        spawnedUnit.transform.position = center;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        gridManager.ClearHighlightedTiles();

        if (!dragging || spawnedUnit == null)
            return;
        dragging = false;

        if (TurnManager.Instance.hasDeployedThisTurn)
        {
            Destroy(spawnedUnit);
            spawnedUnit = null;
            return;
        }

        Vector3 screenPos = new Vector3(
            eventData.position.x,
            eventData.position.y,
            -mainCamera.transform.position.z
        );
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        Vector3Int cell3 = gridManager.groundTilemap.WorldToCell(worldPos);
        Vector2Int gridPos = new Vector2Int(cell3.x, cell3.y);

        if (gridManager.IsTileReachable(gridPos)
            && CanDeployAt(gridPos, wasFirstEverDrop))
        {
            Vector3 center = gridManager.groundTilemap.GetCellCenterWorld(cell3);
            spawnedUnit.transform.position = center;

            if (spawnedUnit.TryGetComponent<UnitController>(out var uc))
            {
                uc.position = gridPos;
                uc.MarkAsWaiting();
            }

            ApplyLayerAndCollider();
            RemoveDraggedCard(eventData);

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

        spawnedUnit = null;
    }

    private bool CanDeployAt(Vector2Int pos, bool isFirstEver)
    {
        var tm = TurnManager.Instance;
        if (isFirstEver)
        {
            bool freeAllowed = tm.currentTeam == Team.J1Team
                ? !tm.j1FreeDropUsed
                : !tm.j2FreeDropUsed;
            if (freeAllowed)
            {
                bool blocked = FindObjectsOfType<UnitController>()
                    .Where(u => u.team != tm.currentTeam)
                    .Any(u =>
                        Mathf.Abs(u.position.x - pos.x) +
                        Mathf.Abs(u.position.y - pos.y)
                        <= freeDropDeadZone
                    );
                if (!blocked) return true;
            }
        }
        // adjacency-only
        return directions4.Any(d =>
        {
            var nb = pos + d;
            var u = gridManager.GetUnitAtPosition(nb);
            return u != null && u.team == tm.currentTeam && !u.hasActed;
        });
    }

    private void ApplyLayerAndCollider()
    {
        int layer = LayerMask.NameToLayer(unitLayerName);
        if (layer < 0) layer = LayerMask.NameToLayer("Default");
        foreach (var t in spawnedUnit.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
        if (spawnedUnit.GetComponentInChildren<Collider2D>() == null)
            spawnedUnit.AddComponent<CircleCollider2D>();
    }

    private void RemoveDraggedCard(PointerEventData eventData)
    {
        var drag = eventData.pointerDrag;
        if (drag == null) return;
        var ui = drag.GetComponent<UnitCardUI>();
        if (ui != null)
        {
            if (TurnManager.Instance.currentTeam == Team.J1Team)
                TurnManager.Instance.player1Deck.Remove(ui.unitData);
            else
                TurnManager.Instance.player2Deck.Remove(ui.unitData);
        }
        Destroy(drag);
    }
}






























