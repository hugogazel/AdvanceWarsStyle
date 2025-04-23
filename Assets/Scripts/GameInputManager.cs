using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class GameInputManager : MonoBehaviour
{
    [Header("Références principales")]
    public GridManager gridManager;
    public Camera mainCamera;
    public ArrowPreviewManager arrowPreviewManager;
    public LayerMask unitLayerMask;

    private TurnManager turnManager;

    void Start()
    {
        if (gridManager == null) gridManager = FindObjectOfType<GridManager>();
        if (mainCamera == null) mainCamera = Camera.main;
        if (arrowPreviewManager == null) arrowPreviewManager = FindObjectOfType<ArrowPreviewManager>();

        turnManager = FindObjectOfType<TurnManager>();
        if (turnManager == null)
            Debug.LogWarning("TurnManager introuvable !");
    }

    void Update()
    {
        // 1) Preview de chemin
        if (gridManager?.selectedUnit != null && !Input.GetMouseButton(0))
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            worldPos.z = 0;
            Vector3Int cell = gridManager.groundTilemap.WorldToCell(worldPos);
            var path = gridManager.FindPath(
                gridManager.selectedUnit,
                gridManager.selectedUnit.position,
                new Vector2Int(cell.x, cell.y)
            );
            arrowPreviewManager?.ShowPathPreview(path);
        }

        // 2) Clic gauche
        if (Input.GetMouseButtonDown(0))
            HandleLeftClick();
    }

    private void HandleLeftClick()
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;

        // A) Clic sur unité
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, 0.2f, unitLayerMask);
        if (hits.Length > 0)
        {
            // Sélection / action d’unité
            Collider2D closest = hits[0];
            float minDist = Vector2.Distance(worldPos, closest.transform.position);
            for (int i = 1; i < hits.Length; i++)
            {
                float d = Vector2.Distance(worldPos, hits[i].transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    closest = hits[i];
                }
            }
            UnitController uc = closest.GetComponent<UnitController>();
            if (uc != null)
            {
                OnUnitClicked(uc);
                return;
            }
        }

        // B) Clic sur case vide ou UI
        Vector3Int cellPos = gridManager.groundTilemap.WorldToCell(worldPos);
        Vector2Int gridPos = new Vector2Int(cellPos.x, cellPos.y);
        OnEmptyCellClicked(gridPos);
    }

    private void OnUnitClicked(UnitController clickedUnit)
    {
        if (clickedUnit.hasActed) return;
        if (clickedUnit.team != turnManager.currentTeam) return;

        if (gridManager.selectedUnit == null)
            SelectUnit(clickedUnit);
        else if (gridManager.selectedUnit == clickedUnit)
            gridManager.actionPanel?.ShowPanel(clickedUnit);
        else if (gridManager.selectedUnit.IsAllied(clickedUnit))
            SelectUnit(clickedUnit);
        else
            Debug.Log("Pour attaquer, utilisez le bouton dédié.");
    }

    private void SelectUnit(UnitController unit)
    {
        gridManager.selectedUnit = unit;
        var reachable = gridManager.ComputeReachableTiles(unit, unit.position);
        gridManager.ClearHighlightedTiles();
        gridManager.HighlightReachableTiles(reachable, gridManager.defaultHighlightColor);
    }

    private void OnEmptyCellClicked(Vector2Int gridPos)
    {
        // Si on survole de l’UI, ne rien faire
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        // 1) Déplacement si une unité est sélectionnée
        if (gridManager.selectedUnit != null && gridManager.highlightedTiles.Count > 0)
        {
            GridCell cell = gridManager.GetCell(gridPos);
            if (cell != null &&
                gridManager.highlightedTiles.Contains(cell) &&
                !gridManager.selectedUnit.hasActed)
            {
                gridManager.selectedUnit.MoveTo(gridPos);
                return;
            }
            else
            {
                // Annulation du déplacement
                gridManager.ClearHighlightedTiles();
                arrowPreviewManager?.ClearPathPreview();
                gridManager.selectedUnit = null;
                return;
            }
        }

        // 2) Sinon, on ouvre le panel de fin de tour
        turnManager?.ShowEndTurnPanelOnly();
    }
}



















