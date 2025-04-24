using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSpawnDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Nom exact de la layer pour les unités")]
    public string unitLayerName = "Unit";

    private Camera mainCamera;
    private GridManager gridManager;

    private GameObject prefabToSpawn;
    private GameObject spawnedUnit;
    private bool dragging = false;

    void Awake()
    {
        if (UnityEngine.SceneManagement.SceneManager
            .GetActiveScene().name != "FirstMapScene")
        {
            enabled = false;
            return;
        }
        mainCamera = Camera.main;
        gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null)
            Debug.LogError("GridManager non trouvé !");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var ui = GetComponent<UnitCardUI>();
        if (ui == null || ui.unitPrefab == null)
            return;

        prefabToSpawn = ui.unitPrefab;
        spawnedUnit = Instantiate(prefabToSpawn);
        if (spawnedUnit.TryGetComponent<UnitController>(out var uc))
            uc.team = TurnManager.Instance.currentTeam;

        dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || spawnedUnit == null) return;

        Vector3 screenPos = eventData.position;
        screenPos.z = -mainCamera.transform.position.z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        var cell = gridManager.groundTilemap.WorldToCell(worldPos);
        var center = gridManager.groundTilemap.GetCellCenterWorld(cell);
        spawnedUnit.transform.position = center;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;
        if (spawnedUnit == null || gridManager == null) return;

        // Calcul de la cellule cible
        Vector3 screenPos = eventData.position;
        screenPos.z = -mainCamera.transform.position.z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;
        var cell = gridManager.groundTilemap.WorldToCell(worldPos);
        var gridPos = new Vector2Int(cell.x, cell.y);

        if (gridManager.IsTileReachable(gridPos))
        {
            // Snap & position interne
            Vector3 center = gridManager.groundTilemap.GetCellCenterWorld(cell);
            spawnedUnit.transform.position = center;

            // On met juste à jour la position interne de l'unité…
            if (spawnedUnit.TryGetComponent<UnitController>(out var uc))
            {
                uc.position = gridPos;
                // → **PLUS DE** gridManager.selectedUnit ni HighlightReachableTiles
            }

            // Assignation de la layer "Unit"
            int layer = LayerMask.NameToLayer(unitLayerName);
            if (layer < 0)
            {
                Debug.LogWarning($"Layer '{unitLayerName}' introuvable, passe en Default.");
                layer = LayerMask.NameToLayer("Default");
            }
            foreach (var tr in spawnedUnit.GetComponentsInChildren<Transform>(true))
                tr.gameObject.layer = layer;

            // Ajout d'un collider si besoin
            if (spawnedUnit.GetComponentInChildren<Collider2D>() == null)
                spawnedUnit.AddComponent<CircleCollider2D>();

            // Suppression de la carte UI et du data du deck
            if (eventData.pointerDrag != null)
            {
                var draggedUi = eventData.pointerDrag.GetComponent<UnitCardUI>();
                if (draggedUi != null)
                {
                    var tm = TurnManager.Instance;
                    if (tm.currentTeam == Team.J1Team)
                        tm.player1Deck.Remove(draggedUi.unitData);
                    else
                        tm.player2Deck.Remove(draggedUi.unitData);
                }
                Destroy(eventData.pointerDrag);
            }
        }
        else
        {
            // Drop invalide → on détruit
            Destroy(spawnedUnit);
        }

        spawnedUnit = null;
    }
}














