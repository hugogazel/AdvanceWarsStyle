using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UnitSpawnDragHandler : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("LayerMask 2D du sol (non utilisé ici)")]
    public LayerMask groundLayerMask2D;

    private Camera mainCamera;
    private GridManager gridManager;

    private GameObject prefabToSpawn;
    private GameObject spawnedUnit;
    private bool dragging = false;

    void Awake()
    {
        // N’active ce script que dans la scène de jeu
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
        UnitCardUI ui = GetComponent<UnitCardUI>();
        if (ui == null || ui.unitPrefab == null)
            return;

        // <<<— AJOUTE CE LOG
        Debug.Log($"[DragStart] unitData = {ui.unitData.unitName}, unitPrefab = {(ui.unitPrefab ? ui.unitPrefab.name : "NULL")}");

        // Instanciation immédiate
        prefabToSpawn = ui.unitPrefab;
        spawnedUnit = Instantiate(prefabToSpawn);
        var uc = spawnedUnit.GetComponent<UnitController>();
        if (uc != null)
            uc.team = TurnManager.Instance.currentTeam;

        dragging = true;

    }



    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || spawnedUnit == null)
            return;

        // 1) Convertit la position écran → monde (plan Z=0)
        Vector3 screenPos = eventData.position;
        screenPos.z = -mainCamera.transform.position.z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        // 2) Trouve la cellule du Tilemap et place au centre
        Vector3Int cell = gridManager.groundTilemap.WorldToCell(worldPos);
        Vector3 center = gridManager.groundTilemap.GetCellCenterWorld(cell);
        spawnedUnit.transform.position = center;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging)
            return;
        dragging = false;

        if (spawnedUnit == null || gridManager == null)
            return;

        // 1) Recalcule la position monde du drop
        Vector3 screenPos = eventData.position;
        screenPos.z = -mainCamera.transform.position.z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0f;

        // 2) Cellule cible
        Vector3Int cell = gridManager.groundTilemap.WorldToCell(worldPos);
        Vector2Int gridPos = new Vector2Int(cell.x, cell.y);

        // 3) Snap ou destruction
        if (gridManager.IsTileReachable(gridPos))
        {
            Vector3 center = gridManager.groundTilemap.GetCellCenterWorld(cell);
            spawnedUnit.transform.position = center;
            var uc = spawnedUnit.GetComponent<UnitController>();
            if (uc != null) uc.position = gridPos;
        }
        else
        {
            Destroy(spawnedUnit);
        }

        spawnedUnit = null;
    }
}









