using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSpawnDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Layer for spawned units (must match unitLayerMask in GameInputManager)")]
    public string unitLayerName = "Units";

    private Camera mainCamera;
    private GridManager gridManager;

    // Prefab and spawned instance
    private GameObject prefabToSpawn;
    private GameObject spawnedUnit;
    private bool dragging = false;

    void Awake()
    {
        // N’active ce script que dans FirstMapScene
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
        // Récupère l'UI et l'unitPrefab
        var ui = GetComponent<UnitCardUI>();
        if (ui == null || ui.unitPrefab == null)
            return;

        prefabToSpawn = ui.unitPrefab;
        spawnedUnit = Instantiate(prefabToSpawn);
        // Définir l'équipe
        if (spawnedUnit.TryGetComponent<UnitController>(out var uc))
            uc.team = TurnManager.Instance.currentTeam;

        dragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || spawnedUnit == null) return;

        // Déplace l'instance unit selon la souris
        Vector3 screenPos = eventData.position;
        screenPos.z = -mainCamera.transform.position.z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        var cell = gridManager.groundTilemap.WorldToCell(worldPos);
        Vector3 center = gridManager.groundTilemap.GetCellCenterWorld(cell);
        spawnedUnit.transform.position = center;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;
        if (spawnedUnit == null || gridManager == null) return;

        // Calcule la cellule de drop
        Vector3 screenPos = eventData.position;
        screenPos.z = -mainCamera.transform.position.z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        var cell = gridManager.groundTilemap.WorldToCell(worldPos);
        Vector2Int gridPos = new Vector2Int(cell.x, cell.y);

        bool valid = gridManager.IsTileReachable(gridPos);
        if (valid)
        {
            // Snap sur la cellule
            Vector3 center = gridManager.groundTilemap.GetCellCenterWorld(cell);
            spawnedUnit.transform.position = center;
            if (spawnedUnit.TryGetComponent<UnitController>(out var uc))
                uc.position = gridPos;

            // Assigne la bonne layer à l'unité (et ses enfants)
            int unitLayer = LayerMask.NameToLayer(unitLayerName);
            EnsureCorrectLayer(spawnedUnit, unitLayer);

            // Ajoute un Collider2D si nécessaire
            if (spawnedUnit.GetComponentInChildren<Collider2D>() == null)
                spawnedUnit.AddComponent<CircleCollider2D>();

            // --- Suppression de la carte UI du deck ---
            if (eventData.pointerDrag != null)
            {
                var draggedUi = eventData.pointerDrag.GetComponent<UnitCardUI>();
                if (draggedUi != null)
                {
                    // Retire l'UnitData du deck du TurnManager
                    var tm = TurnManager.Instance;
                    if (tm.currentTeam == Team.J1Team)
                        tm.player1Deck.Remove(draggedUi.unitData);
                    else
                        tm.player2Deck.Remove(draggedUi.unitData);
                }
                // Détruit la carte UI
                Destroy(eventData.pointerDrag);
            }
        }
        else
        {
            // Drop invalide → détruit l'unité instanciée
            Destroy(spawnedUnit);
        }

        spawnedUnit = null;
    }

    /// <summary>
    /// Applique récursivement la layer donnée à go et à tous ses enfants.
    /// </summary>
    private void EnsureCorrectLayer(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            EnsureCorrectLayer(child.gameObject, layer);
    }
}











